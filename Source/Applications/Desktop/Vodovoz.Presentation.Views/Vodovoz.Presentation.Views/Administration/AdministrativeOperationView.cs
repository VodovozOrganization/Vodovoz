using QS.Views.Dialog;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Administration;
using static Vodovoz.Presentation.ViewModels.Administration.AdministrativeOperationViewModelBase;

namespace Vodovoz.Presentation.Views.Administration
{
	[ToolboxItem(true)]
	public partial class AdministrativeOperationView : DialogViewBase<AdministrativeOperationViewModelBase>
	{
		public AdministrativeOperationView(AdministrativeOperationViewModelBase viewModel)
			: base(viewModel)
		{
			Build();

			ytreeview1.CreateFluentColumnsConfig<LogNode>()
				.AddColumn("DateTime")
				.AddTextRenderer(x => x.DateTime.ToString("G"))
				.AddColumn("Level")
				.AddEnumRenderer(x => x.LogLevel)
				.AddColumn("Message")
				.AddTextRenderer(x => x.Message)
				.Finish();

			ytreeview1.ItemsDataSource = ViewModel.LogStrings;

			ylabelStartTime.Binding
				.AddBinding(ViewModel, vm => vm.StartDateTimeMessage, w => w.LabelProp)
				.InitializeFromSource();

			ylabelEndTimeAndDiff.Binding
				.AddBinding(ViewModel, vm => vm.EndDateTimeAndDiffMessage, w => w.LabelProp)
				.InitializeFromSource();

			ybuttonRunOperation.BindCommand(ViewModel.RunCommand);
		}
	}
}
