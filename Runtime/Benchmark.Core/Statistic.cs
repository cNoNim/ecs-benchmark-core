using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Benchmark.Core
{

public readonly struct Statistic
{
	public readonly string   Name;
	public readonly int      Hash;
	public readonly Sample[] Samples;
	public readonly TimeSpan Sum;
	public readonly TimeSpan Min;
	public readonly TimeSpan Max;
	public readonly TimeSpan Average;
	public readonly TimeSpan Median;
	public readonly double   StandardDeviation;

	public Statistic(string name, int hash, Sample[] samples)
	{
		Hash              = hash;
		Name              = name;
		Samples           = samples;
		Sum               = TimeSpan.FromTicks(samples.Sum(sample => sample.TimeSpan.Ticks));
		Min               = samples.Min(sample => sample.TimeSpan);
		Max               = samples.Max(sample => sample.TimeSpan);
		Average           = Sum / samples.Length;
		Median            = TimeSpan.FromTicks(samples.Median());
		StandardDeviation = samples.StandardDeviation(Average.Ticks);
	}

	public override string ToString() =>
		new StringBuilder().Append(nameof(Name))
						   .Append(": ")
						   .Append(Name)
						   .Append(", ")
						   .Append(nameof(Min))
						   .Append(": ")
						   .Append(Min)
						   .Append(", ")
						   .Append(nameof(Max))
						   .Append(": ")
						   .Append(Max)
						   .Append(", ")
						   .Append(nameof(Average))
						   .Append(": ")
						   .Append(Average)
						   .Append(", ")
						   .Append(nameof(Median))
						   .Append(": ")
						   .Append(Median)
						   .Append(", ")
						   .Append(nameof(Sum))
						   .Append(": ")
						   .Append(Sum)
						   .Append(", ")
						   .Append(nameof(StandardDeviation))
						   .Append(": ")
						   .Append(StandardDeviation)
						   .ToString();

	public void WriteTo(BinaryWriter writer)
	{
		writer.Write(Name);
		writer.Write(Hash);
		writer.Write(Samples.Length);
		foreach (var sample in Samples)
			sample.WriteTo(writer);
	}

	public static Statistic ReadFrom(BinaryReader reader)
	{
		var name    = reader.ReadString();
		var hash    = reader.ReadInt32();
		var count   = reader.ReadInt32();
		var samples = new Sample[count];
		foreach (ref var sample in samples.AsSpan())
			sample = Sample.ReadFrom(reader);
		return new Statistic(name, hash, samples);
	}
}

}
