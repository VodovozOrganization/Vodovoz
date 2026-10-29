using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Problems.Custom;
using Edo.Problems.Exception;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems
{
	public abstract class EdoProblemRegistrarBase
	{
		protected EdoProblemRegistrarBase(
			EdoTaskCustomSourcesPersister customSourcesPersister,
			EdoTaskExceptionSourcesPersister exceptionSourcesPersister
			)
		{
			CustomSourcesPersister = customSourcesPersister ?? throw new ArgumentNullException(nameof(customSourcesPersister));
			ExceptionSourcesPersister = exceptionSourcesPersister ?? throw new ArgumentNullException(nameof(exceptionSourcesPersister));
		}
		
		protected EdoTaskCustomSourcesPersister CustomSourcesPersister { get; }
		protected EdoTaskExceptionSourcesPersister ExceptionSourcesPersister { get; }
		
		protected virtual EdoTaskProblemExceptionSource GetExceptionSource(System.Exception exception)
		{
			var sourceName = exception.GetType().Name;
			var sources = ExceptionSourcesPersister.GetEdoProblemExceptionSources();
			return sources.SingleOrDefault(x => x.Name == sourceName);
		}
		
		protected virtual async Task<EdoTaskProblem> GetExceptionProblemAndActivate(
			IUnitOfWork uow,
			EdoTask task,
			string exceptionMessage,
			EdoTaskProblemExceptionSource source,
			CancellationToken cancellationToken,
			bool needSave = false
		)
		{
			var problem = task.Problems.FirstOrDefault(x => x.SourceName == source.Name);
			
			if(problem is null)
			{
				problem = ExceptionEdoTaskProblem.Create(source.Name, task, exceptionMessage);
				await uow.SaveAsync(problem, cancellationToken: cancellationToken);
			}

			problem.CreationTime = DateTime.Now;
			problem.State = TaskProblemState.Active;
			return problem;
		}
	}
}
