using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.ViewModels.Dialogs.Counterparty
{
	public class ResendCounterpartyEdoDocumentsViewModel : EntityTabViewModelBase<Domain.Client.Counterparty>
	{
		private readonly ICommonServices _commonServices;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private GenericObservableList<EdoContainerSelectableNode> _edoContainerNodes = new GenericObservableList<EdoContainerSelectableNode>();

		private DelegateCommand _resendSelectedEdoDocumentsCommand;
		private DelegateCommand _selectAllCommand;

		public ResendCounterpartyEdoDocumentsViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			ICounterpartyRepository counterpartyRepository) :  base(uowBuilder, uowFactory, commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));

			Title = $"Отправить все неотправленные УПД контрагента {Entity.Name}";

			GetUndeliveredUpd();
		}

		#region Свойства

		public GenericObservableList<EdoContainerSelectableNode> EdoContainerNodes
		{
			get => _edoContainerNodes;
			set => SetField(ref _edoContainerNodes, value);
		}

		#endregion

		private void GetUndeliveredUpd()
		{
			var documents = UoW.GetAll<EdoContainer>()
				.Where(x => 
					x.Counterparty.Id == Entity.Id
					&& x.Type == Domain.Orders.Documents.Type.Upd
					&& !x.Received)
				.OrderBy(x => x.Created)
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
		}
		#endregion

		#region UnselectAll

		private DelegateCommand _unselectAllCommand;
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
		}

		#endregion

		#region InvertSelection

		private DelegateCommand _invertSelectionCommand;
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
			public EdoContainer EdoContainer { get; set; }
		}
	}
}
