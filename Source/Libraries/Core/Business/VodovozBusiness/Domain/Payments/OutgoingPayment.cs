using DateTimeHelpers;
using Gamma.Utilities;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Vodovoz.Core.Domain.Validation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Domain.Payments
{
	/// <summary>
	/// Исходящий платеж
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "исходящий платеж",
		Accusative = "исходящий платеж",
		Genitive = "исходящего платежа",
		Prepositional = "исходящем платеже",
		NominativePlural = "исходящие платежи",
		AccusativePlural = "исходящие платежи",
		GenitivePlural = "исходящих платежей",
		PrepositionalPlural = "исходящих платежах")]
	[HistoryTrace]
	[EntityPermission]
	public class OutgoingPayment : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private int _id;
		private DateTime _createdAt;
		private int _paymentNumber;
		private int? _organizationId;
		private DateTime _paymentDate;
		private string _paymentPurpose;
		private decimal _sum;
		private int? _counterpartyId;
		private int? _financialExpenseCategoryId;
		private int? _cashlessRequestId;
		private string _comment;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Дата создания
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime CreatedAt
		{
			get => _createdAt;
			set => SetField(ref _createdAt, value);
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
		/// Плательщик
		/// </summary>
		[Display(Name = "Плательщик")]
		[HistoryIdentifier(TargetType = typeof(Organization))]
		public virtual int? OrganizationId
		{
			get => _organizationId;
			set => SetField(ref _organizationId, value);
		}

		/// <summary>
		/// Дата оплаты
		/// </summary>
		[Display(Name = "Дата оплаты")]
		public virtual DateTime PaymentDate
		{
			get => _paymentDate;
			set => SetField(ref _paymentDate, value);
		}

		/// <summary>
		/// Основание
		/// Назначение платежа
		/// </summary>
		[Display(Name = "Основание")]
		public virtual string PaymentPurpose
		{
			get => _paymentPurpose;
			set => SetField(ref _paymentPurpose, value);
		}

		/// <summary>
		/// Сумма
		/// </summary>
		[Display(Name = "Сумма")]
		public virtual decimal Sum
		{
			get => _sum;
			set => SetField(ref _sum, value);
		}

		/// <summary>
		/// Поставщик
		/// </summary>
		[Display(Name = "Поставщик")]
		[HistoryIdentifier(TargetType = typeof(Counterparty))]
		public virtual int? CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
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
		/// Идентификатор заявки на выдачу денежных средств по безналичному расчету
		/// </summary>
		[Display(Name = "Заявка на выдачу денежных средств по безналу")]
		[HistoryIdentifier(TargetType = typeof(CashlessRequest))]
		public virtual int? CashlessRequestId
		{
			get => _cashlessRequestId;
			set => SetField(ref _cashlessRequestId, value);
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

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var validationResultFactory = validationContext.GetRequiredService<IValidationResultFactory<OutgoingPayment>>();

			var minimalDate = DateTime.Today.AddDays(-31);
			var maximalDate = DateTime.Today.AddDays(31);

			if(minimalDate > PaymentDate || maximalDate.LatestDayTime() < PaymentDate)
			{
				yield return validationResultFactory.CreateForDateNotInRange(nameof(PaymentDate), minimalDate, maximalDate, PaymentDate);
			}

			if(OrganizationId is null)
			{
				yield return validationResultFactory.CreateForNullProperty(nameof(OrganizationId));
			}

			if(CounterpartyId is null)
			{
				yield return validationResultFactory.CreateForNullProperty(nameof(CounterpartyId));
			}

			if(FinancialExpenseCategoryId is null)
			{
				yield return validationResultFactory.CreateForNullProperty(nameof(FinancialExpenseCategoryId));
			}

			if(PaymentNumber <= 0)
			{
				yield return validationResultFactory.CreateForLeZero(nameof(PaymentNumber));
			}
		}
	}
}
