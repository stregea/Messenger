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
    public class Key
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
        static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Calculate the modulo inverse between two numbers.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static BigInteger modInverse(BigInteger a, BigInteger n)
        {
            BigInteger i = n, v = 0, d = 1;
            while (a > 0)
            {
                BigInteger t = i / a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t * x;
                v = x;
            }

            v %= n;
            if (v < 0) v = (v + n) % n;
            return v;
        }

        /// <summary>
        /// Generate a Public and Private Key
        /// </summary>
        /// <param name="keysize"></param>
        public void keyGen(int keysize)
        {
            var p = new PrimeNumberGenerator().GeneratePrimeNumber(keysize / 2);
            var q = new PrimeNumberGenerator().GeneratePrimeNumber(keysize / 2);
            
            var r = (p - 1) * (q - 1);
            var N = p * q;
            BigInteger E = new BigInteger(65537);
            var D = modInverse(E, r);
            
            var E_Array = E.ToByteArray();
            var D_Array = D.ToByteArray();
            var N_Array = N.ToByteArray();

            var e = BitConverter.GetBytes(E_Array.Length);
            var d = BitConverter.GetBytes(D_Array.Length);
            var n = BitConverter.GetBytes(N_Array.Length);
     
            // check little endian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(e);
                Array.Reverse(d);
                Array.Reverse(n);
            }

            // eeeeEEE...EEEnnnnNNN..NNN
            var public_key = e.Concat(E_Array).Concat(n).Concat(N_Array).ToArray();
            
            // ddddDDD....DDnnnnNNN...NNNN
            var private_key = d.Concat(D_Array).Concat(n).Concat(N_Array).ToArray();

            // convert to base 64
            var public_key_base64 = Convert.ToBase64String(public_key);
            var private_key_base64 = Convert.ToBase64String(private_key);

            // filepath
            var public_filepath = $"{Environment.CurrentDirectory}/public.key";
            var private_filepath = $"{Environment.CurrentDirectory}/private.key";


            // write to the filepath for the public key
            using (StreamWriter outputFile = new StreamWriter(public_filepath))
            {
                outputFile.Write(public_key_base64);
            }

            var listOfEmails = new List<string>();
            
            // build a json stub for the private key
            var info = new Dictionary<string, object>
            {
                {"email", listOfEmails},
                {"key", public_key_base64},
            };
            // serialize the newly created json to be sent to the server
            var json = JsonConvert.SerializeObject(info);

            using (StreamWriter outputFile = new StreamWriter(private_filepath))
            {
                outputFile.WriteLine(private_key_base64);
                outputFile.WriteLine(json);
            }
            
            Console.WriteLine("Public and Private keys generated.");
        }

        /// <summary>
        /// Send a key to a server with HTTP PUT
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task sendKey(string email)
        {
            // read key from disk
            var private_key_path = $"{Environment.CurrentDirectory}/private.key";
            var privateKey = "";

            // read in the private key into an array from memory
            // [0] - private key information
            // [1] - JSON information
            var dataFromKey = System.IO.File.ReadAllLines(private_key_path);

            privateKey = dataFromKey[0];
            var jsonFromKey = dataFromKey[1];

            /*
             * Dynamically update the 'email' key in the JSON.
             *
             *  - Create a list of the current emails
             *  - Check to see if the email being sent in exists
             *    - If it doesn't exists within the list of emails, add it.
             *  - Update the JSON with the new list of emails.
             */

            var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonFromKey);
            var emailList = json["email"];
            var publicKey = json["key"];

            // build the list of all the emails in the JSON
            var listOfEmails = new List<string>();
            foreach (var emails in (JArray) emailList)
            {
                listOfEmails.Add(emails.ToString());
            }

            // if the email in the parameters doesn't already exist within the list, add it.
            if (!listOfEmails.Contains(email))
            {
                listOfEmails.Add(email);
            }

            // json for updating the private key on machine
            var localInfo = new Dictionary<string, object>
            {
                {"email", listOfEmails},
                {"key", publicKey},
            };

            // serialize the newly created json to be sent to the server
            var serverInfo = new Dictionary<string, object>
            {
                {"email", email},
                {"key", publicKey},
            };

            var jsonForMachine = JsonConvert.SerializeObject(localInfo);
            var serverJSON = JsonConvert.SerializeObject(serverInfo);

            // re-write the file
            using (StreamWriter outputFile = new StreamWriter(private_key_path, false))
            {
                outputFile.WriteLine(privateKey);
                outputFile.WriteLine(jsonForMachine);
            }

            var request = new StringContent(serverJSON, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PutAsync($"http://kayrun.cs.rit.edu:5000/Key/{email}", request);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Key Saved");
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
        public async Task getKey(string email)
        {
            var responseBody = "";

            // Submit it a GET Request
            try
            {
                HttpResponseMessage response = await client.GetAsync($"http://kayrun.cs.rit.edu:5000/Key/{email}");
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
                var emailList = json["email"];
                var publicKey = json["key"];

                var emailFilepath = $"{Environment.CurrentDirectory}/{email}.key";

                // serialize the newly created json to be written to disk
                using (StreamWriter outputFile = new StreamWriter(emailFilepath))
                {
                    outputFile.WriteLine(publicKey);
                }
            }
            catch (NullReferenceException)
            {
                Console.Error.WriteLine($"The key for '{email}' does not exist!");
            }
        }
    }
}