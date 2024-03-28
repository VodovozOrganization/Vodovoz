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
using Vodovoz.Core;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Commons
{
	//TODO доработать класс, чтобы нормально резолвились все вьюхи
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
		
		public void RegisterViews()
		{
			ViewModelWidgetResolver.Instance = _viewModelWidgetResolver;
			DialogHelper.FilterWidgetResolver = _viewModelWidgetResolver;
			
			_viewModelWidgetResolver.RegisterWidgetForTabViewModel<RdlViewerViewModel, RdlViewerView>()
				.RegisterWidgetForWidgetViewModel<SearchViewModel, SearchView>();

			typeof(ViewModelWidgetsRegistrar).Assembly.GetTypes()
				.Where(x => x.BaseType != null
							&& x.BaseType.IsGenericType
							&& (x.BaseType.GetGenericTypeDefinition() == typeof(TabViewBase<>)
								|| x.BaseType.GetGenericTypeDefinition() == typeof(DialogViewBase<>)
								|| x.BaseType.GetGenericTypeDefinition() == typeof(WidgetViewBase<>)
								|| x.BaseType.GetGenericTypeDefinition() == typeof(FilterViewBase<>)
								//|| x.BaseType.GetGenericTypeDefinition() == typeof(EntityTabViewBase<,>)
								|| (x.BaseType.GetGenericTypeDefinition() == typeof(ViewBase<>)
									&& (typeof(ViewModelBase).IsAssignableFrom(x.BaseType.GetGenericArguments().First())
									|| typeof(ReportParametersViewModelBase).IsAssignableFrom(x.BaseType.GetGenericArguments().First())))))
				.ToDictionary(x => x, x => x.BaseType.GetGenericArguments().First())
				.ForEach(keyValue =>
				{
					_viewModelWidgetResolver.RegisterViewModelForView(keyValue.Value, keyValue.Key);
					_logger.LogInformation("Зарегистрирована {ViewModel} для {View}", keyValue.Value, keyValue.Key);
				});
		}

		public void RegisterateWidgets()
		{
			ViewModelWidgetResolver.Instance = _viewModelWidgetResolver;

			_viewModelWidgetResolver.RegisterWidgetForTabViewModel<RdlViewerViewModel, RdlViewerView>()
				.RegisterWidgetForWidgetViewModel<SearchViewModel, SearchView>();

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

			var dict = typeof(ViewModelWidgetsRegistrar).Assembly.GetTypes()
				.Where(x => x.BaseType != null
					&& x.BaseType.IsGenericType
					&& (x.BaseType.GetGenericTypeDefinition() == typeof(DialogViewBase<>)
					 || x.BaseType.GetGenericTypeDefinition() == typeof(WidgetViewBase<>)
					 || x.BaseType.GetGenericTypeDefinition() == typeof(FilterViewBase<>)
					 || x.BaseType.GetGenericTypeDefinition() == typeof(EntityTabViewBase<,>)
					 || (x.BaseType.GetGenericTypeDefinition() == typeof(ViewBase<>)
						&& (typeof(WidgetViewModelBase).IsAssignableFrom(x.BaseType.GetGenericArguments().First())
						|| typeof(ReportParametersViewModelBase).IsAssignableFrom(x.BaseType.GetGenericArguments().First())))))
				.ToDictionary(x => x, x => x.BaseType.GetGenericArguments().First());
				
				
				dict.ForEach(keyValue =>
				{
					var viewModelsForCurrentView = _viewModelsTypesList.Where(type => keyValue.Value.IsAssignableFrom(type)).ToList();

					foreach(var viewModel in viewModelsForCurrentView)
					{
						_viewModelWidgetResolver.RegisterWidgetForWidgetViewModel(viewModel, keyValue.Key);
						_logger.LogInformation("Зарегистрирована {ViewModel} для {Widget}", viewModel, keyValue.Key);
					}
				});
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
