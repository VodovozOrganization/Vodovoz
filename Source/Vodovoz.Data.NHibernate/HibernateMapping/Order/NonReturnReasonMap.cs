using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class NonReturnReasonMap : ClassMap<NonReturnReason>
	{
		public NonReturnReasonMap()
		{
			Table("non_return_reasons");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.NeedForfeit).Column("need_forfeit");
		}
	}
}
