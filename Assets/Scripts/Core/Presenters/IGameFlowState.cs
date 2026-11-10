namespace Arknoid.Core.Presenters
{
    /// <summary>Si la partida en este nivel ya ha terminado (game over o nivel superado).</summary>
    public interface IGameFlowState
    {
        bool IsFinished { get; }
    }
}
