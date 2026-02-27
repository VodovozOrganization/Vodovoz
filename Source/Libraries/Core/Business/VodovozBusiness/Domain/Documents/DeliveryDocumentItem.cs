using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Documents
{
    [Appellative (Gender = GrammaticalGender.Feminine,
        NominativePlural = "строки документа доставки",
        Nominative = "строка документа доставки")]
    public class DeliveryDocumentItem : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }
        
        private DeliveryDocument document;
        [Display(Name = "Документ доставки")]
        public virtual DeliveryDocument Document {
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
        
        private DeliveryDirection direction;
        [Display(Name = "Направление доставки")]
        public virtual DeliveryDirection Direction {
            get => direction;
            set => SetField(ref direction, value);
        }
        
        private EmployeeNomenclatureMovementOperation employeeNomenclatureMovementOperation;
        public virtual EmployeeNomenclatureMovementOperation EmployeeNomenclatureMovementOperation { 
            get => employeeNomenclatureMovementOperation;
            set => SetField(ref employeeNomenclatureMovementOperation, value);
        }

        public virtual void CreateOrUpdateOperations()
        {
            var op = employeeNomenclatureMovementOperation ?? new EmployeeNomenclatureMovementOperation();
            op.Amount = Direction == DeliveryDirection.FromClient ? Amount : -Amount;
            op.Nomenclature = Nomenclature;
            op.Employee = Document.RouteListItem.RouteList.Driver;
            op.OperationTime = DateTime.Now;
            EmployeeNomenclatureMovementOperation = op;
        }
    }

    public enum DeliveryDirection
    {
        [Display(Name = "К клиенту")]
        ToClient,
        [Display(Name = "От клиента")]
        FromClient
    }
}
