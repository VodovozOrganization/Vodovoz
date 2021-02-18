using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using System.Threading;
using System.Threading.Tasks;
using BitrixApi.DTO.DataContractJsonSerializer;
using BitrixApi.REST;
using BitrixIntegration.DTO.Mailjet;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Deal = BitrixApi.DTO.Deal;
using DealRequest = BitrixApi.DTO.DealRequest;
using Email = BitrixIntegration.DTO.Email;

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
			Task workerTask = Task.Factory.StartNew(() => ProcessDealCycle(), cancellationToken.Token);
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
			// IMailjetParametersProvider parametersProvider = new BaseParametersProvider();

			// try {
			// 	SetLoginSetting(parametersProvider.MailjetUserId, parametersProvider.MailjetSecretKey);
			// } catch(Exception ex) {
			// 	logger.Error(ex);
			// }
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
			// if(!emailRepository.CanSendByTimeout(email.Recipient.EmailAddress, email.Order, email.OrderDocumentType)) {
			// 	logger.Error("{0} Попытка отправить статус до истечения минимального времени до повторной отправки", GetThreadInfo());
			// 	throw new Exception("Отправка на один и тот же адрес возможна раз в 10 минут");
			// }
			//
			// logger.Debug("{0} Запись в базу информации о письме", GetThreadInfo());
			// using(var uow = UnitOfWorkFactory.CreateWithNewRoot<StoredEmail>($"[ES]Добавление письма на отправку")) {
			// 	//Заполнение нового письма данными
			// 	switch (email.OrderDocumentType)
			// 	{
			// 		case OrderDocumentType.Bill:
			// 			uow.Root.Order = uow.GetById<Order>(email.Order);
			// 			break;
			// 		case OrderDocumentType.BillWSForDebt:
			// 			uow.Root.OrderWithoutShipmentForDebt = uow.GetById<OrderWithoutShipmentForDebt>(email.Order);
			// 			break;
			// 		case OrderDocumentType.BillWSForAdvancePayment:
			// 			uow.Root.OrderWithoutShipmentForAdvancePayment = uow.GetById<OrderWithoutShipmentForAdvancePayment>(email.Order);
			// 			break;
			// 		case OrderDocumentType.BillWSForPayment:
			// 			uow.Root.OrderWithoutShipmentForPayment = uow.GetById<OrderWithoutShipmentForPayment>(email.Order);
			// 			break;
			// 	}
			// 	
			// 	uow.Root.DocumentType = email.OrderDocumentType;
			// 	uow.Root.SendDate = DateTime.Now;
			// 	uow.Root.StateChangeDate = DateTime.Now;
			// 	uow.Root.HtmlText = email.HtmlText;
			// 	uow.Root.Text = email.Text;
			// 	uow.Root.Title = email.Title;
			// 	uow.Root.State = StoredEmailStates.WaitingToSend;
			// 	uow.Root.SenderName = email.Sender.Title;
			// 	uow.Root.SenderAddress = email.Sender.EmailAddress;
			// 	uow.Root.RecipientName = email.Recipient.Title;
			// 	uow.Root.RecipientAddress = email.Recipient.EmailAddress;
			// 	uow.Root.ManualSending = email.ManualSending;
			// 	uow.Root.Author = email.AuthorId != 0 ? uow.GetById<Employee>(email.AuthorId) : null;
			// 	try {
			// 		uow.Save();
			// 	}
			// 	catch(Exception ex) {
			// 		logger.Debug(string.Format("{1} Ошибка при сохранении. Ошибка: {0}", ex.Message, GetThreadInfo()));
			// 		throw ex;
			// 	}
			// 	email.StoredEmailId = uow.Root.Id;
			// 	dealsQueue.Add(email);
			// 	logger.Debug("{0} Письмо добавлено в очередь на отправку. Писем в очереди: {1}", GetThreadInfo(), dealsQueue.Count);
			// 	logger.Debug("{0} Закончил работу.", GetThreadInfo());
			// }
		}

		//основной бесконечный цикл по обработке поступивших штук
		// скорее всего лучше переделать под цикл посылки изменившихся статусов заказов
		static async Task ProcessDealCycle()
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
		}

		static async void ProcessEvent(BitrixPostResponse bitrixEvent)
		{
			if (bitrixEvent != null){
				logger.Info("Поступил Event Bitrix с ");
				await cor.Process(bitrixEvent.Data.Fields.Id);
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

		private static string GetErrors(MailjetMessage[] messages)
		{
			string errorResult = "";
			foreach(var message in messages) {
				foreach(var error in message.Errors) {
					if(!string.IsNullOrWhiteSpace(errorResult)) {
						errorResult += "\n";
					}
					errorResult += string.Format("StatusCode: {0}, ErrorCode: {1}, Error message: {2}", error.StatusCode, error.ErrorCode, error.ErrorMessage);
				}
			}
			return errorResult;
		}

		private static MailjetRequest CreateMailjetRequest(Email email)
		{
			MailjetRequest request = new MailjetRequest {
				Resource = Send.Resource
			};
			var attachments = new JArray();
			foreach(var item in email.AttachmentsBinary) {
				attachments.Add(new JObject{
					{"ContentType", "application/octet-stream"},
					{"Filename", item.Key},
					{"Base64Content", item.Value}
				});
			}
			var inlinedAttachments = new JArray();
			foreach(var item in email.InlinedAttachments) {
				inlinedAttachments.Add(new JObject{
					{"ContentID", item.Key},
					{"ContentType", item.Value.ContentType},
					{"Filename", item.Value.FileName},
					{"Base64Content", item.Value.Base64String}
				});
			}
			var message = new JObject {
				{"From", new JObject {
						{"Email", email.Sender.EmailAddress},
						{"Name", email.Sender.Title}
					}
				},
				{"To", new JArray {
						new JObject {
							{"Email", email.Recipient.EmailAddress},
							{"Name", email.Recipient.Title}
						}
					}
				},
				{"Subject", email.Title},
				{"TextPart", email.Text},
				{"HTMLPart", email.HtmlText},
				{"CustomID", email.StoredEmailId.ToString()},
				{"Attachments", attachments},
				{"InlinedAttachments", inlinedAttachments},
				{"TrackOpens", "account_default"},
				{"TrackClicks", "account_default"}
			};

			request.Property(Send.Messages, new JArray { message });

			return request;
		}
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

