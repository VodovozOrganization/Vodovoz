using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Problems.Custom;
using Edo.Problems.Exception;
using Edo.Problems.Exception.Sources;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;

namespace Edo.Problems
{
	public class FaultEdoProblemRegistrar : EdoProblemRegistrarBase
	{
		private readonly ILogger<FaultEdoProblemRegistrar> _logger;
		private readonly IGenericRepository<EdoTask> _edoTaskRepository;

		public FaultEdoProblemRegistrar(
			ILogger<FaultEdoProblemRegistrar> logger,
			IGenericRepository<EdoTask> edoTaskRepository,
			EdoTaskCustomSourcesPersister customSourcesPersister,
			EdoTaskExceptionSourcesPersister exceptionSourcesPersister
			) : base(customSourcesPersister, exceptionSourcesPersister)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoTaskRepository = edoTaskRepository ?? throw new ArgumentNullException(nameof(edoTaskRepository));
		}
		
		public async Task<bool> TryRegisterExceptionProblem(
			IUnitOfWork uow,
			EdoTask edoTask,
			System.Exception exception,
			CancellationToken cancellationToken
		)
		{
			if(edoTask is null)
			{
				return false;
			}
			
			_logger.LogError(exception, "Регистрируем ошибку при обработке задачи {EdoTaskId}", edoTask.Id);
			
			var source = GetExceptionSource(exception) ?? UnknownException.Create();
			var problem = await GetExceptionProblemAndActivate(uow, edoTask, exception.Message, source, cancellationToken);
			edoTask.UpdateStatusByEdoProblemImportance(source.Importance);

			await uow.SaveAsync(problem, cancellationToken: cancellationToken);
			await uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);

			return true;
		}
		
		public async Task<bool> TryRegisterExceptionProblem(
			IUnitOfWork uow,
			EdoTask edoTask,
			IEnumerable<MassTransitExceptionInfo> exceptions,
			CancellationToken cancellationToken
		)
		{
			if(edoTask is null)
			{
				return false;
			}

			EdoTaskProblemExceptionSource source = null;
			MassTransitExceptionInfo exceptionInfo = null;
			var sources = ExceptionSourcesPersister.GetEdoProblemExceptionSources();
			
			foreach(var exception in exceptions)
			{
				var sourceWithException = TryGetExceptionSource(exception, sources);
				
				if(sourceWithException.Source is null)
				{
					continue;
				}

				source = sourceWithException.Source;
				exceptionInfo = sourceWithException.ExceptionInfo;
				break;
			}
			
			if(source is null)
			{
				source = UnknownException.Create();
				exceptionInfo = exceptions.FirstOrDefault();
			}

			_logger.LogError("Регистрируем ошибку при обработке задачи {EdoTaskId}\n{StackTrace}", edoTask.Id, exceptionInfo?.StackTrace);
			
			var problem = await GetExceptionProblemAndActivate(uow, edoTask, exceptionInfo?.Message, source, cancellationToken);;
			edoTask.UpdateStatusByEdoProblemImportance(source.Importance);

			await uow.SaveAsync(problem, cancellationToken: cancellationToken);
			await uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);

			return true;
		}
		
		public async Task<bool> TryRegisterExceptionProblem(
			IUnitOfWork uow,
			IEnumerable<int> edoTaskIds,
			IEnumerable<MassTransitExceptionInfo> exceptions,
			CancellationToken cancellationToken
		)
		{
			if(edoTaskIds is null || !edoTaskIds.Any())
			{
				return false;
			}

			foreach(var edoTaskId in edoTaskIds)
			{
				var edoTask = _edoTaskRepository.GetFirstOrDefault(uow, x => x.Id == edoTaskId);
				await TryRegisterExceptionProblem(uow, edoTask, exceptions, cancellationToken);
			}

			return true;
		}

		private (MassTransitExceptionInfo ExceptionInfo, EdoTaskProblemExceptionSource Source) TryGetExceptionSource(
			MassTransitExceptionInfo exception,
			IEnumerable<EdoTaskProblemExceptionSource> sources)
		{
			var currentException = exception;

			while(currentException != null)
			{
				var sourceName = exception.ExceptionType;
				var source = sources.SingleOrDefault(x => x.Name == sourceName);

				if(source != null)
				{
					return (currentException, source);
				}
				
				currentException = currentException.InnerException;
			}
			
			return (null, null);
		}
	}
}
