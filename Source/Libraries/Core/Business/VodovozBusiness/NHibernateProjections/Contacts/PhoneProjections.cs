using NHibernate.Criterion;
using QS.Project.DB;
using Vodovoz.Domain.Contacts;
using IProjection = NHibernate.Criterion.IProjection;

namespace Vodovoz.NHibernateProjections.Contacts
{
	public static class PhoneProjections
	{
		public static IProjection GetDigitNumberLeadsWith8()
		{
			Phone phoneAlias = null;

			return CustomProjections.Concat_WS(
				"",
				Projections.Constant("8"),
				Projections.Property(() => phoneAlias.DigitsNumber));
		}

		public static IProjection GetOrderContactDigitNumberLeadsWith8()
		{
			Phone orderContactPhoneAlias = null;

			return CustomProjections.Concat_WS(
				"",
				Projections.Constant("8"),
				Projections.Property(() => orderContactPhoneAlias.DigitsNumber));
		}
	}
}
