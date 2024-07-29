using System.Diagnostics;

namespace Benchmark.Core.Components
{

[DebuggerDisplay("<{Target}:{Damage}:{Ticks}>")]
public struct Attack<TEntity>
{
	public TEntity Target;
	public int     Damage;
	public int     Ticks;
}

}
