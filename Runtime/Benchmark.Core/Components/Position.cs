using System.Diagnostics;
using Unity.Mathematics;

namespace Benchmark.Core.Components
{

[DebuggerDisplay("{V}")]
public struct Position
{
	public float2 V;

	public override string ToString() =>
		$"{nameof(Position)}{V}";
}

}
