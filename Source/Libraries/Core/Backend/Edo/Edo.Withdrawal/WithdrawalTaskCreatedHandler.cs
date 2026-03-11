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
		private readonly IEdoSettings _edoSettings;
		private readonly IEdoRepository _edoRepository;

		public WithdrawalTaskCreatedHandler(
			ILogger<WithdrawalTaskCreatedHandler> logger,
			IHttpClientFactory httpClientFactory,
			IUnitOfWorkFactory uowFactory,
			ITrueMarkApiClient trueMarkApiClient,
			IEdoDocflowRepository edoDocflowRepository,
			ICounterpartyEdoAccountEntityController edoAccountEntityController,
			IGenericRepository<TrueMarkDocument> trueMarkDocumentRepository,
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
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
		}

		public async Task HandleWithdrawal(int withdrawalEdoTaskId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var withdrawalEdoTask = await uow.Session.GetAsync<WithdrawalEdoTask>(withdrawalEdoTaskId, cancellationToken);

				if(withdrawalEdoTask is null)
				{
					throw new InvalidOperationException(
						$"Задача вывода из оборота с Id {withdrawalEdoTaskId} не найдена. Вывод из оборота невозможен");
				}

				if(withdrawalEdoTask.Status == EdoTaskStatus.Completed)
				{
					throw new InvalidOperationException(
						$"Задача вывода из оборота с Id {withdrawalEdoTaskId} уже завершена, повторная обработка не требуется");
				}

				var withdrawalEdoRequest = withdrawalEdoTask.FormalEdoRequest as WithdrawalEdoRequest;
				var order = withdrawalEdoRequest?.Order;

				if(order == null)
				{
					throw new InvalidOperationException(
						$"Для задачи вывода из оборота с Id {withdrawalEdoTaskId} не найден заказ. Вывод из оборота невозможен");
				}

				var client = order.Client;

				if(client.PersonType != PersonType.legal)
				{
					throw new InvalidOperationException(
						$"Контрагент {client.Id} не является юридическим лицов. Вывод из оборота невозможен");
				}

				if(order.PaymentType != Vodovoz.Domain.Client.PaymentType.Cashless)
				{
					throw new InvalidOperationException(
						$"Заказ {order.Id} не по безналу. Вывод из оборота невозможен");
				}

				if(client.ReasonForLeaving != ReasonForLeaving.ForOwnNeeds)
				{
					throw new InvalidOperationException(
						$"В карточке контрагента {client.Id} указана причина выбытия отличная от {ReasonForLeaving.ForOwnNeeds}. " +
						$"Вывод из оборота невозможен");
				}

				var edoAccount =
					_edoAccountEntityController.GetDefaultCounterpartyEdoAccountByOrganizationId(client, order.Contract.Organization.Id);

				if(edoAccount.ConsentForEdoStatus == ConsentForEdoStatus.Agree
					&& client.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered)
				{
					var documents = _edoRepository.GetOrderEdoDocumentsByOrderId(uow, order.Id);
					var orderEdoDocument =
						documents
						.Where(x => x.DocumentTaskId == withdrawalEdoRequest.BaseDocumentEdoTask.Id)
						.FirstOrDefault();

					if(orderEdoDocument?.CreationTime == null)
					{
						throw new InvalidOperationException(
							$"От клиента {client.Id} получено согласие на ЭДО и клиент зарегистрирован в ЧЗ. " +
							"Время отправки документа не найдено. Вывод из оборота невозможен");
					}

					var daysSinceSend = (DateTime.Today - orderEdoDocument.CreationTime.Date).TotalDays;
					var timeoutDays = _edoSettings.ConnectedTrueMarkClientsWithdrawalDocflowTimeoutDays;

					if(daysSinceSend <= timeoutDays)
					{
						throw new InvalidOperationException(
							$"От клиента {client.Id} получено согласие на ЭДО и клиент зарегистрирован в ЧЗ. " +
							$"Документооборот не превысил таймаут в {timeoutDays} дней (прошло {daysSinceSend:F1} дней). " +
							"Вывод из оборота невозможен");
					}

					_logger.LogInformation(
						"Клиент {ClientId} зарегистрирован в ЭДО и ЧЗ, документооборот превысил таймаут в {Days} дней. Вывод из оборота разрешён",
						client.Id,
						timeoutDays);
				}

				var isTrueMarkDocumentExists = _trueMarkDocumentRepository
					.Get(uow, x => x.Order.Id == order.Id && x.Type == TrueMarkDocument.TrueMarkDocumentType.Withdrawal)
					.Any();

				if(isTrueMarkDocumentExists)
				{
					_logger.LogInformation(
						"Заказ {OrderId} уже имеет документ Честного знака для вывода из оборота, повторная обработка не требуется",
						order.Id);

					return;
				}

				var codesInOrder = withdrawalEdoTask.Items
					.Select(x => x.ProductCode)
					.Where(x => x.SourceCodeStatus == SourceProductCodeStatus.Accepted || x.SourceCodeStatus == SourceProductCodeStatus.Changed)
					.Select(x => x.ResultCode.IdentificationCode)
					.ToList();

				if(codesInOrder.Count == 0)
				{
					_logger.LogInformation(
						"Задача вывода из оборота с Id {WithdrawalEdoTaskId} не содержит кодов для вывода из оборота",
						withdrawalEdoTaskId);

					return;
				}

				try
				{
					var productInstancesInfo = await _trueMarkApiClient.GetProductInstanceInfoAsync(codesInOrder, cancellationToken);

					if(productInstancesInfo?.InstanceStatuses is null)
					{
						throw new Exception(productInstancesInfo?.ErrorMessage ?? "Не удалось получить информацию о кодах в ЧЗ");
					}

					var productInstansesForDocument = productInstancesInfo.InstanceStatuses
						.Where(x => x.Status == ProductInstanceStatusEnum.Introduced)
						.ToList();

					if(productInstansesForDocument.Count == 0)
					{
						throw new InvalidOperationException(
							$"После проверки кодов в ЧЗ не найдено кодов со статусом \"В обороте\". " +
							$"Тип задачи: {nameof(WithdrawalEdoTask)}. Id задачи: {withdrawalEdoTaskId}");
					}

					var document = CreateIndividualAccountingWithdrawalDocument(order, productInstansesForDocument);

					var trueMarkDocumentId =
						await _trueMarkApiClient.SendIndividualAccountingWithdrawalDocument(document, order.Contract.Organization.INN, cancellationToken);

					withdrawalEdoTask.Status = EdoTaskStatus.InProgress;

					var trueMarkDocument = new TrueMarkDocument
					{
						Order = order,
						Guid = new Guid(trueMarkDocumentId),
						Organization = order.Contract.Organization,
						Type = TrueMarkDocument.TrueMarkDocumentType.Withdrawal
					};

					await uow.SaveAsync(trueMarkDocument, cancellationToken: cancellationToken);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, ex.Message);

					withdrawalEdoTask.Status = EdoTaskStatus.Problem;
				}

				await uow.SaveAsync(withdrawalEdoTask, cancellationToken: cancellationToken);
				uow.Commit();
			}
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
	}
}
