using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;
using System.Net;
using System.IO;

namespace SopraSteria_CodingGame
{

    class Server
    {
        public string host;
        public long realteamId;
        public string secret;
        public long maxteamId;
        public int socketNumber;

        public Server()
        {
            host = "52.29.48.22";
            realteamId = 50;
            secret = "6Q8TL9A6S7";
            maxteamId = realteamId + 6;
            socketNumber = 2050;
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting the test client");
                //Start of the game

                Server server = new Server();

                UriBuilder uri_builder = new UriBuilder();
                uri_builder.Host = server.host;
                uri_builder.Port = server.socketNumber;
                uri_builder.Path = "/test/createBattle";
                uri_builder.Scheme = "http";
                var query = HttpUtility.ParseQueryString(uri_builder.Query);
                query["teamId"] = server.realteamId.ToString();
                query["secret"] = server.secret;
                uri_builder.Query = query.ToString();
                Console.WriteLine(uri_builder.ToString());
                Uri uri = uri_builder.Uri;
                Console.WriteLine(uri.Query);



                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ServicePoint.Expect100Continue = false;
                request.ProtocolVersion = HttpVersion.Version11;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    Console.WriteLine(sr.ReadToEnd());

                    Console.WriteLine(server.host);
                }
                
            }
            catch (Exception uriException)
            {
                Console.WriteLine(uriException.ToString());
                Console.WriteLine("A problem has occured");
            }

            Console.ReadKey();
        }
    }
}
