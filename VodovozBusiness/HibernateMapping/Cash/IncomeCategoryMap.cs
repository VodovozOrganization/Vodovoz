using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HibernateMapping
{
	public class IncomeCategoryMap : ClassMap<IncomeCategory>
	{
		public IncomeCategoryMap ()
		{
			Table("cash_income_category");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Name).Column ("name");
			Map(x => x.IncomeDocumentType).Column("type_document").CustomType<IncomeInvoiceDocumentTypeStringType>();
		}
	}
}

