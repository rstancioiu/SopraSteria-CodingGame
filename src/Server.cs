using System;


namespace SopraSteria_CodingGame.ServerDetails
{
    public class Server
    {

        private string host;
        private long realTeamId;
        private string secret;
        private long maxTeamId;
        private int port;
        private int httpPort;

        public Server()
        {
            host = "52.29.48.22";
            realTeamId = 50;
            secret = "6Q8TL9A6S7";
            maxTeamId = realTeamId + 6;
            port = 2050;
            httpPort = 8080;
        }

        public string getHost()
        {
            return host;
        }

        public long getRealTeamId()
        {
            return realTeamId;
        }

        public string getSecret()
        {
            return secret;
        }

        public long getMaxTeamId()
        {
            return maxTeamId;
        }

        public int getPort()
        {
            return port;
        }

        public int getHttpPort()
        {
            return httpPort;
        }
    }
}