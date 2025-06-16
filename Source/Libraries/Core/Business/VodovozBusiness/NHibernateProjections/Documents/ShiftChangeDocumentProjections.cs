using NHibernate.Criterion;
using QS.Project.DB;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.NHibernateProjections.Documents
{
	public class ShiftChangeDocumentProjections
	{
		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase названия сущности + Alias<br/>
		/// </summary>
		/// <returns></returns>
		public static IProjection GetStorageProjection()
		{
			Warehouse warehouseAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			
			return Projections.Conditional(
				Restrictions.Where(() => warehouseAlias.Name != null),
				Projections.Property(() => warehouseAlias.Name),
				Projections.Conditional(
						Restrictions.Where(() => carAlias.Id != null),
						CustomProjections.Concat(
							Projections.Property(() => carModelAlias.Name),
							Projections.Constant(" "),
							Projections.Property(() => carAlias.RegistrationNumber)),
						Projections.Constant(string.Empty)));
		}

		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase названия сущности + Alias<br/>
		/// </summary>
		/// <returns></returns>
		public static IProjection GetStorageIdProjection()
		{
			Warehouse warehouseAlias = null;
			Car carAlias = null;
			
			return Projections.Conditional(
				Restrictions.Where(() => warehouseAlias.Name != null),
				Projections.Property(() => warehouseAlias.Id),
				Projections.Conditional(
					Restrictions.Where(() => carAlias.Id != null),
						Projections.Property(() => carAlias.Id),
						Projections.Constant(0)));
		}
	}
}
