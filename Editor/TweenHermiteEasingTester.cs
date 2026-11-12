using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using UnityEngine;

namespace BreezeBlockGames.HyperTween.Editor
{
    [CreateAssetMenu(menuName = "HyperTween/Create TweenHermiteEasingTester", fileName = "TweenHermiteEasingTester", order = 0)]
    public class TweenHermiteEasingTester : ScriptableObject
    {
        public TweenHermiteEasingArgs[] Templates = null!;
        public AnimationCurve[] AnimationCurves = null!;
        
        [ContextMenu("GenerateTemplates")]
        void GenerateTemplates()
        {
            Templates = new TweenHermiteEasingArgs[4];
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Templates[i*2+j] = new TweenHermiteEasingArgs()
                    {
                        m0 = i,
                        m1 = j,
                    };
                }
            }
        }
        
        private void OnValidate()
        {
            if (AnimationCurves?.Length != Templates.Length)
            {
                AnimationCurves = new AnimationCurve[Templates.Length];
            }

            for (var index = 0; index < Templates.Length; index++)
            {
                var animationCurve = AnimationCurves[index];
                animationCurve.ClearKeys();
                
                var template = Templates[index];
                var interpolator = new TweenHermiteEasing(template.m0, template.m1);

                for (int i = 0; i <= 100; i++)
                {
                    var param = (float)i / 100;
                    animationCurve.AddKey(param, interpolator.Interpolate(param));
                }
            }
        }
    }
}