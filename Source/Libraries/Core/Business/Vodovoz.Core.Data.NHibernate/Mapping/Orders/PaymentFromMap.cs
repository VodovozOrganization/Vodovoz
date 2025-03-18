using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Orders
{
	public class PaymentFromMap : ClassMap<PaymentFromEntity>
	{
		public PaymentFromMap()
		{
			Table("payments_from");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Name)
				.Column("name");

			Map(x => x.IsArchive)
				.Column("is_archive");

			Map(x => x.ReceiptRequired)
				.Column("receipt_required");
			
			/*Map(x => x.OrganizationCriterion)
				.Column("organization_criterion");*/
		}
	}
}
