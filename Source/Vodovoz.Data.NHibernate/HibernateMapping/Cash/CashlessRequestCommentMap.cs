using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Cash.CashRequest;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash
{
	internal class CashlessRequestCommentMap : ClassMap<CashlessRequestComment>
	{
		public CashlessRequestCommentMap()
		{
			Table("cashless_request_comments");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.CashlessRequestId).Column("cashless_request_id");
			Map(x => x.Text).Column("text");
			Map(x => x.CreatedAt).Column("created_at");
			Map(x => x.AuthorId).Column("author_id");
			HasMany(x => x.AttachedFileInformations)
				.KeyColumn("cashless_request_comment_id")
				.Cascade.AllDeleteOrphan()
				.Inverse();
		}
	}
}
