using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Permissions;
using Vodovoz.Repository.Cash;

namespace Vodovoz.Domain.Cash
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "расходные одера",
		Nominative = "расходный ордер")]
	[EntityPermission]
	[HistoryTrace]
	public class Expense : PropertyChangedBase, IDomainObject, IValidatableObject, ISubdivisionEntity
	{
		#region Свойства

		public virtual int Id { get; set; }

		private DateTime date;

		[Display (Name = "Дата")]
		public virtual DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}

		private Subdivision relatedToSubdivision;

		[Display(Name = "Относится к подразделению")]
		public virtual Subdivision RelatedToSubdivision {
			get { return relatedToSubdivision; }
			set { SetField(ref relatedToSubdivision, value, () => RelatedToSubdivision); }
		}

		private ExpenseInvoiceDocumentType typeDocument;

		[Display(Name = "Тип документа")]
		public virtual ExpenseInvoiceDocumentType TypeDocument {
			get { return typeDocument; }
			set { SetField(ref typeDocument, value, () => TypeDocument); }
		}

		private ExpenseType typeOperation;

		[Display (Name = "Тип операции")]
		public virtual ExpenseType TypeOperation {
			get { return typeOperation; }
			set {
				if(SetField (ref typeOperation, value, () => TypeOperation))
				{
					if (TypeOperation == ExpenseType.Advance && AdvanceClosed == null)
						AdvanceClosed = false;
					if (TypeOperation != ExpenseType.Advance && AdvanceClosed.HasValue)
						AdvanceClosed = null;
				}
			}
		}

		Employee casher;

		[Display (Name = "Кассир")]
		public virtual Employee Casher {
			get { return casher; }
			set { SetField (ref casher, value, () => Casher); }
		}

		Employee employee;

		[Display (Name = "Сотрудник")]
		public virtual Employee Employee {
			get { return employee; }
			set { SetField (ref employee, value, () => Employee); }
		}

		Order order;

		[Display(Name = "Заказ")]
		public virtual Order Order {
			get { return order; }
			set { SetField(ref order, value, () => Order); }
		}

		ExpenseCategory expenseCategory;

		[Display (Name = "Статья расхода")]
		public virtual ExpenseCategory ExpenseCategory {
			get { return expenseCategory; }
			set { SetField (ref expenseCategory, value, () => ExpenseCategory); }
		}

		string description;

		[Display (Name = "Основание")]
		public virtual string Description {
			get { return description; }
			set { SetField (ref description, value, () => Description); }
		}

		decimal money;

		[Display (Name = "Сумма")]
		public virtual decimal Money {
			get { return money; }
			set {
				SetField (ref money, value, () => Money); 
			}
		}

		bool? advanceClosed;

		[Display (Name = "Аванс закрыт")]
		public virtual bool? AdvanceClosed {
			get { return advanceClosed; }
			set { SetField (ref advanceClosed, value, () => AdvanceClosed); }
		}

		IList<AdvanceClosing> advanceCloseItems;

		[Display (Name = "Документы закрытия аванса")]
		public virtual IList<AdvanceClosing> AdvanceCloseItems {
			get { return advanceCloseItems; }
			set { SetField (ref advanceCloseItems, value, () => AdvanceCloseItems); }
		}

		RouteList routeListClosing;

		public virtual RouteList RouteListClosing
		{
			get{ return routeListClosing; }
			set
			{
				SetField(ref routeListClosing, value, () => RouteListClosing);
			}
		}

		private WagesMovementOperations wagesOperation;

		[Display(Name = "Операция с зарплатой")]
		public virtual WagesMovementOperations WagesOperation
		{
			get { return wagesOperation; }
			set { SetField(ref wagesOperation, value, () => WagesOperation); }
		}

		private ExpenseCashTransferedItem transferedBy;
		[Display(Name = "Перемещен")]
		public virtual ExpenseCashTransferedItem TransferedBy {
			get => transferedBy;
			set => SetField(ref transferedBy, value, () => TransferedBy);
		}

		private CashTransferDocumentBase cashTransferDocument;
		[Display(Name = "Документ перемещения")]
		public virtual CashTransferDocumentBase CashTransferDocument {
			get => cashTransferDocument;
			set => SetField(ref cashTransferDocument, value, () => CashTransferDocument);
		}

		#endregion

		#region Вычисляемые

		public virtual string Title => String.Format("Расходный ордер №{0} от {1:d}", Id, Date);

		public virtual decimal ClosedMoney => AdvanceCloseItems == null ? 0 : AdvanceCloseItems.Sum(x => x.Money);

		public virtual decimal UnclosedMoney => Money - ClosedMoney;

		#endregion

		#region Функции

		public virtual void CalculateCloseState()
		{
			if (TypeOperation != ExpenseType.Advance)
				throw new InvalidOperationException("Метод CalculateCloseState() можно вызываться только для выдачи аванса.");

			if (AdvanceCloseItems == null)
			{
				AdvanceClosed = false;
				return;
			}

			AdvanceClosed = ClosedMoney == Money;
		}

		public virtual AdvanceClosing AddAdvanceCloseItem(Income income, decimal sum)
		{
			if (TypeOperation != ExpenseType.Advance)
				throw new InvalidOperationException("Метод AddAdvanceCloseItem() можно вызываться только для выдачи аванса.");
			
			var closing = new AdvanceClosing(this, income, sum);
			if (AdvanceCloseItems == null)
				AdvanceCloseItems = new List<AdvanceClosing>();
			AdvanceCloseItems.Add(closing);
			CalculateCloseState();
			return closing;
		}

		public virtual AdvanceClosing AddAdvanceCloseItem(AdvanceReport report, decimal sum)
		{
			if (TypeOperation != ExpenseType.Advance)
				throw new InvalidOperationException("Метод AddAdvanceCloseItem() можно вызываться только для выдачи аванса.");

			var closing = new AdvanceClosing(this, report, sum);
			if (AdvanceCloseItems == null)
				AdvanceCloseItems = new List<AdvanceClosing>();
			AdvanceCloseItems.Add(closing);
			CalculateCloseState();
			return closing;
		}

		public virtual void UpdateWagesOperations (IUnitOfWork uow)
		{
			if (TypeOperation == ExpenseType.EmployeeAdvance || TypeOperation == ExpenseType.Salary)
			{
				WagesType operationType = WagesType.GivedAdvance;
				switch (TypeOperation)
				{
					case ExpenseType.EmployeeAdvance:
						operationType = WagesType.GivedAdvance;
						break;
					case ExpenseType.Salary:
						operationType = WagesType.GivedWage;
						break;
				}
				if (WagesOperation == null)
				{
					//Умножаем на -1, так как операция выдачи
					WagesOperation = new WagesMovementOperations
					{
						OperationType = operationType,
						Employee 	  = this.Employee,
						Money 		  = this.Money * (-1),
						OperationTime = DateTime.Now
					};
				} else {
					WagesOperation.OperationType = operationType;
					WagesOperation.Employee 	 = this.Employee;
					WagesOperation.Money 		 = this.Money * (-1);
				}
				uow.Save(WagesOperation);
			} else {
				if (WagesOperation != null)
				{
					uow.Delete(WagesOperation);
				}
			}
		}

		public virtual void FillFromOrder(IUnitOfWork uow)
		{
			var existsExpense = CashRepository.GetExpenseReturnSumForOrder(uow, Order.Id);
			if(Id == 0) {
				decimal orderCash = 0m;
				if(Order.PaymentType == PaymentType.cash || Order.PaymentType == PaymentType.BeveragesWorld) {
					orderCash = Math.Abs(Order.OrderSumReturn) + (Order.ExtraMoney < 0 ? Math.Abs(Order.ExtraMoney) : 0);
				}
				var result = orderCash - existsExpense;
				Money = result < 0 ? 0 : result;

				Description = $"Возврат по самовывозу №{Order.Id} от {Order.DeliveryDate}";
			}
		}

		public virtual void AcceptSelfDeliveryPaid()
		{
			if(Id == 0) {
				Order.AcceptSelfDeliveryExpenseCash(Money);
			} else {
				Order.AcceptSelfDeliveryExpenseCash(Money, Id);
			}
		}

		#endregion

		public Expense() { }

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(validationContext.Items.ContainsKey("IsSelfDelivery") && (bool)validationContext.Items["IsSelfDelivery"]) {
				if(TypeDocument != ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery) {
					yield return new ValidationResult($"Тип документа должен быть { ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery.GetEnumTitle() }.",
					new[] { this.GetPropertyName(o => o.TypeDocument) });
				}
				if(TypeOperation != ExpenseType.ExpenseSelfDelivery) {
					yield return new ValidationResult($"Тип операции должен быть { ExpenseType.ExpenseSelfDelivery.GetEnumTitle() }.",
					new[] { this.GetPropertyName(o => o.TypeOperation) });
				}
				if(ExpenseCategory == null || ExpenseCategory.ExpenseDocumentType != ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery) {
					yield return new ValidationResult("Должна быть выбрана статья расхода для самовывоза.",
					new[] { this.GetPropertyName(o => o.ExpenseCategory) });
				}
				if(Order == null) {
					yield return new ValidationResult("Должен быть выбран заказ.",
					new[] { this.GetPropertyName(o => o.Order) });
				} else {
					if(Order.PaymentType != PaymentType.cash && Order.PaymentType != PaymentType.BeveragesWorld) {
						yield return new ValidationResult("Должен быть выбран наличный заказ");
					}
					if(!Order.SelfDelivery) {
						yield return new ValidationResult("Должен быть выбран заказ с самовывозом");
					}
					if(Math.Abs(Order.OrderSumReturnTotal) < Money) {
						yield return new ValidationResult("Сумма к возврату не может быть больше чем сумма в заказе");
					}
				}
			} else {
				if(TypeOperation == ExpenseType.Advance) {
					if(Employee == null)
						yield return new ValidationResult("Подотчетное лицо должно быть указано.",
							new[] { this.GetPropertyName(o => o.Employee) });
					if(ExpenseCategory == null)
						yield return new ValidationResult("Статья расхода под которую выдаются деньги должна быть заполнена.",
							new[] { this.GetPropertyName(o => o.ExpenseCategory) });

					if(!AdvanceClosed.HasValue)
						yield return new ValidationResult("Отсутствует иформация поле Закрытия аванса. Поле не может быть null.",
							new[] { this.GetPropertyName(o => o.AdvanceClosed) });

				} else {
					if(AdvanceClosed.HasValue)
						yield return new ValidationResult(String.Format("Если это не выдача под аванс {0} должно быть null.", this.GetPropertyName(o => o.AdvanceClosed)),
							new[] { this.GetPropertyName(o => o.AdvanceClosed) });
				}

				if(TypeOperation == ExpenseType.Expense) {
					if(ExpenseCategory == null)
						yield return new ValidationResult("Статья расхода должна быть указана.",
							new[] { this.GetPropertyName(o => o.ExpenseCategory) });
				}
			}

			if(RelatedToSubdivision == null) {
				yield return new ValidationResult("Должно быть выбрано подразделение",
					new[] { this.GetPropertyName(o => o.RelatedToSubdivision) });
			}

			if(Money <= 0)
				yield return new ValidationResult ("Сумма должна больше нуля",
					new[] { this.GetPropertyName (o => o.Money) });

			if(String.IsNullOrWhiteSpace (Description))
				yield return new ValidationResult ("Основание должно быть заполнено.",
					new[] { this.GetPropertyName (o => o.Description) });
								
		}

		#endregion
	}

	public enum ExpenseType
	{
		[Display (Name = "Прочий расход")]
		Expense,
		[Display(Name = "Возврат по самовывозу")]
		ExpenseSelfDelivery,
		[Display (Name = "Аванс подотчетному лицу")]
		Advance,
		[Display (Name = "Аванс сотруднику")]
		EmployeeAdvance,
		[Display (Name = "Выдача зарплаты")]
		Salary
	}

	public class ExpenseTypeStringType : NHibernate.Type.EnumStringType
	{
		public ExpenseTypeStringType () : base (typeof(ExpenseType))
		{
		}
	}

	public enum ExpenseInvoiceDocumentType
	{
		[Display(Name = "Расходный ордер")]
		ExpenseInvoice,
		[Display(Name = "Расходный ордер для документа перемещения ДС")]
		ExpenseTransferDocument,
		[Display(Name = "Расходный ордер для самовывоза")]
		ExpenseInvoiceSelfDelivery,
	}

	public class ExpenseInvoiceDocumentTypeStringType : NHibernate.Type.EnumStringType
	{
		public ExpenseInvoiceDocumentTypeStringType() : base(typeof(ExpenseInvoiceDocumentType))
		{
		}
	}

}

