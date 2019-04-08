using System;
using System.IO;
using System.Threading.Tasks;
using FilesDistributor.Abstract;
using FilesDistributor.Models;
using System.Collections.Generic;
using System.Linq;

using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using Strings = FilesDistributor.Resources.Strings;

namespace FilesDistributor
{
    public class FilesDistributor : IDistributor<FileModel>
    {
        private readonly ILogger _logger;
        private readonly List<Rule> _rules;
        private readonly string _defaultFolder;
        private const int FileCheckTimoutMiliseconds = 1000;

        public FilesDistributor(IEnumerable<Rule> rules, string defaultFolder, ILogger logger)
        {
            _rules = rules.ToList();
            _logger = logger;
            _defaultFolder = defaultFolder;
        }

        public async Task MoveAsync(FileModel item)
        {
            string from = item.FullName;
            foreach (Rule rule in _rules)
            {
                var match = Regex.Match(item.Name, rule.FilePattern);

                if (match.Success && match.Length == item.Name.Length)
                {
                    rule.MatchesCount++;
                    string to = FormDestinationPath(item, rule);
                    _logger.Log(Strings.RuleMatch);
                    await MoveFileAsync(from, to);
                    _logger.Log(string.Format(Strings.FileMovedTemplate, item.FullName, to));
                    return;
                }
            }

            string destination = Path.Combine(_defaultFolder, item.Name);
            _logger.Log(Strings.RuleNoMatch);
            await MoveFileAsync(from, destination);
            _logger.Log(string.Format(Strings.FileMovedTemplate, item.FullName, destination));
        }

        private async Task MoveFileAsync(string from, string to)
        {
            string dir = Path.GetDirectoryName(to);
            Directory.CreateDirectory(dir);
            bool cannotAccessFile = true;
            do
            {
                try
                {
                    if (File.Exists(to))
                    {
                        File.Delete(to);
                    }
                    File.Move(from, to);
                    cannotAccessFile = false;
                }
                catch (FileNotFoundException)
                {
                    _logger.Log(Strings.CannotFindFile);
                    break;
                }
                catch (IOException ioex)
                {
                    var t = ioex.GetType();
                    await Task.Delay(FileCheckTimoutMiliseconds);
                }
            } while (cannotAccessFile);
        }

        private string FormDestinationPath(FileModel file, Rule rule)
        {
            string extension = Path.GetExtension(file.Name);
            string filename = Path.GetFileNameWithoutExtension(file.Name);
            StringBuilder destination = new StringBuilder();
            destination.Append(Path.Combine(rule.DestinationFolder, filename));

            if (rule.IsDateAppended)
            {
                var dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
                dateTimeFormat.DateSeparator = ".";
                destination.Append($"{destination}_{DateTime.Now.ToLocalTime().ToString(dateTimeFormat.ShortDatePattern)}");
            }

            if (rule.IsOrderAppended)
            {
                destination.Append($"_{rule.MatchesCount}");
            }

            destination.Append(extension);
            return destination.ToString();
        }
    }
}
