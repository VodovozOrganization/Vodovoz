using QS.Views.Dialog;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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

			ViewModel.LogStrings.CollectionChanged += OnLogStringsChanged;

			ylabelStartTime.Binding
				.AddBinding(ViewModel, vm => vm.StartDateTimeMessage, w => w.LabelProp)
				.InitializeFromSource();

			ylabelEndTimeAndDiff.Binding
				.AddBinding(ViewModel, vm => vm.EndDateTimeAndDiffMessage, w => w.LabelProp)
				.InitializeFromSource();

			ybuttonRunOperation.BindCommand(ViewModel.RunCommand);
		}

		private void OnLogStringsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			ytreeview1.ScrollToCell(ytreeview1.YTreeModel.PathFromNode(ViewModel.LogStrings.LastOrDefault()), ytreeview1.Columns.First(), true, 0f, 0f);
		}

		public override void Destroy()
		{
			ViewModel.LogStrings.CollectionChanged -= OnLogStringsChanged;

			base.Destroy();
		}
	}
}
