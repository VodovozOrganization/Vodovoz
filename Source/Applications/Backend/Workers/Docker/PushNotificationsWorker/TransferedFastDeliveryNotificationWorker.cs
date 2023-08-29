using FirebaseCloudMessaging.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PushNotificationsWorker.Options;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Orders;

namespace PushNotificationsWorker
{
	internal sealed class TransferedFastDeliveryNotificationWorker : BackgroundService
	{
		private readonly ILogger<TransferedFastDeliveryNotificationWorker> _logger;
		private readonly IFirebaseCloudMessagingService _firebaseService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly TimeSpan _interval;

		public TransferedFastDeliveryNotificationWorker(
			ILogger<TransferedFastDeliveryNotificationWorker> logger,
			IOptions<TransferedFastDeliveryNotificationWorkerSettings> settings,
			IUnitOfWorkFactory unitOfWorkFactory,
			IFirebaseCloudMessagingService firebaseService)
		{
			if(settings is null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			_interval = settings.Value.Interval;

			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_firebaseService = firebaseService ?? throw new ArgumentNullException(nameof(firebaseService));

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot("Сервис PUSH сообщений");
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation(
				$"{nameof(TransferedFastDeliveryNotificationWorker)} is running.");

			while(!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var today = DateTime.Today;

					var routeListsTransferedFrom =
						from routeListAddress in _unitOfWork.Session.Query<RouteListItem>()
						join order in _unitOfWork.Session.Query<Order>()
						on routeListAddress.Order.Id equals order.Id
						join routeList in _unitOfWork.Session.Query<RouteList>()
						on routeListAddress.RouteList.Id equals routeList.Id
						join driver in _unitOfWork.Session.Query<Employee>()
						on routeList.Driver.Id equals driver.Id
						let notNotified = (
							from change in _unitOfWork.Session.Query<FastDeliveryChange>()
							where change.RouteList.Id == routeListAddress.RouteList.Id
							&& change.ChangeType == FastDeliveryChange.ChangeTypeEnum.Transfered
							select change.Id
						).Count() == 0
						where order.IsFastDelivery
						 && routeListAddress.Status == RouteListItemStatus.Transfered
						 && notNotified
						 && routeListAddress.CreationDate >= today
						select routeListAddress;

					if(routeListsTransferedFrom is null)
					{
						throw new InvalidOperationException($"{routeListsTransferedFrom} is null");
					}

					var toNotifyCount = routeListsTransferedFrom.Count();

					if(toNotifyCount > 0)
					{
						_logger.LogInformation(
						"Найдено {Count} адресов для оповещения о перемносе адреса МЛ", toNotifyCount);
					}

					foreach(RouteListItem routeListAddress in routeListsTransferedFrom)
					{
						_logger.LogInformation(
							"Адрес маршрутного листа {RouteListAddressId}, заказ {OrderId} был перенесен",
							routeListAddress.Id,
							routeListAddress.Order.Id);

						await _firebaseService.SendFastDeliveryAddressCanceledNotification(
							routeListAddress.RouteList.Driver.AndroidToken,
							routeListAddress.Order.Id);

						var newChange = new FastDeliveryChange
						{
							RouteList = new RouteList { Id = routeListAddress.RouteList.Id },
							Order = new Order { Id = routeListAddress.Order.Id },
							ChangeType = FastDeliveryChange.ChangeTypeEnum.Transfered,
							CreatedAt = DateTime.Now
						};

						_unitOfWork.Session.Save(newChange);
					}

					_unitOfWork.Session.Flush();

					await Task.Delay(_interval, stoppingToken);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка");
				}
			}
		}
	}
}
