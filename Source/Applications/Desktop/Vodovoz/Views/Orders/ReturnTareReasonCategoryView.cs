using System;
using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReturnTareReasonCategoryView : TabViewBase<ReturnTareReasonCategoryViewModel>
	{
		public ReturnTareReasonCategoryView(ReturnTareReasonCategoryViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			ybtnSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			ybtnCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
			ybtnAddReason.Clicked += (sender, e) => ViewModel.AddReasonCommand.Execute();
			ybtnRemoveReason.Clicked += (sender, e) => ViewModel.RemoveReasonCommand.Execute();

			yentryReturnTareReasonCategoryName.Binding.AddBinding(ViewModel.Entity, vm => vm.Name, w => w.Text).InitializeFromSource();

			ytreeviewReasons.ColumnsConfig = FluentColumnsConfig<ReturnTareReason>
				.Create()
				.AddColumn("Код")
					.AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Причина")
					.AddTextRenderer(x => x.Name)
				.AddColumn("")
				.Finish();

			ytreeviewReasons.ItemsDataSource = ViewModel.Entity.ObservableChildReasons;
			ytreeviewReasons.Selection.Changed += TreeViewReasonsSelectionChanged;
		}

		void TreeViewReasonsSelectionChanged(object sender, EventArgs e)
		{
			var selected = ytreeviewReasons.GetSelectedObject();

			if(selected != null && selected is ReturnTareReason reason)
				ViewModel.SelectedReason = reason;
		}
	}
}
