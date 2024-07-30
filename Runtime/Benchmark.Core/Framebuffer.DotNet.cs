#if !UNITY_2020_3_OR_NEWER
using System;
using Benchmark.Core.Components;
using Benchmark.Core.Hash;

namespace Benchmark.Core
{
	public struct Framebuffer : IDisposable
	{
		private readonly uint[] _buffer;
		private readonly DrawCall[] _draws;
		private readonly int _width;
		private readonly int _height;
		private int _drawCount;

		public Framebuffer(int width, int height, int capacity = 0)
		{
			_width = width;
			_height = height;
			_buffer = new uint[width * height];
			_draws = new DrawCall[capacity];
			_drawCount = 0;
		}

		public ReadOnlySpan<uint> Buffer => _buffer;

		public ReadOnlySpan<DrawCall> Draws => new(_draws, 0, _drawCount);

		public uint HashCode =>
			StableHash32.Hash(0, _buffer);

		public void Clear()
		{
			_buffer.AsSpan().Clear();
		}

		public void Draw(int tick, uint id, int x, int y, SpriteMask c)
		{
			if (_drawCount < _draws.Length)
				_draws[_drawCount++] = new DrawCall(tick, id, x, y, c);
			if (x < 0 || x >= _width ||
			    y < 0 || y >= _height)
				return;
			var index = x + y * _width;
			_buffer[index] |= (uint)c;
		}

		public void Dispose()
		{
		}
	}
}
#endif
