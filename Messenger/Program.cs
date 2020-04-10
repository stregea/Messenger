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

        static async Task Main(string[] args)
        {
            new Key().keyGen(1024);
            // await sendKey("test@rit.edu");
            await new Key().sendKey("jeremy.brown@rit.edu");
            // await new Message().getMessage("jeremy.brown@rit.edu");
            //
            // await new Key().getKey("jeremy.brown@rit.edu");
            // await new Key().getKey("test@rit.edu");
            // await new Key().getKey("sam@rit.edu");
            // await sendKey("hello@rit.edu");
            // await sendKey("sam@rit.edu");
        }
    }
}