using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.AdministrationTools;

namespace Vodovoz.AdministrationTools
{
	[ToolboxItem(true)]
	public partial class OrdersLoadTestingToolView : TabViewBase<OrdersLoadTestingToolViewModel>
	{
		public OrdersLoadTestingToolView(OrdersLoadTestingToolViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Initialize();
		}

		private void Initialize()
		{
			ViewModel.UiMarshal = action =>
				Gtk.Application.Invoke((sender, args) =>
				{
					try
					{
						action?.Invoke();
					}
					catch(Exception ex)
					{
						System.Diagnostics.Debug.WriteLine(ex);
					}
				});

			yspinThreads.Binding
				.AddBinding(ViewModel, vm => vm.ThreadCount, w => w.ValueAsInt)
				.AddBinding(ViewModel, vm => vm.CanEditThreadCount, w => w.Sensitive)
				.InitializeFromSource();

			ylabelStatus.Binding
				.AddBinding(ViewModel, vm => vm.StatusText, w => w.LabelProp)
				.InitializeFromSource();

			ytextviewLog.Binding
				.AddBinding(ViewModel, vm => vm.LogText, w => w.Buffer.Text)
				.InitializeFromSource();

			ybuttonStart.Clicked += (s, e) => ViewModel.StartCommand.Execute();
			ybuttonStop.Clicked += (s, e) => ViewModel.StopCommand.Execute();
			ybuttonClearLog.Clicked += (s, e) => ViewModel.ClearLogCommand.Execute();

			ybuttonStart.Binding
				.AddBinding(ViewModel, vm => vm.IsRunning, w => w.Sensitive, new BooleanInvertedConverter())
				.InitializeFromSource();

			ybuttonStop.Binding
				.AddBinding(ViewModel, vm => vm.IsRunning, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
