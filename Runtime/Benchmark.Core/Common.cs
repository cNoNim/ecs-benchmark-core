using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Benchmark.Core
{

public static class Common
{
	public const float Zero              = 0.0f;
	public const float Half              = 0.5f;
	public const float One               = 1.0f;
	public const float Two               = 2.0f;
	public const float Three             = 3.0f;
	public const float Five              = 5.0f;
	public const float Nine              = 9.0f;
	public const float SpawnAreaMaxX     = 32.0f;
	public const float SpawnAreaMaxY     = 24.0f;
	public const float SpawnAreaMargin   = 10.0f;
	public const int   FrameBufferWidth  = (int) (SpawnAreaMaxX + SpawnAreaMargin * Two);
	public const int   FrameBufferHeight = (int) (SpawnAreaMaxY + SpawnAreaMargin * Two);
	public const float AttackSpeed       = 4.0f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int AttackTicks(float2 position, float2 targetPosition) =>
		(int) (math.distance(position, targetPosition) / AttackSpeed);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float2 GetPosition(float x, float y) =>
		new(x, y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float2 ClampPosition(float2 position) =>
		math.clamp(
			position,
			new float2(-SpawnAreaMaxX                  + Half, -SpawnAreaMaxY                  + Half),
			new float2(SpawnAreaMaxX + SpawnAreaMargin - Half, SpawnAreaMaxY + SpawnAreaMargin - Half));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float2 SelectVelocity(float2 position, float x, float y) =>
		position.x > position.y ? new float2(x, y) : new float2(y, x);
}

}
