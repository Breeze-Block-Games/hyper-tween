# Extending Outputs

In order to extend HyperTween to add new output types, you just need to add a component that extends `ITweenTo<TComponent, TValue>`. HyperTween will automatically generate a `From` version of the component and the systems required.

[!code-csharp[CS](../../UnityShared/Modules/LocalTransforms/Components/TweenLocalPosition.cs)]

## API Extensions Methods

You could add this component to tweens using the regular ECS methods, but it is recommended to create extension methods for `TweenHandle` with overloads with parameters with/without a from component.

[!code-csharp[CS](../../UnityShared/Modules/Transforms/API/TweenTransformExtensions.cs)]

Optionally, you can also add extension methods for `BatchTweenHandle`.

[!code-csharp[CS](../../UnityShared/Modules/Transforms/API/TweenTransformBatchExtensions.cs)]

## Conflict Detection

Adding the `[DetectConflicts]` attribute to the output component will also generate the system required to detect conflicts. See [Miscellaneous Topics](advanced-misc.md) for more information regarding conflicts. By default, this system will detect conflicts by checking if there is an existing tween playing with the same target entity and output component. Alternatively, you can use `[DetectConflicts(typeof(UnityObjectInstanceId))]` for detecting conflicts based on Unity Object instance ids, `[DetectConflicts(typeof(ObjectHashCode))]` to use `System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj)`. Note that you must also add these components to the tween entity using `UnityObjectInstanceId.Create()` and `ObjectHashCode.Create()` respectively.
