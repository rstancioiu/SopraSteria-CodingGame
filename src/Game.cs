using System;
using System.Collections.Generic;
using SopraSteria_CodingGame.PlayerDetails;
using SopraSteria_CodingGame.IATypes;

public class Game
{
    List<Player> players = new List<Player>();
    const int MAX_NUMBER_PLAYERS = 6;
    const int MAX_X = 15;
    const int MAX_Y = 12;
    const int TOURS = 50;
    const int MAX_NUMBER_LOGOS = 10;
    List<Position> caddies = new List<Position>();
    List<Position> rabbits = new List<Position>();
    List<Position> logos = new List<Position>();
    List<string> state = new List<string>();
    List<bool> stateLogos = new List<bool>();
    int round;

	public Game()
	{
        for(int i=0;i< MAX_NUMBER_PLAYERS;++i)
        {
            Player player = new Player(i, EnumIA.GREEDY);
            players.Add(player);
            state.Add("playing");
        }
        round = 1;
        caddies.Add(new Position(1, 1));
        caddies.Add(new Position(1, 3));
        caddies.Add(new Position(1, 5));
        caddies.Add(new Position(1, 7));
        caddies.Add(new Position(1, 9));
        caddies.Add(new Position(1, 11));
        rabbits.Add(new Position(1, 1));
        rabbits.Add(new Position(1, 3));
        rabbits.Add(new Position(1, 5));
        rabbits.Add(new Position(1, 7));
        rabbits.Add(new Position(1, 9));
        rabbits.Add(new Position(1, 11));
        
	}

    public void start_game()
    {
        Random rnd = new Random();
        for (int i = 0; i < MAX_NUMBER_LOGOS; ++i )
        {
            int p1 = rnd.Next(MAX_X / 2 + 2, MAX_X+1);
            int p2 = rnd.Next(MAX_Y+1);
            logos.Add(new Position(p1, p2));
            stateLogos.Add(true);
        }


        for(round = 1 ; round <= TOURS; ++round)
        {
            for (int i = 0; i < MAX_NUMBER_PLAYERS; ++i)
            {
                players[i].updateWorld(generateString());
                string move = players[i].computeMove();
            }
        }

    }

    public string[] generateString()
    {
        string ret = "worldstate::";
        ret += round.ToString() + ";";
        for (int i = 0; i < MAX_NUMBER_PLAYERS; ++i)
        {
            if(i<MAX_NUMBER_PLAYERS-1)
            ret += i.ToString() + "," + rabbits[i].x.ToString() + "," + rabbits[i].y.ToString() + "," + state[i] + ":";
            else
                ret += i.ToString() + "," + rabbits[i].x.ToString() + "," + rabbits[i].y.ToString() + "," + state[i] + ";";
        }
        int j = -1;
        int last = j;
        for (int i = 0; i < MAX_NUMBER_LOGOS; ++i)
        {
            if(stateLogos[i])
            {
                if(last!=-1)
                    ret += logos[last].x + "," + logos[last].y + ":";
                last = i;
            }
        }
        if(last != -1 )
            ret += logos[last].x + "," + logos[last].y + ";";
        for (int i = 0; i < MAX_NUMBER_PLAYERS;++i)
        {
            if(i<MAX_NUMBER_PLAYERS-1)
                ret += i.ToString() + "," + caddies[i].x + "," + caddies[i].y +":";
            else
                ret += i.ToString() + "," + caddies[i].x + "," + caddies[i].y + ";";
        }
        int start = "worldstate::".Length;
        string subString = ret.Substring(start);
        string[] components = subString.Split(';');
        return components;
    }
}
