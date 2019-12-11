using System;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gtk;
using QS.ViewModels;

namespace Vodovoz.Core
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WidgetContainerView : Gtk.Bin
	{
		public BindingControler<WidgetContainerView> Binding { get; private set; }

		public IWidgetResolver WidgetResolver { get; set; } = ViewModelWidgetResolver.Instance;

		private WidgetViewModelBase widgetViewModel;
		public WidgetViewModelBase WidgetViewModel {
			get { return widgetViewModel; }
			set {
				if(widgetViewModel == value)
					return;
				widgetViewModel = value;
				Binding.FireChange(x => x.WidgetViewModel);
				OnViewModelUpdate();
			}
		}

		private Widget widget;
		public Widget Widget {
			get { return widget; }
			protected set {
				if(widget == value)
					return;
				widget?.Destroy();
				widget = value;
				Binding.FireChange(x => x.Widget);
			}
		}

		public WidgetContainerView()
		{
			this.Build();
			Binding = new BindingControler<WidgetContainerView>(this, new Expression<Func<WidgetContainerView, object>>[] {
				(w => w.WidgetViewModel),
				(w => w.Widget)
			});
		}

		protected void OnViewModelUpdate()
		{
			Widget = WidgetResolver.Resolve(WidgetViewModel);
			this.Add(Widget);
			this.ShowAll();
		}
	}
}
