public class Move
{
    public int startIndex;
    public int endIndex;
    public int removeIndex;
    public GameManager.EGameState gameState;
    public int score;

    public Move(int startIndex, int endIndex, int removeIndex, GameManager.EGameState gameState)
    {
        this.startIndex = startIndex;
        this.endIndex = endIndex;
        this.removeIndex = removeIndex;
        this.gameState = gameState;
    }
}
