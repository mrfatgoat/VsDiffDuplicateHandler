using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO.Abstractions;
using VsDiffDuplicateHandler.Configuration;
using VsDiffDuplicateHandler.Services;
using VsDiffDuplicateHandler.Services.Interfaces;

namespace VsDiffDuplicateHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            IDuplicateHandlerApp app = CreateApp(args);
            app.Run();
        }


        private static IDuplicateHandlerApp CreateApp(string[] args)
        {
            IServiceProvider serviceProvider = new ServiceCollection()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder
                        .AddConsole()
                        .AddDebug();
                })
                .AddSingleton<IDuplicateHandlerConfiguration>(isp =>
                {
                    ILogger<Program> logger = isp.GetRequiredService<ILogger<Program>>();
                    DuplicateHandlerConfiguration config = AssertValidArguments(args, logger);
                    return config;
                })
                .AddSingleton<IDuplicateHandlerApp, DuplicateHandlerApp>()
                .AddSingleton<IDuplicateReaderFactory, DuplicateReaderFactory>()
                .AddSingleton<IFileOperationsAbstraction, FileOperationsImplementation>()
                .AddSingleton<IDuplicateReader, XmlDuplicateReader>()
                .AddSingleton<IDuplicateReader, CsvDuplicateReader>()
                .AddSingleton<IFileSystem, FileSystem>()
                .AddSingleton<IDuplicateProcessor, DuplicateProcessor>()
                .AddSingleton<DryRunFileModifier>()
                .AddSingleton<FileModifier>()
                .AddSingleton<IFileModifier>(isp =>
                {
                    IDuplicateHandlerConfiguration config = isp.GetRequiredService<IDuplicateHandlerConfiguration>();
                    if (config.DryRun)
                        return isp.GetRequiredService<DryRunFileModifier>();
                    else
                        return isp.GetRequiredService<FileModifier>();
                })
                .BuildServiceProvider();

            return serviceProvider.GetRequiredService<IDuplicateHandlerApp>();
        }


        private static DuplicateHandlerConfiguration AssertValidArguments(string[] args, ILogger<Program> logger)
        {
            if (args.Length < 2)
            {
                logger.LogError("Expected a \"duplicates\" file and \"good\" base path.");
                throw new ArgumentException();
            }

            bool dryrun =
                args.Length == 3 &&
                args[2].Equals("--dryrun", StringComparison.OrdinalIgnoreCase);

            return new DuplicateHandlerConfiguration()
            {
                DuplicateFilePath = args[0],
                GoodPath = args[1],
                DryRun = dryrun
            };
        }
    }
}
