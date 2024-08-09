using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OrderRatingReasonMap : ClassMap<OrderRatingReason>
	{
		public OrderRatingReasonMap()
		{
			Table("order_rating_reasons");
			
			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.IsForOneStarRating).Column("is_for_one_star_rating");
			Map(x => x.IsForTwoStarRating).Column("is_for_two_star_rating");
			Map(x => x.IsForThreeStarRating).Column("is_for_three_star_rating");
			Map(x => x.IsForFourStarRating).Column("is_for_four_star_rating");
			Map(x => x.IsForFiveStarRating).Column("is_for_five_star_rating");
		}
	}
}
