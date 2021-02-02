using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using NHibernate.Type;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Cash
{
    
    [Appellative (Gender = GrammaticalGender.Feminine,
        NominativePlural = "заявки на выдачу средств",
        Nominative = "заявка на выдачу средств")]
    [EntityPermission]
    [HistoryTrace]
    public class CashRequest : PropertyChangedBase, IDomainObject, IValidatableObject
    {
        public virtual void ChangeState(States newState)
        {
            if(newState == State)
                return;

            string exceptionMessage = $"Некорректная операция. Не предусмотрена смена статуса с {State} на {newState}";

            switch (newState)
            {
                //Подана
                case States.Submited:
                    if (State == States.New || State == States.OnClarification )
                    {
                        State = newState;
                    } else {
                        throw new InvalidOperationException(exceptionMessage);
                    }
                    break;
                //На уточнении
                case States.OnClarification:
                    if (State == States.Agreed || 
                        State == States.GivenForTake || 
                        State == States.Canceled || 
                        State == States.PartiallyClosed) 
                    {
                        State = newState;
                    } else {
                        throw new InvalidOperationException(exceptionMessage);
                    }
                    break;
                //Согласована
                case States.Agreed:
                    if (State == States.Submited)
                    {
                        State = newState;
                    } else {
                        throw new InvalidOperationException(exceptionMessage);
                    }
                    break;
                //Передана на выдачу
                case States.GivenForTake:
                    if (State == States.Agreed)
                    {
                        State = newState;
                    } else {
                        throw new InvalidOperationException(exceptionMessage);
                    }
                    break;
                case States.Canceled:
                    if (State == States.Submited || 
                        State == States.New || 
                        State == States.OnClarification || 
                        State == States.GivenForTake || 
                        State == States.Agreed)
                    {
                        State = newState;
                    } else {
                        throw new InvalidOperationException(exceptionMessage);
                    }
                    break;
                case States.Closed:
                    bool allSumsWasGiven = CheckIsAllSumsClosed();
                    //Если к нам пришло Close значит хотя бы одну закрыли, поэтому не нужно проверять что незакрытых нет совсем
                    State = CheckIsAllSumsClosed() ? newState : States.PartiallyClosed;
                    break;
                
                case States.PartiallyClosed:
                    break;
                
                default:
                    throw new NotImplementedException($"Не реализовано изменение статуса для {newState}");
            }
        }
        
        public virtual bool CheckIsAllSumsClosed()
        {
            if (Sums.Count == 1 && Sums.First() != null)
            {
                return true;
            }

            bool allSumsWasGiven = true;
            foreach (var sum in Sums)
            {
                if (sum.Expense == null)
                {
                    allSumsWasGiven = false;
                    break;
                }
            }

            return allSumsWasGiven;
        }


        #region Свойства

        public virtual string Title => $"Заявка на выдачу ДС №{Id} от {Date:d}";
        
        private States state;
        [Display(Name = "Состояние")]
        public virtual States State {
            get => state;
            set => SetField(ref state, value);
        }
        
        private DocumentTypes documentType;
        [Display(Name = "Тип документа")]
        public virtual DocumentTypes DocumentType {
            get => documentType;
            set => SetField(ref documentType, value);
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
        
        private bool haveReceipt;
        [Display(Name = "Наличие чека")]
        public virtual bool HaveReceipt {
            get => haveReceipt;
            set => SetField(ref haveReceipt, value);
        }
        
        private Organization organization;
        [Display(Name = "Организация")]
        public virtual Organization Organization  {
            get => organization;
            set => SetField(ref organization, value);
        }

        private IList<CashRequestSumItem> sums = new List<CashRequestSumItem>();
        [Display(Name = "Суммы")]
        public virtual IList<CashRequestSumItem> Sums  {
            get => sums;
            set => SetField(ref sums, value);
        }
        
        private GenericObservableList<CashRequestSumItem> observableSums;
        
        public virtual GenericObservableList<CashRequestSumItem> ObservableSums {
            get { return observableSums ?? (observableSums = new GenericObservableList<CashRequestSumItem>(Sums)); }
        }

        #endregion

        public enum States
        {
            [Display(Name = "Новая")]
            New,
            [Display(Name = "На уточнении")]
            OnClarification, // после отправки на пересогласование
            [Display(Name = "Подана")]
            Submited, // после подтверждения
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
        
        public enum DocumentTypes
        {
            [Display(Name = "Заявка на выдачу")]
            CashRequest
        }

        public class CashRequestStateStringType : EnumStringType
        { public CashRequestStateStringType() : base(typeof(States)) { } }
        
        public class CashRequestDocTypeStringType : EnumStringType
        { public CashRequestDocTypeStringType() : base(typeof(DocumentTypes)) { } }
        
        public virtual int Id { get; }
        
        #region IValidatableObject implementation
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Author == null)
            {
                yield return new ValidationResult(
                    "Необходимо выбрать автора",
                    new[] { this.GetPropertyName(o => o.Author)});
            }
            
            if (Sums.Count < 1)
            {
                yield return new ValidationResult(
                    "Необходимо создать хотя бы одну сумму",
                    new[] { this.GetPropertyName(o => o.Sums)});
            }
            
            foreach (var cashRequestSumItem in Sums)
            {
                if (cashRequestSumItem.AccountableEmployee == null)
                {
                    yield return new ValidationResult(
                        string.Format($"У суммы {cashRequestSumItem.Sum} должно быть заполнено подотчетное лицо"),
                        new[] { nameof(Sums)});
                }
            }
            
            if (string.IsNullOrWhiteSpace(Basis))
            {
                yield return new ValidationResult(
                    "Необходимо заполнить основание",
                    new[] { nameof(Basis)});
            }
            
            if (State == States.Agreed && Organization == null)
            {
                yield return new ValidationResult(
                    "Необходимо заполнить организацию",
                    new[] { nameof(Organization)});
            }

            if (!string.IsNullOrWhiteSpace(Basis) && Basis.Length > 1000)
            {
                yield return new ValidationResult(
                    "Длина основания не должна превышать 1000 символов",
                    new[] { nameof(Basis)});
            }
            
            if (!string.IsNullOrWhiteSpace(CancelReason) && CancelReason.Length > 1000)
            {
                yield return new ValidationResult(
                    "Длина причины отмены не должна превышать 1000 символов",
                    new[] { nameof(CancelReason)});
            }
            
            if (!string.IsNullOrWhiteSpace(Explanation) && Explanation.Length > 1000)
            {
                yield return new ValidationResult(
                    "Длина пояснения не должна превышать 1000 символов",
                    new[] { nameof(Explanation)});
            }
            
        }
        #endregion IValidatableObject implementation


        #region SumItems

        public virtual void AddItem(CashRequestSumItem sumItem)
        {
            observableSums.Add(sumItem);
        }

        public virtual void DeleteItem(CashRequestSumItem sumItem)
        {
            if(sumItem == null || !observableSums.Contains(sumItem)) {
                return;
            }
            ObservableSums.Remove(sumItem);
        }

        #endregion

    }
}
