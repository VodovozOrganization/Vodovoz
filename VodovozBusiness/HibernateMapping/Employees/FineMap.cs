using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.HMap
{
	public class FineMap : ClassMap<Fine>
	{
		public FineMap ()
		{
			Table ("fines");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Date).Column ("date");
			Map (x => x.TotalMoney).Column ("total_money");
			HasMany (x => x.Items).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("fine_id");
		}
	}
}