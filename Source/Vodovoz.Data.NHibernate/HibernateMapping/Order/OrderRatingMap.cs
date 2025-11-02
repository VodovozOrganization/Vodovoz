using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OrderRatingMap : ClassMap<OrderRating>
	{
		public OrderRatingMap()
		{
			Table("orders_ratings");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.Source).Column("source");
			Map(x => x.OrderRatingStatus).Column("order_rating_status");
			Map(x => x.Created).Column("created");
			Map(x => x.Rating).Column("rating")
				.CustomType<int>(); // чтобы хибернэйт не преобразовывал к булевым значениям (0, 1) tinyint(1)
			Map(x => x.Comment).Column("comment");

			References(x => x.OnlineOrder).Column("online_order_id");
			References(x => x.Order).Column("order_id");
			References(x => x.ProcessedByEmployee).Column("processed_by_employee_id");

			HasManyToMany(x => x.OrderRatingReasons)
				.Table("orders_ratings_to_order_rating_reasons")
				.ParentKeyColumn("order_rating_id")
				.ChildKeyColumn("order_rating_reason_id");
		}
	}
}
