using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Complaints
{
	public class ComplaintFileMap : ClassMap<ComplaintFile>
	{
		public ComplaintFileMap()
		{
			Table("complaint_files");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.FileStorageId).Column("file_storage_id");
			Map(x => x.ByteFile).Column("binary_file").CustomSqlType("BinaryBlob").LazyLoad();
			References(x => x.Complaint).Column("complaint_id");
			References(x => x.ComplaintDiscussionComment).Column("complaint_discussion_comment_id");
		}
	}
}
