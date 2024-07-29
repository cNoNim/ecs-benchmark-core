using System;
using System.Collections;
using Benchmark.Core;
using Unity.PerformanceTesting;
using UnityEngine.Profiling;
using static Benchmark.Core.Common;

namespace Benchmark.Performance.Tests
{

public abstract class BenchmarkBase
{
	private static readonly Recorder    GCRecorder    = Recorder.Get("GC.Alloc");
	private static readonly SampleGroup SampleGroup   = new("Time", SampleUnit.Microsecond);
	private static readonly SampleGroup SampleGroupGC = new("GC.Alloc", SampleUnit.Undefined);

	protected static IEnumerator Routine(
		IContext context,
		int entityCount,
		int warmUpCount,
		int measurementCount,
		int iterationCount)
	{
		GCRecorder.enabled = false;

		context.Setup(entityCount, new Framebuffer(FrameBufferWidth, FrameBufferHeight));

		yield return null;

		for (var i = 0; i < warmUpCount; i++)
		{
			context.Step(-1);
			yield return null;
		}

		for (var i = 0; i < measurementCount; i++)
		{
			StartGCRecorder();

			using (Measure.Scope(SampleGroup))
				for (var j = 0; j < iterationCount; j++)
					context.Step(i);

			EndGCRecorderAndMeasure(iterationCount);
			yield return null;
		}

		context.Cleanup();
	}

	private static void StartGCRecorder()
	{
		GC.Collect();

		GCRecorder.enabled = false;
		GCRecorder.enabled = true;
	}

	private static void EndGCRecorderAndMeasure(int iterations)
	{
		GCRecorder.enabled = false;
		Measure.Custom(SampleGroupGC, (double) GCRecorder.sampleBlockCount / iterations);
	}
}

}
