using System;
using FilesDistributor.Abstract;

namespace FilesDistributor
{
    public class Logger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
