using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.RepresentationModel;
using QS.Tools;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Infrastructure.ViewModels;

namespace Vodovoz.Filters
{
	public abstract class FilterViewModelBase<TFilter> : WidgetViewModelBase, IQueryFilter, IDisposable, IJournalFilter,
		QSOrmProject.RepresentationModel.IRepresentationFilter,
		QS.RepresentationModel.GtkUI.IRepresentationFilter
		where TFilter : FilterViewModelBase<TFilter>
	{
		public event EventHandler Refiltered;

		IUnitOfWork uow;

		private bool canNotify = true;

		protected IList<ICriterion> Criterions { get; } = new List<ICriterion>();

		public IUnitOfWork UoW {
			get => uow;
			set {
				uow = value;
				ConfigureWithUow();
			}
		}

		protected FilterViewModelBase(IInteractiveService interactiveService) : base(interactiveService)
		{
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			PropertyChanged += (sender, e) => Refiltered?.Invoke(this, EventArgs.Empty);
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
			if(canNotify)
				Refiltered?.Invoke(this, new EventArgs());
		}

		public virtual ICriterion GetFilter()
		{
			ICriterion result = null;
			foreach(var rst in Criterions) {
				if(result == null)
					result = rst;
				else
					result = Restrictions.And(result, rst);
			}
			Criterions.Clear();
			return result;
		}

		protected virtual void ConfigureWithUow() { }

		public void Dispose() => UoW?.Dispose();
	}
}