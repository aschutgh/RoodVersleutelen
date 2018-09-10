using System;
using CommandLine;
using System.Security.Cryptography;

namespace RoodVersleutelen
{
    class Program
    {
        public class Options
        {
            [Option('i', "input", Required = true, HelpText = "Input file to be encrypted or decrypted.")]
            public string Input { get; set; }

            [Option('o', "output", Required = true, HelpText = "Output file.")]
            public string Output { get; set; }

            [Option('e', "encrypt", Required = false, HelpText = "Encrypt input file.")]
            public bool Encrypt { get; set; }

            [Option('d', "decrypt", Required = false, HelpText = "Decrypt input file.")]
            public bool Decrypt { get; set; }
        }

        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);
            result.WithParsed<Options>(o =>
            {
                if (o.Verbose)
                {
                    Console.WriteLine("Lekker uitgebreid doen...");
                }
                if (o.Encrypt)
                {
                    Console.WriteLine("We gaan een bestand encrypten");
                }
                if (o.Decrypt)
                {
                    Console.WriteLine("We gaan een bestand decrypten");
                }
                if (o.Input.Length > 0)
                {
                    Console.WriteLine("Dit bestand gaan we versleutelen {0}", o.Input);
                }
                if (o.Output.Length > 0)
                {
                    Console.WriteLine("Dit bestand gaan we ontsleutelen {0}", o.Output);
                }
            });

            //Parser.Default.ParseArguments<Options>(args)
            //       .WithParsed<Options>(o =>
            //       {
            //           if (o.Verbose)
            //           {
            //               Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
            //               Console.WriteLine("Quick Start Example! App is in Verbose mode!");
            //           }
            //           else
            //           {
            //               Console.WriteLine($"Current Arguments: -v {o.Verbose}");
            //               Console.WriteLine("Quick Start Example!");
            //           }
            //       });
        }
    }
}
