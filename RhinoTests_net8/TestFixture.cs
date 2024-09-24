using System.Reflection;
using Microsoft.Win32;

namespace RhinoTests_net8
{
    /// <summary>
    /// Shared test context across unit tests that loads rhinocommon.dll and grasshopper.dll
    /// </summary>
    public class TestFixture : IDisposable
    {
        private bool initialized = false;
        private static string _rhinoDir = string.Empty;
        private Rhino.Runtime.InProcess.RhinoCore _rhinoCore;

        /// <summary>
        /// Empty Constuctor
        /// </summary>
        public TestFixture()
        {
            //get the correct rhino 8 installation directory
            _rhinoDir = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\McNeel\Rhinoceros\8.0\Install", "Path", null) as string ?? string.Empty;
            Assert.True(Directory.Exists(_rhinoDir), string.Format("Rhino system dir not found: {0}", _rhinoDir));

            // Make sure we are running the tests as 64x
            Assert.True(Environment.Is64BitProcess, "Tests must be run as x64");

            if (initialized)
            {
                throw new InvalidOperationException("Initialize Rhino.Inside once");
            }
            else
            {
                RhinoInside.Resolver.UseLatest = true;
                RhinoInside.Resolver.Initialize();
                initialized = true;
            }

            // Set path to rhino system directory
            string envPath = Environment.GetEnvironmentVariable("path");
            Environment.SetEnvironmentVariable("path", envPath + ";" + _rhinoDir);

            // Start a headless rhino instance using Rhino.Inside
            StartRhino();

            // We have to load grasshopper.dll on the current AppDomain manually for some reason
            AppDomain.CurrentDomain.AssemblyResolve += ResolveGrasshopper;

        }

        /// <summary>
        /// Starting Rhino - loading the relevant libraries
        /// </summary>
        [STAThread]
        public void StartRhino()
        {
            var args = new string[]
            {
                "/netcore"
            };

            _rhinoCore = new Rhino.Runtime.InProcess.RhinoCore(args, Rhino.Runtime.InProcess.WindowStyle.NoWindow);
        }

        /// <summary>
        /// Add Grasshopper.dll to the current Appdomain
        /// </summary>
        private Assembly ResolveGrasshopper(object sender, ResolveEventArgs args)
        {
            var name = args.Name;

            if (!name.StartsWith("Grasshopper"))
            {
                return null;
            }

            var path = Path.Combine(Path.GetFullPath(Path.Combine(_rhinoDir, @"..\")), "Plug-ins\\Grasshopper\\Grasshopper.dll");
            return Assembly.LoadFrom(path);
        }

        /// <summary>
        /// Disposing the context after running all the tests
        /// </summary>
        public void Dispose()
        {
            // do nothing or...
            _rhinoCore?.Dispose();
            _rhinoCore = null;
        }
    }

    /// <summary>
    /// Collection Fixture - shared context across test classes
    /// </summary>
    [CollectionDefinition("RhinoTestingCollection")]
    public class RhinoCollection : ICollectionFixture<TestFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}