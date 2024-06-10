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
using Vodovoz.Application.FirebaseCloudMessaging;
using Microsoft.Extensions.DependencyInjection;

namespace PushNotificationsWorker
{
	internal sealed class TransferedFastDeliveryNotificationWorker : BackgroundService
	{
		private readonly ILogger<TransferedFastDeliveryNotificationWorker> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly TimeSpan _interval;

		public TransferedFastDeliveryNotificationWorker(
			ILogger<TransferedFastDeliveryNotificationWorker> logger,
			IOptions<TransferedFastDeliveryNotificationWorkerSettings> settings,
			IServiceScopeFactory serviceScopeFactory)
		{
			if(settings is null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			_interval = settings.Value.Interval;

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation(
				$"{nameof(TransferedFastDeliveryNotificationWorker)} is running.");

			while(!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = _serviceScopeFactory.CreateScope();

					var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
					var firebaseService = scope.ServiceProvider.GetRequiredService<IFirebaseCloudMessagingService>();

					using var unitOfWork = unitOfWorkFactory.CreateWithoutRoot("Сервис PUSH сообщений");

					var today = DateTime.Today;

					var routeListsTransferedFrom =
						from routeListAddress in unitOfWork.Session.Query<RouteListItem>()
						join order in unitOfWork.Session.Query<Order>()
						on routeListAddress.Order.Id equals order.Id
						join routeList in unitOfWork.Session.Query<RouteList>()
						on routeListAddress.RouteList.Id equals routeList.Id
						join driver in unitOfWork.Session.Query<Employee>()
						on routeList.Driver.Id equals driver.Id
						let notNotified = (
							from change in unitOfWork.Session.Query<FastDeliveryChange>()
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
						
						var userApp = routeListAddress.RouteList.Driver.DriverAppUser;
						
						if(userApp != null && !string.IsNullOrWhiteSpace(userApp.Token))
						{
							await firebaseService.SendFastDeliveryAddressTransferedMessage(
								userApp.Token,
								routeListAddress.Order.Id);
						}

						var newChange = new FastDeliveryChange
						{
							RouteList = new RouteList { Id = routeListAddress.RouteList.Id },
							Order = new Order { Id = routeListAddress.Order.Id },
							ChangeType = FastDeliveryChange.ChangeTypeEnum.Transfered,
							CreatedAt = DateTime.Now
						};

						unitOfWork.Session.Save(newChange);
					}

					unitOfWork.Commit();

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
