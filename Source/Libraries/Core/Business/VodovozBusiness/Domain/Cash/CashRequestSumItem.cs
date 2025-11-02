using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Cash
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "суммы",
		Nominative = "сумма")]
	public class CashRequestSumItem : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private const int _maxCommentLength = 500;
		private CashRequest _cashRequest;
		private string _comment;
		private decimal _sum;
		private DateTime _date;
		private IList<Expense> _expenses;
		private GenericObservableList<Expense> _observableExpenses;
		private Employee _accountableEmployee;

		public virtual int Id { get; }

		[Display(Name = "Запрос")]
		public virtual CashRequest CashRequest
		{
			get => _cashRequest;
			set => SetField(ref _cashRequest, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display(Name = "Сумма")]
		public virtual decimal Sum
		{
			get => _sum;
			set => SetField(ref _sum, value);
		}

		[Display(Name = "Дата")]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		[Display(Name = "Расходные ордера")]
		public virtual IList<Expense> Expenses
		{
			get => _expenses ?? (_expenses = new List<Expense>());
			set => SetField(ref _expenses, value);
		}

		public virtual GenericObservableList<Expense> ObservableExpenses =>
			_observableExpenses ?? (_observableExpenses = new GenericObservableList<Expense>(Expenses));

		[Display(Name = "Подотчетное лицо")]
		public virtual Employee AccountableEmployee
		{
			get => _accountableEmployee;
			set => SetField(ref _accountableEmployee, value);
		}

		public virtual void CreateNewExpense(
			IUnitOfWork unitOfWork,
			Employee casher,
			Subdivision subdivision,
			int? expenseCategoryId,
			string basis,
			Organization organization,
			decimal money)
		{
			var newExpense = new Expense
			{
				Casher = casher,
				Date = DateTime.Now,
				Employee = _accountableEmployee,
				TypeOperation = ExpenseType.Advance,
				ExpenseCategoryId = expenseCategoryId,
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
			if(AccountableEmployee == null)
			{
				yield return new ValidationResult(
					"Необходимо выбрать подотчетное лицо",
					new[] { nameof(AccountableEmployee) });
			}

			if(Sum == 0m)
			{
				yield return new ValidationResult(
					"Сумма должна быть > 0",
					new[] { nameof(Sum) });
			}

			if(Date == default(DateTime))
			{
				yield return new ValidationResult(
					"Необходимо выбрать дату",
					new[] { nameof(Date) });
			}

			if(!string.IsNullOrWhiteSpace(Comment))
			{
				if(Comment.Length > _maxCommentLength)
				{
					yield return new ValidationResult(
						$"Длина комментария превышена на {Comment.Length - _maxCommentLength}",
						new[] { nameof(Comment) });
				}
			}
		}

		#endregion
	}
}
