using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Benchmark.Core
{

public static class StatisticExtensions
{
	public static string ToShortName(this Units units) =>
		units switch
		{
			Units.Nanoseconds  => "ns",
			Units.Microseconds => "µs",
			Units.Millisecond  => "ms",
			Units.Seconds      => "s",
			_                  => throw new ArgumentOutOfRangeException(nameof(units), units, null),
		};

	public static double To(this TimeSpan timeSpan, Units units) =>
		units switch
		{
			Units.Nanoseconds  => timeSpan.Ticks / (TimeSpan.TicksPerMillisecond / 1_000_000.0),
			Units.Microseconds => timeSpan.Ticks / (TimeSpan.TicksPerMillisecond / 1_000.0),
			Units.Millisecond  => timeSpan.TotalMilliseconds,
			Units.Seconds      => timeSpan.TotalSeconds,
			_                  => throw new ArgumentOutOfRangeException(nameof(units), units, null),
		};

	public static long Median(this Sample[] samples)
	{
		var array = ArrayPool<long>.Shared.Rent(samples.Length);
		for (var i = 0; i < array.Length; i++)
			array[i] = samples[i].TimeSpan.Ticks;
		var middleIndex = samples.Length / 2;
		Array.Sort(array, 0, samples.Length);
		return samples.Length % 2 != 0 ? array[middleIndex] : (array[middleIndex - 1] + array[middleIndex]) / 2;
	}

	public static double StandardDeviation(this Sample[] samples, long average)
	{
		var sum = 0L;
		foreach (var sample in samples)
		{
			var ticks = sample.TimeSpan.Ticks;
			sum += (ticks - average) * (ticks - average);
		}

		return Math.Sqrt((double) sum / samples.Length);
	}

	public static int[] Ranks(this IEnumerable<Statistic> array)
	{
		var values = array.Select(
							   (v, i) => new
							   {
								   Value = v,
								   Index = i,
							   })
						  .OrderBy(pair => pair.Value.Average)
						  .ToArray();

		var n = values.Length;
		if (n <= 0)
			return Array.Empty<int>();
		var ranks       = new int[n];
		var currentRank = 1;
		ranks[values[0].Index] = currentRank;
		for (var i = 1; i < n; i++)
			if (AreSame(values[i - 1].Value.Average.Ticks, values[i].Value.Average.Ticks))
				ranks[values[i].Index] = currentRank;
			else
				ranks[values[i].Index] = ++currentRank;
		return ranks;
	}

	private static bool AreSame(long x, long y) =>
		Math.Abs(x - y) < Math.Abs(x + y) / 2.0 * 0.01;
}

}
