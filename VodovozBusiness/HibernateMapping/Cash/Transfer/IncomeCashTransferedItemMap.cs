using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash.CashTransfer;

namespace Vodovoz.HibernateMapping.Cash.Transfer
{
	public class IncomeCashTransferedItemMap : ClassMap<IncomeCashTransferedItem>
	{
		public IncomeCashTransferedItemMap()
		{
			Table("cash_income_transfered_items");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.Document).Column("cash_transfered_document_id");
			References(x => x.Income).Column("cash_income_id");
			Map(x => x.Comment).Column("comment");
		}
	}
}
