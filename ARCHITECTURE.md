# Arquitectura de Arknoid (MVP + SOLID)

## Capas

```
Assets/Scripts/
├── Core/                      Arknoid.Core.asmdef  (noEngineReferences: C# puro, sin UnityEngine)
│   ├── Config/                GameRules, SceneNames, LevelSequence
│   ├── Models/                ScoreModel, LivesModel, BrickModel, LevelModel, PauseModel,
│   │                          RunData, LevelSnapshot, BrickState, WorldPoint
│   │   └── PowerUps/         PowerUpType, efectos (ExtraLife, SpawnBalls, Barrier, BonusScore),
│   │                          PowerUpEffects, PowerUpDropPolicy, RandomPowerUpSelector
│   ├── Persistence/           SaveData, SaveDataBinarySerializer (formato binario, sin E/S de disco)
│   ├── Views/                 Interfaces de vista: IBallView, IPaddleView, IBrickView, IPowerUpView,
│   │                          IScoreView, ILivesView, IPauseView, INavigationView, ITrackable
│   ├── Services/              Interfaces de servicios: IAudioService, IInputService, ISceneLoader,
│   │                          IRunRepository, IHighScoreRepository, IRandom, ITimeControl, ITickable…
│   └── Presenters/            BallPresenter, PaddlePresenter, BrickPresenter, PowerUpPresenter,
│                              ScorePresenter, LivesPresenter, PausePresenter, NavigationPresenter,
│                              GameFlowPresenter, RunAutoSaver, BallsPresenter, BarrierPresenter,
│                              PowerUpSpawner, PresenterGroup
├── Unity/                     Arknoid.Unity.asmdef (depende de Core)
│   ├── Views/                 MonoBehaviours pasivos: BallView, PaddleView, BrickView, PowerUpView,
│   │                          BarrierView, ScreenView
│   ├── Services/              AudioService, KeyboardInputService, UnitySceneLoader,
│   │                          BinaryFileSaveService, UnityRandom, UnityTimeControl,
│   │                          BallFactory, PowerUpFactory, PowerUpCatalog (ScriptableObject)
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

## Power-ups

Todos tienen la misma forma (el mismo prefab base) y se distinguen por el color:

| Color | Prefab | Tipo | Efecto | Valor editable |
|---|---|---|---|---|
| Blanco | `PowerUp` | `ExtraLife` | +N vidas | `Extra Lives` |
| Azul | `PowerUpBlue` | `MultiBallFromStart` | N bolas desde la posición inicial de salida | `Balls From Start` |
| Verde | `PowerUpGreen` | `MultiBallFromBall` | N bolas desde una bola en juego | `Balls From Ball` |
| Naranja | `PowerUpOrange` | `Barrier` | Barrera abajo durante N segundos | `Barrier Duration` |
| Rojo | `PowerUpRed` | `BonusScore` | +N puntos a la partida actual | `Bonus Score` |

**Dónde se editan:** en `Assets/Data/PowerUpCatalog`. Es un único asset para los dos niveles, y los efectos
leen los valores al aplicarse, así que se pueden ajustar también en Play. En el mismo asset están el color,
el grosor y la separación de la barrera.

**Cómo sale uno:** al romperse un ladrillo, `PowerUpDropPolicy` decide si sale power-up (probabilidad y máximo
por nivel, en `SceneInstaller → Rules`). Si sale, `RandomPowerUpSelector` elige **al azar y con la misma
probabilidad** entre todos los prefabs de la lista del catálogo, y `PowerUpFactory` lo instancia.

**Añadir uno nuevo:**
1. Un valor nuevo en `PowerUpType` y una clase `IPowerUpEffect`.
2. Registrarla en `SceneInstaller.ComposePowerUps`.
3. Duplicar un prefab, cambiarle el color y el `Type`, y añadirlo a `Prefabs` en el catálogo.

**Multibola:** `BallsPresenter` gestiona la bola de la escena y las extra, que son clones de ella y no chocan
entre sí. Las extra que caen se destruyen, y si cae la principal mientras quedan otras, se esconde hasta el
siguiente saque. **Solo se pierde una vida cuando cae la última bola en juego.** El modo automático de la pala
(tecla M) sigue a la bola más baja. Máximo de bolas a la vez: `Rules → Max Balls In Play`.

**Barrera:** `BarrierView` se crea por código justo encima del `DeathCollider`, con su mismo ancho. Es un
trigger: las bolas rebotan hacia arriba y los power-ups la atraviesan. Si coges otra mientras está activa,
se renueva la duración.

## Sistema de guardado (binario)

Se guarda en `Application.persistentDataPath/arknoid.sav`. En Windows es
`%USERPROFILE%\AppData\LocalLow\<Company>\<Product>\arknoid.sav`.

- **Qué se guarda:** el récord y la partida en curso (escena, puntos y vidas). Si el nivel está a medias,
  también los **ladrillos que quedan con su vida restante** y los power-ups ya soltados en ese nivel.
- **Formato:** `SaveDataBinarySerializer` (en Core, testeado) escribe con `BinaryWriter`. Empieza con un
  número mágico `ARKN` y una versión, para poder detectar archivos que no son de guardado y migrar formatos
  en el futuro. No se usa `BinaryFormatter` porque está obsoleto y es inseguro.
- **Disco:** `BinaryFileSaveService` escribe primero en un `.tmp` y después lo sustituye, así que si el juego se
  cierra a mitad de una escritura, el guardado anterior queda intacto. Si el archivo está corrupto, se aparta
  como `.corrupt` y se empieza de cero. La primera vez importa el récord antiguo de PlayerPrefs.
- **Cuándo se guarda el nivel a medias** (`RunAutoSaver`): al romperse un ladrillo, al cambiar las vidas,
  al pulsar *Exit* en la pausa y al cerrar el juego.
- **Cómo se identifica cada ladrillo:** por su ruta de índices en la jerarquía (p. ej. `6/1/14`), o por
  `BrickView → Persistent Id` si lo rellenas a mano. Con todos los IDs se calcula una **firma del nivel**.
  Si cambias los ladrillos en el editor, la firma ya no coincide y el nivel guardado se descarta, en lugar
  de restaurar ladrillos equivocados.
- **Al cargar:** `SceneInstaller` destruye los ladrillos que no aparecen en el guardado y da a los demás
  la vida que tenían.

Para borrar el guardado mientras desarrollas, elimina `arknoid.sav` de esa carpeta.

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

Abre `Window → General → Test Runner`. Hay tres grupos:

| Pestaña | Assembly | Qué prueba |
|---|---|---|
| **EditMode** | `Arknoid.Core.Tests` (48) | Modelos y presenters con fakes, sin escenas. `GameplayScenarioTests` conecta los presenters reales como en `SceneInstaller` y prueba combinaciones: 5 power-ups en el mismo frame, multibolas hasta el límite, bolas que caen a la vez, power-ups recogidos justo tras un game over o un nivel superado, valores negativos en el catálogo… |
| **EditMode** | `Arknoid.Unity.EditorTests` (6) | Assets: el catálogo tiene un prefab por tipo, todos con la misma forma pero distinto color, las bolas no chocan con los power-ups (matriz de capas) y Lvl1/Lvl2 tienen el catálogo asignado. |
| **PlayMode** | `Arknoid.PlayMode.Tests` (14) | **Física real** en un mini-nivel creado por código con los prefabs reales. Comprueba que las bolas no se quedan pegadas al `DeathCollider`, que las extra se destruyen al caer, que la barrera para la bola incluso al doble de la velocidad máxima, que los power-ups la atraviesan y no se quedan atrapados, y que se pueden recoger 5 a la vez. Además hay un *smoke test* que carga Lvl1 y Lvl2 (con un guardado temporal, para no pisar el tuyo). |

Errores que encontraron estos tests y ya están corregidos:
- Un multibola recogido en el mismo frame en que acaba la partida creaba bolas después del `Stop()`.
- Un +vida recogido justo después del game over volvía a guardar la partida, y *Load* la "resucitaba".
- Un power-up que no tocaba ni la pala ni el `DeathCollider` no se destruía nunca. Ahora tiene `Max Lifetime`
  (10 s por defecto).
- **La barrera naranja no aparecía en los niveles.** Se medía en el `Start()` de la escena, cuando el Canvas que
  contiene el `DeathCollider` aún no estaba escalado (en la escena tiene escala 0), así que salía con ancho 0.
  Ahora se mide al activarse y se reajusta mientras está activa. Lo cubren
  `Barrier_InsideParentScaledAfterCreation…` y `LevelN_OrangePowerUp_CreatesABarrierThatStopsTheBall`.
- **Lvl1 no tenía `AudioService`**: al abrirlo directamente el juego iba sin sonido. Ahora `SceneInstaller`
  instancia `Assets/Prefabs/AudioService.prefab` si la escena no tiene uno. También se ha añadido el sonido del
  power-up que le faltaba al `SoundManager` de Lvl2.

## Comandos de debug (borrables)

`Assets/Scripts/Unity/DebugTools/PowerUpDebugCommands.cs` se crea solo al darle a Play, sin tocar ninguna escena.
Solo existe en el editor y en las Development Builds.

| Tecla | Acción |
|---|---|
| `F1` | Mostrar u ocultar el panel (también tiene botones) |
| `1` … `5` | Power-up blanco, azul, verde, naranja o rojo justo encima de la pala |
| `Shift` + `1` … `5` | El mismo, pero cayendo desde arriba |
| `0` | Los 5 a la vez |
| `9` | 10 multibolas seguidos (para probar el límite de bolas) |

Van por el mismo camino que el juego (`PowerUpSpawner.Spawn` → presenter → efecto) y no gastan el cupo del nivel.
**Para eliminarlos**, borra la carpeta `Assets/Scripts/Unity/DebugTools`. No hay que tocar nada más.
