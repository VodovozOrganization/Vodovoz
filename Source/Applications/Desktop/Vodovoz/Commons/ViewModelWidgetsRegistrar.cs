using Microsoft.Extensions.Logging;
using MoreLinq;
using QS.Dialog.Gtk;
using QS.Project.Search;
using QS.Project.Search.GtkUI;
using QS.Report.ViewModels;
using QS.Report.Views;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using QS.Views;
using QS.Views.Dialog;
using QS.Views.GtkUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vodovoz.Core;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Documents;
using Vodovoz.Presentation.Views.Common;

namespace Vodovoz.Commons
{
	public sealed class ViewModelWidgetsRegistrar
	{
		private readonly ILogger<ViewModelWidgetsRegistrar> _logger;
		private readonly ViewModelWidgetResolver _viewModelWidgetResolver;
		private readonly List<Type> _viewModelsTypesList;

		public ViewModelWidgetsRegistrar(
			ILogger<ViewModelWidgetsRegistrar> logger,
			ViewModelWidgetResolver viewModelWidgetResolver)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_viewModelWidgetResolver = viewModelWidgetResolver
				?? throw new ArgumentNullException(nameof(viewModelWidgetResolver));

			_viewModelsTypesList = GetViewModels();
		}
		
		public void RegisterateWidgets(params Assembly[] assemblies)
		{
			ViewModelWidgetResolver.Instance = _viewModelWidgetResolver;

			_viewModelWidgetResolver.RegisterWidgetForTabViewModel<RdlViewerViewModel, RdlViewerView>()
				.RegisterWidgetForTabViewModel(typeof(PrintableRdlDocumentViewModel<>), typeof(RdlViewerView))
				.RegisterWidgetForWidgetViewModel<SearchViewModel, SearchView>();

			foreach(var assembly in assemblies)
			{
				assembly.GetTypes()
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
			}

			ConfigureFiltersWidgets(assemblies);
		}

		public void ConfigureFiltersWidgets(params Assembly[] assemblies)
		{
			DialogHelper.FilterWidgetResolver = _viewModelWidgetResolver;

			foreach(var assembly in assemblies)
			{
				assembly.GetTypes()
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
						var viewModelsForCurrentView = _viewModelsTypesList.Where(type => keyValue.Value.IsAssignableFrom(type)).ToList();

						foreach(var viewModel in viewModelsForCurrentView)
						{
							_viewModelWidgetResolver.RegisterWidgetForWidgetViewModel(viewModel, keyValue.Key);
							_logger.LogInformation("Зарегистрирована {ViewModel} для {Widget}", viewModel, keyValue.Key);
						}
					});
			}
		}

		private List<Type> GetViewModels()
		{
			return typeof(VodovozViewModelAssemblyFinder).Assembly.GetTypes()
				.Concat(typeof(ViewModelWidgetsRegistrar).Assembly.GetTypes())
				.Concat(typeof(Vodovoz.Presentation.ViewModels.AssemblyFinder).Assembly.GetTypes())
				.Where(type => type.IsClass
					&& !type.IsAbstract
					&& (typeof(ViewModelBase).IsAssignableFrom(type)
					 || typeof(ReportParametersViewModelBase).IsAssignableFrom(type)
					 || typeof(WindowDialogViewModelBase).IsAssignableFrom(type)))
				.ToList();
		}
	}
}
