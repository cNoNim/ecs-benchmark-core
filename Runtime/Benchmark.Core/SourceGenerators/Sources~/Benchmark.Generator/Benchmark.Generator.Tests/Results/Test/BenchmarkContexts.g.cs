namespace Benchmark
{
	internal static partial class Contexts
	{
		public static global::System.Collections.Generic.IEnumerable<(global::System.Type type, global::System.Func<global::Benchmark.Core.IContext> factory)> Factories
		{
			get
			{
				yield return (typeof(global::Benchmark.Core.TestContext), () => new global::Benchmark.Core.TestContext());
			}
		}
	}
}
