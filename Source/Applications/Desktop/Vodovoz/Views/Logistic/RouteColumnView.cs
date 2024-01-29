using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Logistic;
namespace Vodovoz.Views.Logistic
{
	public partial class RouteColumnView : TabViewBase<RouteColumnViewModel>
	{
		public RouteColumnView(RouteColumnViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yvboxMain.Binding
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();

			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			yentryShortName.Binding
				.AddBinding(ViewModel.Entity, e => e.ShortName, w => w.Text)
				.InitializeFromSource();

			ycheckbuttonIsHighlighted.Binding
				.AddBinding(ViewModel.Entity, e => e.IsHighlighted, w => w.Active)
				.InitializeFromSource();
		}
	}
}
