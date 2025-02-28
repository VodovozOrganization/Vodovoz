using Edo.Common;
using Edo.Problems;
using Edo.Problems.Custom;
using Edo.Problems.Exception;
using Edo.Problems.Validation;
using Edo.Receipt.Dispatcher;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Linq;
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
		public async Task Test1Async()
		{
			// Arrange

			var order = new OrderEntity
			{
				Id = 1
			};

			var orderItem = new OrderItemEntityFixture()
			{
				Nomenclature = new NomenclatureEntity
				{
					IsAccountableInTrueMark = true,
					Gtins = new ObservableList<GtinEntity>()
				},
			};

			orderItem.Nomenclature.Gtins.Add(new GtinEntity
			{
				Id = 1,
				GtinNumber = "Gtin#Test#1",
				Nomenclature = orderItem.Nomenclature,
			});

			orderItem.SetCount(2m);
			orderItem.SetPrice(10m);
			orderItem.SetDiscount(1m);
			orderItem.Order = order;

			order.OrderItems.Add(orderItem);
			order.Contract = new CounterpartyContractEntity();

			var receiptEdoTask = new ReceiptEdoTask
			{
				OrderEdoRequest = new OrderEdoRequest
				{
					Order = order
				},
			};

			var sourceCode = new TrueMarkWaterIdentificationCode
			{
				Id = 1,
				GTIN = "Gtin#Test#1",
				SerialNumber = "Gtin#Test.Serial#1",
				CheckCode = "Gtin#Test.CheckCode#1",
				IsInvalid = false
			};

			receiptEdoTask.Items.Add(new EdoTaskItem
			{
				Id = 1,
				ProductCode = new CarLoadDocumentItemTrueMarkProductCode
				{
					Id = 1,
					CarLoadDocumentItem = new CarLoadDocumentItemEntity
					{
						Id = 1,
						OrderId = order.Id
					},
					SourceCode = sourceCode
				}
			});

			var mainFiscalDocument = new EdoFiscalDocument();

			// Act

			await _forOwnNeedsReceiptEdoTaskHandler.CreateMarkedFiscalDocuments(receiptEdoTask, mainFiscalDocument, default);

			// Assert

			
		}

		private ForOwnNeedsReceiptEdoTaskHandler CreateForOwnNeedsReceiptEdoTaskHandlerFixture(IGenericRepository<TrueMarkWaterGroupCode> waterGroupCodeRepository)
		{
			var logger = Substitute.For<ILogger<ForOwnNeedsReceiptEdoTaskHandler>>();
			var unitOfWork = Substitute.For<IUnitOfWork>();
			var unitOfWorkFactory = Substitute.For<IUnitOfWorkFactory>();
			var edoRepository = Substitute.For<IEdoRepository>();
			var httpClientFactory = Substitute.For<IHttpClientFactory>();
			var edoProblemRegistrar = CreateEdoProblemRegistrarFixture(unitOfWork, unitOfWorkFactory);
			var edoTaskValidator = CreateEdoTaskValidatorFixture(unitOfWorkFactory, edoProblemRegistrar);
			var edoTaskTrueMarkCodeCheckerFactory = CreateEdoTaskItemTrueMarkStatusProviderFactoryFixture(new TrueMarkClientFactoryFixture());
			var transferRequestCreator = CreateTransferRequestCreatorFixture(edoRepository);
			var edoReceiptSettings = Substitute.For<IEdoReceiptSettings>();
			var localCodesValidator = CreateTrueMarkTaskCodesValidatorFixture(edoRepository);
			var trueMarkCodesPool = CreateTrueMarkCodesPoolFixture(unitOfWork);
			var tag1260Checker = CreateTag1260CheckerFixture(httpClientFactory);
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
				trueMarkCodesPool,
				tag1260Checker,
				waterGroupCodeRepository,
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

		private EdoTaskItemTrueMarkStatusProviderFactory CreateEdoTaskItemTrueMarkStatusProviderFactoryFixture(ITrueMarkApiClientFactory trueMarkApiClientFactory)
		{
			return new EdoTaskItemTrueMarkStatusProviderFactory(trueMarkApiClientFactory);
		}

		private TransferRequestCreator CreateTransferRequestCreatorFixture(IEdoRepository edoRepository)
		{
			return new TransferRequestCreator(edoRepository);
		}

		private TrueMarkTaskCodesValidator CreateTrueMarkTaskCodesValidatorFixture(IEdoRepository edoRepository)
		{
			return new TrueMarkTaskCodesValidator(edoRepository);
		}

		private TrueMarkCodesPool CreateTrueMarkCodesPoolFixture(IUnitOfWork unitOfWork)
		{
			return new TrueMarkCodesPoolFixture(unitOfWork);
		}

		private Tag1260Checker CreateTag1260CheckerFixture(IHttpClientFactory httpClientFactory)
		{
			return new Tag1260Checker(httpClientFactory);
		}
	}
}
