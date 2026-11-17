using System;
using Unity.Entities;
using Unity.Properties;

namespace BreezeBlockGames.HyperTween.UnityShared.Journal.Components
{
    public struct TweenJournal : IComponentData
    {
        [Flags]
        public enum Event : int
        {
            None = 0,
            Create = 1 << 0,
            OnPlay = 1 << 1,
            OnStop = 1 << 2,
            Conflict = 1 << 3,
            UpdatedTimer = 1 << 4,
            UpdatedParameter = 1 << 5,
            Output = 1 << 6,
            CreateDuration = 1 << 7,
            CompleteDuration = 1 << 8,
            CreateForkOnPlay = 1 << 9,
            CompleteForkOnPlay = 1 << 10,
            CreateSignalJoinOnStop = 1 << 11,
            CompleteSignalJoinOnStop = 1 << 12,
            CreatePlayOnPlay = 1 << 13,
            CompletePlayOnPlay = 1 << 14,
            CreatePlayOnStop = 1 << 15,
            CompletePlayOnStop = 1 << 16,
            Destroy = 1 << 17
        }
        
        public struct LiteEntry
        {
            [CreateProperty]
            public Event Event;
            [CreateProperty]
            public Entity Entity;
            [CreateProperty]
            public Entity TargetEntity;
            [CreateProperty]
            public float Value;
        }
        
        public struct Entry
        {
            [CreateProperty]
            public LiteEntry LiteEntry;
            [CreateProperty]
            public int Index;
            [CreateProperty]
            public int Frame;
            [CreateProperty]
            public double Time;
            [CreateProperty]
            public int Iteration;

            public Entry(LiteEntry liteEntry, int frame, double time, int index, int iteration)
            {
                LiteEntry = liteEntry;
                Frame = frame;
                Time = time;
                Index = index;
                Iteration = iteration;
            }
        }

    }
}
