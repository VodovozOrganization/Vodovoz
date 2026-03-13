using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Documents
{
    [Appellative (Gender = GrammaticalGender.Feminine,
        Nominative = "перенос номенклатур между водителями",
        NominativePlural = "переносы номенклатур между водителями")]
    public class DriverNomenclatureTransferItem : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }
        
        private AddressTransferDocumentItem documentItem;
        [Display(Name = "Строка документа переноса адресов")]
        public virtual AddressTransferDocumentItem DocumentItem {
            get => documentItem;
            set => SetField(ref documentItem, value);
        }

        private Employee driverFrom;
        [Display(Name = "От водителя")]
        public virtual Employee DriverFrom {
            get => driverFrom;
            set => SetField(ref driverFrom, value);
        }

        private Employee driverTo;
        [Display(Name = "К водителю")]
        public virtual Employee DriverTo {
            get => driverTo;
            set => SetField(ref driverTo, value);
        }

        private Nomenclature nomenclature;
        [Display(Name = "Номенклатура")]
        public virtual Nomenclature Nomenclature {
            get => nomenclature;
            set => SetField(ref nomenclature, value);
        }
        
        private decimal amount;
        [Display(Name = "Количество")]
        public virtual decimal Amount {
            get => amount;
            set => SetField(ref amount, value);
        }
        
        private EmployeeNomenclatureMovementOperation employeeNomenclatureMovementOperationFrom;
        [Display(Name = "Операция списания номенклатуры с баланса сотрудника")]
        public virtual EmployeeNomenclatureMovementOperation EmployeeNomenclatureMovementOperationFrom { 
            get => employeeNomenclatureMovementOperationFrom;
            set => SetField(ref employeeNomenclatureMovementOperationFrom, value);
        }
        
        private EmployeeNomenclatureMovementOperation employeeNomenclatureMovementOperationTo;
        [Display(Name = "Операция зачисления номенклатуры на баланс сотрудника")]
        public virtual EmployeeNomenclatureMovementOperation EmployeeNomenclatureMovementOperationTo { 
            get => employeeNomenclatureMovementOperationTo;
            set => SetField(ref employeeNomenclatureMovementOperationTo, value);
        }

        public virtual void CreateOrUpdateOperations()
        {
            var operationFrom = EmployeeNomenclatureMovementOperationFrom ?? new EmployeeNomenclatureMovementOperation();
            operationFrom.Employee = DriverFrom;
            operationFrom.Amount = -Amount;
            operationFrom.Nomenclature = Nomenclature;
            operationFrom.OperationTime = DateTime.Now;
            EmployeeNomenclatureMovementOperationFrom = operationFrom;
            
            var operationTo = EmployeeNomenclatureMovementOperationTo ?? new EmployeeNomenclatureMovementOperation();
            operationTo.Employee = DriverTo;
            operationTo.Amount = Amount;
            operationTo.Nomenclature = Nomenclature;
            operationTo.OperationTime = DateTime.Now;
            EmployeeNomenclatureMovementOperationTo = operationTo;
        }
    }
}