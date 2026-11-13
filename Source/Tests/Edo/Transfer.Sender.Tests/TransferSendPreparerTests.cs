using Edo.Common;
using Edo.Transfer;
using Edo.Transfer.Sender;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.NHibernate.Extensions;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Settings.Edo;
using Vodovoz.Settings.Nomenclature;
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
				new TransferUpdDocumentBuilder(
					Substitute.For<ITrueMarkCodeRepository>(),
					Substitute.For<IEdoTransferSettings>(),
					Substitute.For<INomenclatureSettings>()),
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
	}
}
