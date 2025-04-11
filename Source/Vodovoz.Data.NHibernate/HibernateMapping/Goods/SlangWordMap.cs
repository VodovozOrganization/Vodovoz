using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class SlangWordMap : ClassMap<SlangWord>
	{
		public SlangWordMap()
		{
			Table("robot_mia_slang_words");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Word).Column("word");
			Map(x => x.RobotMiaParametersId).Column("robot_mia_parameters_id");
		}
	}
}
