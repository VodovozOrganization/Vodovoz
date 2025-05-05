using Edo.Withdrawal.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using VodovozBusiness.Services.TrueMark;

namespace Edo.Withdrawal
{
	public class WithdrawalTaskCreatedHandler
	{
		private readonly ILogger<WithdrawalTaskCreatedHandler> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ITrueMarkApiClient _trueMarkApiClient;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly IOptions<TrueMarkOptions> _options;

		public WithdrawalTaskCreatedHandler(
			ILogger<WithdrawalTaskCreatedHandler> logger,
			IHttpClientFactory httpClientFactory,
			IUnitOfWorkFactory uowFactory,
			ITrueMarkApiClient trueMarkApiClient,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			IOptions<TrueMarkOptions> trueMarkOptions)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_trueMarkApiClient = trueMarkApiClient ?? throw new ArgumentNullException(nameof(trueMarkApiClient));
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_options = trueMarkOptions;
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
					throw new InvalidOperationException($"Задача {nameof(WithdrawalEdoTask)} с Id {withdrawalEdoTaskId} уже завершена");
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

				var client = order.Client;

				if(order.Client.ConsentForEdoStatus != ConsentForEdoStatus.Agree)
				{
					throw new InvalidOperationException($"У клиента {client.Name} отсутствует согласие на ЭДО. Вывод из оборота невозможен");
				}

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

					var codesForDocument = productInstancesInfo.InstanceStatuses
						.Where(x => x.Status == ProductInstanceStatusEnum.Introduced)
						.Select(x => x.IdentificationCode)
						.ToList();

					if(codesForDocument.Count == 0)
					{
						throw new InvalidOperationException(
							$"После проверки кодов в ЧЗ не найдено кодов со статусом \"В обороте\". " +
							$"Тип задачи: {nameof(WithdrawalEdoTask)}. Id задачи: {withdrawalEdoTaskId}");
					}

					var document = CreateIndividualAccountingWithdrawalDocument(order, codesInOrder);

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
				
				await uow.SaveAsync(withdrawalEdoTask, cancellationToken: cancellationToken);
				uow.Commit();
			}
		}

		private string CreateIndividualAccountingWithdrawalDocument(OrderEntity order, IEnumerable<string> identificationCodes)
		{
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
				Products = identificationCodes
			};

			var serializedProductDocument = JsonSerializer.Serialize(productDocument);

			return serializedProductDocument;
		}
	}
}
