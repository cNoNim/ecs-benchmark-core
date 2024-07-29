#if UNITY_2020_3_OR_NEWER
using System;
using Benchmark.Core.Components;
using Benchmark.Core.Hash;
using Unity.Collections;

namespace Benchmark.Core
{

public struct Framebuffer : IDisposable
{
	private          NativeArray<uint>     _buffer;
	private          NativeArray<DrawCall> _draws;
	private readonly int                   _width;
	private readonly int                   _height;
	private          int                   _drawCount;

	public Framebuffer(int width, int height, int capacity = 0)
	{
		_width     = width;
		_height    = height;
		_buffer    = new NativeArray<uint>(width * height, Allocator.Persistent);
		_draws     = new NativeArray<DrawCall>(capacity, Allocator.Persistent);
		_drawCount = 0;
	}

	public ReadOnlySpan<uint> Buffer
	{
		get => _buffer.AsReadOnlySpan();
	}

	public ReadOnlySpan<DrawCall> Draws
	{
		get => _draws.AsReadOnlySpan()[.._drawCount];
	}

	public uint HashCode
	{
		get => StableHash32.Hash(0, _buffer);
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
		if (_drawCount < _draws.Length)
			_draws[_drawCount++] = new DrawCall(
				tick,
				id,
				x,
				y,
				c);
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
		_drawCount = 0;
	}
}

}
#endif
