using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.Domain.Suppliers;
using Vodovoz.ViewModels.Suppliers;
namespace Vodovoz.Views.Suppliers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RequestToSupplierView : TabViewBase<RequestToSupplierViewModel>
	{
		public RequestToSupplierView(RequestToSupplierViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			entName.Binding.AddBinding(ViewModel.Entity, s => s.Name, w => w.Text).InitializeFromSource();
			entName.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();

			enumCmbSuppliersOrdering.ItemsEnum = typeof(SupplierOrderingType);
			enumCmbSuppliersOrdering.Binding.AddBinding(ViewModel.Entity, s => s.SuppliersOrdering, w => w.SelectedItem).InitializeFromSource();
			enumCmbSuppliersOrdering.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();

			txtComment.Binding.AddBinding(ViewModel.Entity, s => s.Comment, w => w.Buffer.Text).InitializeFromSource();
			txtComment.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();

			treeItems.ColumnsConfig = FluentColumnsConfig<FineItem>.Create()
				.AddColumn("№").AddTextRenderer(x => x.Fine.Id.ToString())
				.AddColumn("Сотрудник").AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Сумма штрафа").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.Money))
				.Finish();
			treeItems.Binding.AddBinding(ViewModel, vm => vm.FineItems, w => w.ItemsDataSource).InitializeFromSource();



			btnRefresh.Clicked += (sender, e) => ViewModel.RefreshCommand.Execute();
			btnRefresh.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();

			btnAdd.Clicked += (sender, e) => ViewModel.AddRequestingNomenclatureCommand.Execute();
			btnAdd.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();

			btnRemove.Clicked += (sender, e) => ViewModel.RemoveRequestingNomenclatureCommand.Execute();
			btnRemove.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();

			btnTransfer.Clicked += (sender, e) => ViewModel.TransferRequestingNomenclatureCommand.Execute();
			btnTransfer.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();
		}

	}
}
