using System.Runtime.CompilerServices;

namespace Benchmark.Core.Hash
{

public static partial class StableHash32
{
	private const uint P1 = 0x9E3779B1U;
	private const uint P2 = 0x85EBCA77U;
	private const uint P3 = 0xC2B2AE3DU;
	private const uint P4 = 0x27D4EB2FU;
	private const uint P5 = 0x165667B1U;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static (uint h1, uint h2, uint h3, uint h4) Initialize(uint seed) =>
		(h1: seed + P1 + P2, h2: seed + P2, h3: seed, h4: seed - P1);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ShortInitialize(uint seed) =>
		seed + P5;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint Merge(
		uint v1,
		uint v2,
		uint v3,
		uint v4) =>
		RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint Round(uint hash, uint input) =>
		RotateLeft(hash + input * P2, 13) * P1;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint Finalize(uint hash, uint input) =>
		RotateLeft(hash + input * P3, 17) * P4;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint Avalanche(uint hash)
	{
		hash ^= hash >> 15;
		hash *= P2;
		hash ^= hash >> 13;
		hash *= P3;
		hash ^= hash >> 16;
		return hash;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint RotateLeft(uint value, int offset) =>
		value << offset | value >> 32 - offset;
}

}
