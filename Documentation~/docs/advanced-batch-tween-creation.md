# Batch Tween Creation

HyperTween also has APIs for creating large numbers of tweens at once by creating a single tween as a template, then using `CreateBatch()` to clone the tween. This creates a `BatchTweenHandle` which can be used to efficiently perform operations on all the tweens in the batch.

The created template tween will be destroyed automatically when the BatchTweenHandle is disposed.

This API can be used in combination with `SystemAPI` (similar to [JobifiedTweenCreation](advanced-jobified-tween-creation.md)), or any of the different manual job types, but the example below shows the most performant (and most complex) approach.

[!code-csharp[CS](../../Samples~/Examples/Systems/BatchTweenExampleSystem.cs)]

