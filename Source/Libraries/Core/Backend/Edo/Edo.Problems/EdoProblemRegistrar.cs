using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Common;
using Edo.Problems.Custom;
using Edo.Problems.Exception;
using Edo.Problems.Validation;
using EdoNotifications.Contracts;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems
{
	public class EdoProblemRegistrar : EdoProblemRegistrarBase
	{
		private readonly IUnitOfWork _taskUow;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOutboxNotificationPublisher<EdoNotificationMessage> _notificationPublisher;

		public EdoProblemRegistrar(
			IUnitOfWork taskUow,
			IUnitOfWorkFactory uowFactory,
			EdoTaskCustomSourcesPersister customSourcesPersister,
			EdoTaskExceptionSourcesPersister exceptionSourcesPersister,
			IOutboxNotificationPublisher<EdoNotificationMessage> notificationPublisher
			) : base(customSourcesPersister, exceptionSourcesPersister)
		{
			_taskUow = taskUow ?? throw new ArgumentNullException(nameof(taskUow));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
		}

		public async Task RegisterCustomProblem<TCustomSource>(
			EdoTask edoTask,
			CancellationToken cancellationToken,
			string customMessage = null,
			bool disposeTaskUow = true
			)
			where TCustomSource : EdoTaskProblemCustomSource
		{
			await RegisterCustomProblem<TCustomSource>(
				edoTask.Id,
				new List<EdoTaskItem>(),
				cancellationToken,
				customMessage,
				disposeTaskUow);
		}

		public async Task RegisterCustomProblem<TCustomSource>(
			int edoTaskId,
			CancellationToken cancellationToken,
			string customMessage = null,
			bool disposeTaskUow = true
			)
			where TCustomSource : EdoTaskProblemCustomSource
		{
			await RegisterCustomProblem<TCustomSource>(
				edoTaskId,
				new List<EdoTaskItem>(),
				cancellationToken,
				customMessage,
				disposeTaskUow);
		}

		public virtual async Task RegisterCustomProblem<TCustomSource>(
			int edoTaskId,
			IEnumerable<EdoTaskItem> affectedTaskItems,
			CancellationToken cancellationToken,
			string customMessage = null,
			bool disposeTaskUow = true
			)
			where TCustomSource : EdoTaskProblemCustomSource
		{
			// проблема открываеся специально в отдельном UoW
			// чтобы вместе с проблемой не сохранить все изменения в задаче
			// а UoW задачи обязательно закрывается с откатом транзакции
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var task = uow.GetById<EdoTask>(edoTaskId);
				var source = CustomSourcesPersister.GetCustomSource<TCustomSource>();
				var problem = task.Problems.FirstOrDefault(x => x.SourceName == source.Name)
					?? CustomEdoTaskProblem.Create(source.Name, task, customMessage);

				problem.CreationTime = DateTime.Now;
				problem.State = TaskProblemState.Active;
				
				problem.TaskItems.Clear();
				foreach(var taskItem in affectedTaskItems)
				{
					problem.TaskItems.Add(taskItem);
				}

				task.Status = source.Importance == EdoProblemImportance.Problem
					? EdoTaskStatus.Problem
					: EdoTaskStatus.Waiting;

				await uow.SaveAsync(problem, cancellationToken: cancellationToken);
				await uow.SaveAsync(task, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);
			}

			if(disposeTaskUow)
			{
				_taskUow.Dispose();
			}
		}

		/// <summary>
		/// Регистрирует проблему в задаче не завершая выполнение ее обработки
		/// для того чтобы запись о наличии такой проблемы осталась, 
		/// даже если она будет решена в процессе дальнейшей обработки задачи
		/// </summary>
		/// <typeparam name="TCustomSource"></typeparam>
		/// <param name="edoTask">ЭДО задача по которой региструется проблема</param>
		/// <param name="customMessage">Произвольное сообщение к проблеме</param>
		/// <returns></returns>
		public async Task OptionalRegisterCustomProblem<TCustomSource>(
			EdoTask edoTask,
			CancellationToken cancellationToken,
			bool solved = false,
			string customMessage = null
			)
			where TCustomSource : EdoTaskProblemCustomSource
		{
			await OptionalRegisterCustomProblem<TCustomSource>(
				edoTask, 
				new List<EdoTaskItem>(), 
				cancellationToken, 
				solved, 
				customMessage
			);
		}

		/// <summary>
		/// Регистрирует проблему в задаче не завершая выполнение ее обработки
		/// для того чтобы запись о наличии такой проблемы осталась, 
		/// даже если она будет решена в процессе дальнейшей обработки задачи
		/// </summary>
		/// <typeparam name="TCustomSource"></typeparam>
		/// <param name="edoTask">ЭДО задача по которой региструется проблема</param>
		/// <param name="affectedTaskItems">Строки задачи связанные с проблемой</param>
		/// <param name="customMessage">Произвольное сообщение к проблеме</param>
		/// <returns></returns>
		public async Task OptionalRegisterCustomProblem<TCustomSource>(
			EdoTask edoTask,
			IEnumerable<EdoTaskItem> affectedTaskItems,
			CancellationToken cancellationToken,
			bool solved = false,
			string customMessage = null
			)
			where TCustomSource : EdoTaskProblemCustomSource
		{
			var task = _taskUow.GetById<EdoTask>(edoTask.Id);
			var source = CustomSourcesPersister.GetCustomSource<TCustomSource>();
			var problem = task.Problems.FirstOrDefault(x => x.SourceName == source.Name)
				?? CustomEdoTaskProblem.Create(source.Name, task, customMessage);

			problem.CreationTime = DateTime.Now;
			problem.State = solved ? TaskProblemState.Solved : TaskProblemState.Active;

			problem.TaskItems.Clear();
			foreach(var taskItem in affectedTaskItems)
			{
				problem.TaskItems.Add(taskItem);
			}

			task.Problems.Add(problem);

			if(!solved)
			{
				task.Status = source.Importance == EdoProblemImportance.Problem
					? EdoTaskStatus.Problem
					: EdoTaskStatus.Waiting;
			}

			await _taskUow.SaveAsync(problem, cancellationToken: cancellationToken);
			await _taskUow.SaveAsync(task, cancellationToken: cancellationToken);
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
				var task = uow.GetById<EdoTask>(edoTask.Id);
				var source = GetExceptionSource(exception);
				if(source == null)
				{
					_taskUow.Dispose();
					return false;
				}

				var problem = await GetExceptionProblemAndActivate(uow, task, exception.Message, source, cancellationToken, true);

				var solvedValidationSources = new HashSet<string>(edoTask.Problems
					.OfType<ValidationEdoTaskProblem>()
					.Where(x => x.State == TaskProblemState.Solved)
					.Select(x => x.SourceName));
				foreach(var validationProblem in task.Problems.OfType<ValidationEdoTaskProblem>()
					.Where(x => x.State != TaskProblemState.Solved && solvedValidationSources.Contains(x.SourceName)))
				{
					validationProblem.State = TaskProblemState.Solved;
					await uow.SaveAsync(validationProblem, cancellationToken: cancellationToken);
				}

				// Удаляем старые строки задачи, которые не относятся к обновленной проблеме
				// и добавляем новые строки задачи, которые относятся к проблеме
				foreach(var existsItem in problem.TaskItems.ToList())
				{
					if(affectedTaskItems.Any(x => x.Id == existsItem.Id))
					{
						continue;
					}
					problem.TaskItems.Remove(existsItem);
				}
				foreach(var newItem in affectedTaskItems)
				{
					if(problem.TaskItems.Any(x => x.Id == newItem.Id))
					{
						continue;
					}
					problem.TaskItems.Add(newItem);
				}

				foreach(var existsItem in problem.CustomItems.ToList())
				{
					problem.CustomItems.Remove(existsItem);
					await uow.DeleteAsync(existsItem, cancellationToken: cancellationToken);
				}
				foreach(var customItem in customItems)
				{
					customItem.Problem = problem;
					problem.CustomItems.Add(customItem);
					await uow.SaveAsync(customItem, cancellationToken: cancellationToken);
				}

				task.UpdateStatusByEdoProblemImportance(source.Importance);

				await uow.SaveAsync(problem, cancellationToken: cancellationToken);
				await uow.SaveAsync(task, cancellationToken: cancellationToken);
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

			var uow = _taskUow;
			var invalidResults = validationResults
				.Where(x => !x.IsValid)
				.ToArray();
			var isAllValid = !invalidResults.Any();
			if(!isAllValid)
			{
				uow = _uowFactory.CreateWithoutRoot();
				uow.OpenTransaction();
				edoTask = await uow.Session.GetAsync<EdoTask>(edoTask.Id, cancellationToken);
			}

			var edoProblems = edoTask.Problems;
			var problemsWithoutValidationResults = GetProblemsWithoutValidationResults(edoProblems, validationResults);
			foreach(var validationResult in validationResults)
			{
				var problem = edoProblems.FirstOrDefault(x => x.SourceName == validationResult.Validator.Name);

				switch (problem)
				{
					case null when validationResult.IsValid:
						continue;
					case null:
						problem = new ValidationEdoTaskProblem
						{
							SourceName = validationResult.Validator.Name,
							EdoTask = edoTask,
						};
						break;
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

				if(validationResult.Notification != null)
				{
					await _notificationPublisher.TryPublishAsync(uow, validationResult.Notification, cancellationToken);
				}
			}

			if(isAllValid)
			{
				edoTask.Status = EdoTaskStatus.InProgress;
				
				//оставшиеся проблемы помечаем решенными т.к. валидация прошла успешно и результатов валидации нет
				foreach(var edoTaskProblem in problemsWithoutValidationResults)
				{
					edoTaskProblem.State = TaskProblemState.Solved;
					await uow.SaveAsync(edoTaskProblem, cancellationToken: cancellationToken);
				}
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

		private static IReadOnlyCollection<EdoTaskProblem> GetProblemsWithoutValidationResults(
			IEnumerable<EdoTaskProblem> edoProblems,
			IEnumerable<EdoValidationResult> validationResults)
		{
			var validatedSources = new HashSet<string>(validationResults.Select(x => x.Validator.Name));
			return edoProblems.Where(x => !validatedSources.Contains(x.SourceName)).ToList();
		}

		public void SolveCustomProblem<TCustomSource>(EdoTask edoTask)
			where TCustomSource : EdoTaskProblemCustomSource
		{
			var source = CustomSourcesPersister.GetCustomSource<TCustomSource>();
			SolveProblem(edoTask, source.Name);
		}

		/// <summary>
		/// Закрывает проблему по имени источника.
		/// </summary>
		public void SolveCustomProblem(EdoTask edoTask, string sourceName)
		{
			SolveProblem(edoTask, sourceName);
		}

		public void SolveExceptionProblem<TExceptionSource>(EdoTask edoTask)
			where TExceptionSource : EdoTaskProblemExceptionSource
		{
			var source = ExceptionSourcesPersister.GetExceptionSource<TExceptionSource>();
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
