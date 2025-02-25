using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class FiscalMoneyPositionMap : ClassMap<FiscalMoneyPosition>
	{
		public FiscalMoneyPositionMap()
		{
			Table("edo_fiscal_money_positions");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.PaymentType)
				.Column("payment_type");

			Map(x => x.Sum)
				.Column("sum");
		}
	}
}
