using Edo.Admin;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Problems.Custom;
using Edo.Problems.Exception;
using Edo.Transport;
using EdoService.Library.Factories;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate;
using NSubstitute;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Project.Domain;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Taxcom.Docflow.Utility;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Errors.Edo;
using VodovozBusiness.Services.Edo;
using Xunit;
using IOrderRepository = Vodovoz.EntityRepositories.Orders.IOrderRepository;
using IOrganizationRepository = Vodovoz.Core.Data.Repositories.IOrganizationRepository;

namespace EdoServices.Tests
{
	public class EdoServiceTests
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IUnitOfWork _uow;
		private readonly IOrderRepository _orderRepository;
		private readonly IEdoRepository _edoRepository;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IGenericRepository<FormalEdoRequest> _edoRequestRepository;
		private readonly IGenericRepository<OrderEdoTask> _edoTaskRepository;
		private readonly IEdoRequestCreatedEventPublisher _edoRequestCreatedEventPublisher;
		private readonly ICounterpartyEdoAccountEntityController _counterpartyEdoAccountEntityController;
		private readonly IBus _bus;
		private readonly MessageService _messageService;
		private readonly EdoCancellationService _edoCancellationService;
		private readonly IEdoCancellationValidator _edoCancellationValidator;
		private readonly IPublishEndpoint _publishEndpoint;
		private readonly IUserService _userService;
		private readonly ITaxcomApiFactory _taxcomApiFactory;
		private readonly IEnumerable<IInformalEdoRequestFactory> _requestFactories;
		private readonly IManualEdoRequestFactory _manualEdoRequestFactory;
		private readonly EdoService.Library.EdoService _edoService;
		private readonly EdoProblemRegistrar _problemRegistrar;
		private readonly EdoTaskCustomSourcesPersister _customSourcesPersister;
		private readonly EdoTaskExceptionSourcesPersister _exceptionSourcesPersister;

		public EdoServiceTests()
		{
			_uowFactory = Substitute.For<IUnitOfWorkFactory>();
			_uow = Substitute.For<IUnitOfWork>();
			_orderRepository = Substitute.For<IOrderRepository>();
			_edoRepository = Substitute.For<IEdoRepository>();
			_organizationRepository = Substitute.For<IOrganizationRepository>();

			_edoRequestRepository = Substitute.For<IGenericRepository<FormalEdoRequest>>();
			_edoTaskRepository = Substitute.For<IGenericRepository<OrderEdoTask>>();

			_edoRequestCreatedEventPublisher = Substitute.For<IEdoRequestCreatedEventPublisher>();
			_bus = Substitute.For<IBus>();
			_requestFactories = Enumerable.Empty<IInformalEdoRequestFactory>();
			_manualEdoRequestFactory = Substitute.For<IManualEdoRequestFactory>();

			_customSourcesPersister = Substitute.For<EdoTaskCustomSourcesPersister>(
				_uowFactory,
				Enumerable.Empty<EdoTaskProblemCustomSource>()
			);

			_exceptionSourcesPersister = Substitute.For<EdoTaskExceptionSourcesPersister>(
				_uowFactory,
				Enumerable.Empty<EdoTaskProblemExceptionSource>()
			);

			_problemRegistrar = Substitute.For<EdoProblemRegistrar>(
				_uow,
				_uowFactory,
				_customSourcesPersister,
				_exceptionSourcesPersister
			);

			_messageService = new MessageService(
				Substitute.For<ILogger<MessageService>>(),
				_bus
			);

			_edoCancellationValidator = Substitute.For<IEdoCancellationValidator>();
			_publishEndpoint = Substitute.For<IPublishEndpoint>();
			_edoCancellationService = new EdoCancellationService(
				Substitute.For<ILogger<EdoCancellationService>>(),
				_uow,
				_edoCancellationValidator,
				_problemRegistrar,
				_publishEndpoint
				);
			_userService = Substitute.For<IUserService>();

			_taxcomApiFactory = Substitute.For<ITaxcomApiFactory>();

			_counterpartyEdoAccountEntityController = Substitute.For<ICounterpartyEdoAccountEntityController>();

			_edoService = new EdoService.Library.EdoService(
				_uowFactory,
				_orderRepository,
				_organizationRepository,
				_edoRepository,
				_messageService,
				_userService,
				_edoCancellationService,
				_taxcomApiFactory,
				_edoRequestRepository,
				_edoTaskRepository,
				_counterpartyEdoAccountEntityController,
				_edoRequestCreatedEventPublisher,
				_requestFactories,
				_manualEdoRequestFactory,
				_bus
			);
		}

		[Fact]
		public async Task ResendReceiptDocument_WhenTaskNotFound_ReturnsFailure()
		{
			// Arrange
			var taskId = 123;
			_uowFactory.CreateWithoutRoot(Arg.Any<string>())
				.ReturnsForAnyArgs(x => {
					var uow = Substitute.For<IUnitOfWork>();
					uow.Session.Get<ReceiptEdoTask>(taskId).Returns((ReceiptEdoTask)null);
					return uow;
				});

			// Act
			var result = await _edoService.ResendReceiptDocument(taskId);

			// Assert
			Assert.True(result.IsFailure);
			Assert.Contains(result.Errors, e => e.Code == EdoErrors.HasProblem.Code);
		}

		[Fact]
		public async Task ResendReceiptDocument_WhenOrderNotFound_ReturnsFailure()
		{
			// Arrange
			var taskId = 123;
			var receiptTask = new ReceiptEdoTask
			{
				Id = taskId,
				FormalEdoRequest = new PrimaryEdoRequest
				{
					Order = null
				}
			};

			SetupUowFactoryForReceiptEdoTask(receiptTask);

			// Act
			var result = await _edoService.ResendReceiptDocument(taskId);

			// Assert
			Assert.True(result.IsFailure);
			Assert.Contains(result.Errors, e => e.Code == EdoErrors.HasProblem.Code);
		}

		[Fact]
		public async Task ResendReceiptDocument_WhenTaskIsCompleted_ReturnsFailure()
		{
			// Arrange
			var taskId = 123;
			var order = new OrderEntity { Id = 1 };
			var receiptTask = new ReceiptEdoTask
			{
				Id = taskId,
				Status = EdoTaskStatus.Completed,
				ReceiptStatus = EdoReceiptStatus.New,
				FormalEdoRequest = new PrimaryEdoRequest
				{
					Order = order
				}
			};

			SetupUowFactoryForReceiptEdoTask(receiptTask);

			// Act
			var result = await _edoService.ResendReceiptDocument(taskId);

			// Assert
			Assert.True(result.IsFailure);
			Assert.Contains(result.Errors, e => e == EdoErrors.CreateCannotResendCompletedTask(taskId));
		}

		[Fact]
		public async Task ResendReceiptDocument_WhenReceiptIsCompleted_ReturnsFailure()
		{
			// Arrange
			var taskId = 123;
			var order = new OrderEntity { Id = 1 };
			var receiptTask = new ReceiptEdoTask
			{
				Id = taskId,
				Status = EdoTaskStatus.New,
				ReceiptStatus = EdoReceiptStatus.Completed,
				FormalEdoRequest = new PrimaryEdoRequest
				{
					Order = order
				}
			};

			SetupUowFactoryForReceiptEdoTask(receiptTask);

			// Act
			var result = await _edoService.ResendReceiptDocument(taskId);

			// Assert
			Assert.True(result.IsFailure);
			Assert.Contains(result.Errors, e => e == EdoErrors.CreateCannotResendCompletedReceipt(taskId));
		}

		[Fact]
		public async Task ResendReceiptDocument_WhenReceiptIsSavedToPool_ReturnsFailure()
		{
			// Arrange
			var taskId = 123;
			var order = new OrderEntity { Id = 1 };
			var receiptTask = new ReceiptEdoTask
			{
				Id = taskId,
				Status = EdoTaskStatus.New,
				ReceiptStatus = EdoReceiptStatus.SavedToPool,
				FormalEdoRequest = new PrimaryEdoRequest
				{
					Order = order
				}
			};

			SetupUowFactoryForReceiptEdoTask(receiptTask);

			// Act
			var result = await _edoService.ResendReceiptDocument(taskId);

			// Assert
			Assert.True(result.IsFailure);
			Assert.Contains(result.Errors, e => e == EdoErrors.CreateCannotResendReceiptFromSavedToPool(taskId));
		}

		[Fact]
		public async Task ResendReceiptDocument_WhenFiscalDocumentHasCompletedStage_ReturnsFailure()
		{
			// Arrange
			var taskId = 123;
			var order = new OrderEntity { Id = 1 };
			var receiptTask = new ReceiptEdoTask
			{
				Id = taskId,
				Status = EdoTaskStatus.New,
				ReceiptStatus = EdoReceiptStatus.New,
				FormalEdoRequest = new PrimaryEdoRequest
				{
					Order = order
				},
				FiscalDocuments = new ObservableList<EdoFiscalDocument>
				{
					new() {
						Stage = FiscalDocumentStage.Completed,
						Status = FiscalDocumentStatus.None,
						FiscalNumber = null
					}
				}
			};

			SetupUowFactoryForReceiptEdoTask(receiptTask);

			// Act
			var result = await _edoService.ResendReceiptDocument(taskId);

			// Assert
			Assert.True(result.IsFailure);
			Assert.NotEmpty(result.Errors);
			Assert.Contains(result.Errors, e => e == EdoErrors.CreateCannotResendCompletedReceipt(taskId));
		}

		[Fact]
		public async Task ResendReceiptDocument_WhenFiscalDocumentHasFiscalNumber_ReturnsFailure()
		{
			// Arrange
			var taskId = 123;
			var order = new OrderEntity { Id = 1 };
			var receiptTask = new ReceiptEdoTask
			{
				Id = taskId,
				Status = EdoTaskStatus.New,
				ReceiptStatus = EdoReceiptStatus.New,
				FormalEdoRequest = new PrimaryEdoRequest
				{
					Order = order
				},
				FiscalDocuments = new ObservableList<EdoFiscalDocument>
				{
					new() {
						Stage = FiscalDocumentStage.Preparing,
						Status = FiscalDocumentStatus.None,
						FiscalNumber = "1234567890"
					}
				}
			};

			SetupUowFactoryForReceiptEdoTask(receiptTask);

			// Act
			var result = await _edoService.ResendReceiptDocument(taskId);

			// Assert
			Assert.True(result.IsFailure);
			Assert.Contains(result.Errors, e => e == EdoErrors.CreateCannotResendCompletedReceipt(taskId));
		}

		[Fact]
		public async Task ResendReceiptDocument_WhenFiscalDocumentIsPrinted_ReturnsFailure()
		{
			// Arrange
			var taskId = 123;
			var order = new OrderEntity { Id = 1 };
			var receiptTask = new ReceiptEdoTask
			{
				Id = taskId,
				Status = EdoTaskStatus.New,
				ReceiptStatus = EdoReceiptStatus.New,
				FormalEdoRequest = new PrimaryEdoRequest
				{
					Order = order
				},
				FiscalDocuments = new ObservableList<EdoFiscalDocument>
				{
					new() {
						Stage = FiscalDocumentStage.Preparing,
						Status = FiscalDocumentStatus.Printed,
						FiscalNumber = null
					}
				}
			};

			SetupUowFactoryForReceiptEdoTask(receiptTask);

			// Act
			var result = await _edoService.ResendReceiptDocument(taskId);

			// Assert
			Assert.True(result.IsFailure);
			Assert.Contains(result.Errors, e => e == EdoErrors.CreateCannotResendCompletedReceipt(taskId));
		}

		[Fact]
		public async Task ResendReceiptDocument_WhenValid_ResendsSuccessfully()
		{
			// Arrange
			var taskId = 123;
			var order = new OrderEntity { Id = 1 };
			var productCode = new CarLoadDocumentItemTrueMarkProductCode
			{
				Id = 1,
				SourceCode = new TrueMarkWaterIdentificationCode { Id = 1 }
			};
			var receiptTask = new ReceiptEdoTask
			{
				Id = taskId,
				Status = EdoTaskStatus.New,
				ReceiptStatus = EdoReceiptStatus.New,
				FormalEdoRequest = new PrimaryEdoRequest
				{
					Order = order
				},
				Items = new ObservableList<EdoTaskItem>
				{
					new() {
						ProductCode = productCode,
						CustomerEdoTask = null
					}
				},
				FiscalDocuments = new ObservableList<EdoFiscalDocument>()
			};

			SetupUowFactoryForReceiptEdoTaskWithRequest(receiptTask);

			// Act
			var result = await _edoService.ResendReceiptDocument(taskId);

			// Assert
			Assert.True(result.IsSuccess);
			Assert.Equal(EdoReceiptStatus.New, receiptTask.ReceiptStatus);
		}

		[Fact]
		public async Task ResendReceiptDocument_WhenValidWithFiscalDocuments_ResendsSuccessfully()
		{
			// Arrange
			var taskId = 123;
			var order = new OrderEntity { Id = 1 };
			var productCode = new CarLoadDocumentItemTrueMarkProductCode
			{
				Id = 1,
				SourceCode = new TrueMarkWaterIdentificationCode { Id = 1 }
			};
			var receiptTask = new ReceiptEdoTask
			{
				Id = taskId,
				Status = EdoTaskStatus.New,
				ReceiptStatus = EdoReceiptStatus.New,
				FormalEdoRequest = new PrimaryEdoRequest
				{
					Order = order
				},
				Items = new ObservableList<EdoTaskItem>
				{
					new() {
						ProductCode = productCode,
						CustomerEdoTask = null
					}
				},
				FiscalDocuments = new ObservableList<EdoFiscalDocument>
				{
					new() {
						Stage = FiscalDocumentStage.Preparing,
						Status = FiscalDocumentStatus.Queued,
						FiscalNumber = null
					}
				}
			};

			SetupUowFactoryForReceiptEdoTaskWithRequest(receiptTask);

			// Act
			var result = await _edoService.ResendReceiptDocument(taskId);

			// Assert
			Assert.True(result.IsSuccess);
			Assert.Equal(EdoReceiptStatus.New, receiptTask.ReceiptStatus);
		}

		[Fact]
		public void CanResend_WhenStatusIsCancelled_ReturnsTrue()
		{
			// Act
			var result = _edoService.CanResendEdoDocument(EdoDocumentStatus.Cancelled);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void CanResend_WhenStatusIsError_ReturnsTrue()
		{
			// Act
			var result = _edoService.CanResendEdoDocument(EdoDocumentStatus.Error);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void CanResend_WhenStatusIsNull_ReturnsFalse()
		{
			// Act
			var result = _edoService.CanResendEdoDocument(null);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void CanResend_WhenStatusIsNotResendable_ReturnsFalse()
		{
			// Act
			var result = _edoService.CanResendEdoDocument(EdoDocumentStatus.InProgress);

			// Assert
			Assert.False(result);
		}

		[Theory]
		[InlineData(EdoResendAfterTrueMarkCancellationStatus.CancellationSent)]
		[InlineData(EdoResendAfterTrueMarkCancellationStatus.Completed)]
		public void ResendWithCodesFromPool_WhenCancellationResendAlreadyExists_ReturnsFailure(
			EdoResendAfterTrueMarkCancellationStatus status)
		{
			var taskId = 123;
			var order = new OrderEntity { Id = 1, OrderStatus = OrderStatus.NewOrder };
			var edoTask = CreateDocumentEdoTask(taskId, order);
			var cancellationRequest = new EdoResendAfterTrueMarkCancellationRequest
			{
				OriginalEdoTask = edoTask,
				Status = status
			};

			SetupUowFactoryForDocumentEdoTask(edoTask, new[] { cancellationRequest });

			var result = _edoService.ResendEdoDocumentForOrderWithCodesFromPool(taskId);

			Assert.True(result.IsFailure);
			Assert.Contains(result.Errors, e => e == EdoErrors.TrueMarkCancellationResendAlreadyExists);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void NewResendScenario_WhenOrderIsUndelivered_ReturnsFailure(bool useCodesFromPool)
		{
			var taskId = 123;
			var order = new OrderEntity { Id = 1, OrderStatus = OrderStatus.Canceled };
			var edoTask = CreateDocumentEdoTask(taskId, order);

			SetupUowFactoryForDocumentEdoTask(edoTask);

			var result = useCodesFromPool
				? _edoService.ResendEdoDocumentForOrderWithCodesFromPool(taskId)
				: _edoService.ResendEdoDocumentWithOriginalCodes(taskId);

			Assert.True(result.IsFailure);
			Assert.Contains(result.Errors, e => e == EdoErrors.IsUndeliveredOrder);
		}

		[Fact]
		public void ResendWithOriginalCodes_WhenPoolRequestAlreadyExists_ReturnsFailure()
		{
			var taskId = 123;
			var order = new OrderEntity { Id = 1, OrderStatus = OrderStatus.NewOrder };
			var edoTask = CreateDocumentEdoTask(taskId, order);
			var withdrawalTask = new WithdrawalEdoTask { Id = 321 };
			var withdrawalRequest = new WithdrawalEdoRequest
			{
				BaseDocumentEdoTask = edoTask,
				Task = withdrawalTask
			};
			var withdrawalDocument = new TrueMarkDocument
			{
				Order = order,
				Guid = Guid.NewGuid(),
				IsSuccess = true,
				Type = TrueMarkDocument.TrueMarkDocumentType.Withdrawal,
				WithdrawalEdoTask = withdrawalTask
			};
			var poolRequest = new ManualEdoRequest
			{
				Order = order,
				Task = null
			};

			SetupUowFactoryForDocumentEdoTask(
				edoTask,
				withdrawalRequests: new[] { withdrawalRequest },
				trueMarkDocuments: new[] { withdrawalDocument });
			_edoRepository.GetOrderEdoDocumentByTaskId(Arg.Any<IUnitOfWork>(), taskId)
				.Returns(new OrderEdoDocument { DocumentTaskId = taskId, Status = EdoDocumentStatus.Warning });
			_edoRequestRepository.GetCount(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<Expression<Func<FormalEdoRequest, bool>>>())
				.Returns(callInfo =>
				{
					var predicate = callInfo.Arg<Expression<Func<FormalEdoRequest, bool>>>().Compile();
					return predicate(poolRequest) ? 1 : 0;
				});

			var result = _edoService.ResendEdoDocumentWithOriginalCodes(taskId);

			Assert.True(result.IsFailure);
			Assert.Contains(result.Errors, error => error.Code == "DocumentHasOtherRequests");
		}

		[Fact]
		public void ResendWithOriginalCodes_WhenWithdrawalRequestDoesNotExist_ReturnsFailure()
		{
			var taskId = 123;
			var order = new OrderEntity { Id = 1, OrderStatus = OrderStatus.NewOrder };
			var edoTask = CreateDocumentEdoTask(taskId, order);
			var orderDocument = new OrderEdoDocument
			{
				DocumentTaskId = taskId,
				Status = EdoDocumentStatus.Warning
			};
			var uow = Substitute.For<IUnitOfWork>();
			var transactionOpened = false;
			uow.When(x => x.OpenTransaction()).Do(_ => transactionOpened = true);
			uow.Session.Get<OrderEdoTask>(taskId, LockMode.Upgrade).Returns(_ =>
			{
				Assert.True(transactionOpened);
				return edoTask;
			});
			uow.Session.Get<DocumentEdoTask>(taskId).Returns(edoTask);
			uow.GetAll<EdoResendAfterTrueMarkCancellationRequest>()
				.Returns(Array.Empty<EdoResendAfterTrueMarkCancellationRequest>().AsQueryable());
			uow.GetAll<WithdrawalEdoRequest>().Returns(Array.Empty<WithdrawalEdoRequest>().AsQueryable());
			uow.GetAll<TrueMarkDocument>().Returns(Array.Empty<TrueMarkDocument>().AsQueryable());
			_uowFactory.CreateWithoutRoot(Arg.Any<string>()).ReturnsForAnyArgs(uow);
			_edoRepository.GetOrderEdoDocumentByTaskId(uow, taskId).Returns(orderDocument);

			var result = _edoService.ResendEdoDocumentWithOriginalCodes(taskId);

			Assert.True(result.IsFailure);
			Assert.Contains(result.Errors, error => error == EdoErrors.SuccessfulWithdrawalForResendNotFound);
			uow.DidNotReceive().Save(Arg.Any<EdoResendAfterTrueMarkCancellationRequest>());
			uow.DidNotReceive().Commit();
			_publishEndpoint.DidNotReceiveWithAnyArgs().Publish(default(RequestDocflowCancellationEvent), default);
			_edoRequestCreatedEventPublisher.DidNotReceiveWithAnyArgs().Publish(default, default, default);
		}

		[Theory]
		[InlineData(EdoDocumentStatus.Warning)]
		[InlineData(EdoDocumentStatus.CompletedWithDivergences)]
		public void ResendWithOriginalCodes_WhenWithdrawalDocumentExists_CreatesCancellationRequestWithoutPublishingResend(
			EdoDocumentStatus status)
		{
			var taskId = 123;
			var order = new OrderEntity { Id = 1, OrderStatus = OrderStatus.NewOrder };
			var sourceCode = new TrueMarkWaterIdentificationCode { RawCode = "test-code" };
			var edoTask = CreateDocumentEdoTask(taskId, order);
			edoTask.Items.Add(new EdoTaskItem
			{
				CustomerEdoTask = edoTask,
				ProductCode = new AutoTrueMarkProductCode { SourceCode = sourceCode }
			});
			var withdrawalTask = new WithdrawalEdoTask { Id = 321 };
			var withdrawalRequest = new WithdrawalEdoRequest
			{
				BaseDocumentEdoTask = edoTask,
				Task = withdrawalTask
			};
			var withdrawalDocument = new TrueMarkDocument
			{
				Order = order,
				Guid = Guid.NewGuid(),
				IsSuccess = true,
				Type = TrueMarkDocument.TrueMarkDocumentType.Withdrawal,
				WithdrawalEdoTask = withdrawalTask
			};
			var resendRequest = new ManualEdoRequest { Id = 456, Order = order };
			TrueMarkProductCode[] createdCodes = null;
			var orderDocument = new OrderEdoDocument
			{
				DocumentTaskId = taskId,
				Status = status
			};
			var uow = Substitute.For<IUnitOfWork>();
			var transactionOpened = false;
			uow.When(x => x.OpenTransaction()).Do(_ => transactionOpened = true);
			uow.Session.Get<OrderEdoTask>(taskId, LockMode.Upgrade).Returns(_ =>
			{
				Assert.True(transactionOpened);
				return edoTask;
			});
			uow.Session.Get<DocumentEdoTask>(taskId).Returns(edoTask);
			uow.Session.GetAsync<EdoTask>(taskId, Arg.Any<CancellationToken>()).Returns(edoTask);
			uow.Session.QueryOver<OrderEdoDocument>()
				.Where(Arg.Any<Expression<Func<OrderEdoDocument, bool>>>())
				.SingleOrDefaultAsync(Arg.Any<CancellationToken>())
				.Returns(orderDocument);
			uow.GetAll<EdoResendAfterTrueMarkCancellationRequest>()
				.Returns(Array.Empty<EdoResendAfterTrueMarkCancellationRequest>().AsQueryable());
			uow.GetAll<WithdrawalEdoRequest>().Returns(new[] { withdrawalRequest }.AsQueryable());
			uow.GetAll<TrueMarkDocument>().Returns(new[] { withdrawalDocument }.AsQueryable());
			_uowFactory.CreateWithoutRoot(Arg.Any<string>()).ReturnsForAnyArgs(uow);
			_edoRepository.GetOrderEdoDocumentByTaskId(uow, taskId).Returns(orderDocument);
			_userService.GetCurrentUser().Returns(new UserBase { Name = "Тестовый пользователь" });
			_edoCancellationValidator.CanCancelEdoTask(edoTask).Returns(true);
			_manualEdoRequestFactory.Create(
				uow,
				order,
				Arg.Any<IEnumerable<TrueMarkProductCode>>())
				.Returns(callInfo =>
				{
					createdCodes = callInfo.Arg<IEnumerable<TrueMarkProductCode>>().ToArray();
					return resendRequest;
				});

			var result = _edoService.ResendEdoDocumentWithOriginalCodes(taskId);

			Assert.True(result.IsSuccess);
			var createdCode = Assert.IsType<AutoTrueMarkProductCode>(Assert.Single(createdCodes));
			Assert.Same(sourceCode, createdCode.SourceCode);
			Assert.Equal(SourceProductCodeStatus.New, createdCode.SourceCodeStatus);
			Assert.Equal(EdoTaskStatus.InCancellation, edoTask.Status);
			uow.Received().Save(resendRequest);
			uow.Received().Save(Arg.Is<EdoResendAfterTrueMarkCancellationRequest>(request =>
				request.Order == order
				&& request.OriginalEdoTask == edoTask
				&& request.ResendEdoRequest == resendRequest
				&& request.WithdrawalDocument == withdrawalDocument
				&& request.Status == EdoResendAfterTrueMarkCancellationStatus.WaitingForCancellation));
			uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
			uow.Received().Commit();
			_publishEndpoint.Received(1).Publish(
				Arg.Is<RequestDocflowCancellationEvent>(x =>
					x.TaskId == taskId
					&& x.Reason.Contains("Тестовый пользователь")),
				Arg.Any<CancellationToken>());
			_edoRequestCreatedEventPublisher.DidNotReceiveWithAnyArgs().Publish(default, default, default);
		}

		[Fact]
		public void ResendWithOriginalCodes_WhenWithdrawalRequestExistsWithoutSuccessfulDocument_ReturnsFailure()
		{
			var taskId = 123;
			var order = new OrderEntity { Id = 1, OrderStatus = OrderStatus.NewOrder };
			var edoTask = CreateDocumentEdoTask(taskId, order);
			var withdrawalRequest = new WithdrawalEdoRequest
			{
				BaseDocumentEdoTask = edoTask,
				Task = new WithdrawalEdoTask { Id = 321 }
			};

			SetupUowFactoryForDocumentEdoTask(
				edoTask,
				withdrawalRequests: new[] { withdrawalRequest },
				trueMarkDocuments: Array.Empty<TrueMarkDocument>());
			_edoRepository.GetOrderEdoDocumentByTaskId(Arg.Any<IUnitOfWork>(), taskId)
				.Returns(new OrderEdoDocument { DocumentTaskId = taskId, Status = EdoDocumentStatus.Warning });

			var result = _edoService.ResendEdoDocumentWithOriginalCodes(taskId);

			Assert.True(result.IsFailure);
			Assert.Contains(result.Errors, error => error == EdoErrors.SuccessfulWithdrawalForResendNotFound);
			_edoRequestCreatedEventPublisher.DidNotReceiveWithAnyArgs().Publish(default, default, default);
		}

		[Fact]
		public void ResendWithOriginalCodes_WhenWithdrawalDocumentExistsWithoutRequiredStatus_ReturnsFailure()
		{
			var taskId = 123;
			var order = new OrderEntity { Id = 1, OrderStatus = OrderStatus.NewOrder };
			var edoTask = CreateDocumentEdoTask(taskId, order);
			var withdrawalTask = new WithdrawalEdoTask { Id = 321 };
			var withdrawalRequest = new WithdrawalEdoRequest
			{
				BaseDocumentEdoTask = edoTask,
				Task = withdrawalTask
			};
			var withdrawalDocument = new TrueMarkDocument
			{
				Order = order,
				Guid = Guid.NewGuid(),
				IsSuccess = true,
				Type = TrueMarkDocument.TrueMarkDocumentType.Withdrawal,
				WithdrawalEdoTask = withdrawalTask
			};

			SetupUowFactoryForDocumentEdoTask(
				edoTask,
				withdrawalRequests: new[] { withdrawalRequest },
				trueMarkDocuments: new[] { withdrawalDocument });
			_edoRepository.GetOrderEdoDocumentByTaskId(Arg.Any<IUnitOfWork>(), taskId)
				.Returns(new OrderEdoDocument { DocumentTaskId = taskId, Status = EdoDocumentStatus.Error });

			var result = _edoService.ResendEdoDocumentWithOriginalCodes(taskId);

			Assert.True(result.IsFailure);
			Assert.Contains(result.Errors, error => error == EdoErrors.ResendWithOriginalCodesStatusNotSupported);
			_edoRequestCreatedEventPublisher.DidNotReceiveWithAnyArgs().Publish(default, default, default);
		}

		private static DocumentEdoTask CreateDocumentEdoTask(int taskId, OrderEntity order)
		{
			return new DocumentEdoTask
			{
				Id = taskId,
				FormalEdoRequest = new PrimaryEdoRequest
				{
					Order = order
				}
			};
		}

		private void SetupUowFactoryForDocumentEdoTask(
			DocumentEdoTask edoTask,
			IEnumerable<EdoResendAfterTrueMarkCancellationRequest> cancellationRequests = null,
			IEnumerable<WithdrawalEdoRequest> withdrawalRequests = null,
			IEnumerable<TrueMarkDocument> trueMarkDocuments = null)
		{
			var existingCancellationRequests = cancellationRequests
				?? Array.Empty<EdoResendAfterTrueMarkCancellationRequest>();
			var existingWithdrawalRequests = withdrawalRequests ?? Array.Empty<WithdrawalEdoRequest>();
			var existingTrueMarkDocuments = trueMarkDocuments ?? Array.Empty<TrueMarkDocument>();

			_uowFactory.CreateWithoutRoot(Arg.Any<string>())
				.ReturnsForAnyArgs(_ =>
				{
					var uow = Substitute.For<IUnitOfWork>();
					var transactionOpened = false;
					uow.When(x => x.OpenTransaction()).Do(_ => transactionOpened = true);
					uow.Session.Get<OrderEdoTask>(edoTask.Id, LockMode.Upgrade).Returns(_ =>
					{
						Assert.True(transactionOpened);
						return edoTask;
					});
					uow.Session.Get<OrderEdoTask>(edoTask.Id).Returns(edoTask);
					uow.Session.Get<DocumentEdoTask>(edoTask.Id).Returns(edoTask);
					uow.GetAll<EdoResendAfterTrueMarkCancellationRequest>()
						.Returns(existingCancellationRequests.AsQueryable());
					uow.GetAll<WithdrawalEdoRequest>()
						.Returns(existingWithdrawalRequests.AsQueryable());
					uow.GetAll<TrueMarkDocument>()
						.Returns(existingTrueMarkDocuments.AsQueryable());
					return uow;
				});
		}

		private void SetupUowFactoryForReceiptEdoTask(ReceiptEdoTask receiptTask)
		{
			var taskId = receiptTask.Id;

			_uowFactory.CreateWithoutRoot(Arg.Any<string>())
				.ReturnsForAnyArgs(x => {
					var uow = Substitute.For<IUnitOfWork>();
					uow.Session.Get<ReceiptEdoTask>(taskId).Returns(receiptTask);
					return uow;
				});
		}

		private void SetupUowFactoryForReceiptEdoTaskWithRequest(ReceiptEdoTask receiptTask)
		{
			var taskId = receiptTask.Id;

			_uowFactory.CreateWithoutRoot(Arg.Any<string>())
				.ReturnsForAnyArgs(x => {
					var uow = Substitute.For<IUnitOfWork>();
					uow.Session.Get<ReceiptEdoTask>(taskId).Returns(receiptTask);
					uow.Session.Query<ManualEdoRequest>().Returns(new List<ManualEdoRequest>().AsQueryable());
					uow.SaveAsync(Arg.Any<object>(), cancellationToken: Arg.Any<CancellationToken>())
						.Returns(Task.CompletedTask);
					uow.CommitAsync(Arg.Any<CancellationToken>())
						.Returns(Task.CompletedTask);

					return uow;
				});
		}
	}
}
