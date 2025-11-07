using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.Domain.Chats;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Chats;

namespace Vodovoz.Infrastructure.Persistance.Chats
{
	internal sealed class ChatRepository : IChatRepository
	{
		public Chat GetChatForDriver(IUnitOfWork uow, Employee driver)
		{
			Chat chatAlias = null;

			return uow.Session.QueryOver(() => chatAlias)
				.Where(() => chatAlias.ChatType == ChatType.DriverAndLogists)
				.Where(() => chatAlias.Driver.Id == driver.Id)
				.SingleOrDefault();
		}

		public IList<Chat> GetCurrentUserChats(IUnitOfWork uow)
		{
			Chat chatAlias = null;

			if(ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.IsLogistician))
			{
				return uow.Session.QueryOver(() => chatAlias)
				.Where(() => chatAlias.ChatType == ChatType.DriverAndLogists)
				.List();
			}

			return null;
		}
	}
}
