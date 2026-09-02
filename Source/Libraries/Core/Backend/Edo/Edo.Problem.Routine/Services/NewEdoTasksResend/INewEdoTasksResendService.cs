using System;
using System.Threading;
using System.Threading.Tasks;

namespace Edo.Problem.Routine.Services.NewEdoTasksResend
{
	public interface INewEdoTasksResendService
	{
		Task<int> ResendStaleNewTasks(DateTime maxCreationTime, int batchSize, CancellationToken cancellationToken = default);
	}
}
