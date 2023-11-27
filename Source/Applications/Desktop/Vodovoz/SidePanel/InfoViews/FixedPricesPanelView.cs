using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.Domain.Goods;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModels.ViewModels.SidePanels;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FixedPricesPanelView : WidgetViewBase<FixedPricesPanelViewModel>, IPanelView
	{
		public FixedPricesPanelView(FixedPricesPanelViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			ytreeviewFixedPrices.ColumnsConfig = FluentColumnsConfig<NomenclatureFixedPrice>.Create()
				.AddColumn("Мин.\nкол.").AddNumericRenderer(x => x.MinCount)
				.AddColumn("Фиксированная\nцена").AddNumericRenderer(x => x.Price).Digits(2)
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature.Name)
				.Finish();
			ytreeviewFixedPrices.Binding.AddFuncBinding(ViewModel, vm => vm.FixedPrices, w => w.ItemsDataSource).InitializeFromSource();

			ylabelSource.Binding.AddFuncBinding(ViewModel, vm => vm.Title, w => w.LabelProp).InitializeFromSource();

			ybuttonOpenSource.Clicked += (s, e) => ViewModel.OpenFixedPricesDialogCommand.Execute();
			ViewModel.OpenFixedPricesDialogCommand.CanExecuteChanged += (s, e) =>
				ybuttonOpenSource.Sensitive = ViewModel.OpenFixedPricesDialogCommand.CanExecute();
			ybuttonOpenSource.Sensitive = ViewModel.OpenFixedPricesDialogCommand.CanExecute();
		}

		#region IPanelView implementation

		private IInfoProvider infoProvider;
		public IInfoProvider InfoProvider {
			get => infoProvider;
			set {
				infoProvider = value;
				Refresh();
			}
		}

		public void Refresh()
		{
			IFixedPricesHolderProvider pricesHolderProvider = InfoProvider as IFixedPricesHolderProvider;
			if(pricesHolderProvider == null) {
				return;
			}
			ViewModel.Refresh(pricesHolderProvider.Counterparty, pricesHolderProvider.DeliveryPoint);
		}

		public void OnCurrentObjectChanged(object changedObject)
		{
			Refresh();
		}

		public bool VisibleOnPanel => true;

		#endregion IPanelView implementation

		public override void Destroy()
		{
			ViewModel?.Dispose();
			base.Destroy();
		}
	}
}
