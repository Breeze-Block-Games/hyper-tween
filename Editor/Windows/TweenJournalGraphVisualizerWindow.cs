using BreezeBlockGames.HyperTween.Editor.TweenVisualizer;
using UnityEditor;
using UnityEngine;

namespace BreezeBlockGames.HyperTween.Editor.Windows
{
    public class TweenJournalGraphVisualizerWindow : TweenVisualizerWindow<TweenJournalGraphVisualizer>
    {
        [MenuItem("Window/HyperTween/Tween Journal Graph Visualizer")]
        public static void CreateGraphVisualizer()
        {
            var window = GetWindow<TweenJournalGraphVisualizerWindow>();
            window.titleContent = new GUIContent("Tween Journal Graph Visualizer");
        }
    }
}