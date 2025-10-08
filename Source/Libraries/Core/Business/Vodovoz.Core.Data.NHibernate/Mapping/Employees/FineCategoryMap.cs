using FluentNHibernate.Mapping;
using FluentNHibernate.Utils;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Employees
{
	public class FineCategoryMap : ClassMap<FineCategoryEntity>
	{
		public FineCategoryMap()
		{
			Table("fine_categories");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archieve");
		}
	}
}
