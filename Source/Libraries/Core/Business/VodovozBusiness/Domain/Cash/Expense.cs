using DateTimeHelpers;
using FluentNHibernate.Data;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Services;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Settings.Cash;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.Domain.Cash
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "расходные одера",
		Nominative = "расходный ордер",
		Accusative = "расходный ордер",
		Genitive = "расходного ордера")]
	[EntityPermission]
	[HistoryTrace]
	public class Expense : PropertyChangedBase, IDomainObject, IValidatableObject, ISubdivisionEntity
	{
		private DateTime _date;
		private DateTime _ddrDate;
		private Subdivision _relatedToSubdivision;
		private ExpenseInvoiceDocumentType _typeDocument;
		private ExpenseType _typeOperation;
		private Employee _casher;
		private Employee _employee;
		private Order _order;
		private int? _expenseCategoryId;
		private string _description;
		private decimal _money;
		private bool? _advanceClosed;
		private IList<AdvanceClosing> _advanceCloseItems;
		private RouteList _routeListClosing;
		private WagesMovementOperations _wagesOperation;
		private ExpenseCashTransferedItem _transferedBy;
		private CashTransferDocumentBase _cashTransferDocument;
		private Organization _organisation;
		private CashRequestSumItem _cashRequestSumItem;

		public Expense() { }

		public virtual int Id { get; set; }

		[Display(Name = "Дата")]
		public virtual DateTime Date
		{
			get => _date;
			set
			{
				if(SetField(ref _date, value))
				{
					if(DdrDate < Date)
					{
						DdrDate = Date;
					}
				}
			}
		}

		[Display(Name = "Дата учета ДДР")]
		public virtual DateTime DdrDate
		{
			get => _ddrDate;
			set => SetField(ref _ddrDate, value);
		}

		[Display(Name = "Относится к подразделению")]
		public virtual Subdivision RelatedToSubdivision
		{
			get => _relatedToSubdivision;
			set => SetField(ref _relatedToSubdivision, value);
		}

		[Display(Name = "Тип документа")]
		public virtual ExpenseInvoiceDocumentType TypeDocument
		{
			get => _typeDocument;
			set => SetField(ref _typeDocument, value);
		}

		[Display(Name = "Тип операции")]
		public virtual ExpenseType TypeOperation
		{
			get => _typeOperation;
			set
			{
				if(SetField(ref _typeOperation, value))
				{
					if(TypeOperation == ExpenseType.Advance && AdvanceClosed == null)
					{
						AdvanceClosed = false;
					}

					if(TypeOperation != ExpenseType.Advance && AdvanceClosed.HasValue)
					{
						AdvanceClosed = null;
					}

					if(TypeOperation == ExpenseType.Salary
						|| TypeOperation == ExpenseType.EmployeeAdvance)
					{
						Organisation = null;
					}
				}
			}
		}

		[Display(Name = "Кассир")]
		public virtual Employee Casher
		{
			get => _casher;
			set => SetField(ref _casher, value);
		}

		[Display(Name = "Сотрудник")]
		public virtual Employee Employee
		{
			get => _employee;
			set => SetField(ref _employee, value);
		}

		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Статья расхода")]
		[HistoryIdentifier(TargetType = typeof(FinancialExpenseCategory))]
		public virtual int? ExpenseCategoryId
		{
			get => _expenseCategoryId;
			set => SetField(ref _expenseCategoryId, value);
		}

		[Display(Name = "Основание")]
		public virtual string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}

		[Display(Name = "Сумма")]
		public virtual decimal Money
		{
			get => _money;
			set => SetField(ref _money, value);
		}

		[Display(Name = "Аванс закрыт")]
		public virtual bool? AdvanceClosed
		{
			get => _advanceClosed;
			set => SetField(ref _advanceClosed, value);
		}

		[Display(Name = "Документы закрытия аванса")]
		public virtual IList<AdvanceClosing> AdvanceCloseItems
		{
			get => _advanceCloseItems;
			set => SetField(ref _advanceCloseItems, value);
		}

		public virtual RouteList RouteListClosing
		{
			get => _routeListClosing;
			set => SetField(ref _routeListClosing, value);
		}

		[Display(Name = "Операция с зарплатой")]
		public virtual WagesMovementOperations WagesOperation
		{
			get => _wagesOperation;
			set => SetField(ref _wagesOperation, value);
		}

		[Display(Name = "Перемещен")]
		public virtual ExpenseCashTransferedItem TransferedBy
		{
			get => _transferedBy;
			set => SetField(ref _transferedBy, value);
		}

		[Display(Name = "Документ перемещения")]
		public virtual CashTransferDocumentBase CashTransferDocument
		{
			get => _cashTransferDocument;
			set => SetField(ref _cashTransferDocument, value);
		}

		[Display(Name = "Организация")]
		public virtual Organization Organisation
		{
			get => _organisation;
			set => SetField(ref _organisation, value);
		}

		[Display(Name = "Сумма заявки на выдачу ДС")]
		public virtual CashRequestSumItem CashRequestSumItem
		{
			get => _cashRequestSumItem;
			set => SetField(ref _cashRequestSumItem, value);
		}

		public virtual string Title => $"Расходный ордер №{Id} от {Date:d}";

		public virtual decimal ClosedMoney => AdvanceCloseItems == null ? 0 : AdvanceCloseItems.Sum(x => x.Money);

		public virtual decimal UnclosedMoney => Money - ClosedMoney;

		#region Функции

		public virtual void CalculateCloseState()
		{
			if(TypeOperation != ExpenseType.Advance)
			{
				throw new InvalidOperationException("Метод CalculateCloseState() можно вызываться только для выдачи аванса.");
			}

			if(AdvanceCloseItems == null)
			{
				AdvanceClosed = false;
				return;
			}

			AdvanceClosed = ClosedMoney == Money;
		}

		public virtual AdvanceClosing AddAdvanceCloseItem(Income income, decimal sum)
		{
			if(TypeOperation != ExpenseType.Advance)
			{
				throw new InvalidOperationException("Метод AddAdvanceCloseItem() может вызываться только для выдачи аванса.");
			}

			var closing = new AdvanceClosing(this, income, sum);

			if(AdvanceCloseItems == null)
			{
				AdvanceCloseItems = new List<AdvanceClosing>();
			}

			AdvanceCloseItems.Add(closing);
			CalculateCloseState();
			return closing;
		}

		public virtual AdvanceClosing AddAdvanceCloseItem(AdvanceReport report, decimal sum)
		{
			if(TypeOperation != ExpenseType.Advance)
			{
				throw new InvalidOperationException("Метод AddAdvanceCloseItem() может вызываться только для выдачи аванса.");
			}

			var closing = new AdvanceClosing(this, report, sum);

			if(AdvanceCloseItems == null)
			{
				AdvanceCloseItems = new List<AdvanceClosing>();
			}

			AdvanceCloseItems.Add(closing);
			CalculateCloseState();
			return closing;
		}

		public virtual void UpdateWagesOperations(IUnitOfWork uow)
		{
			if(TypeOperation == ExpenseType.EmployeeAdvance || TypeOperation == ExpenseType.Salary)
			{
				WagesType operationType = WagesType.GivedAdvance;
				switch(TypeOperation)
				{
					case ExpenseType.EmployeeAdvance:
						operationType = WagesType.GivedAdvance;
						break;
					case ExpenseType.Salary:
						operationType = WagesType.GivedWage;
						break;
				}
				if(WagesOperation == null)
				{
					//Умножаем на -1, так как операция выдачи
					WagesOperation = new WagesMovementOperations
					{
						OperationType = operationType,
						Employee = this.Employee,
						Money = this.Money * (-1),
						OperationTime = DateTime.Now
					};
				}
				else
				{
					WagesOperation.OperationType = operationType;
					WagesOperation.Employee = this.Employee;
					WagesOperation.Money = this.Money * (-1);
				}
				uow.Save(WagesOperation);
			}
			else
			{
				if(WagesOperation != null)
				{
					uow.Delete(WagesOperation);
				}
			}
		}

		public virtual void FillFromOrder(IUnitOfWork uow, ICashRepository cashRepository)
		{
			var existsExpense = cashRepository.GetExpenseReturnSumForOrder(uow, Order.Id);
			if(Id == 0)
			{
				decimal orderCash = 0m;
				if(Order.PaymentType == PaymentType.Cash)
				{
					orderCash = Math.Abs(Order.OrderNegativeSum);
				}
				var result = orderCash - existsExpense;
				Money = result < 0 ? 0 : result;

				Description = $"Возврат по самовывозу №{Order.Id} от {Order.DeliveryDate}";
			}
		}

		public virtual void AcceptSelfDeliveryPaid(CallTaskWorker callTaskWorker)
		{
			if(Id == 0)
			{
				Order.AcceptSelfDeliveryExpenseCash(Money, callTaskWorker);
			}
			else
			{
				Order.AcceptSelfDeliveryExpenseCash(Money, callTaskWorker, Id);
			}
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var dateTimeLowerBorder = DateTimeExtensions.Max(Date, DdrDate.FirstDayOfMonth()).Date;

			if(DdrDate < dateTimeLowerBorder)
			{
				yield return new ValidationResult($"Некорректная дата учета ДДР {DdrDate:dd.MM.yyyy}, значение должно быть больше {dateTimeLowerBorder:dd.MM.yyyy}", new[] { nameof(DdrDate) });
			}

			if(validationContext.Items.ContainsKey("IsSelfDelivery") && (bool)validationContext.Items["IsSelfDelivery"])
			{
				if(TypeDocument != ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery)
				{
					yield return new ValidationResult($"Тип документа должен быть {ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery.GetEnumTitle()}.",
					new[] { this.GetPropertyName(o => o.TypeDocument) });
				}

				if(TypeOperation != ExpenseType.ExpenseSelfDelivery)
				{
					yield return new ValidationResult($"Тип операции должен быть {ExpenseType.ExpenseSelfDelivery.GetEnumTitle()}.",
					new[] { this.GetPropertyName(o => o.TypeOperation) });
				}

				var financialCategoriesRepository = validationContext.GetService<IFinancialExpenseCategoriesRepository>();

				var unitOfWork = validationContext.GetService<IUnitOfWork>();

				var targetDocument = financialCategoriesRepository.GetExpenseCategoryTargetDocument(unitOfWork, ExpenseCategoryId);

				if(ExpenseCategoryId == null || targetDocument != TargetDocument.SelfDelivery)
				{
					yield return new ValidationResult("Должна быть выбрана статья расхода для самовывоза.",
					new[] { this.GetPropertyName(o => o.ExpenseCategoryId) });
				}

				if(Order == null)
				{
					yield return new ValidationResult("Должен быть выбран заказ.",
					new[] { this.GetPropertyName(o => o.Order) });
				}
				else
				{
					if(Order.PaymentType != PaymentType.Cash)
					{
						yield return new ValidationResult("Должен быть выбран наличный заказ");
					}
					if(!Order.SelfDelivery)
					{
						yield return new ValidationResult("Должен быть выбран заказ с самовывозом");
					}
					if(Math.Abs(Order.OrderNegativeSum) < Money)
					{
						yield return new ValidationResult("Сумма к возврату не может быть больше чем сумма в заказе");
					}
				}
			}
			else
			{
				if(TypeOperation == ExpenseType.Advance)
				{
					if(Employee == null)
					{
						yield return new ValidationResult("Подотчетное лицо должно быть указано.",
							new[] { this.GetPropertyName(o => o.Employee) });
					}

					if(ExpenseCategoryId == null)
					{
						yield return new ValidationResult("Статья расхода под которую выдаются деньги должна быть заполнена.",
							new[] { this.GetPropertyName(o => o.ExpenseCategoryId) });
					}

					if(!AdvanceClosed.HasValue)
					{
						yield return new ValidationResult("Отсутствует иформация поле Закрытия аванса. Поле не может быть null.",
							new[] { this.GetPropertyName(o => o.AdvanceClosed) });
					}
				}
				else
				{
					if(AdvanceClosed.HasValue)
					{
						yield return new ValidationResult($"Если это не выдача под аванс {this.GetPropertyName(o => o.AdvanceClosed)} должно быть null.",
							new[] { this.GetPropertyName(o => o.AdvanceClosed) });
					}
				}

				if(TypeOperation == ExpenseType.Expense)
				{
					if(ExpenseCategoryId == null)
					{
						yield return new ValidationResult("Статья расхода должна быть указана.",
							new[] { nameof(ExpenseCategoryId) });
					}

					if(ExpenseCategoryId != null
						&& validationContext.Items.ContainsKey(nameof(IFinancialCategoriesGroupsSettings.RouteListClosingFinancialExpenseCategoryId))
						&& ExpenseCategoryId == (int)validationContext.Items[nameof(IFinancialCategoriesGroupsSettings.RouteListClosingFinancialExpenseCategoryId)]
						&& RouteListClosing is null)
					{
						yield return new ValidationResult(
							"Для данной статьи расхода должен быть указан МЛ",
							new[] { nameof(RouteListClosing) });
					}
				}

				if(TypeOperation != ExpenseType.Salary && TypeOperation != ExpenseType.EmployeeAdvance)
				{
					if(Id == 0 && Organisation == null)
					{
						yield return new ValidationResult("Организация должна быть заполнена",
							new[] { nameof(Organisation) });
					}
				}
			}

			if(RelatedToSubdivision == null)
			{
				yield return new ValidationResult("Должно быть выбрано подразделение",
					new[] { this.GetPropertyName(o => o.RelatedToSubdivision) });
			}

			if(Money <= 0)
			{
				yield return new ValidationResult("Сумма должна больше нуля",
					new[] { this.GetPropertyName(o => o.Money) });
			}

			if(string.IsNullOrWhiteSpace(Description))
			{
				yield return new ValidationResult("Основание должно быть заполнено.",
					new[] { this.GetPropertyName(o => o.Description) });
			}
		}

		#endregion
	}
}
