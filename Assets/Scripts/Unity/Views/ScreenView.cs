using System;
using Arknoid.Core.Views;
using TMPro;
using UnityEngine;

namespace Arknoid.Unity.Views
{
    /// <summary>
    /// Vista de la interfaz de cada pantalla (HUD, menú de pausa y botones).
    /// Implementa varias interfaces pequeñas para que cada presenter dependa solo de lo que usa (ISP).
    /// Los botones de la escena llaman a los métodos On...Clicked desde su OnClick().
    /// </summary>
    public class ScreenView : MonoBehaviour, IScoreView, ILivesView, IPauseView, INavigationView
    {
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private TMP_Text highScoreText;

        public event Action ResumeRequested;
        public event Action NewGameRequested;
        public event Action ContinueRequested;
        public event Action NextLevelRequested;
        public event Action MainMenuRequested;

        public void ShowScore(int score) => SetText(scoreText, "Score: " + score);
        public void ShowBestScore(int best) => SetText(highScoreText, "Best: " + best);
        public void ShowLives(int lives) => SetText(livesText, "Lives: " + lives);

        public void SetPauseMenuVisible(bool visible)
        {
            if (pauseMenu != null) pauseMenu.SetActive(visible);
        }

        // --- Callbacks de botones (asignados en el OnClick() de cada Button) ---
        public void OnNewGameClicked() => NewGameRequested?.Invoke();
        public void OnContinueClicked() => ContinueRequested?.Invoke();
        public void OnNextLevelClicked() => NextLevelRequested?.Invoke();
        public void OnMainMenuClicked() => MainMenuRequested?.Invoke();
        public void OnResumeClicked() => ResumeRequested?.Invoke();

        private static void SetText(TMP_Text label, string value)
        {
            if (label != null) label.text = value;
        }
    }
}
