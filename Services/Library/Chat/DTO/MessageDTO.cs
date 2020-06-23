using System;
using System.Runtime.Serialization;
using Vodovoz.Domain.Chats;
using Vodovoz.Domain.Employees;

namespace Chats
{
	[DataContract]
	public class MessageDTO
	{
		//Сообщение
		[DataMember]
		public string Message;

		//Отправитель
		[DataMember]
		public string Sender;

		//Дата и время
		[DataMember]
		public DateTime DateTime;

		public MessageDTO (ChatMessage item)
		{
			Message = item.Message;
			Sender = item.IsServerNotification ? ChatService.UserNameOfServer : item.SenderName;
			DateTime = item.DateTime;
		}

		public MessageDTO (ChatMessage item, Employee driver) : this (item)
		{
			if (!item.IsServerNotification && item.Sender.Id == driver.Id)
				Sender = String.Empty;
		}
	}
}

