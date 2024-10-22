using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;

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
		private const int _commentLimit = 255;
		private int _id;
		private DateTime _creationDate;
		private Employee _authtor;
		private Car _car;
		private Employee _driver;
		private CashTransferDocumentStatuses _status;
		private decimal _transferedSum;
		private CashTransferOperation _cashTransferOperation;
		private Subdivision _cashSubdivisionFrom;
		private Expense _expenseOperation;
		private int? _expenseCategoryId;
		private DateTime? _sendTime;
		private Employee _cashierSender;
		private Subdivision _cashSubdivisionTo;
		private Income _incomeOperation;
		private int? _incomeCategoryId;
		private DateTime? _receiveTime;
		private Employee _cashierReceiver;
		private string _comment;

		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _authtor;
			set => SetField(ref _authtor, value);
		}

		[Display(Name = "Автомобиль")]
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		[Display(Name = "Водитель")]
		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		[Display(Name = "Статус")]
		public virtual CashTransferDocumentStatuses Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Транспортируемая сумма")]
		public virtual decimal TransferedSum
		{
			get => _transferedSum;
			set => SetField(ref _transferedSum, value);
		}

		[Display(Name = "Операция транспортировки денег")]
		public virtual CashTransferOperation CashTransferOperation
		{
			get => _cashTransferOperation;
			set => SetField(ref _cashTransferOperation, value);
		}

		#region sender information

		[Display(Name = "Касса отправитель")]
		public virtual Subdivision CashSubdivisionFrom
		{
			get => _cashSubdivisionFrom;
			set => SetField(ref _cashSubdivisionFrom, value);
		}

		[Display(Name = "Ордер списания денег с кассы отправителя")]
		public virtual Expense ExpenseOperation
		{
			get => _expenseOperation;
			set => SetField(ref _expenseOperation, value);
		}

		[Display(Name = "Статья расхода")]
		public virtual int? ExpenseCategoryId
		{
			get => _expenseCategoryId;
			set => SetField(ref _expenseCategoryId, value);
		}

		[Display(Name = "Время отправки")]
		public virtual DateTime? SendTime
		{
			get => _sendTime;
			set => SetField(ref _sendTime, value);
		}

		[Display(Name = "Отправивший кассир")]
		public virtual Employee CashierSender
		{
			get => _cashierSender;
			set => SetField(ref _cashierSender, value);
		}

		#endregion sender information

		#region receiver information

		[Display(Name = "Касса получатель")]
		public virtual Subdivision CashSubdivisionTo
		{
			get => _cashSubdivisionTo;
			set => SetField(ref _cashSubdivisionTo, value);
		}

		[Display(Name = "Ордер прихода денег в кассу получателя")]
		public virtual Income IncomeOperation
		{
			get => _incomeOperation;
			set => SetField(ref _incomeOperation, value);
		}

		[Display(Name = "Статья прихода")]
		public virtual int? IncomeCategoryId
		{
			get => _incomeCategoryId;
			set => SetField(ref _incomeCategoryId, value);
		}

		[Display(Name = "Время получения")]
		public virtual DateTime? ReceiveTime
		{
			get => _receiveTime;
			set => SetField(ref _receiveTime, value);
		}

		[Display(Name = "Принявший кассир")]
		public virtual Employee CashierReceiver
		{
			get => _cashierReceiver;
			set => SetField(ref _cashierReceiver, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
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
		/// <param name="description">Комментарий основание</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public virtual void Send(Employee sender, string description)
		{
			if(sender == null)
			{
				throw new ArgumentNullException(nameof(sender), $"Не указано кто является кассиром");
			}

			if(Status != CashTransferDocumentStatuses.New)
			{
				throw new InvalidOperationException($"Невозможно отправить документ транспортировки денег не из статуса {CashTransferDocumentStatuses.New.GetEnumTitle()}");
			}

			if(CashTransferOperation != null || ExpenseOperation != null)
			{
				throw new InvalidOperationException($"Денежные средства уже были отправлены ранее в этом же документе, изменить данные о факте отправки денег невозможно");
			}

			TransferedSum = CalculateTransferedSum();

			string exceptionMessage = RaiseValidationAndGetResult();

			if(!string.IsNullOrWhiteSpace(exceptionMessage))
			{
				throw new InvalidOperationException(exceptionMessage);
			}

			DateTime now = DateTime.Now;

			try
			{
				CashTransferOperation newCashTransferOperation = new CashTransferOperation
				{
					SubdivisionFrom = CashSubdivisionFrom,
					SubdivisionTo = CashSubdivisionTo,
					TransferedSum = TransferedSum,
					SendTime = now
				};

				Expense newExpenseOperation = new Expense
				{
					TypeDocument = ExpenseInvoiceDocumentType.ExpenseTransferDocument,
					Description = description,
					Employee = sender,
					Casher = sender,
					Date = now,
					ExpenseCategoryId = ExpenseCategoryId,
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
			}
			catch(Exception)
			{
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
		/// <param name="description">Комментарий основание</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public virtual void Receive(Employee receiver, string description)
		{
			if(receiver == null)
			{
				throw new ArgumentNullException($"Не указано кто является кассиром");
			}

			if(CashTransferOperation == null)
			{
				throw new InvalidOperationException($"Не было создано операции перемещения денежных средств, без нее невозможно создать операцию принятия");
			}

			if(Status != CashTransferDocumentStatuses.Sent)
			{
				throw new InvalidOperationException($"Невозможно принять документ транспортировки денег не из статуса \"{CashTransferDocumentStatuses.Sent.GetEnumTitle()}\"");
			}

			if(IncomeOperation != null || CashTransferOperation.ReceiveTime.HasValue)
			{
				throw new InvalidOperationException($"Денежные средства уже были приняты ранее, изменить данные о факте принятия денег невозможно");
			}

			string exceptionMessage = RaiseValidationAndGetResult();
			if(!string.IsNullOrWhiteSpace(exceptionMessage))
			{
				throw new InvalidOperationException(exceptionMessage);
			}

			DateTime now = DateTime.Now;

			try
			{
				Income newIncomeOperation = new Income
				{
					TypeDocument = IncomeInvoiceDocumentType.IncomeTransferDocument,
					Employee = receiver,
					Casher = receiver,
					Description = description,
					Date = now,
					IncomeCategoryId = IncomeCategoryId,
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
			}
			catch(Exception)
			{
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

			if(validationResult.Any())
			{
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
			if(Driver == null)
			{
				yield return new ValidationResult("Должен быть заполнен водитель", new[] { nameof(Driver) });
			}

			if(Car == null)
			{
				yield return new ValidationResult("Должен быть заполнен автомобиль", new[] { nameof(Car) });
			}

			if(CashSubdivisionFrom == null)
			{
				yield return new ValidationResult("Должна быть заполнена касса из которой переносятся деньги", new[] { nameof(CashSubdivisionFrom) });
			}
			else if(CashSubdivisionFrom.Id == 0)
			{
				yield return new ValidationResult("Должна быть выбрана существующая касса", new[] { nameof(CashSubdivisionFrom) });
			}

			if(CashSubdivisionTo == null)
			{
				yield return new ValidationResult("Должна быть заполнена касса в которую переносятся деньги", new[] { nameof(CashSubdivisionTo) });
			}
			else if(CashSubdivisionTo.Id == 0)
			{
				yield return new ValidationResult("Должна быть выбрана существующая касса", new[] { nameof(CashSubdivisionTo) });
			}

			if(CashSubdivisionFrom != null && CashSubdivisionTo != null && CashSubdivisionFrom.Id == CashSubdivisionTo.Id)
			{
				yield return new ValidationResult("Невозможно перенести деньги в ту же самую кассу");
			}

			if(IncomeCategoryId == null)
			{
				yield return new ValidationResult("Должна быть выбрана статья прихода", new[] { nameof(IncomeCategoryId) });
			}

			if(ExpenseCategoryId == null)
			{
				yield return new ValidationResult("Должна быть выбрана статья расхода", new[] { nameof(ExpenseCategoryId) });
			}

			if(CalculateTransferedSum() <= 0)
			{
				yield return new ValidationResult("Сумма денежных средств для перемещения должна быть больше нуля");
			}

			if(!string.IsNullOrWhiteSpace(Comment) && Comment.Length > _commentLimit)
			{
				yield return new ValidationResult($"Длина комментария превышена на {Comment.Length - _commentLimit}");
			}
		}
	}
}
