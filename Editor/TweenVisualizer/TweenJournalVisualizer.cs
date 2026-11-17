#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using BreezeBlockGames.HyperTween.Editor.VisualElements;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BreezeBlockGames.HyperTween.Editor.TweenVisualizer
{
    public abstract class TweenJournalVisualizer : VisualElement
    {
        private readonly Dictionary<EntityManager, TweenJournalAccess> _tweenJournalSingletonAccessLookup = new();

        private HelpBox _journalingDisabledError = null!;
        private WorldSelector _worldSelector = null!;
        private IsRecordingToggle _isRecordingToggle = null!;

        protected TweenJournalVisualizer()
        {
            RegisterCallbackOnce<AttachToPanelEvent>(_ =>
            {
                _worldSelector = this.Q<WorldSelector>();
                _isRecordingToggle = this.Q<IsRecordingToggle>();
                _journalingDisabledError = this.Q<HelpBox>();
                
                OnAttachToPanel();
                schedule.Execute(OnSchedule).Every(1000/60);
                OnSchedule();
                
                EditorApplication.playModeStateChanged += OnPlayModeStateChange;
            });
            
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                _journalingDisabledError.style.display = DisplayStyle.Flex;
                _journalingDisabledError.text = "Enter playmode to view Hyper Teen journal entries";
                
                EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
            });
            
            return;

            void OnPlayModeStateChange(PlayModeStateChange change)
            {
                if (change == PlayModeStateChange.EnteredPlayMode)
                {
                    ClearAllTweens();
                }
            }
        }

        private void OnSchedule()
        {
            if (!(_worldSelector.Selected?.IsCreated ?? false))
            {
                _journalingDisabledError.style.display = DisplayStyle.Flex;
                _journalingDisabledError.text = "Enter playmode to record Hyper Teen journal entries";
                return;
            }

            var entityManager = _worldSelector.Selected.EntityManager;
            
            if (!_tweenJournalSingletonAccessLookup.TryGetValue(entityManager, out var tweenJournalAccess))
            {
                TweenJournalAccess.TryCreate(entityManager, out tweenJournalAccess);
                _tweenJournalSingletonAccessLookup[entityManager] = tweenJournalAccess;
            }
            
            if (!tweenJournalAccess.TryGet(out var singleton))
            {
                _journalingDisabledError.style.display = DisplayStyle.Flex;
                _journalingDisabledError.text = "Ensure HYPER_TWEEN_ENABLE_JOURNAL is defined in order to record Hyper Teen journal entries";
                return;
            }

            _journalingDisabledError.style.display = DisplayStyle.None;

            try
            {
                if (singleton.ValueRW.Entries.Length == 0)
                {
                    return;
                }

                if (_isRecordingToggle.Enabled)
                {
                    OnReadFromJournal(singleton);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            singleton.ValueRW.Consumed.Value = true;
        }
        
        protected abstract void OnAttachToPanel();
        protected abstract void OnReadFromJournal(RefRW<TweenJournalSingleton> tweenJournalSingleton);
        public abstract void ClearAllTweens();
        public abstract void ClearDestroyedTweens();
    }
}
#endif