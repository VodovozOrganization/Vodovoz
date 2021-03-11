using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
    public partial class DriverCarKindView : TabViewBase<DriverCarKindViewModel>
    {
        public DriverCarKindView(DriverCarKindViewModel viewModel) : base(viewModel)
        {
            this.Build();
            ConfigureDlg();
        }

        private void ConfigureDlg()
        {
            yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();

            buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
            buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false, QS.Navigation.CloseSource.Cancel); };
        }
    }
}
