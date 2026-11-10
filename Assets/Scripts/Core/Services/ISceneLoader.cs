namespace Arknoid.Core.Services
{
    public interface ISceneLoader
    {
        string ActiveSceneName { get; }
        void Load(string sceneName);
    }
}
