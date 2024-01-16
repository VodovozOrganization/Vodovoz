﻿using System;
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
using Vodovoz.Errors;

namespace Vodovoz.ViewModels.Payments
{
	public class AutomaticallyAllocationBalanceWindowViewModel : WindowDialogViewModelBase, IDisposable
	{
		private readonly IInteractiveService _interactiveService;
		private readonly PaymentService _paymentService;
		private readonly IUnitOfWork _unitOfWork;

		private bool _isAllocationState;
		private bool _allocateCompletedPayments = true;

		private UnallocatedBalancesJournalNode _selectedUnallocatedBalancesNode;
		private IList<UnallocatedBalancesJournalNode> _loadedNodes;

		public AutomaticallyAllocationBalanceWindowViewModel(
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
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
			IList<UnallocatedBalancesJournalNode> loadedNodes)
		{
			_selectedUnallocatedBalancesNode = selectedUnallocatedBalancesNode
				?? throw new ArgumentNullException(nameof(selectedUnallocatedBalancesNode));
			_loadedNodes = loadedNodes ?? throw new ArgumentNullException(nameof(loadedNodes));

			OnPropertyChanged(nameof(CanAllocatoByCurrentCounterparty));
		}

		public void AllocateByCurrentCounterparty()
		{
			try
			{
				IsAllocationState = true;

				if(_unitOfWork.Session.GetCurrentTransaction() is null)
				{
					_unitOfWork.Session.BeginTransaction();
				}

				var distributionResult = AllocateByCounterpartyAndOrg(_selectedUnallocatedBalancesNode);
				IsAllocationState = false;

				distributionResult.Match(
					() =>
					{
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
							errors.First().Message);
					});
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

		private Result AllocateByCounterpartyAndOrg(UnallocatedBalancesJournalNode node)
		{
			return _paymentService.DistributeByClientIdAndOrganizationId(
				_unitOfWork,
				node.CounterpartyId,
				node.OrganizationId,
				AllocateCompletedPayments);
		}

		private void AllocateLoadedBalances(IList<UnallocatedBalancesJournalNode> loadedNodes)
		{
			var allocated = 0;
			ProgressBarDisplayable.Start(loadedNodes.Count, 0,
				$"Всего {loadedNodes.Count} клиентов с положительным балансом. Начинаем распределение...");

			var distributionResults = new List<Result>();

			if(_unitOfWork.Session.GetCurrentTransaction() is null)
			{
				_unitOfWork.Session.BeginTransaction();
			}

			foreach(var node in loadedNodes)
			{
				distributionResults.Add(AllocateByCounterpartyAndOrg(node));
				allocated++;
				ProgressBarDisplayable.Add(1, $"Обработано {allocated} клиентов из {loadedNodes.Count}");
			}

			if(!distributionResults.Any()
				|| distributionResults.All(result => result.IsSuccess))
			{
				_unitOfWork.Commit();
				ProgressBarDisplayable.Update($"Балансы {allocated} клиентов разнесены успешно");
				_interactiveService.ShowMessage(
							ImportanceLevel.Info,
							"Распределение успешно завершено");
				return;
			}

			_unitOfWork.Session.GetCurrentTransaction().Rollback();

			_interactiveService.ShowMessage(
				ImportanceLevel.Error,
				distributionResults.First(x => x.IsFailure).Errors.First().Message);
		}

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
