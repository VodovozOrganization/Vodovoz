using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Payments;
using VodovozBusiness.Domain.Cash.CashRequest;
using VodovozBusiness.Domain.Payments;

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
		private IObservableList<OutgoingPayment> _outgoingPayments = new ObservableList<OutgoingPayment>();

		private decimal _sum;
		private Counterparty _counterparty;
		private int? _financialResponsibilityCenterId;
		private DateTime? _paymentDatePlanned;
		private int? _ourOrganizationBankAccountId;
		private int? _supplierBankAccountId;
		private string _billNumber;
		private DateTime? _billDate;
		private string _paymentPurpose;
		private bool _isImidiatelyBill;
		private decimal _vatValue;

		public static readonly PayoutRequestState[] AllowedToChangeFinancialResponsibilityCenterIdStates = new[]
		{
			PayoutRequestState.New,
			PayoutRequestState.Submited,
			PayoutRequestState.AgreedBySubdivisionChief,
			PayoutRequestState.OnClarification,
			PayoutRequestState.Canceled
		};

		public static readonly PayoutRequestState[] AllowedToChangePlainPropertiesStates = new[]
		{
			PayoutRequestState.New,
			PayoutRequestState.Submited,
			PayoutRequestState.AgreedBySubdivisionChief,
			PayoutRequestState.AgreedByFinancialResponsibilityCenter,
			PayoutRequestState.WaitingForAgreedByExecutiveDirector,
			PayoutRequestState.OnClarification,
			PayoutRequestState.Canceled
		};

		public override int Id
		{
			get => base.Id;
			protected set
			{
				if(base.Id != value)
				{
					base.Id = value;
					UpdateOutgoingPayments();
				}
			}
		}

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
		public virtual decimal VatValue
		{
			get => _vatValue;
			set => SetField(ref _vatValue, value);
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
		public virtual IObservableList<OutgoingPayment> OutgoingPayments
		{
			get => _outgoingPayments;
			set => SetField(ref _outgoingPayments, value);
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

		public virtual CashlessRequest Copy1To11()
		{
			return new CashlessRequest
			{
				Date = Date,
				Author = Author,
				Subdivision = Subdivision,
				FinancialResponsibilityCenterId = FinancialResponsibilityCenterId,
				Organization = Organization,
				OurOrganizationBankAccountId = OurOrganizationBankAccountId,
				Counterparty = Counterparty,
				SupplierBankAccountId = SupplierBankAccountId,
				ExpenseCategoryId = ExpenseCategoryId,
				BillNumber = BillNumber,
				BillDate = BillDate,
				Sum = Sum,
				VatValue = VatValue,
				PaymentPurpose = PaymentPurpose,
			};
		}

		public virtual void RemoveOutgoingPayment(int id)
		{
			var paymentToRemove = OutgoingPayments.Where(x => id == x.Id).FirstOrDefault();
			
			if(paymentToRemove is null)
			{
				return;
			}

			paymentToRemove.CashlessRequestId = null;

			OutgoingPayments.Remove(paymentToRemove);
		}

		public virtual void AddOutgoingPayments(IEnumerable<OutgoingPayment> outgoingPayments)
		{
			foreach(var payment in outgoingPayments)
			{
				payment.CashlessRequestId = Id;
				OutgoingPayments.Add(payment);
			}
		}

		public virtual void UpdateOutgoingPayments()
		{
			foreach(var outgoingPayment in OutgoingPayments)
			{
				outgoingPayment.CashlessRequestId = Id;
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

			var stateValidationResult = ValidateState(nextState);

			if(stateValidationResult != ValidationResult.Success)
			{
				yield return ValidateState(nextState);
			}

			switch(nextState)
			{
				case PayoutRequestState.Submited:
				case PayoutRequestState.AgreedBySubdivisionChief:
					var submittedValidationsErrors = ValidateSubmited();

					foreach(var error in submittedValidationsErrors)
					{
						yield return error;
					}

					break;

				case PayoutRequestState.GivenForTake:
				case PayoutRequestState.PartiallyClosed:
				case PayoutRequestState.Closed:
					var submittedValidationsErrors2 = ValidateSubmited();

					foreach(var error in submittedValidationsErrors2)
					{
						yield return error;
					}

					if(OurOrganizationBankAccountId is null)
					{
						yield return new ValidationResult("Не указан расчетный счет нашей организации", new[] { nameof(OurOrganizationBankAccountId) });
					}

					break;
			}
		}

		private IEnumerable<ValidationResult> ValidateSubmited()
		{
			if(Subdivision is null)
			{
				yield return new ValidationResult("Не указано подразделение", new[] { nameof(Subdivision) });
			}

			if(FinancialResponsibilityCenterId is null)
			{
				yield return new ValidationResult("Не указан ЦФО", new[] { nameof(FinancialResponsibilityCenterId) });
			}

			if(PaymentDatePlanned is null)
			{
				yield return new ValidationResult("Не указана дата платежа (план)", new[] { nameof(PaymentDatePlanned) });
			}

			if(Organization is null)
			{
				yield return new ValidationResult("Необходимо заполнить организацию", new[] { nameof(Organization) });
			}

			if(Counterparty is null)
			{
				yield return new ValidationResult("Необходимо заполнить поставщика", new[] { nameof(Counterparty) });
			}

			if(SupplierBankAccountId is null)
			{
				yield return new ValidationResult("Необходимо заполнить расчетный счет поставщика", new[] { nameof(SupplierBankAccountId) });
			}

			if(ExpenseCategoryId is null)
			{
				yield return new ValidationResult("Необходимо заполнить статью расхода", new[] { nameof(ExpenseCategoryId) });
			}

			if(Sum <= 0)
			{
				yield return new ValidationResult("Необходимо заполнить сумму", new[] { nameof(Sum) });
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
					&& PayoutRequestState != PayoutRequestState.OnClarification
					&& PayoutRequestState != PayoutRequestState.Canceled)
				{
					return new ValidationResult(errorMessage, new[] { nameof(PayoutRequestState) });
				}
			}
			else if(nextState == PayoutRequestState.AgreedBySubdivisionChief)
			{
				if(PayoutRequestState != PayoutRequestState.Submited)
				{
					return new ValidationResult(errorMessage, new[] { nameof(PayoutRequestState) });
				}
			}
			else if(nextState == PayoutRequestState.AgreedByFinancialResponsibilityCenter)
			{
				if(PayoutRequestState != PayoutRequestState.AgreedBySubdivisionChief)
				{
					return new ValidationResult(errorMessage, new[] { nameof(PayoutRequestState) });
				}
			}
			else if(nextState == PayoutRequestState.WaitingForAgreedByExecutiveDirector)
			{
				if(PayoutRequestState != PayoutRequestState.AgreedByFinancialResponsibilityCenter)
				{
					return new ValidationResult(errorMessage, new[] { nameof(PayoutRequestState) });
				}
			}
			else if(nextState == PayoutRequestState.GivenForTake)
			{
				if(PayoutRequestState != PayoutRequestState.WaitingForAgreedByExecutiveDirector)
				{
					return new ValidationResult(errorMessage, new[] { nameof(PayoutRequestState) });
				}
			}
			else if(nextState == PayoutRequestState.PartiallyClosed)
			{
				if(PayoutRequestState != PayoutRequestState.GivenForTake)
				{
					return new ValidationResult(errorMessage, new[] { nameof(PayoutRequestState) });
				}
			}
			else if(nextState == PayoutRequestState.OnClarification)
			{
				if(PayoutRequestState != PayoutRequestState.Submited
					&& PayoutRequestState != PayoutRequestState.AgreedBySubdivisionChief
					&& PayoutRequestState != PayoutRequestState.AgreedByFinancialResponsibilityCenter
					&& PayoutRequestState != PayoutRequestState.WaitingForAgreedByExecutiveDirector
					&& PayoutRequestState != PayoutRequestState.Agreed)
				{
					return new ValidationResult(errorMessage, new[] { nameof(PayoutRequestState) });
				}
			}
			else if(nextState == PayoutRequestState.Canceled)
			{
				if(PayoutRequestState != PayoutRequestState.New
					&& PayoutRequestState != PayoutRequestState.Submited
					&& PayoutRequestState != PayoutRequestState.AgreedBySubdivisionChief
					&& PayoutRequestState != PayoutRequestState.AgreedByFinancialResponsibilityCenter
					&& PayoutRequestState != PayoutRequestState.WaitingForAgreedByExecutiveDirector
					&& PayoutRequestState != PayoutRequestState.Agreed
					&& PayoutRequestState != PayoutRequestState.GivenForTake)
				{
					return new ValidationResult(errorMessage, new[] { nameof(PayoutRequestState) });
				}
			}
			else if(nextState == PayoutRequestState.Closed)
			{
				if(PayoutRequestState != PayoutRequestState.GivenForTake
					&& PayoutRequestState != PayoutRequestState.PartiallyClosed)
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

		#endregion IValidationImplementation
	}
}
