using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class FiscalIndustryRequisiteRegulatoryDocumentMap : ClassMap<FiscalIndustryRequisiteRegulatoryDocument>
	{
		public FiscalIndustryRequisiteRegulatoryDocumentMap()
		{
			Table("edo_fiscal_industry_requisite_regulatory_documents");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.FoivId)
				.Column("foiv_id");

			Map(x => x.DocDateTime)
				.Column("doc_date_time");

			Map(x => x.DocNumber)
				.Column("doc_number");
		}
	}
}
