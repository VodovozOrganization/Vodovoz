using System;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.ViewModels.Dialog;
using QS.Navigation;
using QS.Tdi;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;

namespace Vodovoz.ViewModels.Payments
{
	public class AutomaticallyAllocationBalanceWindowViewModel : WindowDialogViewModelBase, ITDICloseControlTab
	{
		private readonly UnAllocatedBalancesJournalNode _selectedUnAllocatedBalancesNode;
		private readonly IList<UnAllocatedBalancesJournalNode> _loadedNodes;
		private readonly int _closingDocumentDeliveryScheduleId;
		private readonly IInteractiveService _interactiveService;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IUnitOfWork _uow;
		private DelegateCommand _allocateByCurrentCounterpartyCommand;
		private DelegateCommand _allocateByAllCounterpartiesWithPositiveBalanceCommand;
		private bool _isAllocationState;
		
		public AutomaticallyAllocationBalanceWindowViewModel(
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IPaymentsRepository paymentsRepository,
			IOrderRepository orderRepository,
			IUnitOfWork uow,
			UnAllocatedBalancesJournalNode selectedUnAllocatedBalancesNode,
			IList<UnAllocatedBalancesJournalNode> loadedNodes,
			int closingDocumentDeliveryScheduleId) : base(navigationManager)
		{
			if(navigationManager == null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_selectedUnAllocatedBalancesNode = selectedUnAllocatedBalancesNode;
			_loadedNodes = loadedNodes ?? throw new ArgumentNullException(nameof(loadedNodes));
			_closingDocumentDeliveryScheduleId = closingDocumentDeliveryScheduleId;
			//IsModal = true;
			Resizable = false;
			WindowPosition = WindowGravity.None;
		}

		public bool IsAllocationState
		{
			get => _isAllocationState;
			set => SetField(ref _isAllocationState, value);
		}

		public IProgressBarDisplayable ProgressBarDisplayable { get; set; }
		
		public DelegateCommand AllocateByCurrentCounterpartyCommand =>
			_allocateByCurrentCounterpartyCommand ?? (_allocateByCurrentCounterpartyCommand =
				new DelegateCommand(
					() =>
					{			
						IsAllocationState = true;
						AllocateByCounterpartyAndOrg(_selectedUnAllocatedBalancesNode);
						IsAllocationState = false;
						Close(false, CloseSource.Self);
					}
				)
			);

		public DelegateCommand AllocateByAllCounterpartiesWithPositiveBalanceCommand =>
			_allocateByAllCounterpartiesWithPositiveBalanceCommand ?? (_allocateByAllCounterpartiesWithPositiveBalanceCommand =
				new DelegateCommand(
					() =>
					{
						if(_loadedNodes.Count == 100)
						{
							IsAllocationState = true;
							ProgressBarDisplayable.Start(1, 0, "Получаем всех клиентов с положительным балансом...");

							var allUnAllocatedBalances =
								_paymentsRepository.GetAllUnAllocatedBalances(_uow, _closingDocumentDeliveryScheduleId)
									.List<UnAllocatedBalancesJournalNode>();

							AllocateLoadedBalances(allUnAllocatedBalances);
						}
						else
						{
							IsAllocationState = true;
							AllocateLoadedBalances(_loadedNodes);
						}
						
						Close(false, CloseSource.Self);
					}
				)
			);
		
		public bool CanClose()
		{
			if(IsAllocationState)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Дождитесь завершения задачи и повторите");
			}

			return !IsAllocationState;
		}

		private void AllocateByCounterpartyAndOrg(UnAllocatedBalancesJournalNode node)
		{
			var balance = node.CounterpartyBalance;
			var paymentNodes = _paymentsRepository.GetAllNotFullyAllocatedPaymentsByClientAndOrg(
				_uow, node.CounterpartyId, node.OrganizationId);

			var orderNodes =
				_orderRepository.GetAllNotFullyPaidOrdersByClientAndOrg(
					_uow,
					node.CounterpartyId,
					node.OrganizationId,
					_closingDocumentDeliveryScheduleId);

			foreach(var paymentNode in paymentNodes)
			{
				if(balance == 0)
				{
					break;
				}

				var unAllocatedSum = paymentNode.PaymentSum - paymentNode.AllocatedSum;
				var payment = _uow.GetById<Payment>(paymentNode.Id);
				
				while(orderNodes.Count > 0)
				{
					var order = _uow.GetById<Order>(orderNodes[0].Id);
					var sumToAllocate = orderNodes[0].OrderSum - orderNodes[0].AllocatedSum;
					
					if(balance >= unAllocatedSum)
					{
						if(sumToAllocate <= unAllocatedSum)
						{
							payment.AddPaymentItem(order, sumToAllocate);
							unAllocatedSum -= sumToAllocate;
							balance -= sumToAllocate;
							orderNodes.RemoveAt(0);
							order.OrderPaymentStatus = OrderPaymentStatus.Paid;
						}
						else
						{
							payment.AddPaymentItem(order, unAllocatedSum);
							orderNodes[0].AllocatedSum += unAllocatedSum;
							balance -= unAllocatedSum;
							order.OrderPaymentStatus = OrderPaymentStatus.PartiallyPaid;
							break;
						}

						if(unAllocatedSum == 0)
						{
							break;
						}
					}
					else
					{
						if(sumToAllocate <= balance)
						{
							payment.AddPaymentItem(order, sumToAllocate);
							balance -= sumToAllocate;
							orderNodes.RemoveAt(0);
							order.OrderPaymentStatus = OrderPaymentStatus.Paid;
						}
						else
						{
							payment.AddPaymentItem(order, balance);
							balance = 0;
							order.OrderPaymentStatus = OrderPaymentStatus.PartiallyPaid;
						}

						if(balance == 0)
						{
							break;
						}
					}
				}

				var allocatedPaymentItems =
					payment.PaymentItems.Where(
						pi => pi.CashlessMovementOperation == null || pi.Sum != pi.CashlessMovementOperation.Expense);

				foreach(var paymentItem in allocatedPaymentItems)
				{
					paymentItem.CreateOrUpdateExpenseOperation();
				}

				_uow.Save(payment);
			}

			_uow.Commit();
		}

		private void AllocateLoadedBalances(IList<UnAllocatedBalancesJournalNode> loadedNodes)
		{
			var allocated = 0;
			ProgressBarDisplayable.Start(loadedNodes.Count, 0,
				$"Всего {loadedNodes.Count} клиентов с положительным балансом. Начинаем распределение...");
			
			foreach(var node in loadedNodes)
			{
				AllocateByCounterpartyAndOrg(node);
				allocated++;
				ProgressBarDisplayable.Add(1, $"Обработано {allocated} клиентов из {loadedNodes.Count}");
			}

			IsAllocationState = false;
			ProgressBarDisplayable.Update("Балансы разнесены успешно");
		}
	}
}
