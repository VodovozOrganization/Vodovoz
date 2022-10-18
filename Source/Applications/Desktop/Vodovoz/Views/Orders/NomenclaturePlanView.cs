using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
    public partial class NomenclaturePlanView : TabViewBase<NomenclaturePlanViewModel>
    {
        public NomenclaturePlanView(NomenclaturePlanViewModel viewModel) : base(viewModel)
        {
            this.Build();
            Configure();
        }

        private void Configure()
        {
            ylabelNomenclature.LabelProp = ViewModel.Entity.Name;
            yentryPlanDay.Binding.AddBinding(ViewModel.Entity, e => e.PlanDay, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();
            yentryPlanMonth.Binding.AddBinding(ViewModel.Entity, e => e.PlanMonth, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();

            buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
            buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);	
        }
    }
}
