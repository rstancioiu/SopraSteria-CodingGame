using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace SopraSteria_CodingGame.ClientDetails
{
    public class Client
    {
        private string ipServer;
        private long teamId;
        private string secret;
        private int port;
        private long gameId;

	    public Client(string ipServer, long teamId, string secret, int port, long gameId)
	    {
            this.ipServer = ipServer;
            this.teamId = teamId;
            this.secret = secret;
            this.port = port;
            this.gameId = gameId;
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
                            int end = message.Length - start;
                            string subString = message.Substring(start);
                            char[] separator = { ';' };
                            string[] components = subString.Split(separator);
                            int round = Int32.Parse(components[0]);
                            Console.WriteLine(round);

                            string action = secret + "%%action::" + teamId + ";" + gameId + ";" + round + ";"
                                        + computeDirection();
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
