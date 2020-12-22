using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Payments
{
    public class PaymentFromYookassa : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }
        
        PaymentStatus paymentStatus;
        [Display(Name = "Статус оплаты")]
        public virtual PaymentStatus PaymentStatus {
        	get => paymentStatus;
        	set => SetField(ref paymentStatus, value);
        }

        DateTime paymentTime;
        [Display(Name = "Дата и время платежа")]
        public virtual DateTime PaymentTime {
        	get => paymentTime;
        	set => SetField(ref paymentTime, value);
        }

        int paymentNr;
        [Display(Name = "Номер операции")]
        public virtual int PaymentNr {
        	get => paymentNr;
        	set => SetField(ref paymentNr, value);
        }

        decimal amountLessCommission;
        [Display(Name = "Сумма за вычетом комиссии")]
        public virtual decimal AmountLessCommission {
        	get => amountLessCommission;
        	set => SetField(ref amountLessCommission, value);
        }
        
        decimal amount;
        [Display(Name = "Сумма платежа")]
        public virtual decimal Amount {
	        get => amount;
	        set => SetField(ref amount, value);
        }

        string description = string.Empty;
        [Display(Name = "Описание")]
        public virtual string Description {
        	get => description;
        	set => SetField(ref description, value);
        }

        string paymentType = string.Empty;
        [Display(Name = "Тип платежа")]
        public virtual string PaymentType {
        	get => paymentType;
        	set => SetField(ref paymentType, value);
        }

        string payerName = string.Empty;
        [Display(Name = "Имя плательщика")]
        public virtual string PayerName {
        	get => payerName;
        	set => SetField(ref payerName, value);
        }
        
        string payerAddress = string.Empty;
        [Display(Name = "Адрес плательщика")]
        public virtual string PayerAddress {
	        get => payerAddress;
	        set => SetField(ref payerAddress, value);
        }
        
        string iNN = string.Empty;
        [Display(Name = "ИНН")]
        public virtual string INN {
	        get => iNN;
	        set => SetField(ref iNN, value);
        }

        bool selected;
        public virtual bool Selected {
        	get => selected;
        	set => SetField(ref selected, value);
        }

        public virtual bool Selectable { get; set; }
        public virtual bool IsDuplicate { get; set; }
        public virtual string Color { get; set; }
    }
}