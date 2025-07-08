using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.EntityRepositories.Settings
{
	public interface IOrderOrganizationSettingsRepository
	{
		IEnumerable<INamedDomainObject> GetSameNomenclaturesInOrganizationBasedOrderContentSettings(
			IUnitOfWork uow, IEnumerable<int> nomenclatureIds, int? settingsId = null);
		IEnumerable<INamedDomainObject> GetSameProductGroupsInOrganizationBasedOrderContentSettings(
			IUnitOfWork uow, IEnumerable<ProductGroup> productGroups, int? settingsId = null);
	}
}
