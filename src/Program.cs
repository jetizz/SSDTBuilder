using System;
using System.IO;
using CommandLine;


namespace SSDTBuilder
{
    class Program
    {
        static void RunApplication(Options options)
        {
            var log = new StatusWriter(options.IsSilent, options.IsVerbose);

            if (!options.Generate)
            {
                log.Error("No output was selected.");
                return;
            }

            var project = new SqlProject(log, options.ProjectPath);
            var generator = new Generator(log, project, options);
            generator.Build();
        }

        static void Main(string[] args)
        {
            var result = Parser.Default
                .ParseArguments<Options>(args)
                .WithParsed(opts => RunApplication(opts));
        }
    }

}
