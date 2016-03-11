using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;
using System.Net;
using System.IO;
using System.Threading;
using SopraSteria_CodingGame.ServerDetails;
using SopraSteria_CodingGame.ClientDetails;

namespace SopraSteria_CodingGame
{

    public class Program
    {
        public static long create_game(Server server)
        {

            try
            {
                Console.WriteLine("Starting the test client");
                //Start of the game
                

                UriBuilder uri_builder = new UriBuilder();
                uri_builder.Host = server.getHost();
                uri_builder.Port = server.getHttpPort();
                uri_builder.Path = "/test/createBattle";
                uri_builder.Scheme = "http";
                var query = HttpUtility.ParseQueryString(uri_builder.Query);
                query["teamId"] = server.getRealTeamId().ToString();
                query["secret"] = server.getSecret();
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
                    string s = sr.ReadToEnd();
                    return long.Parse(s);
                  
                }

            }
            catch (Exception uriException)
            {
                Console.WriteLine(uriException.ToString());
                Console.WriteLine("A problem has occured");
            }
            return 0;
        }

        public static void start_game(Server server,long game_id)
        {
            try
            {
                Console.WriteLine("We are starting the game");
                UriBuilder uri_builder = new UriBuilder();
                uri_builder.Scheme = "http";
                uri_builder.Host = server.getHost();
                uri_builder.Port = server.getHttpPort();
                uri_builder.Path = "/test/startBattle";
                var query = HttpUtility.ParseQueryString(uri_builder.Query);
                query["gameId"] = game_id.ToString();
                query["teamId"] = server.getRealTeamId().ToString();
                query["secret"] = server.getSecret();
                uri_builder.Query = query.ToString();
                Console.WriteLine(uri_builder.ToString());
                Uri uri = uri_builder.Uri;
                Console.WriteLine(uri);


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ServicePoint.Expect100Continue = false;
                request.ProtocolVersion = HttpVersion.Version11;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    Console.WriteLine(sr.ReadToEnd());
                }
            }
            catch (Exception uriException)
            {
                Console.WriteLine(uriException.ToString());
                Console.WriteLine("A problem has occured");
            }
        }

        public static void stop_game(Server server, long game_id)
        {
            try
            {
                Console.WriteLine("We are stopping the game");
                UriBuilder uri_builder = new UriBuilder();
                uri_builder.Scheme = "http";
                uri_builder.Host = server.getHost();
                uri_builder.Port = server.getHttpPort();
                uri_builder.Path = "/test/stopBattle";
                var query = HttpUtility.ParseQueryString(uri_builder.Query);
                query["gameId"] = game_id.ToString();
                query["teamId"] = server.getRealTeamId().ToString();
                query["secret"] = server.getSecret();
                uri_builder.Query = query.ToString();
                Console.WriteLine(uri_builder.ToString());
                Uri uri = uri_builder.Uri;
                Console.WriteLine(uri);


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ServicePoint.Expect100Continue = false;
                request.ProtocolVersion = HttpVersion.Version11;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                
            }
            catch (Exception uriException)
            {
                Console.WriteLine(uriException.ToString());
                Console.WriteLine("A problem has occured");
            }
           
        }

        public static void Main(string[] args)
        {

            Server server = new Server();
            long game_id;
            game_id =  create_game(server);
            Console.CancelKeyPress += delegate
            {
                stop_game(server, game_id);
            };
            Console.WriteLine("Game_id : "+game_id);
            for (long i = server.getRealTeamId(); i < server.getMaxTeamId(); i++)
            {
                Client client = new Client(server.getHost(), i, server.getSecret(), server.getPort(), game_id);
                Thread thread = new Thread(new ThreadStart(client.run));
                thread.Start();
            }
            System.Threading.Thread.Sleep(1000);
            start_game(server,game_id);
            Console.ReadKey();
        }
    }   
}
