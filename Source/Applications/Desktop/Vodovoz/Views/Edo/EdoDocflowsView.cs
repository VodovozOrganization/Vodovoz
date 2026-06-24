using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Edo;
namespace Vodovoz.Views.Edo
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoDocflowsView : WidgetViewBase<EdoDocflowsInOrderViewModel>
	{
		public EdoDocflowsView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			treeviewEdoDocflows.ColumnsConfig = FluentColumnsConfig<EdoDocflowRowInOrderViewModel>.Create()
				.AddColumn("Провайдер")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.EdoProvider).Editable(false)
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.TaxcomDocumentId).Editing(false)
				.AddColumn("Код документооборота")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.TaxcomDocflowId).Editable(false)
				.AddColumn("Статус")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.TaxcomDocflowStatus).Editable(false)
				.AddColumn("")
				.RowCells()
					.XAlign(0.5f)
				.Finish();
			treeviewEdoDocflows.Selection.Mode = Gtk.SelectionMode.Single;
			treeviewEdoDocflows.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Docflows, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedDocflow, w => w.SelectedRow)
				.InitializeFromSource();

			buttonOpenDocflow.Visible = false;
			//buttonsResendToEdo.BindCommand(ViewModel.ResendCommand);
		}
	}
}
