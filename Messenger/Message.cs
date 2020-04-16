using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Messenger
{
    
    
    public class Message
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
        static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Decode a message sent from the server
        /// </summary>
        /// <param name="publicKeyFromSender">The senders public key used to decode their message</param>
        /// <param name="encodedMessage">The message to decode</param>
        /// <returns>The decoded message</returns>
        string DecodeMessage(string publicKeyFromSender, string encodedMessage)
        {
            var byteString = Convert.FromBase64String(publicKeyFromSender);
            
            // obtain the private key information
            var privateKeyFilepath = $"{Environment.CurrentDirectory}/private.key";
            var privateKey = System.IO.File.ReadAllLines(privateKeyFilepath);
            var privateByteString = Convert.FromBase64String(privateKey[0]);
            
            BigInteger E;
            BigInteger D;
            BigInteger N;
            BigInteger C;
            BigInteger P;
            int e;
            int d;
            int n;
            var byteStringIndex = 4;

            // read in the bytes for 'e'
            var eBytes = new byte[] {byteString[0], byteString[1], byteString[2], byteString[3]};
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(eBytes);
            }
            e = (int) new BigInteger(eBytes);

            // read in 'E'
            var eConverter = new byte[e];
            for (var i = 0; i < e; i++)
            {
                eConverter[i] = byteString[byteStringIndex];
                byteStringIndex++;
            }
            E = new BigInteger(eConverter);

            // read in the bytes for 'n'
            var nBytes = new byte[] {byteString[byteStringIndex], byteString[byteStringIndex + 1], 
                byteString[byteStringIndex + 2], byteString[byteStringIndex + 3]};
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
                nConverter[i] = byteString[byteStringIndex];
                byteStringIndex++;
            }
            N = new BigInteger(nConverter);
            
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

            // Console.WriteLine($"e: {e}\nE: {E}\nn: {n}\nN: {N}\nd: {d}\nD: {D}");
           
            // generate the ciphertext
            C = new BigInteger(Convert.FromBase64String(encodedMessage));
            
            // create the "plaintext" with the formula P = C^D % N
            P = BigInteger.ModPow(C, D, N);

            // convert from the BigInteger to ASCII characters
            var decodedMessage = Encoding.ASCII.GetString(P.ToByteArray());

            return decodedMessage;
        }
        
        /// <summary>
        /// Encrypt a message by decoding the public key and then ecrypt the message using
        /// the formula C = P^E % N.
        /// </summary>
        /// <param name="message">The message to encrypt</param>
        /// <returns>The ecrypted message</returns>
        string EncodeMessage(string message)
        {
            var publicKeyFilepath = $"{Environment.CurrentDirectory}/public.key";

            var publicKey = Convert.FromBase64String(System.IO.File.ReadAllLines(publicKeyFilepath)[0]);
            
            BigInteger E;
            // BigInteger D;
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
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task sendMessage(string email, string message)
        {
            var encodedMessage = EncodeMessage(message);
            
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
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Message written");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }
        /// <summary>
        /// Retrieve a public key from the server
        /// http://kayrun.cs.rit.edu:5000/Key/email
        /// </summary>
        /// <param name="email">The email to retrieve the key from</param>
        /// <returns></returns>
        public async Task getMessage(string email)
        {
            var responseBody = "";

            // first check if email is in private key
            // Submit it a GET Request
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
            
            // // try-catch to see if the response was null
            try
            {
                var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
                var messageEmail = json["email"];
                var messageContent = json["content"];
                
                var publicKeyFilepath = $"{Environment.CurrentDirectory}/{email}.key";

                var publicKey = System.IO.File.ReadAllLines(publicKeyFilepath);

                Console.WriteLine("Message Received"); // delete later
                // need to check if public key is in private key
                var decodedMessage = DecodeMessage(publicKey[0], (string)messageContent);
                Console.WriteLine(decodedMessage);

            }
            catch (NullReferenceException)
            {
                Console.Error.WriteLine($"The message from '{email}' does not exist!");
            }
        }
    }
}