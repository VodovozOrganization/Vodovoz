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
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using VodovozBusiness.EntityRepositories.Edo;

namespace Edo.Withdrawal
{
	public class WithdrawalTaskCreatedHandler
	{
		private readonly ILogger<WithdrawalTaskCreatedHandler> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ITrueMarkApiClient _trueMarkApiClient;
		private readonly IEdoDocflowRepository _edoDocflowRepository;
		private readonly IGenericRepository<TrueMarkDocument> _trueMarkDocumentRepository;

		public WithdrawalTaskCreatedHandler(
			ILogger<WithdrawalTaskCreatedHandler> logger,
			IHttpClientFactory httpClientFactory,
			IUnitOfWorkFactory uowFactory,
			ITrueMarkApiClient trueMarkApiClient,
			IEdoDocflowRepository edoDocflowRepository,
			IGenericRepository<TrueMarkDocument> trueMarkDocumentRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_trueMarkApiClient = trueMarkApiClient ?? throw new ArgumentNullException(nameof(trueMarkApiClient));
			_edoDocflowRepository = edoDocflowRepository ?? throw new ArgumentNullException(nameof(edoDocflowRepository));
			_trueMarkDocumentRepository = trueMarkDocumentRepository ?? throw new ArgumentNullException(nameof(trueMarkDocumentRepository));
		}

		public async Task HandleWithdrawal(int withdrawalEdoTaskId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var withdrawalEdoTask = await uow.Session.GetAsync<WithdrawalEdoTask>(withdrawalEdoTaskId, cancellationToken);

				if(withdrawalEdoTask is null)
				{
					throw new InvalidOperationException($"Задача {nameof(WithdrawalEdoTask)} с Id {withdrawalEdoTaskId} не найдена");
				}

				if(withdrawalEdoTask.Status == EdoTaskStatus.Completed)
				{
					_logger.LogInformation(
						$"Задача {nameof(WithdrawalEdoTask)} с Id {withdrawalEdoTaskId} уже завершена, повторная обработка не требуется");

					return;
				}

				var order = withdrawalEdoTask.OrderEdoRequest?.Order;

				if(order == null)
				{
					throw new InvalidOperationException($"Для задачи вывода из оборота с Id {withdrawalEdoTaskId} не найден заказ");
				}

				if(order.PaymentType != Vodovoz.Domain.Client.PaymentType.Cashless)
				{
					throw new InvalidOperationException($"Заказ {order.Id} не по безналу, вывод из оборота невозможен");
				}

				var isTrueMarkDocumentExists = _trueMarkDocumentRepository
					.Get(uow, x => x.Order.Id == order.Id && (x.IsSuccess || (!x.IsSuccess && string.IsNullOrWhiteSpace(x.ErrorMessage))))
					.Any();

				if(isTrueMarkDocumentExists)
				{
					throw new InvalidOperationException(
						$"Заказ {order.Id} уже имеет успешный или необработанный документ Честного знака, вывод из оборота невозможен");
				}
				
				var lastEdoDocflowStatus = _edoDocflowRepository.GetEdoDocflowDataByOrderId(uow, order.Id)
					.OrderByDescending(x => x.EdoRequestCreationTime)
					.Select(x => x.EdoDocFlowStatus)
					.FirstOrDefault();

				if(lastEdoDocflowStatus is null || lastEdoDocflowStatus != EdoDocFlowStatus.Succeed)
				{
					//throw new InvalidOperationException($"Заказ {order.Id} не имеет успешного документооборота, вывод из оборота невозможен");
				}
				
				var client = order.Client;

				if(order.Client.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered)
				{
					throw new InvalidOperationException($"Клиент {client.Name} зарегистрирован в Честном Знаке, вывод из оборота невозможен");
				}

				var codesInOrder = withdrawalEdoTask.Items
					.Select(x => x.ProductCode)
					.Where(x => x.SourceCodeStatus == SourceProductCodeStatus.Accepted || x.SourceCodeStatus == SourceProductCodeStatus.Changed)
					.Select(x => x.ResultCode.IdentificationCode)
					.ToList();

				if(codesInOrder.Count == 0)
				{
					throw new InvalidOperationException(
						$"Задача {nameof(WithdrawalEdoTask)} с Id {withdrawalEdoTaskId} не содержит кодов для вывода из оборота");
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

					var documentSendResponse =
						_trueMarkApiClient.SendIndividualAccountingWithdrawalDocument(document, order.Contract.Organization.INN, cancellationToken);

					withdrawalEdoTask.Status = EdoTaskStatus.InProgress;
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, ex.Message);

					withdrawalEdoTask.Status = EdoTaskStatus.Problem;
					withdrawalEdoTask.Problems.Add(new EdoTaskProblem
					{
						Type = EdoTaskProblemType.Exception,
						SourceName = "Вывод из оборота",
						CreationTime = DateTime.Now,
						State = TaskProblemState.Active
					});
				}

				var trueMarkDocument = new TrueMarkDocument
				{
					Order = order,
					Guid = Guid.NewGuid(),
					IsSuccess = false,
					Organization = order.Contract.Organization
				};

				await uow.SaveAsync(trueMarkDocument, cancellationToken: cancellationToken);
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
				Products = CreateProductIndividualAccountingDtos(order, codes).ToList()
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

				var nomenclatureGtins = nomenclature.Gtins.Select(x => x.GtinNumber).ToArray();

				var nomenclatureCodes = gtinsCodes
					.Where(x => nomenclatureGtins.Contains(x.Key))
					.SelectMany(x => x.Value)
					.ToList();

				if(!nomenclatureCodes.Any())
				{
					continue;
				}

				var productsCount = orderItems.Sum(oi => oi.ActualCount ?? oi.Count);

				var productsTotalCost = nomenclatureOrderItems.Value
					.Sum(oi => oi.Price * (oi.ActualCount ?? oi.Count) - oi.DiscountMoney);

				var productsCostPerItem = productsTotalCost / productsCount;

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
