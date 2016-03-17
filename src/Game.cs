using System;
using System.Collections.Generic;
using SopraSteria_CodingGame.PlayerDetails;

public class Game
{
    List<Player> players = new List<Player>();
    const int MAX_NUMBER_PLAYERS = 6;
    const int STARTING_ID = 50;

	public Game()
	{
        for(int i=STARTING_ID;i< STARTING_ID+MAX_NUMBER_PLAYERS;++i)
        {

        }
	}
}
