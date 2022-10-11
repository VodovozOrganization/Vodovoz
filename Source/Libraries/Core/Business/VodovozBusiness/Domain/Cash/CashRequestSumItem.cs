using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
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

        private IList<Expense> expenses;
        [Display(Name = "Расходные ордера")]
        public virtual IList<Expense> Expenses {
            get => expenses ?? (expenses = new List<Expense>());
            set => SetField(ref expenses, value);
        }

        private GenericObservableList<Expense> observableExpenses;

        public virtual GenericObservableList<Expense> ObservableExpenses
        {
            get { return observableExpenses ?? (observableExpenses = new GenericObservableList<Expense>(Expenses)); }
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
            Organization organization,
            decimal money)
        {
            Expense newExpense =
            new Expense{
                Casher = casher,
                Date = DateTime.Now,
                Employee = accountableEmployee,
                TypeOperation = ExpenseType.Advance,
                ExpenseCategory = expenseCategory,
                Money = money,
                Description = basis ?? "",
                Organisation = organization,
                RelatedToSubdivision = subdivision,
                CashRequestSumItem = this
            };
            ObservableExpenses.Add(newExpense);
            unitOfWork.Save(newExpense);
            
            var distributor = new ExpenseCashOrganisationDistributor();
            distributor.DistributeCashForExpense(unitOfWork, newExpense, false);
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