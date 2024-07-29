using System.Runtime.CompilerServices;
using Benchmark.Core.Hash;
using Benchmark.Core.Shims;

namespace Benchmark.Core.Random
{

public readonly struct RandomGenerator
{
	private readonly uint _seed;

	public RandomGenerator(uint seed) =>
		_seed = seed;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Random(ref int counter, int max) =>
		(int) Random(ref counter, (uint) max);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Random(ref int counter, int min, int max) =>
		(int) Random(ref counter, (uint) (max - min)) + min;

	public long Random(ref int counter, long max)
	{
		if (max < uint.MaxValue)
			return Random(ref counter, (uint) max);

		var bits = Log2Ceiling((ulong) max);
		while (true)
		{
			var result = NextState64(ref counter) >> sizeof(ulong) * 8 - bits;
			if (result < (ulong) max)
				return (long) result;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public long Random(ref int counter, long min, long max) =>
		Random(ref counter, max - min) + min;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float Random(ref int counter, float min, float max) =>
		NextFloat(ref counter) * (max - min) + min;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double Random(ref int counter, double min, double max) =>
		NextDouble(ref counter) * (max - min) + min;

	private double NextDouble(ref int counter) =>
		(NextState64(ref counter) >> 11) * (1.0 / (1ul << 53));

	private float NextFloat(ref int counter) =>
		(NextState(ref counter) >> 8) * (1.0f / (1u << 24));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private uint NextState(ref int counter) =>
		StableHash32.Hash(_seed, counter++);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private ulong NextState64(ref int counter) =>
		StableHash64.Hash(_seed, counter++, counter++);

	private uint Random(ref int counter, uint max)
	{
		var product = (ulong) max * NextState(ref counter);
		var low     = (uint) product;

		if (low < max)
			unchecked
			{
				var remainder = (0u - max) % max;
				while (low < remainder)
				{
					product = (ulong) max * NextState(ref counter);
					low     = (uint) product;
				}
			}

		return (uint) (product >> 32);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int Log2Ceiling(ulong value)
	{
		var result = MathHelper.Log2(value);
		return MathHelper.PopCount(value) != 1 ? result + 1 : result;
	}
}

}
