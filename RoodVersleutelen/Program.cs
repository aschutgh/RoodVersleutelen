using System;
using CommandLine;
using System.Security.Cryptography;

namespace RoodVersleutelen
{
    class Program
    {

        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option('i', "input", Required = false, HelpText = "Input file to be encrypted or decrypted.")]
            public bool Input { get; set; }

            [Option('o', "output", Required = false, HelpText = "Output file.")]
            public bool Output { get; set; }

            [Option('e', "encrypt", Required = false, HelpText = "Encrypt input file.")]
            public bool Encrypt { get; set; }

            [Option('d', "decrypt", Required = false, HelpText = "Decrypt input file.")]
            public bool Decrypt { get; set; }
        }


        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       if (o.Verbose)
                       {
                           Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                           Console.WriteLine("Quick Start Example! App is in Verbose mode!");
                       }
                       else
                       {
                           Console.WriteLine($"Current Arguments: -v {o.Verbose}");
                           Console.WriteLine("Quick Start Example!");
                       }
                   });
        }
    }
}
