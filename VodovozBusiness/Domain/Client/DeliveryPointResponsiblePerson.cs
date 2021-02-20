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
            set => SetField(ref deliveryPointResponsiblePersonType, value);
        }

        /// <summary>
        /// Точка доступа для работы маппинга Nhibernate
        /// </summary>
        public virtual DeliveryPoint DeliveryPoint { get; set; }

        Employee employee;

        [Display(Name = "Сотрудник")]
        public virtual Employee Employee
        {
            get => employee;
            set => SetField(ref employee, value);
        }

        string phone;

        [Display(Name = "Телефон")]
        public virtual string Phone
        {
            get => phone;
            set => SetField(ref phone, value);
        }

        #endregion

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Employee == null)
                yield return new ValidationResult("Необходимо выбрать сотрудника для ответственного за точку доставки лица",
                    new[] { nameof(Employee) });

            if (string.IsNullOrWhiteSpace(Phone))
                yield return new ValidationResult("Телефон ответственного за точку доставки лица не может быть пустым",
                    new[] { nameof(Phone) });
        }
    }
}
