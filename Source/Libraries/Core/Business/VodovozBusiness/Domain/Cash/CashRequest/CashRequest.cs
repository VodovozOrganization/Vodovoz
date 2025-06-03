using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Cash
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "заявки на выдачу наличных денежных средств",
		Nominative = "заявка на выдачу наличных денежных средств",
		Accusative = "заявку на выдачу наличных денежных средств")]
	[HistoryTrace]
	[EntityPermission]
	public class CashRequest : PayoutRequestBase
	{
		private bool _haveReceipt;
		private IList<CashRequestSumItem> _sums = new List<CashRequestSumItem>();
		private GenericObservableList<CashRequestSumItem> _observableSums;

		#region Свойства

		public override string Title => $"Заявка на выдачу ДС №{Id} от {Date:d}";

		public override PayoutRequestDocumentType PayoutRequestDocumentType => PayoutRequestDocumentType.CashRequest;

		[Display(Name = "Наличие чека")]
		public virtual bool HaveReceipt
		{
			get => _haveReceipt;
			set => SetField(ref _haveReceipt, value);
		}

		[Display(Name = "Суммы")]
		public virtual IList<CashRequestSumItem> Sums
		{
			get => _sums;
			set => SetField(ref _sums, value);
		}

		public virtual GenericObservableList<CashRequestSumItem> ObservableSums =>
			_observableSums ?? (_observableSums = new GenericObservableList<CashRequestSumItem>(Sums));

		#endregion

		#region Методы

		public override void ChangeState(PayoutRequestState newState)
		{
			if(newState == PayoutRequestState)
			{
				return;
			}

			var exceptionMessage = $"Некорректная операция. Не предусмотрена смена статуса с {PayoutRequestState} на {newState}";

			switch(newState)
			{
				//Подана
				case PayoutRequestState.Submited:
					if(PayoutRequestState == PayoutRequestState.New
					|| PayoutRequestState == PayoutRequestState.OnClarification)
					{
						PayoutRequestState = newState;
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				//На уточнении
				case PayoutRequestState.OnClarification:
					if(PayoutRequestState == PayoutRequestState.AgreedBySubdivisionChief
					|| PayoutRequestState == PayoutRequestState.Agreed
					|| PayoutRequestState == PayoutRequestState.GivenForTake
					|| PayoutRequestState == PayoutRequestState.Canceled
					|| PayoutRequestState == PayoutRequestState.PartiallyClosed)
					{
						PayoutRequestState = newState;
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				//Согласована руководителем отдела
				case PayoutRequestState.AgreedBySubdivisionChief:
					if(PayoutRequestState == PayoutRequestState.Submited)
					{
						PayoutRequestState = newState;
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				//Согласована ЦФО
				case PayoutRequestState.AgreedByFinancialResponsibilityCenter:
					if(PayoutRequestState == PayoutRequestState.AgreedBySubdivisionChief)
					{
						PayoutRequestState = newState;
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}
					break;
				//Согласована исполнительным директором
				case PayoutRequestState.Agreed:
					if(PayoutRequestState == PayoutRequestState.AgreedByFinancialResponsibilityCenter
						|| PayoutRequestState == PayoutRequestState.AgreedBySubdivisionChief /// Убрать по заполнению ЦФО
						|| PayoutRequestState == PayoutRequestState.Submited)
					{
						PayoutRequestState = newState;
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				//Передана на выдачу
				case PayoutRequestState.GivenForTake:
					if(PayoutRequestState == PayoutRequestState.Agreed)
					{
						PayoutRequestState = newState;
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case PayoutRequestState.Canceled:
					if(PayoutRequestState == PayoutRequestState.Submited
					|| PayoutRequestState == PayoutRequestState.OnClarification
					|| PayoutRequestState == PayoutRequestState.GivenForTake
					|| PayoutRequestState == PayoutRequestState.Agreed
					|| PayoutRequestState == PayoutRequestState.AgreedBySubdivisionChief)
					{
						PayoutRequestState = newState;
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case PayoutRequestState.Closed:
					PayoutRequestState = Sums.All(x => x.Sum == x.ObservableExpenses.Sum(e => e.Money))
						? newState
						: PayoutRequestState.PartiallyClosed;
					break;

				case PayoutRequestState.PartiallyClosed:
					break;

				default:
					throw new NotImplementedException($"Не реализовано изменение статуса для {newState}");
			}
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			foreach(var validationResult in base.Validate(validationContext))
			{
				yield return validationResult;
			}

			if(Sums.Count < 1)
			{
				yield return new ValidationResult("Необходимо создать хотя бы одну сумму", new[] { nameof(Sums) });
			}

			foreach(var cashRequestSumItem in Sums)
			{
				if(cashRequestSumItem.AccountableEmployee == null)
				{
					yield return new ValidationResult(
						string.Format($"У суммы {cashRequestSumItem.Sum} должно быть заполнено подотчетное лицо"), new[] { nameof(Sums) });
				}
			}

			if(PayoutRequestState == PayoutRequestState.Agreed && Organization == null)
			{
				yield return new ValidationResult("Необходимо заполнить организацию", new[] { nameof(Organization) });
			}

			if(DatesInSumItems.Count > 1)
			{
				yield return new ValidationResult("Нельзя в одной заявке указывать разные даты выдачи. Создайте отдельную заявку на другую дату", new[] { nameof(Sums) });
			}
		}

		private List<DateTime> DatesInSumItems => Sums.Select(s => s.Date.Date).Distinct().ToList();

		public virtual void AddItem(CashRequestSumItem sumItem)
		{
			_observableSums.Add(sumItem);
		}

		public virtual void DeleteItem(CashRequestSumItem sumItem)
		{
			if(sumItem == null
			|| !_observableSums.Contains(sumItem))
			{
				return;
			}

			ObservableSums.Remove(sumItem);
		}

		protected override void UpdateFileInformations()
		{
		}

		protected override void UpdateComments()
		{
		}

		#endregion
	}
}
