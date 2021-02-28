using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BitrixApi.DTO.DataContractJsonSerializer;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Deal = BitrixApi.DTO.Deal;
using DealRequest = BitrixApi.DTO.DealRequest;

namespace BitrixIntegration
{
	public static class BitrixManager
	{
		static Logger logger = LogManager.GetCurrentClassLogger();
		static int MaxSendAttemptsCount = 5;
		static int MaxEventSaveAttemptsCount = 5;
		
		static string token = null;
		static CancellationTokenSource cancellationToken = new CancellationTokenSource();
		//Очередь на обработку сделок
		//TODO gavr переделать на коллекцию объектов содержащих статус и заказ и менять статусы у данных заказов в битрикс
		static BlockingCollection<Deal> dealsQueue = new BlockingCollection<Deal>(); 
		//Очередь необработанных
		static BlockingCollection<DealRequest> unsavedEventsQueue = new BlockingCollection<DealRequest>();
		static bool IsInitialized => !(string.IsNullOrWhiteSpace(token));
		static int workerTasksCreatedCounter = 0;
		static IEmailRepository emailRepository = new EmailRepository();
		private static ICoR cor;
		

		static BitrixManager()
		{
			//Установка всем не отправленным письмам из базы статус ошибки отправки в связи с остановкой сервера.
			// Task.Run(() => SetErrorStatusWaitingToSendEmails());

			//Запуск воркеров по обработке заказов
			// StartNewWorker();
			// StartNewWorker();
			// StartNewWorker();
			
			//Запуск воркера по пересохранению ошибочных событий
			// Task.Run(() => ResaveEventWork());
		}

		private static void StartNewWorker()
		{
			workerTasksCreatedCounter++;
			logger.Info("Запуск новой задачи по отправке писем");
			Task workerTask = Task.Factory.StartNew(ProcessDealCycle, cancellationToken.Token);
			// Перезапуск тасков на случай если вылетели из while true
			workerTask.ContinueWith((task) => StartNewWorker());
		}

		public static void StopWorkers()
		{
			cancellationToken.Cancel();
		}

		public static void Init()
		{
			if(IsInitialized) {
				return;
			}
			logger.Error("Token не выставлен");
		}

		public static int CountOrderNewStatusesInQueue()
		{
			return dealsQueue.Count;
		}

		public static void AddEvent(BitrixPostResponse bitrixEvent)
		{
			logger.Info("Получен евент");
			Task.Run(() => {
				Thread.CurrentThread.Name = "AddEventWork";
				ProcessEvent(bitrixEvent);
			});
		}

		public static Tuple<bool, string> sendOrderStatusToBitrix(OrderStatus status, Order order)
		{
			Thread.CurrentThread.Name = "AddNewEmail";
			logger.Debug("Thread {0} Id {1}: Получен новый статус для обновления в битриксе", 
				Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId);
			try {
				SendNewOrderStatus(status, order);
			}
			catch(Exception ex) {
				return new Tuple<bool, string>(false, ex.Message);
			}
			return new Tuple<bool, string>(true, "Письмо добавлено в очередь на отправку");
		}


		#region Ненужное вроде как

		/// <summary> 
		/// Устанавливает статус ошибки отправки, для всех писем в ожидании отправки.
		/// </summary>
		static void SetErrorStatusWaitingToSendEmails()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"[ES]Установка статуса ошибки всем ожидающим")) {
				logger.Debug("Загрузка из базы почты ожидающей отправки");
				var waitingEmails = uow.Session.QueryOver<StoredEmail>()
					.Where(x => x.State == StoredEmailStates.WaitingToSend)
					.List<StoredEmail>();
				foreach(var email in waitingEmails) {
					email.State = StoredEmailStates.SendingError;
					email.AddDescription("Отправка прервана остановкой сервера.");
					uow.Save(email);
				}
				uow.Commit();
			}
		}

		#endregion Ненужное вроде как

		//Добавление новых сделок в очередь на обработку
		static void AddEmailToSend(Deal deal)
		{
			//TODO gavr может делать тут пред обработку, например проверить есть ли у нас такой заказ
			
			
			dealsQueue.Add(deal);
			
			logger.Debug("{0} Сделка добавлена в очередь на обработку. Сделок в очереди: {1}", GetThreadInfo(), dealsQueue.Count);
			logger.Debug("{0} Закончил работу.", GetThreadInfo());
		}


		static void SendNewOrderStatus(OrderStatus newStatus, Order forOrder)
		{
			//TODO gavr отправка статус в битрикс должна быть в репозитории здесь
			
			throw new NotImplementedException();
			
		}

		//основной бесконечный цикл по обработке поступивших штук
		// скорее всего лучше переделать под цикл посылки изменившихся статусов заказов
		static void ProcessDealCycle()
		{
			Thread.CurrentThread.Name = "EmailSendWorker";
			while(true) {
				Deal deal = null;

				deal = dealsQueue.Take();
				logger.Debug("{0} Обработка сделки из очереди из очереди", GetThreadInfo());

				Thread.Sleep(1000);

				if(deal == null) {
					continue;
				}

				try {
					// обработка запроса
				}
				catch(Exception ex) {
					logger.Error(ex, "При обработке ответа на отправку письма возникла ошибка.\n");
				}
				
			}
			throw new NotImplementedException();
			
		}

		static async void ProcessEvent(BitrixPostResponse bitrixEvent)
		{
			
			if (bitrixEvent != null){
				logger.Info("Поступил Event Bitrix с ");
				// await cor.Process(bitrixEvent.Fields.Id);
			}
			else{
				logger.Error("Event Bitrix == null");
				//TODO gavr вылет?
			}
			
			
			// else{
			// 	foreach (var bitrixEventPayload in bitrixEvent.Payloads){
			// 		foreach (var field in bitrixEventPayload.Field){
			// 			await cor.Process(field.Id);
			// 			Thread.Sleep(1000);
			// 		}
			// 	}
			// }
		}

		static void TryResaveEvent(Deal unsavedEvent)
		{
			Thread.CurrentThread.Name = "ResaveEventWork";
			//Попытка пересохранения каждые 20 сек.
			Thread.Sleep(20000);
			// ProcessEvent(unsavedEvent);
			//TODO gavr тут передача обработки нового статуса для посылки в битрикс
		}

		#region Service methods

		private static string GetThreadInfo()
		{
			return string.Format("Thread {0} Id {1}:", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId);
		}

		private static void SaveErrorInfo(IUnitOfWorkGeneric<StoredEmail> uow, string errorInfo)
		{
			uow.Root.State = StoredEmailStates.SendingError;
			uow.Root.AddDescription(errorInfo);
			uow.Save();
		}

		// private static string GetErrors(MailjetMessage[] messages)
		// {
		// 	string errorResult = "";
		// 	foreach(var message in messages) {
		// 		foreach(var error in message.Errors) {
		// 			if(!string.IsNullOrWhiteSpace(errorResult)) {
		// 				errorResult += "\n";
		// 			}
		// 			errorResult += string.Format("StatusCode: {0}, ErrorCode: {1}, Error message: {2}", error.StatusCode, error.ErrorCode, error.ErrorMessage);
		// 		}
		// 	}
		// 	return errorResult;
		// }

	
		#region На выброс
		// private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		// {
		// 	DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		// 	dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
		// 	return dtDateTime;
		// }
		#endregion На выброс
	

		public static void SetToken(string _token)
		{
			if(IsInitialized) {
				return;
			}
			token = _token;
		}

		public static void SetCoR(ICoR _cor)
		{
			cor = _cor;
		}

		#endregion
	}
}

