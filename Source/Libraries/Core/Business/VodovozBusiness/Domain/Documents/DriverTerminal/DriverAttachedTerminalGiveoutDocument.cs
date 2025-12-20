using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Utilities.Text;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.DriverTerminal
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "документ выдачи терминала водителя",
		NominativePlural = "документы выдачи терминалов водителей")]
	[EntityPermission]
	[HistoryTrace]
	public class DriverAttachedTerminalGiveoutDocument : DriverAttachedTerminalDocumentBase
	{
		public virtual string Title =>
			$"Выдача терминала водителю {PersonHelper.PersonNameWithInitials(Driver.LastName, Driver.Name, Driver.Patronymic)}";

		public override string ToString() =>
			$"Выдача терминала {CreationDate.ToShortDateString()} в {CreationDate.ToShortTimeString()}\r\n" +
			$"со склада {GoodsAccountingOperation.Warehouse}";

		public override void CreateMovementOperations(Warehouse writeOffWarehouse, Nomenclature terminal)
		{
			GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation
			{
				Warehouse = writeOffWarehouse,
				Amount = -1,
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
