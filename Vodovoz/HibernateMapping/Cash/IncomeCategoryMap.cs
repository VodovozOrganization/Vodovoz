using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HMap
{
	public class IncomeCategoryMap : ClassMap<IncomeCategory>
	{
		public IncomeCategoryMap ()
		{
			Table("cash_income_category");
			Not.LazyLoad ();

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Name).Column ("name");
		}
	}
}

