#if DEBUG
using System;
using System.Collections.Generic;

namespace Benchmark.Core
{

public interface ILogs
{
	public IEnumerable<(DateTimeOffset stamp, string message)> Logs { get; }
}

}
#endif
