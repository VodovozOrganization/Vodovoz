using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;

namespace Vodovoz.ViewModels.Orders
{
	public class PromotionalSetViewModel : EntityTabViewModelBase<PromotionalSet>
	{
		public PromotionalSetViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Рекламные наборы";
			UoW = uowBuilder.CreateUoW<PromotionalSet>(unitOfWorkFactory);
			CreateCommands();
		}

		private IEnumerable<DiscountReason> discountReasonSource;
		public IEnumerable<DiscountReason> DiscountReasonSource {
			get => discountReasonSource ?? (discountReasonSource = UoW.GetAll<DiscountReason>());
		}

		private PromotionalSetItem selectedPromoItem;
		public PromotionalSetItem SelectedPromoItem {
			get => selectedPromoItem;
			set {
				SetField(ref selectedPromoItem, value);
				OnPropertyChanged(nameof(CanRemoveNomenclature));
			}
		}

		private PromotionalSetActionBase selectedAction;
		public PromotionalSetActionBase SelectedAction {
			get => selectedAction;
			set {
				SetField(ref selectedAction, value);
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

		public bool CanRemoveNomenclature => SelectedPromoItem != null;
		public bool CanRemoveAction => selectedAction != null && !UoW.GetAll<Order>().Any(o => o.PromotionalSets.Any(ps => ps.PromoSetName == Entity.PromoSetName));

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
				var nomenFilter = new NomenclatureFilterViewModel(CommonServices.InteractiveService);
				nomenFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder(),
					x => x.SelectCategory = NomenclatureCategory.water,
					x => x.SelectSaleCategory = SaleCategory.forSale);

				var nomenJournalViewModel = new NomenclaturesJournalViewModel(nomenFilter, UnitOfWorkFactory, CommonServices) {
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
			(actionType) => {
				PromotionalSetActionWidgetResolver resolver = new PromotionalSetActionWidgetResolver();
				SelectedActionViewModel = resolver.Resolve(Entity, actionType);

				if(SelectedActionViewModel is ICreationControl) {
					(SelectedActionViewModel as ICreationControl).AcceptCreation += (newAction) => {
						Entity.ObservablePromotionalSetActions.Add(newAction);
					};
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
	}
}
