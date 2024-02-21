using Microsoft.Extensions.Logging;
using MoreLinq;
using QS.Dialog.Gtk;
using QS.Project.Search;
using QS.Project.Search.GtkUI;
using QS.Report.ViewModels;
using QS.Report.Views;
using QS.ViewModels;
using QS.Views;
using QS.Views.Dialog;
using QS.Views.GtkUI;
using System;
using System.Linq;
using Vodovoz.Core;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.ViewModels.ViewModels.Settings;
using Vodovoz.Views.Settings;
using Vodovoz.ViewWidgets.Permissions;

namespace Vodovoz.Commons
{
	public sealed class ViewModelWidgetsRegistrar
	{
		private readonly ILogger<ViewModelWidgetsRegistrar> _logger;
		private readonly ViewModelWidgetResolver _viewModelWidgetResolver;

		public ViewModelWidgetsRegistrar(
			ILogger<ViewModelWidgetsRegistrar> logger,
			ViewModelWidgetResolver viewModelWidgetResolver)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_viewModelWidgetResolver = viewModelWidgetResolver
				?? throw new ArgumentNullException(nameof(viewModelWidgetResolver));
		}

		public void RegisterateWidgets()
		{
			ViewModelWidgetResolver.Instance = _viewModelWidgetResolver;

			_viewModelWidgetResolver.RegisterWidgetForTabViewModel<RdlViewerViewModel, RdlViewerView>()
				.RegisterWidgetForWidgetViewModel<SearchViewModel, SearchView>()
				.RegisterWidgetForWidgetViewModel<PresetUserPermissionsViewModel, PresetPermissionsView>()
				.RegisterWidgetForWidgetViewModel<PresetSubdivisionPermissionsViewModel, PresetPermissionsView>()
				.RegisterWidgetForWidgetViewModel<WarehousesSettingsViewModel, NamedDomainEntitiesSettingsView>();

			typeof(ViewModelWidgetsRegistrar).Assembly.GetTypes()
				.Where(x => x.BaseType != null
					&& x.BaseType.IsGenericType
					&& (x.BaseType.GetGenericTypeDefinition() == typeof(TabViewBase<>)
						|| (x.BaseType.GetGenericTypeDefinition() == typeof(ViewBase<>)
							&& typeof(TabViewModelBase).IsAssignableFrom(x.BaseType.GetGenericArguments().First()))))
				.ToDictionary(x => x, x => x.BaseType.GetGenericArguments().First())
				.ForEach(keyValue =>
				{
					_viewModelWidgetResolver.RegisterWidgetForTabViewModel(keyValue.Value, keyValue.Key);
					_logger.LogInformation("Зарегистрирована {ViewModel} для {Widget}", keyValue.Value, keyValue.Key);
				});

			ConfigureFiltersWidgets();
		}

		public void ConfigureFiltersWidgets()
		{
			DialogHelper.FilterWidgetResolver = _viewModelWidgetResolver;

			typeof(ViewModelWidgetsRegistrar).Assembly.GetTypes()
				.Where(x => x.BaseType != null
					&& x.BaseType.IsGenericType
					&& (x.BaseType.GetGenericTypeDefinition() == typeof(DialogViewBase<>)
					 || x.BaseType.GetGenericTypeDefinition() == typeof(WidgetViewBase<>)
					 || x.BaseType.GetGenericTypeDefinition() == typeof(FilterViewBase<>)
					 || x.BaseType.GetGenericTypeDefinition() == typeof(EntityTabViewBase<,>)
					 || (x.BaseType.GetGenericTypeDefinition() == typeof(ViewBase<>)
						&& (typeof(WidgetViewModelBase).IsAssignableFrom(x.BaseType.GetGenericArguments().First())
						|| typeof(ReportParametersViewModelBase).IsAssignableFrom(x.BaseType.GetGenericArguments().First())))))
				.ToDictionary(x => x, x => x.BaseType.GetGenericArguments().First())
				.ForEach(keyValue =>
				{
					_viewModelWidgetResolver.RegisterWidgetForWidgetViewModel(keyValue.Value, keyValue.Key);
					_logger.LogInformation("Зарегистрирована {ViewModel} для {Widget}", keyValue.Value, keyValue.Key);
				});
		}
	}
}
