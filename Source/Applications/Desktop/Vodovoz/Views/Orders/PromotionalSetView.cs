using Gamma.ColumnConfig;
using Gtk;
using QS.Project.Services;
using QS.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class PromotionalSetView : TabViewBase<PromotionalSetViewModel>
	{
		public PromotionalSetView(PromotionalSetViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			btnSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			btnSave.Binding
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();

			btnCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);

			yentryPromotionalSetName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanUpdate, w => w.Sensitive)
				.InitializeFromSource();

			yChkIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanUpdate, w => w.Sensitive)
				.InitializeFromSource();

			yentryDiscountReason.Binding
				.AddBinding(ViewModel.Entity, e => e.DiscountReasonInfo, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanUpdate, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbCanEditNomCount.Binding
				.AddBinding(ViewModel.Entity, e => e.CanEditNomenclatureCount, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanUpdate, w => w.Sensitive)
				.InitializeFromSource();
			/*
			ycheckCanBeAddedWithOtherPromoSets.Binding.AddBinding(ViewModel.Entity, e => e.CanBeAddedWithOtherPromoSets, w => w.Active)
													  .AddBinding(ViewModel, vm => vm.CanUpdate, w => w.Sensitive)
													  .InitializeFromSource();

			ycheckForTheFirstOrderOnlyToTheAddress.Binding.AddBinding(ViewModel.Entity, e => e.CanBeReorderedWithoutRestriction, w => w.Active)
														  .AddBinding(ViewModel, vm => vm.CanUpdate, w => w.Sensitive)
														  .InitializeFromSource();
			ycheckForTheFirstOrderOnlyToTheAddress.Sensitive = ViewModel.CanChangeType;
			*/
			widgetcontainerview.Binding
				.AddBinding(ViewModel, vm => vm.SelectedActionViewModel, w => w.WidgetViewModel);

			ybtnAddNomenclature.Clicked += (sender, e) => ViewModel.AddNomenclatureCommand.Execute();
			ybtnAddNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.CanUpdate, b => b.Sensitive)
				.InitializeFromSource();

			ybtnRemoveNomenclature.Clicked += (sender, e) => ViewModel.RemoveNomenclatureCommand.Execute();
			ybtnRemoveNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.CanRemoveNomenclature, b => b.Sensitive)
				.InitializeFromSource();

			yEnumButtonAddAction.ItemsEnum = typeof(PromotionalSetActionType);
			yEnumButtonAddAction.EnumItemClicked += (sender, e) => ViewModel.AddActionCommand.Execute((PromotionalSetActionType)e.ItemEnum);
			yEnumButtonAddAction.Binding
				.AddBinding(ViewModel, vm => vm.CanUpdate, w => w.Sensitive)
				.InitializeFromSource();

			ybtnRemoveAction.Clicked += (sender, e) => ViewModel.RemoveActionCommand.Execute();
			ybtnRemoveAction.Binding
				.AddBinding(ViewModel, vm => vm.CanRemoveAction, w => w.Sensitive)
				.InitializeFromSource();

			ConfigureTreeActions();
			ConfigureTreePromoSetsItems();

			ylblCreationDate.Text = ViewModel.CreationDate;
		}

		private void ConfigureTreePromoSetsItems()
		{
			yTreePromoSetItems.ColumnsConfig = new FluentColumnsConfig<PromotionalSetItem>()
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(i => i.Nomenclature.Id.ToString())
				.AddColumn("Товар")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(i => i.Nomenclature.Name)
				.AddColumn("Кол-во")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(i => i.Count)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter((c, n) => c.Digits = n.Nomenclature.Unit == null ? 0 : (uint)n.Nomenclature.Unit.Digits)
					.WidthChars(10)
					.Editing()
					.AddTextRenderer(i => i.Nomenclature.Unit == null ? string.Empty : i.Nomenclature.Unit.Name, false)
				.AddColumn("Скидка")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(i => i.ManualChangingDiscount).Editing(true)
					.AddSetter(
						(c, n) => c.Adjustment = n.IsDiscountInMoney
							? new Adjustment(0, 0, 1000000000, 1, 100, 1)
							: new Adjustment(0, 0, 100, 1, 100, 1)
					)
					.Digits(2)
					.WidthChars(10)
					.AddTextRenderer(n => n.IsDiscountInMoney ? CurrencyWorks.CurrencyShortName : "%", false)
				.AddColumn("Скидка \nв рублях?")
					.AddToggleRenderer(x => x.IsDiscountInMoney)
				.AddColumn("")
				.Finish();

			yTreePromoSetItems.ItemsDataSource = ViewModel.Entity.ObservablePromotionalSetItems;
			yTreePromoSetItems.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanUpdate, w => w.Sensitive)
				.AddBinding(vm => vm.SelectedPromoItem, w => w.SelectedRow)
				.InitializeFromSource();
		}

		private void ConfigureTreeActions()
		{
			yTreeActionsItems.ItemsDataSource = ViewModel.Entity.ObservablePromotionalSetActions;
			yTreeActionsItems.ColumnsConfig = new FluentColumnsConfig<PromotionalSetActionBase>()
				.AddColumn("Действие")
					.AddTextRenderer(x => x.Title)
				.Finish();
			yTreeActionsItems.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanUpdate, w => w.Sensitive)
				.AddBinding(vm => vm.SelectedAction, w => w.SelectedRow)
				.InitializeFromSource();
		}
	};
}
