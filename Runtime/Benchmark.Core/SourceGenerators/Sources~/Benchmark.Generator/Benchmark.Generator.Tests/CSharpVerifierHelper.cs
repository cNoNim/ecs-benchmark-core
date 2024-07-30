using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BenchmarkGenerator.Tests;

public static class CSharpVerifierHelper
{
	public static ImmutableDictionary<string, ReportDiagnostic> NullableWarnings { get; } =
		GetNullableWarningsFromCompiler();

	private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
	{
		string[] args = { "/warnaserror:nullable" };
		var commandLineArguments = CSharpCommandLineParser.Default.Parse(
			args,
			Environment.CurrentDirectory,
			Environment.CurrentDirectory);
		return commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;
	}
}
