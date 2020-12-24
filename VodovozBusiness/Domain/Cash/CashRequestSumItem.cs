using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Cash
{
    [Appellative (Gender = GrammaticalGender.Feminine,
        NominativePlural = "суммы",
        Nominative = "сумма")] 
    public class CashRequestSumItem: PropertyChangedBase, IDomainObject, IValidatableObject
    {
        public virtual int Id { get; }
        
        private CashRequest cashRequest;
        [Display(Name = "Запрос")]
        public virtual CashRequest CashRequest {
            get { return cashRequest; }
            set { SetField(ref cashRequest, value); }
        }

        private string comment;
        [Display(Name = "Комментарий")]
        public virtual string Comment {
            get => comment;
            set => SetField(ref comment, value);
        }
        
        private decimal sum;
        [Display(Name = "Сумма")]
        public virtual decimal Sum {
            get => sum;
            set => SetField(ref sum, value);
        }
        
        private DateTime date;
        [Display(Name = "Дата")]
        public virtual DateTime Date {
            get => date;
            set => SetField(ref date, value);
        }

        private Expense expense;
        [Display(Name = "Расход")]
        public virtual Expense Expense {
            get => expense;
            set => SetField(ref expense, value);
        }

        private Employee accountableEmployee;
        [Display(Name = "Подотчетное лицо")]
        public virtual Employee AccountableEmployee {
            get => accountableEmployee;
            set => SetField(ref accountableEmployee, value);
        }

        public virtual void CreateNewExpense(
            IUnitOfWork unitOfWork,
            Employee casher,
            Subdivision subdivision,
            ExpenseCategory expenseCategory,
            string basis,
            Organization organization)
        {
            Expense newExpense =
            new Expense{
                Casher = casher,
                Date = Date,
                Employee = accountableEmployee,
                TypeOperation = ExpenseType.Advance,
                ExpenseCategory = expenseCategory,
                Money = Sum,
                Description = basis ?? "",
                Organisation = organization,
                RelatedToSubdivision = subdivision
            };
            this.Expense = newExpense;
            unitOfWork.Save(newExpense);
        }

        #region Validation

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AccountableEmployee == null)
            {
                yield return new ValidationResult(
                    "Необходимо выбрать подотчетное лицо",
                    new[] { nameof(AccountableEmployee) });
            }
            if (Sum == 0m)
            {
                yield return new ValidationResult(
                    "Сумма должна быть > 0",
                    new[] { nameof(Sum) });
            }
        }

        #endregion
     
    }
}