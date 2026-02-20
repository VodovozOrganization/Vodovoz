using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class WithdrawalEdoRequestMap : SubclassMap<WithdrawalEdoRequest>
	{
		public WithdrawalEdoRequestMap()
		{
			DiscriminatorValue(nameof(EdoRequestType.Withdrawal));
		}
	}
}
