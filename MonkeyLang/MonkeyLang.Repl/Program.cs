using CommandLine;
using MonkeyLang;
using MonkeyLang.Repl;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Repl
{
    class Program
    {
        private CompositionContainer Container { get; }

        [Import(typeof(MonkeyRepl))]
        private MonkeyRepl Repl { get; set; }

        private Program()
        {
            DirectoryCatalog catalog = new DirectoryCatalog(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            Container = new CompositionContainer(catalog);

            try
            {
                this.Container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }
        }

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                    .WithParsed<Options>(o =>
                    {
                        if (o.Trace)
                        {
                            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
                            Trace.AutoFlush = true;
                        }
                    });

            Program p = new Program();
            p.Repl.Start();
        }
    }

    public class Options
    {
        [Option('t', "trace", Required = false, HelpText = "Output trace information.")]
        public bool Trace { get; set; }
    }
}
