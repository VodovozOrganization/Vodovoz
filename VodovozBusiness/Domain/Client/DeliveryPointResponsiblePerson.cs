using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Client
{
    [Appellative(Gender = GrammaticalGender.Feminine,
        NominativePlural = "ответственные лица",
        Nominative = "ответственное лицо"
    )]
    [HistoryTrace]
    [EntityPermission]
    public class DeliveryPointResponsiblePerson : PropertyChangedBase, IDomainObject, IValidatableObject
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Свойства

        public virtual int Id { get; set; }

        DeliveryPointResponsiblePersonType deliveryPointResponsiblePersonType;

        [Display(Name = "Тип")]
        public virtual DeliveryPointResponsiblePersonType DeliveryPointResponsiblePersonType
        {
            get => deliveryPointResponsiblePersonType;
            set => SetField(ref deliveryPointResponsiblePersonType, value, () => DeliveryPointResponsiblePersonType);
        }

        Employee employee;

        [Display(Name = "Сотрудник")]
        public virtual Employee Employee
        {
            get => employee;
            set => SetField(ref employee, value, () => Employee);
        }

        string phone;

        [Display(Name = "Телефон")]
        public virtual string Phone
        {
            get => phone;
            set => SetField(ref phone, value, () => Phone);
        }

        #endregion

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }
}
