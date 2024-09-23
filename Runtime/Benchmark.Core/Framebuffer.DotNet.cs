#if !UNITY_2020_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Benchmark.Core.Components;

namespace Benchmark.Core
{

public readonly struct Framebuffer
	: IDisposable,
	  IFramebuffer
{
	public readonly   int            Width;
	public readonly   int            Height;
	internal readonly int[]         Buffer;
	internal readonly List<DrawCall> Draws;

	public Framebuffer(int width, int height, int capacity = 0)
	{
		Width  = width;
		Height = height;
		Buffer = new int[width * height];
		Draws  = new List<DrawCall>(capacity);
	}

	public ReadOnlySpan<int> BufferSpan
	{
		get => Buffer;
	}

	public ReadOnlySpan<DrawCall> DrawsSpan
	{
		get => CollectionsMarshal.AsSpan(Draws);
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
		if (Draws.Count < Draws.Capacity)
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
		Buffer[index] |= (int) c;
	}

	public void Dispose() {}
}

}
#endif
