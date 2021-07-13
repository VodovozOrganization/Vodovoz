using System;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents.DriverTerminal
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "документ выдачи терминала водителя",
		NominativePlural = "документы выдачи терминалов водителей")]
	[EntityPermission]
	public class DriverAttachedTerminalGiveoutDocument : DriverAttachedTerminalDocumentBase
	{
		public virtual string Title =>
			$"Выдан терминал {CreationDate.ToShortDateString()} в {CreationDate.ToShortTimeString()}\r\n " +
			$"со склада {WarehouseMovementOperation?.WriteoffWarehouse?.Name}";

		public override void CreateMovementOperations(Warehouse writeoffWarehouse, Nomenclature terminal)
		{
			WarehouseMovementOperation = new WarehouseMovementOperation
			{
				WriteoffWarehouse = writeoffWarehouse,
				IncomingWarehouse = null,
				Amount = 1,
				Equipment = null,
				Nomenclature = terminal,
				OperationTime = CreationDate
			};

			EmployeeNomenclatureMovementOperation = new EmployeeNomenclatureMovementOperation
			{
				Amount = 1,
				Employee = Driver,
				Nomenclature = terminal,
				OperationTime = CreationDate
			};
		}
	}
}
