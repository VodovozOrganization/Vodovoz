using System;
using System.Threading;
using System.Threading.Tasks;
using Mango.Contracts.V1.Response;

namespace Mango.Business.Interfaces
{
	public interface IMangoApiClient
	{
		Task<GroupsResponse> GetGroupsAsync(
			CancellationToken cancellationToken);

		Task<CallsResponse> GetCallsAsync(
			string key,
			CancellationToken cancellationToken);

		Task<CallsStatResponse> GetCallsStatAsync(
			DateTime fromDate,
			DateTime toDate,
			CancellationToken cancellationToken);
	}
}
