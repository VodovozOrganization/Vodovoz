using System;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.ViewModels.Dialog;
using QS.Navigation;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;

namespace Vodovoz.ViewModels.Payments
{
	public class AutomaticallyAllocationBalanceWindowViewModel : WindowDialogViewModelBase, IDisposable
	{
		private readonly UnallocatedBalancesJournalNode _selectedUnallocatedBalancesNode;
		private readonly IList<UnallocatedBalancesJournalNode> _loadedNodes;
		private readonly int _closingDocumentDeliveryScheduleId;
		private readonly IInteractiveService _interactiveService;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IUnitOfWork _uow;
		private DelegateCommand _allocateByCurrentCounterpartyCommand;
		private DelegateCommand _allocateByAllCounterpartiesWithPositiveBalanceCommand;
		private bool _isAllocationState;
		private bool _allocateCompletedPayments = true;
		
		public AutomaticallyAllocationBalanceWindowViewModel(
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IPaymentsRepository paymentsRepository,
			IOrderRepository orderRepository,
			IUnitOfWorkFactory uowFactory,
			UnallocatedBalancesJournalNode selectedUnallocatedBalancesNode,
			IList<UnallocatedBalancesJournalNode> loadedNodes,
			int closingDocumentDeliveryScheduleId) : base(navigationManager)
		{
			if(navigationManager == null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_uow = (uowFactory ?? throw new ArgumentNullException(nameof(uowFactory))).CreateWithoutRoot();
			_selectedUnallocatedBalancesNode =
				selectedUnallocatedBalancesNode ?? throw new ArgumentNullException(nameof(selectedUnallocatedBalancesNode));
			_loadedNodes = loadedNodes ?? throw new ArgumentNullException(nameof(loadedNodes));
			_closingDocumentDeliveryScheduleId = closingDocumentDeliveryScheduleId;
			Resizable = false;
			Deletable = false;
			WindowPosition = WindowGravity.None;
		}

		public bool IsAllocationState
		{
			get => _isAllocationState;
			set => SetField(ref _isAllocationState, value);
		}

		public bool AllocateCompletedPayments
		{
			get => _allocateCompletedPayments;
			set => SetField(ref _allocateCompletedPayments, value);
		}

		public IProgressBarDisplayable ProgressBarDisplayable { get; set; }
		
		public DelegateCommand AllocateByCurrentCounterpartyCommand =>
			_allocateByCurrentCounterpartyCommand ?? (_allocateByCurrentCounterpartyCommand =
				new DelegateCommand(
					() =>
					{
						try
						{
							IsAllocationState = true;
							AllocateByCounterpartyAndOrg(_selectedUnallocatedBalancesNode);
							IsAllocationState = false;
						}
						catch(Exception e)
						{
							_interactiveService.ShowMessage(
								ImportanceLevel.Error, "Возникла непредвиденная ошибка, перезапустите операцию");
						}
						finally
						{
							Close(false, CloseSource.Self);
						}
					}
				)
			);

		public DelegateCommand AllocateByAllCounterpartiesWithPositiveBalanceCommand =>
			_allocateByAllCounterpartiesWithPositiveBalanceCommand ?? (_allocateByAllCounterpartiesWithPositiveBalanceCommand =
				new DelegateCommand(
					() =>
					{
						try
						{
							IsAllocationState = true;
							if(_loadedNodes.Count == 100)
							{
								ProgressBarDisplayable.Start(1, 0, "Получаем всех клиентов с положительным балансом...");

								var allUnAllocatedBalances =
									_paymentsRepository.GetAllUnallocatedBalances(_uow, _closingDocumentDeliveryScheduleId)
										.List<UnallocatedBalancesJournalNode>();

								AllocateLoadedBalances(allUnAllocatedBalances);
							}
							else
							{
								AllocateLoadedBalances(_loadedNodes);
							}
							IsAllocationState = false;
						}
						catch(Exception e)
						{
							_interactiveService.ShowMessage(
								ImportanceLevel.Error, "Возникла непредвиденная ошибка, перезапустите операцию");
						}
						finally
						{
							Close(false, CloseSource.Self);
						}
					}
				)
			);
		
		private void AllocateByCounterpartyAndOrg(UnallocatedBalancesJournalNode node)
		{
			var balance = node.CounterpartyBalance;
			var paymentNodes = _paymentsRepository.GetAllNotFullyAllocatedPaymentsByClientAndOrg(
				_uow, node.CounterpartyId, node.OrganizationId, AllocateCompletedPayments);

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

				var unallocatedSum = paymentNode.UnallocatedSum;
				var payment = _uow.GetById<Payment>(paymentNode.Id);
				
				while(orderNodes.Count > 0)
				{
					var order = _uow.GetById<Order>(orderNodes[0].Id);
					var sumToAllocate = orderNodes[0].OrderSum - orderNodes[0].AllocatedSum;
					
					if(balance >= unallocatedSum)
					{
						if(sumToAllocate <= unallocatedSum)
						{
							payment.AddPaymentItem(order, sumToAllocate);
							unallocatedSum -= sumToAllocate;
							balance -= sumToAllocate;
							orderNodes.RemoveAt(0);
							order.OrderPaymentStatus = OrderPaymentStatus.Paid;
						}
						else
						{
							payment.AddPaymentItem(order, unallocatedSum);
							orderNodes[0].AllocatedSum += unallocatedSum;
							balance -= unallocatedSum;
							order.OrderPaymentStatus = OrderPaymentStatus.PartiallyPaid;
							break;
						}

						if(unallocatedSum == 0)
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

				if(payment.Status != PaymentState.completed)
				{
					payment.CreateIncomeOperation();
					payment.Status = PaymentState.completed;
				}

				_uow.Save(payment);
			}

			_uow.Commit();
		}

		private void AllocateLoadedBalances(IList<UnallocatedBalancesJournalNode> loadedNodes)
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
			
			ProgressBarDisplayable.Update("Балансы разнесены успешно");
		}

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}
