using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Documents
{
    public class WayBillDocumentItem : PropertyChangedBase, IDomainObject
    {
        public int Id { get; }
        
        int sequenceNumber;
        [Display (Name = "Время возвращения в гараж")]
        public virtual int SequenceNumber {
            get => sequenceNumber;
            set => SetField(ref sequenceNumber, value);
        }
        
        string counterpartyName;
        [Display (Name = "Имя контрагента")]
        public virtual string CounterpartyName {
            get => counterpartyName;
            set => SetField (ref counterpartyName, value);
        }
        
        string addressFrom;
        [Display (Name = "Адрес отправки")]
        public virtual string AddressFrom {
            get => addressFrom;
            set => SetField (ref addressFrom, value);
        }

        string addressTo;
        [Display (Name = "Адрес доставки")]
        public virtual string AddressTo {
            get => addressTo;
            set => SetField (ref addressTo, value);
        }
        
        TimeSpan hoursFrom;
        [Display (Name = "Часы отправления из точки")]
        public virtual TimeSpan HoursFrom {
            get => hoursFrom;
            set => SetField (ref hoursFrom, value);
        }
        
        TimeSpan hoursTo;
        [Display (Name = "Часы прибытия в точку")]
        public virtual TimeSpan HoursTo {
            get => hoursTo;
            set => SetField (ref hoursTo, value);
        }

        decimal mileage ;
        [Display (Name = "Пробег")]
        public virtual decimal Mileage  {
            get => mileage ;
            set => SetField(ref mileage , value);
        }
        
        string driverLastName;
        [Display (Name = "Модель автомобиля")]
        public virtual string DriverLastName {
            get => driverLastName;
            set => SetField(ref driverLastName, value);
        }
        
        DocTemplate wayBillTemplate;

        [Display (Name = "Шаблон договора")]
        public virtual DocTemplate DocumentTemplate {
            get => wayBillTemplate;
            protected set => SetField(ref wayBillTemplate, value);
        }

    }
}