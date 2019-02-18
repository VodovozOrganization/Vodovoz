using System;
using System.Collections.Generic;
using System.Reflection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Project.Domain;
using Vodovoz.Domain.Orders;
using QSBanks;

namespace Vodovoz.Domain.Permissions
{
	public class EntitiesWithPermissionFinder : IEntitiesWithPermissionFinder
	{
		public IEnumerable<Type> FindTypes()
		{
			var qsBanksAssembly = Assembly.GetAssembly(typeof(Bank));
			var qsDomainAssembly = Assembly.GetAssembly(typeof(EntityUserPermission));
			var vodovozDomainAssembly = Assembly.GetAssembly(typeof(Order));
			return DomainHelper.GetHavingAttributeEntityTypes<EntityPermissionAttribute>(x => x.IsClass && !x.IsAbstract, qsDomainAssembly, vodovozDomainAssembly, qsBanksAssembly);
		}
	}
}
