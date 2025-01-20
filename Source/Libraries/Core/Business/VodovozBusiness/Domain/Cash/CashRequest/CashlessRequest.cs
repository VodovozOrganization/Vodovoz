using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Payments;
using VodovozBusiness.Domain.Cash.CashRequest;

namespace Vodovoz.Domain.Cash
{
	/// <summary>
	/// Заявка на оплату по безналу
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "заявки на оплату по безналу",
		Nominative = "заявка на оплату по безналу",
		Accusative = "заявку на оплату по безналу")]
	[HistoryTrace]
	[EntityPermission]
	public class CashlessRequest : PayoutRequestBase, IHasAttachedFilesInformations<CashlessRequestFileInformation>
	{
		private IObservableList<CashlessRequestComment> _comments = new ObservableList<CashlessRequestComment>();
		private IObservableList<CashlessRequestFileInformation> _attachedFileInformations = new ObservableList<CashlessRequestFileInformation>();
		private IObservableList<Payment> _payments = new ObservableList<Payment>();

		private decimal _sum;
		private Counterparty _counterparty;
		private int? _financialResponsibilityCenterId;
		private DateTime? _paymentDatePlanned;
		private int? _ourOrganizationBankAccountId;
		private int? _supplierBankAccountId;
		private string _billNumber;
		private DateTime? _billDate;
		private VAT _vatType;
		private string _paymentPurpose;
		private bool _isImidiatelyBill;

		public override string Title => $"Заявка на оплату по Б/Н №{Id} от {Date:d}";

		public override PayoutRequestDocumentType PayoutRequestDocumentType => PayoutRequestDocumentType.CashlessRequest;

		/// <summary>
		/// Идентификатор центра финансовой ответственности
		/// </summary>
		[Display(Name = "ЦФО")]
		[HistoryIdentifier(TargetType = typeof(FinancialResponsibilityCenter))]
		public virtual int? FinancialResponsibilityCenterId
		{
			get => _financialResponsibilityCenterId;
			set => SetField(ref _financialResponsibilityCenterId, value);
		}

		/// <summary>
		/// Дата платежа (план)
		/// </summary>
		[Display(Name = "Дата платежа (план)")]
		public virtual DateTime? PaymentDatePlanned
		{
			get => _paymentDatePlanned;
			set => SetField(ref _paymentDatePlanned, value);
		}

		/// <summary>
		/// Расчетный счет нашей организации
		/// </summary>
		[Display(Name = "Расчетный счёт (Н)")]
		[HistoryIdentifier(TargetType = typeof(Account))]
		public virtual int? OurOrganizationBankAccountId
		{
			get => _ourOrganizationBankAccountId;
			set => SetField(ref _ourOrganizationBankAccountId, value);
		}

		/// <summary>
		/// Поставщик
		/// </summary>
		[Display(Name = "Поставщик")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		/// <summary>
		/// Расчетный счет поставщика
		/// </summary>
		[Display(Name = "Расчетный счёт (П)")]
		[HistoryIdentifier(TargetType = typeof(Account))]
		public virtual int? SupplierBankAccountId
		{
			get => _supplierBankAccountId;
			set => SetField(ref _supplierBankAccountId, value);
		}

		/// <summary>
		/// Номер счёта
		/// </summary>
		[Display(Name = "Номер счёта")]
		public virtual string BillNumber
		{
			get => _billNumber;
			set => SetField(ref _billNumber, value);
		}

		/// <summary>
		/// Дата счёта
		/// </summary>
		[Display(Name = "Дата счёта")]
		public virtual DateTime? BillDate
		{
			get => _billDate;
			set => SetField(ref _billDate, value);
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
		/// Ставка НДС в счёте
		/// </summary>
		[Display(Name = "Ставка НДС в счёте")]
		public virtual VAT VatType
		{
			get => _vatType;
			set => SetField(ref _vatType, value);
		}

		/// <summary>
		/// Назначение платежа
		/// </summary>
		[Display(Name = "Назначение платежа")]
		public virtual string PaymentPurpose
		{
			get => _paymentPurpose;
			set => SetField(ref _paymentPurpose, value);
		}

		/// <summary>
		/// Комментарии
		/// </summary>
		[Display(Name = "Комментарии")]
		public virtual IObservableList<CashlessRequestComment> Comments
		{
			get => _comments;
			set => SetField(ref _comments, value);
		}

		/// <summary>
		/// Информация о прикрепленных файлах
		/// </summary>
		[Display(Name = "Информация о прикрепленных файлах")]
		public virtual IObservableList<CashlessRequestFileInformation> AttachedFileInformations
		{
			get => _attachedFileInformations;
			set => SetField(ref _attachedFileInformations, value);
		}

		/// <summary>
		/// Платежи
		/// </summary>
		[Display(Name = "Платежи")]
		public virtual IObservableList<Payment> Payments
		{
			get => _payments;
			set => SetField(ref _payments, value);
		}

		/// <summary>
		/// Срочный платёж
		/// </summary>
		[Display(Name = "Срочный платёж")]
		public virtual bool IsImidiatelyBill
		{
			get => _isImidiatelyBill;
			set => SetField(ref _isImidiatelyBill, value);
		}

		#region Методы

		/// <summary>
		/// Изменение статуса
		/// </summary>
		/// <param name="newState">Новый статус</param>
		public override void ChangeState(PayoutRequestState newState)
		{
			if(newState == PayoutRequestState)
			{
				return;
			}

			PayoutRequestState = newState;
		}

		/// <summary>
		/// Добавление информации о файле
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		public virtual void AddFileInformation(string fileName)
		{
			if(AttachedFileInformations.Any(afi => afi.FileName == fileName))
			{
				return;
			}

			AttachedFileInformations.Add(new CashlessRequestFileInformation
			{
				FileName = fileName,
				CashlessReqwuestId = Id
			});
		}

		/// <summary>
		/// Удаление информации о файле
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		public virtual void RemoveFileInformation(string fileName)
		{
			AttachedFileInformations.Remove(AttachedFileInformations.FirstOrDefault(afi => afi.FileName == fileName));
		}

		#endregion

		/// <summary>
		/// Обновление информации о файлах
		/// 
		/// Обновляет идентификатор заявки на оплату в информации о файлах
		/// </summary>
		protected override void UpdateFileInformations()
		{
			foreach(var fileInformation in AttachedFileInformations)
			{
				fileInformation.CashlessReqwuestId = Id;
			}
		}

		/// <summary>
		/// Добавление комментария
		/// </summary>
		/// <param name="cashlessRequestComment">Комментарий</param>
		public virtual void AddComment(CashlessRequestComment cashlessRequestComment)
		{
			cashlessRequestComment.CashlessRequestId = Id;
			Comments.Add(cashlessRequestComment);
		}

		/// <summary>
		/// Обновление комментариев
		/// 
		/// Обновляет идентификатор заявки на оплату в комментариях
		/// </summary>
		protected override void UpdateComments()
		{
			foreach(var comment in Comments)
			{
				comment.CashlessRequestId = Id;
			}
		}

		#region IValidationImplementation

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			#region Без учета статуса

			if(Author == null)
			{
				yield return new ValidationResult(
					"Автор не указан. Ваш пользователь не привязан к сотруднику, которого можно указать в качестве автора",
					new[] { nameof(Author) });
			}

			if(Subdivision == null)
			{
				yield return new ValidationResult("Не указано подразделение", new[] { nameof(Subdivision) });
			}

			if(FinancialResponsibilityCenterId == null)
			{
				yield return new ValidationResult("Не указан ЦФО", new[] { nameof(FinancialResponsibilityCenterId) });
			}

			if(Sum < 0)
			{
				yield return new ValidationResult("Сумма должна быть положительной", new[] { nameof(Sum) });
			}

			#endregion

			validationContext.Items.TryGetValue("next_state", out var nextStateValue);
			if(!Enum.TryParse(nextStateValue?.ToString(), ignoreCase: true, out PayoutRequestState nextState))
			{
				yield break;
			}

			yield return ValidateState(nextState);

			switch(nextState)
			{
				case PayoutRequestState.Canceled:
					if(string.IsNullOrWhiteSpace(CancelReason))
					{
						yield return new ValidationResult("Не указана причина отмены", new[] { nameof(CancelReason) });
					}

					break;
				case PayoutRequestState.GivenForTake:
				{
					if(Organization == null)
					{
						yield return new ValidationResult("Необходимо заполнить организацию", new[] { nameof(Organization) });
					}

					break;
				}
				case PayoutRequestState.Closed:
				{
					if(ExpenseCategoryId == null)
					{
						yield return new ValidationResult("Необходимо заполнить статью расхода", new[] { nameof(ExpenseCategoryId) });
					}

					break;
				}
				case PayoutRequestState.OnClarification:
					if(string.IsNullOrWhiteSpace(ReasonForSendToReappropriate))
					{
						yield return new ValidationResult("Необходимо заполнить причину отправки на пересогласование",
							new[] { nameof(ReasonForSendToReappropriate) });
					}

					break;
			}
		}

		private ValidationResult ValidateState(PayoutRequestState nextState)
		{
			if(nextState == PayoutRequestState)
			{
				return ValidationResult.Success;
			}

			var errorMessage = $"Некорректная операция. Не предусмотрена смена статуса с {PayoutRequestState} на {nextState}";

			if(nextState == PayoutRequestState.Submited)
			{
				if(PayoutRequestState != PayoutRequestState.New
				&& PayoutRequestState != PayoutRequestState.OnClarification)
				{
					return new ValidationResult(errorMessage, new[] { nameof(PayoutRequestState) });
				}
			}
			else if(nextState == PayoutRequestState.OnClarification)
			{
				if(PayoutRequestState != PayoutRequestState.Agreed
				&& PayoutRequestState != PayoutRequestState.GivenForTake
				&& PayoutRequestState != PayoutRequestState.Canceled)
				{
					return new ValidationResult(errorMessage, new[] { nameof(PayoutRequestState) });
				}
			}
			else if(nextState == PayoutRequestState.Agreed)
			{
				if(PayoutRequestState != PayoutRequestState.Submited)
				{
					return new ValidationResult(errorMessage, new[] { nameof(PayoutRequestState) });
				}
			}
			else if(nextState == PayoutRequestState.GivenForTake)
			{
				if(PayoutRequestState != PayoutRequestState.Agreed)
				{
					return new ValidationResult(errorMessage, new[] { nameof(PayoutRequestState) });
				}
			}
			else if(nextState == PayoutRequestState.Canceled)
			{
				if(PayoutRequestState != PayoutRequestState.Submited
				&& PayoutRequestState != PayoutRequestState.OnClarification
				&& PayoutRequestState != PayoutRequestState.GivenForTake
				&& PayoutRequestState != PayoutRequestState.Agreed)
				{
					return new ValidationResult(errorMessage, new[] { nameof(PayoutRequestState) });
				}
			}
			else if(nextState == PayoutRequestState.Closed)
			{
				if(PayoutRequestState != PayoutRequestState.GivenForTake)
				{
					return new ValidationResult(errorMessage, new[] { nameof(PayoutRequestState) });
				}
			}
			else
			{
				return new ValidationResult($"Не реализовано изменение статуса для {nextState}", new[] { nameof(PayoutRequestState) });
			}

			return ValidationResult.Success;
		}

		#endregion
	}
}
