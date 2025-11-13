using Edo.Common;
using Edo.Problems;
using Edo.Problems.Custom;
using Edo.Problems.Exception;
using Edo.Problems.Validation;
using Edo.Receipt.Dispatcher;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate.Util;
using NSubstitute;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using TrueMark.Library;
using TrueMarkApi.Client;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Settings.Edo;
using Vodovoz.Settings.Organizations;
using Xunit;

namespace Receipt.Dispatcher.Tests
{
	public class CreateMarkedFiscalDocumentsTests
	{
		private GenericRepositoryFixture<TrueMarkWaterGroupCode> _waterGroupCodeRepository;
		private ForOwnNeedsReceiptEdoTaskHandler _forOwnNeedsReceiptEdoTaskHandler;

		public CreateMarkedFiscalDocumentsTests()
		{
			_waterGroupCodeRepository = new GenericRepositoryFixture<TrueMarkWaterGroupCode>();
			_forOwnNeedsReceiptEdoTaskHandler = CreateForOwnNeedsReceiptEdoTaskHandlerFixture(_waterGroupCodeRepository);
		}

		[Fact]
		public async Task CreateMarkedFiscalDocuments_Should_Create_Equal_Count_Of_FiscalInventPositions_As_OrderItems_Count_And_Have_Correct_Price_For_TrueMarkAccountableItems()
		{
			// Arrange

			var receiptEdoTask = CreateTestReceiptEdoTaskForTest(
				// Nomenclatures
				new (IEnumerable<int> gtinIds, IEnumerable<int> groupGtinIds, bool isAccountableInTrueMark)[]
				{
					(new [] { 1, 2 }, new [] { 1, 2 }, true),
					(new [] { 3, 4 }, new [] { 3, 4 }, true),
					(Array.Empty<int>(), Array.Empty<int>(), false)
				},
				// OrderItems
				new (int nomenclatureId, decimal count, decimal price, decimal discount)[]
				{
					(1, 7m, 20m, 5m),
					(1, 7m, 20m, 5m),
					(3, 1m, 20m, 5m)
				},
				// Identification Codes
				new (bool isInValid, int gtinId)[]
				{
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 2),
					(false, 2),
					(false, 2),
					(false, 2),
					(false, 2),
					(false, 2),
				},
				// Group Codes
				new (int? parentWaterCodeId, int groupGtinId, bool isInValid, IEnumerable<int> childWaterCodeIds)[]
				{
					(null, 1, false, new []{1, 2, 3, 4, 5, 6}),
					(null, 2, false, new []{7, 8, 9, 10, 11, 12})
				},
				// CarLoad Document Items for IdentificationCodes
				new[]
				{
					1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12
				});

			var mainFiscalDocument = new EdoFiscalDocument();

			// Act

			await _forOwnNeedsReceiptEdoTaskHandler.UpdateMarkedFiscalDocuments(receiptEdoTask, mainFiscalDocument, default);

			// Assert

			Assert.Equal(receiptEdoTask.FiscalDocuments.Sum(x => x.InventPositions.Sum(x => x.Quantity)),
				receiptEdoTask.OrderEdoRequest.Order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark).Sum(x => x.Count));

			Assert.Equal(
				receiptEdoTask.FiscalDocuments.Sum(x => x.InventPositions.Sum(x => x.Price * x.Quantity - x.DiscountSum)),
				receiptEdoTask.OrderEdoRequest.Order.OrderItems
					.Where(x =>
						x.Nomenclature.IsAccountableInTrueMark
						&& x.Count > 0)
					.Sum(x => x.Sum));
		}

		// Групповые коды должны распределиться на несколько строк заказа с одной номенклатурой, но разными по стоимости и количеству
		// суммарное количество кодов, входящих в состав групповых равно сумме количества строк заказа
		// количество строк заказа меньше количества групп кодов
		// итоговая стоимость в чеке должна быть как в исходном заказе
		[Fact]
		public async Task CreateMarkedFiscalDocuments_ShoudDistributeGroupCodes_WhenSameItemsOfOneNomenclatureButDifferentPricesExists_AndGroupCodesCountMoreThanOrderItemsCount()
		{
			// Arrange

			var receiptEdoTask = CreateTestReceiptEdoTaskForTest(
				// Nomenclatures
				new (IEnumerable<int> gtinIds, IEnumerable<int> groupGtinIds, bool isAccountableInTrueMark)[]
				{
					(new [] { 1, 2 }, new [] { 1, 2 }, true),
					(Array.Empty<int>(), Array.Empty<int>(), false)
				},
				// OrderItems
				new (int nomenclatureId, decimal count, decimal price, decimal discount)[]
				{
					(1, 4m, 100m, 10m),
					(1, 4m, 140m, 15m),
					(1, 2m, 125m, 5m)
				},
				// Identification Codes
				new (bool isInValid, int gtinId)[]
				{
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
				},
				// Group Codes
				new (int? parentWaterCodeId, int groupGtinId, bool isInValid, IEnumerable<int> childWaterCodeIds)[]
				{
					(null, 1, false, new []{ 1, 2, 3 }),
					(null, 1, false, new []{ 4, 5, 6 }),
					(null, 1, false, new []{ 7, 8, 9 })
				},
				// Edo Task Item Identification Codes
				new[]
				{
					1, 2, 3, 4, 5, 6, 7, 8, 9, 10
				});

			var mainFiscalDocument = new EdoFiscalDocument();

			// Act

			await _forOwnNeedsReceiptEdoTaskHandler.UpdateMarkedFiscalDocuments(receiptEdoTask, mainFiscalDocument, default);

			// Assert
			//Ожидаем 4 строки чека: 3 с групповыми кодами, 1 с индивидуальным
			Assert.Equal(4, receiptEdoTask.FiscalDocuments.SelectMany(x => x.InventPositions).Count());

			Assert.Equal(receiptEdoTask.FiscalDocuments.Sum(x => x.InventPositions.Sum(x => x.Quantity)),
				receiptEdoTask.OrderEdoRequest.Order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark).Sum(x => x.Count));

			Assert.Equal(
				receiptEdoTask.FiscalDocuments.Sum(x => x.InventPositions.Sum(x => x.Price * x.Quantity - x.DiscountSum)),
				receiptEdoTask.OrderEdoRequest.Order.OrderItems
					.Where(x =>
						x.Nomenclature.IsAccountableInTrueMark
						&& x.Count > 0)
					.Sum(x => x.Sum));
		}

		// Проверка, что если не получается распределить группы - все возьмется из пула
		[Fact]
		public async Task CreateMarkedFiscalDocuments_Should_Create_Equal_Count_Of_FiscalInventPositions_As_OrderItems_Count_And_Have_Correct_Price_For_TrueMarkAccountableItems_And_Use_Codes_From_Pool()
		{
			// Arrange

			var receiptEdoTask = CreateTestReceiptEdoTaskForTest(
				// Nomenclatures
				new (IEnumerable<int> gtinIds, IEnumerable<int> groupGtinIds, bool isAccountableInTrueMark)[]
				{
					(new [] { 1, 2 }, new [] { 1, 2 }, true),
					(new [] { 3, 4 }, new [] { 3, 4 }, true),
					(Array.Empty<int>(), Array.Empty<int>(), false)
				},
				// OrderItems
				new[]
				{
					(1, 5m, 2m, 1m),
					(1, 5m, 2m, 1m),
					(3, 1m, 2m, 1m)
				},
				// Identification Codes
				new (bool isInValid, int gtinId)[]
				{
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 2),
					(false, 2),
					(false, 2),
					(false, 2),
					(false, 2),
					(false, 2),
				},
				// Group Codes
				new (int? parentWaterCodeId, int groupGtinId, bool isInValid, IEnumerable<int> childWaterCodeIds)[]
				{
					(null, 1, false, new []{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }),
				},
				// CarLoad Document Items for IdentificationCodes
				new[]
				{
					1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12
				});

			var mainFiscalDocument = new EdoFiscalDocument();

			// Act

			await _forOwnNeedsReceiptEdoTaskHandler.UpdateMarkedFiscalDocuments(receiptEdoTask, mainFiscalDocument, default);

			// Assert

			Assert.Equal(receiptEdoTask.FiscalDocuments.Sum(x => x.InventPositions.Sum(x => x.Quantity)),
				receiptEdoTask.OrderEdoRequest.Order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark).Sum(x => x.Count));

			Assert.Equal(
				receiptEdoTask.FiscalDocuments.Sum(x => x.InventPositions.Sum(x => x.Price * x.Quantity - x.DiscountSum)),
				receiptEdoTask.OrderEdoRequest.Order.OrderItems
					.Where(x =>
						x.Nomenclature.IsAccountableInTrueMark
						&& x.Count > 0)
					.Sum(x => x.Sum));

			Assert.All(
				receiptEdoTask.Items
					.Where(x => x.ProductCode.SourceCode != null)
					.Select(x => x.ProductCode.SourceCode.Gtin),
				x => Assert
					.DoesNotContain(x, receiptEdoTask.FiscalDocuments
					.SelectMany(x => x.InventPositions.Select(x => x.EdoTaskItem.ProductCode.ResultCode.Gtin))));
		}

		private ReceiptEdoTask CreateTestReceiptEdoTaskForTest(
			IEnumerable<(IEnumerable<int> gtinIds, IEnumerable<int> groupGtinIds, bool isAccountableInTrueMark)> nomenclaturesParameters,
			IEnumerable<(int nomenclatureId, decimal count, decimal price, decimal discount)> orderItemsParameters,
			IEnumerable<(bool isValid, int gtinId)> waterIdentificationCodesParameters,
			IEnumerable<(int? parentWaterCodeId, int groupGtinId, bool isInValid, IEnumerable<int> childWaterCodeIds)> waterGroupCodesParameters,
			IEnumerable<int> edoTaskItemIdentificationCodesParameters)
		{
			var orderItemAutoIncrementId = 1;
			var nomenclatureAutoIncrementId = 1;
			var gtinAutoIncrementId = 1;
			var groupGtinAutoIncrementId = 1;
			var carLoadDocumentItemAutoIncrementId = 1;
			var edoTaskItemAutoIncrementId = 1;
			var carLoadDocumentItemTrueMarkProductCodeAutoIncrementId = 1;
			var waterIdentificationCodeAutoIncrementId = 1;
			var waterGroupCodeAutoIncrementId = 1;

			var gtins = new List<GtinEntity>();

			var maxGtinId = nomenclaturesParameters.Max(x => x.gtinIds.Any() ? x.gtinIds.Max() : 0);

			for(var i = 1; i < maxGtinId; i++)
			{
				gtins.Add(CreateGtin(gtinAutoIncrementId++));
			}

			var groupGtins = new List<GroupGtinEntity>();

			var maxGroupGtinId = nomenclaturesParameters.Max(x => x.groupGtinIds.Any() ? x.groupGtinIds.Max() : 0);

			for(var i = 1; i < maxGroupGtinId; i++)
			{
				groupGtins.Add(CreateGroupGtin(groupGtinAutoIncrementId++));
			}

			var nomenclatures = new List<NomenclatureEntity>();

			foreach(var (gtinIds, groupGtinIds, isAccountableInTrueMark) in nomenclaturesParameters)
			{
				nomenclatures.Add(CreateNomenclature(
					nomenclatureAutoIncrementId++,
					isAccountableInTrueMark,
					gtins.Where(x => gtinIds.Contains(x.Id)),
					groupGtins.Where(x => groupGtinIds.Contains(x.Id))));
			}

			var orderItems = new List<OrderItemEntity>();

			foreach(var (nomenclatureId, count, price, discount) in orderItemsParameters)
			{
				orderItems.Add(CreateOrderItem(orderItemAutoIncrementId++, nomenclatures[nomenclatureId - 1], count, price, discount));
			}

			var order = CreateOrderForTest(orderItems);

			var receiptEdoTask = new ReceiptEdoTask
			{
				OrderEdoRequest = new OrderEdoRequest
				{
					Order = order
				},
			};

			var trueMarkWaterIdentificationCodes = new List<TrueMarkWaterIdentificationCode>();

			foreach((var isValid, var gtinId) in waterIdentificationCodesParameters)
			{
				trueMarkWaterIdentificationCodes.Add(CreateTrueMarkWaterIdentificationCode(waterIdentificationCodeAutoIncrementId++, CreateNewGtinCode(gtinId), isValid));
			}

			foreach(var waterIdentificationCodeId in edoTaskItemIdentificationCodesParameters)
			{
				receiptEdoTask.Items.Add(CreateEdoTaskItem(
					edoTaskItemAutoIncrementId++,
					carLoadDocumentItemTrueMarkProductCodeAutoIncrementId++,
					carLoadDocumentItemAutoIncrementId++,
					order.Id,
					trueMarkWaterIdentificationCodes[waterIdentificationCodeId - 1]));
			}

			var trueMarkWaterGroupCodes = new List<TrueMarkWaterGroupCode>();

			var groupCodesCounter = 1;

			foreach((int? parentWaterCodeId, int groupGtinId, bool isInValid, IEnumerable<int> childWaterCodeIds) in waterGroupCodesParameters.OrderBy(x => x.parentWaterCodeId))
			{
				var groupCode = new TrueMarkWaterGroupCode
				{
					Id = waterGroupCodeAutoIncrementId++,
					IsInvalid = isInValid,
					GTIN = CreateNewGroupGtinCode(groupGtinId),
				};

				if(parentWaterCodeId != null)
				{
					trueMarkWaterGroupCodes[parentWaterCodeId.Value - 1].AddInnerGroupCode(groupCode);
				}

				foreach(var childId in childWaterCodeIds)
				{
					var currentWaterIdentificationCode = trueMarkWaterIdentificationCodes[childId - 1];

					if(groupCode.GTIN == null)
					{
						groupCode.GTIN = currentWaterIdentificationCode.Gtin;
					}

					groupCode.RawCode ??= $"{groupCode.GTIN}Raw{groupCodesCounter++}";

					groupCode.AddInnerWaterCode(currentWaterIdentificationCode);
				}

				trueMarkWaterGroupCodes.Add(groupCode);
				_waterGroupCodeRepository.Data.Add(groupCode);
			}

			return receiptEdoTask;
		}

		private EdoTaskItem CreateEdoTaskItem(
			int edoTaskItemAutoIncrementId,
			int carLoadDocumentItemTrueMarkProductCodeAutoIncrementId,
			int carLoadDocumentItemAutoIncrementId,
			int orderId,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode)
		{
			var sourceCode = trueMarkWaterIdentificationCode;

			return new EdoTaskItem
			{
				Id = edoTaskItemAutoIncrementId,
				ProductCode = new CarLoadDocumentItemTrueMarkProductCode
				{
					Id = carLoadDocumentItemTrueMarkProductCodeAutoIncrementId,
					CarLoadDocumentItem = new CarLoadDocumentItemEntity
					{
						Id = carLoadDocumentItemAutoIncrementId,
						OrderId = orderId
					},
					SourceCode = sourceCode
				}
			};
		}

		private GtinEntity CreateGtin(int id)
		{
			return new GtinEntity
			{
				Id = id,
				GtinNumber = CreateNewGtinCode(id)
			};
		}

		private GroupGtinEntity CreateGroupGtin(int id)
		{
			return new GroupGtinEntity
			{
				Id = id,
				GtinNumber = CreateNewGroupGtinCode(id)
			};
		}

		private TrueMarkWaterIdentificationCode CreateTrueMarkWaterIdentificationCode(int id, string gtin, bool isInvalid)
		{
			return new TrueMarkWaterIdentificationCode
			{
				Id = id,
				Gtin = gtin,
				SerialNumber = CreateNewGtinSerial(id),
				CheckCode = CreateNewGtinCheckCode(id),
				IsInvalid = isInvalid
			};
		}

		private string CreateNewGtinCode(int id) => $"Gtin#Test#{id}";
		private string CreateNewGroupGtinCode(int id) => $"GroupGtin#Test#{id}";
		private string CreateNewGtinSerial(int id) => $"Gtin#Test.Serial#{id}";
		private string CreateNewGtinCheckCode(int id) => $"Gtin#Test.CheckCode#{id}";

		private OrderEntity CreateOrderForTest(IEnumerable<OrderItemEntity> orderItems)
		{
			var order = new OrderEntity
			{
				Id = 1,
				Contract = new CounterpartyContractEntity()
			};

			foreach(var orderItem in orderItems)
			{
				orderItem.Order = order;

				order.OrderItems.Add(orderItem);
			}

			return order;
		}

		private NomenclatureEntity CreateNomenclature(int id, bool isAccountableInTrueMark, IEnumerable<GtinEntity> gtins, IEnumerable<GroupGtinEntity> groupGtins)
		{
			var nomenclature = new NomenclatureEntity
			{
				Id = id,
				IsAccountableInTrueMark = isAccountableInTrueMark,
				Gtins = new ObservableList<GtinEntity>(),
				GroupGtins = new ObservableList<GroupGtinEntity>()
			};

			foreach(var gtin in gtins)
			{
				gtin.Nomenclature = nomenclature;
				nomenclature.Gtins.Add(gtin);
			}

			foreach(var groupGtin in groupGtins)
			{
				groupGtin.Nomenclature = nomenclature;
				nomenclature.GroupGtins.Add(groupGtin);
			}

			return nomenclature;
		}

		private OrderItemEntity CreateOrderItem(int id, NomenclatureEntity nomenclature, decimal count, decimal price, decimal discount)
		{
			var orderItem = new OrderItemEntityFixture()
			{
				Id = id,
				Nomenclature = nomenclature
			};

			orderItem.SetCount(count);
			orderItem.SetPrice(price);
			orderItem.SetMoneyDiscount(discount);

			return orderItem;
		}

		private ForOwnNeedsReceiptEdoTaskHandler CreateForOwnNeedsReceiptEdoTaskHandlerFixture(
			IGenericRepository<TrueMarkWaterGroupCode> waterGroupCodeRepository,
			IGenericRepository<TrueMarkProductCode> productCodeRepository = null)
		{
			var logger = Substitute.For<ILogger<ForOwnNeedsReceiptEdoTaskHandler>>();
			var unitOfWork = Substitute.For<IUnitOfWork>();
			var unitOfWorkFactory = Substitute.For<IUnitOfWorkFactory>();
			var edoRepository = Substitute.For<IEdoRepository>();
			var httpClientFactory = Substitute.For<IHttpClientFactory>();
			var edoProblemRegistrar = CreateEdoProblemRegistrarFixture(unitOfWork, unitOfWorkFactory);
			var edoTaskValidator = CreateEdoTaskValidatorFixture(unitOfWorkFactory, edoProblemRegistrar);
			var edoTaskTrueMarkCodeCheckerFactory = Substitute.For<EdoTaskItemTrueMarkStatusProviderFactory>();
			var transferRequestCreator = CreateTransferRequestCreatorFixture(edoRepository);
			var edoReceiptSettings = Substitute.For<IEdoReceiptSettings>();
			var localCodesValidator = CreateTrueMarkTaskCodesValidatorFixture(edoRepository, Substitute.For<TrueMarkApiClient>());
			var tag1260Checker = CreateTag1260CheckerFixture(httpClientFactory);
			var trueMarkCodeRepository = Substitute.For<ITrueMarkCodeRepository>();
			var saveCodesService = Substitute.For<ISaveCodesService>();
			var bus = Substitute.For<IBus>();

			return new ForOwnNeedsReceiptEdoTaskHandler(
				logger,
				unitOfWork,
				edoTaskValidator,
				edoProblemRegistrar,
				edoTaskTrueMarkCodeCheckerFactory,
				transferRequestCreator,
				edoRepository,
				edoReceiptSettings,
				localCodesValidator,
				Substitute.For<ITrueMarkCodesPool>() as ReceiptTrueMarkCodesPool, 
				tag1260Checker,
				trueMarkCodeRepository,
				productCodeRepository ?? Substitute.For<IGenericRepository<TrueMarkProductCode>>(),
				Substitute.For<IEdoOrderContactProvider>(),
				saveCodesService,
				Substitute.For<IOrganizationSettings>(),
				bus);
		}

		private EdoTaskValidator CreateEdoTaskValidatorFixture(IUnitOfWorkFactory unitOfWorkFactory, EdoProblemRegistrar edoProblemRegistrar)
		{
			var logger = Substitute.For<ILogger<EdoTaskValidator>>();
			var edoTaskValidatorsProvider = CreateEdoTaskValidatorsProviderFixture(CreateEdoTaskValidatorsPersisterFixture(unitOfWorkFactory));
			var serviceProvider = Substitute.For<IServiceProvider>();

			return new EdoTaskValidator(logger, edoTaskValidatorsProvider, serviceProvider, edoProblemRegistrar);
		}

		private EdoProblemRegistrar CreateEdoProblemRegistrarFixture(IUnitOfWork unitOfWork, IUnitOfWorkFactory unitOfWorkFactory)
		{
			var customSourcesPersister = CreateEdoTaskCustomSourcesPersisterFixture(unitOfWorkFactory);

			var exceptionSourcesPersister = CreateEdoTaskExceptionSourcesPersisterFixture(unitOfWorkFactory);

			return new EdoProblemRegistrar(unitOfWork, unitOfWorkFactory, customSourcesPersister, exceptionSourcesPersister);
		}

		private EdoTaskCustomSourcesPersister CreateEdoTaskCustomSourcesPersisterFixture(IUnitOfWorkFactory unitOfWorkFactory)
		{
			return new EdoTaskCustomSourcesPersister(unitOfWorkFactory, Enumerable.Empty<EdoTaskProblemCustomSource>());
		}

		private EdoTaskExceptionSourcesPersister CreateEdoTaskExceptionSourcesPersisterFixture(IUnitOfWorkFactory unitOfWorkFactory)
		{
			return new EdoTaskExceptionSourcesPersister(unitOfWorkFactory, Enumerable.Empty<EdoTaskProblemExceptionSource>());
		}

		private EdoTaskValidatorsProvider CreateEdoTaskValidatorsProviderFixture(EdoTaskValidatorsPersister edoTaskValidatorsPersister)
		{
			return new EdoTaskValidatorsProvider(edoTaskValidatorsPersister);
		}

		private EdoTaskValidatorsPersister CreateEdoTaskValidatorsPersisterFixture(IUnitOfWorkFactory unitOfWorkFactory)
		{
			return new EdoTaskValidatorsPersister(unitOfWorkFactory, Enumerable.Empty<IEdoTaskValidator>());
		}

		private TransferRequestCreator CreateTransferRequestCreatorFixture(IEdoRepository edoRepository)
		{
			return new TransferRequestCreator(edoRepository);
		}

		private TrueMarkTaskCodesValidator CreateTrueMarkTaskCodesValidatorFixture(IEdoRepository edoRepository, ITrueMarkApiClient trueMarkApiClient)
		{
			return new TrueMarkTaskCodesValidator(edoRepository, trueMarkApiClient);
		}

		private TrueMarkCodesPool CreateTrueMarkCodesPoolFixture(IUnitOfWork unitOfWork)
		{
			return new TrueMarkCodesPoolFixture(unitOfWork);
		}

		private Tag1260Checker CreateTag1260CheckerFixture(IHttpClientFactory httpClientFactory)
		{
			return new Tag1260Checker(httpClientFactory);
		}

		[Fact]
		public void CheckProductCodesForDuplicatesAndUpdateIfNeed_ShouldUpdateProductCode_WhenDuplicatesExist()
		{
			// Arrange
			var productCodes = new List<TrueMarkProductCode>
			{
				new RouteListItemTrueMarkProductCode { Id = 1, SourceCode = new TrueMarkWaterIdentificationCode { Id = 11 }, ResultCode = new TrueMarkWaterIdentificationCode{Id = 11 } },
				new RouteListItemTrueMarkProductCode { Id = 2, SourceCode = new TrueMarkWaterIdentificationCode{ Id = 11 } },
				new RouteListItemTrueMarkProductCode { Id = 3, SourceCode = new TrueMarkWaterIdentificationCode{ Id = 12 }, ResultCode = new TrueMarkWaterIdentificationCode{Id = 11 } }
			};

			var mockWaterGroupCodeRepository = Substitute.For<IGenericRepository<TrueMarkWaterGroupCode>>();
			var mockProductCodeRepository = Substitute.For<IGenericRepository<TrueMarkProductCode>>();
			
			mockProductCodeRepository
				.Get(Arg.Any<IUnitOfWork>(), Arg.Any<Expression<Func<TrueMarkProductCode, bool>>>(), Arg.Any<int>())
				.Returns(new List<TrueMarkProductCode>()
				{
					new RouteListItemTrueMarkProductCode { Id = 11, SourceCode = new TrueMarkWaterIdentificationCode { Id = 10 }, ResultCode = new TrueMarkWaterIdentificationCode{ Id = 11 } },
					new RouteListItemTrueMarkProductCode { Id = 12, SourceCode = new TrueMarkWaterIdentificationCode{ Id = 10 }, ResultCode = new TrueMarkWaterIdentificationCode{ Id = 12 } },
					new RouteListItemTrueMarkProductCode { Id = 13, SourceCode = new TrueMarkWaterIdentificationCode{ Id = 13 }, ResultCode = new TrueMarkWaterIdentificationCode{ Id = 11 } }
				});


			var handler = CreateForOwnNeedsReceiptEdoTaskHandlerFixture(mockWaterGroupCodeRepository, mockProductCodeRepository);

			// Act
			handler.CheckProductCodesForDuplicatesAndUpdateIfNeed(productCodes);

			// Assert
			Assert.Equal(SourceProductCodeStatus.Problem, productCodes[0].SourceCodeStatus);
			Assert.Equal(ProductCodeProblem.Duplicate, productCodes[0].Problem);
			Assert.Equal(2, productCodes[0].DuplicatesCount);
			Assert.Null(productCodes[0].ResultCode);

			Assert.Equal(SourceProductCodeStatus.Problem, productCodes[1].SourceCodeStatus);
			Assert.Equal(ProductCodeProblem.Duplicate, productCodes[1].Problem);
			Assert.Equal(2, productCodes[1].DuplicatesCount);
			Assert.Null(productCodes[1].ResultCode);

			Assert.Equal(SourceProductCodeStatus.Problem, productCodes[2].SourceCodeStatus);
			Assert.Equal(ProductCodeProblem.Duplicate, productCodes[2].Problem);
			Assert.Equal(1, productCodes[2].DuplicatesCount);
			Assert.Null(productCodes[2].ResultCode);
		}

		[Fact]
		public void CheckProductCodesForDuplicatesAndUpdateIfNeed_ShouldNotUpdateProductCode_WhenNoDuplicatesExist()
		{
			// Arrange
			var productCodes = new List<TrueMarkProductCode>
			{
				new RouteListItemTrueMarkProductCode { Id = 1, SourceCode = new TrueMarkWaterIdentificationCode { Id = 10 }, ResultCode = new TrueMarkWaterIdentificationCode{Id = 11} },
				new RouteListItemTrueMarkProductCode { Id = 2, SourceCode = new TrueMarkWaterIdentificationCode { Id = 12 } },
				new RouteListItemTrueMarkProductCode { Id = 3, SourceCode = new TrueMarkWaterIdentificationCode{ Id = 13 }, ResultCode = new TrueMarkWaterIdentificationCode{Id = 13 } },
			};

			var mockWaterGroupCodeRepository = Substitute.For<IGenericRepository<TrueMarkWaterGroupCode>>();
			var mockProductCodeRepository = Substitute.For<IGenericRepository<TrueMarkProductCode>>();

			mockProductCodeRepository
				.Get(Arg.Any<IUnitOfWork>(), Arg.Any<Expression<Func<TrueMarkProductCode, bool>>>(), Arg.Any<int>())
				.Returns(new List<TrueMarkProductCode>()
				{
					new RouteListItemTrueMarkProductCode { Id = 1, SourceCode = new TrueMarkWaterIdentificationCode { Id = 10 }, ResultCode = new TrueMarkWaterIdentificationCode{Id = 11} },
					new RouteListItemTrueMarkProductCode { Id = 12, SourceCode = new TrueMarkWaterIdentificationCode{ Id = 10 }, ResultCode = new TrueMarkWaterIdentificationCode{ Id = 14 } },
					new RouteListItemTrueMarkProductCode { Id = 13, SourceCode = new TrueMarkWaterIdentificationCode{ Id = 13 }, ResultCode = new TrueMarkWaterIdentificationCode{ Id = 15 } }
				});

			var handler = CreateForOwnNeedsReceiptEdoTaskHandlerFixture(mockWaterGroupCodeRepository, mockProductCodeRepository);

			// Act
			handler.CheckProductCodesForDuplicatesAndUpdateIfNeed(productCodes);

			// Assert
			Assert.Equal(SourceProductCodeStatus.New, productCodes[0].SourceCodeStatus);
			Assert.Equal(ProductCodeProblem.None, productCodes[0].Problem);
			Assert.Equal(0, productCodes[0].DuplicatesCount);
			Assert.NotNull(productCodes[0].ResultCode);

			Assert.Equal(SourceProductCodeStatus.New, productCodes[1].SourceCodeStatus);
			Assert.Equal(ProductCodeProblem.None, productCodes[1].Problem);
			Assert.Equal(0, productCodes[1].DuplicatesCount);
			Assert.Null(productCodes[1].ResultCode);

			Assert.Equal(SourceProductCodeStatus.New, productCodes[2].SourceCodeStatus);
			Assert.Equal(ProductCodeProblem.None, productCodes[2].Problem);
			Assert.Equal(0, productCodes[2].DuplicatesCount);
			Assert.NotNull(productCodes[2].ResultCode);
		}

		[Fact]
		public void GetProductCodesWithoutConsolidatedIdentificationCodes_ShouldReturnProductCodes_WhichNotContainsConsolidatedIdentificationCodes()
		{
			// Arrange
			var edoTaskItems = new List<EdoTaskItem>
			{
				new()
				{
					ProductCode = new RouteListItemTrueMarkProductCode
					{
						SourceCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = 1 },
						ResultCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = 2 }
					}
				},
				new()
				{
					ProductCode = new RouteListItemTrueMarkProductCode
					{
						SourceCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = null, ParentTransportCodeId = null },
						ResultCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = null, ParentTransportCodeId = null }
					}
				},
				new()
				{
					ProductCode = new RouteListItemTrueMarkProductCode
					{
						SourceCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = null, ParentTransportCodeId = null },
						ResultCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = 3 }
					}
				},
				new()
				{
					ProductCode = new RouteListItemTrueMarkProductCode
					{
						SourceCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = null, ParentTransportCodeId = null },
						ResultCode = null
					}
				},
				new()
				{
					ProductCode = new RouteListItemTrueMarkProductCode
					{
						SourceCode = new TrueMarkWaterIdentificationCode { ParentTransportCodeId = 3 },
						ResultCode = null
					}
				},
				new()
				{
					ProductCode = new RouteListItemTrueMarkProductCode
					{
						SourceCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = null, ParentTransportCodeId = null },
						ResultCode = new TrueMarkWaterIdentificationCode { ParentTransportCodeId = 3 }
					}
				},
			};

			// Act
			var result = _forOwnNeedsReceiptEdoTaskHandler.GetProductCodesWithoutConsolidatedIdentificationCodes(edoTaskItems);

			// Assert
			Assert.Equal(2, result.Count);
			Assert.Contains(result, x => x.Id == edoTaskItems[1].ProductCode.Id);
			Assert.Contains(result, x => x.Id == edoTaskItems[3].ProductCode.Id);
		}
	}
}
