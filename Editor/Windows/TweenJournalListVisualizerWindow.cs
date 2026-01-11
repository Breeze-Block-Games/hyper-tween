using BreezeBlockGames.HyperTween.Editor.TweenVisualizer;
using UnityEditor;
using UnityEngine;

namespace BreezeBlockGames.HyperTween.Editor.Windows
{
    public class TweenJournalListVisualizerWindow : TweenVisualizerWindow<TweenJournalListVisualizer>
    {
        [MenuItem("Window/HyperTween/Tween Journal List Visualizer")]
        public static void CreateListVisualizer()
        {
            var window = GetWindow<TweenJournalListVisualizerWindow>();
            window.titleContent = new GUIContent("Tween Journal List Visualizer");
        }
    }
}