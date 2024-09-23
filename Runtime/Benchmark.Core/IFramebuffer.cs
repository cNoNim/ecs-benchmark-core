using Benchmark.Core.Components;

namespace Benchmark.Core
{

public interface IFramebuffer
{
	public void Draw(
		int tick,
		uint id,
		int x,
		int y,
		SpriteMask c);
}

}
