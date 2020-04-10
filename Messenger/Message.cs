using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Messenger
{
    
    
    public class Message
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
        static readonly HttpClient client = new HttpClient();


        string DecryptMessage(string publicKeyFromSender, string message)
        {

            return "";
        }
        
        string EncryptMessage(string publicKey, string message)
        {

            return "";
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
            Console.WriteLine(responseBody);
            // // try-catch to see if the response was null
            try
            {
                var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
                var messageEmail = json["email"];
                var messageContent = json["content"];
                var messageTime = json["messageTime"];
            
                var emailFilepath = $"{Environment.CurrentDirectory}/{email}.key";
            
                // serialize the newly created json to be written to disk
                // using (StreamWriter outputFile = new StreamWriter(emailFilepath))
                // {
                //     outputFile.WriteLine(publicKey);
                // }
            }
            catch (NullReferenceException)
            {
                Console.Error.WriteLine($"The message from '{email}' does not exist!");
            }
        }
    }
}