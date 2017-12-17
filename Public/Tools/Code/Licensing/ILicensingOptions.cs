namespace Quokka.Public.Tools
{
    public interface ILicensingOptions
    {
        int MaxConfigurations { get; }
        int MaxControllers { get; }
        int MaxObjects { get; }
        int MaxStates { get; }
        int MaxModules { get; }
        int MaxSequences { get; }
        int MaxTimeout { get; }
    }
}
