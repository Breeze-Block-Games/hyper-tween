using BreezeBlockGames.HyperTween.Editor.TweenVisualizer;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BreezeBlockGames.HyperTween.Editor.Windows
{
    public abstract class TweenVisualizerWindow<TVisualizer> : EditorWindow
        where TVisualizer : TweenJournalVisualizer, new()
    {
        [SerializeField]
        private VisualTreeAsset _windowVisualTreeAsset = null!;
    
        [SerializeField]
        private VisualTreeAsset _additionalToolbarAsset = null!;
        
        [SerializeField]
        private VisualTreeAsset _contentVisualTreeAsset = null!;
        
        [SerializeField]
        private StyleSheet _styleSheetAsset = null!;

        public void CreateGUI()
        {
            rootVisualElement.styleSheets.Add(_styleSheetAsset);

            var tweenJournalVisualizer = new TVisualizer();
            
            var window = _windowVisualTreeAsset.Instantiate();
            tweenJournalVisualizer.Add(window);
            window.StretchToParentSize();

            if (_additionalToolbarAsset != null)
            {
                var toolbar = window.Q<Toolbar>();
                var additionalToolbar = _additionalToolbarAsset.Instantiate();
                toolbar.Add(additionalToolbar);
            }
            
            var clearAllButton = window.Q<Button>("ClearAllButton");
            clearAllButton.clickable.clicked += () => tweenJournalVisualizer.ClearAllTweens();
            
            var clearDestroyedButton = window.Q<Button>("ClearDestroyedButton");
            clearDestroyedButton.clickable.clicked += () => tweenJournalVisualizer.ClearDestroyedTweens();
            
            var contentContainer = window.Q<VisualElement>("Content");
            
            var content = _contentVisualTreeAsset.Instantiate();
            contentContainer.Add(content);
            content.StretchToParentSize();

            // Add last so everything is ready by the time this attaches
            rootVisualElement.Add(tweenJournalVisualizer);
            tweenJournalVisualizer.StretchToParentSize();
        }
    }
}