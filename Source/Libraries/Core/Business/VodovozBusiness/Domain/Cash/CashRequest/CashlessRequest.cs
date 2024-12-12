using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Cash.CashRequest;

namespace Vodovoz.Domain.Cash
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "заявки на оплату по безналу",
		Nominative = "заявка на оплату по безналу",
		Accusative = "заявку на оплату по безналу")]
	[HistoryTrace]
	[EntityPermission]
	public class CashlessRequest : PayoutRequestBase, IHasAttachedFilesInformations<CashlessRequestFileInformation>
	{
		private decimal _sum;
		private Counterparty _counterparty;
		private IObservableList<CashlessRequestFileInformation> _attachedFileInformations = new ObservableList<CashlessRequestFileInformation>();

		#region Свойства

		public override string Title => $"Заявка на оплату по Б/Н №{Id} от {Date:d}";

		public override PayoutRequestDocumentType PayoutRequestDocumentType => PayoutRequestDocumentType.CashlessRequest;

		[Display(Name = "Сумма")]
		public virtual decimal Sum
		{
			get => _sum;
			set => SetField(ref _sum, value);
		}

		[Display(Name = "Поставщик")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		[Display(Name = "Информация о прикрепленных файлах")]
		public virtual IObservableList<CashlessRequestFileInformation> AttachedFileInformations
		{
			get => _attachedFileInformations;
			set => SetField(ref _attachedFileInformations, value);
		}

		#endregion

		#region Методы

		public override void ChangeState(PayoutRequestState newState)
		{
			if(newState == PayoutRequestState)
			{
				return;
			}

			PayoutRequestState = newState;
		}

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

		public virtual void RemoveFileInformation(string fileName)
		{
			AttachedFileInformations.Remove(AttachedFileInformations.FirstOrDefault(afi => afi.FileName == fileName));
		}

		#endregion

		protected override void UpdateFileInformations()
		{
			foreach(var fileInformation in AttachedFileInformations)
			{
				fileInformation.CashlessReqwuestId = Id;
			}
		}

		#region IValidationImplementation

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			#region Без учета статуса

			foreach(var validationResult in base.Validate(validationContext))
			{
				yield return validationResult;
			}

			if(Sum <= 0)
			{
				yield return new ValidationResult("Сумма должна быть больше нуля", new[] { nameof(Sum) });
			}

			if(!AttachedFileInformations.Any())
			{
				yield return new ValidationResult("Необходимо добавить хотя бы один файл", new[] { nameof(AttachedFileInformations) });
			}

			if(Counterparty == null)
			{
				yield return new ValidationResult("Необходимо указать поставщика", new[] { nameof(Counterparty) });
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
