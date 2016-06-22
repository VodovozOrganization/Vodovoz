using System;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Chat;
using ChatClass = Vodovoz.Domain.Chat.Chat;

namespace Vodovoz.Repository.Chat
{
	public static class ChatRepository
	{
		public static ChatClass GetChatForDriver(IUnitOfWork uow, Employee driver) {
			ChatClass chatAlias = null;

			return uow.Session.QueryOver<ChatClass> (() => chatAlias)
				.Where (() => chatAlias.ChatType == ChatType.DriverAndLogists)
				.Where (() => chatAlias.Driver.Id == driver.Id)
				.SingleOrDefault();
		}
	}
}

