using System;
using Gamma.Binding.Converters;
using Gtk;
using QS.Views.GtkUI;
using QSWidgetLib;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Key = Gdk.Key;

namespace Vodovoz.Filters.GtkViews
{
	public partial class TrackPointJournalFilterView : FilterViewBase<TrackPointJournalFilterViewModel>
	{
		public TrackPointJournalFilterView(TrackPointJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yvalidatedentryRouteListId.KeyReleaseEvent += OnKeyReleased;
			yvalidatedentryRouteListId.ValidationMode = ValidationType.numeric;
			yvalidatedentryRouteListId.Binding.AddBinding(ViewModel, vm => vm.RouteListId, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();
			
			ylabelRouteList.Binding.AddBinding(ViewModel, vm => vm.RouteListLabelText, w => w.LabelProp).InitializeFromSource();
		}

		private void OnKeyReleased(object sender, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Key.Return)
			{
				ViewModel.Update();
			}
		}
	}
}
