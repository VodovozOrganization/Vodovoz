using System;
using Vodovoz.Infrastructure.ViewModels;
using QS.DomainModel.UoW;
using QS.Tools;
using NHibernate.Criterion;
using QS.RepresentationModel;
using Vodovoz.Infrastructure.Services;

namespace Vodovoz.Filters
{
	public abstract class FilterViewModelBase<TFilter> : WidgetViewModelBase, IQueryFilter, IDisposable, IJournalFilter, 
		QSOrmProject.RepresentationModel.IRepresentationFilter, 
		QS.RepresentationModel.GtkUI.IRepresentationFilter
		where TFilter : FilterViewModelBase<TFilter>
	{
		public event EventHandler Refiltered;

		public abstract ICriterion GetFilter();

		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				ConfigureWithUow();
			}
		}

		private bool canNotify = true;

		protected FilterViewModelBase(IInteractiveService interactiveService) : base(interactiveService)
		{
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			PropertyChanged += (sender, e) => {
				Refiltered?.Invoke(this, EventArgs.Empty);
			};
		}

		/// <summary>
		/// Для установки свойств фильтра без перезапуска фильтрации на каждом изменении
		/// обновления журналов при каждом выставлении ограничения.
		/// </summary>
		/// <param name="setters">Лямбды ограничений</param>
		public void SetAndRefilterAtOnce(params Action<TFilter>[] setters)
		{
			canNotify = false;
			TFilter filter = this as TFilter;
			foreach(var item in setters) {
				item(filter);
			}
			canNotify = true;
			OnRefiltered();
		}

		protected void OnRefiltered()
		{
			if(canNotify) {
				Refiltered?.Invoke(this, new EventArgs());
			}
		}

		protected virtual void ConfigureWithUow() { }

		public void Dispose()
		{
			if(UoW != null) {
				UoW.Dispose();
			}
		}

	}
}
