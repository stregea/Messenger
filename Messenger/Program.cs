using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            Console.WriteLine($"{null, -4}- keyGen  - generate a keypair (public and private keys) and store them locally on the disk.");
            Console.WriteLine($"{null, -14}* argument 1: keysize - the total size of the key in bits.\n");
            Console.WriteLine($"{null, -4}- sendKey - sends the public key to the server.");
            Console.WriteLine($"{null, -14}* argument 1: email - the email to send the public key to.\n");
            Console.WriteLine($"{null, -4}- getKey  - retrieve a base64 encoded public key for a particular user (not usually yourself)");
            Console.WriteLine($"{null, -14}* argument 1: email - the email to receive the public key from.\n");
            Console.WriteLine($"{null, -4}- sendMsg - this will base64 encode a message for a user within the server.");
            Console.WriteLine($"{null, -14}* argument 1: email - the email to send the message to.");
            Console.WriteLine($"{null, -14}* argument 2: plaintext - the message to encode and send to the email specified.\n");
            Console.WriteLine($"{null, -4}- getMsg  - this will retrieve the base64 encoded message for a particular user, while it is possible to download messages");
            Console.WriteLine($"{null, -16}for any user, you will only be able to decode messages for which you have the private key.");
            Console.WriteLine($"{null, -14}* argument 1: email - the email to receive the message from.");
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
           
            // if a user doesn't enter any arguments
            if (args.Length < 2)
            {
                Console.Error.WriteLine($"ERROR: Too few arguments!");
                Help();
            }
            else
            {
                switch (args[0])
                {
                    case "keyGen":
                        if (IsNumeric(args[1]))
                        {
                            var bits = Int32.Parse(args[1]);
                            new Key().keyGen(bits);
                        }
                        else
                        {
                            Console.WriteLine("Invalid type for bits. Please enter a valid integer.");
                        }
                        break;
                    case "sendKey":
                        await new Key().sendKey(args[1]);

                        break;
                    case "getKey":
                        await new Key().getKey(args[1]);
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
                            await new Message().sendMessage(args[1], message);
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Too few arguments!");
                            Help();
                        }
                        break;
                    case "getMsg":
                        await new Message().getMessage(args[1]);
                        break;
                    default:
                        Help();
                        return;
                }
            }
            // new Key().keyGen(1024);
            //
            // await new Message().sendMessage("jsb@cs.rit.edu", "Project3");
            //
            // await new Key().sendKey("sdt1093@rit.edu");
            //
            // await new Message().sendMessage("sdt1093@rit.edu", "It worked!");
            //
            // await new Key().getKey("sdt1093@rit.edu");
            //
            // await new Message().sendMessage("sdt1093@rit.edu", "It worked!");
            //
            // await new Message().getMessage("sdt1093@rit.edu");
            // await new Message().getMessage("jsb@cs.rit.edu");


            // await new Key().getKey("jsb@cs.rit.edu");
            // await new Key().getKey("exc5774@rit.edu");
            // await new Message().getMessage("exc5774@rit.edu");

            // await new Message().sendMessage("", "Hello");
            // new Message().test();

            // await new Message().getMessage("jeremy.brown@rit.edu");
            // await new Message().getMessage("exc5774@rit.edu");
            //

        }
    }
}