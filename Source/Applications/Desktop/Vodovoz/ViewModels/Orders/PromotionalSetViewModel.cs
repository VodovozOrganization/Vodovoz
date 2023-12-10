using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.Orders
{
	public class PromotionalSetViewModel : EntityTabViewModelBase<PromotionalSet>, IPermissionResult
	{
		private ILifetimeScope _lifetimeScope;

		public PromotionalSetViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			ILifetimeScope lifetimeScope) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			CanChangeType = commonServices.CurrentPermissionService.ValidatePresetPermission( "can_change_the_type_of_promo_set" );

			if(!CanRead)
				AbortOpening("У вас недостаточно прав для просмотра");

			TabName = "Промонаборы";
			UoW = uowBuilder.CreateUoW<PromotionalSet>(unitOfWorkFactory);
			CreateCommands();
		}

		public string CreationDate => Entity.Id != 0 ? Entity.CreateDate.ToString("dd-MM-yyyy") : String.Empty;

		private PromotionalSetItem _selectedPromoItem;
		public PromotionalSetItem SelectedPromoItem {
			get => _selectedPromoItem;
			set {
				SetField(ref _selectedPromoItem, value);
				OnPropertyChanged(nameof(CanRemoveNomenclature));
			}
		}

		private PromotionalSetActionBase _selectedAction;
		public PromotionalSetActionBase SelectedAction {
			get => _selectedAction;
			set {
				SetField(ref _selectedAction, value);
				OnPropertyChanged(nameof(CanRemoveAction));
			}
		}

		private WidgetViewModelBase selectedActionViewModel;
		public WidgetViewModelBase SelectedActionViewModel {
			get => selectedActionViewModel;
			set {
				SetField(ref selectedActionViewModel, value);
			}
		}

		#region Permissions

		public bool CanCreate => PermissionResult.CanCreate;
		public bool CanRead => PermissionResult.CanRead;
		public bool CanUpdate => PermissionResult.CanUpdate;
		public bool CanDelete => PermissionResult.CanDelete;

		public bool CanCreateOrUpdate => Entity.Id == 0 ? CanCreate : CanUpdate;
		public bool CanRemoveNomenclature => SelectedPromoItem != null && CanUpdate;
		public bool CanRemoveAction => _selectedAction != null && CanDelete && Entity.Id == 0;

		public bool CanChangeType { get; }

		#endregion

		#region Commands

		private void CreateCommands()
		{
			CreateAddNomenclatureCommand();
			CreateRemoveNomenclatureCommand();
			CreateAddActionCommand();
			CreateRemoveActionCommand();
		}

		public DelegateCommand AddNomenclatureCommand;

		private void CreateAddNomenclatureCommand()
		{
			AddNomenclatureCommand = new DelegateCommand(
			() => {
				var nomenFilter = new NomenclatureFilterViewModel();
				nomenFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder(),
					x => x.SelectCategory = NomenclatureCategory.water,
					x => x.SelectSaleCategory = SaleCategory.forSale);

				var nomenJournalViewModel = new NomenclaturesJournalViewModel(_lifetimeScope, nomenFilter, UnitOfWorkFactory, CommonServices)
				{
					SelectionMode = JournalSelectionMode.Single
				};

				nomenJournalViewModel.OnEntitySelectedResult += (sender, e) => {
					var selectedNode = e.SelectedNodes.Cast<NomenclatureJournalNode>().FirstOrDefault();
					if(selectedNode == null) {
						return;
					}
					var nomenclature = UoW.GetById<Nomenclature>(selectedNode.Id);
					if(Entity.ObservablePromotionalSetItems.Any(i => i.Nomenclature.Id == nomenclature.Id))
						return;
					Entity.ObservablePromotionalSetItems.Add(new PromotionalSetItem {
						Nomenclature = nomenclature,
						Count = 0,
						Discount = 0,
						PromoSet = Entity
					});
				};
				TabParent.AddSlaveTab(this, nomenJournalViewModel);
			},
		  () => true
		  );
		}

		public DelegateCommand RemoveNomenclatureCommand;

		private void CreateRemoveNomenclatureCommand()
		{
			RemoveNomenclatureCommand = new DelegateCommand(
			() => Entity.ObservablePromotionalSetItems.Remove(SelectedPromoItem),
			() => CanRemoveNomenclature
			);
			RemoveNomenclatureCommand.CanExecuteChangedWith(this, x => CanRemoveNomenclature);
		}

		public DelegateCommand<PromotionalSetActionType> AddActionCommand;

		private void CreateAddActionCommand()
		{
			AddActionCommand = new DelegateCommand<PromotionalSetActionType>(
			(actionType) =>
			{
				SelectedActionViewModel = _lifetimeScope.Resolve<AddFixPriceActionViewModel>(
					new TypedParameter(typeof(IUnitOfWork), UoW),
					new TypedParameter(typeof(PromotionalSet), Entity));

				if(SelectedActionViewModel is ICreationControl) {
					(SelectedActionViewModel as ICreationControl).CancelCreation += () => {
						SelectedActionViewModel = null;
					};
				}
			}
			);
		}

		public DelegateCommand RemoveActionCommand;

		private void CreateRemoveActionCommand()
		{
			RemoveActionCommand = new DelegateCommand(
			() => Entity.ObservablePromotionalSetActions.Remove(SelectedAction),
			() => CanRemoveAction
				);
		}

		#endregion

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
