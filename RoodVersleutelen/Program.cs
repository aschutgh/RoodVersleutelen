using System;
using CommandLine;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;





// dependency: CommandLineParser (2.3.0)
// https://github.com/commandlineparser/commandline


//string input = "some text";
//byte[] array = Encoding.ASCII.GetBytes(input);


// usage (to encrypt): dotnet RoodVersleutelen.dll -e -i filetoencrypt
// usage (to decrypt): dotnet RoodVersleutelen.dll -d -i filetodecrypt -o filetodecrypt.plaintext

namespace RoodVersleutelen
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input file to be encrypted or decrypted.")]
        public string Input { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output file.")]
        public string Output { get; set; }

        [Option('e', "encrypt", Required = false, HelpText = "Encrypt input file.")]
        public bool Encrypt { get; set; }

        [Option('d', "decrypt", Required = false, HelpText = "Decrypt input file.")]
        public bool Decrypt { get; set; }
    }

    class Program
    {
        // sourse: https://ourcodeworld.com/articles/read/471/how-to-encrypt-and-decrypt-files-using-the-aes-encryption-algorithm-in-c-sharp
        //  Call this function to remove the key from memory after use for security
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

        /// <summary>
        /// Creates a random salt that will be used to encrypt your file. This method is required on FileEncrypt.
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateRandomSalt()
        {
            byte[] data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    // Fille the buffer with the generated data
                    rng.GetBytes(data);
                }
            }

            return data;
        }

        /// <summary>
        /// Encrypts a file from its path and a plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="password"></param>
        static void FileEncrypt(string inputFile, string password)
        {
            //http://stackoverflow.com/questions/27645527/aes-encryption-on-large-files

            //generate random salt
            byte[] salt = GenerateRandomSalt();

            //create output file name
            FileStream fsCrypt = new FileStream(inputFile + ".aes", FileMode.Create);

            //convert password string to byte arrray
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            //Set Rijndael symmetric encryption algorithm
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            //http://stackoverflow.com/questions/2659214/why-do-i-need-to-use-the-rfc2898derivebytes-class-in-net-instead-of-directly
            //"What it does is repeatedly hash the user password along with the salt." High iteration counts.
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            //Cipher modes: http://security.stackexchange.com/questions/52665/which-is-the-best-cipher-mode-and-padding-mode-for-aes-encryption
            //AES.Mode = CipherMode.CFB;

            // write salt to the begining of the output file, so in this case can be random every time
            fsCrypt.Write(salt, 0, salt.Length);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

            FileStream fsIn = new FileStream(inputFile, FileMode.Open);

            //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
            byte[] buffer = new byte[1048576];
            int read;

            try
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //Application.DoEvents(); // -> for responsive GUI, using Task will be better!
                    cs.Write(buffer, 0, read);
                }

                // Close up
                fsIn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                cs.Close();
                fsCrypt.Close();
            }
        }

        /// <summary>
        /// Decrypts an encrypted file with the FileEncrypt method through its path and the plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="password"></param>
        static void FileDecrypt(string inputFile, string outputFile, string password)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.PKCS7;
            //AES.Mode = CipherMode.CFB;

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

            FileStream fsOut = new FileStream(outputFile, FileMode.Create);

            int read;
            byte[] buffer = new byte[1048576];

            try
            {
                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //Application.DoEvents();
                    fsOut.Write(buffer, 0, read);
                }
            }
            catch (CryptographicException ex_CryptographicException)
            {
                Console.WriteLine("CryptographicException error: " + ex_CryptographicException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            try
            {
                cs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error by closing CryptoStream: " + ex.Message);
            }
            finally
            {
                fsOut.Close();
                fsCrypt.Close();
            }
        }
        // ==========================================================================

        static string AskPassword()
        {
            string password;
            Console.Write("Enter password: ");
            password = Console.ReadLine();
            return password;



            //SecureString securePwd = new SecureString();
            //ConsoleKeyInfo key;

            //Console.Write("Enter password: ");
            //do
            //{
            //    key = Console.ReadKey(true);

            //    // Ignore any key out of range.
            //    if (((int)key.Key) >= 65 && ((int)key.Key <= 90))
            //    {
            //        // Append the character to the password.
            //        securePwd.AppendChar(key.KeyChar);
            //        Console.Write("*");
            //    }
            //    // Exit if Enter key is pressed.
            //} while (key.Key != ConsoleKey.Enter);
            //Console.WriteLine();
            //return securePwd;
        }

        static void Main(string[] args)
        {
            bool encrypt = false;
            bool decrypt = false;
            string inputfile = "";
            string outputfile = "";

            var result = Parser.Default.ParseArguments<Options>(args);
            result.WithParsed<Options>(o =>
            {
                //if (o.Verbose)
                //{
                //    Console.WriteLine("Lekker uitgebreid doen...");
                //}
                if (o.Encrypt)
                {
                    encrypt = true;
                }
                if (o.Decrypt)
                {
                    decrypt = true;
                }
                if (o.Input.Length > 0)
                {
                    inputfile = o.Input;
                }
                //if (o.Output.Length > 0)
                //{
                outputfile = o.Output;
                //}
            });
            if (encrypt == true)
                FileEncrypt(inputfile, AskPassword().ToString());
            if (decrypt == true)
                FileDecrypt(inputfile, outputfile, AskPassword().ToString());
        }
    }
}
