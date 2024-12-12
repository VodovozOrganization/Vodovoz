using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Data.NHibernate.Mapping.TrueMark.TrueMarkProductCodes
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
