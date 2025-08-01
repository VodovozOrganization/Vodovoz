using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.DriverTerminal
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "документ терминала водителя",
		NominativePlural = "документы терминалов водителей")]
	public class DriverAttachedTerminalDocumentBase : PropertyChangedBase, IDomainObject
	{
		private Employee _author;
		private Employee _driver;
		private DateTime _creationDate;
		private WarehouseBulkGoodsAccountingOperation _goodsAccountingOperation;
		private EmployeeNomenclatureMovementOperation _employeeNomenclatureMovementOperation;

		#region Свойства
		public virtual int Id { get; }

		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}
		
		[Display(Name = "Водитель")]
		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		[Display(Name = "Операция передвижения товаров по складу")]
		public virtual WarehouseBulkGoodsAccountingOperation GoodsAccountingOperation
		{
			get => _goodsAccountingOperation;
			set => SetField(ref _goodsAccountingOperation, value);
		}
		
		[Display(Name = "Операция передвижения товаров сотрудника")]
		public virtual EmployeeNomenclatureMovementOperation EmployeeNomenclatureMovementOperation
		{
			get => _employeeNomenclatureMovementOperation;
			set => SetField(ref _employeeNomenclatureMovementOperation, value);
		}
		#endregion

		public virtual void CreateMovementOperations(Warehouse warehouse, Nomenclature terminal)
		{
			throw new NotImplementedException();
		}
	}

	public enum AttachedTerminalDocumentType
	{
		[Display(Name = "Выдача")]
		Giveout,
		[Display(Name = "Возврат")]
		Return
	}
}
