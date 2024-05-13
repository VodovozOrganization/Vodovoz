using FluentNHibernate.Mapping;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Fuel
{
	public class FuelTransactionMap : ClassMap<FuelTransaction>
	{
		public FuelTransactionMap()
		{
			Table("fuel_transactions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.TransactionId).Column("transaction_id");
			Map(x => x.TransactionDate).Column("transaction_date");
			Map(x => x.CardId).Column("card_id");
			Map(x => x.SalePointId).Column("sale_point_id");
			Map(x => x.ProductId).Column("product_id");
			Map(x => x.ProductCategoryId).Column("product_category_id");
			Map(x => x.Quantity).Column("quantity");
			Map(x => x.PricePerItem).Column("price_per_item");
			Map(x => x.TotalSum).Column("total_sum");
			Map(x => x.CardNumber).Column("card_number");
		}
	}
}
