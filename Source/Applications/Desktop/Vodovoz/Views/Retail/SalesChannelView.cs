using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Retail;

namespace Vodovoz.Views.Retail
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class SalesChannelView : TabViewBase<SalesChannelViewModel>
    {
        public SalesChannelView(SalesChannelViewModel viewModel) : base(viewModel)
        {
            this.Build();

            ConfigureDlg();
        }

        void ConfigureDlg()
        {
            yentrySalesChannelName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();

            buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
            buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
        }
    }
}
