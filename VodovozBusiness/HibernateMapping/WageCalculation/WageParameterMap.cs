using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.HibernateMapping.WageCalculation
{
	public class WageParameterMap : ClassMap<WageParameter>
	{
		public WageParameterMap()
		{
			Table("wage_parameters");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Comment).Column("comment");
			Map(x => x.WageCalcType).Column("wage_calc_type").CustomType<WageCalculationTypeStringType>();
			Map(x => x.WageCalcRate).Column("wage_calc_rate");
			Map(x => x.QuantityOfFullBottlesToSell).Column("full_quantity");
			Map(x => x.QuantityOfEmptyBottlesToTake).Column("empty_quantity");
			Map(x => x.IsArchive).Column("is_archive");
		}
	}
}