using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Benchmark.Core.Components
{

public interface IAttack
{
	public int Damage { get; }
}

[DebuggerDisplay("<{Target}:{Damage}:{Ticks}>")]
public struct Attack<TEntity> : IAttack
{
	public TEntity Target;
	public int     Damage;
	public int     Ticks;

	public override string ToString() =>
		$"{nameof(Attack<TEntity>)}<{Target}:{Damage}:{Ticks}>";

	int IAttack.Damage
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Damage;
	}
}

}
