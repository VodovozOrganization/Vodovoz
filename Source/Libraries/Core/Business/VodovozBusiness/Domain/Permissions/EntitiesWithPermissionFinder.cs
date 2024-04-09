using System;
using System.Collections.Generic;
using System.Reflection;
using QS.Banks.Domain;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Project.Domain;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Permissions
{
	public class EntitiesWithPermissionFinder : IEntitiesWithPermissionFinder
	{
		public IEnumerable<Type> FindTypes()
		{
			var qsCommonAssembly = Assembly.GetAssembly(typeof(MeasurementUnits));
			var qsBanksAssembly = Assembly.GetAssembly(typeof(Bank));
			var qsDomainAssembly = Assembly.GetAssembly(typeof(EntityUserPermission));
			var vodovozDomainAssembly = Assembly.GetAssembly(typeof(Order));
			var coreData = Assembly.GetAssembly(typeof(Vodovoz.Core.Domain.AssemblyFinder));
			return DomainHelper.GetHavingAttributeEntityTypes<EntityPermissionAttribute>(x => x.IsClass && !x.IsAbstract, qsDomainAssembly, vodovozDomainAssembly, qsBanksAssembly, qsCommonAssembly, coreData);
		}
	}
}
