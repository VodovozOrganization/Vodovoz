using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class TrueMarkDocumentMap : ClassMap<TrueMarkDocument>
	{
		public TrueMarkDocumentMap()
		{
			Table("true_mark_api_documents");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.CreationDate).Column("creation_date").ReadOnly();
			Map(x => x.Guid).Column("guid");
			Map(x => x.IsSuccess).Column("is_success");
			Map(x => x.ErrorMessage).Column("error_message");
			Map(x => x.Type).Column("type");

			References(x => x.Order).Column("order_id");
			References(x => x.Organization).Column("organization_id");
		}
	}
}
