using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EmailService.Mailjet;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace EmailService
{
	public static class EmailManager
	{
		static Logger logger = LogManager.GetCurrentClassLogger();
		static int MaxSendAttemptsCount = 5;
		static int MaxEventSaveAttemptsCount = 5;
		static string userId = null;
		static string userSecretKey = null;
		static CancellationTokenSource cancellationToken = new CancellationTokenSource();
		static BlockingCollection<OrderEmail> emailsQueue = new BlockingCollection<OrderEmail>();
		static BlockingCollection<MailjetEvent> unsavedEventsQueue = new BlockingCollection<MailjetEvent>();
		static bool IsInitialized => !(string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(userSecretKey));
		static int workerTasksCreatedCounter = 0;
		static IEmailRepository emailRepository = new EmailRepository();


		static EmailManager()
		{
			//Установка всем не отправленным письмам из базы статус ошибки отправки в связи с остановкой сервера.
			Task.Run(() => SetErrorStatusWaitingToSendEmails());

			//Запуск воркеров по отправке писем
			StartNewWorker();
			StartNewWorker();
			StartNewWorker();

			//Запуск воркера по пересохранению ошибочных событий
			Task.Run(() => ResaveEventWork());
		}

		private static void StartNewWorker()
		{
			workerTasksCreatedCounter++;
			logger.Info("Запуск новой задачи по отправке писем");
			Task workerTask = Task.Factory.StartNew(() => ProcessEmailMailjet(), cancellationToken.Token);
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

			IMailjetParametersProvider parametersProvider = new BaseParametersProvider(new ParametersProvider());

			try {
				SetLoginSetting(parametersProvider.MailjetUserId, parametersProvider.MailjetSecretKey);
			} catch(Exception ex) {
				logger.Error(ex);
			}
		}

		public static int GetEmailsInQueue()
		{
			return emailsQueue.Count;
		}

		public static void AddEvent(MailjetEvent mailjetEvent)
		{
			Task.Run(() => {
				Thread.CurrentThread.Name = "AddEventWork";
				ProcessEvent(mailjetEvent);
			});
		}

		public static Tuple<bool, string> AddEmail(OrderEmail email)
		{
			Thread.CurrentThread.Name = "AddNewEmail";
			logger.Debug("Thread {0} Id {1}: Получено новое письмо на отправку", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId);
			try {
				AddEmailToSend(email);
			}
			catch(Exception ex) {
				return new Tuple<bool, string>(false, ex.Message);
			}
			return new Tuple<bool, string>(true, "Письмо добавлено в очередь на отправку");
		}


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

		static void AddEmailToSend(OrderEmail email)
		{
			if(!emailRepository.CanSendByTimeout(email.Recipient.EmailAddress, email.Order, email.OrderDocumentType)) {
				logger.Error("{0} Попытка отправить почту до истечения минимального времени до повторной отправки", GetThreadInfo());
				throw new Exception("Отправка на один и тот же адрес возможна раз в 10 минут");
			}

			logger.Debug("{0} Запись в базу информации о письме", GetThreadInfo());
			using(var uow = UnitOfWorkFactory.CreateWithNewRoot<StoredEmail>($"[ES]Добавление письма на отправку")) {
				//Заполнение нового письма данными
				switch (email.OrderDocumentType)
				{
					case OrderDocumentType.Bill:
						uow.Root.Order = uow.GetById<Order>(email.Order);
						break;
					case OrderDocumentType.BillWSForDebt:
						uow.Root.OrderWithoutShipmentForDebt = uow.GetById<OrderWithoutShipmentForDebt>(email.Order);
						break;
					case OrderDocumentType.BillWSForAdvancePayment:
						uow.Root.OrderWithoutShipmentForAdvancePayment = uow.GetById<OrderWithoutShipmentForAdvancePayment>(email.Order);
						break;
					case OrderDocumentType.BillWSForPayment:
						uow.Root.OrderWithoutShipmentForPayment = uow.GetById<OrderWithoutShipmentForPayment>(email.Order);
						break;
				}
				
				uow.Root.DocumentType = email.OrderDocumentType;
				uow.Root.SendDate = DateTime.Now;
				uow.Root.StateChangeDate = DateTime.Now;
				uow.Root.State = StoredEmailStates.WaitingToSend;
				uow.Root.RecipientAddress = email.Recipient.EmailAddress;
				uow.Root.ManualSending = email.ManualSending;
				uow.Root.Author = email.AuthorId != 0 ? uow.GetById<Employee>(email.AuthorId) : null;
				try {
					uow.Save();
				}
				catch(Exception ex) {
					logger.Debug(string.Format("{1} Ошибка при сохранении. Ошибка: {0}", ex.Message, GetThreadInfo()));
					throw ex;
				}
				email.StoredEmailId = uow.Root.Id;
				emailsQueue.Add(email);
				logger.Debug("{0} Письмо добавлено в очередь на отправку. Писем в очереди: {1}", GetThreadInfo(), emailsQueue.Count);
				logger.Debug("{0} Закончил работу.", GetThreadInfo());
			}
		}

		static async Task ProcessEmailMailjet()
		{
			Thread.CurrentThread.Name = "EmailSendWorker";
			while(true) {
				OrderEmail email = null;

				email = emailsQueue.Take();
				logger.Debug("{0} Отправка письма из очереди", GetThreadInfo());

				Thread.Sleep(1000);

				if(email == null) {
					continue;
				}

				if(email.StoredEmailId == 0) {
					logger.Debug("{0} Письмо не было сохранено перед добавлением в очередь. Добавлено повторно в очередь", GetThreadInfo());
					AddEmailToSend(email);
					continue;
				}

				using(var uow = UnitOfWorkFactory.CreateForRoot<StoredEmail>(email.StoredEmailId, $"[ES]Задача отправки через Mailjet")) {
					MailjetClient client = new MailjetClient(userId, userSecretKey) {
						Version = ApiVersion.V3_1,
					};
					try {
						//формируем письмо в формате mailjet для отправки
						var request = CreateOrderMailjetRequest(email);
						MailjetResponse response = null;
						try {
							logger.Debug("{0} Отправка запроса на сервер Mailjet", GetThreadInfo());
							response = await client.PostAsync(request);
						}
						catch(Exception ex) {
							logger.Error("{1} Не удалось отправить письмо: \n{0}", ex, GetThreadInfo());
							SaveErrorInfo(uow, ex.ToString());
							continue;
						}

						MailjetMessage[] messages = response.GetData().ToObject<MailjetMessage[]>();

						logger.Debug("{1} Получен ответ: Code {0}", response.StatusCode, GetThreadInfo());
						
						if(response.IsSuccessStatusCode) {
							uow.Root.State = StoredEmailStates.SendingComplete;
							foreach(var message in messages) {
								if(message.CustomID == uow.Root.Id.ToString()) {
									foreach(var messageTo in message.To) {
										if(messageTo.Email == email.Recipient.EmailAddress) {
											uow.Root.ExternalId = messageTo.MessageID.ToString();
										}
									}
								}
							}
							uow.Save();
							logger.Debug(response.GetData());
						} else {
							switch(response.StatusCode) {

							//Unauthorized
							//Incorrect Api Key / API Secret Key or API key may be expired.
							case 401:
								Init();
								if(email.SendAttemptsCount >= MaxSendAttemptsCount) {
									SaveErrorInfo(uow, GetErrors(messages));
								} else {
									emailsQueue.Add(email);
								}
								break;

							//Too Many Requests
							//Reach the maximum number of calls allowed per minute.
							case 429:
								if(email.SendAttemptsCount >= MaxSendAttemptsCount) {
									SaveErrorInfo(uow, GetErrors(messages));
								} else {
									emailsQueue.Add(email);
								}
								break;

							//Internal Server Error
							case 500:
								SaveErrorInfo(uow, string.Format("Внутренняя ошибка сервера Mailjet: {0}", GetErrors(messages)));
								break;
							default:
								SaveErrorInfo(uow, GetErrors(messages));
								break;
							}

							logger.Debug(response.GetData());
							logger.Debug("{1} ErrorMessage: {0}\n", response.GetErrorMessage(), GetThreadInfo());
						}
					}
					catch(Exception ex) {
						logger.Error(ex, "При обработке ответа на отправку письма возникла ошибка.\n");
					}
				}
			}
		}

		static void ProcessEvent(MailjetEvent mailjetEvent)
		{
			if(mailjetEvent.AttemptCount > 0) {
				logger.Debug("{1} Повторная обработка события с сервера Mailjet. Попытка {2}/{3} \n{0}", mailjetEvent, GetThreadInfo(), mailjetEvent.AttemptCount, MaxSendAttemptsCount);
			} else {
				logger.Debug("{1} Обработка события с сервера Mailjet \n{0}", mailjetEvent, GetThreadInfo());
			}

			//Запись информации о письме в базу
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"[ES]Обработка события Mailjet")) {
				var emailAction = emailRepository.GetStoredEmailByMessageId(uow, mailjetEvent.MessageID.ToString());
				if(emailAction == null) {
					int mailId;
					if(int.TryParse(mailjetEvent.CustomID, out mailId)) {
						emailAction = uow.GetById<StoredEmail>(mailId);
					}
				}
				if(emailAction != null) {
					var eventDate = UnixTimeStampToDateTime(mailjetEvent.Time);
					if(eventDate > emailAction.StateChangeDate) {
						emailAction.StateChangeDate = eventDate;
						switch(mailjetEvent.Event) {
						case "sent":
							emailAction.State = StoredEmailStates.Delivered;
							break;
						case "open":
							emailAction.State = StoredEmailStates.Opened;
							break;
						case "spam":
							emailAction.State = StoredEmailStates.MarkedAsSpam;
							break;
						case "bounce":
						case "blocked":
							emailAction.State = StoredEmailStates.Undelivered;
							emailAction.AddDescription(mailjetEvent.GetErrorInfo());
							break;
						}
						try {
							uow.Save(emailAction);
							uow.Commit();
						}
						catch(Exception ex) {
							mailjetEvent.AttemptCount++;
							if(mailjetEvent.AttemptCount <= MaxEventSaveAttemptsCount) {
								unsavedEventsQueue.Add(mailjetEvent);
							}
							logger.Error("{1} Произошла ошибка при сохранении: {0}", ex.Message, GetThreadInfo());
						}
					}
				} else {
					logger.Error("{0} Событие проигнорировано. Не найдено письмо в БД связанное с событием с сервера Mailjet.", GetThreadInfo());
				}
			}
		}

		static void ResaveEventWork()
		{
			Thread.CurrentThread.Name = "ResaveEventWorkStarter";
			while(true) {
				MailjetEvent unsavedEvent = unsavedEventsQueue.Take();
				logger.Debug("{1} Взято не сохраненное событие из очереди для попытки пересохранения. Оставшееся кол-во событий в очереди: {0}", unsavedEventsQueue.Count, GetThreadInfo());
				Task.Run(() => TryResaveEvent(unsavedEvent));
			}
		}

		static void TryResaveEvent(MailjetEvent unsavedEvent)
		{
			Thread.CurrentThread.Name = "ResaveEventWork";
			//Попытка пересохранения каждые 20 сек.
			Thread.Sleep(20000);
			ProcessEvent(unsavedEvent);
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

		private static MailjetRequest CreateOrderMailjetRequest(OrderEmail email)
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


		private static MailjetRequest CreateMailjetRequest(Email email)
		{
			MailjetRequest request = new MailjetRequest
			{
				Resource = Send.Resource
			};
			var attachments = new JArray();
			if(email.AttachmentsBinary != null)
            {
				foreach (var item in email.AttachmentsBinary)
				{
					attachments.Add(new JObject{
						{"ContentType", "application/octet-stream"},
						{"Filename", item.Key},
						{"Base64Content", item.Value}
					});
				}
			}
			
			var inlinedAttachments = new JArray();
			if (email.InlinedAttachments != null)
            {
				foreach (var item in email.InlinedAttachments)
				{
					inlinedAttachments.Add(new JObject{
						{"ContentID", item.Key},
						{"ContentType", item.Value.ContentType},
						{"Filename", item.Value.FileName},
						{"Base64Content", item.Value.Base64String}
					});
				}
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

		public static async Task<bool> SendEmail(Email email)
        {
			try
			{
				MailjetClient client = new MailjetClient(userId, userSecretKey)
				{
					Version = ApiVersion.V3_1,
				};

				var request = CreateMailjetRequest(email);

				MailjetResponse response = null;

				try
				{
					logger.Debug("{0} Отправка запроса на сервер Mailjet", GetThreadInfo());
					response = await client.PostAsync(request);
				}
				catch (Exception ex)
				{
					logger.Error("{1} Не удалось отправить письмо: \n{0}", ex, GetThreadInfo());
					return false;
				}

				MailjetMessage[] messages = response.GetData().ToObject<MailjetMessage[]>();

				logger.Debug("{1} Получен ответ: Code {0}", response.StatusCode, GetThreadInfo());

				if (response.IsSuccessStatusCode)
				{
					logger.Debug(response.GetData());
					return true;
				}
				else
				{
					logger.Debug(response.GetData());
					logger.Debug("{1} ErrorMessage: {0}\n", response.GetErrorMessage(), GetThreadInfo());
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, "При обработке ответа на отправку письма возникла ошибка.\n");
			}
			return false;
		}

		private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
			return dtDateTime;
		}

		private static void ClearLoginSetting()
		{
			userId = null;
			userSecretKey = null;
		}

		private static void SetLoginSetting(string id, string key)
		{
			if(IsInitialized) {
				return;
			}
			userId = id;
			userSecretKey = key;
		}

		#endregion
	}
}
