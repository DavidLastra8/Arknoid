namespace Arknoid.Core.Models.PowerUps
{
    /// <summary>
    /// Tipos de power-up. Cada prefab indica el suyo en su PowerUpView.
    /// No cambies el número de los existentes: está guardado en los prefabs.
    /// </summary>
    public enum PowerUpType
    {
        ExtraLife = 0,            // blanco
        MultiBallFromStart = 1,   // azul: bolas desde la posición de salida
        MultiBallFromBall = 2,    // verde: bolas desde una bola que ya está en juego
        Barrier = 3,              // naranja: barrera temporal abajo
        BonusScore = 4,           // rojo: puntos extra
    }
}
