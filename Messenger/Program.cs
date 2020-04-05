using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Messenger
{
    
        /// <summary>
    /// Static class that is used for an BigInteger object to call to determine if it's a prime number
    /// </summary>
    static class Prime
    {
        /// <summary>
        /// Determine if the number is most likely prime
        /// </summary>
        /// <param name="value">The number being checked</param>
        /// <param name="witnesses">The bumber if witnesses</param>
        /// <returns>True if prime, false otherwise.</returns>
        public static Boolean IsProbablyPrime(this BigInteger value, int witnesses = 10) {
            
            if (value <= 1) return false;
            
            if (witnesses <= 0) witnesses = 10;
            
            BigInteger d = value - 1; int s = 0;
            
            while (d % 2 == 0) {
                d /= 2;
                s += 1;
            }
            
            Byte[] bytes = new Byte[value.ToByteArray().LongLength]; 
            BigInteger a;
            for (int i = 0; i < witnesses; i++) {
                do {
                    var Gen = new Random(); Gen.NextBytes(bytes);
                    a = new BigInteger(bytes);
                } while (a < 2 || a >= value - 2);
                
                BigInteger x = BigInteger.ModPow(a, d, value); 
                if (x == 1 || x == value - 1) continue;
                
                for (int r = 1; r < s; r++) {
                    x = BigInteger.ModPow(x, 2, value); 
                    if (x == 1) return false;
                    if (x == value - 1) break;
                }
                if (x != value - 1) return false; }
            return true;
        }
    }

    /// <summary>
    /// Class that has the capability of creating a random prime number of size 'n' bits.
    /// </summary>
    public class PrimeNumberGenerator
    {
        // used to randomly fill a byte array.
        RNGCryptoServiceProvider csp = new RNGCryptoServiceProvider();
        
        // lock for parallel threading.
        object myLock = new object();

        /// <summary>
        /// Generate a random prime number of 'n' bits.
        /// </summary>
        /// <param name="bits">The number of bits of the size of the prime number to generate.</param>
        public BigInteger GeneratePrimeNumber(int bits)
        {
            var sw = new Stopwatch();
            var counter = 0;
            BigInteger ret = 0;
            
            // may not need to use parallel for in future
            Parallel.For(0, Int32.MaxValue, (i, state) =>
            {
                var bytes = new byte[bits / 8];
                csp.GetBytes(bytes); // fill the byte array
                var number = BigInteger.Abs(new BigInteger(bytes)); // taking absolute value to guarantee no negatives

                if (counter < 1)
                {
                    if (number.IsProbablyPrime())
                    {
                        lock (myLock)
                        {
                            ret = number;
                            counter++;
                        }
                    }
                }
                else
                {
                    state.Stop(); // terminate the rest of the threads
                }
            });
            return ret;
        }


        public BigInteger GenerateE(int bits, BigInteger upper)
        {
            var ret = (BigInteger) 65536;

            var tmp = GeneratePrimeNumber(bits);


            if (tmp >= 3 && tmp <= upper)
            {
                return tmp;
            }

            return ret;

        }
    }
    class Program
    {
    
        // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
        static readonly HttpClient client = new HttpClient();
 
        static async Task Test()
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try	
            {
                Console.WriteLine("here");
                HttpResponseMessage response = await client.GetAsync("http://kayrun.cs.rit.edu:5000/Message/user@foo.com");
                // HttpResponseMessage response = await client.PostAsync("http://kayrun.cs.rit.edu:5000/Message/email");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(responseBody);
            }  
            catch(HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");	
                Console.WriteLine("Message :{0} ",e.Message);
            }
        }


        /// <summary>
        /// Calculate the modulo inverse between two numbers.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        static BigInteger modInverse(BigInteger a, BigInteger n)
        {
            BigInteger i = n, v = 0, d = 1;
            while (a>0) {
                BigInteger t = i/a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t*x;
                v = x; }
            v %= n;
            if (v<0) v = (v+n)%n;
            return v;
        }

        /// <summary>
        /// Generate a Public and Private Key
        /// </summary>
        /// <param name="keysize"></param>
        static void keyGen(int keysize)
        {
            var p = new PrimeNumberGenerator().GeneratePrimeNumber(keysize / 2);
            var q = new PrimeNumberGenerator().GeneratePrimeNumber(keysize / 2);
            var r = (p - 1) * (q - 1);
            var N = p * q;
            var E = new PrimeNumberGenerator().GenerateE(keysize, r);
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
            var public_key_base64 = Convert.ToBase64String(public_key, 0, public_key.Length);
            var private_key_base64 = Convert.ToBase64String(private_key, 0, public_key.Length);

            // filepath
            var public_filepath = $"{Environment.CurrentDirectory}/public.key";
            var private_filepath = $"{Environment.CurrentDirectory}/private.key";

            // write to the filepath
            using (StreamWriter outputFile = new StreamWriter(public_filepath))
            {
                outputFile.WriteLine(public_key_base64);
            }
            
            using (StreamWriter outputFile = new StreamWriter(private_filepath))
            {
                outputFile.WriteLine(private_key_base64);
            }
        }

        static async Task sendKey(string email)
        {
            // read key from disk
            var public_key_path = $"{Environment.CurrentDirectory}/public.key";
            var public_key = "";
            if (File.Exists(public_key_path)) {  
                // Read entire text file content in one string    
                public_key = File.ReadAllText(public_key_path);  
                Console.WriteLine(public_key);  
            }

            var info = new Dictionary<string, string>
            {
                {"email", email},
                {"key", public_key}
            };
            
            var json = new FormUrlEncodedContent(info);
            // var response = await client.GetAsync("http://kayrun.cs.rit.edu:5000/Key/user@foo.com");
            var response = await client.PostAsync($"http://kayrun.cs.rit.edu:5000/Key/{email}", json);

            var responseString = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine(responseString);
            Console.WriteLine(response.StatusCode);

            // json
        }
        static async Task Main(string[] args)
        {
            // keyGen(1024);
            await sendKey("test@rit.edu");
        }
    }
}