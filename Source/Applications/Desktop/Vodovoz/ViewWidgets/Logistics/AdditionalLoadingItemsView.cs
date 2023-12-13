using System.Linq;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewWidgets.Logistics
{
	public partial class AdditionalLoadingItemsView : WidgetViewBase<AdditionalLoadingItemsViewModel>
	{
		public AdditionalLoadingItemsView(AdditionalLoadingItemsViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ytreeNomenclatures.Selection.Mode = SelectionMode.Multiple;
			ytreeNomenclatures.ColumnsConfig = FluentColumnsConfig<AdditionalLoadingDocumentItem>.Create()
				.AddColumn("Кол-во")
					.MinWidth(75)
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Amount)
					.Adjustment(new Adjustment(1, 1, 1000000, 1, 100, 0))
					.AddSetter((c, node) => c.Editable = ViewModel.CanEdit)
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
				.AddColumn("ТМЦ")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Nomenclature.Name)
				.Finish();
			ytreeNomenclatures.Selection.Changed += (sender, args) =>
				ybuttonRemove.Sensitive = ytreeNomenclatures.GetSelectedObjects<AdditionalLoadingDocumentItem>().Any() && ViewModel.CanEdit;

			ybuttonRemove.Clicked += (sender, args) =>
				ViewModel.RemoveItems(ytreeNomenclatures.GetSelectedObjects<AdditionalLoadingDocumentItem>());
			ybuttonRemove.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ybuttonAdd.Clicked += (sender, args) => ViewModel.AddItem();
			ybuttonAdd.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ViewModel.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == nameof(ViewModel.AdditionalLoadingDocument))
				{
					UpdateTreeView();
				}
			};
			UpdateTreeView();
		}

		private void UpdateTreeView()
		{
			ytreeNomenclatures.ItemsDataSource = ViewModel?.AdditionalLoadingDocument?.ObservableItems;
		}

		protected override void OnDestroyed()
		{
			ViewModel.Dispose();
			base.OnDestroyed();
		}
	}
}
