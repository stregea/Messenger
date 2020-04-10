using System;
using System.Diagnostics;
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
}