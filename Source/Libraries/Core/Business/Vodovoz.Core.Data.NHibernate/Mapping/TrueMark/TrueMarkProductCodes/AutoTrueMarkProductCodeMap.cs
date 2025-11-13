using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Data.NHibernate.Mapping.TrueMark.TrueMarkProductCodes
{
	public class AutoTrueMarkProductCodeMap : SubclassMap<AutoTrueMarkProductCode>
	{
		public AutoTrueMarkProductCodeMap()
		{
			DiscriminatorValue("Auto");
		}
	}
}
