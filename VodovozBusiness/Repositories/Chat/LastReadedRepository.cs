using System;
using QSOrmProject;
using Vodovoz.Domain.Chat;
using Vodovoz.Domain.Employees;
using ChatClass = Vodovoz.Domain.Chat.Chat;

namespace Vodovoz.Repository.Chat
{
	public static class LastReadedRepository
	{
		public static LastReadedMessage GetChatLastReadedMessageForEmloyee(IUnitOfWork uow, ChatClass chat, Employee employee) {
			LastReadedMessage lastReadedAlias = null;

			return uow.Session.QueryOver<LastReadedMessage> (() => lastReadedAlias)
				.Where (() => lastReadedAlias.Chat.Id == chat.Id)
				.Where (() => lastReadedAlias.Employee.Id == employee.Id)
				.SingleOrDefault();
		}
	}
}

