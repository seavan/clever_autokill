using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace clever_autokill
{
    public class ProcessKiller
    {
        private const string LOG_SOURCE = "clever_autokill.log";
       
        public ProcessKiller()
        {

        }

        public void Execute()
        {
            Kill(Select(Scan()));    
        }

        IEnumerable<Process> Scan()
        {
            return Process.GetProcesses();
        }

        void Kill(IEnumerable<Process> list)
        {
            foreach (var process in list)
            {
                Log("{3} Process to kill: {0} ID: {1}, memory {2} Mb", process.ProcessName, process.Id, process.WorkingSet64 / 1024 / 1024, DateTime.Now);
                process.Kill();
                Log("{0} Kill succeeded. Nice.", DateTime.Now);
            }
        }

        IEnumerable<Process> Select(IEnumerable<Process> items)
        {
            if (MaxMemory > 0)
            {
                items = items.Where(s => s.WorkingSet64 > MaxMemory*1024*1024);
            }

            if (!String.IsNullOrEmpty(NameFilter))
            {
                var pattern = NameFilter.Replace("*", ".*?");
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                items = items.Where(s => regex.IsMatch(s.ProcessName));
            }

            return items;
        }

        void Log(string message, params object[] items)
        {
            using (StreamWriter w = File.AppendText(LOG_SOURCE))
            {
                w.WriteLine(message, items);
            }
            Console.WriteLine(message, items);
        }

        public string NameFilter { get; set; }
        public long MaxMemory { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var processKiller = new ProcessKiller();
            processKiller.NameFilter = "*dev*";
            processKiller.MaxMemory = 100;
            processKiller.Execute();
        }
    } 

}

