using Gamma.Binding;
using Gtk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.Presentation.Views
{
	[ToolboxItem(true)]
	public partial class IncludeExcludeFilterGroupView : WidgetViewBase<IncludeExludeFilterGroupViewModel>
	{
		[Obsolete("Не использовать, только для дизайнера!!")]
		public IncludeExcludeFilterGroupView() { }

		public IncludeExcludeFilterGroupView(IncludeExludeFilterGroupViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Initialize();
		}

		private void Initialize()
		{
			ybuttonClearExcludes.Binding.AddBinding(ViewModel, vm => vm.WithExcludes, w => w.Visible).InitializeFromSource();

			ybuttonClearExcludes.Clicked += (s, e) =>
			{
				ViewModel.ClearExcludesCommand.Execute();
				ytreeviewElements.QueueDraw();
			};

			ybuttonClearIncludes.Clicked += (s, e) =>
			{
				ViewModel.ClearIncludesCommand.Execute();
				ytreeviewElements.QueueDraw();
			};

			yentrySearch.Binding
				.AddBinding(ViewModel, vm => vm.SearchString, w => w.Text)
				.InitializeFromSource();

			yentrySearch.KeyReleaseEvent += (s, e) =>
			{
				if(e.Event.Key == Gdk.Key.Return
					&& ViewModel.SearchString != ViewModel.SearchString)
				{
					ViewModel.SearchString = ViewModel.SearchString;
				}
			};

			ybuttonSearchClear.Clicked += (s, e) => ViewModel.ClearSearchStringCommand.Execute();

			ycheckbuttonShowArchive.Binding
				.AddBinding(ViewModel, vm => vm.ShowArchived, w => w.Active)
				.InitializeFromSource();

			ViewModel.FilteredElementsChanged += (s, e) => ReBindElementsList();

			ytreeviewElements.Binding
				.AddBinding(ViewModel, vm => vm.Elements, w => w.ItemsDataSource)
				.InitializeFromSource();

			ReBindElementsList();
		}

		private void ReBindElementsList()
		{
			var recursiveModel = new RecursiveTreeModel<IncludeExcludeElement>(
				ViewModel.Elements,
				x => x.Parent,
				x => x.Children);

			ytreeviewElements.YTreeModel = recursiveModel;

			var columnConfig = ytreeviewElements.CreateFluentColumnsConfig<IncludeExcludeElement>();

			columnConfig.AddColumn("\t✔️")
				.AddToggleRenderer(x => x.Include)
				.AddSetter((cell, node) => cell.Activatable = node.IsEditable)
				.ToggledEvent(OnElementCheckboxToggled);

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

		private void OnElementCheckboxToggled(object o, ToggledArgs args)
		{
			Gtk.Application.Invoke((s, a) => ViewModel.RaiseSelectionChangedCommand.Execute());
			ytreeviewElements.QueueDraw();
		}

		public override void Destroy()
		{
			base.Destroy();
		}
	}
}
