#if !UNITY_2018_3_OR_NEWER
using System;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Unity.Mathematics
{

public readonly struct float2
{
	public readonly float x;
	public readonly float y;

	public float2(float x, float y)
	{
		this.x = x;
		this.y = y;
	}
}

#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
public static class math
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float2 clamp(float2 value, float2 lower, float2 upper)
	{
		var min = new float2(MathF.Min(upper.x, value.x), MathF.Min(upper.y, value.y));
		var max = new float2(MathF.Max(lower.x, min.x), MathF.Max(lower.y, min.y));
		return max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float distance(float2 x, float2 y)
	{
		var v = new float2(y.x - x.x, y.y - x.y);
		return MathF.Sqrt(v.x * v.x + v.y * v.y);
	}
}

}
#endif
