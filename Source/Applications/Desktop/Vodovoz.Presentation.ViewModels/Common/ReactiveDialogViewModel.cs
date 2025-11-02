using QS.Navigation;
using QS.ViewModels.Dialog;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Reactive.Disposables;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public class ReactiveDialogViewModel : DialogViewModelBase, IReactiveObject, IDisposable
	{
		protected CompositeDisposable Subscriptions { get; }
		public event PropertyChangingEventHandler PropertyChanging;

		public ReactiveDialogViewModel(INavigationManager navigation) : base(navigation)
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
