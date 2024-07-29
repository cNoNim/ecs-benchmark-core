using System.Diagnostics;
using Unity.Mathematics;

namespace Benchmark.Core.Components
{

[DebuggerDisplay("<{Entity}:{Position}>")]
public struct Target<TEntity>
{
	public TEntity Entity;
	public float2  Position;

	public Target(TEntity entity, Position position)
	{
		Entity   = entity;
		Position = position.V;
	}
}

}
