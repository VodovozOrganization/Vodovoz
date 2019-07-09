using System;
using System.Collections.Generic;
using Gtk;
using QS.Dialog.GtkUI;
using QS.Project.Filter;
using QS.RepresentationModel;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.ViewModels;
using QS.RepresentationModel;
using QS.Dialog.GtkUI;
using QS.Project.Filter;
using QS.Journal.GtkUI;
using Vodovoz.Infrastructure.Services;

namespace Vodovoz.Core
{
	public class ViewModelWidgetResolver : ITDIWidgetResolver, IFilterWidgetResolver
	{
		private static ViewModelWidgetResolver instance;
		public static ViewModelWidgetResolver Instance {
			get {
				if(instance == null) {
					instance = new ViewModelWidgetResolver();
				}
				return instance; 
			}
		}

		private ViewModelWidgetResolver()
		{

		}

		private Dictionary<Type, Type> viewModelWidgets = new Dictionary<Type, Type>();

		public Widget Resolve(ITdiTab tab)
		{
			if(tab is Widget) {
				return (Widget)tab;
			}

			if(JournalViewFactory.TryCreateView(out Widget widget, tab)) {
				return widget;
			}

			if(tab is EntityRepresentationSelectorAdapter) {
				return Resolve((tab as EntityRepresentationSelectorAdapter).JournalTab);
			}

			Type tabType = tab.GetType();
			if(!viewModelWidgets.ContainsKey(tabType)) {
				throw new ApplicationException($"Не настроено сопоставление для {tabType.Name}");
			}

			var widgetCtorInfo = viewModelWidgets[tabType].GetConstructor(new[] { tabType });
			widget = (Widget)widgetCtorInfo.Invoke(new object[] { tab });
			return widget;
		}

		public Widget Resolve(IJournalFilter filter)
		{
			if(filter == null) {
				return null;
			}

			if(filter is Widget) {
				return (Widget)filter;
			}
			Type filterType = filter.GetType();
			if(!viewModelWidgets.ContainsKey(filterType)) {
				throw new ApplicationException($"Не настроено сопоставление для {filterType.Name}");
			}

			var widgetCtorInfo = viewModelWidgets[filterType].GetConstructor(new[] { filterType });
			Widget widget = (Widget)widgetCtorInfo.Invoke(new object[] { filter });
			return widget;
		}

		public Widget Resolve(QS.Project.Journal.IJournalFilter filter)
		{
			if(filter == null) {
				return null;
			}

			if(filter is Widget) {
				return (Widget)filter;
			}

			Type filterType = filter.GetType();
			if(!viewModelWidgets.ContainsKey(filterType)) {
				throw new ApplicationException($"Не настроено сопоставление для {filterType.Name}");
			}

			var widgetCtorInfo = viewModelWidgets[filterType].GetConstructor(new[] { filterType });
			Widget widget = (Widget)widgetCtorInfo.Invoke(new object[] { filter });
			return widget;
		}

		public ViewModelWidgetResolver RegisterWidgetForTabViewModel<TViewModel, TWidget>()
			where TViewModel : TabViewModelBase
			where TWidget : Widget
		{
			Type viewModelType = typeof(TViewModel);
			Type widgetType = typeof(TWidget);
			if(viewModelWidgets.ContainsKey(viewModelType)) {
				throw new InvalidOperationException($"Модель представления {viewModelType.Name} уже зарегистрирована");
			}
			viewModelWidgets.Add(viewModelType, widgetType);

			return this;
		}

		public ViewModelWidgetResolver RegisterWidgetForFilterViewModel<TViewModel, TWidget>()
			where TViewModel : FilterViewModelBase<TViewModel>
			where TWidget : Widget
		{
			Type viewModelType = typeof(TViewModel);
			Type widgetType = typeof(TWidget);
			if(viewModelWidgets.ContainsKey(viewModelType)) {
				throw new InvalidOperationException($"Модель представления {viewModelType.Name} уже зарегистрирована");
			}
			viewModelWidgets.Add(viewModelType, widgetType);

			return this;
		}
	}
}
