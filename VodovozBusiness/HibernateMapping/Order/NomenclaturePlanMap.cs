using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Order
{
	public class NomenclaturePlanMap : ClassMap<NomenclaturePlan>
	{
		public NomenclaturePlanMap()
		{
			Table("nomenclature_plan");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.PlanDay).Column("plan_day");
			Map(x => x.PlanMonth).Column("plan_month");

            References(x => x.Nomenclature).Column("nomenclature_id");
        }
	}
}

