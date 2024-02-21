using MoreLinq;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.Project.Search;
using QS.Project.Search.GtkUI;
using QS.Report.ViewModels;
using QS.Report.Views;
using QS.ViewModels;
using QS.Views;
using QS.Views.Dialog;
using QS.Views.GtkUI;
using System.Linq;
using Vodovoz.Core;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.ViewModels.ViewModels.Settings;
using Vodovoz.Views.Settings;
using Vodovoz.ViewWidgets.Permissions;

namespace Vodovoz.Extensions
{
	public static class WidgetResolverExtensions
	{
		public static ViewModelWidgetResolver ConfigureWidgets(this ViewModelWidgetResolver widgetResolver)
		{
			ViewModelWidgetResolver.Instance = widgetResolver;

			widgetResolver.RegisterWidgetForTabViewModel<RdlViewerViewModel, RdlViewerView>()
				.RegisterWidgetForWidgetViewModel<SearchViewModel, SearchView>()
				.RegisterWidgetForWidgetViewModel<PresetUserPermissionsViewModel, PresetPermissionsView>()
				.RegisterWidgetForWidgetViewModel<PresetSubdivisionPermissionsViewModel, PresetPermissionsView>()
				.RegisterWidgetForWidgetViewModel<WarehousesSettingsViewModel, NamedDomainEntitiesSettingsView>();

			typeof(WidgetResolverExtensions).Assembly.GetTypes()
				.Where(x => x.BaseType != null
					&& x.BaseType.IsGenericType
					&& (x.BaseType.GetGenericTypeDefinition() == typeof(TabViewBase<>)
						|| (x.BaseType.GetGenericTypeDefinition() == typeof(ViewBase<>)
							&& typeof(TabViewModelBase).IsAssignableFrom(x.BaseType.GetGenericArguments().First()))))
				.ToDictionary(x => x, x => x.BaseType.GetGenericArguments().First())
				.ForEach(keyValue =>
				{
					widgetResolver.RegisterWidgetForTabViewModel(keyValue.Value, keyValue.Key);
				});

			widgetResolver.ConfigureFiltersWidgets();

			return widgetResolver;
		}

		public static IFilterWidgetResolver ConfigureFiltersWidgets(this ViewModelWidgetResolver widgetResolver)
		{
			DialogHelper.FilterWidgetResolver = widgetResolver;

			typeof(WidgetResolverExtensions).Assembly.GetTypes()
				.Where(x => x.BaseType != null
					&& x.BaseType.IsGenericType
					&& (x.BaseType.GetGenericTypeDefinition() == typeof(DialogViewBase<>)
					 || x.BaseType.GetGenericTypeDefinition() == typeof(WidgetViewBase<>)
					 || x.BaseType.GetGenericTypeDefinition() == typeof(FilterViewBase<>)
					 || x.BaseType.GetGenericTypeDefinition() == typeof(EntityTabViewBase<,>)
					 || (x.BaseType.GetGenericTypeDefinition() == typeof(ViewBase<>)
						&& (typeof(WidgetViewModelBase).IsAssignableFrom(x.BaseType.GetGenericArguments().First())
						|| typeof(ReportParametersViewModelBase).IsAssignableFrom((x.BaseType.GetGenericArguments().First()))))))
				.ToDictionary(x => x, x => x.BaseType.GetGenericArguments().First())
				.ForEach(keyValue =>
				{
					widgetResolver.RegisterWidgetForWidgetViewModel(keyValue.Value, keyValue.Key);
				});

			return widgetResolver;
		}
	}
}
