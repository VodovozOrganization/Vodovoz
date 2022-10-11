using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Chats;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories.Chats
{
	public interface IChatMessageRepository
	{
		IList<ChatMessage> GetChatMessagesForPeriod(IUnitOfWork uow, Chat chat, int days);
		IList<UnreadedChatDTO> GetUnreadedChatMessages(IUnitOfWork uow, Employee forEmployee, bool accessLogisticChat);
		Dictionary<int, int> GetLastChatMessages(IUnitOfWork uow);
	}
}