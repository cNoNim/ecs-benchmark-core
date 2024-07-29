using System.Diagnostics;

namespace Benchmark.Core.Components
{

[DebuggerDisplay("<{Tick}>")]
public struct Data
{
	public int Tick;

	public override string ToString() =>
		$"{nameof(Data)}<{Tick}>";
}

}
