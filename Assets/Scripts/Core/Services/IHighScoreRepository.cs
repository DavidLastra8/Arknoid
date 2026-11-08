namespace Arknoid.Core.Services
{
    public interface IHighScoreRepository
    {
        int LoadHighScore();
        void SaveHighScore(int score);
    }
}
