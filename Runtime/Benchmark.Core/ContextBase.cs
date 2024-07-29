using System;
using System.Runtime.CompilerServices;
using Benchmark.Core.Components;
using Benchmark.Core.Random;
using Unity.Mathematics;
using static Benchmark.Core.Common;

namespace Benchmark.Core
{

public abstract class ContextBase : IContext
{
	public const     int         RespawnTicks = 5;
	private readonly string?     _name;
	protected        Framebuffer Framebuffer;

	protected ContextBase(string? name) =>
		_name = name;

	protected int EntityCount { get; private set; }

	Framebuffer IContext.Framebuffer
	{
		get => Framebuffer;
	}

	public void Setup(int entityCount, Framebuffer framebuffer)
	{
		Framebuffer = framebuffer;
		EntityCount = entityCount;
		DoSetup();
	}

	public void Step(int tick)
	{
		Framebuffer.Clear();
		DoRun(tick);
	}

	public void Cleanup() =>
		DoCleanup();

	public static UnitType SpawnUnit(
		in Data data,
		ref Unit unit,
		out Health health,
		out Damage damage,
		out Sprite sprite,
		out Position position,
		out Velocity velocity)
	{
		unit.SpawnTick = data.Tick;
		var rng = new RandomGenerator(unit.Seed);
		var unitType = rng.Random(ref unit.Counter, 1, 100) switch
		{
			<= 3  => UnitType.NPC,
			<= 30 => UnitType.Hero,
			_     => UnitType.Monster,
		};

		switch (unitType)
		{
		case UnitType.NPC:
			health.Hp       = rng.Random(ref unit.Counter, 6, 12);
			damage.Defence  = rng.Random(ref unit.Counter, 3, 8);
			damage.Attack   = 0;
			damage.Cooldown = 0;
			break;
		case UnitType.Hero:
			health.Hp       = rng.Random(ref unit.Counter, 5, 15);
			damage.Defence  = rng.Random(ref unit.Counter, 2, 6);
			damage.Attack   = rng.Random(ref unit.Counter, 4, 10);
			damage.Cooldown = rng.Random(ref unit.Counter, 2, 4);
			break;
		case UnitType.Monster:
			health.Hp       = rng.Random(ref unit.Counter, 4, 12);
			damage.Defence  = rng.Random(ref unit.Counter, 2, 8);
			damage.Attack   = rng.Random(ref unit.Counter, 3, 9);
			damage.Cooldown = rng.Random(ref unit.Counter, 3, 5);
			break;
		default:
			throw new InvalidOperationException();
		}

		sprite.Character = SpriteMask.Spawn;
		var x = rng.Random(ref unit.Counter, Zero, SpawnAreaMaxX + Two * SpawnAreaMargin) - SpawnAreaMargin;
		var y = rng.Random(ref unit.Counter, Zero, SpawnAreaMaxY + Two * SpawnAreaMargin) - SpawnAreaMargin;
		position.V = GetPosition(x, y);
		velocity   = default;
		return unitType;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void MovementSystemForEach(ref Position position, in Velocity velocity)
	{
		var pos = position.V;
		var vel = velocity.V;
		position.V = ClampPosition(new float2(pos.x + vel.x, pos.y + vel.y));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void UpdateDataSystemForEach(ref Data data) =>
		data.Tick++;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void UpdateVelocitySystemForEach(
		ref Velocity velocity,
		ref Unit unit,
		in Data data,
		in Position position)
	{
		if (data.Tick % 10 != 0)
			return;

		var rng = new RandomGenerator(unit.Seed);
		var x   = rng.Random(ref unit.Counter, Three, Nine);
		var y   = rng.Random(ref unit.Counter, Zero,  Five);
		velocity.V = SelectVelocity(position.V, x, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RenderSystemForEach(
		Framebuffer framebuffer,
		in Position position,
		in Sprite sprite,
		in Unit unit,
		in Data data) =>
		framebuffer.Draw(
			data.Tick,
			unit.Id,
			(int) (position.V.x + SpawnAreaMargin),
			(int) (position.V.y + SpawnAreaMargin),
			sprite.Character);

	protected abstract void DoSetup();
	protected abstract void DoRun(int tick);
	protected abstract void DoCleanup();

	public override string ToString() =>
		_name
	 ?? GetType()
		   .Name;
}

}
