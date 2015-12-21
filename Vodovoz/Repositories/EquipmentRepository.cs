using NHibernate.Criterion;
using Vodovoz.Domain;
using System.Collections.Generic;
using QSOrmProject;
using System.Linq;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Repository
{
	public static class EquipmentRepository
	{
		public static QueryOver<Equipment> GetEquipmentWithTypesQuery (List<EquipmentType> types)
		{
			Nomenclature nomenclatureAlias = null;
			var Query = QueryOver.Of<Equipment> ()
				.JoinAlias (e => e.Nomenclature, () => nomenclatureAlias)
				.Where (() => nomenclatureAlias.Type.IsIn (types));
			return Query;
		}

		public static Equipment GetEquipmentForSaleByNomenclature (IUnitOfWork uow, Nomenclature nomenclature)
		{
			Equipment equipmentAlias = null;
			WarehouseMovementOperation operationAddAlias = null;

			var subqueryEquipmentAvailable = QueryOver.Of<WarehouseMovementOperation> (() => operationAddAlias)
				.OrderBy (() => operationAddAlias.OperationTime).Desc
				.Where (() => equipmentAlias.Id == operationAddAlias.Equipment.Id)
				.Select (op=>op.IncomingWarehouse)
				.Take (1);

			return uow.Session.QueryOver<Equipment> (() => equipmentAlias)				
				.Where (() => equipmentAlias.Nomenclature.Id == nomenclature.Id)
				.Where (() => !equipmentAlias.OnDuty)
				.Where (Subqueries.IsNotNull (subqueryEquipmentAvailable.DetachedCriteria))
				.Take (1)
				.List ().First ();
		}

		public static QueryOver<Equipment> GetEquipmentByNomenclature (Nomenclature nomenclature)
		{
			Nomenclature nomenclatureAlias = null;

			return QueryOver.Of<Equipment> ()
				.JoinAlias (e => e.Nomenclature, () => nomenclatureAlias)
				.Where (() => nomenclatureAlias.Id == nomenclature.Id);
		}
	}
}

