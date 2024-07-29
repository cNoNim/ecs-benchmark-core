using System;
using System.IO;

namespace Benchmark.Core
{

public readonly struct Sample
{
	public const    int      Size = 52;
	public readonly TimeSpan TimeSpan;
	public readonly int      Gen0CollectionCount;
	public readonly int      Gen1CollectionCount;
	public readonly int      Gen2CollectionCount;
	public readonly long     TotalUsedMemory;
	public readonly long     TotalReservedMemory;
	public readonly long     GcUsedMemory;
	public readonly long     SystemUsedMemory;

	public Sample(
		TimeSpan timeSpan,
		int gen0CollectionCount,
		int gen1CollectionCount,
		int gen2CollectionCount,
		long totalUsedMemory,
		long totalReservedMemory,
		long gcUsedMemory,
		long systemUsedMemory)
	{
		TimeSpan            = timeSpan;
		Gen0CollectionCount = gen0CollectionCount;
		Gen1CollectionCount = gen1CollectionCount;
		Gen2CollectionCount = gen2CollectionCount;
		TotalUsedMemory     = totalUsedMemory;
		TotalReservedMemory = totalReservedMemory;
		GcUsedMemory        = gcUsedMemory;
		SystemUsedMemory    = systemUsedMemory;
	}

	public void WriteTo(BinaryWriter writer)
	{
		writer.Write(TimeSpan.Ticks);
		writer.Write(Gen0CollectionCount);
		writer.Write(Gen1CollectionCount);
		writer.Write(Gen2CollectionCount);
		writer.Write(TotalUsedMemory);
		writer.Write(TotalReservedMemory);
		writer.Write(GcUsedMemory);
		writer.Write(SystemUsedMemory);
	}

	public static Sample ReadFrom(BinaryReader reader) =>
		new(
			TimeSpan.FromTicks(reader.ReadInt64()),
			reader.ReadInt32(),
			reader.ReadInt32(),
			reader.ReadInt32(),
			reader.ReadInt64(),
			reader.ReadInt64(),
			reader.ReadInt64(),
			reader.ReadInt64());
}

}
