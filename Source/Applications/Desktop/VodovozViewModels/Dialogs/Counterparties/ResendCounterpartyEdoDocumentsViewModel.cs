using EdoService;
using EdoService.Library;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Orders.Documents;
using DocumentContainerType = Vodovoz.Core.Domain.Documents.DocumentContainerType;

namespace Vodovoz.ViewModels.Dialogs.Counterparties
{
	public class ResendCounterpartyEdoDocumentsViewModel : EntityTabViewModelBase<Domain.Client.Counterparty>
	{
		private List<EdoContainerSelectableNode> _edoContainerNodes = new List<EdoContainerSelectableNode>();

		private DelegateCommand _resendSelectedEdoDocumentsCommand;
		private DelegateCommand _selectAllCommand;
		private DelegateCommand _unselectAllCommand;
		private DelegateCommand _invertSelectionCommand;
		private DelegateCommand _cancelCommand;
		private readonly IEdoService _edoService;

		public ResendCounterpartyEdoDocumentsViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			List<int> ordersIds,
			IEdoService edoService) :  base(uowBuilder, uowFactory, commonServices)
		{
			_edoService = edoService ?? throw new ArgumentNullException(nameof(edoService));

			Title = $"Повторная отправка неотправленных УПД контрагента {Entity.Name}";

			CanResendSelectedEdoDocuments = CommonServices.CurrentPermissionService.ValidatePresetPermission("can_resend_edo_documents");

			GetLastUpdByOrderIds(ordersIds);
		}

		public event EventHandler EdoContainerNodesListChanged;

		#region Свойства

		public List<EdoContainerSelectableNode> EdoContainerNodes
		{
			get => _edoContainerNodes;
			set => SetField(ref _edoContainerNodes, value);
		}

		#endregion

		private void GetLastUpdByOrderIds(List<int> orderIds)
		{
			var startDate = new DateTime(2022, 10, 15);

			var documents = UoW.GetAll<EdoContainer>()
				.Where(x => 
					x.Counterparty.Id == Entity.Id
					&& x.Type == DocumentContainerType.Upd
					&& !x.IsIncoming
					&& x.EdoDocFlowStatus != EdoDocFlowStatus.Succeed
					&& x.Created >= startDate
					&& orderIds.Contains(x.Order.Id))
				.OrderByDescending(x => x.Created)
				.GroupBy(x => x.Order.Id)			
				.ToList();

			foreach (var item in documents)
			{
				var updDocument = item
					.OrderByDescending(x => x.Created)
					.Select(x => new EdoContainerSelectableNode { IsSelected = true, EdoContainer = x })
					.FirstOrDefault();

				EdoContainerNodes.Add(updDocument);
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

		public bool CanResendSelectedEdoDocuments { get; }

		private void ResendSelectedEdoDocuments()
		{
			var documentsToResend = _edoContainerNodes.Where(x => x.IsSelected).ToList();
			var infoMessage = $"Будет отправлено {documentsToResend.Count}";

			CommonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Info, infoMessage);

			foreach(var document in documentsToResend)
			{
				var order = document.EdoContainer.Order;

				var edoValidateResult = _edoService.ValidateOrderForDocument(order, document.EdoContainer.Type);

				var errorMessages = edoValidateResult.Errors.Select(x => x.Message).ToArray();

				if(edoValidateResult.IsFailure)
				{
					if(edoValidateResult.Errors.Any(error => error.Code == Errors.Edo.EdoErrors.AlreadyPaidUpd)
						&& !CommonServices.InteractiveService.Question(
							"Вы уверены, что хотите отправить повторно?\n" +
							string.Join("\n - ", errorMessages),
							"Требуется подтверждение!"))
					{
						continue;
					}
				}

				_edoService.SetNeedToResendEdoDocumentForOrder(order, DocumentContainerType.Upd);
			}

			Close(false, CloseSource.Cancel);
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
