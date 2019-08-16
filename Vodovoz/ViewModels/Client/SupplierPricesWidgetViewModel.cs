using System;
using System.Linq;
using QS.Commands;
using QS.DomainModel.Config;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.JournalViewModels;

namespace Vodovoz.ViewModels.Client
{
	public class SupplierPricesWidgetViewModel : EntityWidgetViewModelBase<Counterparty>
	{
		readonly IEntityConfigurationProvider entityConfigurationProvider;
		readonly ITdiTab dialogTab;

		public SupplierPricesWidgetViewModel(Counterparty entity, IUnitOfWork uow, ITdiTab dialogTab, IEntityConfigurationProvider entityConfigurationProvider, ICommonServices commonServices) : base(entity, commonServices)
		{
			this.dialogTab = dialogTab ?? throw new ArgumentNullException(nameof(dialogTab));
			this.entityConfigurationProvider = entityConfigurationProvider ?? throw new ArgumentNullException(nameof(entityConfigurationProvider));
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			CreateCommands();
			//UpdateAcessibility();

		}

		void CreateCommands()
		{
			CreateAddItemCommand();
			CreateRemoveItemCommand();
			CreateEditItemCommand();
		}

		public bool CanAdd { get; set; } = true;
		public bool CanEdit { get; set; } = false;//задача редактирования пока не актуальна
		public bool CanRemove { get; set; }

		#region Commands

		#region AddItemCommand

		public DelegateCommand AddItemCommand { get; private set; }

		private void CreateAddItemCommand()
		{
			AddItemCommand = new DelegateCommand(
				() => {
					var filter = new NomenclatureFilterViewModel(CommonServices.InteractiveService);
					NomenclaturesJournalViewModel journalViewModel = new NomenclaturesJournalViewModel(
						filter,
						entityConfigurationProvider,
						CommonServices
					) {
						SelectionMode = JournalSelectionMode.Single
					};
					journalViewModel.OnEntitySelectedResult += (sender, e) => {
						var selectedNode = e.SelectedNodes.FirstOrDefault();
						if(selectedNode == null)
							return;
						//Entity.AddFine(UoW.GetById<Fine>(selectedNode.Id));
					};
					dialogTab.TabParent.AddSlaveTab(dialogTab, journalViewModel);
				},
				() => true
			);
		}

		#endregion AddItemCommand

		#region RemoveItemCommand

		public DelegateCommand RemoveItemCommand { get; private set; }

		private void CreateRemoveItemCommand()
		{
			RemoveItemCommand = new DelegateCommand(
				() => { },
				() => true
			);
		}

		#endregion RemoveItemCommand

		#region EditItemCommand

		public DelegateCommand EditItemCommand { get; private set; }

		private void CreateEditItemCommand()
		{
			EditItemCommand = new DelegateCommand(
				() => { },
				() => true
			);
		}

		#endregion EditItemCommand

		#endregion Commands

	}
}
