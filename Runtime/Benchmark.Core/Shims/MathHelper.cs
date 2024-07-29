using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Benchmark.Core.Shims
{

internal static class MathHelper
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int PopCount(ulong value)
#if UNITY_2018_3_OR_NEWER
		=> math.countbits(value);
#elif NET5_0_OR_GREATER
		=> System.Numerics.BitOperations.PopCount(value);
#else
	{
		const ulong c1 = 0x_55555555_55555555ul;
		const ulong c2 = 0x_33333333_33333333ul;
		const ulong c3 = 0x_0F0F0F0F_0F0F0F0Ful;
		const ulong c4 = 0x_01010101_01010101ul;

		value -= (value >> 1) & c1;
		value = (value & c2) + ((value >> 2) & c2);
		value = (((value + (value >> 4)) & c3) * c4) >> 56;

		return (int)value;
	}
#endif

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int RoundUpToPowerOf2(int value)
#if UNITY_2021_3_OR_NEWER
		=> Mathf.NextPowerOfTwo(value);
#elif NET5_0_OR_GREATER
			=> (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)value);
#else
		{
			--value;
			value |= value >> 1;
			value |= value >> 2;
			value |= value >> 4;
			value |= value >> 8;
			value |= value >> 16;
			return value + 1;
		}
#endif

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int TrailingZeroCount(ulong value)
#if UNITY_2018_3_OR_NEWER
		=> math.tzcnt(value);
#elif NET5_0_OR_GREATER
		=> System.Numerics.BitOperations.TrailingZeroCount(value);
#else
	{
		var lo = (uint)value;
		if (lo == 0)
			return 32 + TrailingZeroCount((uint)(value >> 32));
		return TrailingZeroCount(lo);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int TrailingZeroCount(uint value) =>
		Unsafe.AddByteOffset(
			ref System.Runtime.InteropServices.MemoryMarshal.GetReference(TrailingZeroCountDeBruijn),
			(System.IntPtr)(int)(((value & (uint)-(int)value) * 0x077CB531u) >> 27)
		);

	private static System.ReadOnlySpan<byte> TrailingZeroCountDeBruijn =>
		new byte[]
		{
			00, 01, 28, 02, 29, 14, 24, 03,
			30, 22, 20, 15, 25, 17, 04, 08,
			31, 27, 13, 23, 21, 19, 16, 07,
			26, 12, 18, 06, 11, 05, 10, 09
		};
#endif
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint RotateLeft(uint value, int offset)
#if UNITY_2018_3_OR_NEWER
		=> math.rol(value, offset);
#elif NET5_0_OR_GREATER
		=> System.Numerics.BitOperations.RotateLeft(value, offset);
#else
		=> (value << offset) | (value >> (32 - offset));
#endif
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong RotateLeft(ulong value, int offset)
#if UNITY_2018_3_OR_NEWER
		=> math.rol(value, offset);
#elif NET5_0_OR_GREATER
		=> System.Numerics.BitOperations.RotateLeft(value, offset);
#else
		=> (value << offset) | (value >> (64 - offset));
#endif

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Log2(ulong value)
#if NET5_0_OR_GREATER
		=> System.Numerics.BitOperations.Log2(value);
#else
	{
		value |= 1;
		var hi = (uint) (value >> 32);
		if (hi == 0)
			return Log2((uint) value);
		return 32 + Log2(hi);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static int Log2(uint val) =>
		(int) ((BitConverter.DoubleToInt64Bits(val) >> 52) + 1) & 0xFF;
#endif
}

}
