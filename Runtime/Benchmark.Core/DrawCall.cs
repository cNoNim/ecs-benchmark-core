using Benchmark.Core.Components;

namespace Benchmark.Core
{

public readonly struct DrawCall
{
	private readonly int        _tick;
	private readonly uint       _id;
	private readonly int        _x;
	private readonly int        _y;
	private readonly SpriteMask _sprite;

	public DrawCall(
		int tick,
		uint id,
		int x,
		int y,
		SpriteMask sprite)
	{
		_tick   = tick;
		_id     = id;
		_x      = x;
		_y      = y;
		_sprite = sprite;
	}

	public void Deconstruct(
		out int tick,
		out uint id,
		out int x,
		out int y,
		out SpriteMask sprite)
	{
		tick   = _tick;
		id     = _id;
		x      = _x;
		y      = _y;
		sprite = _sprite;
	}
}

}
