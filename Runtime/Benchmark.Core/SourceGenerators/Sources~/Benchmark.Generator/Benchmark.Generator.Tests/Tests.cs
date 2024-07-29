using Benchmark.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = BenchmarkGenerator.Tests.CSharpIncrementalGeneratorVerifier<Benchmark.Generator.ContextGenerator>;

namespace BenchmarkGenerator.Tests;

[TestClass]
public class Tests
{
	[TestMethod]
	public async Task Test() =>
		await VerifyCS.VerifySourceGeneratorAsync(string.Empty,
			nameof(Test),
			additionalAssembliesOverride: [typeof(IContext).Assembly],
			"BenchmarkContexts.g.cs");
}
