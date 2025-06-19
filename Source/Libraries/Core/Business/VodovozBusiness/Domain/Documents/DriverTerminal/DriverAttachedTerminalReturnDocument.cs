using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Utilities.Text;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.DriverTerminal
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "документ возврата терминала водителя",
		NominativePlural = "документы возврата терминалов водителей")]
	[EntityPermission]
	[HistoryTrace]
	public class DriverAttachedTerminalReturnDocument : DriverAttachedTerminalDocumentBase
	{
		public virtual string Title =>
			$"Возрат терминала водителем {PersonHelper.PersonNameWithInitials(Driver.LastName, Driver.Name, Driver.Patronymic)}";

		public override string ToString() =>
			$"Возрат терминала {CreationDate.ToShortDateString()} в {CreationDate.ToShortTimeString()}\r\n" +
			$"на склад {GoodsAccountingOperation.Warehouse.Name}";

		public override void CreateMovementOperations(Warehouse incomeWarehouse, Nomenclature terminal)
		{
			GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation
			{
				Warehouse = incomeWarehouse,
				Amount = 1,
				Nomenclature = terminal,
				OperationTime = CreationDate
			};

			EmployeeNomenclatureMovementOperation = new EmployeeNomenclatureMovementOperation
			{
				Amount = -1,
				Employee = Driver,
				Nomenclature = terminal,
				OperationTime = CreationDate
			};
		}
	}
}
