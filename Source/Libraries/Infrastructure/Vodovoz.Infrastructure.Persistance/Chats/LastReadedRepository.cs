using QS.DomainModel.UoW;
using Vodovoz.Domain.Chats;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Chats;

namespace Vodovoz.Infrastructure.Persistance.Chats
{
	internal sealed class LastReadedRepository : ILastReadedRepository
	{
		public LastReadedMessage GetLastReadedMessageForEmployee(IUnitOfWork uow, Chat chat, Employee employee)
		{
			LastReadedMessage lastReadedAlias = null;

			return uow.Session.QueryOver(() => lastReadedAlias)
				.Where(() => lastReadedAlias.Chat.Id == chat.Id)
				.Where(() => lastReadedAlias.Employee.Id == employee.Id)
				.SingleOrDefault();
		}

		public int GetLastReadedMessagesCountForEmployee(IUnitOfWork uow, Chat chat, Employee employee)
		{
			ChatMessage chatMessageAlias = null;
			var lastMessage = GetLastReadedMessageForEmployee(uow, chat, employee);

			var query = uow.Session.QueryOver(() => chatMessageAlias)
				.Where(() => chatMessageAlias.Chat.Id == chat.Id);

			if(lastMessage == null)
			{
				return query.RowCount();
			}

			return query.Where(() => chatMessageAlias.DateTime > lastMessage.LastDateTime).RowCount();
		}
	}
}

