using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Data.NHibernate.Mapping.TrueMark.TrueMarkProductCodes
{
	/// <summary>
	/// Маппинг переотправленного кода ЧЗ товара.
	/// </summary>
	public class ResentTrueMarkProductCodeMap : SubclassMap<ResentTrueMarkProductCode>
	{
		/// <summary>
		/// Настраивает хранение переотправленного кода.
		/// </summary>
		public ResentTrueMarkProductCodeMap()
		{
			DiscriminatorValue("Resent");
		}
	}
}
