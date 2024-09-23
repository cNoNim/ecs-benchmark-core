#if UNITY_2020_3_OR_NEWER
using System;
using Benchmark.Core.Components;
using Unity.Collections;

namespace Benchmark.Core
{

public struct Framebuffer
	: IDisposable,
	  IFramebuffer
{
	public readonly int                  Width;
	public readonly int                  Height;
	internal        NativeArray<int>     Buffer;
	internal        NativeList<DrawCall> Draws;

	public Framebuffer(int width, int height, int capacity = 0)
	{
		Width  = width;
		Height = height;
		Buffer = new NativeArray<int>(width * height, Allocator.Persistent);
		Draws  = new NativeList<DrawCall>(capacity, Allocator.Persistent);
	}

	public ReadOnlySpan<int> BufferSpan
	{
		get => Buffer.AsReadOnlySpan();
	}

	public ReadOnlySpan<DrawCall> DrawsSpan
	{
		get =>
			Draws.AsArray()
				 .AsReadOnlySpan();
	}

	public void Clear() =>
		Buffer.AsSpan()
			  .Clear();

	public void Draw(
		int tick,
		uint id,
		int x,
		int y,
		SpriteMask c)
	{
		if (Draws.Length < Draws.Capacity)
			Draws.Add(
				new DrawCall(
					tick,
					id,
					x,
					y,
					c));
		if (x < 0
		 || x >= Width
		 || y < 0
		 || y >= Height)
			return;
		var index = x + y * Width;
		Buffer.AsSpan()[index] |= (int) c;
	}

	public void Dispose()
	{
		Buffer.Dispose();
		Draws.Dispose();
	}
}

}
#endif
