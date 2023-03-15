using NHibernate.Criterion;
using QS.Project.DB;
using Vodovoz.Domain.Contacts;
using IProjection = NHibernate.Criterion.IProjection;

namespace Vodovoz.NHibernateProjections.Contacts
{
	public static class PhoneProjections
	{
		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase названия сущности + Alias<br/>
		/// Используется <see cref="Phone"/> phoneAlias
		/// </summary>
		/// <returns></returns>
		public static IProjection GetDigitNumberLeadsWith8()
		{
			Phone phoneAlias = null;

			return CustomProjections.Concat_WS(
				"",
				Projections.Constant("8"),
				Projections.Property(() => phoneAlias.DigitsNumber));
		}

		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase названия сущности + Alias<br/>
		/// Используется <see cref="Phone"/> orderContactPhoneAlias
		/// </summary>
		/// <returns></returns>
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
