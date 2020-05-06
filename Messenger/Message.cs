/// Name: Samuel Tregea
/// Professor: Jeremy Brown
/// Project3: Messenger
/// File: Message.cs
/// Desctiption:
///             This class aids in decoding and encoding messages along with
///             sending and receiving messages from the server http://kayrun.cs.rit.edu:5000/Message/email
///             where the email has been specified in the main Program.cs class
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Messenger
{
    
    
    public class Message
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
        static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Decode a message sent from the server
        /// </summary>
        /// <param name="privateKey">The private key used to decode a message</param>
        /// <param name="encodedMessage">The message to decode</param>
        /// <returns>The decoded message</returns>
        string DecodeMessage(string privateKey, string encodedMessage)
        {
            var privateByteString = Convert.FromBase64String(privateKey);
            
            BigInteger D;
            BigInteger N;
            BigInteger C;
            BigInteger P;
            int d;
            int n;
            var byteStringIndex = 4;


            // get 'd' from the private key
            var dBytes = new byte[] {privateByteString[0], privateByteString[1], privateByteString[2], privateByteString[3]};
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(dBytes);
            }
            d = (int) new BigInteger(dBytes);
            
            // Read in D
            var dConverter = new byte[d];
            var privateByteStringIndex = 4;
            for (int i = 0; i < d; i++)
            {
                dConverter[i] = privateByteString[privateByteStringIndex];
                privateByteStringIndex++;
            }
            D = new BigInteger(dConverter);

            // read in the bytes for 'n'
            var nBytes = new byte[] {privateByteString[privateByteStringIndex], privateByteString[privateByteStringIndex + 1], 
                privateByteString[privateByteStringIndex + 2], privateByteString[privateByteStringIndex + 3]};
            privateByteStringIndex += 4;
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(nBytes);
            }
            n = (int) new BigInteger(nBytes);
            
            // read in 'N'
            var nConverter = new byte[n];
            for (var i = 0; i < n; i++)
            {
                nConverter[i] = privateByteString[privateByteStringIndex];
                privateByteStringIndex++;
            }
            N = new BigInteger(nConverter);

            // generate the ciphertext
            C = new BigInteger(Convert.FromBase64String(encodedMessage));
            
            // create the "plaintext" with the formula P = C^D % N
            P = BigInteger.ModPow(C, D, N);

            // convert from the BigInteger to ASCII characters
            var decodedMessage = Encoding.ASCII.GetString(P.ToByteArray());

            return decodedMessage;
        }
        
        /// <summary>
        /// Encrypt a message by decoding the public key and then encode the message using
        /// the formula C = P^E % N.
        /// </summary>
        /// <param name="email">The email associated to the public key</param>
        /// <param name="message">The message to encrypt</param>
        /// <returns>The ecrypted message</returns>
        string EncodeMessage(string email, string message)
        {
            var publicKeyFilepath = $"{Environment.CurrentDirectory}/{email}.key";

            var publicKey = Convert.FromBase64String(System.IO.File.ReadAllLines(publicKeyFilepath)[0]);
            
            BigInteger E;
            BigInteger N;
            BigInteger C;
            BigInteger P;
            int e;
            int n;
            var byteStringIndex = 4;

            // read in the bytes for 'e'
            var eBytes = new byte[] {publicKey[0], publicKey[1], publicKey[2], publicKey[3]};
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(eBytes);
            }
            e = (int) new BigInteger(eBytes);

            // read in 'E'
            var eConverter = new byte[e];
            for (var i = 0; i < e; i++)
            {
                eConverter[i] = publicKey[byteStringIndex];
                byteStringIndex++;
            }
            E = new BigInteger(eConverter);

            // read in the bytes for 'n'
            var nBytes = new byte[] {publicKey[byteStringIndex], publicKey[byteStringIndex + 1], 
                publicKey[byteStringIndex + 2], publicKey[byteStringIndex + 3]};
            byteStringIndex += 4;
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(nBytes);
            }
            n = (int) new BigInteger(nBytes);
            
            // read in 'N'
            var nConverter = new byte[n];
            for (var i = 0; i < n; i++)
            {
                nConverter[i] = publicKey[byteStringIndex];
                byteStringIndex++;
            }
            N = new BigInteger(nConverter);
            
            var messageBytes = Encoding.ASCII.GetBytes(message);

            P = new BigInteger(messageBytes);

            C = BigInteger.ModPow(P, E, N);

            var encodedMessage = Convert.ToBase64String(C.ToByteArray());
            
            return encodedMessage;
        }
        
        /// <summary>
        /// Encrypt and send a message to a user specified by their email.
        /// </summary>
        /// <param name="email">The email to send the message to</param>
        /// <param name="message">The message to be encoded and send</param>
        /// <returns></returns>
        public async Task SendMessage(string email, string message)
        {

            var file_to_check = $"{email}.key";
            
            // check if email's public key exists in the directory

            if (File.Exists(file_to_check))
            {
                var encodedMessage = EncodeMessage(email, message);
            
                // serialize the newly created json to be sent to the server
                var serverInfo = new Dictionary<string, object>
                {
                    {"email", email},
                    {"content", encodedMessage},
                };

                var serverJSON = JsonConvert.SerializeObject(serverInfo);

                var request = new StringContent(serverJSON, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PutAsync($"http://kayrun.cs.rit.edu:5000/Message/{email}", request);
                    Console.WriteLine("Message written");
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            else
            {
                Console.WriteLine($"Key does not exist for {email}");
            }

        }

        /// <summary>
        /// Function used to check to see if an email exists within the local private key.
        /// </summary>
        /// <param name="emailToCheck">The email to determine if it exists within the private key</param>
        /// <param name="jsonEmail">the "email" key in the json from the private key</param>
        /// <returns>true if an email exists, false otherwise</returns>
        bool EmailInKey(string emailToCheck, JArray jsonEmail)
        {
            var ret = false;

            foreach (var email in jsonEmail)
            {
                if (email.ToString() == emailToCheck)
                {
                    ret = true;
                }
            }
            return ret;
        }
        
        /// <summary>
        /// Retrieve a message from the server
        /// http://kayrun.cs.rit.edu:5000/Key/email
        /// </summary>
        /// <param name="email">The email to retrieve the message from</param>
        /// <returns></returns>
        public async Task GetMessage(string email)
        {
            var responseBody = "";

            // obtain the private key information
            var privateKeyFilepath = $"{Environment.CurrentDirectory}/private.key";
            var privateKey = System.IO.File.ReadAllLines(privateKeyFilepath);
            
            try
            {
                HttpResponseMessage response = await client.GetAsync($"http://kayrun.cs.rit.edu:5000/Message/{email}");
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            
            // try-catch to see if the response was null
            try
            {
                var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
                var privateKeyInformation = JsonConvert.DeserializeObject<Dictionary<string, object>>(privateKey[1]);
                var messageEmail = json["email"];
                var messageContent = json["content"];

                JArray listOfEmails = (JArray) privateKeyInformation["email"];
                
                // check to see if public key of sender is in private key
                if (EmailInKey(email, listOfEmails))
                {
                    var decodedMessage = DecodeMessage(privateKey[0], (string)messageContent);
                    Console.WriteLine(decodedMessage);
                }
                else
                {
                    Console.WriteLine("Message cannot be decoded");
                }
            }
            catch (NullReferenceException)
            {
                Console.Error.WriteLine($"The message from '{email}' does not exist!");
            }
        }
    }
}