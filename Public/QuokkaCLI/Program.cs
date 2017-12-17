using Quokka.Public.Transformation;
using Quokka.Public.Tools;
using Quokka.Public.Bootstrap;
using Quokka.Public.Content;

namespace QuokkaCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var c = new QuokkaContainer()
                    .WithContentDomain(eContentDomain.Public)
                    .WithProjectTransformation<CloudTransformation>()
                    .WithRuntimeConfiguration(RuntimeConfiguration.FromCommandLineArguments(args)))
            {
                c.Resolve<QuokkaMain>().Run(args);
            }
        }
    }
}