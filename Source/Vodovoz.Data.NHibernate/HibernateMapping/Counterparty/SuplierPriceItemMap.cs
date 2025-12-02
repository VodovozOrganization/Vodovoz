using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class SuplierPriceItemMap : ClassMap<SupplierPriceItem>
	{
		public SuplierPriceItemMap()
		{
			Table("supplier_price_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.PaymentType).Column("payment_type").CustomType<SupplierPaymentTypeStringType>();
			Map(x => x.Price).Column("price");
			Map(x => x.Comment).Column("comment");
			Map(x => x.PaymentCondition).Column("payment_condition").CustomType<PaymentConditionStringType>();
			Map(x => x.DeliveryType).Column("delivery_type").CustomType<DeliveryTypeStringType>();
			Map(x => x.AvailabilityForSale).Column("availability_for_sale").CustomType<AvailabilityForSaleStringType>();
			Map(x => x.ChangingDate).Column("changed_date").ReadOnly();

			References(x => x.NomenclatureToBuy).Column("nomenclature_id");
			References(x => x.Supplier).Column("supplier_id");
			References(x => x.VatRate).Column("vat_rate_id");
		}
	}
}
