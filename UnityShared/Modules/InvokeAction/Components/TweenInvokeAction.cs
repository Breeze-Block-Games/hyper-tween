using System;
using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Invoke.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.InvokeAction.Components
{
    public class TweenInvokeAction : ITweenBufferedInvokeOnPlay, ITweenBufferedInvokeOnStop
    {
        public readonly struct Context
        {
            public readonly Entity Entity;
            public readonly Entity TargetEntity;
            public readonly EntityCommandBuffer EntityCommandBuffer;
            public readonly TweenDurationOverflow TweenDurationOverflow;
            public readonly TweenFactory<EntityCommandBufferTweenBuilder> TweenFactory;
            
            public Context(Entity entity, Entity targetEntity, EntityCommandBuffer entityCommandBuffer, TweenDurationOverflow tweenDurationOverflow, TweenFactory<EntityCommandBufferTweenBuilder> tweenFactory)
            {
                Entity = entity;
                EntityCommandBuffer = entityCommandBuffer;
                TweenDurationOverflow = tweenDurationOverflow;
                TweenFactory = tweenFactory;
                TargetEntity = targetEntity;
            }
        }
        
        public Action<Context>? Action;

        public void Invoke(Entity tweenEntity, Entity targetEntity, EntityCommandBuffer entityCommandBuffer, TweenFactory<EntityCommandBufferTweenBuilder> tweenFactory, in TweenDurationOverflow tweenDurationOverflow)
        {
            Action?.Invoke(new Context(tweenEntity, targetEntity, entityCommandBuffer, tweenDurationOverflow, tweenFactory));
        }
        
    }
}