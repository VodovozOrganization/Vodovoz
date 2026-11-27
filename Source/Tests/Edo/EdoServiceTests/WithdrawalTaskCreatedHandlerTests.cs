using Edo.Common;
using Edo.Problems;
using Edo.Problems.Custom;
using Edo.Problems.Custom.Sources.Withdrawal;
using Edo.Problems.Exception;
using Edo.Withdrawal;
using Microsoft.Extensions.Logging;
using NHibernate;
using NSubstitute;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMark.Contracts.Documents;
using TrueMark.Contracts.Responses;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Client;
using Vodovoz.Settings.Edo;
using Xunit;
using IEdoRepository = Vodovoz.Core.Data.Repositories.IEdoRepository;

namespace EdoServices.Tests
{
	public class WithdrawalTaskCreatedHandlerTests
	{
		private readonly ILogger<WithdrawalTaskCreatedHandler> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ITrueMarkApiClient _trueMarkApiClient;
		private readonly ICounterpartyEdoAccountEntityController _edoAccountEntityController;
		private readonly IGenericRepository<TrueMarkDocument> _trueMarkDocumentRepository;
		private readonly ITrueMarkCodesValidator _trueMarkTaskCodesValidator;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly IEdoSettings _edoSettings;
		private readonly IEdoRepository _edoRepository;

		public WithdrawalTaskCreatedHandlerTests()
		{
			_logger = Substitute.For<ILogger<WithdrawalTaskCreatedHandler>>();
			_uowFactory = Substitute.For<IUnitOfWorkFactory>();
			_trueMarkApiClient = Substitute.For<ITrueMarkApiClient>();
			_edoAccountEntityController = Substitute.For<ICounterpartyEdoAccountEntityController>();
			_trueMarkDocumentRepository = Substitute.For<IGenericRepository<TrueMarkDocument>>();
			_trueMarkTaskCodesValidator = Substitute.For<ITrueMarkCodesValidator>();
			_trueMarkTaskCodesValidator.ValidateAsync(
				Arg.Any<OrderEdoTask>(),
				Arg.Any<EdoTaskItemTrueMarkStatusProvider>(),
				Arg.Any<CancellationToken>())
				.Returns(new TrueMarkTaskValidationResult(Enumerable.Empty<TrueMarkCodeValidationResult>()));

			_edoTaskTrueMarkCodeCheckerFactory = new EdoTaskItemTrueMarkStatusProviderFactory(_trueMarkApiClient);

			var customSourcesPersister = Substitute.For<EdoTaskCustomSourcesPersister>(
				_uowFactory,
				Enumerable.Empty<EdoTaskProblemCustomSource>());
			var exceptionSourcesPersister = Substitute.For<EdoTaskExceptionSourcesPersister>(
				_uowFactory,
				Enumerable.Empty<EdoTaskProblemExceptionSource>());

			_edoProblemRegistrar = Substitute.For<EdoProblemRegistrar>(
				Substitute.For<IUnitOfWork>(),
				_uowFactory,
				customSourcesPersister,
				exceptionSourcesPersister,
				Substitute.For<Notifications.Infrastructure.INotificationPublisher<EdoNotifications.Contracts.EdoNotificationMessage>>());

			_edoSettings = Substitute.For<IEdoSettings>();
			_edoRepository = Substitute.For<IEdoRepository>();
		}

		private class TestOrderEntity : OrderEntity
		{
			public void SetClient(CounterpartyEntity client) => Client = client;
			public void SetPaymentType(PaymentType paymentType) => PaymentType = paymentType;
			public void SetDeliveryDate(DateTime? deliveryDate) => DeliveryDate = deliveryDate;
		}

		private class TestOrderItemEntity : OrderItemEntity
		{
			public TestOrderItemEntity(NomenclatureEntity nomenclature, decimal price, decimal count)
			{
				Nomenclature = nomenclature;
				Price = price;
				Count = count;
				ActualCount = count;
			}
		}

		private WithdrawalTaskCreatedHandler CreateHandler()
		{
			return new WithdrawalTaskCreatedHandler(
				_logger,
				_uowFactory,
				_trueMarkApiClient,
				_edoAccountEntityController,
				_trueMarkDocumentRepository,
				_trueMarkTaskCodesValidator,
				_edoTaskTrueMarkCodeCheckerFactory,
				_edoProblemRegistrar,
				_edoSettings,
				_edoRepository);
		}

		[Fact]
		public async Task HandleWithdrawal_WhenMixedOrderWaterAndPetFood_SendsTwoDocumentsWithDifferentProductGroups()
		{
			// Arrange
			var taskId = 101;
			var orderId = 202;
			var orgInn = "7800000001";
			var task = CreateValidWithdrawalTask(taskId, orderId, orgInn, hasWater: true, hasPetFood: true);

			var (mainUow, _) = SetupUnitOfWorks(task);

			_trueMarkDocumentRepository.GetAsync(
				mainUow,
				Arg.Any<Expression<Func<TrueMarkDocument, bool>>>(),
				cancellationToken: Arg.Any<CancellationToken>())
				.Returns(Result.Success<IEnumerable<TrueMarkDocument>>(Enumerable.Empty<TrueMarkDocument>()));

			_trueMarkApiClient.GetProductInstanceInfoAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
				.Returns(new ProductInstancesInfoResponse
				{
					InstanceStatuses = new[]
					{
						new ProductInstanceStatus { IdentificationCode = "010460000000000121water1", Gtin = "04600000000001", Status = ProductInstanceStatusEnum.Applied },
						new ProductInstanceStatus { IdentificationCode = "010460000000000221petfood1", Gtin = "04600000000002", Status = ProductInstanceStatusEnum.Applied }
					}
				});

			_trueMarkApiClient.SendIndividualAccountingWithdrawalDocument(
				Arg.Any<string>(), orgInn, TrueMarkProductGroup.Water, Arg.Any<CancellationToken>())
				.Returns(Guid.NewGuid().ToString());

			_trueMarkApiClient.SendIndividualAccountingWithdrawalDocument(
				Arg.Any<string>(), orgInn, TrueMarkProductGroup.PetFood, Arg.Any<CancellationToken>())
				.Returns(Guid.NewGuid().ToString());

			var handler = CreateHandler();

			// Act
			await handler.HandleWithdrawal(taskId, CancellationToken.None);

			// Assert
			await _trueMarkApiClient.Received(1).SendIndividualAccountingWithdrawalDocument(
				Arg.Any<string>(), orgInn, TrueMarkProductGroup.Water, Arg.Any<CancellationToken>());
			await _trueMarkApiClient.Received(1).SendIndividualAccountingWithdrawalDocument(
				Arg.Any<string>(), orgInn, TrueMarkProductGroup.PetFood, Arg.Any<CancellationToken>());

			await mainUow.Received(2).SaveAsync(Arg.Any<TrueMarkDocument>(), cancellationToken: Arg.Any<CancellationToken>());
			await mainUow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
			Assert.Equal(EdoTaskStatus.Completed, task.Status);
		}

		[Fact]
		public async Task HandleWithdrawal_WhenWaterGroupAlreadySentByCurrentTask_SendsOnlyPetFood()
		{
			// Arrange
			var taskId = 102;
			var orderId = 203;
			var orgInn = "7800000002";
			var task = CreateValidWithdrawalTask(taskId, orderId, orgInn, hasWater: true, hasPetFood: true);

			var (mainUow, _) = SetupUnitOfWorks(task);

			var existingWaterDoc = new TrueMarkDocument
			{
				Id = 1,
				Order = task.FormalEdoRequest.Order,
				WithdrawalEdoTask = task,
				ProductGroup = TrueMarkProductGroup.Water,
				Type = TrueMarkDocument.TrueMarkDocumentType.Withdrawal
			};

			_trueMarkDocumentRepository.GetAsync(
				mainUow,
				Arg.Any<Expression<Func<TrueMarkDocument, bool>>>(),
				cancellationToken: Arg.Any<CancellationToken>())
				.Returns(Result.Success<IEnumerable<TrueMarkDocument>>(new[] { existingWaterDoc }));

			_trueMarkApiClient.GetProductInstanceInfoAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
				.Returns(new ProductInstancesInfoResponse
				{
					InstanceStatuses = new[]
					{
						new ProductInstanceStatus { IdentificationCode = "010460000000000221petfood1", Gtin = "04600000000002", Status = ProductInstanceStatusEnum.Applied }
					}
				});

			_trueMarkApiClient.SendIndividualAccountingWithdrawalDocument(
				Arg.Any<string>(), orgInn, TrueMarkProductGroup.PetFood, Arg.Any<CancellationToken>())
				.Returns(Guid.NewGuid().ToString());

			var handler = CreateHandler();

			// Act
			await handler.HandleWithdrawal(taskId, CancellationToken.None);

			// Assert
			await _trueMarkApiClient.DidNotReceive().SendIndividualAccountingWithdrawalDocument(
				Arg.Any<string>(), orgInn, TrueMarkProductGroup.Water, Arg.Any<CancellationToken>());
			await _trueMarkApiClient.Received(1).SendIndividualAccountingWithdrawalDocument(
				Arg.Any<string>(), orgInn, TrueMarkProductGroup.PetFood, Arg.Any<CancellationToken>());

			await mainUow.Received(1).SaveAsync(Arg.Any<TrueMarkDocument>(), cancellationToken: Arg.Any<CancellationToken>());
			Assert.Equal(EdoTaskStatus.Completed, task.Status);
		}

		[Fact]
		public async Task HandleWithdrawal_WhenDocumentForSameProductGroupExistsFromOtherTask_RegistersProblemAndStops()
		{
			// Arrange
			var taskId = 103;
			var otherTaskId = 999;
			var orderId = 204;
			var orgInn = "7800000003";
			var task = CreateValidWithdrawalTask(taskId, orderId, orgInn, hasWater: true, hasPetFood: false);

			var otherTask = new WithdrawalEdoTask { Id = otherTaskId };

			var (mainUow, _) = SetupUnitOfWorks(task);

			var existingDocFromOtherTask = new TrueMarkDocument
			{
				Id = 2,
				Order = task.FormalEdoRequest.Order,
				WithdrawalEdoTask = otherTask,
				ProductGroup = TrueMarkProductGroup.Water,
				Type = TrueMarkDocument.TrueMarkDocumentType.Withdrawal
			};

			_trueMarkDocumentRepository.GetAsync(
				mainUow,
				Arg.Any<Expression<Func<TrueMarkDocument, bool>>>(),
				cancellationToken: Arg.Any<CancellationToken>())
				.Returns(Result.Success<IEnumerable<TrueMarkDocument>>(new[] { existingDocFromOtherTask }));

			var handler = CreateHandler();

			// Act
			await handler.HandleWithdrawal(taskId, CancellationToken.None);

			// Assert
			await _edoProblemRegistrar.Received(1).RegisterCustomProblem<WithdrawalDocumentForOrderExists>(
				taskId,
				Arg.Any<IEnumerable<EdoTaskItem>>(),
				Arg.Any<CancellationToken>());

			await _trueMarkApiClient.DidNotReceiveWithAnyArgs().SendIndividualAccountingWithdrawalDocument(
				default, default, default, default);
			Assert.NotEqual(EdoTaskStatus.Completed, task.Status);
		}

		[Fact]
		public async Task HandleWithdrawal_WhenLegacyDocumentWithNullWithdrawalEdoTaskExists_RegistersProblemAndStops()
		{
			// Arrange
			var taskId = 104;
			var orderId = 205;
			var orgInn = "7800000004";
			var task = CreateValidWithdrawalTask(taskId, orderId, orgInn, hasWater: true, hasPetFood: false);

			var (mainUow, _) = SetupUnitOfWorks(task);

			var legacyDoc = new TrueMarkDocument
			{
				Id = 3,
				Order = task.FormalEdoRequest.Order,
				WithdrawalEdoTask = null,
				ProductGroup = TrueMarkProductGroup.Water,
				Type = TrueMarkDocument.TrueMarkDocumentType.Withdrawal
			};

			_trueMarkDocumentRepository.GetAsync(
				mainUow,
				Arg.Any<Expression<Func<TrueMarkDocument, bool>>>(),
				cancellationToken: Arg.Any<CancellationToken>())
				.Returns(Result.Success<IEnumerable<TrueMarkDocument>>(new[] { legacyDoc }));

			var handler = CreateHandler();

			// Act
			await handler.HandleWithdrawal(taskId, CancellationToken.None);

			// Assert
			await _edoProblemRegistrar.Received(1).RegisterCustomProblem<WithdrawalDocumentForOrderExists>(
				taskId,
				Arg.Any<IEnumerable<EdoTaskItem>>(),
				Arg.Any<CancellationToken>());

			await _trueMarkApiClient.DidNotReceiveWithAnyArgs().SendIndividualAccountingWithdrawalDocument(
				default, default, default, default);
			Assert.NotEqual(EdoTaskStatus.Completed, task.Status);
		}

		private (IUnitOfWork MainUow, IUnitOfWork DocUow) SetupUnitOfWorks(WithdrawalEdoTask task)
		{
			var mainUow = Substitute.For<IUnitOfWork>();
			var mainSession = Substitute.For<ISession>();
			mainUow.Session.Returns(mainSession);

			mainSession.GetAsync<WithdrawalEdoTask>(Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(task);
			mainSession.GetAsync<OrderEntity>(Arg.Any<object>(), Arg.Any<CancellationToken>())
				.Returns(task.FormalEdoRequest.Order);
			mainSession.GetAsync<OrganizationEntity>(Arg.Any<object>(), Arg.Any<CancellationToken>())
				.Returns(task.FormalEdoRequest.Order.Contract.Organization);

			var orderDoc = new OrderEdoDocument
			{
				DocumentTaskId = task.FormalEdoRequest is WithdrawalEdoRequest wr && wr.BaseDocumentEdoTask != null ? wr.BaseDocumentEdoTask.Id : task.Id,
				Status = EdoDocumentStatus.Sent,
				CreationTime = DateTime.Today.AddDays(-10)
			};
			_edoRepository.GetOrderEdoDocumentsByOrderId(mainUow, task.FormalEdoRequest.Order.Id).Returns(new[] { orderDoc });
			_edoSettings.WithdrawalDocflowTimeoutDays.Returns(5);

			var docUow = Substitute.For<IUnitOfWork>();
			var docSession = Substitute.For<ISession>();
			docUow.Session.Returns(docSession);

			docSession.GetAsync<OrderEntity>(Arg.Any<object>(), Arg.Any<CancellationToken>())
				.Returns(task.FormalEdoRequest.Order);
			docSession.GetAsync<OrganizationEntity>(Arg.Any<object>(), Arg.Any<CancellationToken>())
				.Returns(task.FormalEdoRequest.Order.Contract.Organization);
			docSession.GetAsync<WithdrawalEdoTask>(Arg.Any<object>(), Arg.Any<CancellationToken>())
				.Returns(task);

			_uowFactory.CreateWithoutRoot(
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<int>())
				.Returns(mainUow, docUow);

			return (mainUow, docUow);
		}

		private static WithdrawalEdoTask CreateValidWithdrawalTask(int taskId, int orderId, string orgInn, bool hasWater, bool hasPetFood)
		{
			var client = new CounterpartyEntity
			{
				PersonType = PersonType.legal,
				ReasonForLeaving = ReasonForLeaving.ForOwnNeeds,
				INN = "7811111111"
			};

			var org = new OrganizationEntity
			{
				Id = 1,
				INN = orgInn
			};

			var contract = new CounterpartyContractEntity
			{
				Organization = org
			};

			var order = new TestOrderEntity
			{
				Id = orderId,
				Contract = contract
			};
			order.SetClient(client);
			order.SetPaymentType(PaymentType.Cashless);
			order.SetDeliveryDate(DateTime.Today);

			var baseDocumentTask = new DocumentEdoTask { Id = 888 };

			var request = new WithdrawalEdoRequest
			{
				Id = 50,
				Order = order,
				BaseDocumentEdoTask = baseDocumentTask
			};

			var task = new WithdrawalEdoTask
			{
				Id = taskId,
				FormalEdoRequest = request,
				Status = EdoTaskStatus.New,
				Problems = new ObservableList<EdoTaskProblem>(),
				Items = new ObservableList<EdoTaskItem>()
			};

			if(hasWater)
			{
				var waterNom = new NomenclatureEntity
				{
					Id = 1,
					Name = "Вода 19л",
					IsAccountableInTrueMark = true
				};
				waterNom.Gtins.Add(new GtinEntity { GtinNumber = "04600000000001" });
				order.OrderItems.Add(new TestOrderItemEntity(waterNom, 100m, 1m));

				var identificationCode = new TrueMarkIdentificationCode
				{
					Gtin = "04600000000001",
					SerialNumber = "water1",
					ProductGroup = TrueMarkProductGroup.Water
				};
				var waterProductCode = new AutoTrueMarkProductCode
				{
					SourceCodeStatus = SourceProductCodeStatus.Accepted,
					ResultCode = identificationCode
				};
				task.Items.Add(new EdoTaskItem
				{
					Id = 1,
					ProductCode = waterProductCode
				});
			}

			if(hasPetFood)
			{
				var petFoodNom = new NomenclatureEntity
				{
					Id = 2,
					Name = "Корм для кошек",
					IsAccountableInTrueMark = true
				};
				petFoodNom.Gtins.Add(new GtinEntity { GtinNumber = "04600000000002" });
				order.OrderItems.Add(new TestOrderItemEntity(petFoodNom, 200m, 1m));

				var petFoodCode = new TrueMarkIdentificationCode
				{
					Gtin = "04600000000002",
					SerialNumber = "petfood1",
					ProductGroup = TrueMarkProductGroup.PetFood
				};
				var petFoodProductCode = new AutoTrueMarkProductCode
				{
					SourceCodeStatus = SourceProductCodeStatus.Accepted,
					ResultCode = petFoodCode
				};
				task.Items.Add(new EdoTaskItem
				{
					Id = 2,
					ProductCode = petFoodProductCode
				});
			}

			request.Task = task;

			return task;
		}
	}
}
