﻿using Gtk;
using QS.Journal.GtkUI;
using QS.Tdi;
using QS.Views.GtkUI;
using Vodovoz.Core;
using Vodovoz.ViewModels.Dialogs.Complaints;

namespace Vodovoz.Views.Complaints
{
	public partial class ComplaintsJournalsView : TabViewBase<ComplaintsJournalsViewModel>
	{
		public ComplaintsJournalsView(ComplaintsJournalsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
			UpdateJournal();
		}

		private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.Journal):
					UpdateJournal();
					break;
				default:
					break;
			}
		}

		private Widget _journalView;

		private void UpdateJournal()
		{
			_journalView?.Destroy();
			if(ViewModel.Journal == null)
			{
				return;
			}

			_journalView = ViewModelWidgetResolver.Instance.Resolve((ITdiTab)ViewModel.Journal);
			yvboxJournal.Add(_journalView);
			_journalView.Show();
		}
	}
}
