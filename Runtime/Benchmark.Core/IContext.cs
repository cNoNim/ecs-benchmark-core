namespace Benchmark.Core
{

public interface IContext
{
	public Framebuffer Framebuffer { get; }
	public void Setup(int entityCount, Framebuffer framebuffer);
	public void Step(int tick);
	public void Cleanup();
}

}
