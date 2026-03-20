using Edo.Common;
using Edo.Problems;
using Edo.Problems.Custom.Sources.Withdrawal;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMark.Contracts.Documents;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Settings.Edo;
using VodovozBusiness.EntityRepositories.Edo;
using IEdoRepository = Vodovoz.Core.Data.Repositories.IEdoRepository;

namespace Edo.Withdrawal
{
	public class WithdrawalTaskCreatedHandler
	{
		private readonly ILogger<WithdrawalTaskCreatedHandler> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ITrueMarkApiClient _trueMarkApiClient;
		private readonly IEdoDocflowRepository _edoDocflowRepository;
		private readonly ICounterpartyEdoAccountEntityController _edoAccountEntityController;
		private readonly IGenericRepository<TrueMarkDocument> _trueMarkDocumentRepository;
		private readonly ITrueMarkCodesValidator _trueMarkTaskCodesValidator;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly IEdoSettings _edoSettings;
		private readonly IEdoRepository _edoRepository;

		private EdoTaskStatus[] _availiableEdoTaskStatuses => new[]
		{
			EdoTaskStatus.New,
			EdoTaskStatus.Waiting,
			EdoTaskStatus.Problem
		};

		public WithdrawalTaskCreatedHandler(
			ILogger<WithdrawalTaskCreatedHandler> logger,
			IHttpClientFactory httpClientFactory,
			IUnitOfWorkFactory uowFactory,
			ITrueMarkApiClient trueMarkApiClient,
			IEdoDocflowRepository edoDocflowRepository,
			ICounterpartyEdoAccountEntityController edoAccountEntityController,
			IGenericRepository<TrueMarkDocument> trueMarkDocumentRepository,
			ITrueMarkCodesValidator trueMarkTaskCodesValidator,
			EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
			EdoProblemRegistrar edoProblemRegistrar,
			IEdoSettings edoSettings,
			IEdoRepository edoRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_trueMarkApiClient = trueMarkApiClient ?? throw new ArgumentNullException(nameof(trueMarkApiClient));
			_edoDocflowRepository = edoDocflowRepository ?? throw new ArgumentNullException(nameof(edoDocflowRepository));
			_edoAccountEntityController = edoAccountEntityController ?? throw new ArgumentNullException(nameof(edoAccountEntityController));
			_trueMarkDocumentRepository = trueMarkDocumentRepository ?? throw new ArgumentNullException(nameof(trueMarkDocumentRepository));
			_trueMarkTaskCodesValidator = trueMarkTaskCodesValidator ?? throw new ArgumentNullException(nameof(trueMarkTaskCodesValidator));
			_edoTaskTrueMarkCodeCheckerFactory = edoTaskTrueMarkCodeCheckerFactory ?? throw new ArgumentNullException(nameof(edoTaskTrueMarkCodeCheckerFactory));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
		}

		public async Task HandleWithdrawal(int withdrawalEdoTaskId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot(nameof(WithdrawalTaskCreatedHandler)))
			{
				var withdrawalEdoTask = await uow.Session.GetAsync<WithdrawalEdoTask>(withdrawalEdoTaskId, cancellationToken);

				if(withdrawalEdoTask is null)
				{
					throw new InvalidOperationException(
						$"Задача вывода из оборота с Id {withdrawalEdoTaskId} не найдена. Вывод из оборота невозможен");
				}

				var withdrawalEdoRequest = withdrawalEdoTask.FormalEdoRequest as WithdrawalEdoRequest;
				var order = withdrawalEdoRequest?.Order;

				if(order == null)
				{
					throw new InvalidOperationException(
						$"Для задачи вывода из оборота с Id {withdrawalEdoTaskId} не найден заказ. Вывод из оборота невозможен");
				}

				var client = order.Client;

				if(!_availiableEdoTaskStatuses.Contains(withdrawalEdoTask.Status))
				{
					_logger.LogInformation(
						"Задача вывода из оборота с Id {WithdrawalEdoTaskId} имеет статус {EdoTaskStatus}, который не позволяет обработать задачу. " +
						"Статус задачи должен быть одним из следующих: {AvailableStatuses}",
						withdrawalEdoTask.Id,
						withdrawalEdoTask.Status,
						string.Join(", ", _availiableEdoTaskStatuses));
					return;
				}

				if(client.PersonType != PersonType.legal)
				{
					await _edoProblemRegistrar.RegisterCustomProblem<WithdrawalCanBeCreatedOnlyForLegalPersons>(
						withdrawalEdoTask,
						Enumerable.Empty<EdoTaskItem>(),
						cancellationToken);
					return;
				}

				if(order.PaymentType != Vodovoz.Domain.Client.PaymentType.Cashless)
				{
					await _edoProblemRegistrar.RegisterCustomProblem<WithdrawalCanBeCreatedOnlyForCashlessOrders>(
						withdrawalEdoTask,
						Enumerable.Empty<EdoTaskItem>(),
						cancellationToken);
					return;
				}

				if(client.ReasonForLeaving != ReasonForLeaving.ForOwnNeeds)
				{
					await _edoProblemRegistrar.RegisterCustomProblem<WithdrawalCanBeCreatedOnlyForOwnNeedsOrders>(
						withdrawalEdoTask,
						Enumerable.Empty<EdoTaskItem>(),
						cancellationToken);
					return;
				}

				var orderOrganizationId = order.Contract.Organization.Id;
				var orderOrganizationInn = order.Contract.Organization.INN;

				var edoAccount =
					_edoAccountEntityController.GetDefaultCounterpartyEdoAccountByOrganizationId(client, orderOrganizationId);

				if(edoAccount.ConsentForEdoStatus == ConsentForEdoStatus.Agree
					&& CounterpartyEntity.RegisteredInTrueMarkStatuses.Contains(client.RegistrationInChestnyZnakStatus))
				{
					var canCreateWithdrawal =
						CanCreateWithdrawalForRegisteredInTrueMarkClientOrder(uow, withdrawalEdoRequest, order, client);

					if(!canCreateWithdrawal)
					{
						await _edoProblemRegistrar.RegisterCustomProblem<WithdrawalCanNotBeCreatedForRegisteredInTrueMarkClient>(
							withdrawalEdoTask,
							Enumerable.Empty<EdoTaskItem>(),
							cancellationToken);
						return;
					}
				}

				var isTrueMarkDocumentExists = await IsTrueMarkWithdrawalDocumentExists(uow, order.Id, cancellationToken);

				if(isTrueMarkDocumentExists)
				{
					await _edoProblemRegistrar.RegisterCustomProblem<WithdrawalDocumentForOrderExists>(
						withdrawalEdoTask,
						Enumerable.Empty<EdoTaskItem>(),
						cancellationToken);
					return;
				}

				try
				{
					var trueMarkCodesChecker =
						_edoTaskTrueMarkCodeCheckerFactory.Create(withdrawalEdoTask);
					var codesValidationResult =
						await _trueMarkTaskCodesValidator.ValidateAsync(withdrawalEdoTask, trueMarkCodesChecker, cancellationToken);

					if(!codesValidationResult.IsAllValid || !codesValidationResult.ReadyToSell)
					{
						var invalidTaskItems =
							codesValidationResult.CodeResults
							.Where(x => !x.IsValid)
							.Select(x => x.EdoTaskItem);

						await _edoProblemRegistrar.RegisterCustomProblem<WithdrawalTaskHasInvalidCodes>(
							withdrawalEdoTask,
							invalidTaskItems,
							cancellationToken);

						return;
					}

					await CreateTrueMarkDocument(uow, withdrawalEdoTask, order, orderOrganizationInn, cancellationToken);
					await SetCompletedWithdrawalEdoTaskStatus(uow, withdrawalEdoTask, cancellationToken);

					await uow.CommitAsync(cancellationToken);
				}
				catch(Exception ex)
				{
					var isProblemRegistered = await _edoProblemRegistrar.TryRegisterExceptionProblem(withdrawalEdoTask, ex, cancellationToken);
					if(!isProblemRegistered)
					{
						throw;
					}
				}
			}
		}

		private bool CanCreateWithdrawalForRegisteredInTrueMarkClientOrder(IUnitOfWork uow, WithdrawalEdoRequest withdrawalEdoRequest, OrderEntity order, CounterpartyEntity client)
		{
			var documents = _edoRepository.GetOrderEdoDocumentsByOrderId(uow, order.Id);

			var orderEdoDocument =
				documents
				.Where(x => x.DocumentTaskId == withdrawalEdoRequest.BaseDocumentEdoTask.Id)
				.FirstOrDefault();

			if(orderEdoDocument?.CreationTime == null)
			{
				_logger.LogWarning(
					"От клиента {ClientId} получено согласие на ЭДО и клиент зарегистрирован в ЧЗ. " +
					"Время отправки документа не найдено. Вывод из оборота невозможен",
					client.Id);

				return false;
			}

			var daysSinceSend = (DateTime.Today - orderEdoDocument.CreationTime.Date).TotalDays;
			var timeoutDays = _edoSettings.ConnectedTrueMarkClientsWithdrawalDocflowTimeoutDays;

			if(daysSinceSend < timeoutDays)
			{
				_logger.LogInformation(
					"От клиента {ClientId} получено согласие на ЭДО и клиент зарегистрирован в ЧЗ. " +
					"Документооборот не превысил таймаут в {Days} дней (прошло {DaysSinceSend} дней). " +
					"Вывод из оборота невозможен",
					client.Id,
					timeoutDays,
					daysSinceSend);

				return false;
			}

			_logger.LogInformation(
				"Клиент {ClientId} зарегистрирован в ЭДО и ЧЗ, документооборот превысил таймаут в {Days} дней. Вывод из оборота разрешён",
				client.Id,
				timeoutDays);

			return true;
		}

		private async Task<bool> IsTrueMarkWithdrawalDocumentExists(IUnitOfWork uow, int orderId, CancellationToken cancellationToken)
		{
			var existingDocuments = await _trueMarkDocumentRepository
				.GetAsync(
					uow,
					x => x.Order.Id == orderId && x.Type == TrueMarkDocument.TrueMarkDocumentType.Withdrawal,
					cancellationToken: cancellationToken);

			return existingDocuments.Value.Any();
		}

		private async Task<IEnumerable<ProductInstanceStatus>> GetCodesInstanceStatuses(
			WithdrawalEdoTask withdrawalEdoTask,
			CancellationToken cancellationToken)
		{
			var codesInOrder = withdrawalEdoTask.Items
				.Select(x => x.ProductCode)
				.Where(x => x.SourceCodeStatus == SourceProductCodeStatus.Accepted || x.SourceCodeStatus == SourceProductCodeStatus.Changed)
				.Select(x => x.ResultCode.IdentificationCode)
				.ToList();

			var productInstancesInfo = await _trueMarkApiClient.GetProductInstanceInfoAsync(codesInOrder, cancellationToken);

			if(productInstancesInfo?.InstanceStatuses is null)
			{
				throw new Exception(productInstancesInfo?.ErrorMessage ?? "Не удалось получить информацию о кодах в ЧЗ");
			}

			var productInstansesForDocument = productInstancesInfo.InstanceStatuses;

			if(productInstansesForDocument.Count() == 0)
			{
				throw new InvalidOperationException(
					$"При проверке кодов в ЧЗ указанные коды не найдены. " +
					$"Тип задачи: {nameof(WithdrawalEdoTask)}. Id задачи: {withdrawalEdoTask.Id}");
			}

			return productInstansesForDocument;
		}

		private async Task CreateTrueMarkDocument(IUnitOfWork uow, WithdrawalEdoTask withdrawalEdoTask, OrderEntity order, string orderOrganizationInn, CancellationToken cancellationToken)
		{
			var document =
				await CreateIndividualAccountingWithdrawalDocument(order, withdrawalEdoTask, cancellationToken);

			var trueMarkDocumentId =
				await _trueMarkApiClient.SendIndividualAccountingWithdrawalDocument(document, orderOrganizationInn, cancellationToken);

			var trueMarkDocument = new TrueMarkDocument
			{
				Order = order,
				Guid = new Guid(trueMarkDocumentId),
				Organization = order.Contract.Organization,
				Type = TrueMarkDocument.TrueMarkDocumentType.Withdrawal
			};

			await uow.SaveAsync(trueMarkDocument, cancellationToken: cancellationToken);
		}

		private async Task<string> CreateIndividualAccountingWithdrawalDocument(
			OrderEntity order,
			WithdrawalEdoTask withdrawalEdoTask,
			CancellationToken cancellationToken)
		{
			var productInstansesForDocument =
				await GetCodesInstanceStatuses(withdrawalEdoTask, cancellationToken);

			return CreateIndividualAccountingWithdrawalDocument(order, productInstansesForDocument);
		}

		private string CreateIndividualAccountingWithdrawalDocument(OrderEntity order, IEnumerable<ProductInstanceStatus> codes)
		{
			var products = CreateProductIndividualAccountingDtos(order, codes).ToList();

			if(!products.Any())
			{
				throw new InvalidOperationException($"Не удалось сформировать товары для вывода из оборота для заказа {order.Id}");
			}

			var productDocument = new ProductDocumentIndividualAccountingDto
			{
				Inn = order.Contract.Organization.INN,
				BuyerInn = order.Client.INN,
				Action = "OWN_USE",
				ActionDate = order.DeliveryDate.Value,
				DocumentType = "OTHER",
				DocumentNumber = order.Id.ToString(),
				DocumentDate = order.DeliveryDate.Value,
				PrimaryDocumentCustomName = "UTD",
				Products = products
			};

			var serializedProductDocument = JsonSerializer.Serialize(productDocument);

			return serializedProductDocument;
		}

		private IEnumerable<ProductIndividualAccountingDto> CreateProductIndividualAccountingDtos(OrderEntity order, IEnumerable<ProductInstanceStatus> codes)
		{
			var products = new List<ProductIndividualAccountingDto>();

			var gtinsCodes = codes
				.GroupBy(c => c.Gtin)
				.ToDictionary(x => x.Key, x => x.ToArray());

			var nomenclaturesOrderItems = order.OrderItems
				.GroupBy(x => x.Nomenclature)
				.ToDictionary(x => x.Key, x => x.ToArray());

			foreach(var nomenclatureOrderItems in nomenclaturesOrderItems)
			{
				var nomenclature = nomenclatureOrderItems.Key;
				var orderItems = nomenclatureOrderItems.Value;

				if(!nomenclature.IsAccountableInTrueMark)
				{
					continue;
				}

				var productsCount = (int)orderItems.Sum(oi => oi.ActualCount ?? oi.Count);

				var productsTotalCost = nomenclatureOrderItems.Value
					.Sum(oi => oi.Price * (oi.ActualCount ?? oi.Count) - oi.DiscountMoney);

				var productsCostPerItem = productsTotalCost / productsCount;

				var nomenclatureGtins = nomenclature.Gtins.Select(x => x.GtinNumber).ToArray();

				var nomenclatureCodes = gtinsCodes
					.Where(x => nomenclatureGtins.Contains(x.Key))
					.SelectMany(x => x.Value)
					.Take(productsCount)
					.ToList();

				if(!nomenclatureCodes.Any())
				{
					continue;
				}

				decimal addedCodesTotalCostSum = 0;

				foreach(var code in nomenclatureCodes)
				{
					var productCost = productsCostPerItem;

					if(addedCodesTotalCostSum + productCost > productsTotalCost)
					{
						productCost = productsTotalCost - addedCodesTotalCostSum;
					}

					if(productCost <= 0)
					{
						throw new InvalidOperationException(
							$"Стоимость товара {nomenclature.Name} с кодом {code.IdentificationCode} не может быть меньше или равна нулю.");
					}

					addedCodesTotalCostSum += productCost;

					products.Add(new ProductIndividualAccountingDto
					{
						TrueMarkCode = code.IdentificationCode,
						ProductCost = productCost
					});
				}
			}

			return products;
		}

		private static async Task SetCompletedWithdrawalEdoTaskStatus(
			IUnitOfWork uow,
			WithdrawalEdoTask withdrawalEdoTask,
			CancellationToken cancellationToken)
		{
			withdrawalEdoTask.Problems.Clear();
			withdrawalEdoTask.Status = EdoTaskStatus.Completed;
			await uow.SaveAsync(withdrawalEdoTask, cancellationToken: cancellationToken);
		}
	}
}
