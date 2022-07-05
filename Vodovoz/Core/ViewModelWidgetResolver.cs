using System;
using System.Collections.Generic;
using Gtk;
using QS.Dialog.GtkUI;
using QS.Journal.GtkUI;
using QS.Project.Filter;
using QS.RepresentationModel;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.ViewModels;
using QS.Views.Resolve;
using Vodovoz.Infrastructure.Services;

namespace Vodovoz.Core
{
	public class ViewModelWidgetResolver : ITDIWidgetResolver, IFilterWidgetResolver, IWidgetResolver, IGtkViewResolver 
	{
		private static ViewModelWidgetResolver instance;
		public static ViewModelWidgetResolver Instance {
			get {
				if(instance == null) {
					instance = new ViewModelWidgetResolver();
				}
				return instance; 
			}
			set => instance = value;
		}

		public ViewModelWidgetResolver()
		{
		}

		private Dictionary<Type, Type> viewModelWidgets = new Dictionary<Type, Type>();

		public virtual Widget Resolve(ITdiTab tab)
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
				throw new WidgetResolveException($"Не настроено сопоставление для {tabType.Name}");
			}

			var widgetCtorInfo = viewModelWidgets[tabType].GetConstructor(new[] { tabType });
			widget = (Widget)widgetCtorInfo.Invoke(new object[] { tab });
			return widget;
		}

		public virtual Widget Resolve(IJournalFilter filter)
		{
			if(filter == null) {
				return null;
			}

			if(filter is Widget) {
				return (Widget)filter;
			}
			Type filterType = filter.GetType();
			if(!viewModelWidgets.ContainsKey(filterType)) {
				throw new WidgetResolveException($"Не настроено сопоставление для {filterType.Name}");
			}

			var widgetCtorInfo = viewModelWidgets[filterType].GetConstructor(new[] { filterType });
			Widget widget = (Widget)widgetCtorInfo.Invoke(new object[] { filter });
			return widget;
		}

		public virtual Widget Resolve(QS.Project.Journal.IJournalFilter filter)
		{
			if(filter == null) {
				return null;
			}

			if(filter is Widget) {
				return (Widget)filter;
			}

			Type filterType = filter.GetType();
			if(!viewModelWidgets.ContainsKey(filterType)) {
				throw new WidgetResolveException($"Не настроено сопоставление для {filterType.Name}");
			}

			var widgetCtorInfo = viewModelWidgets[filterType].GetConstructor(new[] { filterType });
			Widget widget = (Widget)widgetCtorInfo.Invoke(new object[] { filter });
			return widget;
		}

		public virtual Widget Resolve(ViewModelBase footer)
		{
			if(footer == null)
				return null;

			Type footerType = footer.GetType();
			if(!viewModelWidgets.ContainsKey(footerType)) {
				throw new WidgetResolveException($"Не настроено сопоставление для {footerType.Name}");
			}

			var widgetCtorInfo = viewModelWidgets[footerType].GetConstructor(new[] { footerType });
			Widget widget = (Widget)widgetCtorInfo.Invoke(new object[] { footer });
			return widget;
		}

		public virtual Widget Resolve(WidgetViewModelBase viewModel)
		{
			if(viewModel == null)
				return null;

			Type filterType = viewModel.GetType();
			if(!viewModelWidgets.ContainsKey(filterType)) {
				throw new WidgetResolveException($"Не настроено сопоставление для {filterType.Name}");
			}

			var widgetCtorInfo = viewModelWidgets[filterType].GetConstructor(new[] { filterType });
			Widget widget = (Widget)widgetCtorInfo.Invoke(new object[] { viewModel });
			return widget;
		}

		public virtual ViewModelWidgetResolver RegisterWidgetForTabViewModel<TViewModel, TWidget>()
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

		public virtual ViewModelWidgetResolver RegisterWidgetForWidgetViewModel<TViewModel, TWidget>()
			where TViewModel : ViewModelBase
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
