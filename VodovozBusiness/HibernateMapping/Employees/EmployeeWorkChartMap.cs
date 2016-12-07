using System;
using FluentNHibernate.Mapping;

namespace Vodovoz
{
	public class EmployeeWorkChartMap : ClassMap<EmployeeWorkChart>
	{
		public EmployeeWorkChartMap()
		{
			Table("employee_work_charts");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Date)	 .Column("date");
			Map(x => x.StartTime).Column("start_time");
			Map(x => x.EndTime)	 .Column("end_time");

			References(x => x.Employee).Columns("employee_id");
		}
	}
}

