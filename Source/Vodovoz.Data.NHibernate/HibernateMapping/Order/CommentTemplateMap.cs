using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class CommentTemplateMap : ClassMap<CommentTemplate>
	{
		public CommentTemplateMap()
		{
			Table("comment_templates");
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Comment).Column("comment");
		}
	}
}
