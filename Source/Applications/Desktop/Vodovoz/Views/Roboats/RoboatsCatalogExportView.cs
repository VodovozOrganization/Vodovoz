using Gtk;
using QS.Journal.GtkUI;
using QS.Tdi;
using QS.Views.GtkUI;
using QS.Views.Resolve;
using System;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Core;
using Vodovoz.Domain.Roboats;
using Vodovoz.ViewModels.Dialogs.Roboats;

namespace Vodovoz.Views.Roboats
{
	public partial class RoboatsCatalogExportView : TabViewBase<RoboatsCatalogExportViewModel>
	{
		private JournalView _journalView;
		private Widget _dialogView;
		private readonly IGtkViewResolver _gtkViewResolver;

		public RoboatsCatalogExportView(
			RoboatsCatalogExportViewModel viewModel,
			IGtkViewResolver gtkViewResolver) : base(viewModel)
		{
			_gtkViewResolver = gtkViewResolver ?? throw new ArgumentNullException(nameof(gtkViewResolver));
			Build();
			Configure();
		}

		private void Configure()
		{
			comboCatalog.ItemsEnum = typeof(RoboatsEntityType);
			object[] enums = ViewModel.DeniedEntityTypes.Cast<object>().ToArray();
			comboCatalog.AddEnumToHideList(enums);
			comboCatalog.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedExportType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			buttonExport.Clicked += (s, e) => ViewModel.StartExport.Execute();
			ViewModel.StartExport.CanExecuteChanged += (s, e) => buttonExport.Sensitive = ViewModel.StartExport.CanExecute();
			ViewModel.StartExport.RaiseCanExecuteChanged();

			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
			UpdateJournal();
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.Journal):
					UpdateJournal();
					break;
				case nameof(ViewModel.Dialog):
					UpdateDialog();
					break;
				default:
					break;
			}
		}


		private void UpdateJournal()
		{
			_journalView?.Destroy();
			if(ViewModel.Journal == null)
			{
				return;
			}

			if(ViewModel.Journal.FailInitialize)
			{
				return;
			}

			_journalView = new JournalView(ViewModel.Journal, _gtkViewResolver);
			journalHolder.Add(_journalView);
			_journalView.Show();
		}


		private void UpdateDialog()
		{
			_dialogView?.Destroy();
			if(ViewModel.Dialog == null)
			{
				return;
			}
			_dialogView = _gtkViewResolver.Resolve(ViewModel.Dialog);
			dialogHolder.Add(_dialogView);
			_dialogView.Show();
		}
	}
}
