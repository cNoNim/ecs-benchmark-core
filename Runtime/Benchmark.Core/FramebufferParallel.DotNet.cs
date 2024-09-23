#if !UNITY_2020_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Benchmark.Core.Components;

namespace Benchmark.Core
{

public readonly struct FramebufferParallel
	: IFramebuffer
{
	public readonly  int            Width;
	public readonly  int            Height;
	private readonly int[]         _buffer;
	private readonly List<DrawCall> _draws;

	public FramebufferParallel(
		int width,
		int height,
		int[] buffer,
		List<DrawCall> draws)
	{
		Width   = width;
		Height  = height;
		_buffer = buffer;
		_draws  = draws;
	}

	public ReadOnlySpan<int> Buffer
	{
		get => _buffer;
	}

	public ReadOnlySpan<DrawCall> Draws
	{
		get => CollectionsMarshal.AsSpan(_draws);
	}

	public void Clear() =>
		_buffer.AsSpan()
			   .Clear();

	public void Draw(
		int tick,
		uint id,
		int x,
		int y,
		SpriteMask c)
	{
		if (_draws.Count < _draws.Capacity)
			lock (_draws)
				_draws.Add(
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

		ref var location = ref _buffer[index];
		do
		{
			var value = location | (int) c;
			if (value == location)
				return;
			Interlocked.CompareExchange(ref location, value, location);
		} while (true);
	}
}

public static class FramebufferParallelExtensions
{
	public static FramebufferParallel AsParallel(this in Framebuffer framebuffer) =>
		new(
			framebuffer.Width,
			framebuffer.Height,
			framebuffer.Buffer,
			framebuffer.Draws);
}

}
#endif
