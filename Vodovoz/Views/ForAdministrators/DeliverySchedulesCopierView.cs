using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.ForAdministrators;

namespace Vodovoz.Views.ForAdministrators
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliverySchedulesCopierView : TabViewBase<DeliverySchedulesCopierViewModel>
	{
		public DeliverySchedulesCopierView(DeliverySchedulesCopierViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			cmbDistrictSrc.ItemsList = ViewModel.GetAllDistricts();
			cmbDistrictSrc.SetRenderTextFunc<ScheduleRestrictedDistrict>(x => string.Format("{0}: {1}", x.Id, x.DistrictName));
			cmbDistrictSrc.Binding.AddBinding(ViewModel, vm => vm.SourceDistrict, w => w.SelectedItem).InitializeFromSource();

			buttonSave.Binding.AddBinding(ViewModel, vm => vm.CanSave, w => w.Sensitive).InitializeFromSource();
			buttonSave.Clicked += (sender, e) => ViewModel.SaveCommand.Execute();

			buttonCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();

			btnCopy.Clicked += (sender, e) => ViewModel.CopySchedulesCommand.Execute();
			btnCopy.Binding.AddBinding(ViewModel, vm => vm.CanCopy, w => w.Sensitive).InitializeFromSource();

			treeEditableDistricts.ColumnsConfig = FluentColumnsConfig<ScheduleRestrictedDistrict>.Create()
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DistrictName)
				.AddColumn("Зарплатный район")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.WageDistrict.Name)
				.AddColumn("")
				.Finish()
				;
			treeEditableDistricts.ItemsDataSource = ViewModel.ObservableDistrictsToEdit;
			treeEditableDistricts.Selection.Changed += (sender, e) => ViewModel.ItemSelected = GetSelectedItem() != null;

			btnAddDistricts.Clicked += (sender, e) => ViewModel.AddDistrictCommand.Execute();
			btnAddDistricts.Binding.AddBinding(ViewModel, vm => vm.CanAddDistricts, w => w.Sensitive).InitializeFromSource();

			btnRemove.Clicked += (sender, e) => ViewModel.RemoveCommand.Execute(GetSelectedItem());
			btnRemove.Binding.AddBinding(ViewModel, vm => vm.ItemSelected, w => w.Sensitive).InitializeFromSource();
		}
		ScheduleRestrictedDistrict GetSelectedItem() => treeEditableDistricts.GetSelectedObject<ScheduleRestrictedDistrict>();
	}
}
