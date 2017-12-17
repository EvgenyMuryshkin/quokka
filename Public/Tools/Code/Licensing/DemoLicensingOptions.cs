namespace Quokka.Public.Tools
{
    public class DemoLicensingOptions : ILicensingOptions
    {
        public int MaxConfigurations => 2;

        public int MaxControllers => 10;

        public int MaxObjects => 10;

        public int MaxStates => 150;

        public int MaxModules => 5;

        public int MaxSequences => 10;

        public int MaxTimeout => 10;
    }
}
