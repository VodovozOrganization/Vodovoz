using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PacsSettingsView : WidgetViewBase<PacsSettingsViewModel>
	{
		public PacsSettingsView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			pacsdomainsettingsview1.ViewModel = ViewModel.DomainSettingsViewModel;

			ViewModel.PropertyChanged += ViewModelPropertyChanged;
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(PacsSettingsViewModel.DomainSettingsViewModel):
					pacsdomainsettingsview1.ViewModel = ViewModel.DomainSettingsViewModel;
					break;
				default:
					break;
			}
		}
	}
}
