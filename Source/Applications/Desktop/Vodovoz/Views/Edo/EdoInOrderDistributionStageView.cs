using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.ViewModels.Edo;

namespace Vodovoz.Views.Edo
{
	[ToolboxItem(true)]
	public partial class EdoInOrderDistributionStageView : WidgetViewBase<EdoInOrderDistributionStageViewModel>
	{
		public EdoInOrderDistributionStageView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			ylabelDescription.Binding
				.AddBinding(ViewModel, vm => vm.Description, w => w.LabelProp)
				.InitializeFromSource();
		}
	}
}
