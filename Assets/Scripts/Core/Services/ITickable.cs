namespace Arknoid.Core.Services
{
    /// <summary>Lo implementan los presenters que necesitan actualizarse cada frame. El SceneInstaller los llama desde Update.</summary>
    public interface ITickable
    {
        void Tick(float deltaTime);
    }
}
