using Autofac;
using Gamma.Utilities;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Journal.Search;
using QS.Project.Search;
using QS.Services;
using QS.Utilities.Enums;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Settings.Delivery;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;
using VodovozBusiness.Domain.Payments;
using VodOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class ManualCashlessIncomeMatchingViewModel : EntityTabViewModelBase<CashlessIncome>
	{
		private const string _error = "Ошибка";
		private decimal _allocatedSum;
		private decimal _currentBalance;
		private decimal _counterpartyTotalDebt;
		private decimal _counterpartyClosingDocumentsOrdersDebt;
		private decimal _counterpartyWaitingForPaymentOrdersDebt;
		private decimal _counterpartyOtherOrdersDebt;
		private decimal _sumToAllocate;
		private decimal _lastBalance;

		private ILifetimeScope _lifetimeScope;
		private readonly IPaymentItemsRepository _paymentItemsRepository;

		public ManualCashlessIncomeMatchingViewModel(
			ILifetimeScope lifetimeScope,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IPaymentItemsRepository paymentItemsRepository,
			ICounterpartyJournalFactory counterpartyJournalFactory) : base(uowBuilder, uowFactory, commonServices)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_paymentItemsRepository = paymentItemsRepository ?? throw new ArgumentNullException(nameof(paymentItemsRepository));
			CounterpartyAutocompleteSelectorFactory =
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope);

			if(uowBuilder.IsNewEntity)
			{
				throw new AbortCreatingPageException(
					"Невозможно создать новую загрузку выписки из текущего диалога, необходимо использовать диалоги создания",
					_error);
			}

			//4914 Feature Переделать работу с доступами к диалогу
			/*var curEditor = Entity.CurrentEditorUser;
			if(curEditor != null)
			{
				throw new AbortCreatingPageException(
					$"Невозможно открыть диалог ручного распределения платежа №{Entity.PaymentNum}," +
					$" т.к. он уже открыт пользователем: {curEditor.Name}",
					_error);
			}*/

			//UpdateCurrentEditor();
			TabName = "Ручное распределение платежей";

			ConfigurePaymentsViewModels();
			CreateCommands();
			
			/*
			GetLastBalance();
			UpdateSumToAllocate();
			UpdateCurrentBalance();
			GetCounterpartyDebt();
			ConfigureEntityChangingRelations();
			UpdateNodes();
			*/

			//TabClosed += OnTabClosed;

			//4914 Feature убрать в методы добавления/удаления строк платежа
			/*Entity.ObservableItems.ElementRemoved += (_, _1, _2) => OnPropertyChanged(nameof(CanChangeCounterparty));

			Entity.ObservableItems.ElementAdded += (_, _1) => OnPropertyChanged(nameof(CanChangeCounterparty));*/
		}

		private void ConfigurePaymentsViewModels()
		{
			PaymentsViewModels = new List<ManualPaymentMatchingViewModel>();
			
			foreach(var payment in Entity.Payments)
			{
				AddNewPaymentViewModel(payment);
			}
		}

		public bool AddNewPayment(out ManualPaymentMatchingViewModel paymentViewModel)
		{
			paymentViewModel = null;
			
			if(Entity.TryAddNewPayment(AllocatedSum, out var payment))
			{
				paymentViewModel = AddNewPaymentViewModel(payment);
				return true;
			}
			
			ShowWarningMessage("Нельзя добавить платеж при полностью распределенной сумме!!!");
			return false;
		}

		private ManualPaymentMatchingViewModel AddNewPaymentViewModel(Payment payment)
		{
			var paymentViewModel = _lifetimeScope.Resolve<ManualPaymentMatchingViewModel>();
			paymentViewModel.Initialize(payment, UoW, this);
			PaymentsViewModels.Add(paymentViewModel);
			
			return paymentViewModel;
		}

		#region Свойства

		public decimal AllocatedSum
		{
			get => _allocatedSum;
			set => SetField(ref _allocatedSum, value);
		}

		public decimal CurrentBalance
		{
			get => _currentBalance;
			set => SetField(ref _currentBalance, value);
		}

		public decimal CounterpartyTotalDebt
		{
			get => _counterpartyTotalDebt;
			set => SetField(ref _counterpartyTotalDebt, value);
		}

		public decimal CounterpartyWaitingForPaymentOrdersDebt
		{
			get => _counterpartyWaitingForPaymentOrdersDebt;
			set => SetField(ref _counterpartyWaitingForPaymentOrdersDebt, value);
		}

		public decimal CounterpartyClosingDocumentsOrdersDebt
		{
			get => _counterpartyClosingDocumentsOrdersDebt;
			set => SetField(ref _counterpartyClosingDocumentsOrdersDebt, value);
		}

		public decimal CounterpartyOtherOrdersDebt
		{
			get => _counterpartyOtherOrdersDebt;
			set => SetField(ref _counterpartyOtherOrdersDebt, value);
		}

		public decimal SumToAllocate
		{
			get => _sumToAllocate;
			set => SetField(ref _sumToAllocate, value);
		}

		public decimal LastBalance
		{
			get => _lastBalance;
			set => SetField(ref _lastBalance, value);
		}

		#endregion

		public void Calculate(ManualPaymentMatchingAllocatingNode node)
		{
			if(CurrentBalance <= 0)
			{
				return;
			}

			var tempSum = AllocatedSum + node.ActualOrderSum - node.LastPayments - node.CurrentPayment;

			if(tempSum <= SumToAllocate)
			{
				AllocatedSum += node.ActualOrderSum - node.LastPayments - node.CurrentPayment;
				node.CurrentPayment = node.ActualOrderSum - node.LastPayments;
			}
			else
			{
				node.CurrentPayment += SumToAllocate - AllocatedSum;
				AllocatedSum = SumToAllocate;
			}

			UpdateCounterpartyDebt(node);

			node.OldCurrentPayment = node.CurrentPayment;

			UpdateCurrentBalance();
		}

		public void ReCalculate(ManualPaymentMatchingAllocatingNode node)
		{
			if(node.CurrentPayment == 0)
			{
				return;
			}

			AllocatedSum -= node.CurrentPayment;

			node.CurrentPayment = 0;

			UpdateCounterpartyDebt(node);

			node.OldCurrentPayment = 0;

			UpdateCurrentBalance();
		}

		public void CurrentPaymentChangedByUser(ManualPaymentMatchingAllocatingNode node)
		{
			if(node?.CurrentPayment == 0 && node?.OldCurrentPayment == 0)
			{
				return;
			}

			if(node.CurrentPayment < 0)
			{
				node.CurrentPayment = node.OldCurrentPayment;
				return;
			}

			var difference = node.ActualOrderSum - node.LastPayments;

			if(difference == 0)
			{
				node.CurrentPayment = difference;
				return;
			}

			if(node.CurrentPayment > difference)
			{
				node.CurrentPayment = difference;
			}

			AllocatedSum += node.CurrentPayment - node.OldCurrentPayment;

			UpdateCounterpartyDebt(node);

			node.OldCurrentPayment = node.CurrentPayment;

			UpdateCurrentBalance();
		}

		private void CreateCommands()
		{
			CreateCompleteAllocationCommand();
			CreateSaveViewModelCommand();
		}

		public void UpdateCurrentBalance() => CurrentBalance = SumToAllocate - AllocatedSum;

		private void UpdateCounterpartyDebt(ManualPaymentMatchingAllocatingNode node)
		{
			var addedPaymentSum = node.CurrentPayment - node.OldCurrentPayment;

			CounterpartyTotalDebt -= addedPaymentSum;

			if(node.IsClosingDocumentsOrder)
			{
				CounterpartyClosingDocumentsOrdersDebt -= addedPaymentSum;
			}

			if(node.OrderStatus == OrderStatus.WaitForPayment)
			{
				CounterpartyWaitingForPaymentOrdersDebt -= addedPaymentSum;
			}

			if(!node.IsClosingDocumentsOrder && node.OrderStatus != OrderStatus.WaitForPayment)
			{
				CounterpartyOtherOrdersDebt -= addedPaymentSum;
			}
		}

		#region Commands
		public DelegateCommand SaveViewModelCommand { get; private set; }
		private void CreateSaveViewModelCommand()
		{
			SaveViewModelCommand = new DelegateCommand(
				CompleteAllocation,
				() => true
			);
		}

		public DelegateCommand CompleteAllocationCommand { get; private set; }
		private void CreateCompleteAllocationCommand()
		{
			CompleteAllocationCommand = new DelegateCommand(
				CompleteAllocation,
				() => true
			);
		}

		public IEntityAutocompleteSelectorFactory CounterpartyAutocompleteSelectorFactory { get; }
		public IList<ManualPaymentMatchingViewModel> PaymentsViewModels { get; private set; }

		#endregion Commands

		private void CompleteAllocation()
		{
			var valid = CommonServices.ValidationService.Validate(Entity);
			if(!valid)
			{
				return;
			}

			//4914 Feature
			/*
			if(Entity.Status == PaymentState.Cancelled)
			{
				ShowWarningMessage($"Платеж находится в статусе {Entity.Status.GetEnumTitle()} распределения не возможны!");
				SaveAndCloseDialog();
				return;
			}

			if(CurrentBalance < 0)
			{
				ShowWarningMessage("Остаток не может быть отрицательным!");
				return;
			}

			if(CurrentBalance > 0)
			{
				if(!AskQuestion("Внимание! Имеется нераспределенный остаток. " +
								"Оставить его на балансе и завершить распределение?", "Внимание!"))
				{
					return;
				}
			}

			AllocateOrders();
			CreateOperations();

			var notCancelledItems = Entity.PaymentItems.Where(x => x.PaymentItemStatus != AllocationStatus.Cancelled);

			foreach(var item in notCancelledItems)
			{
				if(item.Order.OrderPaymentStatus == OrderPaymentStatus.Paid)
				{
					continue;
				}

				var otherPaymentsSum =
					_paymentItemsRepository.GetAllocatedSumForOrderWithoutCurrentPayment(UoW, item.Order.Id, Entity.Id);

				var totalSum = otherPaymentsSum + item.Sum;

				item.Order.OrderPaymentStatus = item.Order.OrderSum > totalSum
					? OrderPaymentStatus.PartiallyPaid
					: OrderPaymentStatus.Paid;
			}

			Entity.Status = PaymentState.completed;

			SaveAndCloseDialog();
			*/
		}

		private void SaveAndCloseDialog()
		{
			//UpdateCurrentEditor();

			try
			{
				SaveAndClose();
			}
			catch
			{
				ShowErrorMessage("При сохранении платежа произошла ошибка. Переоткройте диалог.");
				UoW.Session.Clear();
				RemoveCurrentEditor(UoW);
				Close(false, CloseSource.Self);
			}
		}

		/*private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(
				p => p.Counterparty,
				() => CounterpartyIsNull);
		}*/

		private void CreateOperations()
		{
			/*Entity.CreateIncomeOperation();

			foreach(PaymentItem item in Entity.ObservableItems)
			{
				item.CreateOrUpdateExpenseOperation();
			}*/
		}

		private void AllocateOrders()
		{
			/*var list = ListNodes.Where(x => x.CurrentPayment > 0);

			foreach(var node in list)
			{
				var order = UoW.GetById<VodOrder>(node.Id);
				Entity.AddPaymentItem(order, node.CurrentPayment);
			}
			*/
		}

		public void ClearProperties()
		{
			AllocatedSum = default(decimal);
			CurrentBalance = SumToAllocate;
		}

		/*private void UpdateCurrentEditor()
		{
			if(Entity.CurrentEditorUser != null)
			{
				DeleteCurrentEditor();
			}
			else
			{
				Entity.CurrentEditorUser = CurrentUser;
				UoW.Save();
			}
		}

		private void DeleteCurrentEditor()
		{
			Entity.CurrentEditorUser = null;
		}

		private void OnTabClosed(object sender, EventArgs e)
		{
			if(Entity.CurrentEditorUser != null)
			{
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
				{
					RemoveCurrentEditor(uow);
				}
			}
		}*/

		private void RemoveCurrentEditor(IUnitOfWork uow)
		{
			var curPayment = uow.GetById<Payment>(Entity.Id);
			curPayment.CurrentEditorUser = null;
			uow.Save(curPayment);
			uow.Commit();
		}

		public override bool Save(bool close)
		{
			if(TabParent != null && TabParent.CheckClosingSlaveTabs(this))
			{
				return false;
			}

			return base.Save(close);
		}

		public override void Close(bool askSave, CloseSource source)
		{
			if(TabParent != null && TabParent.CheckClosingSlaveTabs(this))
			{
				return;
			}

			base.Close(askSave, source);
		}

		/*public void UpdateCMOCounterparty()
		{
			if(Entity.CashlessMovementOperation != null && Entity.Counterparty?.Id != Entity.CashlessMovementOperation.Counterparty?.Id)
			{
				Entity.CashlessMovementOperation.Counterparty = Entity.Counterparty;
			}
		}*/
	}
}
