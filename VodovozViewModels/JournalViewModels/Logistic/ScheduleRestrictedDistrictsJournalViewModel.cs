using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.Search;
using QS.Project.Journal.Search.Criterion;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.JournalNodes;

namespace Vodovoz.JournalViewModels.Logistic
{
	public class ScheduleRestrictedDistrictsJournalViewModel : SingleEntityJournalViewModelBase<ScheduleRestrictedDistrict, EntityTabViewModelBase<ScheduleRestrictedDistrict>, ScheduleRestrictedDistrictJournalNode>
	{
		public ScheduleRestrictedDistrictsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, ICriterionSearch criterionSearch) : base(unitOfWorkFactory, commonServices, criterionSearch)
		{
			TabName = "Журнал районов доставки";
			UpdateOnChanges(typeof(ScheduleRestrictedDistrict));
		}

		Func<ICriterion> restrictionFunc;
		public void SetRestriction(Func<ICriterion> restrictionFunc) => this.restrictionFunc = restrictionFunc;

		protected override Func<IUnitOfWork, IQueryOver<ScheduleRestrictedDistrict>> ItemsSourceQueryFunction => uow => {
			ScheduleRestrictedDistrictJournalNode resultAlias = null;
			WageDistrict wageDistrictAlias = null;

			var query = uow.Session.QueryOver<ScheduleRestrictedDistrict>()
								   .Left.JoinAlias(d => d.WageDistrict, () => wageDistrictAlias)
								   .SelectList(list => list
									  .Select(d => d.Id).WithAlias(() => resultAlias.Id)
									  .Select(d => d.DistrictName).WithAlias(() => resultAlias.Name)
									  .Select(() => wageDistrictAlias.Name).WithAlias(() => resultAlias.WageDistrict)
								   )
								   .TransformUsing(Transformers.AliasToBean<ScheduleRestrictedDistrictJournalNode>())
								   ;

			if(restrictionFunc != null)
				query.Where(restrictionFunc.Invoke());

			return query;
		};

		public void HideCreateAndOpenBtns()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
		}

		protected override Func<EntityTabViewModelBase<ScheduleRestrictedDistrict>> CreateDialogFunction => () => throw new NotImplementedException();

		protected override Func<ScheduleRestrictedDistrictJournalNode, EntityTabViewModelBase<ScheduleRestrictedDistrict>> OpenDialogFunction => n => throw new NotImplementedException();
	}
}