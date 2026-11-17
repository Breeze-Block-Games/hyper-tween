# Modules

Currently, HyperTween has very few tween output components as the framework is still experimental and creating your own is designed to be as simple as possible. Additionally, adding more tween outputs does potentially add overhead as it increases the number of systems present.

For this reason, it is possible to disable modules that you don't need, removing those components and systems and any associated overhead.

The following modules currently exist, mainly due to them being use cases that I require for other projects:

## InvokeAction

`BreezeBlockGames.HyperTween.UnityShared.Modules.InvokeAction`

Allows invoking actions when tweens play or stop, as described in [Basic Tween Invocation](basic-tween-invocation.md).

Add `HYPER_TWEEN_DISABLE_MODULE_INVOKE_ACTION` to your project settings to disable this module.

## R3

`BreezeBlockGames.HyperTween.UnityShared.Modules.R3`

Adds very basic and preliminary compatibility with [R3](https://github.com/Cysharp/R3)

Allows updating `ReactiveProperty<int>` with tweens.

Add `HYPER_TWEEN_DISABLE_MODULE_R3` to your project settings to disable this module.

## Timeline

`BreezeBlockGames.HyperTween.UnityShared.Modules.Timeline`

Allows controlling playback of `PlayableDirector` with tweens.

Add `HYPER_TWEEN_DISABLE_MODULE_TIMELINE` to your project settings to disable this module.

## Transforms

`BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms`

Allows tweening managed `Transform` components on regular GameObjects.

Add `HYPER_TWEEN_DISABLE_MODULE_TRANSFORMS` to your project settings to disable this module.

## UI

`BreezeBlockGames.HyperTween.UnityShared.Modules.UI`

Allows tweening various properties of UGUI components.

Add `HYPER_TWEEN_DISABLE_MODULE_UI` to your project settings to disable this module. 

## UniTask

`BreezeBlockGames.HyperTween.UnityShared.Modules.UniTask`

Adds very basic and preliminary compatibility with [UniTask](https://github.com/Cysharp/UniTask)

Allows converting of tweens to `UniTask` so they can be awaited.

Add `HYPER_TWEEN_DISABLE_MODULE_UNITASK` to your project settings to disable this module.