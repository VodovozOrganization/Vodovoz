using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.ViewModels.Edo;
namespace Vodovoz.Views.Edo
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoTaskInOrderResolveView : WidgetViewBase<EdoTaskInOrderResolveViewModel>
	{
		private EdoDocflowsView _edoDocflowsView;

		public EdoTaskInOrderResolveView()
		{
			this.Build();
			
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			ViewModel.PropertyChanged += ViewModelPropertyChanged;

			vboxTaskView.Visible = false;
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(EdoTaskInOrderResolveViewModel.DocsContentViewModel))
			{
				_edoDocflowsView?.Destroy();
				_edoDocflowsView = null;

				if(ViewModel.DocsContentViewModel != null && ViewModel.DocsContentViewModel is EdoDocflowsInOrderViewModel)
				{
					_edoDocflowsView = new EdoDocflowsView();
					_edoDocflowsView.ViewModel = (EdoDocflowsInOrderViewModel)ViewModel.DocsContentViewModel;
					vboxDocsView.PackStart(_edoDocflowsView, true, true, 0);
					_edoDocflowsView.Show();
					return;
				}
			}
		}
	}
}
