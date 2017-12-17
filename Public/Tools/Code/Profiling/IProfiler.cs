namespace Quokka.Public.Profiling
{
    public interface IProfiler
    {
        string Name { get; }
        long ElapsedMilliseconds { get; }
        void Checkpoint(string tag);
        object Serialize();
    }

    public interface IProfilerFactory
    {
        IProfiler Default { get; }

        IProfiler Create(string name);

        object Serialize();
    }
}
