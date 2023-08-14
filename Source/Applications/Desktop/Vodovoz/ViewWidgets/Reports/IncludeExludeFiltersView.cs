using Gamma.Binding;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.ViewWidgets.Reports
{
	[ToolboxItem(true)]
	public partial class IncludeExludeFiltersView : WidgetViewBase<IncludeExludeFiltersViewModel>
	{
		public IncludeExludeFiltersView(IncludeExludeFiltersViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			ybuttonSelectAll.Clicked += (s, e) => ViewModel.SelectAllCommand.Execute();

			ybuttonUnselect.Clicked += (s, e) => ViewModel.UnselectAllCommand.Execute();

			yentrySearch.Binding
				.AddBinding(ViewModel, vm => vm.SearchString, w => w.Text)
				.InitializeFromSource();

			yentrySearch.KeyReleaseEvent += (s, e) =>
			{
				if(e.Event.Key == Gdk.Key.Return
					&& ViewModel.CurrentSearchString != ViewModel.SearchString)
				{
					ViewModel.CurrentSearchString = ViewModel.SearchString;
				}
			};

			ybuttonSearchClear.Clicked += (s, e) => ViewModel.ClearSearchStringCommand.Execute();

			ycheckbuttonShowArchive.Binding
				.AddBinding(ViewModel, vm => vm.ShowArchived, w => w.Active)
				.InitializeFromSource();

			ytreeviewFilters.CreateFluentColumnsConfig<IncludeExcludeFilter>()
				.AddColumn("").AddTextRenderer(x => x.Title)
				.Finish();

			ytreeviewFilters.HeadersVisible = false;

			ytreeviewFilters.ItemsDataSource = ViewModel.Filters;

			ytreeviewFilters.YTreeModel.EmitModelChanged();

			ViewModel.Filters.CollectionChanged += (s, e) => ytreeviewFilters.YTreeModel?.EmitModelChanged();

			ytreeviewFilters.Selection.Mode = Gtk.SelectionMode.Single;

			ytreeviewFilters.Binding
				.AddBinding(ViewModel, vm => vm.ActiveFilter, w => w.SelectedRow)
				.InitializeFromSource();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;

			ViewModel.FilteredElementsChanged += (s, e) => ReBindElementsList();

			ytreeviewElements.Binding.AddBinding(ViewModel, vm => vm.Elements, w => w.ItemsDataSource).InitializeFromSource();

			ReBindElementsList();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.ActiveFilter))
			{
				ReBindElementsList();

				return;
			}
		}

		private void ReBindElementsList()
		{
			if(ViewModel.ActiveFilter != null)
			{
				var recursiveModel = new RecursiveTreeModel<IncludeExcludeElement>(
					ViewModel.Elements,
					x => x.Parent,
					x => x.Children);

				ytreeviewElements.YTreeModel = recursiveModel;

				ytreeviewElements.CreateFluentColumnsConfig<IncludeExcludeElement>()
					.AddColumn("✔️").AddToggleRenderer(x => x.Include)
					.AddColumn("X").AddToggleRenderer(x => x.Exclude)
					.AddColumn("").AddTextRenderer(x => x.Title ?? "")
					.AddSetter((cell, node) =>
					{
						if(cell == null)
						{
							return;
						}

						if(!string.IsNullOrWhiteSpace(ViewModel.SearchString))
						{
							cell.Markup = node.Title.Replace(ViewModel.SearchString, $"<b>{ViewModel.SearchString}</b>");
						}
					})
					.Finish();
			}

			ytreeviewElements.YTreeModel?.EmitModelChanged();
		}
	}
}
