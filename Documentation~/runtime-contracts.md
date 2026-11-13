# Runtime contracts

## Time and target lifetime

Tween time advances with the world delta. Serial sequences forward all unused time across short children; parallel joins forward the remainder from the last child to finish. Destroying an output target suppresses writes while the timer, stop signal, callbacks and sequence continue. A target must exist when attaching its Unity object through the builder. Destroying it after setup, including before deferred playback, is supported. A stopped transform tween releases its transform array and lookup entries. The lifetime guarantee applies to external output targets; explicitly destroying the tween entity also removes its timer.

Managed output components attach directly to the tween entity. `WithTarget` forwards unmanaged ECS outputs; indirect managed output forwarding is unsupported. Use a separate tween with the managed target attached directly instead.

Direct batch local-transform outputs initialize a missing LocalTransform to identity on play. Supplied position, rotation and scale remain the starting values. Zero-duration outputs write the endpoint.

## Collection rollback

Pair `Push()` with `Pop(name).Play()` on success or `Discard()` on failure. Discard removes the current group without playing it, destroys tweens and clones created through its collection factory (including partial or compound construction), and preserves its enclosing group. It throws `InvalidOperationException` when no group is open. Dispose the collection to release its own native storage.

Complete a collection group before its command buffer plays back. A command-buffer factory belongs to its current recording buffer; obtain a fresh factory after an await that may cross playback. These rules also apply to nested groups. Externally supplied tween handles retain their own descendant ownership; create compound graphs through the collection factory for full rollback tracking.

`ToTransformInstanceIds` returns owned native storage. `CollectionHelper.Dispose(array)` handles both built-in and custom allocator handles.

## Batch ranges

Batch range APIs use separate `sourceIndex`, `destinationIndex` and `count` values. Source and destination indices are zero based. A count of zero writes nothing and permits an index equal to the corresponding array length. A count of -1 consumes the remaining source range. Other negative values, or ranges extending beyond either array, fail before mutation.

For example, durations `[1, 2, 3, 4]` with `sourceIndex: 2, destinationIndex: 1, count: 2` configure destination tweens 1 and 2 with durations 3 and 4. Destination tween 0 is untouched.

The original `WithDurations(durations, startIndex)` overload remains available. `startIndex` means the source offset, with destination offset zero; prefer the independent-offset overload for new code. Selected durations add the timer and overflow components as well as the duration. Unselected tweens keep their existing configuration.

## Builder capabilities

| Builder | Unmanaged components, buffers and sequences | Entity names | Managed components / Unity objects |
| --- | --- | --- | --- |
| EntityManager | Supported on the Unity main thread | Supported | Supported on the Unity main thread |
| EntityCommandBuffer | Supported at playback | Supported | Record and play back on the Unity main thread |
| EntityCommandBuffer parallel writer | Supported from jobs | Supported | Unsupported |
| ExclusiveEntityTransaction | Supported in its exclusive transaction | Unsupported | Unsupported |

Unsupported operations throw `NotSupportedException` describing the backend restriction. A job parallel writer cannot record Unity managed components: UI, Timeline, R3 targets, Action callbacks and UniTask completion hooks need a main-thread builder. Transform instance IDs and unmanaged ECS outputs remain usable from supported job builders. Unity object API access itself must stay on the main thread.

## Optional modules

Transforms bridges unmanaged local-transform output to Unity transforms. UI writes Image and CanvasGroup outputs. InvokeAction runs play/stop callbacks. UniTask completes the returned task when the tween stops, including a conflict stop; it does not distinguish successful completion from cancellation. A null handle returns an already completed task. R3 animates an integer reactive property.

Timeline uses InvokeAction ordering and depends on the InvokeAction module. Disabling InvokeAction also requires disabling Timeline; the other module disable symbols apply to their own modules. R3 and UniTask additionally require their corresponding installed packages. Managed module targets use the builder restrictions above.

HyperTween does not promise arbitrary cross-thread access to Unity objects or managed component construction from jobs.
