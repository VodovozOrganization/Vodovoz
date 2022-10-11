using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;
using Vodovoz.Domain;

namespace Vodovoz.HibernateMapping
{
    public class FinancialDistrictMap : ClassMap<FinancialDistrict>
    {
        public FinancialDistrictMap()
        {
            Table("financial_districts");

            Id(x => x.Id).Column("id").GeneratedBy.Native();

            Map(x => x.Name).Column("name");
            Map(x => x.Border).Column("border").CustomType<MySQL57GeometryType>();
            References(x => x.FinancialDistrictsSet).Column("financial_districts_set_id");

            References(x => x.Organization).Column("organization_id");
            References(x => x.CopyOf).Column("copy_of_financial_district_id");
        }
    }
}