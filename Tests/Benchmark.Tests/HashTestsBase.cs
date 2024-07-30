using System.Collections.Generic;
using System.Text;
using Benchmark.Core;
using Benchmark.Core.Hash;
using NUnit.Framework;

namespace Benchmark.Tests
{

public abstract class HashTestsBase
{
	protected static void TestContexts(IEnumerable<IContext> contexts, int entityCount, int ticks)
	{
		uint? hash = default;
		var   fail = false;
		var   sb   = new StringBuilder();
		sb.AppendLine();
		foreach (var context in contexts)
			try
			{
				Test(
					context,
					ref hash,
					entityCount,
					ticks);
			}
			catch (AssertionException assert)
			{
				fail = true;
				sb.AppendLine(assert.Message);
			}

		if (fail)
			Assert.Fail(sb.ToString());
	}

	private static void Test(
		IContext context,
		ref uint? hash,
		int entityCount,
		int ticks)
	{
		using var framebuffer = new Framebuffer(Common.FrameBufferWidth, Common.FrameBufferHeight, entityCount * ticks);
		context.Setup(entityCount, framebuffer);
		try
		{
			var hashCode = new StableHashCode();
			for (var i = 0; i < ticks; i++)
			{
				context.Step(i);
				hashCode.Add(framebuffer.Buffer);
			}

			TestContext.Out.WriteLine(context.ToString());
			var sb = new StringBuilder();
			foreach (var (tick, id, x, y, c) in framebuffer.Draws)
				sb.AppendFormat(
					"{0:0000},{1},{2},{3},{4}\n",
					tick,
					id,
					x,
					y,
					c);
			TestContext.Out.Write(sb.ToString());
			var newHash = (uint) hashCode.ToHashCode();
			if (hash != null)
				Assert.That(newHash, Is.EqualTo(hash), context.ToString());
			hash = newHash;
		}
		finally
		{
			context.Cleanup();
		}
	}
}

}
