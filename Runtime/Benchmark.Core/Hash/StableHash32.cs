using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Benchmark.Core.Hash
{

public static partial class StableHash32
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(uint seed, ReadOnlySpan<int> values) =>
		Hash(seed, MemoryMarshal.Cast<int, uint>(values));

	[Pure]
	public static uint Hash(uint seed, ReadOnlySpan<uint> buf)
	{
		uint hash;
		var  index  = 0;
		var  length = buf.Length;

		if (length >= 4 * sizeof(uint))
		{
			var (h1, h2, h3, h4) = Initialize(seed);

			var limit = length - sizeof(uint);
			do
			{
				h1 = Round(h1, buf[index++]);
				h2 = Round(h2, buf[index++]);
				h3 = Round(h3, buf[index++]);
				h4 = Round(h4, buf[index++]);
			} while (index <= limit);

			hash = Merge(
				h1,
				h2,
				h3,
				h4);
		}
		else
			hash = ShortInitialize(seed);

		hash += (uint) length * sizeof(uint);

		while (index < length)
			hash = Finalize(hash, buf[index++]);

		return Avalanche(hash);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(uint seed, int v1) =>
		Hash(seed, (uint) v1);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(uint seed, int v1, int v2) =>
		Hash(seed, (uint) v1, (uint) v2);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(
		uint seed,
		int v1,
		int v2,
		int v3) =>
		Hash(
			seed,
			(uint) v1,
			(uint) v2,
			(uint) v3);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(
		uint seed,
		int v1,
		int v2,
		int v3,
		int v4) =>
		Hash(
			seed,
			(uint) v1,
			(uint) v2,
			(uint) v3,
			(uint) v4);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(
		uint seed,
		int v1,
		int v2,
		int v3,
		int v4,
		int v5) =>
		Hash(
			seed,
			(uint) v1,
			(uint) v2,
			(uint) v3,
			(uint) v4,
			(uint) v5);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(
		uint seed,
		int v1,
		int v2,
		int v3,
		int v4,
		int v5,
		int v6) =>
		Hash(
			seed,
			(uint) v1,
			(uint) v2,
			(uint) v3,
			(uint) v4,
			(uint) v5,
			(uint) v6);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(
		uint seed,
		int v1,
		int v2,
		int v3,
		int v4,
		int v5,
		int v6,
		int v7) =>
		Hash(
			seed,
			(uint) v1,
			(uint) v2,
			(uint) v3,
			(uint) v4,
			(uint) v5,
			(uint) v6,
			(uint) v7);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(
		uint seed,
		int v1,
		int v2,
		int v3,
		int v4,
		int v5,
		int v6,
		int v7,
		int v8) =>
		Hash(
			seed,
			(uint) v1,
			(uint) v2,
			(uint) v3,
			(uint) v4,
			(uint) v5,
			(uint) v6,
			(uint) v7,
			(uint) v8);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(uint seed, uint v1)
	{
		var hash = ShortInitialize(seed);
		hash += sizeof(uint);
		hash =  Finalize(hash, v1);
		return Avalanche(hash);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(uint seed, uint v1, uint v2)
	{
		var hash = ShortInitialize(seed);
		hash += 2 * sizeof(uint);
		hash =  Finalize(hash, v1);
		hash =  Finalize(hash, v2);
		return Avalanche(hash);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(
		uint seed,
		uint v1,
		uint v2,
		uint v3)
	{
		var hash = ShortInitialize(seed);
		hash += 3 * sizeof(uint);
		hash =  Finalize(hash, v1);
		hash =  Finalize(hash, v2);
		hash =  Finalize(hash, v3);
		return Avalanche(hash);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(
		uint seed,
		uint v1,
		uint v2,
		uint v3,
		uint v4)
	{
		var (h1, h2, h3, h4) = Initialize(seed);

		h1 = Round(h1, v1);
		h2 = Round(h2, v2);
		h3 = Round(h3, v3);
		h4 = Round(h4, v4);

		var hash = Merge(
			h1,
			h2,
			h3,
			h4);
		hash += 4 * sizeof(uint);
		return Avalanche(hash);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(
		uint seed,
		uint v1,
		uint v2,
		uint v3,
		uint v4,
		uint v5)
	{
		var (h1, h2, h3, h4) = Initialize(seed);

		h1 = Round(h1, v1);
		h2 = Round(h2, v2);
		h3 = Round(h3, v3);
		h4 = Round(h4, v4);

		var hash = Merge(
			h1,
			h2,
			h3,
			h4);
		hash += 5 * sizeof(uint);
		hash =  Finalize(hash, v5);
		return Avalanche(hash);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(
		uint seed,
		uint v1,
		uint v2,
		uint v3,
		uint v4,
		uint v5,
		uint v6)
	{
		var (h1, h2, h3, h4) = Initialize(seed);

		h1 = Round(h1, v1);
		h2 = Round(h2, v2);
		h3 = Round(h3, v3);
		h4 = Round(h4, v4);

		var hash = Merge(
			h1,
			h2,
			h3,
			h4);
		hash += 6 * sizeof(uint);
		hash =  Finalize(hash, v5);
		hash =  Finalize(hash, v6);
		return Avalanche(hash);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(
		uint seed,
		uint v1,
		uint v2,
		uint v3,
		uint v4,
		uint v5,
		uint v6,
		uint v7)
	{
		var (h1, h2, h3, h4) = Initialize(seed);

		h1 = Round(h1, v1);
		h2 = Round(h2, v2);
		h3 = Round(h3, v3);
		h4 = Round(h4, v4);

		var hash = Merge(
			h1,
			h2,
			h3,
			h4);
		hash += 7 * sizeof(uint);
		hash =  Finalize(hash, v5);
		hash =  Finalize(hash, v6);
		hash =  Finalize(hash, v7);
		return Avalanche(hash);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static uint Hash(
		uint seed,
		uint v1,
		uint v2,
		uint v3,
		uint v4,
		uint v5,
		uint v6,
		uint v7,
		uint v8)
	{
		var (h1, h2, h3, h4) = Initialize(seed);

		h1 = Round(h1, v1);
		h2 = Round(h2, v2);
		h3 = Round(h3, v3);
		h4 = Round(h4, v4);

		h1 = Round(h1, v4);
		h2 = Round(h2, v6);
		h3 = Round(h3, v7);
		h4 = Round(h4, v8);

		var hash = Merge(
			h1,
			h2,
			h3,
			h4);
		hash += 8 * sizeof(uint);
		return Avalanche(hash);
	}
}

}
