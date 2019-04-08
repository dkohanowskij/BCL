using System;
using System.Threading;
using System.Threading.Tasks;
using FilesDistributor.Abstract;
using FilesDistributor.Models;
using System.Configuration;
using System.Collections.Generic;
using System.Globalization;
using FilesDistributor.EventArgs;
using FilesDistributor.Resources;
using DirectoryElement = FilesDistributor.Configuration.DirectoryElement;
using FileSystemMonitorConfigSection = FilesDistributor.Configuration.FileSystemMonitorConfigSection;
using RuleElement = FilesDistributor.Configuration.RuleElement;

namespace FilesDistributor
{
    public class FileDistributorService
    {
        private static List<string> _directories;
        private static List<Rule> _rules;
        private static IDistributor<FileModel> _distributor;

        static async Task Main(string[] args)
        {
            FileSystemMonitorConfigSection config = ConfigurationManager.GetSection("fileSystemSection") as FileSystemMonitorConfigSection;

            if (config != null)
            {
                ReadConfiguration(config);
            }
            else
            {
                Console.Write(Strings.ConfigNotFounded);
                return;
            }

            Console.WriteLine(config.Culture.DisplayName);

            ILogger logger = new Logger();
            _distributor = new FilesDistributor(_rules, config.Rules.DefaultDirectory, logger);
            ILocationsWatcher<FileModel> watcher = new FilesWatcher(_directories, logger);

            watcher.Created += OnCreated;
            
            CancellationTokenSource source = new CancellationTokenSource();

            Console.CancelKeyPress += (o, e) =>
            {
                watcher.Created -= OnCreated;
                source.Cancel();
            };

            await Task.Delay(TimeSpan.FromMilliseconds(-1), source.Token);
        }

        private static async void OnCreated(object sender, CreatedEventArgs<FileModel> args)
        {
            await _distributor.MoveAsync(args.CreatedItem);
        }

        private static void ReadConfiguration(FileSystemMonitorConfigSection config)
        {
            _directories = new List<string>(config.Directories.Count);
            _rules = new List<Rule>();

            foreach (DirectoryElement directory in config.Directories)
            {
                _directories.Add(directory.Path);
            }

            foreach (RuleElement rule in config.Rules)
            {
                _rules.Add(new Rule
                {
                    FilePattern = rule.FilePattern,
                    DestinationFolder = rule.DestinationFolder,
                    IsDateAppended = rule.IsDateAppended,
                    IsOrderAppended = rule.IsOrderAppended
                });
            }

            CultureInfo.DefaultThreadCurrentCulture = config.Culture;
            CultureInfo.DefaultThreadCurrentUICulture = config.Culture;
            CultureInfo.CurrentUICulture = config.Culture;
            CultureInfo.CurrentCulture = config.Culture;
        }
    }
}
