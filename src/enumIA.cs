namespace SopraSteria_CodingGame.IATypes
{
    public enum EnumIA
    {
        /*
            This is the most basic IA we developped
            It will choose a random direction between north, east, south and west
            It doesn't check any collisions and doesn't jump
        */
        RANDOM,

        /*
            Greedy is an intuitive IA rushing the objectives
            It uses the shortest path to pick up the nearest logo
            If no logo is available, it will try to hit an other rabbit carrying a logo
            It checks collisions and doesn't go outside the map
            It doesn't jump (yet !)
        */
        GREEDY
    }
}