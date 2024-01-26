using Autofac;
using Gtk;
using QS.Banks.Domain;
using QS.Deletion.Views;
using QS.Dialog.GtkUI;
using QS.HistoryLog;
using QS.Journal.GtkUI;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using QS.Views.Resolve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vodovoz.Infrastructure.Services;
using IJournalFilter = QS.RepresentationModel.IJournalFilter;

namespace Vodovoz.Core
{
	public class ViewModelWidgetResolver : ITDIWidgetResolver, IFilterWidgetResolver, IWidgetResolver, IGtkViewResolver
	{
		private readonly Dictionary<Type, Type> _viewModelWidgets = new Dictionary<Type, Type>();

		private ClassNamesBaseGtkViewResolver _classNamesBaseGtkViewResolver;

		private static ViewModelWidgetResolver _instance;

		private readonly ILifetimeScope _lifetimeScope = Startup.AppDIContainer; 

		public static ViewModelWidgetResolver Instance
		{
			get
			{
				if(_instance == null)
				{
					_instance = new ViewModelWidgetResolver();
				}
				return _instance;
			}
			set => _instance = value;
		}

		public ViewModelWidgetResolver()
		{
			var usedAssemblies = new Assembly[] {
				Assembly.GetAssembly(typeof(DeletionView)),
				Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
				Assembly.GetAssembly(typeof(Data.NHibernate.AssemblyFinder)),
				Assembly.GetAssembly(typeof(Bank)),
				Assembly.GetAssembly(typeof(HistoryMain)),
				Assembly.GetAssembly(typeof(MainWindow)),
				Assembly.GetAssembly(typeof(VodovozViewModelAssemblyFinder))
			};

			_classNamesBaseGtkViewResolver = new ClassNamesBaseGtkViewResolver(new ViewFactory(), usedAssemblies);
		}

		public virtual Widget Resolve(ITdiTab tab)
		{
			if(tab is Widget iTdiWidget)
			{
				return iTdiWidget;
			}

			if(tab is JournalViewModelBase journalTab)
			{
				return new JournalView(journalTab, this);
			}

			if(tab is EntityRepresentationSelectorAdapter)
			{
				return Resolve((tab as EntityRepresentationSelectorAdapter).JournalTab);
			}

			Type tabType = tab.GetType();

			if(!_viewModelWidgets.ContainsKey(tabType))
			{
				throw new WidgetResolveException($"Не настроено сопоставление для {tabType.Name}");
			}

			var constructor = _viewModelWidgets[tabType]
				.GetConstructors()
				.Where(x => x
					.GetParameters()
					.FirstOrDefault(p => p.ParameterType == tabType) != null)
				.FirstOrDefault();

			var constructorParameterTypes = constructor
				.GetParameters()
				.Select(cp => cp.ParameterType);

			var constructorParameters = new List<object>();

			foreach(var parameterType in constructorParameterTypes)
			{
				if(parameterType == tabType)
				{
					constructorParameters.Add(tab);
					continue;
				}

				constructorParameters.Add(_lifetimeScope.Resolve(parameterType));
			}

			var widget = (Widget)constructor.Invoke(constructorParameters.ToArray());

			constructorParameters.Clear();

			return widget;
		}

		public virtual Widget Resolve(IJournalFilter filter)
		{
			if(filter == null)
			{
				return null;
			}

			if(filter is Widget filterWidget)
			{
				return filterWidget;
			}

			Type filterType = filter.GetType();

			if(!_viewModelWidgets.ContainsKey(filterType))
			{
				throw new WidgetResolveException($"Не настроено сопоставление для {filterType.Name}");
			}

			var widgetCtorInfo = _viewModelWidgets[filterType].GetConstructor(new[] { filterType });
			Widget widget = (Widget)widgetCtorInfo.Invoke(new object[] { filter });
			return widget;
		}

		public virtual Widget Resolve(QS.Project.Journal.IJournalFilter filter)
		{
			if(filter == null)
			{
				return null;
			}

			if(filter is Widget filterWidget)
			{
				return filterWidget;
			}

			Type filterType = filter.GetType();

			if(!_viewModelWidgets.ContainsKey(filterType))
			{
				throw new WidgetResolveException($"Не настроено сопоставление для {filterType.Name}");
			}

			var widgetCtorInfo = _viewModelWidgets[filterType].GetConstructor(new[] { filterType });
			Widget widget = (Widget)widgetCtorInfo.Invoke(new object[] { filter });
			return widget;
		}

		public virtual Widget Resolve(ViewModelBase footer)
		{
			if(footer == null)
			{
				return null;
			}

			Type footerType = footer.GetType();
			if(!_viewModelWidgets.ContainsKey(footerType))
			{
				var result = _classNamesBaseGtkViewResolver.Resolve(footer);
				if(result != null)
				{
					return result;
				}
				throw new WidgetResolveException($"Не настроено сопоставление для {footerType.Name}");
			}

			var widgetCtorInfo = _viewModelWidgets[footerType].GetConstructor(new[] { footerType });
			Widget widget = (Widget)widgetCtorInfo.Invoke(new object[] { footer });
			return widget;
		}

		public virtual Widget Resolve(WidgetViewModelBase viewModel)
		{
			if(viewModel == null)
			{
				return null;
			}

			Type filterType = viewModel.GetType();
			if(!_viewModelWidgets.ContainsKey(filterType))
			{
				throw new WidgetResolveException($"Не настроено сопоставление для {filterType.Name}");
			}

			var widgetCtorInfo = _viewModelWidgets[filterType].GetConstructor(new[] { filterType });
			Widget widget = (Widget)widgetCtorInfo.Invoke(new object[] { viewModel });
			return widget;
		}

		public virtual ViewModelWidgetResolver RegisterWidgetForTabViewModel<TViewModel, TWidget>()
			where TViewModel : DialogViewModelBase
			where TWidget : Widget
		{
			Type viewModelType = typeof(TViewModel);
			Type widgetType = typeof(TWidget);
			if(_viewModelWidgets.ContainsKey(viewModelType))
			{
				throw new InvalidOperationException($"Модель представления {viewModelType.Name} уже зарегистрирована");
			}
			_viewModelWidgets.Add(viewModelType, widgetType);

			return this;
		}

		public virtual ViewModelWidgetResolver RegisterWidgetForWidgetViewModel<TViewModel, TWidget>()
			where TViewModel : ViewModelBase
			where TWidget : Widget
		{
			Type viewModelType = typeof(TViewModel);
			Type widgetType = typeof(TWidget);
			if(_viewModelWidgets.ContainsKey(viewModelType))
			{
				throw new InvalidOperationException($"Модель представления {viewModelType.Name} уже зарегистрирована");
			}
			_viewModelWidgets.Add(viewModelType, widgetType);

			return this;
		}
	}
}
