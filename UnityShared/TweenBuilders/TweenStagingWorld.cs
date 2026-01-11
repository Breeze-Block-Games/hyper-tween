using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace BreezeBlockGames.HyperTween.UnityShared.TweenBuilders
{
    public static class TweenStagingWorld
    {
        private static World? _instance;

        public static World Instance
        {
            get
            {
                if (_instance?.IsCreated ?? false)
                {
                    return _instance;
                }
                
                _instance = new World("TweenStagingWorld", WorldFlags.Staging, Allocator.Persistent);
                Application.quitting += () => _instance.Dispose();

                return _instance;
            }
        }
    }
}