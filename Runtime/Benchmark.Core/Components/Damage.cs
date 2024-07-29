using System.Diagnostics;

namespace Benchmark.Core.Components
{

[DebuggerDisplay("<{Attack}:{Defence}:{Cooldown}>")]
public struct Damage
{
	public int Attack;
	public int Defence;
	public int Cooldown;

	public override string ToString() =>
		$"{nameof(Damage)}<{Attack}:{Defence}:{Cooldown}>";
}

}
