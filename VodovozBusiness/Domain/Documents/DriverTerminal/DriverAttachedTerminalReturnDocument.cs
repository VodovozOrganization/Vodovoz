using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Utilities.Text;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

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
			$"на склад {WarehouseMovementOperation.IncomingWarehouse.Name}";

		public override void CreateMovementOperations(Warehouse incomeWarehouse, Nomenclature terminal)
		{
			WarehouseMovementOperation = new WarehouseMovementOperation
			{
				WriteoffWarehouse = null,
				IncomingWarehouse = incomeWarehouse,
				Amount = 1,
				Equipment = null,
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
