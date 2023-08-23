using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class AdditionalLoadingDocumentMap : ClassMap<AdditionalLoadingDocument>
	{
		public AdditionalLoadingDocumentMap()
		{
			Table("additional_loading_documents");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreationDate).Column("creation_date").Insert().Not.Update();
			References(x => x.Author).Column("author_id");

			HasMany(x => x.Items).KeyColumn("additional_loading_document_id").Cascade.AllDeleteOrphan().Inverse();
		}
	}
}
