using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
using Unity.Entities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BreezeBlockGames.HyperTween.Editor.VisualElements
{
    [UxmlElement]    
    public partial class WorldSelector : VisualElement
    {
        private WorldListChangeTracker _worldListChangeTracker;
        private World? _selected;
        
        private TextElement _worldNameTextElement = null!;
        private ToolbarMenu _toolbarMenu = null!;

        public World? Selected
        {
            get => _selected;
            private set
            {
                _selected = value;
                SelectedWorldName = _selected?.Name ?? "None";
            }
        }

        [UxmlAttribute] private string PrefKey { get; set; } = null!;

        private string SelectedWorldName
        {
            get => EditorPrefs.GetString(PrefKey, string.Empty);
            set
            {
                EditorPrefs.SetString(PrefKey, value);
                _worldNameTextElement.text = value;
            }
        }

        public WorldSelector()
        {
            RegisterCallbackOnce<AttachToPanelEvent>(_ =>
            {
                _toolbarMenu = this.Q<ToolbarMenu>();
                _worldNameTextElement = _toolbarMenu.Q<TextElement>();
                
                schedule.Execute(UpdateWorlds).Every(1000/100);
            });
        }

        private void UpdateWorlds(TimerState obj)
        {
            if (!_worldListChangeTracker.HasChanged())
            {
                return;
            }
            
            _toolbarMenu.menu.ClearItems();
            foreach (var world in World.All)
            {
                _toolbarMenu.menu.AppendAction(world.Name, _ =>
                {
                    Selected = world;
                    _toolbarMenu.text = world.Name;
                });

                if (world.Name == SelectedWorldName)
                {
                    Selected = world;
                }
            }
        }
    }
}