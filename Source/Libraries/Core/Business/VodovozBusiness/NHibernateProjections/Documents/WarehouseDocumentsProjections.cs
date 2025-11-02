using NHibernate;
using NHibernate.Criterion;
using QS.Project.DB;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.NHibernateProjections.Documents
{
	public static class WarehouseDocumentsProjections
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
			Employee employeeAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			
			return Projections.Conditional(
				Restrictions.Where(() => warehouseAlias.Name != null),
				Projections.Property(() => warehouseAlias.Name),
				Projections.Conditional(
					Restrictions.Where(() => employeeAlias.Id != null),
					CustomProjections.Concat(
						Projections.Property(() => employeeAlias.LastName),
						Projections.Constant(" "),
						Projections.Property(() => employeeAlias.Name),
						Projections.Constant(" "),
						Projections.Property(() => employeeAlias.Patronymic)),
					Projections.Conditional(
						Restrictions.Where(() => carAlias.Id != null),
						CustomProjections.Concat(
							Projections.Property(() => carModelAlias.Name),
							Projections.Constant(" "),
							Projections.Property(() => carAlias.RegistrationNumber)),
						Projections.Constant(string.Empty))));
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
			Employee employeeAlias = null;
			Car carAlias = null;
			
			return Projections.Conditional(
				Restrictions.Where(() => warehouseAlias.Name != null),
				Projections.Property(() => warehouseAlias.Id),
				Projections.Conditional(
					Restrictions.Where(() => employeeAlias.Id != null),
					Projections.Property(() => employeeAlias.Id),
					Projections.Conditional(
						Restrictions.Where(() => carAlias.Id != null),
						Projections.Property(() => carAlias.Id),
						Projections.Constant(0))));
		}
		
		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase from + названия сущности + Alias<br/>
		/// </summary>
		/// <returns></returns>
		public static IProjection GetFromStorageProjection(string defaultName)
		{
			Warehouse fromWarehouseAlias = null;
			Employee fromEmployeeAlias = null;
			Car fromCarAlias = null;
			CarModel fromCarModelAlias = null;
			
			return Projections.Conditional(
				Restrictions.Where(() => fromWarehouseAlias.Name != null),
				Projections.Property(() => fromWarehouseAlias.Name),
				Projections.Conditional(
					Restrictions.Where(() => fromEmployeeAlias.Id != null),
					CustomProjections.Concat(
						Projections.Property(() => fromEmployeeAlias.LastName),
						Projections.Constant(" "),
						Projections.Property(() => fromEmployeeAlias.Name),
						Projections.Constant(" "),
						Projections.Property(() => fromEmployeeAlias.Patronymic)),
					Projections.Conditional(
						Restrictions.Where(() => fromCarAlias.Id != null),
						CustomProjections.Concat(
							Projections.Property(() => fromCarModelAlias.Name),
							Projections.Constant(" "),
							Projections.Property(() => fromCarAlias.RegistrationNumber)),
						Projections.Constant(defaultName, NHibernateUtil.String))));
		}

		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase to + названия сущности + Alias<br/>
		/// </summary>
		/// <returns></returns>
		public static IProjection GetToStorageProjection(string defaultName)
		{
			Warehouse toWarehouseAlias = null;
			Employee toEmployeeAlias = null;
			Car toCarAlias = null;
			CarModel toCarModelAlias = null;

			return Projections.Conditional(
				Restrictions.Where(() => toWarehouseAlias.Name != null),
				Projections.Property(() => toWarehouseAlias.Name),
				Projections.Conditional(
					Restrictions.Where(() => toEmployeeAlias.Id != null),
					CustomProjections.Concat(
						Projections.Property(() => toEmployeeAlias.LastName),
						Projections.Constant(" "),
						Projections.Property(() => toEmployeeAlias.Name),
						Projections.Constant(" "),
						Projections.Property(() => toEmployeeAlias.Patronymic)),
					Projections.Conditional(
						Restrictions.Where(() => toCarAlias.Id != null),
						CustomProjections.Concat(
							Projections.Property(() => toCarModelAlias.Name),
							Projections.Constant(" "),
							Projections.Property(() => toCarAlias.RegistrationNumber)),
						Projections.Constant(defaultName, NHibernateUtil.String))));
			
		}

		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase from + названия сущности + Alias<br/>
		/// </summary>
		/// <returns></returns>
		public static IProjection GetFromStorageIdProjection()
		{
			Warehouse fromWarehouseAlias = null;
			Employee fromEmployeeAlias = null;
			Car fromCarAlias = null;
			
			return Projections.Conditional(
				Restrictions.Where(() => fromWarehouseAlias.Name != null),
				Projections.Property(() => fromWarehouseAlias.Id),
				Projections.Conditional(
					Restrictions.Where(() => fromEmployeeAlias.Id != null),
					Projections.Property(() => fromEmployeeAlias.Id),
					Projections.Conditional(
						Restrictions.Where(() => fromCarAlias.Id != null),
						Projections.Property(() => fromCarAlias.Id),
						Projections.Property(() => fromCarAlias.Id))));
		}

		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase to + названия сущности + Alias<br/>
		/// </summary>
		/// <returns></returns>
		public static IProjection GetToStorageIdProjection()
		{
			Warehouse toWarehouseAlias = null;
			Employee toEmployeeAlias = null;
			Car toCarAlias = null;

			return Projections.Conditional(
				Restrictions.Where(() => toWarehouseAlias.Name != null),
				Projections.Property(() => toWarehouseAlias.Id),
				Projections.Conditional(
					Restrictions.Where(() => toEmployeeAlias.Id != null),
					Projections.Property(() => toEmployeeAlias.Id),
					Projections.Conditional(
						Restrictions.Where(() => toCarAlias.Id != null),
						Projections.Property(() => toCarAlias.Id),
						Projections.Property(() => toCarAlias.Id))));
		}
	}
}
