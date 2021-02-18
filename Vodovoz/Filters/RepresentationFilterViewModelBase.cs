using System;
using QS.DomainModel.UoW;
using QS.Project.Filter;
using QS.Services;
using QSOrmProject.RepresentationModel;
using QS.RepresentationModel;

namespace Vodovoz.Filters
{
	public abstract class RepresentationFilterViewModelBase<TFilter> : FilterViewModelBase<TFilter>, IRepresentationFilter, IJournalFilter
		where TFilter : FilterViewModelBase<TFilter>
	{
		#region QSOrmProject.RepresentationModel.IRepresentationFilter implementation
		IUnitOfWork IRepresentationFilter.UoW => UoW;
		event EventHandler IRepresentationFilter.Refiltered {
			add => OnFiltered += value;
			remove => OnFiltered -= value;
		}
		#endregion

		#region QS.RepresentationModel.IJournalFilter implementation
		IUnitOfWork IJournalFilter.UoW => UoW;
		event EventHandler IJournalFilter.Refiltered {
			add => OnFiltered += value;
			remove => OnFiltered -= value;
		}
		#endregion
	}
}