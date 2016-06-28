using System;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Chat;
using ChatClass = Vodovoz.Domain.Chat.Chat;
using System.Collections.Generic;
using QSProjectsLib;

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

		public static IList<ChatClass> GetCurrentUserChats(IUnitOfWork uow, Employee employee) {
			//employee пригодится в дальнейшем
			ChatClass chatAlias = null;

			if (QSMain.User.Permissions ["logistican"]) {
				return uow.Session.QueryOver<ChatClass> (() => chatAlias)
				.Where (() => chatAlias.ChatType == ChatType.DriverAndLogists)
				.List ();
			} else
				return null;
		}
	}
}

