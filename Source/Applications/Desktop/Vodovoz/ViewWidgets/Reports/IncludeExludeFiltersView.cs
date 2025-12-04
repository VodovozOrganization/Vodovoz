using Gamma.Binding;
using Gtk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.ViewWidgets.Reports
{
	[ToolboxItem(true)]
	public partial class IncludeExludeFiltersView : WidgetViewBase<IncludeExludeFiltersViewModel>
	{
		[Obsolete("Не использовать, только для дизайнера!!")]
		public IncludeExludeFiltersView() { }

		public IncludeExludeFiltersView(IncludeExludeFiltersViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Initialize();
		}

		private void Initialize()
		{
			ybuttonClearAllIncludes.Clicked += (s, e) =>
			{
				ViewModel.ClearAllIncludesCommand.Execute();
				ytreeviewElements.QueueDraw();
			};

			ybuttonClearAllExcludes.Binding.AddBinding(ViewModel, vm => vm.WithExcludes, w => w.Visible).InitializeFromSource();

			ybuttonClearAllExcludes.Clicked += (s, e) =>
			{
				ViewModel.ClearAllExcludesCommand.Execute();
				ytreeviewElements.QueueDraw();
			};

			ybuttonClearExcludes.Binding.AddBinding(ViewModel, vm => vm.WithExcludes, w => w.Visible).InitializeFromSource();

			ybuttonClearExcludes.Clicked += (s, e) =>
			{
				ViewModel.ActiveFilter?.ClearExcludesCommand.Execute();
				ytreeviewElements.QueueDraw();
			};

			ybuttonClearIncludes.Clicked += (s, e) =>
			{
				ViewModel.ActiveFilter?.ClearIncludesCommand.Execute();
				ytreeviewElements.QueueDraw();
			};

			buttonInfo.Clicked += (s, e) => ViewModel.ShowInfoCommand.Execute();

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

			var columnConfig = ytreeviewFilters.CreateFluentColumnsConfig<IncludeExcludeFilter>();

			columnConfig
				.AddColumn("✔️")
					.AddNumericRenderer(x => x.IncludedCount)
					.AddSetter((c, n) =>
					{
						c.ForegroundGdk = Rc.GetStyle(this).Foreground(StateType.Normal);
						c.Text = n.IncludedCount == 0 ? "" : n.IncludedCount.ToString();
					});

			if(ViewModel.WithExcludes)
			{
				columnConfig
					.AddColumn("X")
						.AddNumericRenderer(x => x.ExcludedCount)
						.AddSetter((c, n) =>
						{
							c.ForegroundGdk = Rc.GetStyle(this).Foreground(StateType.Normal);
							c.Text = n.ExcludedCount == 0 ? "" : n.ExcludedCount.ToString();
						});
			}

			columnConfig
				.AddColumn("")
					.AddTextRenderer(x => x.Title);

			columnConfig.Finish();

			ytreeviewFilters.ItemsDataSource = ViewModel.Filters;

			ytreeviewFilters.Selection.Mode = Gtk.SelectionMode.Single;

			ytreeviewFilters.Binding
				.AddBinding(ViewModel, vm => vm.ActiveFilter, w => w.SelectedRow)
				.InitializeFromSource();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;

			ViewModel.FilteredElementsChanged += (s, e) => ReBindElementsList();

			ytreeviewElements.Binding
				.AddBinding(ViewModel, vm => vm.Elements, w => w.ItemsDataSource)
				.InitializeFromSource();

			ReBindElementsList();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.ActiveFilter))
			{
				ReBindElementsList();
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

				var columnConfig = ytreeviewElements.CreateFluentColumnsConfig<IncludeExcludeElement>();
				
				var includeMapping = columnConfig.AddColumn("\t✔️")
					.AddToggleRenderer(x => x.Include)
					.AddSetter((cell, node) => cell.Activatable = node.IsEditable)
					.ToggledEvent(OnElementCheckboxToggled);

				if(ViewModel.ActiveFilter.IsRadio)
				{
					includeMapping.Radio();
				}

				if(ViewModel.WithExcludes)
				{
					columnConfig.AddColumn("X")
						.HeaderAlignment(0.5f)
						.AddToggleRenderer(x => x.Exclude)
						.AddSetter((cell, node) => cell.Activatable = node.IsEditable)
						.ToggledEvent(OnElementCheckboxToggled);
				}

				columnConfig
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
						});

				columnConfig.Finish();
			}
		}

		private void OnElementCheckboxToggled(object o, ToggledArgs args)
		{
			Gtk.Application.Invoke((s, a) => ViewModel.RaiseSelectionChangedCommand.Execute());
			ytreeviewElements.QueueDraw();
		}
	}
}
