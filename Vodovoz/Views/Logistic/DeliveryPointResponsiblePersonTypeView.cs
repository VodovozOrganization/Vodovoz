using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class DeliveryPointResponsiblePersonTypeView : TabViewBase<DeliveryPointResponsiblePersonTypeViewModel>
    {
        public DeliveryPointResponsiblePersonTypeView(DeliveryPointResponsiblePersonTypeViewModel viewModel) : base(viewModel)
        {
            this.Build();

            ConfigureDlg();
        }

        void ConfigureDlg()
        {
            yentryDeliveryPointResponsiblePersonTypeName.Binding.AddBinding(ViewModel.Entity, e => e.Title, w => w.Text).InitializeFromSource();

            buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
            buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
        }
    }
}
