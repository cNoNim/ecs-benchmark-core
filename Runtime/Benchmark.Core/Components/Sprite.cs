using System;
using System.Diagnostics;

namespace Benchmark.Core.Components
{

[Flags]
public enum SpriteMask
{
	None    = 0,
	Spawn   = 1 << 0,
	Grave   = 1 << 1,
	NPC     = 1 << 2,
	Hero    = 1 << 3,
	Monster = 1 << 4,
}

[DebuggerDisplay("{Character}")]
public struct Sprite
{
	public SpriteMask Character;

	public override string ToString() =>
		$"{nameof(Sprite)}<{Character}>";
}

}
