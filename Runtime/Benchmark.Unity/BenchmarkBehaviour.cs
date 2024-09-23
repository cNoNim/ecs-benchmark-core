using System;
using System.Collections;
using System.Diagnostics;
using Benchmark.Core;
using Benchmark.Core.Hash;
using Unity.Profiling;
using UnityEngine;
using static Benchmark.Core.Common;

namespace Benchmark.Unity
{

public class BenchmarkBehaviour : MonoBehaviour
{
	[Min(0)]
	public int EntityCount = 2048;

	[Range(0, 8)]
	public int WarmupCount;

	[Range(0, 16384)]
	public int MeasurementCount = 1024;

	[Min(1)]
	public int IterationCount = 1;

	public string         Name          { get; private set; } = string.Empty;
	public BenchmarkState State         { get; private set; }
	public double         StateProgress { get; private set; }

	public void Run(IContext context, Action<int, Sample[]> callback) =>
		StartCoroutine(
			RunRoutine(
				context,
				Math.Max(0, EntityCount),
				MeasurementCount,
				Math.Max(1, IterationCount),
				WarmupCount,
				callback));

	private IEnumerator RunRoutine(
		IContext context,
		int entityCount,
		int measurementCount,
		int iterationCount,
		int warmupCount,
		Action<int, Sample[]> callback)
	{
		Name = context.ToString();
		context.Setup(entityCount, new Framebuffer(FrameBufferWidth, FrameBufferHeight));
		var samples  = new Sample[measurementCount];
		var hashCode = new StableHashCode();
		try
		{
			for (var w = 0; w < warmupCount; w++)
			{
				State         = BenchmarkState.WarmUp;
				StateProgress = (double) w / warmupCount;
				context.Step(-1);
				hashCode.Add(context.Framebuffer.BufferSpan);
				yield return null;
			}

			GC.Collect();

			using var totalUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
			using var totalReservedMemoryRecorder =
				ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
			using var gcUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");
			using var systemUsedMemoryRecorder =
				ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");

			yield return null;

			var stopwatch = Stopwatch.StartNew();
			for (var m = 0; m < measurementCount; m++)
			{
				var gen0 = GC.CollectionCount(0);
				var gen1 = GC.CollectionCount(1);
				var gen2 = GC.CollectionCount(2);

				var totalUsedMemory     = totalUsedMemoryRecorder.LastValue;
				var totalReservedMemory = totalReservedMemoryRecorder.LastValue;
				var gcUsedMemory        = gcUsedMemoryRecorder.LastValue;
				var systemUsedMemory    = systemUsedMemoryRecorder.LastValue;

				State         = BenchmarkState.Measurement;
				StateProgress = (double) m / measurementCount;
				var startStamp = stopwatch.Elapsed;
				var elapsed    = TimeSpan.Zero;
				try
				{
					for (var i = 0; i < iterationCount; i++)
						context.Step(i);

					elapsed = stopwatch.Elapsed - startStamp;
					yield return null;
				}
				finally
				{
					samples[m] = new Sample(
						elapsed,
						GC.CollectionCount(0)                 - gen0,
						GC.CollectionCount(1)                 - gen1,
						GC.CollectionCount(2)                 - gen2,
						totalUsedMemoryRecorder.LastValue     - totalUsedMemory,
						totalReservedMemoryRecorder.LastValue - totalReservedMemory,
						gcUsedMemoryRecorder.LastValue        - gcUsedMemory,
						systemUsedMemoryRecorder.LastValue    - systemUsedMemory);
				}

				hashCode.Add(context.Framebuffer.BufferSpan);
			}

			StateProgress = 1.0;
		}
		finally
		{
			State = BenchmarkState.Done;
			context.Cleanup();
			callback.Invoke(hashCode.ToHashCode(), samples);
		}
	}
}

}
