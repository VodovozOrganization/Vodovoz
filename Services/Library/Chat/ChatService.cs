using System;
using System.Collections.Generic;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Chats;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Chats;
using Vodovoz.EntityRepositories.Employees;
using ChatClass = Vodovoz.Domain.Chats.Chat;

namespace Chats
{
	public class ChatService : IChatService
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly IChatMessageRepository _chatMessageRepository = new ChatMessageRepository();
		private static readonly IChatRepository _chatRepository = new ChatRepository();
		
		public static string UserNameOfServer = "Электронный друг";

		#region IChatService implementation

		public bool SendMessageToLogistician (string authKey, string message)
		{
			try {
				using (var uow = UnitOfWorkFactory.CreateWithoutRoot($"[CS]Отправка сообщения логисту"))
				{
					var driver = new EmployeeRepository().GetDriverByAuthKey(uow, authKey);
					if (driver == null)
						return false;

					var chat = _chatRepository.GetChatForDriver(uow, driver);
					if (chat == null)
					{
						chat = new ChatClass();
						chat.ChatType = ChatType.DriverAndLogists;
						chat.Driver = driver;
					}

					ChatMessage chatMessage = new ChatMessage();
					chatMessage.Chat = chat;
					chatMessage.DateTime = DateTime.Now;
					chatMessage.Message = message;
					chatMessage.Sender = driver;

					chat.Messages.Add(chatMessage);
					uow.Save(chat);
					uow.Commit();
					return true;
				}
			} catch (Exception e) {
				logger.Error (e);
				return false;
			}
		}

		public bool SendMessageToDriver (int senderId, int recipientId, string message)
		{
			try
			{
				using (var senderUoW = UnitOfWorkFactory.CreateForRoot<Employee>(senderId, $"[CS]Отправка сообщения водителю {recipientId}"))
				{
					var recipient = senderUoW.GetById<Employee>(recipientId);

					var chat = _chatRepository.GetChatForDriver(senderUoW, recipient);
					if (chat == null)
					{
						chat = new ChatClass();
						chat.ChatType = ChatType.DriverAndLogists;
						chat.Driver = recipient;
					}

					ChatMessage chatMessage = new ChatMessage();
					chatMessage.Chat = chat;
					chatMessage.DateTime = DateTime.Now;
					chatMessage.Message = message;
					chatMessage.Sender = senderUoW.Root;

					chat.Messages.Add(chatMessage);
					senderUoW.Save(chat);
					senderUoW.Commit();

					FCMHelper.SendMessage(recipient.AndroidToken, senderUoW.Root.ShortName, message);
					return true;
				}
			} catch (Exception e) {
				logger.Error (e);
				return false;
			}
		}

		public List<MessageDTO> AndroidGetChatMessages (string authKey, int days)
		{
			try {
				using (var uow = UnitOfWorkFactory.CreateWithoutRoot($"[CS]Получение сообщений чата"))
				{
					var driver = new EmployeeRepository().GetDriverByAuthKey(uow, authKey);
					if (driver == null)
						return null;

					var chat = _chatRepository.GetChatForDriver(uow, driver);
					if (chat == null)
						return null;
					var messages = new List<MessageDTO>();
					var chatMessages = _chatMessageRepository.GetChatMessagesForPeriod(uow, chat, days);
					foreach (var m in chatMessages)
					{
						messages.Add(new MessageDTO(m, driver));
					}
					return messages;
				}
			} catch (Exception e) {
				logger.Error (e);
				return null;
			}
		}

		public bool SendOrderStatusNotificationToDriver (int senderId, int routeListItemId) {
			try {
				using (var senderUoW = UnitOfWorkFactory.CreateForRoot<Employee>(senderId, $"[CS]Отправка сообщения о изменении статуса заказа"))
				{
					var routeListItem = senderUoW.GetById<RouteListItem>(routeListItemId);
					var driver = routeListItem.RouteList.Driver;

					if (driver == null)
						return false;

					var chat = _chatRepository.GetChatForDriver(senderUoW, driver);
					if (chat == null)
					{
						chat = new ChatClass();
						chat.ChatType = ChatType.DriverAndLogists;
						chat.Driver = driver;
					}

					ChatMessage chatMessage = new ChatMessage();
					chatMessage.Chat = chat;
					chatMessage.DateTime = DateTime.Now;
					chatMessage.IsAutoCeated = true;
					chatMessage.Message = String.Format("Заказ №{0} из маршрутного листа №{1} был переведен в статус \"{2}\".",
						routeListItem.Order.Id,
						routeListItem.RouteList.Id,
						routeListItem.Status.GetEnumTitle());
					chatMessage.Sender = senderUoW.Root;

					chat.Messages.Add(chatMessage);
					senderUoW.Save(chat);
					senderUoW.Commit();
					var message = String.Format("Изменение статуса заказа №{0}", routeListItem.Order.Id);

					FCMHelper.SendOrderStatusChangeMessage(driver.AndroidToken, senderUoW.Root.ShortName, message);
					return true;
				}
			} catch (Exception e) {
				logger.Error (e);
				return false;
			}
		}

		public bool SendDeliveryScheduleNotificationToDriver (int senderId, int routeListItemId) {
			try {
				using (var senderUoW = UnitOfWorkFactory.CreateForRoot<Employee>(senderId, $"[CS]Отправка сообщения о изменении времени заказа"))
				{
					var routeListItem = senderUoW.GetById<RouteListItem>(routeListItemId);
					var driver = routeListItem.RouteList.Driver;

					if (driver == null)
						return false;

					var chat = _chatRepository.GetChatForDriver(senderUoW, driver);
					if (chat == null)
					{
						chat = new ChatClass();
						chat.ChatType = ChatType.DriverAndLogists;
						chat.Driver = driver;
					}

					ChatMessage chatMessage = new ChatMessage();
					chatMessage.Chat = chat;
					chatMessage.DateTime = DateTime.Now;
					chatMessage.IsAutoCeated = true;
					chatMessage.Message = String.Format("У заказа №{0} из маршрутного листа №{1} было изменено время доставки на \"{2}\".",
						routeListItem.Order.Id,
						routeListItem.RouteList.Id,
						routeListItem.Order.DeliverySchedule.DeliveryTime);
					chatMessage.Sender = senderUoW.Root;

					chat.Messages.Add(chatMessage);
					senderUoW.Save(chat);
					senderUoW.Commit();
					var message = String.Format("Изменение времени доставки заказа №{0}", routeListItem.Order.Id);

					FCMHelper.SendOrderDeliveryScheduleChangeMessage(driver.AndroidToken, senderUoW.Root.ShortName, message);
					return true;
				}
			} catch (Exception e) {
				logger.Error (e);
				return false;
			}
		}
		#endregion

		/// <summary>
		/// Sends the server notification to driver.
		/// ВНИМАНИЕ!!! Делает коммит UoW.
		/// </summary>
		public static bool SendServerNotificationToDriver (IUnitOfWork uow, Employee driver, string message, string androidNotification) {
			try {
				if (driver == null)
					return false;

				var chat = _chatRepository.GetChatForDriver(uow, driver);
				if (chat == null) {
					chat = new ChatClass ();
					chat.ChatType = ChatType.DriverAndLogists;
					chat.Driver = driver;
				}

				ChatMessage chatMessage = new ChatMessage ();
				chatMessage.Chat = chat;
				chatMessage.DateTime = DateTime.Now;
				chatMessage.Message = message;
				chatMessage.IsServerNotification = true;
				chatMessage.IsAutoCeated = true;

				chat.Messages.Add (chatMessage);
				uow.Save (chat);
				uow.Commit ();

				FCMHelper.SendOrderStatusChangeMessage (driver.AndroidToken, UserNameOfServer, androidNotification);
				return true;
			} catch (Exception e) {
				logger.Error (e);
				return false;
			}
		}

	}
}

