using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class LogisticsRequirementsView : WidgetViewBase<LogisticsRequirementsViewModel>
	{
		public LogisticsRequirementsView()
		{
			this.Build();
		}

		public LogisticsRequirementsView(LogisticsRequirementsViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			ycheckbuttonForwarder.Binding.AddBinding(ViewModel.Entity, r => r.ForwarderRequired, w => w.Active);
			ycheckbuttonDocuments.Binding.AddBinding(ViewModel.Entity, r => r.DocumentsRequired, w => w.Active);
			ycheckbuttonNationality.Binding.AddBinding(ViewModel.Entity, r => r.RussianDriverRequired, w => w.Active);
			ycheckbuttonPass.Binding.AddBinding(ViewModel.Entity, r => r.PassRequired, w => w.Active);
			ycheckbuttonLargus.Binding.AddBinding(ViewModel.Entity, r => r.LagrusRequired, w => w.Active);
		}
	}
}
