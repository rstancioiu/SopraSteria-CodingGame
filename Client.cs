using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using SopraSteria_CodingGame.ServerDetails;

namespace SopraSteria_CodingGame.ClientDetails
{
    public class Client
    {
        //Everything needed to communicate with the server
        private string ipServer;
        private long teamId;
        private string secret;
        private int port;
        private long gameId;

        //Player instance to compute moves
        Player player;

	    public Client(Server server, long gameId, long teamId)
	    {
            //Server communication related
            this.ipServer = server.getHost();
            this.secret = server.getSecret();
            this.port = server.getPort();
            this.gameId = gameId;
            this.teamId = teamId;

            //World related
            this.player = new Player(teamId);
	    }

        public void run()
        {
            Console.WriteLine("Client started");
            IPEndPoint serverAddress = new IPEndPoint(IPAddress.Parse(ipServer), port);
            Console.WriteLine("Server : " + serverAddress.ToString());
            TcpClient clientSocket = new TcpClient();
            clientSocket.Connect(serverAddress);

            NetworkStream serverStream = clientSocket.GetStream();
            StreamReader rd = new StreamReader(serverStream);
            StreamWriter wr = new StreamWriter(serverStream);
            try
            {
                Console.WriteLine("Connection to game : " + gameId + " - team id : " + teamId);
                wr.WriteLine(secret + "%%inscription::" + gameId.ToString() + ";" + teamId.ToString());
                wr.Flush();
                string message = null;
                do
                {
                    message=rd.ReadLine();
                    
                    Console.WriteLine("Server raw response : " + message);
                    if (message != null)
                    {
                        if (message.Equals("Inscription OK"))
                        {
                            Console.WriteLine("I am in the game");
                        }
                        else if (message.StartsWith("worldstate::"))
                        {
                            int start = "worldstate::".Length;
                            string subString = message.Substring(start);
                            string[] components = subString.Split(';');
                            player.updateWorld(components);

                            int round = Int32.Parse(components[0]);
                            string action = secret + "%%action::" + teamId + ";" + gameId + ";" + round + ";"
                                        + player.computeMove();

                            wr.WriteLine(action);
                            wr.Flush();
                        }
                        else if (message.Equals("Inscription KO"))
                        {
                            Console.WriteLine("inscription KO");
                        }
                        else if (message.Equals("game over"))
                        {
                            Console.WriteLine("game over");

                        }
                        else if (message.Equals("action OK"))
                        {
                            Console.WriteLine("The Action was taken into consideration");
                        }
                    }
                } while (message != null);
                Console.WriteLine("Message null");
                rd.Close();
                wr.Close();
                serverStream.Close();
                clientSocket.Close();
            }
            catch(Exception e)
            {
                rd.Close();
                wr.Close();
                serverStream.Close();
                clientSocket.Close();
                Console.WriteLine(e.ToString());
            }

        }
    
        private string computeDirection()
        {
            return "E";
        }
    }
}
