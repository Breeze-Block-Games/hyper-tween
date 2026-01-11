#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;

namespace BreezeBlockGames.HyperTween.Editor.VisualElements
{
    [UxmlElement]
    public partial class IsRecordingToggle : VisualElement
    {
        [UxmlAttribute] private string PrefsKey { get; set; } = null!;
        
        public bool Enabled
        {
            get => EditorPrefs.GetBool(PrefsKey, false);
            private set => EditorPrefs.SetBool(PrefsKey, value);
        }
        
        public IsRecordingToggle()
        {
            RegisterCallbackOnce<AttachToPanelEvent>(_ =>
            {
                var toggle = this.Q<Toggle>();
                toggle.value = Enabled;

                toggle.RegisterValueChangedCallback(e =>
                {
                    Enabled = e.newValue;
                    if (e.newValue)
                    {
                        AddToClassList("checked");
                    }
                    else
                    {
                        RemoveFromClassList("checked");
                    }
                });
            });
        }
    }
}
#endif