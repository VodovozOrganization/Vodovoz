using System;
using Gtk;
using QS.Tdi;
using QS.Tdi.Gtk;
using System.Collections.Generic;
using Vodovoz.Infrastructure.ViewModels;
using Vodovoz.Filters;
using QS.RepresentationModel;
using QS.Dialog.GtkUI;

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
			Type tabType = tab.GetType();
			if(!viewModelWidgets.ContainsKey(tabType)) {
				throw new ApplicationException($"Не настроено сопоставление для {tabType.Name}");
			}

			var widgetCtorInfo = viewModelWidgets[tabType].GetConstructor(new[] { tabType });
			Widget widget = (Widget)widgetCtorInfo.Invoke(new object[] { tab });
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
