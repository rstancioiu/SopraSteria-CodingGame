using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SopraSteria_CodingGame
{
    //Inner class to describe a position
    public class Position
    {
        public int x;
        public int y;
        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    //Inner class to describe a rabbit controlled by a player
    public class Rabbit
    {
        public Position pos;
        public bool isStunned;
        public int score;
        public Rabbit(Position pos)
        {
            this.pos = pos;
            isStunned = false;
        }
    }

    //Player class controlled by a Client instance
    public class Player
    {
        public long playerId;
        private Dictionary<long, Position> posBaskets = new Dictionary<long, Position>();
        private Dictionary<long, Rabbit> rabbits = new Dictionary<long, Rabbit>();
        private List<Position> posLogos = new List<Position>();
        //Guid : great seed for random in c#
        Random random_manager = new Random(Guid.NewGuid().GetHashCode());

        //Constructor
        public Player(long playerId)
        {
            this.playerId = playerId;
        }

        //Updates the player's vision of the world given server information
        public void updateWorld(string[] message)
        {
            //Get information regarding the rabbits
            string playerInformation = message[1];
            string[] players = playerInformation.Split(':');
            foreach(string p in players)
            {
                string[] info = p.Split(',');
                Position pos = new Position(int.Parse(info[1]), int.Parse(info[2]));
                Rabbit r = new Rabbit(pos);
                r.score = int.Parse(info[3]);
                r.isStunned = (info[4] == "stunned");
                rabbits[long.Parse(info[0])] = r;
            }

            //Now with the logos
            posLogos.Clear();
            string logoInformation = message[2];
            string[] logos = logoInformation.Split(':');
            foreach(string l in logos)
            {
                string[] info = l.Split(',');
                Position p = new Position(int.Parse(info[0]), int.Parse(info[1]));
                posLogos.Add(p);
            }

            //And finally, set the baskets if this is first round
            int round = Int32.Parse(message[0]);
            if(round==1)
            {
                string basketInformation = message[3];
                string[] baskets = basketInformation.Split(':');
                foreach (string b in baskets)
                {
                    string[] info = b.Split(',');
                    Position p = new Position(int.Parse(info[0]), int.Parse(info[1]));
                    long playerId = long.Parse(info[2]);
                    posBaskets[playerId] = p;
                }
            }
        }

        public string computeMove()
        {
            string[] directions = { "N", "E", "S", "O" };
            int randomIndex = random_manager.Next(0, directions.Length);
            return directions[randomIndex];
        }
    }
}
