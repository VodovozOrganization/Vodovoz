using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.HibernateMapping.Employees
{
	public class CitizenshipMap : ClassMap<Citizenship>
	{
		public CitizenshipMap()
		{
			Table("citizenship");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}
}
