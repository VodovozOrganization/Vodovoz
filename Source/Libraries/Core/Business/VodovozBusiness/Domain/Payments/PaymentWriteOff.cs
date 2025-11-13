using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Payments;
using VodovozBusiness.Domain.Operations;

namespace VodovozBusiness.Domain.Payments
{
	/// <summary>
	/// Списание с баланса клиента
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "списания с баланса клиента",
		Nominative = "списание с баланса клиента",
		Genitive = "списания с баланса клиента",
		GenitivePlural = "списаний с баланса клиента",
		Prepositional = "списании с баланса клиента",
		PrepositionalPlural = "списаниях с баланса клиента")]
	[HistoryTrace]
	[EntityPermission]
	public class PaymentWriteOff : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private int _paymentNumber;
		private DateTime _date;
		private string _comment = "";
		private int? _counterpartyId;
		private int? _financialExpenseCategoryId;
		private decimal _sum;
		private string _reason;
		private CashlessMovementOperation _cashlessMovementOperation;
		private int? _organizationId;

		public PaymentWriteOff()
		{
			CashlessMovementOperation = new CashlessMovementOperation();
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Дата списания
		/// </summary>
		[Display(Name = "Дата списания")]
		public virtual DateTime Date
		{
			get => _date;
			set
			{
				if(SetField(ref _date, value))
				{
					CashlessMovementOperation.OperationTime = value;
				}
			}
		}

		/// <summary>
		/// Номер платежа
		/// </summary>
		[Display(Name = "Номер платежа")]
		public virtual int PaymentNumber
		{
			get => _paymentNumber;
			set => SetField(ref _paymentNumber, value);
		}

		/// <summary>
		/// Причина списания
		/// </summary>
		[Display(Name = "Причина списания")]
		public virtual string Reason
		{
			get => _reason;
			set => SetField(ref _reason, value);
		}

		/// <summary>
		/// Идентификатор контрагента
		/// </summary>
		[Display(Name = "Идентификатор контрагента")]
		[HistoryIdentifier(TargetType = typeof(Counterparty))]
		public virtual int? CounterpartyId
		{
			get => _counterpartyId;
			set
			{
				if(SetField(ref _counterpartyId, value))
				{
					if(value.HasValue)
					{
						CashlessMovementOperation.Counterparty = new Counterparty { Id = value.Value };
					}
					else
					{
						CashlessMovementOperation.Counterparty = null;
					}
				}
			}
		}

		/// <summary>
		/// Идентификатор организации
		/// </summary>
		[Display(Name = "Идентификатор организации")]
		[HistoryIdentifier(TargetType = typeof(Organization))]
		public virtual int? OrganizationId
		{
			get => _organizationId;
			set
			{
				if(SetField(ref _organizationId, value))
				{
					if(value.HasValue)
					{
						CashlessMovementOperation.Organization = new Organization { Id = value.Value };
					}
					else
					{
						CashlessMovementOperation.Organization = null;
					}
				}
			}
		}

		/// <summary>
		/// Сумма
		/// </summary>
		[Display(Name = "Сумма")]
		public virtual decimal Sum
		{
			get => _sum;
			set
			{
				if(value < 0)
				{
					OnPropertyChanged(nameof(Sum));
					return;
				}

				if(SetField(ref _sum, value))
				{
					CashlessMovementOperation.Expense = value;
				}
			}
		}

		/// <summary>
		/// Статья расхода
		/// </summary>
		[Display(Name = "Статья расхода")]
		[HistoryIdentifier(TargetType = typeof(FinancialExpenseCategory))]
		public virtual int? FinancialExpenseCategoryId
		{
			get => _financialExpenseCategoryId;
			set => SetField(ref _financialExpenseCategoryId, value);
		}

		/// <summary>
		/// Комментарий
		/// </summary>
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		/// <summary>
		/// Операция передвижения безнала
		/// </summary>
		[Display(Name = "Операция передвижения безнала")]
		public virtual CashlessMovementOperation CashlessMovementOperation
		{
			get => _cashlessMovementOperation;
			set => SetField(ref _cashlessMovementOperation, value);
		}

		/// <summary>
		/// Валидация
		/// </summary>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Date == default)
			{
				yield return new ValidationResult("Необходимо заполнить дату", new[] { nameof(Date) });
			}

			if(PaymentNumber == default)
			{
				yield return new ValidationResult("Необходимо заполнить номер платежа", new[] { nameof(PaymentNumber) });
			}

			if(CounterpartyId is null)
			{
				yield return new ValidationResult("Необходимо заполнить контрагента", new[] { nameof(CounterpartyId) });
			}

			if(OrganizationId is null)
			{
				yield return new ValidationResult("Необходимо заполнить организацию", new[] { nameof(OrganizationId) });
			}

			if(string.IsNullOrWhiteSpace(Reason))
			{
				yield return new ValidationResult("Необходимо заполнить причину", new[] { nameof(Reason) });
			}

			if(Sum <= 0)
			{
				yield return new ValidationResult("Сумма должна быть больше нуля", new[] { nameof(Sum) });
			}

			if(Id == 0 && CounterpartyId != null && OrganizationId != null)
			{
				var paymentsRepository = validationContext.GetRequiredService<IPaymentsRepository>();
				var unitOfWork = validationContext.GetRequiredService<IUnitOfWork>();

				var balance = Math.Max(
					0,
					paymentsRepository.GetCounterpartyLastBalance(unitOfWork, CounterpartyId.Value, OrganizationId.Value));

				if(Sum > balance)
				{
					yield return new ValidationResult($"Для нового списания сумма должна быть меньше текущего баланса контрагента по компании: {balance}", new[] { nameof(Sum) });
				}
			}

			if(FinancialExpenseCategoryId is null)
			{
				yield return new ValidationResult("Необходимо заполнить статью списания", new[] { nameof(FinancialExpenseCategoryId) });
			}
		}
	}
}
