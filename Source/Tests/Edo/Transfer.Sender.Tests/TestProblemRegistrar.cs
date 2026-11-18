using Edo.Common;
using Edo.Problems;
using Edo.Problems.Custom;
using Edo.Problems.Exception;
using Edo.Transfer;
using Edo.Transfer.Sender;
using EdoNotifications.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notifications.Infrastructure;
using NSubstitute;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Transfer.Sender.Tests
{
	/// <summary>
	/// Настоящий регистратор, у которого подменён только поиск источника проблемы.
	/// </summary>
	internal class TestProblemRegistrar : EdoProblemRegistrar
	{
		public TestProblemRegistrar(IUnitOfWork taskUow, IUnitOfWorkFactory uowFactory)
			: base(
				taskUow,
				uowFactory,
				(EdoTaskCustomSourcesPersister)RuntimeHelpers.GetUninitializedObject(typeof(EdoTaskCustomSourcesPersister)),
				(EdoTaskExceptionSourcesPersister)RuntimeHelpers.GetUninitializedObject(typeof(EdoTaskExceptionSourcesPersister)),
				Substitute.For<INotificationPublisher<EdoNotificationMessage>>())
		{
		}

		public bool RegisterAnyException { get; set; }

		public int ProblemsCreated { get; private set; }

		protected override EdoTaskProblemExceptionSource GetExceptionSource(Exception exception) =>
			RegisterAnyException ? new TestProblemSource() : null;

		protected override Task<EdoTaskProblem> GetExceptionProblemAndActivate(
			IUnitOfWork uow,
			EdoTask task,
			string exceptionMessage,
			EdoTaskProblemExceptionSource source,
			CancellationToken cancellationToken,
			bool needSave = false)
		{
			ProblemsCreated++;

			var problem = ExceptionEdoTaskProblem.Create(source.Name, task, exceptionMessage);
			problem.TaskItems = new ObservableList<EdoTaskItem>();
			problem.CustomItems = new ObservableList<EdoProblemCustomItem>();

			return Task.FromResult<EdoTaskProblem>(problem);
		}
	}
}
