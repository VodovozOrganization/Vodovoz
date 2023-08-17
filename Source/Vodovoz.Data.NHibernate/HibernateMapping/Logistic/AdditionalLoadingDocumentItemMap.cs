using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class AdditionalLoadingDocumentItemMap : ClassMap<AdditionalLoadingDocumentItem>
	{
		public AdditionalLoadingDocumentItemMap()
		{
			Table("additional_loading_document_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Amount).Column("amount");
			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.AdditionalLoadingDocument).Column("additional_loading_document_id");
		}
	}
}
