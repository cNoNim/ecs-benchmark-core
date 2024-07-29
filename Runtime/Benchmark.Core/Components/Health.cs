using System.Diagnostics;

namespace Benchmark.Core.Components
{

[DebuggerDisplay("{Hp}")]
public struct Health
{
	public int Hp;

	public override string ToString() =>
		$"{nameof(Health)}<{Hp}>";
}

}
