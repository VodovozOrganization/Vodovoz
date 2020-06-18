using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using Vodovoz.Domain.Logistic;
using Vodovoz.FilterViewModels.Logistic;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.JournalViewModels.Logistic
{
	public class DistrictsSetJournalViewModel : FilterableSingleEntityJournalViewModelBase<DistrictsSet, DistrictsSetViewModel, DistrictsSetJournalNode, DistrictsSetJournalFilterViewModel>
	{
		public DistrictsSetJournalViewModel(DistrictsSetJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) 
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			
			TabName = "Журнал наборов районов";
		}

		private readonly IUnitOfWorkFactory unitOfWorkFactory;

		protected override Func<IUnitOfWork, IQueryOver<DistrictsSet>> ItemsSourceQueryFunction => uow => {
			DistrictsSet DistrictsSetAlias = null;
			DistrictsSetJournalNode resultAlias = null;
			
			var query = uow.Session.QueryOver<DistrictsSet>(() => DistrictsSetAlias);

			// if(FilterViewModel?.ExcludedDistrictsSet?.Any() ?? false) {
			// 	query.WhereRestrictionOn(() => DistrictsSetAlias.Id).Not.IsIn(FilterViewModel.ExcludedDistrictsSet);
			// }
			// if(FilterViewModel?.DistrictsSetType != null) {
			// 	query.Where(Restrictions.Eq(Projections.Property<DistrictsSet>(x => x.DistrictsSetType), FilterViewModel.DistrictsSetType));
			// }

			// query.Where(GetSearchCriterion(() => DistrictsSetAlias));

			return query
				.SelectList(list => list
				   .Select(x => x.Id).WithAlias(() => resultAlias.Id)
				   .Select(x => x.Name).WithAlias(() => resultAlias.Name)
				)
				.TransformUsing(Transformers.AliasToBean<DistrictsSetJournalNode>());
		};

		protected override Func<DistrictsSetViewModel> CreateDialogFunction => () => new DistrictsSetViewModel(EntityUoWBuilder.ForCreate(), unitOfWorkFactory, commonServices);

		protected override Func<DistrictsSetJournalNode, DistrictsSetViewModel> OpenDialogFunction => node => new DistrictsSetViewModel(EntityUoWBuilder.ForOpen(node.Id), unitOfWorkFactory, commonServices);
		
	}
}
