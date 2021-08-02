using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
    public partial class DriverWorkScheduleSetView : TabViewBase<DriverWorkScheduleSetViewModel>
    {
        public DriverWorkScheduleSetView(DriverWorkScheduleSetViewModel viewModel) : base(viewModel)
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

            ytreeDriverWorkSchedules.ColumnsConfig = FluentColumnsConfig<DriverWorkScheduleNode>.Create()
                .AddColumn("")
                    .HeaderAlignment(0.5f)
                    .MinWidth(40)
                    .AddToggleRenderer(x => x.AtWork)
                    .XAlign(0.5f)
                .AddColumn("День")
                    .HeaderAlignment(0.5f)
                    .AddTextRenderer(x => x.WeekDay.GetEnumTitle())
                .AddColumn("Ходки")
                .AddComboRenderer(x => x.DaySchedule)
                    .SetDisplayFunc(x => x.Name)
                    .FillItems(ViewModel.DeliveryDaySchedules)
                    .Editing()
                .Finish();
            ytreeDriverWorkSchedules.ItemsDataSource = ViewModel.ObservableDriverWorkSchedules;
            ytreeDriverWorkSchedules.Sensitive = ViewModel.CanEdit;
        }
    }
}
