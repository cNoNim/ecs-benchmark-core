#if UNITY_2020_3_OR_NEWER
using System.Threading;
using Benchmark.Core.Components;
using Unity.Collections;

namespace Benchmark.Core
{

public struct FramebufferParallel : IFramebuffer
{
	private readonly int                  _width;
	private readonly int                  _height;
	private readonly NativeArray<int>     _buffer;
	private          NativeList<DrawCall> _draws;
	private          SpinLock             _spinLock;

	public FramebufferParallel(
		int width,
		int height,
		NativeArray<int> buffer,
		NativeList<DrawCall> draws)
	{
		_width    = width;
		_height   = height;
		_buffer   = buffer;
		_draws    = draws;
		_spinLock = default;
	}

	public void Draw(
		int tick,
		uint id,
		int x,
		int y,
		SpriteMask c)
	{
		if (_draws.Length < _draws.Capacity)
		{
			var locked = false;
			try
			{
				_spinLock.Enter(ref locked);
				_draws.Add(
					new DrawCall(
						tick,
						id,
						x,
						y,
						c));
			}
			finally
			{
				if (locked)
					_spinLock.Exit();
			}
		}
		if (x < 0
		 || x >= _width
		 || y < 0
		 || y >= _height)
			return;
		var     index    = x + y * _width;
		ref var location = ref _buffer.AsSpan()[index];
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
