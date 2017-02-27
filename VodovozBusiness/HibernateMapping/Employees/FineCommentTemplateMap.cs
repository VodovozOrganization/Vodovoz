using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz
{
	public class FineCommentTemplateMap : ClassMap<FineCommentTemplate>
	{
		public FineCommentTemplateMap ()
		{
			Table ("fine_comment_templates");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.Comment).Column ("comment");
		}
	}
}

