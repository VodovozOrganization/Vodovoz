using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping.Cash.Transfer
{
	public class CashTransferOperationMap : ClassMap<CashTransferOperation>
	{
		public CashTransferOperationMap()
		{
			Table("cash_transfer_operations");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.CashTransferDocument).Column("cash_transfer_document_id").Cascade.All();
			References(x => x.SubdivisionFrom).Column("cash_subdivision_from_id");
			References(x => x.SubdivisionTo).Column("cash_subdivision_to_id");
			Map(x => x.TransferedSum).Column("transfered_sum");
			Map(x => x.SendTime).Column("send_time");
			Map(x => x.ReceiveTime).Column("receive_time");
		}
	}
}
