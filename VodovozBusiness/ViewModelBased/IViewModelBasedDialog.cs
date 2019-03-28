using System;
using System.ComponentModel;
using QS.DomainModel.Entity;
using QS.Tdi;

namespace Vodovoz.ViewModelBased
{
	public interface IViewModelBaseView<TEntity> : ITdiDialog
	where TEntity : class, INotifyPropertyChanged, IDomainObject, new()
	{
	}

	public interface IViewModelBasedDialog<out TViewModel, TEntity> : IViewModelBaseView<TEntity>
		where TViewModel : ViewModel<TEntity>
		where TEntity : class, INotifyPropertyChanged, IDomainObject, new()
	{
		TViewModel ViewModel { get; }
	}
}
