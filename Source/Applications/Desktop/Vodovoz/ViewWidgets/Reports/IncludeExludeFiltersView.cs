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

			ybuttonClearAllExcludes.Clicked += (s, e) =>
			{
				ViewModel.ClearAllExcludesCommand.Execute();
				ytreeviewElements.QueueDraw();
			};

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

			ytreeviewFilters.CreateFluentColumnsConfig<IncludeExcludeFilter>()
				.AddColumn("✔️")
				.AddNumericRenderer(x => x.IncludedCount)
				.AddSetter((c, n) =>
				{
					c.ForegroundGdk = Rc.GetStyle(this).Foreground(StateType.Normal);
					if(n.IncludedCount == 0)
					{
						c.Text = "";
					}
					else
					{
						c.Text = n.IncludedCount.ToString();
					}
				})
				.AddColumn("X")
				.AddNumericRenderer(x => x.ExcludedCount)
				.AddSetter((c, n) =>
				{
					c.ForegroundGdk = Rc.GetStyle(this).Foreground(StateType.Normal);
					if(n.ExcludedCount == 0)
					{
						c.Text = "";
					}
					else
					{
						c.Text = n.ExcludedCount.ToString();
					}
				})
				.AddColumn("").AddTextRenderer(x => x.Title)
				.Finish();

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

				ytreeviewElements.CreateFluentColumnsConfig<IncludeExcludeElement>()
					.AddColumn("\t✔️")
					.AddToggleRenderer(x => x.Include).ToggledEvent(OnElementCheckboxToggled)
					.AddColumn("X").AddToggleRenderer(x => x.Exclude).ToggledEvent(OnElementCheckboxToggled)
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
		}

		private void OnElementCheckboxToggled(object o, ToggledArgs args)
		{
			Gtk.Application.Invoke((s, a) => ViewModel.RaiseSelectionChangedCommand.Execute());
			ytreeviewElements.QueueDraw();
		}
	}
}
