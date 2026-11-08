using System.Collections.Generic;

namespace Arknoid.Core.Config
{
    /// <summary>
    /// Orden de los niveles. Para añadir un nivel nuevo basta con añadir un paso
    /// (abierto a extensión, cerrado a modificación): ningún presenter cambia.
    /// </summary>
    public class LevelSequence
    {
        public readonly struct Step
        {
            public Step(string level, string transitionScene, string nextLevel)
            {
                Level = level;
                TransitionScene = transitionScene;
                NextLevel = nextLevel;
            }

            public string Level { get; }
            public string TransitionScene { get; }
            public string NextLevel { get; }
        }

        private readonly List<Step> steps;

        public LevelSequence(IEnumerable<Step> steps)
        {
            this.steps = new List<Step>(steps);
        }

        public string FirstLevel => steps.Count > 0 ? steps[0].Level : SceneNames.Level1;

        public bool TryGetStep(string level, out Step step)
        {
            foreach (var s in steps)
            {
                if (s.Level == level)
                {
                    step = s;
                    return true;
                }
            }

            step = default;
            return false;
        }

        /// <summary>Lvl1 → TransitionLvl2 → Lvl2 → TransitionLvl1 → Lvl1 …</summary>
        public static LevelSequence Default() => new LevelSequence(new[]
        {
            new Step(SceneNames.Level1, SceneNames.TransitionToLevel2, SceneNames.Level2),
            new Step(SceneNames.Level2, SceneNames.TransitionToLevel1, SceneNames.Level1),
        });
    }
}
