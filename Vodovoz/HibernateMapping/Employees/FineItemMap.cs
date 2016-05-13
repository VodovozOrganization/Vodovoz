using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.HMap
{
	public class FineItemMap : ClassMap<FineItem>
	{
		public FineItemMap ()
		{
			Table ("fines_items");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Money).Column ("money");
			References(x => x.Fine).Column("fine_id");
			References(x => x.Employee).Column("employee_id");
		}
	}
}