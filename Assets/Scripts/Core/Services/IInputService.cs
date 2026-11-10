namespace Arknoid.Core.Services
{
    public interface IInputService
    {
        /// <summary>True durante el frame en que se pulsa la tecla de pausa.</summary>
        bool PauseTogglePressed { get; }

        /// <summary>True durante el frame en que se pulsa la tecla del modo automático.</summary>
        bool AutoplayTogglePressed { get; }
    }
}
