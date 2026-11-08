# Arquitectura de Arknoid (MVP + SOLID)

## Capas

```
Assets/Scripts/
├── Core/                      Arknoid.Core.asmdef  (noEngineReferences: C# puro, sin UnityEngine)
│   ├── Config/                GameRules, SceneNames, LevelSequence
│   ├── Models/                ScoreModel, LivesModel, BrickModel, LevelModel, PauseModel,
│   │                          RunData, PowerUpDropPolicy, ExtraLifeEffect
│   ├── Views/                 Interfaces de vista: IBallView, IPaddleView, IBrickView, IPowerUpView,
│   │                          IScoreView, ILivesView, IPauseView, INavigationView, ITrackable
│   ├── Services/              Interfaces de servicios: IAudioService, IInputService, ISceneLoader,
│   │                          IRunRepository, IHighScoreRepository, IRandom, ITimeControl, ITickable…
│   └── Presenters/            BallPresenter, PaddlePresenter, BrickPresenter, PowerUpPresenter,
│                              ScorePresenter, LivesPresenter, PausePresenter, NavigationPresenter,
│                              GameFlowPresenter, PresenterGroup
├── Unity/                     Arknoid.Unity.asmdef (depende de Core)
│   ├── Views/                 MonoBehaviours pasivos: BallView, PaddleView, BrickView, PowerUpView, ScreenView
│   ├── Services/              AudioService, KeyboardInputService, UnitySceneLoader,
│   │                          PlayerPrefsSaveService, UnityRandom, UnityTimeControl
│   └── Installers/            SceneInstaller (composition root de cada escena)
└── Tests/EditMode/            Arknoid.Core.Tests.asmdef: tests de modelos y presenters con fakes
```

Las dependencias van en una sola dirección: **Unity → Core**. Como `Arknoid.Core` tiene
`noEngineReferences`, si alguien intenta usar `UnityEngine` en un modelo o presenter, no compila.

## Cómo encaja el MVP

| Pieza | Responsabilidad | Ejemplo |
|---|---|---|
| **Model** | Estado y reglas, sin UI ni Unity | `BrickModel.TakeHit()`, `LevelModel.Cleared` |
| **View** | Mostrar cosas y avisar de lo que pasa (eventos). No toma decisiones | `BrickView.HitByBall`, `ScreenView.ShowScore()` |
| **Presenter** | Escucha a la vista y al modelo, aplica la lógica y actualiza ambos | `BrickPresenter`: golpe → daño → puntos → power-up → romper |
| **Installer** | Crea todo y lo conecta (inyección por constructor) | `SceneInstaller.Start()` |

Ejemplo del flujo cuando la pelota golpea un ladrillo:

```
BrickView.OnCollisionEnter2D ──HitByBall──▶ BrickPresenter
                                             ├─ audio.Play(Hit)
                                             ├─ ScoreModel.Add() ──CurrentChanged──▶ ScorePresenter ─▶ ScreenView.ShowScore
                                             ├─ BrickModel.TakeHit()
                                             ├─ PowerUpDropPolicy.TryDrop() ─▶ BrickView.SpawnPowerUp() ─▶ PowerUpPresenter
                                             ├─ BrickView.Break()
                                             └─ LevelModel.NotifyBrickDestroyed() ──Cleared──▶ GameFlowPresenter ─▶ guardar + cargar transición
```

## SOLID en el proyecto

- **S – Responsabilidad única.** El antiguo `BrickManager` hacía colisiones, daño, puntuación, power-ups,
  sonido y cambio de nivel. Ahora cada cosa tiene su clase: vista (colisión), modelo (vida),
  presenter (orquestación), `LevelModel` (ladrillos restantes) y `GameFlowPresenter` (cambio de escena).
- **O – Abierto/cerrado.** Para un nivel nuevo, se añade un paso a `LevelSequence`. Para un power-up nuevo,
  se implementa `IPowerUpEffect`. Para otro sistema de guardado, otra clase con `IRunRepository`.
  En ningún caso hay que tocar los presenters.
- **L – Sustitución de Liskov.** Cualquier implementación de una interfaz se puede usar en lugar de otra:
  `NullAudioService` en vez de `AudioService`, o los fakes de los tests en vez de las vistas reales.
- **I – Segregación de interfaces.** `ScreenView` implementa cuatro interfaces pequeñas (`IScoreView`,
  `ILivesView`, `IPauseView`, `INavigationView`), y cada presenter solo ve la que necesita.
  `PaddlePresenter` solo conoce `ITrackable` de la pelota, no `IBallView` completo.
- **D – Inversión de dependencias.** Los presenters dependen de interfaces (`IAudioService`, `ISceneLoader`, …),
  nunca de `SoundManager.instance`, `SceneManager` o `PlayerPrefs`. Por eso se pueden testear sin Unity.

## Montaje en las escenas

No hace falta añadir componentes nuevos, porque los scripts se han renombrado **conservando su GUID** (`.meta`):

| Antes | Ahora | GameObject |
|---|---|---|
| `GameManager` | `SceneInstaller` | GameManager |
| `UIManager` | `ScreenView` | UiManager / UI controller |
| `BallManager` | `BallView` | Circle (prefab) / Ball |
| `PlayerMovement` | `PaddleView` | Square (prefab) / Player |
| `BrickManager` | `BrickView` | Brick1HP…Brick5HP (prefabs) |
| `PowerUpManager` | `PowerUpView` | PowerUp (prefab) |
| `SoundManager` | `AudioService` | SoundManager |

Los valores del Inspector se conservan gracias a `[FormerlySerializedAs]` (por ejemplo `brickHP` → `hitPoints`).
Los `OnClick()` de los botones se han reapuntado a `ScreenView`:

| Escena | Botón | Método |
|---|---|---|
| InitialScreen | Start | `OnNewGameClicked` |
| InitialScreen | Load | `OnContinueClicked` |
| TransitionLvl1 / TransitionLvl2 | Go to Level N | `OnNextLevelClicked` |
| GameOverScene | Restart | `OnMainMenuClicked` |
| Lvl1 / Lvl2 | Continue (pausa) | `OnResumeClicked` |
| Lvl1 / Lvl2 | Exit (pausa) | `OnMainMenuClicked` |

`SceneInstaller` detecta solo el tipo de escena: si hay ladrillos, monta la partida; si no, solo la UI y la
navegación. Los valores de juego (vidas, fuerza de saque, aceleración, probabilidad de power-up…) están en
`SceneInstaller → Rules`, en el Inspector.

## Bugs corregidos

1. **El nivel 2 no se podía jugar.** `Lvl1Brick`/`Lvl2Brick` eran `static` y no se reiniciaban. Al llegar a Lvl2,
   `Lvl1Brick` seguía a 0 y se volvía a cargar la transición en bucle. Ahora cada nivel tiene su `LevelModel`.
2. **La puntuación se sumaba mal.** Cada ladrillo acumulaba su propio `GameScore` y enviaba el total
   (10, luego 20 → +30). Ahora cada golpe suma los puntos del ladrillo una sola vez.
3. **Límite de power-ups global.** `powerUpCount` era `static`: tras 2 power-ups en toda la partida no salían más.
   Ahora el máximo es por nivel.
4. **`using UnityEditorInternal;`** en `UIManager` impedía compilar la build. Eliminado
   (y también `Unity.VisualScripting`, que no se usaba).
5. **La build arrancaba en Lvl1.** `InitialScreen` es ahora la primera escena de Build Settings.
6. **Continue / Load no cargaba nada.** El botón llamaba a `UIManager.LoadData()`, que solo leía textos.
   Ahora carga la partida guardada (nivel, puntos y vidas).
7. **Al pasar al siguiente nivel se guardaba el nivel recién superado** (y en cada frame, desde el `Update` de
   todos los ladrillos). Ahora se guarda una vez y apunta al nivel siguiente.
8. **Al superar Lvl2 se saltaba la pantalla `TransitionLvl1`.** Ahora el flujo es
   Lvl1 → TransitionLvl2 → Lvl2 → TransitionLvl1 → Lvl1.
9. **NullReference** al jugar Lvl1 directamente (no tenía `SoundManager`) y en escenas sin `Slider`.
   Ahora se usa `NullAudioService` y las vistas comprueban sus referencias.
10. **La pausa llamaba a `SetActive` y a `Time.timeScale` en cada frame.** Ahora solo se hace cuando cambia el
    estado, y `timeScale` vuelve a 1 al salir de la escena.
11. **El power-up caía según los FPS** (0.1 por frame). Ahora usa `Time.deltaTime` (6 u/s ≈ igual que a 60 FPS).
12. **El sonido se cortaba** al reproducir otro, porque se reutilizaba `clip` + `Play()`. Ahora usa `PlayOneShot`.

## Tests

`Window → General → Test Runner → EditMode → Run All`. Hay 16 tests de modelos y presenters, y se ejecutan
sin escenas porque usan los fakes de `Tests/EditMode/Fakes.cs`.
