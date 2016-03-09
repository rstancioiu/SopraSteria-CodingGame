

import java.io.IOException;
import java.net.URISyntaxException;

public class LauncherFinal {
	
	private static final String ipServer = "xxxxxxxx"; // donne le jour de la battle
	private static final long teamId = 0000000; // a renseigner par votre valeur
	private static final String secret = "xxxxxxxxxxxxxxxxxx"; // a renseigner par votre valeur
	private static final int socketNumber = 0000000000; // variable par partie le jour de la battle
	private static long gameId = 0000000; // variable par partie le jour de la battle

	public static void main(String[] zero) throws IOException, URISyntaxException, InterruptedException {
		new Client(ipServer, teamId, secret, socketNumber, gameId).run();
	}
}
