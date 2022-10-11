using System.Linq;
using Gamma.ColumnConfig;
using Gtk;
using QS.Dialog.GtkUI;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
    public partial class DriverDistrictPrioritySetView : TabViewBase<DriverDistrictPrioritySetViewModel>
    {
        public DriverDistrictPrioritySetView(DriverDistrictPrioritySetViewModel viewModel) : base(viewModel)
        {
            this.Build();
            Configure();
        }

        private void Configure()
        {
            hboxInfo.Visible = ViewModel.IsInfoVisible;

            ybuttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);

            ybuttonAccept.Sensitive = ViewModel.CanEdit;
            ybuttonAccept.Clicked += (sender, args) => ViewModel.AcceptCommand.Execute();

            ylabelAuthor.Binding.AddBinding(ViewModel, vm => vm.Author, w => w.Text).InitializeFromSource();
            ylabelCode.Binding.AddBinding(ViewModel, vm => vm.Id, w => w.Text).InitializeFromSource();
            ylabelDateActivated.Binding.AddBinding(ViewModel, vm => vm.DateActivated, w => w.Text).InitializeFromSource();
            ylabelDateDeactivated.Binding.AddBinding(ViewModel, vm => vm.DateDeactivated, w => w.Text).InitializeFromSource();

            ytreeDriverDistrictPriorities.ColumnsConfig = FluentColumnsConfig<DriverDistrictPriorityNode>.Create()
            	.AddColumn("Приоритет")
                    .AddNumericRenderer(x => x.Priority + 1)
                    .XAlign(0.5f)
            	.AddColumn("Район")
                    .HeaderAlignment(0.5f)
                    .AddTextRenderer(x => x.District.DistrictName)
                    .XAlign(0.5f)
                .AddColumn("Активная\nверсия районов")
                    .HeaderAlignment(0.5f)
                    .AddToggleRenderer(x => x.District.DistrictsSet.Status == DistrictsSetStatus.Active)
                    .XAlign(0.5f)
                .AddColumn("Код версии")
                    .HeaderAlignment(0.5f)
                    .AddNumericRenderer(x => x.District.DistrictsSet.Id)
                    .XAlign(0.5f)
                .AddColumn("")
                .Finish();
            ytreeDriverDistrictPriorities.Selection.Mode = SelectionMode.Multiple;
            ytreeDriverDistrictPriorities.Reorderable = true;
            ytreeDriverDistrictPriorities.ItemsDataSource = ViewModel.ObservableDriverDistrictPriorities;
            ytreeDriverDistrictPriorities.DragEnd += (o, args) => ViewModel.CheckAndFixDistrictsPrioritiesCommand.Execute();
            ytreeDriverDistrictPriorities.Sensitive = ViewModel.CanEdit;

            ybuttonAddDistricts.Clicked += (sender, args) => ViewModel.AddDistrictsCommand.Execute();
            ybuttonAddDistricts.Sensitive = ViewModel.CanEdit;

            ybuttonDeleteDistricts.Clicked += (sender, args) => ViewModel.DeleteDistrictsCommand.Execute(
                ytreeDriverDistrictPriorities.GetSelectedObjects<DriverDistrictPriorityNode>());
            ybuttonDeleteDistricts.Sensitive = ViewModel.CanEdit;
        }
    }
}
