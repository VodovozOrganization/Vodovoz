using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HibernateMapping
{
	public class BaseParameterMap : ClassMap<BaseParameter>
	{
		public BaseParameterMap()
		{
			Table("base_parameters");

			Id(x => x.Name).Column("name").GeneratedBy.Assigned();

			Map(x => x.StrValue).Column("str_value");
		}
	}
}
