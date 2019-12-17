using System;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;
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
				OnPropertyChanged(nameof(CanRemove));
			}
		}

		#region Commands
		public bool CanRemove => SelectedPromoItem != null;

		private void CreateCommands()
		{
			CreateAddNomenculatureCommand();
			CreateRemoveNomenculatureCommand();
		}

		public DelegateCommand AddNomenculatureCommand;

		private void CreateAddNomenculatureCommand()
		{
			AddNomenculatureCommand = new DelegateCommand(
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
						Id = nomenclature.Id,
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

		public DelegateCommand RemoveNomenculatureCommand;

		private void CreateRemoveNomenculatureCommand()
		{
			RemoveNomenculatureCommand = new DelegateCommand(
			() => {
				Entity.ObservablePromotionalSetItems.Remove(SelectedPromoItem);
			},
			() => CanRemove
			);
			RemoveNomenculatureCommand.CanExecuteChangedWith(this, x => CanRemove);
		}

		public DelegateCommand<PromosetActionType> AddActionCommand;

		private void CreateAddActionCommand()
		{
			AddActionCommand = new DelegateCommand<PromosetActionType>(
			(actionType) => {
				switch(actionType) {
					case PromosetActionType.FixPrice: AddFixPriceAction(); break;
					default: throw new ArgumentException();
				}
			}
			);
		}

		#endregion
		private void AddFixPriceAction()
		{

		}

	}
}
