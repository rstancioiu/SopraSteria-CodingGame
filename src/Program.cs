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
using SopraSteria_CodingGame.IATypes;

namespace SopraSteria_CodingGame
{
    public class Program
    {

        /*
        General method to send requests to the server : create, start, or stop the battle
        Parameters :
            - server contains the proper instance of the Server object
            - path contains the desired action, eg "/test/createBattle"
            - game_id should be set to -1 if it's not needed in this request
        Return value is a tuple :
            - First argument indicates if the request succeeded
            - Second arguments contains the response from the server, or the error
        */
        private static Tuple<bool, string> make_request(Server server, string path, long game_id)
        {
            try
            {
                UriBuilder uri_builder = new UriBuilder();
                uri_builder.Host = server.getHost();
                uri_builder.Port = server.getHttpPort();
                uri_builder.Path = path;
                uri_builder.Scheme = "http";
                var query = HttpUtility.ParseQueryString(uri_builder.Query);
                if (game_id != -1)
                    query["gameId"] = game_id.ToString();
                query["teamId"] = server.getRealTeamId().ToString();
                query["secret"] = server.getSecret();
                uri_builder.Query = query.ToString();
                Uri uri = uri_builder.Uri;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ServicePoint.Expect100Continue = false;
                request.ProtocolVersion = HttpVersion.Version11;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    return new Tuple<bool, string>(true, sr.ReadToEnd());
                }
            }
            catch (Exception uriException)
            {
                return new Tuple<bool, string>(false, uriException.ToString());
            }
        }

        public class PlayerStats
        {
            public int wins;
            public int totalScore;
            public PlayerStats()
            {
                this.wins = 0;
                this.totalScore = 0;
            }
        }

        /*
        Main method of the IA
        Here, we create, start and stop the battle if needed
        Instances of the Client class are also created and launched here
        */
        public static void Main(string[] args)
        {
            /* ----------------------------------------------------------------
                                   OFFLINE BENCHMARKING
            ----------------------------------------------------------------*/
            int NB_GAMES = 500;
            PlayerStats[] stats = new PlayerStats[6];

            for(int i=0; i<NB_GAMES; i++)
            {
                Game g = new Game();
                g.run();
                List<int> scores = g.result();
                int maxi = scores.Aggregate((s, s2) => s > s2 ? s : s2);
                for(int j=0; j < scores.Count; j++)
                {
                    ((PlayerStats)stats[j]).totalScore += scores[j];
                    if(scores[j] == maxi)
                    {
                        ((PlayerStats)stats[j]).wins++;
                    }
                }
            }
            Console.WriteLine("--------- BENCHMARK RESULTS ---------");
            Console.WriteLine("NB GAMES : " + NB_GAMES);
            int step = 0;
            foreach (PlayerStats p in stats)
            {
                Console.WriteLine("--- Player " + step);
                Console.WriteLine("     Wins : " + p.wins + " / " + NB_GAMES);
                Console.WriteLine("     AVG Score : " + p.totalScore/NB_GAMES);
            }

            return;
            /* ----------------------------------------------------------------
                                        ONLINE BATTLES
            ----------------------------------------------------------------*/
            Tuple<bool, string> request_manager;
            Server server = new Server();

            //Create battle
            request_manager = make_request(server, "/test/createBattle", -1);
            long game_id;
            if(request_manager.Item1)
            {
                game_id = long.Parse(request_manager.Item2);
                if(game_id==-1)
                {
                    Console.WriteLine("Could not create battle (one is already ongoing !)");
                    return;
                }
                else
                {
                    Console.WriteLine("Battle successfully created, game id : " + game_id);
                }
            }
            else
            {
                Console.WriteLine("Could not create battle, error : " + request_manager.Item2);
                return;
            }

            //Add CTRL-C handling to stop battle early if needed
            Console.CancelKeyPress += delegate
            {
                Console.WriteLine("Battle stopped.");
                make_request(server, "/test/stopBattle", game_id);
                return;
            };

            //Create players threads
            for (long i = server.getRealTeamId(); i < server.getMaxTeamId(); i++)
            {
                Client client = new Client(server, game_id, i, EnumIA.GREEDY);
                Thread thread = new Thread(new ThreadStart(client.run));
                thread.Start();
            }

            Console.WriteLine("Players created, waiting before starting game...");
            System.Threading.Thread.Sleep(10000);

            //Start the battle
            request_manager = make_request(server, "/test/startBattle", game_id);
            if (request_manager.Item1)
            {
                Console.WriteLine("Battle successfully started !");
            }
            else
            {
                Console.WriteLine("Could not start battle, error : " + request_manager.Item2);
                return;
            }

            Console.ReadKey();
        }
    }   
}
