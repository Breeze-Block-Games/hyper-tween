# Getting Started

## Installation

HyperTween can be easily installed using the [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html) using the following git URL:

`https://github.com/Breeze-Block-Games/hyper-tween.git`


## Setup

If you are using [Assembly Definition](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html) files in your project, you must add a reference to the `BreezeBlockGames.HyperTween.UnityShared` assembly in your script's assembly definition. Additionally, you must reference the [Module](modules.md) assembly definitions you wish to use.

See [Basic Tween Creation](basic-tween-creation.md) for how to create your first tween.

## Next Steps

If you are only planning to use HyperTween to tween basic Unity Transforms, and performance is not a concern, then the [Basic Usage](basic-tween-creation.md) section will have all the information that you need.

However, if you plan to use HyperTween within an ECS based project, or you want to cheaply create and run thousands of tweens, then also check out the [Advanced Usage](advanced-jobified-tween-creation.md) section.