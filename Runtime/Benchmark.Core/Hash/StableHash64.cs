using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Benchmark.Core.Shims;

namespace Benchmark.Core.Hash
{

public static class StableHash64
{
	private const ulong P1 = 0x9E3779B185EBCA87UL;
	private const ulong P2 = 0xC2B2AE3D27D4EB4FUL;
	private const ulong P3 = 0x165667B19E3779F9UL;
	private const ulong P4 = 0x85EBCA77C2B2AE63UL;
	private const ulong P5 = 0x27D4EB2F165667C5UL;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static ulong Hash(ulong seed, ReadOnlySpan<long> values) =>
		Hash(seed, MemoryMarshal.Cast<long, ulong>(values));

	[Pure]
	public static ulong Hash(ulong seed, ReadOnlySpan<ulong> buf)
	{
		ulong hash;
		var   index  = 0;
		var   length = buf.Length;

		if (length >= 4 * sizeof(ulong))
		{
			var (h1, h2, h3, h4) = Initialize(seed);

			var limit = length - sizeof(ulong);
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
			hash = MergeRound(hash, h1);
			hash = MergeRound(hash, h2);
			hash = MergeRound(hash, h3);
			hash = MergeRound(hash, h4);
		}
		else
			hash = ShortInitialize(seed);

		hash += (uint) length * sizeof(ulong);

		while (index < length)
			hash = Finalize(hash, buf[index++]);

		return Avalanche(hash);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static ulong Hash(ulong seed, int v1, int v2) =>
		Hash(seed, (uint) v1, (uint) v2);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static ulong Hash(ulong seed, uint v1, uint v2) =>
		Hash(seed, (ulong) v1 << 32 | v2);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static ulong Hash(ulong seed, long v1) =>
		Hash(seed, (ulong) v1);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static ulong Hash(ulong seed, ulong v1)
	{
		var hash = ShortInitialize(seed);
		hash += sizeof(ulong);
		hash =  Finalize(hash, v1);
		return Avalanche(hash);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static (ulong h1, ulong h2, ulong h3, ulong h4) Initialize(ulong seed) =>
		(h1: seed + P1 + P2, h2: seed + P2, h3: seed, h4: seed - P1);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong ShortInitialize(ulong seed) =>
		seed + P5;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong Merge(
		ulong v1,
		ulong v2,
		ulong v3,
		ulong v4) =>
		MathHelper.RotateLeft(v1, 1)
	  + MathHelper.RotateLeft(v2, 7)
	  + MathHelper.RotateLeft(v3, 12)
	  + MathHelper.RotateLeft(v4, 18);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong Round(ulong hash, ulong input) =>
		MathHelper.RotateLeft(hash + input * P2, 31) * P1;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong MergeRound(ulong hash, ulong input) =>
		(hash ^ Round(0, input)) * P1 + P4;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong Finalize(ulong hash, ulong input) =>
		MathHelper.RotateLeft(hash ^ Round(0, input), 27) * P1 + P4;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong Avalanche(ulong hash)
	{
		hash ^= hash >> 33;
		hash *= P2;
		hash ^= hash >> 29;
		hash *= P3;
		hash ^= hash >> 32;
		return hash;
	}
}

}
