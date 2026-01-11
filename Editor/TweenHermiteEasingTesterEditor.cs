using UnityEditor;
using UnityEngine;

namespace BreezeBlockGames.HyperTween.Editor
{
    [CustomEditor(typeof(TweenHermiteEasingTester))]
    public class TweenHermiteEasingTesterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            TweenHermiteEasingTester tweenHermiteEasingTester = (TweenHermiteEasingTester)target;

            for (var i = 0; i < tweenHermiteEasingTester.Templates.Length; i++)
            {
                var template = tweenHermiteEasingTester.Templates[i];
                
                EditorGUILayout.LabelField($"m0: {template.m0}, m1: {template.m1}");
                Rect curveRect = EditorGUILayout.GetControlRect(GUILayout.Height(300));
                EditorGUI.CurveField(curveRect, "Animation Curve", tweenHermiteEasingTester.AnimationCurves[i]);
            }
        }
    }
}