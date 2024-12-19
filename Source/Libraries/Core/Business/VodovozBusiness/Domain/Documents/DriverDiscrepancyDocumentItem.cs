using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        Nominative = "строка документа расхождения разгрузки водителя",
        NominativePlural = "строки документа расхождения разгрузки водителя")]
    public class DriverDiscrepancyDocumentItem : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }
        
        private DriverDiscrepancyDocument document;
        [Display(Name = "Документ расхождения разгрузки водителя")]
        public virtual DriverDiscrepancyDocument Document {
            get => document;
            set => SetField(ref document, value);
        }

        private decimal amount;
        [Display(Name = "Количество")]
        public virtual decimal Amount {
            get => amount;
            set => SetField(ref amount, value);
        }

        private Nomenclature nomenclature;
        [Display(Name = "Номенклатура")]
        public virtual Nomenclature Nomenclature {
            get => nomenclature;
            set => SetField(ref nomenclature, value);
        }
        
        private DiscrepancyReason discrepancyReason;
        [Display(Name = "Причина расхождений")]
        public virtual DiscrepancyReason DiscrepancyReason {
            get => discrepancyReason;
            set => SetField(ref discrepancyReason, value);
        }
        
        private EmployeeNomenclatureMovementOperation employeeNomenclatureMovementOperation;
        public virtual EmployeeNomenclatureMovementOperation EmployeeNomenclatureMovementOperation { 
            get => employeeNomenclatureMovementOperation;
            set => SetField(ref employeeNomenclatureMovementOperation, value);
        }

        public virtual void CreateOrUpdateOperations()
        {
            var op = employeeNomenclatureMovementOperation ?? new EmployeeNomenclatureMovementOperation();
            op.Amount = DiscrepancyReason == DiscrepancyReason.UnloadedExcessively ? Amount : -Amount;
            op.Nomenclature = Nomenclature;
            op.Employee = Document.RouteList.Driver;
            op.OperationTime = DateTime.Now;
            EmployeeNomenclatureMovementOperation = op;
        }
    }
    
    public enum DiscrepancyReason
    {
        [Display(Name = "Сдал больше, чем требовалось")]
        UnloadedExcessively,
        [Display(Name = "Сдал меньше, чем требовалось")]
        UnloadedDeficiently
    }
}
