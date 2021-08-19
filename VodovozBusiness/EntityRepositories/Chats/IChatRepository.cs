using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Chats;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories.Chats
{
	public interface IChatRepository
	{
		Chat GetChatForDriver(IUnitOfWork uow, Employee driver);
		IList<Chat> GetCurrentUserChats(IUnitOfWork uow);
	}
}