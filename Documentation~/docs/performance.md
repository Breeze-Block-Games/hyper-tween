# Performance

Currently, HyperTween has only been benchmarked against [PrimeTween](https://github.com/KyryloKuzyk/PrimeTween)

At the time of writing, [MagicTween](https://github.com/AnnulusGames/MagicTween) is not compatible with the latest version of Unity, so I was unable to compare against it.

These benchmarks were performed on an ASUS TUF Gaming F15, 12th Gen Intel(R) Core(TM) i7-12700H (20 CPUs), ~2.3GHz, with the machine plugged into the power supply, using the IL2CPP scripting backend.

The broad trend is that HyperTween is faster than PrimeTween at creating tweens, and significantly faster at updating transforms. This is without even factoring in the fact that you can use HyperTween in a purely ECS context - in this case it's even faster.

![Managed Transform Create](../images/perf/svg_plots/managedtransform_create.svg)

![Managed Transform Update](../images/perf/svg_plots/managedtransform_update.svg)

In the graphs below Direct vs Indirect refers to tweens that output to a LocalTransform on the same entity vs tweens that output to a different entity. Surprisingly, Direct tweens do not update faster than Indirect tweens even though the memory access pattern is more linear. In future this type of tween may be deprecated as it requires an additional system and there doesn't seem to be much benefit.

![Indirect Unmanaged Transform Create](../images/perf/svg_plots/indirectunmanagedtransform_create.svg)

![Indirect Unmanaged Transform Update](../images/perf/svg_plots/indirectunmanagedtransform_update.svg)

![Direct Unmanaged Transform Create](../images/perf/svg_plots/directunmanagedtransform_create.svg)

![Direct Unmanaged Transform Update](../images/perf/svg_plots/directunmanagedtransform_update.svg)