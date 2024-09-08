using System;
using System.Buffers;

namespace Benchmark.Core.Algorithms
{

public static class RadixSort
{
	public static void SortWithIndirection(Span<uint> data, Span<int> indirection, int length)
	{
		var source = ArrayPool<ulong>.Shared.Rent(length);
		var temp   = ArrayPool<ulong>.Shared.Rent(length);

		for (var i = 0; i != length; ++i)
			source[i] = (ulong) i << 32 | data[i];

		Sort(
			0,
			source,
			temp,
			length);
		Sort(
			1,
			temp,
			source,
			length);
		Sort(
			2,
			source,
			temp,
			length);
		Sort(
			3,
			temp,
			source,
			length);

		for (var i = 0; i != length; ++i)
			unchecked
			{
				var item = source[i];
				data[i]        = (uint) item;
				indirection[i] = (int) (item >> 32);
			}

		ArrayPool<ulong>.Shared.Return(source);
		ArrayPool<ulong>.Shared.Return(temp);
	}

	private static void Sort(
		int byteIndex,
		Span<ulong> source,
		Span<ulong> dest,
		int length)
	{
		Span<uint> counter = stackalloc uint[256];
		var        shift   = byteIndex * 8;

		for (var i = 0; i != length; ++i)
			counter[(byte) (source[i] >> shift)]++;

		var indexStart = 0u;
		for (var i = 0; i < 256; ++i)
		{
			var count = counter[i];
			counter[i] =  indexStart;
			indexStart += count;
		}

		for (var i = 0; i < length; ++i)
		{
			var sourceItem = source[i];
			dest[(int) counter[(byte) (sourceItem >> shift)]++] = sourceItem;
		}
	}
}

}
