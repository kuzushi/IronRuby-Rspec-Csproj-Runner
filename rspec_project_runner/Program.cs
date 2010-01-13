using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using CmdParser;
using System.Reflection;

namespace Rspec.Project.Runner
{
    
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                ProgramArguments programArguments = ProgramArguments.Parse(args);

                // nothing passed in, show usage
                if (args.Length == 0)
                {
                    Console.Write(programArguments.GetUsageString());
                    return 0;
                }

                if (!programArguments.IsValid())
                {
                    Console.Write(programArguments.Error.Message);
                    return programArguments.Error.Severity;
                }

                SpecBuilder builder = new SpecBuilder(programArguments);
                builder.Render();
            }
            catch (CmdException cmdEx)
            {
                Console.WriteLine(cmdEx.Message);
                Console.WriteLine("Note: For a usage summary, use -?");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Oops! We got a problem dave! Error: " + ex.Message);
            }
            

            return 0;
        }

        private static void ShowUsage(Parameters parameters)
        {
            string helpString = parameters.GetUsageString(Assembly.GetExecutingAssembly(), 12);
            Console.WriteLine("\nUsage:");
            Console.WriteLine(helpString);
            Console.WriteLine("Note: For detailed help, use -?? or -help.");
        }

        private static void ShowHelp(Parameters parameters)
        {
            switch (parameters.HelpChars)
            {
                case "?":
                    ShowUsage(parameters);
                    return;
                case "help":
                case "??":
                    string helpString = parameters.GetDetailedHelp(Assembly.GetExecutingAssembly());
                    Console.WriteLine();
                    Console.Write(helpString);
                    Console.WriteLine("Note: For a usage summary, use -?.");
                    return;
            }
        }
    }
}
