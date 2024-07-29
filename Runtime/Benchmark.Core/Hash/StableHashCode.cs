using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Benchmark.Core.Hash
{

public struct StableHashCode
{
	private          uint _v1,     _v2,     _v3, _v4;
	private          uint _queue1, _queue2, _queue3;
	private          uint _length;
	private readonly uint _seed;

	public StableHashCode(uint seed = 0)
	{
		_seed   = seed;
		_v1     = 0;
		_v2     = 0;
		_v3     = 0;
		_v4     = 0;
		_queue1 = 0;
		_queue2 = 0;
		_queue3 = 0;
		_length = 0;
	}

	public void Add(in ReadOnlySpan<uint> buffer) =>
		Add(StableHash32.Hash(_seed, buffer));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add<T>(T value) =>
		Add(value?.GetHashCode() ?? 0);

	public void Add<T>(T value, IEqualityComparer<T>? comparer) =>
		Add(value is null ? 0 : comparer?.GetHashCode(value) ?? value.GetHashCode());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Add(int value) =>
		Add((uint) value);

	private void Add(uint val)
	{
		var previousLength = _length++;
		var position       = previousLength % 4;

		switch (position)
		{
		case 0:
			_queue1 = val;
			break;
		case 1:
			_queue2 = val;
			break;
		case 2:
			_queue3 = val;
			break;
		// position == 3
		default:
		{
			if (previousLength == 3)
				(_v1, _v2, _v3, _v4) = StableHash32.Initialize(_seed);

			_v1 = StableHash32.Round(_v1, _queue1);
			_v2 = StableHash32.Round(_v2, _queue2);
			_v3 = StableHash32.Round(_v3, _queue3);
			_v4 = StableHash32.Round(_v4, val);
			break;
		}
		}
	}

	public int ToHashCode()
	{
		var length   = _length;
		var position = length % 4;
		var hash = length < 4
			? StableHash32.ShortInitialize(_seed)
			: StableHash32.Merge(
				_v1,
				_v2,
				_v3,
				_v4);
		hash += length * 4;
		if (position > 0)
		{
			hash = StableHash32.Finalize(hash, _queue1);
			if (position > 1)
			{
				hash = StableHash32.Finalize(hash, _queue2);
				if (position > 2)
					hash = StableHash32.Finalize(hash, _queue3);
			}
		}

		hash = StableHash32.Avalanche(hash);
		return (int) hash;
	}
}

}
