using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Employees
{
	public class FineCategoryMap : ClassMap<FineCategoryEntity>
	{
		public FineCategoryMap()
		{
			Table("fine_categories");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
		}
	}
}
