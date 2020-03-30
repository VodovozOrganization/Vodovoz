using System;
using Vodovoz.SearchViewModels;
using Gtk;
using QS.Project.Search.GtkUI;

namespace Vodovoz.Views.Search
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SolrSearchView : Gtk.Bin
	{
		private readonly SingleEntrySolrCriterionSearchViewModel viewModel;

		public SolrSearchView(SingleEntrySolrCriterionSearchViewModel viewModel)
		{
			this.Build();
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			buttonSwitchMode.Clicked += ButtonSwitchMode_Clicked ;
			viewModel.PropertyChanged += ViewModel_PropertyChanged;
			UpdateView();
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(viewModel.SolrDisable):
					UpdateView();
					break;
				default:
					break;
			}
		}

		HBox viewContent;

		SolrSingleEntrySearchView searchView = null;

		private void UpdateView()
		{
			viewContent?.Destroy();
			viewContent = new HBox();

			//if(!viewModel.SolrDisable) {
			//	viewModel.SearchModelGeneric.UpdateSolrServiceAvailability();
			//}
			if(viewModel.SolrDisable) {
				var table = new Table(3, 1, false);

				var sv = new SingleEntrySearchView(viewModel);
				viewContent.Add(sv);
			} else {
				viewModel.SearchModelGeneric.UpdateSolrServiceAvailability();
				searchView = new SolrSingleEntrySearchView(viewModel);
				viewContent.Add(searchView);
			}
			hboxSearchView.Add(viewContent);

			hboxSearchView.ShowAll();
		}

		private void ButtonSwitchMode_Clicked(object sender, EventArgs e)
		{
			viewModel.SolrDisable = !buttonSwitchMode.Active;
		}

	}
}
