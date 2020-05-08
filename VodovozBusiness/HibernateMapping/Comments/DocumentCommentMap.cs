using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Comments;

namespace Vodovoz.HibernateMapping.Comments
{
	public class DocumentCommentMap : ClassMap<DocumentComment>
	{
		public DocumentCommentMap()
		{
			Table("document_comments");
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Comment).Column("comment");

			References(x => x.Author).Column("employee_id");

			/*HasManyToMany(x => x.CallTasks).Table("document_comments_to_call_tasks")
								.ParentKeyColumn("comment_id")
								.ChildKeyColumn("call_task_id")
								.LazyLoad();*/
		}
	}
}
