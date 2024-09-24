namespace RhinoTests
{
    [Collection("RhinoTestingCollection")]

    public class SomeTestsThatUseRhinoInside
    {
        [Fact]
        public void ATest()
        {
            bool thing = true;
        }
    }
}