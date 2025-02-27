using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash
{
	public class CashRequestMap : SubclassMap<CashRequest>
	{
		public CashRequestMap()
		{
			DiscriminatorValue(PayoutRequestDocumentType.CashRequest.ToString());
			Map(cr => cr.HaveReceipt).Column("have_receipt");
			HasMany(cr => cr.Sums).Inverse().Cascade.All().LazyLoad().KeyColumn("cash_request_id");
		}
	}
}
