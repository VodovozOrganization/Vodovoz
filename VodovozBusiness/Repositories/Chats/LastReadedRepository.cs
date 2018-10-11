using QS.DomainModel.UoW;
using Vodovoz.Domain.Chats;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Repository.Chats
{
	public static class LastReadedRepository
	{
		public static LastReadedMessage GetLastReadedMessageForEmloyee(IUnitOfWork uow, Chat chat, Employee employee) {
			LastReadedMessage lastReadedAlias = null;

			return uow.Session.QueryOver<LastReadedMessage> (() => lastReadedAlias)
				.Where (() => lastReadedAlias.Chat.Id == chat.Id)
				.Where (() => lastReadedAlias.Employee.Id == employee.Id)
				.SingleOrDefault();
		}

		public static int GetLastReadedMessagesCountForEmployee(IUnitOfWork uow, Chat chat, Employee employee) {
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

