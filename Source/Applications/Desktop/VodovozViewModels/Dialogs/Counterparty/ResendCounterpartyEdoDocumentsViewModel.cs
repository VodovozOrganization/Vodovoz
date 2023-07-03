using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.ViewModels.Dialogs.Counterparty
{
	public class ResendCounterpartyEdoDocumentsViewModel : EntityTabViewModelBase<Domain.Client.Counterparty>
	{
		private readonly ICommonServices _commonServices;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private List<EdoContainerSelectableNode> _edoContainerNodes = new List<EdoContainerSelectableNode>();

		private DelegateCommand _resendSelectedEdoDocumentsCommand;
		private DelegateCommand _selectAllCommand;
		private DelegateCommand _unselectAllCommand;
		private DelegateCommand _invertSelectionCommand;
		private DelegateCommand _cancelCommand;

		public ResendCounterpartyEdoDocumentsViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			ICounterpartyRepository counterpartyRepository) :  base(uowBuilder, uowFactory, commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));

			Title = $"Повторная отправка неотправленных УПД контрагента {Entity.Name}";

			GetUndeliveredUpd();
		}

		public event EventHandler EdoContainerNodesListChanged;

		#region Свойства

		public List<EdoContainerSelectableNode> EdoContainerNodes
		{
			get => _edoContainerNodes;
			set => SetField(ref _edoContainerNodes, value);
		}

		#endregion

		private void GetUndeliveredUpd()
		{
			var startDate = new DateTime(2022, 10, 15);

			var documents = UoW.GetAll<EdoContainer>()
				.Where(x => 
					x.Counterparty.Id == Entity.Id
					&& x.Type == Domain.Orders.Documents.Type.Upd
					&& !x.IsIncoming
					&& x.EdoDocFlowStatus != EdoDocFlowStatus.Succeed
					&& x.Created >= startDate)
				.OrderByDescending(x => x.Created)
				.Select( x => new EdoContainerSelectableNode { IsSelected = true, EdoContainer = x })
				.ToList();

			foreach (var document in documents)
			{
				EdoContainerNodes.Add(document);
			}
		}

		#region Commands

		#region ResendSelectedEdoDocumentsCommand
		public DelegateCommand ResendSelectedEdoDocumentsCommand
		{
			get
			{
				if(_resendSelectedEdoDocumentsCommand == null)
				{
					_resendSelectedEdoDocumentsCommand = new DelegateCommand(ResendSelectedEdoDocuments, () => CanResendSelectedEdoDocuments);
					_resendSelectedEdoDocumentsCommand.CanExecuteChangedWith(this, x => x.CanResendSelectedEdoDocuments);
				}
				return _resendSelectedEdoDocumentsCommand;
			}
		}

		public bool CanResendSelectedEdoDocuments => true;

		private void ResendSelectedEdoDocuments()
		{
			var documentsToResend = _edoContainerNodes.Where(x => x.IsSelected).ToList();
			var infoMessage = $"Будет отправлено {documentsToResend.Count}";

			ServicesConfig.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Info, infoMessage);

			//Close(false, CloseSource.Cancel);
		}
		#endregion

		#region SelectAll

		public DelegateCommand SelectAllCommand
		{
			get
			{
				if(_selectAllCommand == null)
				{
					_selectAllCommand = new DelegateCommand(SelectAll, () => CanSelectAll);
					_selectAllCommand.CanExecuteChangedWith(this, x => x.CanSelectAll);
				}
				return _selectAllCommand;
			}
		}

		public bool CanSelectAll => true;

		private void SelectAll()
		{
			foreach(var item in _edoContainerNodes)
			{
				item.IsSelected = true;
			}
			EdoContainerNodesListChanged?.Invoke(this, EventArgs.Empty);
		}
		#endregion

		#region UnselectAll

		public DelegateCommand UnselectAllCommand
		{
			get
			{
				if(_unselectAllCommand == null)
				{
					_unselectAllCommand = new DelegateCommand(UnselectAll, () => CanUnselectAll);
					_unselectAllCommand.CanExecuteChangedWith(this, x => x.CanUnselectAll);
				}
				return _unselectAllCommand;
			}
		}

		public bool CanUnselectAll => true;

		private void UnselectAll()
		{
			foreach(var item in _edoContainerNodes)
			{
				item.IsSelected = false;
			}
			EdoContainerNodesListChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region InvertSelection

		public DelegateCommand InvertSelectionCommand
		{
			get
			{
				if(_invertSelectionCommand == null)
				{
					_invertSelectionCommand = new DelegateCommand(InvertSelection, () => CanInvertSelection);
					_invertSelectionCommand.CanExecuteChangedWith(this, x => x.CanInvertSelection);
				}
				return _invertSelectionCommand;
			}
		}

		public bool CanInvertSelection => true;

		private void InvertSelection()
		{
			foreach(var item in _edoContainerNodes)
			{
				item.IsSelected = !item.IsSelected;
			}
			EdoContainerNodesListChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region CancelCommand

		public DelegateCommand CancelCommand
		{
			get
			{
				if(_cancelCommand == null)
				{
					_cancelCommand = new DelegateCommand(Cancel, () => CanCancel);
					_cancelCommand.CanExecuteChangedWith(this, x => x.CanCancel);
				}
				return _cancelCommand;
			}
		}

		public bool CanCancel => true;

		private void Cancel()
		{
			Close(false, CloseSource.Cancel);
		}

		#endregion

		#endregion

		public override void Dispose()
		{
			UoW?.Dispose();
			base.Dispose();
		}

		public class EdoContainerSelectableNode
		{
			public bool IsSelected { get; set; }
			public EdoDocFlowStatus EdoDocFlowStatus => EdoContainer.EdoDocFlowStatus;
			public EdoContainer EdoContainer { get; set; }
		}
	}
}
