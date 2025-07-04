using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Settings;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Infrastructure.Persistance.Settings
{
	public class OrderOrganizationSettingsRepository : IOrderOrganizationSettingsRepository
	{
		public IEnumerable<INamedDomainObject> GetSameNomenclaturesInOrganizationBasedOrderContentSettings(
			IUnitOfWork uow, IEnumerable<int> nomenclatureIds, int? settingsId = null)
		{
			Nomenclature nomenclatureAlias = null;
			NamedDomainObjectNode resultAlias = null;

			var query = uow.Session.QueryOver<OrganizationBasedOrderContentSettings>()
				.JoinAlias(settings => settings.Nomenclatures, () => nomenclatureAlias)
				.WhereRestrictionOn(() => nomenclatureAlias.Id).IsInG(nomenclatureIds)
				.And(s => s.Id != settingsId)
				.SelectList(list => list
					.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
				)
				.TransformUsing(Transformers.AliasToBean<NamedDomainObjectNode>());
			
			return query.List<NamedDomainObjectNode>();
		}
		
		public IEnumerable<INamedDomainObject> GetSameProductGroupsInOrganizationBasedOrderContentSettings(
			IUnitOfWork uow, IEnumerable<ProductGroup> settingProductGroups, int? settingsId = null)
		{
			ProductGroup productGroupAlias = null;
			NamedDomainObjectNode resultAlias = null;
			
			var productGroups = uow.Session.QueryOver<OrganizationBasedOrderContentSettings>()
				.JoinAlias(s => s.ProductGroups, () => productGroupAlias)
				.Where(s => s.Id != settingsId)
				.Select(Projections.Entity(() => productGroupAlias))
				.List<ProductGroup>();

			return (
				from productGroup in productGroups
				from settingProductGroup in settingProductGroups
				where settingProductGroup.IsChildOf(productGroup)
				select new NamedDomainObjectNode
				{
					Name = settingProductGroup.Name,
					Id = settingProductGroup.Id
				})
				.Cast<INamedDomainObject>()
				.ToList();
		}
	}
}
