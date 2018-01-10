using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using VsDiffDuplicateHandler.Configuration;
using VsDiffDuplicateHandler.Services;
using VsDiffDuplicateHandler.Services.Interfaces;

namespace VsDiffDuplicateHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IDuplicateHandlerConfiguration>(AssertValidArguments(args));
            services.AddSingleton<IDuplicateReaderFactory, DuplicateReaderFactory>();
            services.AddSingleton<IXmlLoader, XmlLoader>();
            services.AddSingleton<IDuplicateReader, XmlDuplicateReader>();
            services.AddSingleton<IDuplicateReader, CsvDuplicateReader>();
            services.AddSingleton<System.IO.Abstractions.IFileSystem, System.IO.Abstractions.FileSystem>();
            services.AddSingleton<IDuplicateProcessor, DuplicateProcessor>();
            services.AddSingleton<IFileModifier>(isp =>
            {
                IDuplicateHandlerConfiguration config = isp.GetRequiredService<IDuplicateHandlerConfiguration>();
                if (config.DryRun)
                    return new DryRunFileModifier();
                else
                    return new FileModifier();
            });

            IServiceProvider sp = services.BuildServiceProvider();
            IDuplicateProcessor dupProc = sp.GetRequiredService<IDuplicateProcessor>();

            dupProc.ProcessDuplicates();
        }


        #region File Operations
        
        private static void DeleteFile(string file)
        {
            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception deleting {file}");
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine($"DELETED {file}");
        }

        private static void MoveFile(string file, string dest)
        {
            try
            {
                File.Move(file, dest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception moving {file}");
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine($"MOVED {file} to {Path.GetDirectoryName(dest)}");
        }
        
        #endregion

        private static IDuplicateHandlerConfiguration AssertValidArguments(string[] args)
        {
            if (args.Length < 2)
            {
                // TODO: Abstract logging
                Console.WriteLine("Expected a \"duplicates\" file and \"good\" base path.");
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
