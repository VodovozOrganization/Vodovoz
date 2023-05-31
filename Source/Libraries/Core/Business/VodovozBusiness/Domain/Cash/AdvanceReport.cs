using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Domain.Cash
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "авансовые отчеты",
		Nominative = "авансовый отчет")]
	[EntityPermission]
	[HistoryTrace]
	public class AdvanceReport : PropertyChangedBase, IDomainObject, IValidatableObject, ISubdivisionEntity
	{
		private DateTime _date;
		private Subdivision _relatedToSubdivision;
		private Employee _casher;
		private Employee _accountable;
		private ExpenseCategory _expenseCategory;
		private Income _changeReturn;
		private string _description;
		private decimal _money;
		private Organization _organisation;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Дата")]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		[Display(Name = "Относится к подразделению")]
		public virtual Subdivision RelatedToSubdivision
		{
			get => _relatedToSubdivision;
			set => SetField(ref _relatedToSubdivision, value);
		}

		[Display(Name = "Кассир")]
		public virtual Employee Casher
		{
			get => _casher;
			set => SetField(ref _casher, value);
		}

		[Display(Name = "Подотчетное лицо")]
		public virtual Employee Accountable
		{
			get => _accountable;
			set => SetField(ref _accountable, value);
		}

		[Display(Name = "Статья расхода")]
		public virtual ExpenseCategory ExpenseCategory
		{
			get => _expenseCategory;
			set => SetField(ref _expenseCategory, value);
		}

		[Display(Name = "Возврат сдачи")]
		public virtual Income ChangeReturn
		{
			get => _changeReturn;
			set => SetField(ref _changeReturn, value);
		}


		[Display(Name = "Основание")]
		public virtual string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}

		[Display(Name = "Сумма")]
		public virtual decimal Money
		{
			get => _money;
			set => SetField(ref _money, value);
		}

		[Display(Name = "Организация")]
		public virtual Organization Organisation
		{
			get => _organisation;
			set => SetField(ref _organisation, value);
		}

		public virtual string Title => $"Авансовый отчет №{Id} от {Date:d}";

		public virtual bool NeedValidateOrganisation { get; set; }

		#endregion

		public AdvanceReport() { }

		public virtual List<AdvanceClosing> CloseAdvances(
			out Expense surcharge,
			out Income returnChange,
			List<Expense> advances)
		{
			surcharge = null;
			returnChange = null;

			if(advances.Any(a => a.ExpenseCategory != ExpenseCategory))
			{
				throw new InvalidOperationException("Нельзя что бы авансовый отчет, закрывал авансы выданные по другим статьям.");
			}

			decimal totalExpense = advances.Sum(a => a.UnclosedMoney);
			decimal balance = totalExpense - Money;

			var resultClosing = new List<AdvanceClosing>();

			if(balance < 0)
			{
				surcharge = new Expense
				{
					Casher = Casher,
					Date = Date,
					Employee = Accountable,
					TypeOperation = ExpenseType.Advance,
					Organisation = Organisation,
					ExpenseCategory = ExpenseCategory,
					Money = Math.Abs(balance),
					Description = $"Доплата денежных средств сотруднику по авансовому отчету №{Id}",
					AdvanceClosed = true,
					RelatedToSubdivision = RelatedToSubdivision
				};
				resultClosing.Add(surcharge.AddAdvanceCloseItem(this, surcharge.Money));
			}
			else if(balance > 0)
			{
				returnChange = new Income
				{
					Casher = Casher,
					Date = Date,
					Employee = Accountable,
					ExpenseCategory = ExpenseCategory,
					TypeOperation = IncomeType.Return,
					Organisation = Organisation,
					Money = Math.Abs(balance),
					Description = $"Возврат в кассу денежных средств по авансовому отчету №{Id}",
					RelatedToSubdivision = RelatedToSubdivision
				};
				ChangeReturn = returnChange;
			}

			foreach(var adv in advances)
			{
				resultClosing.Add(adv.AddAdvanceCloseItem(this, adv.UnclosedMoney));
			}

			return resultClosing;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Accountable == null)
			{
				yield return new ValidationResult("Подотчетное лицо должно быть указано.",
					new[] { this.GetPropertyName(o => o.Accountable) });
			}

			if(ExpenseCategory == null)
			{
				yield return new ValidationResult("Статья расхода должна быть указана.",
					new[] { this.GetPropertyName(o => o.ExpenseCategory) });
			}

			if(Money <= 0)
			{
				yield return new ValidationResult("Сумма должна иметь значение отличное от 0.",
					new[] { this.GetPropertyName(o => o.Money) });
			}

			if(string.IsNullOrWhiteSpace(Description))
			{
				yield return new ValidationResult("Основание должно быть заполнено.",
					new[] { this.GetPropertyName(o => o.Description) });
			}

			if(RelatedToSubdivision == null)
			{
				yield return new ValidationResult("Должно быть выбрано подразделение",
					new[] { this.GetPropertyName(o => o.RelatedToSubdivision) });
			}

			if(Id == 0 && NeedValidateOrganisation && Organisation == null)
			{
				yield return new ValidationResult("Организация должна быть заполнена",
					new[] { nameof(Organisation) });
			}
		}

		#endregion
	}
}
