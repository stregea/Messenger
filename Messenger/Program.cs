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
            
            // await new Key().sendKey("exc5774@rit.edu");
            // await new Key().sendKey("sdt1093@rit.edu");
            // await new Key().sendKey("mjc282334@rit.edu");
            
            // await new Message().getMessage("jsb@cs.rit.edu");

            await new Key().sendKey("sdt1093@rit.edu");
            // //
            await new Key().getKey("sdt1093@rit.edu");
            //
            await new Message().sendMessage("sdt1093@rit.edu", "this is a test");
            // await new Key().getKey("jsb@cs.rit.edu");

            // await new Message().getMessage("sdt1093@rit.edu");
            await new Message().getMessage("sdt1093@rit.edu");
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