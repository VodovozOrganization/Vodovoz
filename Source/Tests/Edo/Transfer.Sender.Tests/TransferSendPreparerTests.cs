using Edo.Common;
using Edo.Problems;
using Edo.Problems.Custom;
using Edo.Problems.Exception;
using Edo.Transfer;
using Edo.Transfer.Sender;
using EdoNotifications.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate;
using Notifications.Infrastructure;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.NHibernate.Extensions;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Xunit;

namespace Transfer.Sender.Tests
{
	/// <summary>
	/// Проверяет, что подготовка отправки трансфера останавливается при ошибке и не продолжает работу с закрытым UoW.
	/// </summary>
	public class TransferSendPreparerTests
	{
		private const int _taskId = 100;

		private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
		private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
		private readonly ITransferTaskRepository _transferTaskRepository = Substitute.For<ITransferTaskRepository>();
		private readonly TestProblemRegistrar _registrar;
		private readonly TransferSendPreparer _preparer;

		public TransferSendPreparerTests()
		{
			var task = new TransferEdoTask
			{
				Id = _taskId,
				TransferStatus = EdoTransferTaskStage.PreparingToSend,
				Problems = new ObservableList<EdoTaskProblem>()
			};

			_uow.Session.GetAsync<TransferEdoTask>(Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(task);

			var problemUow = Substitute.For<IUnitOfWork>();
			problemUow.GetById<EdoTask>(_taskId).Returns(task);

			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			uowFactory.CreateWithoutRoot(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(problemUow);

			_registrar = new TestProblemRegistrar(_uow, uowFactory);
			_preparer = new TransferSendPreparer(
				Substitute.For<ILogger<TransferSendPreparer>>(),
				_uow,
				_transferTaskRepository,
				Substitute.For<ITransferOrderHeaderPreparer>(),
				Substitute.For<ITrueMarkCodeRepository>(),
				_registrar,
				_publishEndpoint);
		}

		private void CreateTransferOrderThrows(Exception exception) =>
			_transferTaskRepository
				.GetAllCodesForTransferTaskAsync(_uow, Arg.Any<TransferEdoTask>(), Arg.Any<CancellationToken>())
				.ThrowsAsync(exception);

		private async Task AssertNothingContinued()
		{
			await _uow.DidNotReceive().SaveAsync(Arg.Any<object>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
			await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
			await _publishEndpoint.ReceivedWithAnyArgs(0).Publish<object>(default, default);
		}

		/// <summary>
		/// Ошибка массовой вставки идет общим путем: источника проблемы для нее нет, поэтому она проходит до верха без сохранения, события и коммита.
		/// </summary>
		[Fact]
		public async Task PrepareSendAsync_WhenBulkSqlInsertFailed_RethrowsWithoutContinuing()
		{
			var failure = new BulkSqlInsertException("Ошибка массовой вставки");
			CreateTransferOrderThrows(failure);

			var exception = await Assert.ThrowsAsync<BulkSqlInsertException>(() => _preparer.PrepareSendAsync(_taskId, CancellationToken.None));

			Assert.Same(failure, exception);
			Assert.Equal(0, _registrar.ProblemsCreated);
			await AssertNothingContinued();
		}

		/// <summary>
		/// Проблема зарегистрирована и UoW закрыт: обработка останавливается, сохранения, события и коммита нет.
		/// </summary>
		[Fact]
		public async Task PrepareSendAsync_WhenProblemRegistered_StopsWithoutContinuing()
		{
			CreateTransferOrderThrows(new InvalidOperationException("Известная проблема"));
			_registrar.RegisterAnyException = true;

			await _preparer.PrepareSendAsync(_taskId, CancellationToken.None);

			Assert.Equal(1, _registrar.ProblemsCreated);
			_uow.Received().Dispose();
			await AssertNothingContinued();
		}

		/// <summary>
		/// Ошибка без зарегистрированного источника проблемы пробрасывается, как и раньше.
		/// </summary>
		[Fact]
		public async Task PrepareSendAsync_WhenProblemNotRegistered_Rethrows()
		{
			CreateTransferOrderThrows(new InvalidOperationException("Неизвестная ошибка"));

			await Assert.ThrowsAsync<InvalidOperationException>(() => _preparer.PrepareSendAsync(_taskId, CancellationToken.None));

			await AssertNothingContinued();
		}

		/// <summary>
		/// Настоящий регистратор, у которого подменён только поиск источника проблемы.
		/// </summary>
		private class TestProblemRegistrar : EdoProblemRegistrar
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

		private class TestProblemSource : EdoTaskProblemExceptionSource
		{
			public override string Name => "TestProblem";
			public override string Description => "Тестовая проблема";
			public override string Recommendation => "Нет";
			public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
		}
	}
}
