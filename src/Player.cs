using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SopraSteria_CodingGame.IATypes;

namespace SopraSteria_CodingGame
{
    //Inner class to describe a position
    public struct Position
    {
        public int x;
        public int y;
        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public static Position operator +(Position p1, Position p2)
        {
            return new Position(p1.x + p2.x, p1.y + p2.y);
        }
        public static bool operator ==(Position p1, Position p2)
        {
            return (p1.x==p2.x && p1.y==p2.y);
        }
        public static bool operator !=(Position p1, Position p2)
        {
            return (p1.x != p2.x || p1.y != p2.y);
        }
        public override bool Equals(object obj)
        {
            return (x == ((Position)obj).x && y == ((Position)obj).y);
        }
        public override int GetHashCode()
        {
            return 0;
        }
        public override string ToString()
        {
            return "(" + x + " - " + y + ")";
        }
    }

    //Inner class to describe a rabbit controlled by a player
    public class Rabbit
    {
        public Position pos;
        public bool isStunned;
        public int score;
        public int nbJumps;
        public bool hasLogo;
        public Rabbit(Position pos)
        {
            this.pos = pos;
            isStunned = false;
            score = 0;
            nbJumps = 0;
            hasLogo = false;
        }
    }

    //Player class controlled by a Client instance
    public class Player
    {
        public long playerId;
        private EnumIA typeIA;
        private long lastOpponent;

        private Dictionary<long, Position> posBaskets = new Dictionary<long, Position>();
        private Dictionary<long, Rabbit> rabbits = new Dictionary<long, Rabbit>();
        private List<Position> posLogos = new List<Position>();

        private const int MAX_X = 15;
        private const int MAX_Y = 12;
        private const int MAX_AVAILABLE_JUMPS = 3;

        private Position[] deltaPos =
        {
            new Position(-1, 0),
            new Position(1, 0),
            new Position(0, 1),
            new Position(0, -1)
        };
        Position NONE = new Position(-1, -1);

        //Guid : great seed for random in c#
        Random random_manager = new Random(Guid.NewGuid().GetHashCode());

        /* ---------------------------------------------------------------
                Player init and update methods
        ---------------------------------------------------------------*/

        //Constructor
        public Player(long playerId, EnumIA typeIA)
        {
            this.playerId = playerId;
            this.typeIA = typeIA;
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
                long idRabbit = long.Parse(info[0]);

                //Special case for jumps
                if (rabbits.ContainsKey(idRabbit))
                {
                    Rabbit rold = rabbits[idRabbit];
                    r.nbJumps = rold.nbJumps;
               
                    //Detect if this rabbit used a jump
                    if (distance(rold.pos, r.pos) > 1)
                        r.nbJumps--;
                }

                rabbits[idRabbit] = r;
            }

            //Now with the logos
            //Logos on the board go in posLogos
            //Logos carried by rabbits go inside rabbits structure
            posLogos.Clear();
            string logoInformation = message[2];
            string[] logos = logoInformation.Split(':');
            foreach(string l in logos)
            {
                string[] info = l.Split(',');
                Position p = new Position(int.Parse(info[0]), int.Parse(info[1]));
                List<Rabbit> rHere = rabbits.Where(k => k.Value.pos == p).Select(k => k.Value).ToList();
                if(rHere.Count > 0)
                    rHere.First().hasLogo = true;
                else
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
                    Position p = new Position(int.Parse(info[1]), int.Parse(info[2]));
                    long basketId = long.Parse(info[0]);
                    posBaskets[basketId] = p;
                }
            }
        }

        /* ---------------------------------------------------------------
                IA methods
        ---------------------------------------------------------------*/

        //Manhattan distance
        public int distance(Position p1, Position p2)
        {
            return (System.Math.Abs(p1.x - p2.x) + System.Math.Abs(p1.y - p2.y));
        }

        //Main IA method, to compute the next move for our rabbit
        public string computeMove()
        {
            switch (typeIA)
            {
                case EnumIA.RANDOM:
                    return computeRandom();
                case EnumIA.GREEDY:
                    string move = computeGreedy();
                    Console.WriteLine("=== Greedy IA : CHOSEN MOVE IS " + move);
                    return move;
                default:
                    break;
            }
            return "";
        }

        /* ---------------------------------------------------------------
                BFS : returns a list of Position to get from source to target
                The shortest path avoiding other rabbits is chosen
                But target can be a rabbit
        ---------------------------------------------------------------*/
        public List<Position> BFS(Position source, Position target)
        {
            Queue<Position> q = new Queue<Position>();
            List<Position> l = new List<Position>();
            //Default is false for boolean type so no worry
            bool[,] visited = new bool[16, 13];

            // 2D-Array that helps find the previous searched position of the current position
            Position[,] prevPos = new Position[16, 13];

            q.Enqueue(source);
            bool targetFound = false;
            while (q.Count > 0 && !targetFound)
            {
                Position lastPos = q.Dequeue();

                //For each adjacent position
                foreach(Position delta in deltaPos)
                {
                    Position newPos = lastPos + delta;

                    //If there is no rabbit here and it's not outside the map and it's not visited, add it to queue
                    List<Rabbit> rHere = rabbits.Where(k => k.Value.pos == newPos).Select(k => k.Value).ToList();
                    if(rHere.Count == 0 && check(newPos) && !visited[newPos.x, newPos.y])
                    {
                        // the previous position of the new postion is the current position
                        prevPos[newPos.x, newPos.y] = lastPos;
                        visited[newPos.x, newPos.y] = true;                    

                        //If we reached the target, return the current path
                        if (newPos == target)
                        {
                            targetFound = true;
                            break;
                        }
                        q.Enqueue(newPos);
                    }
                    else if(target==newPos)
                    {
                        prevPos[newPos.x, newPos.y] = lastPos;
                        visited[newPos.x, newPos.y] = true;
                        targetFound = true;
                        break;
                    }
                }
            }
            //Return an empty list if there is no solution
            //This might be possible if rabbits block the way
            if(!targetFound) return new List<Position>();
            
            // Compute the inverse path
            while (q.Count > 0) q.Dequeue();
            List<Position> ret = new List<Position>();
            Position aux = target;
            do
            {
                ret.Add(aux);
                aux = prevPos[aux.x, aux.y];
            } while (aux != source);
            ret.Add(source);
            // Reversing the path
            ret.Reverse();

            return ret;
        }

        /* ---------------------------------------------------------------
                RANDOM IA
        ---------------------------------------------------------------*/
        public string computeRandom()
        {
            string[] directions = { "N", "E", "S", "O" };
            int randomIndex = random_manager.Next(0, directions.Length);
            return directions[randomIndex];
        }

        /* ---------------------------------------------------------------
                GREEDY IA
        ---------------------------------------------------------------*/
        int availableJumps = 0;
        Dictionary<long, Rabbit> rabbitWhoStunned = new Dictionary<long, Rabbit>();

        public bool check(Position p)
        {
            if (p.x >= 0 && p.x <= MAX_X && p.y >= 0 && p.y <= MAX_Y)
                return true;
            return false;
        }
      
        public string computeGreedy()
        {
            Rabbit rabbitPlayer = rabbits[playerId];
            Position nextStep = NONE;
            bool DEBUG = false;

            //If we are stunned, nothing to be done
            if (rabbitPlayer.isStunned)
            {
                return "";
            }

            //If we carry a logo, we need to go to the basket
            if (rabbitPlayer.hasLogo)
            {
                if (DEBUG)
                    Console.WriteLine("--- Greedy IA : Go to basket");
                Position myBasket = posBaskets[playerId];
                List<Position> pathToBasket = BFS(rabbitPlayer.pos, myBasket);
                if (pathToBasket.Count > 0)
                {
                    nextStep = pathToBasket[1];
                }

                // Do special moves when the rabbit is near another rabbit or use of super jump
                foreach(KeyValuePair<long,Rabbit> rabbit in rabbits)
                {
                    if (rabbit.Key != playerId )
                    {
                        int rabbitId = (int)rabbit.Key;
                        
                        // The rabbit is 2 case away form a rabbit with a logo -> stun him
                        if (rabbit.Value.hasLogo && distance(rabbitPlayer.pos, rabbit.Value.pos) == 2 && 
                            rabbit.Key != lastOpponent && rabbit.Value.pos!=posBaskets[rabbit.Key] && !rabbitWhoStunned.ContainsKey(rabbit.Key))
                        {
                            List<Position> chosenPath = BFS(rabbitPlayer.pos, rabbit.Value.pos);
                            nextStep = chosenPath[1];
                            rabbitWhoStunned.Clear();
                            rabbitWhoStunned.Add(rabbit.Key,rabbit.Value);
                            break;
                        }

                        // The rabbit is one case away from a stunned rabbit and has a logo -> double jump
                        else if (rabbit.Value.isStunned && distance(rabbitPlayer.pos, rabbit.Value.pos) == 1 && availableJumps<MAX_AVAILABLE_JUMPS)
                        {
                            Position newStep = new Position();
                            newStep.x = (nextStep.x == rabbitPlayer.pos.x) ? nextStep.x : rabbitPlayer.pos.x - (rabbitPlayer.pos.x - nextStep.x) * 2;
                            newStep.y = (nextStep.y == rabbitPlayer.pos.y) ? nextStep.y : rabbitPlayer.pos.y - (rabbitPlayer.pos.y - nextStep.y) * 2;
                            if (check(newStep))
                            {
                                nextStep = newStep;
                                availableJumps++;
                                break;
                            }
                        }
                    }
                }
            }

            //Otherwise, we need to get a logo somewhere
            else
            {
                // Do special moves when the rabbit is near another rabbit or use of super jump
                foreach (KeyValuePair<long, Rabbit> rabbit in rabbits)
                {
                    if (rabbit.Key != playerId)
                    {
                        int rabbitId = (int)rabbit.Key;

                        // The rabbit is 2 case away form a rabbit with a logo -> stun him 
                        // Though he is safe when he is home
                        if (rabbit.Value.hasLogo && distance(rabbitPlayer.pos, rabbit.Value.pos) == 2 &&
                            rabbit.Key != lastOpponent && rabbit.Value.pos != posBaskets[rabbit.Key] &&  !rabbitWhoStunned.ContainsKey(rabbit.Key))
                        {
                            List<Position> chosenPath = BFS(rabbitPlayer.pos, rabbit.Value.pos);
                            nextStep = chosenPath[1];
                            rabbitWhoStunned.Clear();
                            rabbitWhoStunned.Add(rabbit.Key, rabbit.Value);
                            break;
                        }
                        // The rabbit is one case away from a stunned rabbit and has a logo -> double jump
                        else if (rabbit.Value.isStunned && distance(rabbitPlayer.pos, rabbit.Value.pos) == 1 && availableJumps < MAX_AVAILABLE_JUMPS)
                        {
                            Position newStep = new Position();
                            newStep.x = (nextStep.x == rabbitPlayer.pos.x) ? nextStep.x : rabbitPlayer.pos.x - (rabbitPlayer.pos.x - nextStep.x) * 2;
                            newStep.y = (nextStep.y == rabbitPlayer.pos.y) ? nextStep.y : rabbitPlayer.pos.y - (rabbitPlayer.pos.y - nextStep.y) * 2;
                            if (check(newStep))
                            {
                                nextStep = newStep;
                                availableJumps++;
                                break;
                            }
                        }
                    }
                }

                //If there are free logos remaining
                if (posLogos.Count > 0)
                {
                    if (DEBUG)
                        Console.WriteLine("--- Greedy IA : There are free logos");
                    //Sort them by distance
                    List<Position> logos = new List<Position>(posLogos);
                    logos.Sort(delegate (Position p1, Position p2)
                    {
                        return distance(rabbitPlayer.pos, p1).CompareTo(distance(rabbitPlayer.pos, p2));
                    });

                    if (DEBUG)
                        Console.WriteLine("--- Greedy IA : Best logo is @" + logos.First());
                    //Try to go to the closest one, or next ones if impossible
                    foreach (Position logo in logos)
                    {
                        List<Position> pathToLogo = BFS(rabbitPlayer.pos, logo);
                        if (pathToLogo.Count > 0)
                        {
                            nextStep = pathToLogo[1];
                            if (DEBUG)
                                Console.WriteLine("--- Greedy IA : Moving towards logo " + logo + " by pos " + nextStep);
                            break;
                        }
                    }
                }

                //Or if there was no free logo or no path could be found, we can steal an other rabbit
                if (nextStep == NONE)
                {
                    //Get the positions of rabbits with logos that we can hit, so 3 conditions :
                    // - The rabbit structure verifies the boolean hasLogo
                    // - The rabbit identifier isn't our last opponent
                    // - The rabbit is at least one block away
                    // - The rabbit is not the one who the rabbit stunned previously
                    List<Position> rabbitsWithLogos = rabbits.Where(r => (r.Value.hasLogo
                                                                    && r.Key != lastOpponent
                                                                    && distance(r.Value.pos, rabbitPlayer.pos) >= 1
                                                                    && !rabbitWhoStunned.ContainsKey(r.Key)))
                                                                    .Select(r => r.Value.pos).ToList();

                    if (DEBUG)
                        Console.WriteLine("--- Greedy IA : Trying to steal rabbits, candidates : " + rabbitsWithLogos.Count);

                    if(rabbitsWithLogos.Count>0)
                    {
                        int minimum = 0x7fffffff; 
                        //Try to go to the closest one, or next ones if impossible
                        foreach (Position rtarget in rabbitsWithLogos)
                        {
                            List<Position> pathToRabbit = BFS(rabbitPlayer.pos, rtarget);
                            if (pathToRabbit.Count > minimum)
                            {
                                minimum = pathToRabbit.Count;
                                nextStep = pathToRabbit[1];
                                if (DEBUG)
                                    Console.WriteLine("--- Greedy IA : Moving towards rabbit " + rtarget + " by pos " + nextStep);
                            }
                        }
                    }
                    else
                    {
                        foreach(Position delta in deltaPos)
                        {
                            if(check(delta+rabbitPlayer.pos))
                            {
                                nextStep = delta + rabbitPlayer.pos;
                                break;
                            }
                        }
                    }
                }
            }

            //Update the structure if we moved and hit someone
            if(nextStep != NONE)
            {
                List<KeyValuePair<long, Rabbit>> rNext = rabbits.Where(k =>
                                                         (distance(k.Value.pos, nextStep) == 1)).ToList();
                //Hit a single rabbit
                if (rNext.Count == 1)
                {
                    if (DEBUG)
                        Console.WriteLine("--- Greedy IA : Geez ! We just hit rabbit @pos " + rNext.First().Value.pos);
                    long r2 = rNext.First().Key;
                    lastOpponent = r2;
                }
                //Combo
                else if (rNext.Count > 1)
                {
                    if (DEBUG)
                        Console.WriteLine("--- Greedy IA : Geez ! We just hit many rabbits");
                    lastOpponent = -1;
                }

                //Return the correct action
                if (nextStep.x - rabbitPlayer.pos.x == 1)
                    return "E";
                if (nextStep.x - rabbitPlayer.pos.x == -1)
                    return "O";
                if (nextStep.y - rabbitPlayer.pos.y == 1)
                    return "S";
                if (nextStep.y - rabbitPlayer.pos.y == -1)
                    return "N";
                if (nextStep.x - rabbitPlayer.pos.x == 2)
                    return "JE";
                if (nextStep.x - rabbitPlayer.pos.x == -2)
                    return "JO";
                if (nextStep.y - rabbitPlayer.pos.y == 2)
                    return "JS";
                if (nextStep.y - rabbitPlayer.pos.y == -2)
                    return "JN";
                //Duh, mistake
                return "";
            }
            //No good action was found
            else
            {
                return "";
            }
        }
    }
}
