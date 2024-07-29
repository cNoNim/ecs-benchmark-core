using System.Diagnostics;
using Unity.Mathematics;

namespace Benchmark.Core.Components
{

[DebuggerDisplay("{V}")]
public struct Velocity
{
	public float2 V;

	public override string ToString() =>
		$"{nameof(Velocity)}{V}";
}

}
