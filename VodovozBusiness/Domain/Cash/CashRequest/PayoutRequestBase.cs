using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NHibernate.Type;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Cash
{
    [Appellative (Gender = GrammaticalGender.Feminine,
        NominativePlural = "заявки на выдачу средств",
        Nominative = "заявка на выдачу средств")]
    [EntityPermission]
    public abstract class PayoutRequestBase : PropertyChangedBase, IDomainObject, IValidatableObject
    {
	    #region Свойства
	    public virtual int Id { get; }

	    public abstract string Title { get; }

	    [Display(Name = "Тип документа")]
	    public virtual PayoutRequestDocumentType PayoutRequestDocumentType { get; set; }


        private PayoutRequestState _payoutRequestState;
        [Display(Name = "Состояние")]
        public virtual PayoutRequestState PayoutRequestState {
            get => _payoutRequestState;
            set => SetField(ref _payoutRequestState, value);
        }

        private bool possibilityNotToReconcilePayments;
        [Display(Name = "Возможность не пересогласовывать выплаты")]
        public virtual bool PossibilityNotToReconcilePayments
        {
            get => possibilityNotToReconcilePayments;
            set => SetField(ref possibilityNotToReconcilePayments, value);
        }

        private DateTime date = DateTime.Now;
        [Display(Name = "Дата")]
        public virtual DateTime Date {
            get => date;
            set => SetField(ref date, value);
        }

        private Employee author;
        [Display(Name = "Автор")]
        public virtual Employee Author {
            get => author;
            set => SetField(ref author, value);
        }

        /// <summary>
        /// Не понятное поле, не было в ТЗ, не использовать в логике до задачи по изменению
        /// </summary>
        private Subdivision subdivision;
        [Display(Name = "Подразделение")]
        public virtual Subdivision Subdivision {
            get => subdivision;
            set => SetField(ref subdivision, value);
        }

        private ExpenseCategory expenseCategory;
        [Display(Name = "Статья расхода")]
        public virtual ExpenseCategory ExpenseCategory {
            get => expenseCategory;
            set => SetField(ref expenseCategory, value);
        }

        private string basis;
        [Display(Name = "Основание")]
        public virtual string Basis {
            get => basis;
            set => SetField(ref basis, value);
        }

        private string explanation;
        [Display(Name = "Пояснение")]
        public virtual string Explanation {
            get => explanation;
            set => SetField(ref explanation, value);
        }

        private string reasonForSendToReappropriate;
        [Display(Name = "Причина отправки на согласование")]
        public virtual string ReasonForSendToReappropriate {
            get => reasonForSendToReappropriate;
            set => SetField(ref reasonForSendToReappropriate, value);
        }

        private string cancelReason;
        [Display(Name = "Причина отмены")]
        public virtual string CancelReason {
            get => cancelReason;
            set => SetField(ref cancelReason, value);
        }

        private Organization organization;
        [Display(Name = "Организация")]
        public virtual Organization Organization  {
            get => organization;
            set => SetField(ref organization, value);
        }

        #endregion

        public abstract IEnumerable<ValidationResult> Validate(ValidationContext validationContext);
        public abstract void ChangeState(PayoutRequestState newState);

        #region SumItems


        #endregion

        public class CashRequestStateStringType : EnumStringType
        {
	        public CashRequestStateStringType() : base(typeof(PayoutRequestState)) { }
        }

        public class CashRequestDocTypeStringType : EnumStringType
        {
	        public CashRequestDocTypeStringType() : base(typeof(PayoutRequestDocumentType)) { }
        }
    }

    public enum PayoutRequestDocumentType
    {
	    [Display(Name = "Заявка на выдачу наличных ДС")]
	    CashRequest,
	    [Display(Name = "Заявка на оплату по Б/Н")]
	    CashlessRequest
    }

    public enum PayoutRequestState
    {
	    [Display(Name = "Новая")]
	    New,
	    [Display(Name = "На уточнении")]
	    OnClarification, // после отправки на пересогласование
	    [Display(Name = "Подана")]
	    Submitted, // после подтверждения
	    [Display(Name = "Согласована")]
	    Agreed, // после согласования
	    [Display(Name = "Передана на выдачу")]
	    GivenForTake,
	    [Display(Name = "Частично закрыта")]
	    PartiallyClosed, // содержит не выданные суммы
	    [Display(Name = "Отменена")]
	    Canceled,
	    [Display(Name = "Закрыта")]
	    Closed // все суммы выданы
    }

    public enum PayoutRequestUserRole
    {
	    [Display(Name = "Заявитель")]
	    RequestCreator,
	    [Display(Name = "Согласователь")]
	    Coordinator,
	    [Display(Name = "Финансист")]
	    Financier,
	    [Display(Name = "Кассир")]
	    Cashier,
	    [Display(Name = "Другие")]
	    Other,
	    [Display(Name = "Бухгалтер")]
	    Accountant
    }
}
