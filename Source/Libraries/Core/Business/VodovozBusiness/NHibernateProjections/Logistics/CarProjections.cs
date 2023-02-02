using NHibernate.Criterion;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.NHibernateProjections.Logistics
{
	public static class CarProjections
	{
		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase названия сущности + Alias<br/>
		/// Используется <see cref="CarVersion"/> carVersionAlias
		/// </summary>
		/// <returns></returns>
		public static IProjection GetIsCompanyCarProjection()
		{
			CarVersion carVersionAlias = null;

			return Projections.Conditional(
				Restrictions.Eq(Projections.Property(() => carVersionAlias.CarOwnType), CarOwnType.Company),
				Projections.Constant(true),
				Projections.Constant(false));
		}
	}
}
