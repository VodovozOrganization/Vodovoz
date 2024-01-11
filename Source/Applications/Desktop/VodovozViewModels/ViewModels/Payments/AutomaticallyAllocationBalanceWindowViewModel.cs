using System;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.ViewModels.Dialog;
using QS.Navigation;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Application.Payments;
using NHibernate;

namespace Vodovoz.ViewModels.Payments
{
	public class AutomaticallyAllocationBalanceWindowViewModel : WindowDialogViewModelBase, IDisposable
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly PaymentService _paymentService;
		private readonly IUnitOfWork _unitOfWork;

		private bool _isAllocationState;
		private bool _allocateCompletedPayments = true;

		private UnallocatedBalancesJournalNode _selectedUnallocatedBalancesNode;
		private IList<UnallocatedBalancesJournalNode> _loadedNodes;
		private int _closingDocumentDeliveryScheduleId;

		public AutomaticallyAllocationBalanceWindowViewModel(
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IPaymentsRepository paymentsRepository,
			PaymentService paymentService,
			IUnitOfWorkFactory uowFactory)
			: base(navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			if(uowFactory is null)
			{
				throw new ArgumentNullException(nameof(uowFactory));
			}

			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
			Title = "Автоматическое распределение положительного баланса";

			_unitOfWork = uowFactory.CreateWithoutRoot(Title);

			Resizable = false;
			Deletable = false;
			WindowPosition = WindowGravity.None;

			AllocateByCurrentCounterpartyCommand = new DelegateCommand(AllocateByCurrentCounterparty, () => CanAllocatoByCurrentCounterparty);
			AllocateByAllCounterpartiesWithPositiveBalanceCommand = new DelegateCommand(AllocateByAllCounterpartiesWithPositiveBalance);
		}

		public bool IsAllocationState
		{
			get => _isAllocationState;
			set
			{
				if(SetField(ref _isAllocationState, value))
				{
					OnPropertyChanged(nameof(CanAllocatoByCurrentCounterparty));
				}
			}
		}

		public bool AllocateCompletedPayments
		{
			get => _allocateCompletedPayments;
			set => SetField(ref _allocateCompletedPayments, value);
		}

		public IProgressBarDisplayable ProgressBarDisplayable { get; set; }

		public DelegateCommand AllocateByCurrentCounterpartyCommand { get; }
		public DelegateCommand AllocateByAllCounterpartiesWithPositiveBalanceCommand { get; }

		public bool CanAllocatoByCurrentCounterparty => _selectedUnallocatedBalancesNode != null && !IsAllocationState;

		public void Configure(
			UnallocatedBalancesJournalNode selectedUnallocatedBalancesNode,
			IList<UnallocatedBalancesJournalNode> loadedNodes,
			int closingDocumentDeliveryScheduleId)
		{
			_selectedUnallocatedBalancesNode = selectedUnallocatedBalancesNode
				?? throw new ArgumentNullException(nameof(selectedUnallocatedBalancesNode));
			_loadedNodes = loadedNodes ?? throw new ArgumentNullException(nameof(loadedNodes));
			_closingDocumentDeliveryScheduleId = closingDocumentDeliveryScheduleId;

			OnPropertyChanged(nameof(CanAllocatoByCurrentCounterparty));
		}

		public void AllocateByCurrentCounterparty()
		{
			try
			{
				IsAllocationState = true;
				AllocateByCounterpartyAndOrg(_selectedUnallocatedBalancesNode);
				IsAllocationState = false;
			}
			catch(Exception)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error, "Возникла непредвиденная ошибка, перезапустите операцию");
			}
			finally
			{
				Close(false, CloseSource.Self);
			}
		}

		private void AllocateByAllCounterpartiesWithPositiveBalance()
		{
			try
			{
				IsAllocationState = true;
				if(_loadedNodes.Count == 100)
				{
					ProgressBarDisplayable.Start(1, 0, "Получаем всех клиентов с положительным балансом...");

					var allUnAllocatedBalances =
						_paymentService.GetAllUnallocatedBalancesForAutomaticDistribution(_unitOfWork);

					AllocateLoadedBalances(allUnAllocatedBalances.Value.ToList());
				}
				else
				{
					AllocateLoadedBalances(_loadedNodes);
				}
				IsAllocationState = false;
			}
			catch(Exception)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error, "Возникла непредвиденная ошибка, перезапустите операцию");
			}
			finally
			{
				Close(false, CloseSource.Self);
			}
		}

		private void AllocateByCounterpartyAndOrg(UnallocatedBalancesJournalNode node)
		{
			var distributionResult = _paymentService.DistributeByClientIdAndOrganizationId(
				_unitOfWork,
				node.CounterpartyId,
				node.OrganizationId);

			distributionResult.Match(
				() =>
				{
					if(_unitOfWork.Session.GetCurrentTransaction() is null)
					{
						_unitOfWork.Session.BeginTransaction();
					}

					_unitOfWork.Commit();

					_interactiveService.ShowMessage(
						ImportanceLevel.Info,
						"Распределение успешно завершено");
				},
				errors =>
				{
					_unitOfWork.Session.GetCurrentTransaction()?.Rollback();

					_interactiveService.ShowMessage(
						ImportanceLevel.Error,
						"Не удалось завершить распределение: \n" + string.Join("\n", errors.Select(e => e.Message)));
				});
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
			_unitOfWork?.Dispose();
		}
	}
}
