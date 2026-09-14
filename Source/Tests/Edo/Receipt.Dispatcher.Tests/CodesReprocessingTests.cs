using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Edo.Admin;
using Edo.Common;
using Edo.Common.Services;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Problems.Custom;
using Edo.Problems.Custom.Sources;
using Edo.Problems.Exception;
using Edo.Problems.Validation;
using Edo.Receipt.Dispatcher;
using Edo.Tender;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using NSubstitute;
using NSubstitute.Extensions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using TrueMark.Codes.Pool;
using TrueMark.Contracts;
using TrueMark.Contracts.Responses;
using TrueMark.Library;
using TrueMarkApi.Client;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Data.Repositories.Goods;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Settings.Edo;
using Vodovoz.Settings.Organizations;
using Xunit;

namespace Receipt.Dispatcher.Tests
{
	public class CodesReprocessingTests
	{
		private const string Inn = "0000000000";
		private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
		private readonly IBus _bus = Substitute.For<IBus>();
		private readonly ITrueMarkApiClient _api = Substitute.For<ITrueMarkApiClient>();
		private readonly TrueMarkTaskCodesValidator _validator;
		private readonly ForOwnNeedsReceiptEdoTaskHandler _receiptHandler;
		private readonly TenderEdoTaskHandler _tenderHandler;
		private readonly ProductInstanceStatus _codeStatus;
		private readonly TrueMarkWaterIdentificationCode _code = new TrueMarkWaterIdentificationCode
		{
			Id = 100, Gtin = "04600000000001", SerialNumber = "code", RawCode = "code"
		};

		public CodesReprocessingTests()
		{
			ConfigurePreload<TrueMarkProductCode>();
			ConfigurePreload<EdoTaskItem>();
			var repository = Substitute.For<IEdoRepository>();
			var nomenclatures = Substitute.For<INomenclatureRepository>();
			repository.GetEdoOrganizationsAsync(Arg.Any<CancellationToken>())
				.Returns(new[] { new OrganizationEntity { INN = Inn } });
			nomenclatures.GetGtinsAsync(Arg.Any<CancellationToken>())
				.Returns(new[] { new GtinEntity { GtinNumber = _code.Gtin } });
			_codeStatus = new ProductInstanceStatus
			{
				IdentificationCode = _code.IdentificationCode, Gtin = _code.Gtin,
				OwnerInn = Inn, Status = ProductInstanceStatusEnum.Introduced,
				ExpirationDate = DateTime.Today.AddDays(-1)
			};
			_api.GetProductInstanceInfoAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
				.Returns(new ProductInstancesInfoResponse { InstanceStatuses = new[] { _codeStatus } });
			_validator = new TrueMarkTaskCodesValidator(repository, nomenclatures, _api);
			var factory = Substitute.For<IUnitOfWorkFactory>();
			var registrar = new EdoProblemRegistrar(_uow, factory,
				new EdoTaskCustomSourcesPersister(factory, new EdoTaskProblemCustomSource[]
				{
					new IndustryRequisiteMissingOrganizationToken(), new IndustryRequisiteRegualtoryDocumentIsMissing(),
					new IndustryRequisiteCheckApiError()
				}),
				new EdoTaskExceptionSourcesPersister(factory, Array.Empty<EdoTaskProblemExceptionSource>()));
			var taskValidator = new EdoTaskValidator(Substitute.For<ILogger<EdoTaskValidator>>(),
				new EdoTaskValidatorsProvider(new EdoTaskValidatorsPersister(factory, Array.Empty<IEdoTaskValidator>())),
				Substitute.For<IServiceProvider>(), registrar);
			var providerFactory = new EdoTaskItemTrueMarkStatusProviderFactory(_api);
			var waterCodeService = Substitute.For<ITrueMarkWaterCodeService>();
			var cancellation = new EdoCancellationService(Substitute.For<ILogger<EdoCancellationService>>(),
				_uow, Substitute.For<IEdoCancellationValidator>(), registrar, waterCodeService, _bus);
			var receiptSettings = Substitute.For<IEdoReceiptSettings>();
			receiptSettings.MaxCodesInReceiptCount.Returns(1000);
			_receiptHandler = new ForOwnNeedsReceiptEdoTaskHandler(
				Substitute.For<ILogger<ForOwnNeedsReceiptEdoTaskHandler>>(), _uow, taskValidator, registrar,
				providerFactory, new TransferRequestCreator(repository), repository, receiptSettings, _validator,
				new ReceiptTrueMarkCodesPool(_uow), Substitute.For<ITrueMarkCodesPoolCodeProvider>(),
				new Tag1260Checker(Substitute.For<IHttpClientFactory>()), Substitute.For<ITrueMarkCodeRepository>(),
				Substitute.For<IGenericRepository<TrueMarkProductCode>>(), Substitute.For<IEdoOrderContactProvider>(),
				Substitute.For<ISaveCodesService>(), Substitute.For<IOrganizationSettings>(), _bus, cancellation, waterCodeService);
			_tenderHandler = new TenderEdoTaskHandler(Substitute.For<ILogger<TenderEdoTaskHandler>>(),
				_uow, taskValidator, providerFactory, registrar, Substitute.For<ITrueMarkCodeRepository>(),
				_validator, new TransferRequestCreator(repository), _bus, cancellation);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task InvalidCode_SavesThenRestartsSameTask(bool tender)
		{
			var task = CreateTask(tender);
			var request = task.FormalEdoRequest;
			var document = (task as ReceiptEdoTask)?.FiscalDocuments.Single();
			var committed = false;
			var publishedAfterCommit = false;
			_uow.CommitAsync(Arg.Any<CancellationToken>()).Returns(_ => { committed = true; return Task.CompletedTask; });
			_bus.Publish(Arg.Any<ReceiptTaskCreatedEvent>(), Arg.Any<CancellationToken>())
				.Returns(_ => { publishedAfterCommit = committed; return Task.CompletedTask; });
			_bus.Publish(Arg.Any<TenderTaskCreatedEvent>(), Arg.Any<CancellationToken>())
				.Returns(_ => { publishedAfterCommit = committed; return Task.CompletedTask; });

			await HandleTransfer(task);

			AssertNew(task);
			Assert.True(publishedAfterCommit);
			Assert.Same(request, task.FormalEdoRequest);
			Assert.Same(_code, task.Items.Single().ProductCode.ResultCode);
			Assert.Empty(task.Problems);
			await _uow.Received(1).SaveAsync(task, cancellationToken: default);
			await _uow.Received(1).CommitAsync(default);
			if(tender)
			{
				await _bus.Received(1).Publish(Arg.Is<TenderTaskCreatedEvent>(x => x.TenderEdoTaskId == task.Id), default);
			}
			else
			{
				Assert.Same(document, ((ReceiptEdoTask)task).FiscalDocuments.Single());
				await _bus.Received(1).Publish(Arg.Is<ReceiptTaskCreatedEvent>(x => x.ReceiptEdoTaskId == task.Id), default);
			}
			await _bus.DidNotReceive().Publish(Arg.Any<ReceiptReadyToSendEvent>(), Arg.Any<CancellationToken>());
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task PublishFailure_LeavesNewTaskForResendWorker(bool tender)
		{
			var task = CreateTask(tender);
			_bus.Publish(Arg.Any<ReceiptTaskCreatedEvent>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromException(new InvalidOperationException("broker")));
			_bus.Publish(Arg.Any<TenderTaskCreatedEvent>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromException(new InvalidOperationException("broker")));
			await HandleTransfer(task);
			AssertNew(task);
			Assert.Empty(task.Problems);
			await _uow.Received(1).CommitAsync(default);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task CommitFailure_DoesNotPublishRestart(bool tender)
		{
			var task = CreateTask(tender);
			_uow.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.FromException(new InvalidOperationException("commit")));
			await Assert.ThrowsAsync<InvalidOperationException>(() => HandleTransfer(task));
			Assert.Empty(_bus.ReceivedCalls());
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ApiFailure_DoesNotResetTaskOrPublish(bool tender)
		{
			var task = CreateTask(tender);
			_api.GetProductInstanceInfoAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromException<ProductInstancesInfoResponse>(new InvalidOperationException("api")));
			await Assert.ThrowsAsync<InvalidOperationException>(() => HandleTransfer(task));
			Assert.Equal(EdoTaskStatus.InProgress, task.Status);
			Assert.Empty(_bus.ReceivedCalls());
		}

		[Theory]
		[InlineData(false, EdoTaskStatus.Completed)]
		[InlineData(false, EdoTaskStatus.Cancelled)]
		[InlineData(false, EdoTaskStatus.InCancellation)]
		[InlineData(true, EdoTaskStatus.Completed)]
		[InlineData(true, EdoTaskStatus.Cancelled)]
		[InlineData(true, EdoTaskStatus.InCancellation)]
		public async Task TerminalTask_IsNotReset(bool tender, EdoTaskStatus status)
		{
			var task = CreateTask(tender);
			task.Status = status;
			await Validate(task);
			await HandleTransfer(task);
			Assert.Equal(status, task.Status);
			Assert.Empty(_bus.ReceivedCalls());
			await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
		}

		[Theory]
		[InlineData(EdoReceiptStatus.New)]
		[InlineData(EdoReceiptStatus.SavedToPool)]
		[InlineData(EdoReceiptStatus.Sending)]
		[InlineData(EdoReceiptStatus.Sent)]
		[InlineData(EdoReceiptStatus.Completed)]
		public async Task Receipt_OtherStagesAreNotReset(EdoReceiptStatus stage)
		{
			var task = (ReceiptEdoTask)CreateTask(false);
			task.ReceiptStatus = stage;
			await Validate(task);
			await HandleTransfer(task);
			Assert.Equal(stage, task.ReceiptStatus);
			Assert.Equal(EdoTaskStatus.InProgress, task.Status);
			Assert.Empty(_bus.ReceivedCalls());
		}

		[Theory]
		[InlineData(TenderEdoTaskStage.New)]
		[InlineData(TenderEdoTaskStage.Sending)]
		[InlineData(TenderEdoTaskStage.ManualUploaded)]
		public async Task Tender_OtherStagesAreNotReset(TenderEdoTaskStage stage)
		{
			var task = (TenderEdoTask)CreateTask(true);
			task.Stage = stage;
			await Validate(task);
			await HandleTransfer(task);
			Assert.Equal(stage, task.Stage);
			Assert.Equal(EdoTaskStatus.InProgress, task.Status);
			Assert.Empty(_bus.ReceivedCalls());
		}

		[Theory]
		[InlineData(FiscalDocumentStage.Sent)]
		[InlineData(FiscalDocumentStage.Completed)]
		public async Task Receipt_WithAlreadySentDocumentIsNotReset(FiscalDocumentStage stage)
		{
			var task = (ReceiptEdoTask)CreateTask(false);
			task.FiscalDocuments.Add(new EdoFiscalDocument { Stage = stage });
			await Validate(task);
			await HandleTransfer(task);
			Assert.Equal(EdoReceiptStatus.Transfering, task.ReceiptStatus);
			Assert.Equal(EdoTaskStatus.InProgress, task.Status);
			Assert.Empty(_bus.ReceivedCalls());
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ValidCodesContinueToSending(bool tender)
		{
			_codeStatus.ExpirationDate = DateTime.Today.AddDays(1);
			var task = CreateTask(tender);
			await HandleTransfer(task);
			Assert.Equal(EdoTaskStatus.InProgress, task.Status);
			if(tender)
			{
				Assert.Equal(TenderEdoTaskStage.Sending, ((TenderEdoTask)task).Stage);
				Assert.Empty(_bus.ReceivedCalls());
			}
			else
			{
				Assert.Equal(EdoReceiptStatus.Sending, ((ReceiptEdoTask)task).ReceiptStatus);
				await _bus.Received(1).Publish(Arg.Is<ReceiptReadyToSendEvent>(x => x.ReceiptEdoTaskId == task.Id), default);
			}
			await _uow.Received(1).CommitAsync(default);
		}

		[Theory]
		[InlineData(DocumentEdoTaskStage.New)]
		[InlineData(DocumentEdoTaskStage.Transfering)]
		[InlineData(DocumentEdoTaskStage.Sending)]
		[InlineData(DocumentEdoTaskStage.Sent)]
		[InlineData(DocumentEdoTaskStage.Completed)]
		public async Task Upd_OnlyTransferStageIsReset(DocumentEdoTaskStage stage)
		{
			var receipt = CreateTask(false);
			var task = new DocumentEdoTask
			{
				Stage = stage, Status = EdoTaskStatus.InProgress, DocumentType = EdoDocumentType.UPD,
				FormalEdoRequest = receipt.FormalEdoRequest, Items = receipt.Items
			};
			await Validate(task);
			Assert.Equal(stage == DocumentEdoTaskStage.Transfering ? DocumentEdoTaskStage.New : stage, task.Stage);
			Assert.Equal(stage == DocumentEdoTaskStage.Transfering ? EdoTaskStatus.New : EdoTaskStatus.InProgress, task.Status);
		}

		[Theory]
		[InlineData(ReasonForLeaving.Resale)]
		[InlineData(ReasonForLeaving.Tender)]
		public async Task Receipt_ScannedCodesDoNotTriggerReplacement(ReasonForLeaving reason)
		{
			var task = (ReceiptEdoTask)CreateTask(false);
			task.FormalEdoRequest.Order.Client.ReasonForLeaving = reason;
			await Validate(task);
			Assert.Equal(EdoReceiptStatus.Transfering, task.ReceiptStatus);
			Assert.Equal(EdoTaskStatus.InProgress, task.Status);
		}

		private OrderEdoTask CreateTask(bool tender)
		{
			OrderEdoTask task = tender
				? new TenderEdoTask { Stage = TenderEdoTaskStage.Transfering }
				: new ReceiptEdoTask { ReceiptStatus = EdoReceiptStatus.Transfering };
			task.Id = 10;
			task.Status = EdoTaskStatus.InProgress;
			task.Problems = new ObservableList<EdoTaskProblem>();
			task.FormalEdoRequest = new ManualEdoRequest
			{
				Id = 20, Task = task, Order = new TestOrder(tender ? ReasonForLeaving.Tender : ReasonForLeaving.ForOwnNeeds)
			};
			task.Items.Add(new EdoTaskItem
			{
				CustomerEdoTask = task,
				ProductCode = new AutoTrueMarkProductCode { SourceCode = _code, ResultCode = _code, SourceCodeStatus = SourceProductCodeStatus.Accepted }
			});
			if(task is ReceiptEdoTask receipt)
			{
				receipt.FiscalDocuments.Add(new EdoFiscalDocument { Stage = FiscalDocumentStage.Preparing });
			}
			return task;
		}

		private Task<TrueMarkTaskValidationResult> Validate(OrderEdoTask task) =>
			_validator.ValidateAsync(task, new EdoTaskItemTrueMarkStatusProvider(task, _api), default);

		private void ConfigurePreload<T>() where T : class
		{
			var query = Substitute.For<IQueryOver<T, T>, ISupportSelectModeQueryOver<T, T>>();
			query.ReturnsForAll<IQueryOver<T, T>>(query);
			query.ListAsync(Arg.Any<CancellationToken>()).Returns(new List<T>());
			_uow.Session.QueryOver<T>().Returns(query);
		}

		private Task HandleTransfer(OrderEdoTask task)
		{
			if(task is ReceiptEdoTask receipt)
			{
				return _receiptHandler.HandleTransferComplete(receipt, default);
			}
			_uow.Session.GetAsync<TransferEdoRequestIteration>(30, default).Returns(new TransferEdoRequestIteration
			{
				Id = 30, OrderEdoTask = task, Status = TransferEdoRequestIterationStatus.Completed
			});
			return _tenderHandler.HandleTransfered(30, default);
		}

		private static void AssertNew(OrderEdoTask task)
		{
			Assert.Equal(EdoTaskStatus.New, task.Status);
			if(task is ReceiptEdoTask receipt)
			{
				Assert.Equal(EdoReceiptStatus.New, receipt.ReceiptStatus);
			}
			else
			{
				Assert.Equal(TenderEdoTaskStage.New, ((TenderEdoTask)task).Stage);
			}
		}

		private class TestOrder : OrderEntity
		{
			public TestOrder(ReasonForLeaving reason)
			{
				Client = new CounterpartyEntity { ReasonForLeaving = reason };
				Contract = new CounterpartyContractEntity { Organization = new OrganizationEntity { INN = Inn } };
			}
		}
	}
}
