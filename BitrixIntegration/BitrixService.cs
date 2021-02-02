using System;
using System.Net;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using BitrixApi.DTO.DataContractJsonSerializer;
using BitrixApi.REST;
using BitrixIntegration.DTO.Mailjet;
using BitrixIntegration.ServiceInterfaces;
using QS.DomainModel.UoW;
using QS.Project.DB;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services;
using Email = BitrixIntegration.DTO.Email;

namespace BitrixIntegration
{
	public class BitrixService : IBitrixService, IMailjetEventService, IBitrixServiceWeb
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IBitrixServiceSettings bitrixServiceSettings;

		public BitrixService(IBitrixServiceSettings bitrixServiceSettings)
		{
			BitrixManager.Init();
			this.bitrixServiceSettings = bitrixServiceSettings ?? throw new ArgumentNullException(nameof(bitrixServiceSettings));
		}

		public void PostEvent(MailjetEvent content)
		{
			// BitrixManager.AddEvent(content);
			//
			// //Необходимо обязательно отправлять в ответ http code 200 - OK
			// WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
		}

		public Tuple<bool, string> SendEmail(Email mail)
		{
			return BitrixManager.AddEmail(mail);
		}

		public int Add(int a, int b)
		{
			return a + b;
		}
		
		//TODO gavr переделать на WCFные DTO
		public void PostEvent(BitrixApi.DTO.DealRequest content)
		{
			BitrixManager.AddEvent(content.Result);
			if (WebOperationContext.Current != null)
				WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
		}

		public async Task TestWithoutWebHook(string token)
		{
			/*
		 * 1. Сопоставление контрагента (по новому полю) BitrixId 
			1. Если не получилось сопоставить по BitrixId, то делаем запрос всех данных контрагента в битрикс
			2. По телефону из пришедших данных битрикса делаем сопоставление
			  2.1 Приводим его к формату 9999999999
			  2.2 Ищем телефон в нашей базе по полю digit_number
			  2.3 Выбираем контрагента (если это физик то ищем его у нас по телефону + фамилии(только фамилии чтобы исключить дубли в тех случаях когда допущены ошибки в имени    или отчестве) иначе если это юрик то по телефону + целиком строка названия компании ) и заполняем у него BitrixId (чтобы в следующий раз он нашелся без сопоставления)
			3. Если не нашли то создаем нового контрагента (тоже с  BitrixId)
		 */
			var uowFactory = new DefaultUnitOfWorkFactory(new DefaultSessionProvider());
			var uow = uowFactory.CreateWithoutRoot();

			// using (uow){
				var restApi = BitrixRestApiFabric.CreateBitrixRestApi(token);
				var deal = await restApi.GetDealAsync(138788);

				// Если у нас есть заказ отправляем 200
				var matchedOrder = Matcher.MatchOrderByBitrixId(uow, deal);
				if (matchedOrder != null) {
					//TODO gavr а это точно отправка 200 по WCFфовски?
					WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
					return;
				}
				else{
					// Сопоставляем контрагента
					var contact = await restApi.GetContact(deal.ContancId);
					Matcher.MatchContact(uow, contact);
					// не сошелся, создаем контрагента
					// CreateCounterpartyFromContact(contact)
				
					//Сопоставляем Точку доставки
					// var deliveryPoint = await restApi.GetDeliveryPoint(???);
					// Matcher.MatchContact(uow, contact);
					// не сошелся, создаем Точку доставки
					// CreateDeliveryPointFrom???(???)
				
					//Сопоставляем Товары
					// var OrderItems = await restApi.GetContact(deal.ContancId);
					// Matcher.MatchContact(uow, contact);
					// не сошелись группы, создаем группы, создаем товары
					// CreateOrderGroupFrom???(???)
					// не сошелись товары, создаем товары
					// CreateOrderItemsFrom???(???)
				
					
					//Создаем заказ
				}
			// }
			
			// if (matchedOrder == null)
			// {
			// 	
			// 	//Пытаемся сопоставить
			// 	if ("Получилось сопоставить с одним из существующих")
			// 	{
			// 		// Добавляем ему этот bitrixId
			// 	}
			// 	else
			// 	{
			// 		bool deliveryPointFound = false;
			// 		bool orderFound = false;
			// 		
			//
			// 		#region Сопоставление заказа
			//
			// 		//Сопоставить по bitrixId не получилос, но может быть он у нас есть но не сопоставлен
			// 		//Делая запрос всех необходимых данных этого заказа
			// 		//Достаем из них телефон и приводим к формату 9999999999
			// 		//Ищем телефон в нашей базе по полю digit_number
			//
			// 		#endregion Сопоставление заказа
			//
			// 		#region Сопоставление точки доставки
			//
			//
			//
			// 		#endregion Сопоставление точки доставки
			//
			// 	}
			// }
			// else
			// {
			// 	//У нас уже есть такой заказ и делать ничего не надо
			// 	//Возвращаем 200
			// }
		}

		public void OnCrmDealUpdate(DealRequest dealRequest)
		{
			/*
			 * 1. Сопоставление контрагента (по новому полю) BitrixId 
				1. Если не получилось сопоставить по BitrixId, то делаем запрос всех данных контрагента в битрикс
				2. По телефону из пришедших данных битрикса делаем сопоставление
				  2.1 Приводим его к формату 9999999999
				  2.2 Ищем телефон в нашей базе по полю digit_number
				  2.3 Выбираем контрагента (если это физик то ищем его у нас по телефону + фамилии(только фамилии чтобы исключить дубли в тех случаях когда допущены ошибки в имени    или отчестве) иначе если это юрик то по телефону + целиком строка названия компании ) и заполняем у него BitrixId (чтобы в следующий раз он нашелся без сопоставления)
				3. Если не нашли то создаем нового контрагента (тоже с  BitrixId)
			 */



			// var deal = dealRequest.Result;
			// using (var uow = UnitOfWorkFactory.CreateWithNewRoot<Order>($"[ES]Добавление письма на отправку"))
			// {
			//
			// switch (deal.OrderDocumentType)
			// {
			// 	case OrderDocumentType.Bill:
			// 		uow.Root.Order = uow.GetById<Order>(deal.Order);
			// 		break;
			// 	case OrderDocumentType.BillWSForDebt:
			// 		uow.Root.OrderWithoutShipmentForDebt = uow.GetById<OrderWithoutShipmentForDebt>(deal.Order);
			// 		break;
			// 	case OrderDocumentType.BillWSForAdvancePayment:
			// 		uow.Root.OrderWithoutShipmentForAdvancePayment =
			// 			uow.GetById<OrderWithoutShipmentForAdvancePayment>(deal.Order);
			// 		break;
			// 	case OrderDocumentType.BillWSForPayment:
			// 		uow.Root.OrderWithoutShipmentForPayment =
			// 			uow.GetById<OrderWithoutShipmentForPayment>(deal.Order);
			// 		break;
			// }
			//
			// uow.Root.DocumentType = email.OrderDocumentType;
			// uow.Root.SendDate = DateTime.Now;
			// uow.Root.StateChangeDate = DateTime.Now;
			// uow.Root.HtmlText = email.HtmlText;
			// uow.Root.Text = email.Text;
			// uow.Root.Title = email.Title;
			// uow.Root.State = StoredEmailStates.WaitingToSend;
			// uow.Root.SenderName = email.Sender.Title;
			// uow.Root.SenderAddress = email.Sender.EmailAddress;
			// uow.Root.RecipientName = email.Recipient.Title;
			// uow.Root.RecipientAddress = email.Recipient.EmailAddress;
			// uow.Root.ManualSending = email.ManualSending;
			// uow.Root.Author = email.AuthorId != 0 ? uow.GetById<Employee>(email.AuthorId) : null;
			// try {
			// 	uow.Save();
			// }
			// catch(Exception ex) {
			// 	logger.Debug(string.Format("{1} Ошибка при сохранении.", ex.Message, GetThreadInfo()));
			// 	throw ex;
			// }

			// }

		}

		public bool ServiceStatus()
		{
			int emailsInQueue = BitrixManager.GetEmailsInQueue();
			if(emailsInQueue > bitrixServiceSettings.MaxEmailsInQueueForWorkingService) {
				return false;
			}
			return true;
		}
	}
}
