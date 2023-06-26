using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
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
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.Domain.Cash
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "приходные одера",
		Nominative = "приходный ордер",
		Accusative = "приходный ордер",
		Genitive = "приходного ордера")]
	[EntityPermission]
	[HistoryTrace]
	public class Income : PropertyChangedBase, IDomainObject, IValidatableObject, ISubdivisionEntity
	{
		private DateTime _date;
		private Subdivision _relatedToSubdivision;
		private IncomeInvoiceDocumentType _typeDocument;
		private IncomeType _typeOperation;
		private Employee _casher;
		private Employee _employee;
		private Counterparty _customer;
		private Order _order;
		private int? _incomeCategoryId;
		private int? _expenseCategoryId;
		private string _description;
		private decimal _money;
		private RouteList _routeListClosing;
		private IncomeCashTransferedItem _transferedBy;
		private CashTransferDocumentBase _cashTransferDocument;
		private string _cashierReviewComment;
		private Organization _organisation;

		public Income() { }

		public virtual int Id { get; set; }

		[Display(Name = "Дата")]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		[Display(Name = "Относится к подразделению")]
		public virtual Subdivision RelatedToSubdivision
		{
			get => _relatedToSubdivision;
			set => SetField(ref _relatedToSubdivision, value);
		}

		[Display(Name = "Тип документа")]
		public virtual IncomeInvoiceDocumentType TypeDocument
		{
			get => _typeDocument;
			set => SetField(ref _typeDocument, value);
		}

		[Display(Name = "Тип операции")]
		public virtual IncomeType TypeOperation
		{
			get => _typeOperation;
			set
			{
				if(SetField(ref _typeOperation, value))
				{
					switch(TypeOperation)
					{
						case IncomeType.Return:
							IncomeCategoryId = null;
							Customer = null;
							break;
						case IncomeType.Common:
							IncomeCategoryId = null;
							Customer = null;
							break;
						case IncomeType.Payment:
							IncomeCategoryId = null;
							break;
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

		[Display(Name = "Клиент")]
		public virtual Counterparty Customer
		{
			get => _customer;
			set => SetField(ref _customer, value);
		}

		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Статья дохода")]
		[HistoryIdentifier(TargetType = typeof(FinancialIncomeCategory))]
		public virtual int? IncomeCategoryId
		{
			get => _incomeCategoryId;
			set => SetField(ref _incomeCategoryId, value);
		}

		/// <summary>
		/// Используется только для отслеживания возвратных возвратных денег с типом операции Return
		/// </summary>
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
			set
			=> SetField(ref _money, value);
		}

		[PropertyChangedAlso(
			nameof(Employee),
			nameof(Description))]
		public virtual RouteList RouteListClosing
		{
			get => _routeListClosing;
			set
			{
				if(SetField(ref _routeListClosing, value))
				{
					if(value is null)
					{
						Description = "";
						Employee = default;
					}
					else
					{
						Description = $"Приход по МЛ №{RouteListClosing.Id} от {RouteListClosing.Date:d}";
						Employee = RouteListClosing.Driver;
					}
				}
			}
		}

		[Display(Name = "Перемещен")]
		public virtual IncomeCashTransferedItem TransferedBy
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

		[Display(Name = "Комментарий по закрытию кассы")]
		public virtual string CashierReviewComment
		{
			get => _cashierReviewComment;
			set => SetField(ref _cashierReviewComment, value);
		}

		[Display(Name = "Организация")]
		public virtual Organization Organisation
		{
			get => _organisation;
			set => SetField(ref _organisation, value);
		}

		public virtual string Title => $"Приходный ордер №{Id} от {Date:d}";

		#region RunTimeOnly

		public virtual List<Expense> AdvanceForClosing { get; protected set; }

		public virtual bool NoFullCloseMode { get; set; }

		#endregion

		#region Функции

		public virtual void AcceptSelfDeliveryPaid(ICallTaskWorker callTaskWorker)
		{
			if(Id == 0)
			{
				Order.AcceptSelfDeliveryIncomeCash(Money, callTaskWorker);
			}
			else
			{
				Order.AcceptSelfDeliveryIncomeCash(Money, callTaskWorker, Id);
			}
		}

		public virtual void PrepareCloseAdvance(List<Expense> advances)
		{
			if(TypeOperation != IncomeType.Return)
			{
				throw new InvalidOperationException("Метод PrepareCloseAdvance() можно вызываться только для возврата аванса.");
			}

			AdvanceForClosing = advances;
		}

		public virtual void CloseAdvances(IUnitOfWork uow)
		{
			if(TypeOperation != IncomeType.Return)
			{
				throw new InvalidOperationException($"Приходный ордер должен иметь тип '{AttributeUtil.GetEnumTitle(IncomeType.Return)}'");
			}

			if(NoFullCloseMode)
			{
				var advance = AdvanceForClosing.First();
				advance.AddAdvanceCloseItem(this, Money);
				uow.Save(advance);
			}
			else
			{
				foreach(var advance in AdvanceForClosing)
				{
					advance.AddAdvanceCloseItem(this, advance.Money - advance.ClosedMoney);
					uow.Save(advance);
				}
			}
		}

		public virtual void FillFromOrder(IUnitOfWork uow, ICashRepository cashRepository)
		{
			if(Id == 0)
			{
				var existsIncome = cashRepository.GetIncomePaidSumForOrder(uow, Order.Id);
				decimal orderCash = 0m;

				if(Order.PaymentType == PaymentType.Cash)
				{
					orderCash = Order.OrderSum;
				}

				var result = orderCash - existsIncome;
				Money = result < 0 ? 0 : result;

				Description = $"Самовывоз №{Order.Id} от {Order.DeliveryDate}";
			}
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(validationContext.Items.ContainsKey("IsSelfDelivery") && (bool)validationContext.Items["IsSelfDelivery"])
			{
				if(TypeDocument != IncomeInvoiceDocumentType.IncomeInvoiceSelfDelivery)
				{
					yield return new ValidationResult($"Тип документа должен быть {IncomeInvoiceDocumentType.IncomeInvoiceSelfDelivery.GetEnumTitle()}.",
					new[] { this.GetPropertyName(o => o.TypeDocument) });
				}

				if(TypeOperation != IncomeType.Payment)
				{
					yield return new ValidationResult($"Тип операции должен быть {IncomeType.Payment.GetEnumTitle()}.",
					new[] { this.GetPropertyName(o => o.TypeOperation) });
				}

				var financialCategoriesRepository = validationContext.GetService<IFinancialIncomeCategoriesRepository>();

				var unitOfWork = validationContext.GetService<IUnitOfWork>();

				var targetDocument = financialCategoriesRepository.GetIncomeCategoryTargetDocument(unitOfWork, IncomeCategoryId);

				if(IncomeCategoryId == null || targetDocument != TargetDocument.SelfDelivery)
				{
					yield return new ValidationResult("Должна быть выбрана статья дохода для самовывоза.",
					new[] { this.GetPropertyName(o => o.IncomeCategoryId) });
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

					if(Order.OrderPositiveSum < Money)
					{
						yield return new ValidationResult("Сумма к оплате не может быть больше чем сумма в заказе");
					}
				}
			}
			else
			{
				if(TypeOperation == IncomeType.Return)
				{
					if(Employee == null)
					{
						yield return new ValidationResult("Подотчетное лицо должно быть указано.",
							new[] { this.GetPropertyName(o => o.Employee) });
					}

					if(ExpenseCategoryId == null)
					{
						yield return new ValidationResult("Статья по которой брались деньги должна быть указана.",
							new[] { this.GetPropertyName(o => o.ExpenseCategoryId) });
					}

					if(Id == 0)
					{
						if(Organisation == null)
						{
							yield return new ValidationResult("Организация должна быть заполнена",
								new[] { nameof(Organisation) });
						}

						if(AdvanceForClosing == null || AdvanceForClosing.Count == 0)
						{
							yield return new ValidationResult("Не указаны авансы которые должны быть закрыты этим возвратом в кассу.",
								new[] { this.GetPropertyName(o => o.AdvanceForClosing) });
						}
						else
						{
							if(NoFullCloseMode)
							{
								var advance = AdvanceForClosing.First();

								if(Money > advance.UnclosedMoney)
								{
									yield return new ValidationResult("Сумма возврата не должна превышать сумму которую брал человек за вычетом уже возвращенных средств.",
										new[] { this.GetPropertyName(o => o.AdvanceForClosing) });
								}
							}
							else
							{
								decimal closedSum = AdvanceForClosing.Sum(x => x.UnclosedMoney);

								if(closedSum != Money)
								{
									throw new InvalidOperationException("Сумма закрытых авансов должна соответствовать сумме возврата.");
								}
							}
						}
					}
				}

				if(TypeOperation != IncomeType.Return)
				{
					if(IncomeCategoryId == null)
					{
						yield return new ValidationResult("Статья дохода должна быть указана.",
							new[] { this.GetPropertyName(o => o.IncomeCategoryId) });
					}
				}

				if(TypeOperation == IncomeType.Payment)
				{
					if(Customer == null)
					{
						yield return new ValidationResult("Клиент должен быть указан.",
							new[] { this.GetPropertyName(o => o.Customer) });
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
