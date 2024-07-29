using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Benchmark.Generator;

[Generator]
public class ContextGenerator : IIncrementalGenerator
{
	private const string BenchmarkCore = "Benchmark.Core";
	private const string BenchmarkCoreIContext = "Benchmark.Core.IContext";
	private static readonly string EnumerableOfTFullName = typeof(IEnumerable<>).FullName;
	private static readonly string FuncFullName = typeof(Func<>).FullName;
	private static readonly string TupleFullName = typeof(ValueTuple<,>).FullName;
	private static readonly string TypeFullName = typeof(Type).FullName;

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var types = context.CompilationProvider
			.SelectMany(static (c, _) =>
				c.SourceModule.ReferencedAssemblySymbols)
			.Where(static a =>
				a.Name == BenchmarkCore ||
				a.Modules.Any(m =>
					m.ReferencedAssemblies.Any(ra => ra.Name == BenchmarkCore)))
			.SelectMany(static (a, _) =>
				GetAllNamespaces(a.GlobalNamespace))
			.SelectMany(static (ns, _) =>
				ns.GetTypeMembers())
			.Combine(context.CompilationProvider
				.Select(static (c, _) =>
					c.GetTypeByMetadataName(BenchmarkCoreIContext)))
			.Where(static tuple =>
				tuple.Right != null &&
				!tuple.Left.IsAbstract &&
				tuple.Left.AllInterfaces.Contains(tuple.Right) &&
				tuple.Left.Constructors.Any(constructor => constructor.Parameters.IsEmpty))
			.Select(static (tuple, _) =>
				tuple.Left.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
			.Collect();

		var enumerableType = context.CompilationProvider
			.Select(static (c, _) =>
			{
				var i = c.GetTypeByMetadataName(BenchmarkCoreIContext);
				if (i == null)
					return null;

				var enumerableType = c.GetTypeByMetadataName(EnumerableOfTFullName);
				if (enumerableType == null)
					return null;
				var funcType = c.GetTypeByMetadataName(FuncFullName);
				if (funcType == null)
					return null;
				var tupleType = c.GetTypeByMetadataName(TupleFullName);
				if (tupleType == null)
					return null;
				var typeType = c.GetTypeByMetadataName(TypeFullName);
				if (typeType == null)
					return null;

				var type = enumerableType.Construct(c.CreateTupleTypeSymbol(
					ImmutableArray.Create<ITypeSymbol>([typeType, funcType.Construct(i)]),
					ImmutableArray.Create(["type", "factory"])));
				return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			});

		context.RegisterSourceOutput(types.Combine(enumerableType), Generate);
	}

	private static void Generate(SourceProductionContext ctx,
		(ImmutableArray<string> Types, string EnumerableType) parameters)
	{
		try
		{
			GenerateInternal(ctx, parameters.Types, parameters.EnumerableType);
		}
		catch (Exception exception)
		{
			ctx.ReportDiagnostic(Diagnostic.Create(
				new DiagnosticDescriptor(
					"BG0000",
					$"An exception was thrown by the {nameof(ContextGenerator)} generator",
					$"An exception was thrown by the {nameof(ContextGenerator)} generator: '{{0}}'",
					nameof(ContextGenerator),
					DiagnosticSeverity.Error,
					isEnabledByDefault: true),
				Location.None,
				exception.ToString()));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void GenerateInternal(
		SourceProductionContext ctx,
		ImmutableArray<string> types,
		string contextEnumerable)
	{
		if (types.Length == 0 || contextEnumerable == null)
			return;

		var sb = new StringBuilder();
		var nl = Environment.NewLine;
		sb.Append("namespace Benchmark").Append(nl).Append("{").Append(nl);
		sb.Append("\tinternal static partial class Contexts").Append(nl).Append("\t{").Append(nl);
		sb.Append("\t\tpublic static ");
		sb.Append(contextEnumerable);
		sb.Append(" Factories").Append(nl).Append("\t\t{").Append(nl);
		sb.Append("\t\t\tget").Append(nl).Append("\t\t\t{").Append(nl);

		foreach (var type in types)
		{
			sb.Append("\t\t\t\tyield return (typeof(");
			sb.Append(type);
			sb.Append("), () => new ");
			sb.Append(type);
			sb.Append("());");
			sb.Append(nl);
		}

		sb.Append("\t\t\t}").Append(nl).Append("\t\t}").Append(nl).Append("\t}").Append(nl).Append("}").Append(nl);
		var sourceText = SourceText.From(sb.ToString(), Encoding.UTF8);
		ctx.AddSource("BenchmarkContexts.g.cs", sourceText);
	}

	private static ImmutableArray<INamespaceSymbol> GetAllNamespaces(INamespaceSymbol root)
	{
#pragma warning disable RS1024 // https://github.com/dotnet/roslyn-analyzers/issues/4568
		var builder = new HashSet<INamespaceSymbol>(SymbolEqualityComparer.Default);
#pragma warning restore RS1024
		var visit = new Queue<INamespaceSymbol>();
		visit.Enqueue(root);

		do
		{
			var search = visit.Dequeue();
			builder.Add(search);

			foreach (var space in search.GetNamespaceMembers())
			{
				if (builder.Contains(space))
					continue;

				visit.Enqueue(space);
			}
		} while (visit.Count > 0);

		return builder.ToImmutableArray();
	}
}
