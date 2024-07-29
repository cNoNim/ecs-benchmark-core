using System;
using System.Collections;
using Benchmark.Core;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Profiling;
using static Benchmark.Core.Common;

namespace Benchmark.Performance.Tests
{

public abstract class BenchmarkBase
{
	private readonly Recorder    _gcRecorder    = Recorder.Get("GC.Alloc");
	private readonly SampleGroup _sampleGroup   = new("Time", SampleUnit.Microsecond);
	private readonly SampleGroup _sampleGroupGC = new("GC.Alloc", SampleUnit.Undefined);

	[SetUp]
	public void Setup() =>
		_gcRecorder.enabled = false;

	protected IEnumerator Routine(
		IContext context,
		int entityCount,
		int warmUpCount,
		int measurementCount,
		int iterationCount)
	{
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

			using (Measure.Scope(_sampleGroup))
				for (var j = 0; j < iterationCount; j++)
					context.Step(i);

			EndGCRecorderAndMeasure(iterationCount);
			yield return null;
		}

		context.Cleanup();
	}

	private void StartGCRecorder()
	{
		GC.Collect();

		_gcRecorder.enabled = false;
		_gcRecorder.enabled = true;
	}

	private void EndGCRecorderAndMeasure(int iterations)
	{
		_gcRecorder.enabled = false;
		Measure.Custom(_sampleGroupGC, (double) _gcRecorder.sampleBlockCount / iterations);
	}
}

}
