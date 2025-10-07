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
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			enumFineType.ItemsEnum = typeof(FineTypes);
			enumFineType.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.FineType, w => w.SelectedItem)
				.AddBinding(vm => vm.CanEditFineType, w => w.Sensitive)
				.InitializeFromSource();

			ylabelOverspending.Binding
				.AddBinding(ViewModel, vm => vm.IsFuelOverspendingFine, w => w.Visible)
				.InitializeFromSource();
			yspinLiters.Adjustment.Upper = (double)FineItem.LitersOverspendingLimit;
			yspinLiters.Binding
				.AddBinding(ViewModel, vm => vm.Liters, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.IsFuelOverspendingFine, w => w.Visible)
				.InitializeFromSource();

			ylabelRequestRouteList.Binding
				.AddBinding(ViewModel, vm => vm.CanShowRequestRouteListMessage, w => w.Visible)
				.InitializeFromSource();

			ylabelAuthor.Binding
				.AddBinding(ViewModel.Entity, e => e.Author, w => w.LabelProp, new EmployeeToLastNameWithInitialsConverter())
				.InitializeFromSource();

			entryRouteList.ViewModel = ViewModel.RouteListViewModel;

			ylabelDate.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.Date.ToString("D"), w => w.LabelProp)
				.AddBinding(ViewModel, e => e.DateEditable, w => w.Visible, new BooleanInvertedConverter())
				.InitializeFromSource();

			ydatepicker.Binding
				.AddBinding(ViewModel.Entity, e => e.Date, w => w.Date)
				.AddBinding(ViewModel, e => e.DateEditable, w => w.Visible)
				.InitializeFromSource();
			ydatepicker.IsEditable = true;


			yspinMoney.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsStandartFine && vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.TotalMoney, w => w.ValueAsDecimal)
				.InitializeFromSource();

			ybuttonDivideByAll.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsStandartFine, w => w.Visible)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonDivideByAll.Clicked += (sender, e) => ViewModel.DivideByAllCommand.Execute();

			yenumcomboboxFineCategory.ItemsEnum = typeof(FineCategory);

			yentryFineReasonString.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.FineReasonString, w => w.Text)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			yentryFineReasonString.Binding.InitializeFromSource();

			ybuttonGetReasonFromTemplate.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsStandartFine, w => w.Visible)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonGetReasonFromTemplate.Clicked += (sender, e) => ViewModel.SelectReasonTemplateCommand.Execute();

			ybuttonAdd.Binding
				.AddBinding(ViewModel, vm => vm.IsNew, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonAdd.Clicked += (sender, e) => ViewModel.AddFineItemCommand.Execute();

			ybuttonRemove.Binding
				.AddBinding(ViewModel, vm => vm.IsNew, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonRemove.Clicked += (sender, e) => ViewModel.DeleteFineItemCommand.Execute(GetSelectedFineItem());

			ytreeviewItems.ColumnsConfig = FluentColumnsConfig<FineItem>.Create()
				.AddColumn("Сотрудник").AddTextRenderer(x => x.Employee.FullName)
				.AddColumn("Штраф").AddNumericRenderer(x => x.Money).Editing(ViewModel.IsStandartFine).Digits(2)
				.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
				.AddColumn("Причина штрафа").AddTextRenderer(x => x.Fine.FineReasonString)
				.Finish();
			ytreeviewItems.Binding
				.AddBinding(ViewModel.Entity, e => e.ObservableItems, w => w.ItemsDataSource)
				.InitializeFromSource();

			ylabelCarEvent.Binding.AddBinding(ViewModel, vm => vm.CarEvent, w => w.LabelProp).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			buttonSave.Sensitive = ViewModel.CanEdit;
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(ViewModel.AskSaveOnClose, QS.Navigation.CloseSource.Cancel);
		}

		private FineItem GetSelectedFineItem()
		{
			return ytreeviewItems.GetSelectedObject() as FineItem;
		}
	}
}
