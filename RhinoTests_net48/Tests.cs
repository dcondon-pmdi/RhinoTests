using Xunit;

namespace RhinoTests_net48
{
    [Collection("RhinoTestingCollection")]

    public class SomeTestsThatUseRinoInside
    {
        [Fact]
        public void ATest()
        {
            bool thing = true;
        }
    }
}