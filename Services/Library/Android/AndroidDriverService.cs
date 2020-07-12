using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Repository.Logistics;
using QS.DomainModel.Tracking;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.Services;
using Vodovoz.Tools.CallTasks;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Core.DataService;
using Android.DTO;
using SmsPaymentService;

namespace Android
{
	public class AndroidDriverService : IAndroidDriverService, IAndroidDriverServiceWeb
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		private readonly WageParameterService wageParameterService;
		private readonly IDriverServiceParametersProvider parameters;
		private readonly ChannelFactory<ISmsPaymentService> smsPaymentChannelFactory;
		private readonly IDriverNotificator driverNotificator;

		public AndroidDriverService(
			WageParameterService wageParameterService, 
			IDriverServiceParametersProvider parameters,
			ChannelFactory<ISmsPaymentService> smsPaymentChannelFactory,
			IDriverNotificator driverNotificator
			)
		{
			this.wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
			this.smsPaymentChannelFactory = smsPaymentChannelFactory ?? throw new ArgumentNullException(nameof(smsPaymentChannelFactory));
			this.driverNotificator = driverNotificator ?? throw new ArgumentNullException(nameof(driverNotificator));
		}

		private CallTaskWorker callTaskWorker;
		public virtual CallTaskWorker CallTaskWorker {
			get {
				if(callTaskWorker == null) {
					callTaskWorker = new CallTaskWorker(
						CallTaskSingletonFactory.GetInstance(),
						new CallTaskRepository(),
						OrderSingletonRepository.GetInstance(),
						EmployeeSingletonRepository.GetInstance(),
						new BaseParametersProvider(),
						null,
						null);
				}
				return callTaskWorker;
			}
			set { callTaskWorker = value; }
		}

		/// <summary>
		/// Const value, equals to android code version on AndroidManifest.xml
		/// Needed for version checking. Increment this value on each API change.
		/// This is minimal version works with current API.
		/// </summary>
		private const int VERSION_CODE = 11;

		#region IAndroidDriverService implementation

		public CheckVersionResultDTO CheckApplicationVersion(int versionCode)
		{
			using (var uow = UnitOfWorkFactory.CreateWithoutRoot("[ADS]Проверка текущей версии приложения"))
			{
				BaseParameter lastVersionParameter = null;
				BaseParameter lastVersionNameParameter = null;
				lastVersionParameter = uow.Session.Get<BaseParameter>("last_android_version_code");
				lastVersionNameParameter = uow.Session.Get<BaseParameter>("last_android_version_name");


				var result = new CheckVersionResultDTO();
				result.DownloadUrl = "market://details?id=ru.qsolution.vodovoz.driver";
				result.NewVersion = lastVersionNameParameter?.StrValue;

				int lastVersionCode = 0;
				Int32.TryParse(lastVersionParameter?.StrValue, out lastVersionCode);

				if (lastVersionCode > versionCode)
					result.Result = CheckVersionResultDTO.ResultType.CanUpdate;

				if (VERSION_CODE > versionCode)
					result.Result = CheckVersionResultDTO.ResultType.NeedUpdate;

				return result;
			}
		}

		/// <summary>
		/// Authenticating driver by login and password.
		/// </summary>
		/// <returns>authentication string or <c>null</c></returns>
		/// <param name="login">Login.</param>
		/// <param name="password">Password.</param>
		public string Auth (string login, string password)
		{
			#if DEBUG
			logger.Debug("Auth called with args:\nlogin: {0}\npassword: {1}", login, password);
			#endif
			try
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot("[ADS]Авторизация пользователя"))
				{
					var employee = EmployeeRepository.GetDriverByAndroidLogin(uow, login);

					if (employee == null)
						return null;

					//Generating hash from driver password
					var hash = (new SHA1Managed()).ComputeHash(Encoding.UTF8.GetBytes(employee.AndroidPassword));
					var hashString = string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
					if (password == hashString)
					{
						//Creating session auth key if needed
						if (String.IsNullOrEmpty(employee.AndroidSessionKey))
						{
							employee.AndroidSessionKey = Guid.NewGuid().ToString();
							uow.Save(employee);
							uow.Commit();
						}
						return employee.AndroidSessionKey;
					}
				}
			}
			catch (Exception e)
			{
				logger.Error(e);
			}
			return null;
		}

		/// <summary>
		/// Checking authentication key
		/// </summary>
		/// <returns><c>true</c>, if auth was checked, <c>false</c> otherwise.</returns>
		/// <param name="authKey">Auth key.</param>
		public bool CheckAuth (string authKey)
		{
			#if DEBUG
			logger.Debug("CheckAuth called with args; authKey: {0}", authKey);
			#endif
			try {
				using (IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot("[ADS]Проверка авторизации пользователя"))
				{
					return CheckAuth(uow, authKey);
				}
			}
			catch (Exception e)
			{
				logger.Error(e);
			}
			return false;
		}

		private bool CheckAuth(IUnitOfWork uow, string authKey)
		{
			var driver = EmployeeRepository.GetDriverByAuthKey(uow, authKey);
			if (driver == null)
				logger.Warn("Неудачная попытка авторизации по ключу {0}", authKey);
			return driver != null;
		}

		/// <summary>
		/// Gets the route lists for driver authenticated with the specified key.
		/// </summary>
		/// <returns>The route lists or <c>null</c>.</returns>
		/// <param name="authKey">Authentication key.</param>
		public List<RouteListDTO> GetRouteLists (string authKey)
		{
			#if DEBUG
			logger.Debug("GetRouteLists called with args:\nauthKey: {0}", authKey);
			#endif
			try
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot("[ADS]Получение списка маршрутных листов"))
				{
					if (!CheckAuth(uow, authKey))
						return null;
					
					var result = new List<RouteListDTO>();
					var driver = EmployeeRepository.GetDriverByAuthKey(uow, authKey);
					var routeLists = RouteListRepository.GetDriverRouteLists(uow, driver, RouteListStatus.EnRoute, DateTime.Today);

					foreach (RouteList rl in routeLists)
					{
						result.Add(new RouteListDTO(rl));
					}
					return result;
				}
			}
			catch (Exception e)
			{
				logger.Error(e);
			}
			return null;
		}

		public List<ShortOrderDTO> GetRouteListOrders (string authKey, int routeListId)
		{
			#if DEBUG
			logger.Debug("GetRouteListOrders called with args:\nauthKey: {0}\nrouteListId: {1}", authKey, routeListId);
			#endif

			try
			{
				if (!CheckAuth (authKey))
					return null;

				using (var routeListUoW = UnitOfWorkFactory.CreateForRoot<RouteList>(routeListId, "[ADS]Получение списка заказов в МЛ"))
				{
					if (routeListUoW == null || routeListUoW.Root == null)
						return null;

					var orders = new List<ShortOrderDTO>();
					foreach (RouteListItem item in routeListUoW.Root.Addresses)
					{
						orders.Add(new ShortOrderDTO(item));
					}
					return orders;
				}
			}
			catch (Exception e)
			{
				logger.Error(e);
			}
			return null;
		}

		public OrderDTO GetOrderDetailed (string authKey, int orderId)
		{
			#if DEBUG
			logger.Debug("GetOrderDetailed called with args:\nauthKey: {0}\norderId: {1}", authKey, orderId);
			#endif

			try
			{
				if (!CheckAuth (authKey))
					return null;

				using (var orderUoW = UnitOfWorkFactory.CreateForRoot<Order>(orderId, "[ADS]Детальная информация по заказу"))
				{
					if (orderUoW == null || orderUoW.Root == null)
						return null;
					var routeListItem = RouteListItemRepository.GetRouteListItemForOrder(orderUoW, orderUoW.Root);
					OrderDTO orderDTO = new OrderDTO(routeListItem);
					SmsPaymentStatus? smsPaymentStatus = OrderSingletonRepository.GetInstance().GetOrderPaymentStatus(orderUoW, orderUoW.Root.Id);
					if(smsPaymentStatus == null) {
						orderDTO.PaymentStatus = PaymentStatus.None;
					} else {
						switch(smsPaymentStatus.Value) {
						case SmsPaymentStatus.WaitingForPayment:
							orderDTO.PaymentStatus = PaymentStatus.WaitingForPayment;
							break;
						case SmsPaymentStatus.Paid:
							orderDTO.PaymentStatus = PaymentStatus.Paid;
							break;
						case SmsPaymentStatus.Cancelled:
							orderDTO.PaymentStatus = PaymentStatus.Cancelled;
							break;
						}
					}

					return orderDTO;
				}
			}
			catch (Exception e)
			{
				logger.Error(e);
			}
			return null;
		}
		
		public int? StartOrResumeTrack (string authKey, int routeListId)
		{
			try
			{
				using (var uow = UnitOfWorkFactory.CreateWithoutRoot("[ADS]Старт записи трека"))
				{
					if (!CheckAuth(authKey))
						return null;

					var track = TrackRepository.GetTrackForRouteList(uow, routeListId);

					if (track != null)
						return track.Id;

					track = new Track();

					track.RouteList = uow.GetById<RouteList>(routeListId);
					track.Driver = EmployeeRepository.GetDriverByAuthKey(uow, authKey);
					track.StartDate = DateTime.Now;
					uow.Save(track);
					uow.Commit();

					return track.Id;
				}
			}
			catch (Exception e)
			{
				logger.Error(e);
			}
			return null;
		}

		public bool SendCoordinates (string authKey, int trackId, TrackPointList TrackPointList)
		{
			RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties [RemoteEndpointMessageProperty.Name];
			if (prop != null)
				logger.Info(RusNumber.Case(TrackPointList.Count, "Получена {3} координата по треку", "Получено {3} координаты по треку", "Получено {3} координат по треку")
				            + " {0} c ip{1}:{2}", trackId, prop.Address, prop.Port, TrackPointList.Count);

			if (!CheckAuth (authKey))
				return false;

			return TracksService.ReceivedCoordinates(trackId, TrackPointList);
		}
		

		public bool ChangeOrderStatus2(string authKey, int orderId, string status, string bottlesReturned)
		{

			logger.Debug("Change order status2:\n orderId: {0}\n status: {1}\n bottles: {2}", orderId, status, bottlesReturned);

			try {
				if(!CheckAuth(authKey))
					return false;

				using(var orderUoW = UnitOfWorkFactory.CreateForRoot<Order>(orderId, $"[ADS]v2 Изменение статуса заказа {orderId}")) {
					if(orderUoW == null || orderUoW.Root == null)
						return false;

					var routeListItem = RouteListItemRepository.GetRouteListItemForOrder(orderUoW, orderUoW.Root);
					if(routeListItem == null)
						return false;

					if(routeListItem.Status == RouteListItemStatus.Transfered) {
						logger.Error("Попытка переключить статус у переданного адреса. address_id = {0}", routeListItem.Id);
						return false;
					}

					switch(status) {
					case "EnRoute": routeListItem.UpdateStatus(orderUoW, RouteListItemStatus.EnRoute, CallTaskWorker); break;
					case "Completed": routeListItem.UpdateStatus(orderUoW, RouteListItemStatus.Completed, CallTaskWorker); break;
					case "Canceled": routeListItem.UpdateStatus(orderUoW, RouteListItemStatus.Canceled, CallTaskWorker); break;
					case "Overdue": routeListItem.UpdateStatus(orderUoW, RouteListItemStatus.Overdue, CallTaskWorker); break;
					default: return false;
					}

					int bottles;
					if(int.TryParse(bottlesReturned, out bottles)) {
						routeListItem.DriverBottlesReturned = bottles;
						logger.Debug("Changed! order status2:\n orderId: {0}\n status: {1}\n bottles: {2}", orderId, status, bottles);

					}

					orderUoW.Save(routeListItem);
					orderUoW.Commit();
					return true;
				}
			}
			catch(Exception e) {
				logger.Error(e);
			}
			return false;
		}


		public bool EnablePushNotifications (string authKey, string token)
		{
			try
			{
				using (var uow = UnitOfWorkFactory.CreateWithoutRoot($"[ADS]Включение Push уведомлений"))
				{
					if (!CheckAuth(uow, authKey))
						return false;
					var driver = EmployeeRepository.GetDriverByAuthKey(uow, authKey);
					if (driver == null)
						return false;
					driver.AndroidToken = token;
					uow.Save(driver);
					uow.Commit();
					return true;
				}
			} 
			catch (Exception e) 
			{
				logger.Error (e);
				return false;
			}
		}

		public bool DisablePushNotifications (string authKey)
		{
			try
			{
				using (var uow = UnitOfWorkFactory.CreateWithoutRoot($"[ADS]Отключение Push-уведомлений"))
				{
					if (!CheckAuth(uow, authKey))
						return false;
					var driver = EmployeeRepository.GetDriverByAuthKey(uow, authKey);
					if (driver == null)
						return false;
					driver.AndroidToken = null;
					uow.Save(driver);
					uow.Commit();
					return true;
				}
			} 
			catch (Exception e) 
			{
				logger.Error (e);
				return false;
			}
		}

		public bool FinishRouteList (string authKey, int routeListId) {
			try
			{
				using (var uow = UnitOfWorkFactory.CreateWithoutRoot($"[ADS]Маршрутный лист {routeListId} завершен"))
				{

					if (!CheckAuth(uow, authKey))
						return false;

					var routeList = uow.GetById<RouteList>(routeListId);

					if (routeList == null)
						return false;

					if(routeList.Status != RouteListStatus.EnRoute) {
						logger.Error("Отмена попытки завершить маршрутный лист №{0}, в статусе отличном от 'В Пути'", routeList.Id);
						return false;
					}

					if(routeList.Addresses.Any(r => r.Status == RouteListItemStatus.EnRoute))
					{
						logger.Error("Была отменена попытка закрыть маршрутный лист {0}, с адресами в статусе 'В Пути'", routeList.Id);
						return false;
					}

					routeList.CompleteRoute(wageParameterService, CallTaskWorker);
					uow.Save(routeList);
					uow.Commit();
					return true;
				}
			}
			catch (Exception e)
			{
				logger.Error(e);
			}
			return false;
		}

		public bool ServiceStatus()
		{
			int activeUoWCount = UowWatcher.GetActiveUoWCount();
			if(activeUoWCount > parameters.MaxUoWAllowed) {
				return false;
			}
			return true;
		}

		public PaymentInfoDTO GetPaymentStatus(string authKey, int orderId)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"[ADS] GetPaymentStatus method")) {
				PaymentInfoDTO result = new PaymentInfoDTO(orderId, PaymentStatus.None);

				if(!CheckAuth(uow, authKey))
					return result;


				ISmsPaymentService smsPaymentService = smsPaymentChannelFactory.CreateChannel();
				if(smsPaymentService == null) {
					logger.Warn($"Невозможно получить статус платежа. {nameof(smsPaymentService)} is null");
					return result;
				}

				PaymentResult paymentResult = null;
				try {
					paymentResult = smsPaymentService.GetActualPaymentStatus(orderId);
				}
				catch(Exception ex) {
					logger.Error(ex);
				}
				if(paymentResult == null || paymentResult.Status == PaymentResult.MessageStatus.Error || paymentResult.PaymentStatus == null) {
					result.Status = PaymentStatus.None;
					return result;
				}

				switch(paymentResult.PaymentStatus.Value) {
					case SmsPaymentStatus.WaitingForPayment:
						result.Status = PaymentStatus.WaitingForPayment;
						break;
					case SmsPaymentStatus.Paid:
						result.Status = PaymentStatus.Paid;
						break;
					case SmsPaymentStatus.Cancelled:
						result.Status = PaymentStatus.Cancelled;
						break;
				}

				return result;
			}
		}

		public PaymentInfoDTO CreateOrderPayment(string authKey, int orderId, string phoneNumber)
		{
			ISmsPaymentService smsPaymentService = smsPaymentChannelFactory.CreateChannel();
			if(smsPaymentService == null) {
				logger.Warn($"Невозможно создать платеж для заказа {orderId}. {nameof(smsPaymentService)} is null");
				return new PaymentInfoDTO(orderId, PaymentStatus.None);
			}

			PaymentResult paymentResult = smsPaymentService.SendPayment(orderId, phoneNumber);
			if(paymentResult == null || paymentResult.Status == PaymentResult.MessageStatus.Error) {
				return new PaymentInfoDTO(orderId, PaymentStatus.None);
			}

			if(paymentResult.PaymentStatus == null) {
				return new PaymentInfoDTO(orderId, PaymentStatus.None);
			} else {
				switch(paymentResult.PaymentStatus.Value) {
					case SmsPaymentStatus.WaitingForPayment:
						return new PaymentInfoDTO(orderId, PaymentStatus.WaitingForPayment);
					case SmsPaymentStatus.Paid:
						return new PaymentInfoDTO(orderId, PaymentStatus.Paid);
					case SmsPaymentStatus.Cancelled:
						return new PaymentInfoDTO(orderId, PaymentStatus.Cancelled);
					default:
						return new PaymentInfoDTO(orderId, PaymentStatus.None);
				}
			}
		}

		public bool RefreshPaymentStatus(int orderId)
		{
			try {
				if(orderId < 1) {
					logger.Warn($"Передан неверный номер заказа ({orderId}) при попытке обновить статус платежа");
					return false;
				}

				using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
					RouteListItem routeListItemAlias = null;
					RouteList routeListAlias = null;
					Order orderAlias = null;

					var routeLists = uow.Session.QueryOver<RouteList>(() => routeListAlias)
					   .Left.JoinAlias(() => routeListAlias.Addresses, () => routeListItemAlias)
					   .Left.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
					   .Where(() => orderAlias.Id == orderId)
					   .Where(() => !routeListItemAlias.WasTransfered)
					   .Where(() => routeListItemAlias.Status == RouteListItemStatus.EnRoute)
					   .List();
					if(!routeLists.Any()) {
						logger.Warn($"При обновлении статуса платежа для заказа ({orderId}) не был найден МЛ");
						return false;
					}

					RouteList rl = routeLists.First();
					string token = rl.Driver.AndroidToken;

					if(string.IsNullOrWhiteSpace(token)) {
						logger.Warn($"Водителю ({rl.Driver.GetPersonNameWithInitials()}. Id:{rl.Driver.Id}) не присвоен Token для уведомлений.");
						return false;
					}
					driverNotificator.SendOrderPaymentStatusChangedMessage(token, "Веселый водовоз", "Обновлен статус платежа");
				}
				return true;
			}
			catch(Exception ex) {
				logger.Error(ex);
				return false;
			}
		}

		#endregion
	}
}