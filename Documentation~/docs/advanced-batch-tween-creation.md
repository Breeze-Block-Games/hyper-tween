# Batch Tween Creation

HyperTween also has APIs for creating large numbers of tweens at once by creating a single tween as a template, then using `CreateBatch()` to clone the tween. This creates a `BatchTweenHandle` which can be used to efficiently perform operations on all the tweens in the batch.

Note that the created template tween will be destroyed automatically when the BatchTweenHandle is disposed.

[!code-csharp[CS](../../Samples~/Examples/Systems/BatchTweenExampleSystem.cs)]

