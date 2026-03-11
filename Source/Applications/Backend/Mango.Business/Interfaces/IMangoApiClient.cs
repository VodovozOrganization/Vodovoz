using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mango.Business.Interfaces
{
	public interface IMangoApiClient
	{
		Task<string> GetCallsRawJsonAsync(
			DateTime startDate,
			DateTime endDate,
			CancellationToken cancellationToken);
	}
}
