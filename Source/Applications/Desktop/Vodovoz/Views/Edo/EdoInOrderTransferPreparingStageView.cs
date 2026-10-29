using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.ViewModels.Edo;

namespace Vodovoz.Views.Edo
{
	[ToolboxItem(true)]
	public partial class EdoInOrderTransferPreparingStageView : WidgetViewBase<EdoInOrderTransferPreparingStageViewModel>
	{
		public EdoInOrderTransferPreparingStageView()
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
