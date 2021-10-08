using System;
using Bitrix;
using Bitrix.DTO;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;

namespace BitrixIntegration
{
	public class DealSynchronizer
	{
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		private readonly IBitrixRepository _bitrixRepository;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IBitrixClient _bitrixClient;

		public DealSynchronizer(
			IBitrixRepository bitrixRepository,
			IUnitOfWorkFactory uowFactory,
			IBitrixClient bitrixClient)
		{
			_bitrixRepository = bitrixRepository ?? throw new ArgumentNullException(nameof(bitrixRepository));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_bitrixClient = bitrixClient ?? throw new ArgumentNullException(nameof(bitrixClient));
		}

		public void SynchronizeDeals()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var registrations = _bitrixRepository.GetDealRegistrationsToSync(uow);
				foreach(var registration in registrations)
				{
					if(registration.Order == null)
					{
						throw new InvalidOperationException(
							$"Нельзя синхронизировать регистрацию сделки {registration.Id}: не указан заказ");
					}

					var newStatus = MatchOrderStatusToDealStatus(registration.Order.OrderStatus);
					var dealId = registration.BitrixId;
					var statusSet = _bitrixClient.SetStatusToDeal(newStatus, dealId).GetAwaiter().GetResult();
					if(statusSet)
					{
						registration.NeedSync = false;
					}
					else
					{
						_logger.Warn($"Не удалось установить статус {newStatus} для сделки {dealId}");
						registration.NeedSync = true;
					}
					registration.Success = newStatus == DealStatus.Success;
					registration.ProcessedDate = DateTime.Now;

					uow.Save(registration);
				}

				uow.Commit();
			}
		}

		private DealStatus MatchOrderStatusToDealStatus(OrderStatus orderStatus)
			=> orderStatus switch
			{
				OrderStatus.NewOrder => DealStatus.InProgress,
				OrderStatus.WaitForPayment => DealStatus.InProgress,
				OrderStatus.Accepted => DealStatus.InProgress,
				OrderStatus.InTravelList => DealStatus.InProgress,
				OrderStatus.OnLoading => DealStatus.InProgress,
				OrderStatus.OnTheWay => DealStatus.InProgress,

				OrderStatus.Canceled => DealStatus.Fail,
				OrderStatus.DeliveryCanceled => DealStatus.Fail,
				OrderStatus.NotDelivered => DealStatus.Fail,

				OrderStatus.Shipped => DealStatus.Success,
				OrderStatus.UnloadingOnStock => DealStatus.Success,
				OrderStatus.Closed => DealStatus.Success,
				_ => throw new ArgumentOutOfRangeException()
			};
	}
}
