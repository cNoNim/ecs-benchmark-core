using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace BenchmarkGenerator.Tests;

public static class CSharpIncrementalGeneratorVerifier<TIncrementalGenerator>
	where TIncrementalGenerator : IIncrementalGenerator, new()
{
	public static DiagnosticResult CompilerError(string compileError) => DiagnosticResult.CompilerError(compileError);

	public static DiagnosticResult CompilerWarning(string compilerWarning) =>
		DiagnosticResult.CompilerWarning(compilerWarning);

	public static DiagnosticResult CompilerInfo(string compilerInfo) => new(compilerInfo, DiagnosticSeverity.Info);

	public static async Task VerifySourceGeneratorAsync(
		string source,
		string generatedFolderName = "Default",
		params string[] expectedFileNames)
		=> await VerifySourceGeneratorAsync(
			source,
			DiagnosticResult.EmptyDiagnosticResults,
			true,
			generatedFolderName,
			expectedFileNames);

	public static async Task VerifySourceGeneratorAsync(
		string source,
		string generatedFolderName = "Default",
		IEnumerable<Assembly>? additionalAssembliesOverride = null,
		params string[] expectedFileNames)
		=> await VerifySourceGeneratorAsync(
			source,
			DiagnosticResult.EmptyDiagnosticResults,
			additionalAssembliesOverride ?? [],
			true,
			generatedFolderName,
			expectedFileNames);

	public static async Task VerifySourceGeneratorAsync(string source, params DiagnosticResult[] expected)
		=> await VerifySourceGeneratorAsync(source, expected, false);

	public static async Task VerifySourceGeneratorAsync(string source, DiagnosticResult expected,
		IEnumerable<Assembly> additionalAssembliesOverride)
		=> await VerifySourceGeneratorAsync(source, [expected], additionalAssembliesOverride, false);

	public static async Task VerifySourceGeneratorAsync(string source, DiagnosticResult expected,
		Assembly additionalAssemblyOverride)
		=> await VerifySourceGeneratorAsync(source, [expected], [additionalAssemblyOverride], false);

	public static async Task VerifySourceGeneratorAsync(string source, DiagnosticResult[] expected,
		Assembly additionalAssemblyOverride)
		=> await VerifySourceGeneratorAsync(source, expected, [additionalAssemblyOverride], false);

	public static async Task VerifySourceGeneratorAsync(string source, DiagnosticResult[] expected,
		IEnumerable<Assembly> additionalAssembliesOverride)
		=> await VerifySourceGeneratorAsync(source, expected, additionalAssembliesOverride, false);

	private static async Task VerifySourceGeneratorAsync(
		string source,
		DiagnosticResult[] expected,
		bool checksGeneratedSource = true,
		string generatedFolderName = "Default",
		params string[] expectedFileNames)
		=> await VerifySourceGeneratorAsync(
			source,
			expected,
			[],
			checksGeneratedSource,
			generatedFolderName,
			expectedFileNames);

	private static async Task VerifySourceGeneratorAsync(
		string source,
		DiagnosticResult[] expected,
		IEnumerable<Assembly> additionalAssembliesOverride,
		bool checksGeneratedSource = true,
		string generatedFolderName = "Default",
		params string[] expectedFileNames)
	{
		var test = new Test { TestCode = source.ReplaceLineEndings() };
		foreach (var additionalReference in additionalAssembliesOverride)
			test.TestState.AdditionalReferences.Add(additionalReference);

		var executingAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
		if (checksGeneratedSource)
		{
			var generatedFolderPath = Path.Join(executingAssemblyPath, "Results", generatedFolderName);
			Directory.CreateDirectory(generatedFolderPath);
			var foundExpectedFiles =
				Directory.EnumerateFiles(generatedFolderPath)
					.Select(Path.GetFileName)
					.Where(expectedFileNames.Contains);
			var existingSources = foundExpectedFiles.Select(file =>
			{
				if (file == null)
					throw new InvalidOperationException();

				var fileDir = Path.Join(generatedFolderPath, file);
				var text = File.ReadAllText(fileDir).ReplaceLineEndings();
				return (Test.GetTestPath(file), SourceText.From(text, Encoding.UTF8));
			});
			test.TestState.GeneratedSources.AddRange(existingSources);
		}

		test.TestBehaviors = checksGeneratedSource ? TestBehaviors.None : TestBehaviors.SkipGeneratedSourcesCheck;
		test.ExpectedDiagnostics.AddRange(expected);
		await test.RunAsync(CancellationToken.None);
	}

	private class Test : SourceGeneratorTest<DefaultVerifier>
	{
		public static string GetTestPath(string filename)
			=> Path.Join(GetFilePathPrefixForGenerator(typeof(TIncrementalGenerator)), filename);

		private static string GetFilePathPrefixForGenerator(Type sourceGenType)
			=> Path.Combine(sourceGenType.Assembly.GetName().Name ?? string.Empty, sourceGenType.FullName!);

		public Test()
		{
			ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
			SolutionTransforms.Add((solution, projectId) =>
			{
				var compilationOptions = solution.GetProject(projectId)?.CompilationOptions;
				if (compilationOptions == null)
					throw new ArgumentException("ProjectId does not exist");
				compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
					compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
				solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

				return solution;
			});
		}

		private static LanguageVersion DefaultLanguageVersion =>
			Enum.TryParse("Default", out LanguageVersion version) ? version : LanguageVersion.CSharp9;


		protected override CompilationOptions CreateCompilationOptions()
			=> new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

		protected override ParseOptions CreateParseOptions()
			=> new CSharpParseOptions(DefaultLanguageVersion, DocumentationMode.Diagnose);

		protected override IEnumerable<Type> GetSourceGenerators() =>
			[typeof(TIncrementalGenerator)];

		protected override string DefaultFileExt =>
			"cs";

		public override string Language =>
			LanguageNames.CSharp;
	}
}
