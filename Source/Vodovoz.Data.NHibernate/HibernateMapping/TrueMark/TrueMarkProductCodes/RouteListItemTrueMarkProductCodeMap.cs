using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Data.NHibernate.HibernateMapping.TrueMark.TrueMarkProductCodes
{
	public class RouteListItemTrueMarkProductCodeMap : SubclassMap<RouteListItemTrueMarkProductCode>
	{
		public RouteListItemTrueMarkProductCodeMap()
		{
			DiscriminatorValue("RouteListItem");

			References(x => x.RouteListItem).Column("route_list_item_id");
		}
	}
}
