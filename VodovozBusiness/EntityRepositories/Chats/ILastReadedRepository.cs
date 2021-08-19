using QS.DomainModel.UoW;
using Vodovoz.Domain.Chats;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories.Chats
{
	public interface ILastReadedRepository
	{
		LastReadedMessage GetLastReadedMessageForEmployee(IUnitOfWork uow, Chat chat, Employee employee);
		int GetLastReadedMessagesCountForEmployee(IUnitOfWork uow, Chat chat, Employee employee);
	}
}