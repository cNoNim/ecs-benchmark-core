using System.Diagnostics;

namespace Benchmark.Core.Components
{

[DebuggerDisplay("<{Id}:{Seed}:{Counter}>")]
public struct Unit
{
	public uint Id;
	public uint Seed;
	public int  Counter;
	public int  SpawnTick;
	public int  RespawnTick;

	public override string ToString() =>
		$"{nameof(Unit)}<{Id}:{Seed}:{Counter}>";

	public struct NPC
	{
		public override string ToString() =>
			nameof(NPC);
	}

	public struct Hero
	{
		public override string ToString() =>
			nameof(Hero);
	}

	public struct Monster
	{
		public override string ToString() =>
			nameof(Monster);
	}
}

}
