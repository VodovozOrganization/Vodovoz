using BitrixApi.Contracts.Dto.Responses;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Results;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Errors.Clients;
using Vodovoz.Errors.Orders;

namespace BitrixApi.Library.Services
{
	/// <inheritdoc/>
	public class OrdersService : IOrdersService
	{
		private readonly ILogger<OrdersService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IOrderRepository _orderRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger">Логгер</param>
		/// <param name="uow">Unit of work</param>
		/// <param name="orderRepository">Репозиторий заказов</param>
		/// <param name="counterpartyRepository">Репозиторий контрагентов</param>
		public OrdersService(
			ILogger<OrdersService> logger,
			IUnitOfWork uow,
			IOrderRepository orderRepository,
			ICounterpartyRepository counterpartyRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
		}

		/// <inheritdoc/>
		public async Task<Result<GetOrdersResponse>> GetOrdersByPhoneNumberFromDate(string phone, DateTime startDate, CancellationToken cancellationToken)
		{
			var phoneDigitsNumber = phone.NormalizePhone();

			var counterpartyIdsByCounterpartyPhone =
				(await _counterpartyRepository.GetCounterpartyIdsByCounterpartyPhoneNumber(_uow, phoneDigitsNumber, cancellationToken))
				.ToArray();

			var deliveryPointIdsWithCounterpartyIds =
				(await _counterpartyRepository.GetDeliveryPointIdsWithCounterpartyIdsByDeliveryPointPhoneNumber(
					_uow, phoneDigitsNumber, cancellationToken))
				.ToArray();

			var counterpartyIds =
				counterpartyIdsByCounterpartyPhone
				.Concat(deliveryPointIdsWithCounterpartyIds.Select(x => x.CounterpartyId))
				.Distinct()
				.ToArray();

			if(!counterpartyIds.Any())
			{
				_logger.LogInformation(
					"Контрагент по номеру телефона {PhoneDigitsNumber} не найден",
					phoneDigitsNumber);

				return Result.Failure<GetOrdersResponse>(CounterpartyErrors.NotFound);
			}

			if(counterpartyIds.Length > 1)
			{
				_logger.LogInformation(
					"По номеру телефона {PhoneDigitsNumber} найдено более одного контрагента: {CounterpartyIds}",
					phoneDigitsNumber,
					string.Join(",", counterpartyIds));

				return Result.Failure<GetOrdersResponse>(CounterpartyErrors.MoreThanOneFoundByPhoneNumber);
			}

			var counterpartyId = counterpartyIds.First();

			// Если телефон добавлен к самому контрагенту - возвращаем все его заказы независимо от точки доставки,
			// если только к точкам доставки - ограничиваем выборку заказами на эти точки
			var orderIds = counterpartyIdsByCounterpartyPhone.Any()
				? (await _orderRepository.GetOrderIdsByCounterpartyFromDate(
					_uow,
					counterpartyId,
					startDate,
					_orderRepository.GetUndeliveryStatuses(),
					cancellationToken))
				.ToArray()
				: (await _orderRepository.GetOrderIdsByCounterpartyAndDeliveryPointsFromDate(
					_uow,
					counterpartyId,
					deliveryPointIdsWithCounterpartyIds.Select(x => x.DeliveryPointId).ToArray(),
					startDate,
					_orderRepository.GetUndeliveryStatuses(),
					cancellationToken))
				.ToArray();

			if(!orderIds.Any())
			{
				_logger.LogInformation(
					"Заказы по номеру телефона {PhoneDigitsNumber} с датой создания не ранее {StartDate} не найдены",
					phoneDigitsNumber,
					startDate);

				return Result.Failure<GetOrdersResponse>(OrderErrors.NotFoundByPhoneAndStartDate);
			}

			return new GetOrdersResponse
			{
				OrderIds = string.Join(",", orderIds)
			};
		}
	}
}
