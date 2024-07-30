#if !UNITY_2020_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Benchmark.Core.Components;

namespace Benchmark.Core
{

public readonly struct Framebuffer : IDisposable
{
	private readonly uint[]         _buffer;
	private readonly List<DrawCall> _draws;
	private readonly int            _width;
	private readonly int            _height;

	public Framebuffer(int width, int height, int capacity = 0)
	{
		_width  = width;
		_height = height;
		_buffer = new uint[width * height];
		_draws  = new List<DrawCall>(capacity);
	}

	public ReadOnlySpan<uint> Buffer
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
			_draws.Add(
				new DrawCall(
					tick,
					id,
					x,
					y,
					c));
		if (x < 0
		 || x >= _width
		 || y < 0
		 || y >= _height)
			return;
		var index = x + y * _width;
		_buffer[index] |= (uint) c;
	}

	public void Dispose() {}
}

}
#endif
