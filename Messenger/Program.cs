/// Name: Samuel Tregea
/// Professor: Jeremy Brown
/// Project3: Messenger
/// File: Program.cs
/// Desctiption:
///             This program uses public key encryption to send secure messages to other users on the server.
///             The server runs on http://kayrun.cs.rit.edu:5000
///             with the urls: http://kayrun.cs.rit.edu:5000/Key/
///                       and http://kayrun.cs.rit.edu:5000/Message/
///
///
///              There are several options you can provide as the first command line argument, they are:
///                • -h / help
///                • keyGen
///                • sendKey
///                • getKey
///                • sendMsg
///                • getMsg
/// 
///              Each of these options will accomplish a basic task with the details of each task
///              being mentioned within the Help() function

using System;
using System.Threading.Tasks;

namespace Messenger
{
    class Program
    {
        /// <summary>
        /// Print out a help menu for the user.
        /// </summary>
        static void Help()
        {
            Console.WriteLine("dotnet run <option> <other arguments>");
            Console.WriteLine("Options:");
            Console.WriteLine($"{null,-4}-h {null, -5}- print out this help menu.\n");
            Console.WriteLine($"{null,-4}help {null, -3}- print out this help menu.\n");
            Console.WriteLine($"{null,-4}keyGen  - generate a keypair (public and private keys) and store them locally on the disk.");
            Console.WriteLine($"{null,-14}• argument 1: keysize - the total size of the key in bits.\n");
            Console.WriteLine($"{null,-4}sendKey - sends the public key to the server.");
            Console.WriteLine($"{null,-14}• argument 1: email - the email to send the public key to.\n");
            Console.WriteLine($"{null,-4}getKey  - retrieve a base64 encoded public key for a particular user.");
            Console.WriteLine($"{null,-14}• argument 1: email - the email to receive the public key from.\n");
            Console.WriteLine($"{null,-4}sendMsg - this will base64 encode a message for a user within the server.");
            Console.WriteLine($"{null,-14}• argument 1: email - the email to send the message to.");
            Console.WriteLine($"{null,-14}• argument 2: plaintext - the message to encode and send to the email specified.\n");
            Console.WriteLine($"{null,-4}getMsg  - this will retrieve the base64 encoded message for a particular user, while it is possible to download messages");
            Console.WriteLine($"{null,-16}for any user, you will only be able to decode messages for which you have the private key.");
            Console.WriteLine($"{null,-14}• argument 1: email - the email to receive the message from.");
        }

        /// <summary>
        /// Determine if a string is numeric.
        /// </summary>
        /// <param name="number">The number to check.</param>
        /// <returns>True if a number, false otherwise.</returns>
        static Boolean IsNumeric(string number)
        {
            return int.TryParse(number, out _);
        }

        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                // if there is one argument and it reads -h or help
                if (args.Length == 1)
                {
                    switch (args[0])
                    {
                        case "-h":
                        case "help":
                            Help();
                            return;
                    }
                }

                // if a user doesn't enter proper amount of arguments
                Console.Error.WriteLine($"ERROR: Too few arguments!");
                Help();
            }
            else
            {
                switch (args[0])
                {
                    case "-h":
                    case "help":
                        Help();
                        break;
                    case "keyGen":
                        if (IsNumeric(args[1]))
                        {
                            var bits = Int32.Parse(args[1]);
                            new Key().KeyGen(bits);
                        }
                        else
                        {
                            Console.WriteLine("Invalid type for bits. Please enter a valid integer.");
                        }

                        break;
                    case "sendKey":
                        await new Key().SendKey(args[1]);
                        break;
                    case "getKey":
                        await new Key().GetKey(args[1]);
                        break;
                    case "sendMsg":
                        if (args.Length >= 3)
                        {
                            var message = "";
                            // build the string from the command line
                            for (var i = 2; i < args.Length; i++)
                            {
                                message += args[i] + " ";
                            }

                            await new Message().SendMessage(args[1], message);
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Too few arguments!");
                            Help();
                        }
                        break;
                    case "getMsg":
                        await new Message().GetMessage(args[1]);
                        break;
                    default:
                        Console.Error.WriteLine($"ERROR: Invalid arguments!");
                        Help();
                        return;
                }
            }
        }
    }
}