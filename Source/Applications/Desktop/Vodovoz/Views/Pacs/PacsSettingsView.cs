using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[ToolboxItem(true)]
	public partial class PacsSettingsView : WidgetViewBase<PacsSettingsViewModel>
	{
		public PacsSettingsView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			domainSettingsView.ViewModel = ViewModel.DomainSettingsViewModel;

			buttonInnerPhones.BindCommand(ViewModel.OpenInnerPhonesReferenceBookCommand);
			buttonOperators.BindCommand(ViewModel.OpenOperatorsReferenceBookCommand);
			buttonWorkShifts.BindCommand(ViewModel.OpenWorkShiftsReferenceBookCommand);

			ViewModel.PropertyChanged += ViewModelPropertyChanged;
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(PacsSettingsViewModel.DomainSettingsViewModel):
					domainSettingsView.ViewModel = ViewModel.DomainSettingsViewModel;
					break;
				default:
					break;
			}
		}
	}
}
