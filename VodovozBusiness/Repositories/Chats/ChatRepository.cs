using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Chats;
using System.Collections.Generic;
using QS.Project.Repositories;

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

			if (UserPermissionRepository.CurrentUserPresetPermissions ["logistican"]) {
				return uow.Session.QueryOver<Chat> (() => chatAlias)
				.Where (() => chatAlias.ChatType == ChatType.DriverAndLogists)
				.List ();
			} else
				return null;
		}
	}
}