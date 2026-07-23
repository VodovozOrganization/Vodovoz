using Edo.Transport;
using EdoService.Library.Factories;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors.Edo;
using Xunit;
using IOrderRepository = Vodovoz.EntityRepositories.Orders.IOrderRepository;

namespace EdoServices.Tests
{
	public class EdoServiceTests
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IUnitOfWork _uow;
		private readonly IOrderRepository _orderRepository;
		private readonly IEdoRepository _edoRepository;
		private readonly IGenericRepository<ReceiptEdoTask> _receiptRepository;
		private readonly IEdoRequestCreatedEventPublisher _edoRequestCreatedEventPublisher;
		private readonly IBus _bus;
		private readonly IEnumerable<IInformalEdoRequestFactory> _requestFactories;
		private readonly EdoService.Library.EdoService _edoService;

		public EdoServiceTests()
		{
			_uowFactory = Substitute.For<IUnitOfWorkFactory>();
			_uow = Substitute.For<IUnitOfWork>();
			_orderRepository = Substitute.For<IOrderRepository>();
			_edoRepository = Substitute.For<IEdoRepository>();
			_receiptRepository = Substitute.For<IGenericRepository<ReceiptEdoTask>>();
			_edoRequestCreatedEventPublisher = Substitute.For<IEdoRequestCreatedEventPublisher>();
			_bus = Substitute.For<IBus>();
			_requestFactories = Enumerable.Empty<IInformalEdoRequestFactory>();

			_edoService = new EdoService.Library.EdoService(
				_uowFactory,
				_orderRepository,
				_receiptRepository,
				_edoRepository,
				_edoRequestCreatedEventPublisher,
				_bus,
				_requestFactories);
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
			Assert.Equal(EdoTaskStatus.Cancelled, receiptTask.Status);
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
			Assert.Equal(EdoTaskStatus.Cancelled, receiptTask.Status);
			Assert.Equal(EdoReceiptStatus.New, receiptTask.ReceiptStatus);
		}

		[Fact]
		public async Task ResendReceiptFromSavedToPool_WhenAllTasksInSavedToPool_ResendsSuccessfully()
		{
			// Arrange
			var orderId = 1;
			var orderTaskId = 100;
			var order = new Order { Id = orderId };
			var tasks = new List<ReceiptEdoTask>
			{
				new() {
					Id = 1,
					ReceiptStatus = EdoReceiptStatus.SavedToPool,
					FormalEdoRequest = new PrimaryEdoRequest { Order = order }
				},
				new() {
					Id = 2,
					ReceiptStatus = EdoReceiptStatus.SavedToPool,
					FormalEdoRequest = new PrimaryEdoRequest { Order = order }
				}
			};

			var tasksResult = Result.Success<IEnumerable<ReceiptEdoTask>>(tasks);
			SetupReceiptRepository(tasksResult);

			_orderRepository.GetOrderByIdAsync(Arg.Any<IUnitOfWork>(), orderId, Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(order));

			_uow.SaveAsync(Arg.Any<ManualEdoRequest>(), cancellationToken: Arg.Any<CancellationToken>())
				.Returns(Task.CompletedTask);

			_uow.CommitAsync(Arg.Any<CancellationToken>())
				.Returns(Task.CompletedTask);

			// Act
			var result = await _edoService.ResendReceiptFromSavedToPool(_uow, orderTaskId, orderId, CancellationToken.None);

			// Assert
			Assert.True(result.IsSuccess);
			await _uow.Received().SaveAsync(Arg.Any<ManualEdoRequest>(), cancellationToken: Arg.Any<CancellationToken>());
			await _uow.Received().CommitAsync(cancellationToken: Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ResendReceiptFromSavedToPool_WhenTaskNotInSavedToPool_ReturnsFailure()
		{
			// Arrange
			var orderId = 1;
			var orderTaskId = 100;
			var order = new Order { Id = orderId };
			var tasks = new List<ReceiptEdoTask>
			{
				new() {
					Id = 1,
					ReceiptStatus = EdoReceiptStatus.SavedToPool,
					FormalEdoRequest = new PrimaryEdoRequest { Order = order }
				},
				new() {
					Id = 2,
					ReceiptStatus = EdoReceiptStatus.Sending,
					FormalEdoRequest = new PrimaryEdoRequest { Order = order }
				}
			};

			var tasksResult = Result.Success<IEnumerable<ReceiptEdoTask>>(tasks);
			SetupReceiptRepository(tasksResult);

			_orderRepository.GetOrderByIdAsync(Arg.Any<IUnitOfWork>(), orderId, Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(order));

			_uow.SaveAsync(Arg.Any<ManualEdoRequest>(), cancellationToken: Arg.Any<CancellationToken>())
				.Returns(Task.CompletedTask);

			_uow.CommitAsync(Arg.Any<CancellationToken>())
				.Returns(Task.CompletedTask);

			// Act
			var result = await _edoService.ResendReceiptFromSavedToPool(_uow, orderTaskId, orderId, CancellationToken.None);

			// Assert
			Assert.True(result.IsFailure);
			Assert.Contains(result.Errors, e => e == EdoErrors.CreateCannotResendReceiptFromSavedToPoolTask(orderId));
			await _uow.DidNotReceive().SaveAsync(Arg.Any<ManualEdoRequest>(), cancellationToken: Arg.Any<CancellationToken>());
			await _uow.DidNotReceive().CommitAsync(cancellationToken: Arg.Any<CancellationToken>());
		}

		[Fact]
		public void CanResend_WhenStatusIsCancelled_ReturnsTrue()
		{
			// Act
			var result = _edoService.CanResend(EdoDocumentStatus.Cancelled);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void CanResend_WhenStatusIsError_ReturnsTrue()
		{
			// Act
			var result = _edoService.CanResend(EdoDocumentStatus.Error);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void CanResend_WhenStatusIsNull_ReturnsFalse()
		{
			// Act
			var result = _edoService.CanResend(null);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void CanResend_WhenStatusIsNotResendable_ReturnsFalse()
		{
			// Act
			var result = _edoService.CanResend(EdoDocumentStatus.InProgress);

			// Assert
			Assert.False(result);
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

		private void SetupReceiptRepository(Result<IEnumerable<ReceiptEdoTask>> tasksResult)
		{
			_receiptRepository
				.GetAsync(Arg.Any<IUnitOfWork>(), Arg.Any<Expression<Func<ReceiptEdoTask, bool>>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
				.ReturnsForAnyArgs(Task.FromResult(tasksResult));
		}
	}
}
