using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class LogisticsAreaMap : ClassMap<LogisticsArea>
	{
		public LogisticsAreaMap ()
		{
			Table("logistics_area");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Name).Column ("name");
		}
	}
}

