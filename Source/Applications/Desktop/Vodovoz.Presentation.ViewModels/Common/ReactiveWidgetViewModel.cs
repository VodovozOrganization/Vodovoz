using QS.ViewModels;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Reactive.Disposables;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public class ReactiveWidgetViewModel : WidgetViewModelBase, IReactiveObject, IDisposable
	{
		protected CompositeDisposable Subscriptions { get; }
		public event PropertyChangingEventHandler PropertyChanging;

		public ReactiveWidgetViewModel()
		{
			Subscriptions = new CompositeDisposable();
		}

		void IReactiveObject.RaisePropertyChanged(PropertyChangedEventArgs args)
		{
			RaisePropertyChanged(args);
		}

		public void RaisePropertyChanging(PropertyChangingEventArgs args)
		{
			PropertyChanging?.Invoke(this, args);
		}

		public virtual void Dispose()
		{
			Subscriptions.Dispose();
		}
	}
}
