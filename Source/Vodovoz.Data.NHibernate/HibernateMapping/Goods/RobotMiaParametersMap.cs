using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class RobotMiaParametersMap : ClassMap<RobotMiaParameters>
	{
		public RobotMiaParametersMap()
		{
			Table("robot_mia_parameters");
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.NomenclatureId).Column("nomenclature_id");
			Map(x => x.GoodsOnlineAvailability).Column("goods_online_availability");

			HasMany(x => x.SlangWords)
				.Not.LazyLoad()
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.KeyColumn("robot_mia_parameters_id");
		}
	}
}
