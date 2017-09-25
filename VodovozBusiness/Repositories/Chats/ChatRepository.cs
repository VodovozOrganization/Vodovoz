using QSOrmProject;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Chats;
<<<<<<< Updated upstream:VodovozBusiness/Repositories/Chats/ChatRepository.cs
=======
using ChatClass = Vodovoz.Domain.Chats.Chat;
>>>>>>> Stashed changes:VodovozBusiness/Repositories/Chat/ChatRepository.cs
using System.Collections.Generic;
using QSProjectsLib;

namespace Vodovoz.Repository.Chats
{
	public static class ChatRepository
	{
		public static Chat GetChatForDriver(IUnitOfWork uow, Employee driver) {
			Chat chatAlias = null;

			return uow.Session.QueryOver<Chat> (() => chatAlias)
				.Where (() => chatAlias.ChatType == ChatType.DriverAndLogists)
				.Where (() => chatAlias.Driver.Id == driver.Id)
				.SingleOrDefault();
		}

		public static IList<Chat> GetCurrentUserChats(IUnitOfWork uow, Employee employee) {
			//employee пригодится в дальнейшем
			Chat chatAlias = null;

			if (QSMain.User.Permissions ["logistican"]) {
				return uow.Session.QueryOver<Chat> (() => chatAlias)
				.Where (() => chatAlias.ChatType == ChatType.DriverAndLogists)
				.List ();
			} else
				return null;
		}
	}
}

