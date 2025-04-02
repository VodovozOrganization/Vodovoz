using NHibernate.Type;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Cash
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "заявки на выдачу средств",
		Nominative = "заявка на выдачу средств")]
	public abstract class PayoutRequestBase : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private PayoutRequestState _payoutRequestState;
		private bool _possibilityNotToReconcilePayments;
		private DateTime _date = DateTime.Now;
		private Employee _author;
		private Subdivision _subdivision;
		private int? _expenseCategoryId;
		private string _basis;
		private string _explanation;
		private string _reasonForSendToReappropriate;
		private string _cancelReason;
		private Organization _organization;
		private int _id;

		#region Свойства

		public virtual int Id
		{
			get => _id;
			protected set
			{
				if(_id != value)
				{
					_id = value;

					UpdateFileInformations();
					UpdateComments();
				}
			}
		}

		public abstract string Title { get; }

		[Display(Name = "Тип документа")] public virtual PayoutRequestDocumentType PayoutRequestDocumentType { get; set; }

		[Display(Name = "Состояние")]
		public virtual PayoutRequestState PayoutRequestState
		{
			get => _payoutRequestState;
			protected set => SetField(ref _payoutRequestState, value);
		}

		[Display(Name = "Возможность не пересогласовывать выплаты")]
		public virtual bool PossibilityNotToReconcilePayments
		{
			get => _possibilityNotToReconcilePayments;
			set => SetField(ref _possibilityNotToReconcilePayments, value);
		}

		[Display(Name = "Дата")]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision
		{
			get => _subdivision;
			set => SetField(ref _subdivision, value);
		}

		[Display(Name = "Статья расхода")]
		[HistoryIdentifier(TargetType = typeof(FinancialExpenseCategory))]
		public virtual int? ExpenseCategoryId
		{
			get => _expenseCategoryId;
			set => SetField(ref _expenseCategoryId, value);
		}

		[Display(Name = "Основание")]
		public virtual string Basis
		{
			get => _basis;
			set => SetField(ref _basis, value);
		}

		[Display(Name = "Пояснение")]
		public virtual string Explanation
		{
			get => _explanation;
			set => SetField(ref _explanation, value);
		}

		[Display(Name = "Причина отправки на согласование")]
		public virtual string ReasonForSendToReappropriate
		{
			get => _reasonForSendToReappropriate;
			set => SetField(ref _reasonForSendToReappropriate, value);
		}

		[Display(Name = "Причина отмены")]
		public virtual string CancelReason
		{
			get => _cancelReason;
			set => SetField(ref _cancelReason, value);
		}

		[Display(Name = "Организация")]
		public virtual Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		#endregion

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Author == null)
			{
				yield return new ValidationResult(
					"Автор не указан. Ваш пользователь не привязан к сотруднику, которого можно указать в качестве автора",
					new[] { nameof(Author) });
			}

			if(Subdivision == null)
			{
				yield return new ValidationResult("Не указано подразделение автора", new[] { nameof(Subdivision) });
			}

			if(string.IsNullOrWhiteSpace(Basis))
			{
				yield return new ValidationResult("Необходимо заполнить основание", new[] { nameof(Basis) });
			}

			if(!string.IsNullOrWhiteSpace(Basis)
			&& Basis.Length > 1000)
			{
				yield return new ValidationResult("Длина основания не должна превышать 1000 символов", new[] { nameof(Basis) });
			}

			if(!string.IsNullOrWhiteSpace(CancelReason)
			&& CancelReason.Length > 1000)
			{
				yield return new ValidationResult("Длина причины отмены не должна превышать 1000 символов", new[] { nameof(CancelReason) });
			}

			if(!string.IsNullOrWhiteSpace(Explanation)
			&& Explanation.Length > 200)
			{
				yield return new ValidationResult("Длина пояснения не должна превышать 200 символов", new[] { nameof(Explanation) });
			}

			if(ExpenseCategoryId == null)
			{
				yield return new ValidationResult("Необходимо выбрать статью расхода", new[] { nameof(ExpenseCategoryId) });
			}
		}

		protected abstract void UpdateFileInformations();
		protected abstract void UpdateComments();

		public abstract void ChangeState(PayoutRequestState newState);
	}
}
