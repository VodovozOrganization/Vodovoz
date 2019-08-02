using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Employees;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Domain.Employees;
using Gamma.ColumnConfig;

namespace Vodovoz.Views.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FineView : TabViewBase<FineViewModel>
	{
		public FineView(FineViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			enumFineType.Binding.AddBinding(ViewModel.Entity, e => e.FineType, w => w.SelectedItem).InitializeFromSource();
			enumFineType.Binding.AddBinding(ViewModel, vm => vm.CanEditFineType, w => w.Sensitive).InitializeFromSource();

			ylabelOverspending.Binding.AddBinding(ViewModel, vm => vm.IsFuelOverspendingFine, w => w.Visible).InitializeFromSource();
			yspinLiters.Binding.AddBinding(ViewModel.Entity, e => e.LitersOverspending, w => w.ValueAsDecimal).InitializeFromSource();
			yspinLiters.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			yspinLiters.Binding.AddBinding(ViewModel, vm => vm.IsFuelOverspendingFine, w => w.Visible).InitializeFromSource();

			ylabelRequestRouteList.Binding.AddBinding(ViewModel, vm => vm.CanShowRequestRouteListMessage, w => w.Visible).InitializeFromSource();

			ylabelAuthor.Binding.AddBinding(ViewModel.Entity, e => e.Author, w => w.LabelProp, new EmployeeToLastNameWithInitialsConverter()).InitializeFromSource();

			yentryreferenceRouteList.Binding.AddBinding(ViewModel.Entity, e => e.RouteList, w => w.Subject).InitializeFromSource();
			yentryreferenceRouteList.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ylabelDate.Binding.AddFuncBinding(ViewModel.Entity, e => e.Date.ToString("D"), w => w.LabelProp).InitializeFromSource();

			yspinMoney.Binding.AddBinding(ViewModel.Entity, e => e.TotalMoney, w => w.ValueAsDecimal).InitializeFromSource();
			yspinMoney.Binding.AddBinding(ViewModel, vm => vm.IsStandartFine, w => w.Sensitive).InitializeFromSource();

			ybuttonDivideByAll.Binding.AddBinding(ViewModel, vm => vm.IsStandartFine, w => w.Visible).InitializeFromSource();

			yentryFineReasonString.Binding.AddBinding(ViewModel.Entity, e => e.FineReasonString, w => w.Text).InitializeFromSource();
			yentryFineReasonString.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ybuttonGetReasonFromTemplate.Binding.AddBinding(ViewModel, vm => vm.IsStandartFine, w => w.Visible).InitializeFromSource();

			ybuttonAdd.Clicked += (sender, e) => { ViewModel.AddFineItemCommand.Execute(); };
			ViewModel.AddFineItemCommand.CanExecuteChanged += (sender, e) => { ybuttonAdd.Sensitive = ViewModel.AddFineItemCommand.CanExecute(); };

			ybuttonRemove.Binding.AddBinding(ViewModel, vm => vm.IsStandartFine, w => w.Sensitive).InitializeFromSource();
			ViewModel.DeleteFineItemCommand.CanExecuteChanged += (sender, e) => { ybuttonRemove.Sensitive = ViewModel.DeleteFineItemCommand.CanExecute(GetSelectedFineItem()); };

			ytreeviewItems.ColumnsConfig = FluentColumnsConfig<FineItem>.Create()
				.AddColumn("Сотрудник").AddTextRenderer(x => x.Employee.FullName)
				.AddColumn("Штраф").AddNumericRenderer(x => x.Money).Editing(ViewModel.IsStandartFine).Digits(2)
				.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
				.AddColumn("Причина штрафа").AddTextRenderer(x => x.Fine.FineReasonString)
				.Finish();
			ytreeviewItems.Binding.AddBinding(ViewModel.Entity, e => e.ObservableItems, w => w.ItemsDataSource).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false); };
		}

		private FineItem GetSelectedFineItem()
		{
			return ytreeviewItems.GetSelectedObject() as FineItem;
		}
	}
}
