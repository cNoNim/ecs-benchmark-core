#if UNITY_2020_3_OR_NEWER
using System;
using Benchmark.Core.Components;
using Unity.Collections;

namespace Benchmark.Core
{

public struct Framebuffer : IDisposable
{
	private          NativeArray<uint>    _buffer;
	private          NativeList<DrawCall> _draws;
	private readonly int                  _width;
	private readonly int                  _height;

	public Framebuffer(int width, int height, int capacity = 0)
	{
		_width  = width;
		_height = height;
		_buffer = new NativeArray<uint>(width * height, Allocator.Persistent);
		_draws  = new NativeList<DrawCall>(capacity, Allocator.Persistent);
	}

	public ReadOnlySpan<uint> Buffer
	{
		get => _buffer.AsReadOnlySpan();
	}

	public ReadOnlySpan<DrawCall> Draws
	{
		get =>
			_draws.AsArray()
				  .AsReadOnlySpan();
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
		if (_draws.Length < _draws.Capacity)
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

	public void Dispose()
	{
		_buffer.Dispose();
		_draws.Dispose();
	}
}

}
#endif
