using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NLog;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Gamma.Utilities;
using Vodovoz.Domain.Logistic;
using System.Linq;

namespace Vodovoz.Domain.Cash.CashTransfer
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы перемещения денежных средств",
		Nominative = "документ перемещения денежных средств",
		Prepositional = "документе перемещения денежных средств",
		PrepositionalPlural = "документах перемещения денежных средств"
	)]
	[HistoryTrace]
	[EntityPermission]
	public abstract class CashTransferDocumentBase : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		Logger logger = LogManager.GetCurrentClassLogger();

		private int id;
		public virtual int Id {
			get => id;
			set => SetField(ref id, value, () => Id);
		}

		private DateTime creationDate;
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate {
			get => creationDate;
			set => SetField(ref creationDate, value, () => CreationDate);
		}

		private Employee authtor;
		[Display(Name = "Автор")]
		public virtual Employee Author {
			get => authtor;
			set => SetField(ref authtor, value, () => Author);
		}

		private Car car;
		[Display(Name = "Автомобиль")]
		public virtual Car Car {
			get => car;
			set => SetField(ref car, value, () => Car);
		}

		private Employee driver;
		[Display(Name = "Водитель")]
		public virtual Employee Driver {
			get => driver;
			set => SetField(ref driver, value, () => Driver);
		}

		private CashTransferDocumentStatuses status;
		[Display(Name = "Статус")]
		public virtual CashTransferDocumentStatuses Status {
			get => status;
			set => SetField(ref status, value, () => Status);
		}

		private decimal transferedSum;
		[Display(Name = "Транспортируемая сумма")]
		public virtual decimal TransferedSum {
			get => transferedSum;
			set => SetField(ref transferedSum, value, () => TransferedSum);
		}

		private CashTransferOperation cashTransferOperation;
		[Display(Name = "Операция транспортировки денег")]
		public virtual CashTransferOperation CashTransferOperation {
			get => cashTransferOperation;
			set => SetField(ref cashTransferOperation, value, () => CashTransferOperation);
		}

		#region sender information

		private Subdivision cashSubdivisionFrom;
		[Display(Name = "Касса отправитель")]
		public virtual Subdivision CashSubdivisionFrom {
			get => cashSubdivisionFrom;
			set => SetField(ref cashSubdivisionFrom, value, () => CashSubdivisionFrom);
		}

		private Expense expenseOperation;
		[Display(Name = "Ордер списания денег с кассы отправителя")]
		public virtual Expense ExpenseOperation {
			get => expenseOperation;
			set => SetField(ref expenseOperation, value, () => ExpenseOperation);
		}

		private ExpenseCategory expenseCategory;
		[Display(Name = "Статья расхода")]
		public virtual ExpenseCategory ExpenseCategory {
			get => expenseCategory;
			set => SetField(ref expenseCategory, value, () => ExpenseCategory);
		}

		private DateTime? sendTime;
		[Display(Name = "Время отправки")]
		public virtual DateTime? SendTime {
			get => sendTime;
			set => SetField(ref sendTime, value, () => SendTime);
		}

		private Employee cashierSender;
		[Display(Name = "Отправивший кассир")]
		public virtual Employee CashierSender {
			get => cashierSender;
			set => SetField(ref cashierSender, value, () => CashierSender);
		}

		#endregion sender information

		#region receiver information

		private Subdivision cashSubdivisionTo;
		[Display(Name = "Касса получатель")]
		public virtual Subdivision CashSubdivisionTo {
			get => cashSubdivisionTo;
			set => SetField(ref cashSubdivisionTo, value, () => CashSubdivisionTo);
		}

		private Income incomeOperation;
		[Display(Name = "Ордер прихода денег в кассу получателя")]
		public virtual Income IncomeOperation {
			get => incomeOperation;
			set => SetField(ref incomeOperation, value, () => IncomeOperation);
		}

		private IncomeCategory incomeCategory;
		[Display(Name = "Статья прихода")]
		public virtual IncomeCategory IncomeCategory {
			get => incomeCategory;
			set => SetField(ref incomeCategory, value, () => IncomeCategory);
		}


		private DateTime? receiveTime;
		[Display(Name = "Время получения")]
		public virtual DateTime? ReceiveTime {
			get => receiveTime;
			set => SetField(ref receiveTime, value, () => ReceiveTime);
		}

		private Employee cashierReceiver;
		[Display(Name = "Принявший кассир")]
		public virtual Employee CashierReceiver {
			get => cashierReceiver;
			set => SetField(ref cashierReceiver, value, () => CashierReceiver);
		}

		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		#endregion receiver information

		public virtual string Title => $"№{Id} от {CreationDate}";

		public CashTransferDocumentBase()
		{
		}

		#region state changing methods

		/// <summary>
		/// Отправка документа перемещения денежных средств.
		/// При отправке проверяются, правильно ли заполнен документ, имеет ли он необходимый для отправки статус.
		/// Если документ не удовлетворяет условиям, бросается исключение <see cref="ArgumentNullException"></see>
		/// или <see cref="InvalidOperationException"></see>.
		/// Если при создании операций возникло не предвиденное исключение документ откатывается на исходное состояние и бросается возникшее исключение.
		/// </summary>
		/// <param name="sender">Кассир, который будет указан в документе и операциях перемещения как отправитель</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public virtual void Send(Employee sender)
		{
			if(sender == null) {
				throw new ArgumentNullException($"Не указано кто является кассиром");
			}

			if(Status != CashTransferDocumentStatuses.New) {
				throw new InvalidOperationException($"Невозможно отправить документ транспортировки денег не из статуса {CashTransferDocumentStatuses.New.GetEnumTitle()}");
			}

			if(CashTransferOperation != null || ExpenseOperation != null) {
				throw new InvalidOperationException($"Денежные средства уже были отправлены ранее в этом же документе, изменить данные о факте отправки денег невозможно");
			}

			TransferedSum = CalculateTransferedSum();

			string exceptionMessage = RaiseValidationAndGetResult();
			if(!string.IsNullOrWhiteSpace(exceptionMessage)) {
				throw new InvalidOperationException(exceptionMessage);
			}

			DateTime now = DateTime.Now;

			try {
				CashTransferOperation newCashTransferOperation = new CashTransferOperation {
					CashTransferDocument = this,
					SubdivisionFrom = CashSubdivisionFrom,
					SubdivisionTo = CashSubdivisionTo,
					TransferedSum = TransferedSum,
					SendTime = now
				};

				Expense newExpenseOperation = new Expense {
					TypeDocument = ExpenseInvoiceDocumentType.ExpenseTransferDocument,
					Casher = sender,
					Date = now,
					ExpenseCategory = ExpenseCategory,
					TypeOperation = ExpenseType.Expense,
					Money = TransferedSum,
					RelatedToSubdivision = CashSubdivisionFrom,
					CashTransferDocument = this
				};

				Status = CashTransferDocumentStatuses.Sent;
				CashierSender = sender;
				SendTime = now;
				CashTransferOperation = newCashTransferOperation;
				ExpenseOperation = newExpenseOperation;
			} catch(Exception) {
				//восстанавливаем состояние
				Status = CashTransferDocumentStatuses.New;
				CashTransferOperation = null;
				ExpenseOperation = null;
				throw;
			}
		}

		/// <summary>
		/// Принятие документа перемещения денежных средств.
		/// При принятии проверяется, правильно ли заполнен документ, имеет ли он необходимый для принятия статус.
		/// Если документ не удовлетворяет условиям, бросается исключение <see cref="ArgumentNullException"></see>
		/// или <see cref="InvalidOperationException"></see>.
		/// Если при создании операций возникло не предвиденное исключение документ откатывается на исходное состояние и бросается возникшее исключение.
		/// </summary>
		/// <param name="receiver">Кассир, который будет указан в документе и операциях перемещения как отправитель</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public virtual void Receive(Employee receiver)
		{
			if(receiver == null) {
				throw new ArgumentNullException($"Не указано кто является кассиром");
			}

			if(CashTransferOperation == null) {
				throw new InvalidOperationException($"Не было создано операции перемещения денежных средств, без нее невозможно создать операцию принятия");
			}

			if(Status != CashTransferDocumentStatuses.Sent) {
				throw new InvalidOperationException($"Невозможно принять документ транспортировки денег не из статуса \"{CashTransferDocumentStatuses.Sent.GetEnumTitle()}\"");
			}

			if(IncomeOperation != null && CashTransferOperation.ReceiveTime.HasValue) {
				throw new InvalidOperationException($"Денежные средства уже были приняты ранее, изменить данные о факте принятия денег невозможно");
			}

			string exceptionMessage = RaiseValidationAndGetResult();
			if(!string.IsNullOrWhiteSpace(exceptionMessage)) {
				throw new InvalidOperationException(exceptionMessage);
			}

			DateTime now = DateTime.Now;

			try {
				Income newIncomeOperation = new Income {
					TypeDocument = IncomeInvoiceDocumentType.IncomeTransferDocument,
					Casher = receiver,
					Date = now,
					IncomeCategory = IncomeCategory,
					TypeOperation = IncomeType.Common,
					Money = TransferedSum,
					RelatedToSubdivision = CashSubdivisionTo,
					CashTransferDocument = this
				};

				Status = CashTransferDocumentStatuses.Received;
				CashierReceiver = receiver;
				ReceiveTime = now;
				CashTransferOperation.ReceiveTime = now;
				IncomeOperation = newIncomeOperation;
			} catch(Exception) {
				//восстанавливаем состояние
				Status = CashTransferDocumentStatuses.Sent;
				CashierReceiver = null;
				CashTransferOperation.ReceiveTime = null;
				IncomeOperation = null;
				throw;
			}
		}

		#endregion

		private string RaiseValidationAndGetResult()
		{
			string result = null;
			var validationResult = Validate(new ValidationContext(this, new Dictionary<object, object>()));
			if(validationResult.Any()) {
				result = string.Join(Environment.NewLine, validationResult.Select(x => x.ErrorMessage));
			}
			return result;
		}

		//для воможности переопределения расчета суммы
		protected virtual decimal CalculateTransferedSum()
		{
			return TransferedSum;
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Driver == null) {
				yield return new ValidationResult("Должен быть заполнен водитель", new[] { nameof(Driver) });
			}
			if(Car == null) {
				yield return new ValidationResult("Должен быть заполнен автомобиль", new[] { nameof(Car) });
			}
			if(CashSubdivisionFrom == null) {
				yield return new ValidationResult("Должна быть заполнена касса из которой переносятся деньги", new[] { nameof(CashSubdivisionFrom) });
			} else if(CashSubdivisionFrom.Id == 0) {
				yield return new ValidationResult("Должна быть выбрана существующая касса", new[] { nameof(CashSubdivisionFrom) });
			}
			if(CashSubdivisionTo == null) {
				yield return new ValidationResult("Должна быть заполнена касса в которую переносятся деньги", new[] { nameof(CashSubdivisionTo) });
			} else if(CashSubdivisionTo.Id == 0) {
				yield return new ValidationResult("Должна быть выбрана существующая касса", new[] { nameof(CashSubdivisionTo) });
			}
			if(CashSubdivisionFrom != null && CashSubdivisionTo != null && CashSubdivisionFrom.Id == CashSubdivisionTo.Id) {
				yield return new ValidationResult("Невозможно перенести деньги в ту же самую кассу");
			}
			if(IncomeCategory == null) {
				yield return new ValidationResult("Должна быть выбрана статья прихода", new[] { nameof(IncomeCategory) });
			}
			if(ExpenseCategory == null) {
				yield return new ValidationResult("Должна быть выбрана статья расхода", new[] { nameof(ExpenseCategory) });
			}
			if(CalculateTransferedSum() <= 0) {
				yield return new ValidationResult("Сумма денежных средств для перемещения должна быть больше нуля");
			}
		}
	}

	public enum CashTransferDocumentStatuses
	{
		[Display(Name = "Новый")]
		New,
		[Display(Name = "Отправлен")]
		Sent,
		[Display(Name = "Получен")]
		Received
	}

	public class CashTransferDocumentStatusesStringType : NHibernate.Type.EnumStringType
	{
		public CashTransferDocumentStatusesStringType() : base(typeof(CashTransferDocumentStatuses))
		{
		}
	}
}
