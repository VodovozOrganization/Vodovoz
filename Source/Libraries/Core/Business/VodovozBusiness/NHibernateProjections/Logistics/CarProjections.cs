using NHibernate.Criterion;
using QS.Project.DB;
using Vodovoz.Domain.Employees;
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

		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase названия сущности + Alias<br/>
		/// Используются:
		/// <see cref="Car"/> carAlias<br/>
		/// <see cref="CarModel"/> carModelAlias<br/>
		/// <see cref="CarManufacturer"/> carManufacturerAlias<br/>
		/// <see cref="Employee"/> driverAlias<br/>
		/// </summary>
		/// <returns></returns>
		public static IProjection GetCarTitleProjection()
		{
			Car carAlias = null;
			CarModel carModelAlias = null;
			CarManufacturer carManufacturerAlias = null;
			Employee driverAlias = null;

			return CustomProjections.Concat_WS(
				" ",
				Projections.Property(() => carManufacturerAlias.Name),
				Projections.Property(() => carModelAlias.Name),
				Projections.Property(() => carAlias.RegistrationNumber),
				Projections.Property(() => driverAlias.Name));
		}
		
		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase названия сущности + Alias<br/>
		/// Используются:
		/// <see cref="Car"/> carAlias<br/>
		/// <see cref="CarModel"/> carModelAlias<br/>
		/// </summary>
		/// <returns></returns>
		public static IProjection GetCarModelWithRegistrationNumber()
		{
			Car carAlias = null;
			CarModel carModelAlias = null;

			return CustomProjections.Concat(
				Projections.Property(() => carModelAlias.Name),
				Projections.Constant(" ("),
				Projections.Property(() => carAlias.RegistrationNumber),
				Projections.Constant(")"));
		}
	}
}
