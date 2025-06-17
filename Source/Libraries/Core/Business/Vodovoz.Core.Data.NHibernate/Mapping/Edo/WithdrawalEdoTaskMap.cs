using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class WithdrawalEdoTaskMap : SubclassMap<WithdrawalEdoTask>
	{
		public WithdrawalEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.Withdrawal));

			Extends(typeof(OrderEdoTask));
		}
	}
}
