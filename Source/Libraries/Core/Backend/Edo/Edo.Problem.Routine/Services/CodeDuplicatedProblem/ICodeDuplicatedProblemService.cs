using System.Threading;
using System.Threading.Tasks;

namespace Edo.Problem.Routine.Services.CodeDuplicatedProblem
{
	public interface ICodeDuplicatedProblemService
	{
		Task ProcessProblemTasks(CancellationToken cancellationToken);
	}
}