using Edo.Problems.Custom;
using Edo.Problems.Exception;
using Edo.Problems.Validation;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems
{
	public class EdoProblemRegistrar
	{
		private readonly IUnitOfWork _taskUow;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly EdoTaskCustomSourcesPersister _customSourcesPersister;
		private readonly EdoTaskExceptionSourcesPersister _exceptionSourcesPersister;

		public EdoProblemRegistrar(
			IUnitOfWork taskUow,
			IUnitOfWorkFactory uowFactory,
			EdoTaskCustomSourcesPersister customSourcesPersister,
			EdoTaskExceptionSourcesPersister exceptionSourcesPersister
			)
		{
			_taskUow = taskUow ?? throw new ArgumentNullException(nameof(taskUow));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_customSourcesPersister = customSourcesPersister ?? throw new ArgumentNullException(nameof(customSourcesPersister));
			_exceptionSourcesPersister = exceptionSourcesPersister ?? throw new ArgumentNullException(nameof(exceptionSourcesPersister));
		}

		public async Task RegisterCustomProblem<TCustomSource>(
			EdoTask edoTask,
			CancellationToken cancellationToken,
			string customMessage = null
			)
			where TCustomSource : EdoTaskProblemCustomSource
		{
			await RegisterCustomProblem<TCustomSource>(edoTask, new List<EdoTaskItem>(), cancellationToken, customMessage);
		}

		public async Task RegisterCustomProblem<TCustomSource>(
			EdoTask edoTask,
			IEnumerable<EdoTaskItem> affectedTaskItems,
			CancellationToken cancellationToken,
			string customMessage = null
			)
			where TCustomSource : EdoTaskProblemCustomSource
		{
			// проблема открываеся специально в отдельном UoW
			// чтобы вместе с проблемой не сохранить все изменения в задаче
			// а UoW задачи обязательно закрывается с откатом транзакции
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var source = _customSourcesPersister.GetCustomSource<TCustomSource>();
				var problem = edoTask.Problems.FirstOrDefault(x => x.SourceName == source.Name);
				if(problem == null)
				{
					problem = new CustomEdoTaskProblem
					{
						SourceName = source.Name,
						EdoTask = edoTask,
						CustomMessage = customMessage
					};
				}

				problem.CreationTime = DateTime.Now;
				problem.State = TaskProblemState.Active;

				problem.TaskItems.Clear();
				foreach(var taskItem in affectedTaskItems)
				{
					problem.TaskItems.Add(taskItem);
				}

				if(source.Importance == EdoProblemImportance.Problem)
				{
					edoTask.Status = EdoTaskStatus.Problem;
				}
				else
				{
					edoTask.Status = EdoTaskStatus.Waiting;
				}

				await uow.SaveAsync(problem, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);
			}
			_taskUow.Dispose();
		}

		public async Task<bool> TryRegisterExceptionProblem(
			EdoTask edoTask,
			EdoProblemException exception,
			CancellationToken cancellationToken
			)
		{
			return await TryRegisterExceptionProblem(
				edoTask, 
				exception.InnerException, 
				exception.ProblemItems,
				exception.CustomItems,
				cancellationToken
			);
		}

		public async Task<bool> TryRegisterExceptionProblem(
			EdoTask edoTask,
			System.Exception exception,
			CancellationToken cancellationToken
			)
		{
			return await TryRegisterExceptionProblem(
				edoTask, 
				exception, 
				new List<EdoTaskItem>(), 
				new List<EdoProblemCustomItem>(),
				cancellationToken
			);
		}

		public async Task<bool> TryRegisterExceptionProblem(
			EdoTask edoTask,
			System.Exception exception,
			IEnumerable<EdoTaskItem> affectedTaskItems,
			IEnumerable<EdoProblemCustomItem> customItems,
			CancellationToken cancellationToken
			)
		{
			// проблема открываеся специально в отдельном UoW
			// чтобы вместе с проблемой не сохранить все изменения в задаче
			// а UoW задачи обязательно закрывается с откатом транзакции

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var sourceName = exception.GetType().Name;
				var sources = _exceptionSourcesPersister.GetEdoProblemExceptionSources();
				var source = sources.SingleOrDefault(x => x.Name == sourceName);
				if(source == null)
				{
					_taskUow.Dispose();
					return false;
				}

				var problem = edoTask.Problems.FirstOrDefault(x => x.SourceName == sourceName);
				if(problem == null)
				{
					problem = new ExceptionEdoTaskProblem
					{
						SourceName = sourceName,
						EdoTask = edoTask,
						ExceptionMessage = exception.Message
					};
				}

				problem.CreationTime = DateTime.Now;
				problem.State = TaskProblemState.Active;

				problem.TaskItems.Clear();
				foreach(var taskItem in affectedTaskItems)
				{
					problem.TaskItems.Add(taskItem);
				}

				problem.CustomItems.Clear();
				foreach(var customItem in customItems)
				{
					customItem.Problem = problem;
					problem.CustomItems.Add(customItem);
				}

				if(source.Importance == EdoProblemImportance.Problem)
				{
					edoTask.Status = EdoTaskStatus.Problem;
				}
				else
				{
					edoTask.Status = EdoTaskStatus.Waiting;
				}

				await uow.SaveAsync(problem, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);

				_taskUow.Dispose();
				return true;
			}
		}

		internal async Task UpdateValidationProblems(
			EdoTask edoTask,
			IEnumerable<EdoValidationResult> validationResults,
			CancellationToken cancellationToken
			)
		{
			// Если есть хоть одна проблема, то сохраняем задачу и проблемы в отдельном UoW
			// После сохранения проблем, коммитим uow
			// И обязательно закрываем uow задачи

			// А если проблем нет, то все прошлые проблемы разрешившиеся в текущем вызове валидации
			// закрываем в текущем UoW задачи, будут сохранены в момент коммита изменений по задаче

			IUnitOfWork uow = _taskUow;
			var invalidResults = validationResults.Where(x => !x.IsValid);
			var isAllValid = !invalidResults.Any();
			if(!isAllValid)
			{
				uow = _uowFactory.CreateWithoutRoot();
				uow.OpenTransaction();
				edoTask = await uow.Session.GetAsync<EdoTask>(edoTask.Id, cancellationToken);
			}

			foreach(var validationResult in validationResults)
			{
				var problem = edoTask.Problems.FirstOrDefault(x => x.SourceName == validationResult.Validator.Name);
				if(problem == null && validationResult.IsValid)
				{
					continue;
				}

				if(problem == null)
				{
					problem = new ValidationEdoTaskProblem
					{
						SourceName = validationResult.Validator.Name,
						EdoTask = edoTask,
					};
				}

				if(validationResult.IsValid)
				{
					problem.State = TaskProblemState.Solved;
				}
				else
				{
					problem.State = TaskProblemState.Active;
					problem.TaskItems.Clear();
					foreach(var problemItem in validationResult.ProblemItems)
					{
						problem.TaskItems.Add(problemItem);
					}
				}

				await uow.SaveAsync(problem, cancellationToken: cancellationToken);
			}

			if(isAllValid)
			{
				edoTask.Status = EdoTaskStatus.InProgress;
			}
			else
			{
				var hasProblemImportance = invalidResults
					.Any(x => x.Validator.Importance == EdoProblemImportance.Problem);

				if(hasProblemImportance)
				{
					edoTask.Status = EdoTaskStatus.Problem;
				}
				else
				{
					edoTask.Status = EdoTaskStatus.Waiting;
				}

				await uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);
				uow.Dispose();

				// обязательно закрываем UoW задачи если проблемы сохраняются в отдельном UoW
				_taskUow.Dispose();
			}

		}

		public void SolveCustomProblem<TCustomSource>(EdoTask edoTask)
			where TCustomSource : EdoTaskProblemCustomSource
		{
			var source = _customSourcesPersister.GetCustomSource<TCustomSource>();
			SolveProblem(edoTask, source.Name);
		}

		public void SolveExceptionProblem<TExceptionSource>(EdoTask edoTask)
			where TExceptionSource : EdoTaskProblemExceptionSource
		{
			var source = _exceptionSourcesPersister.GetExceptionSource<TExceptionSource>();
			SolveProblem(edoTask, source.Name);
		}

		private void SolveProblem(EdoTask edoTask, string sourceName)
		{
			// проблема закрывается специально в UoW задачи
			// чтобы все изменения по успешной задаче записались в единой транзакции

			var foundProblem = edoTask.Problems.FirstOrDefault(x => x.SourceName == sourceName);
			if(foundProblem == null)
			{
				return;
			}

			foundProblem.State = TaskProblemState.Solved;
		}
	}
}
