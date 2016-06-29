using System;
using QSOrmProject;
using Vodovoz.Domain.Chat;
using Vodovoz.Domain.Employees;
using ChatClass = Vodovoz.Domain.Chat.Chat;

namespace Vodovoz.Repository.Chat
{
	public static class LastReadedRepository
	{
		public static LastReadedMessage GetLastReadedMessageForEmloyee(IUnitOfWork uow, ChatClass chat, Employee employee) {
			LastReadedMessage lastReadedAlias = null;

			return uow.Session.QueryOver<LastReadedMessage> (() => lastReadedAlias)
				.Where (() => lastReadedAlias.Chat.Id == chat.Id)
				.Where (() => lastReadedAlias.Employee.Id == employee.Id)
				.SingleOrDefault();
		}

		public static int GetLastReadedMessagesCountForEmployee(IUnitOfWork uow, ChatClass chat, Employee employee) {
			LastReadedMessage lastReadedAlias = null;
			ChatMessage chatMessageAlias = null;
			var lastMessage = GetLastReadedMessageForEmloyee(uow, chat, employee);

			var query = uow.Session.QueryOver<ChatMessage>(() => chatMessageAlias)
				.Where(() => chatMessageAlias.Chat.Id == chat.Id);
			if (lastMessage == null)
				return query.RowCount();
			else
				return query
					.Where(() => chatMessageAlias.DateTime > lastMessage.LastDateTime)
					.RowCount();
		}
	}
}

