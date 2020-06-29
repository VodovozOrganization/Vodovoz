using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Journals.JournalNodes;

namespace Vodovoz.Journals.JournalViewModels.Logistic
{
	public class DistrictsJournalViewModel : SingleEntityJournalViewModelBase<District, EntityTabViewModelBase<District>, DistrictJournalNode>
	{
		public DistrictsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал районов доставки";
			UpdateOnChanges(typeof(District));
		}

		Func<ICriterion> restrictionFunc;
		public void SetRestriction(Func<ICriterion> restrictionFunc) => this.restrictionFunc = restrictionFunc;

		protected override Func<IUnitOfWork, IQueryOver<District>> ItemsSourceQueryFunction => uow => {
			DistrictJournalNode resultAlias = null;
			WageDistrict wageDistrictAlias = null;

			var query = uow.Session.QueryOver<District>()
								   .Left.JoinAlias(d => d.WageDistrict, () => wageDistrictAlias)
								   .SelectList(list => list
									  .Select(d => d.Id).WithAlias(() => resultAlias.Id)
									  .Select(d => d.DistrictName).WithAlias(() => resultAlias.Name)
									  .Select(() => wageDistrictAlias.Name).WithAlias(() => resultAlias.WageDistrict)
								   )
								   .TransformUsing(Transformers.AliasToBean<DistrictJournalNode>())
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

		protected override Func<EntityTabViewModelBase<District>> CreateDialogFunction => () => throw new NotImplementedException();

		protected override Func<DistrictJournalNode, EntityTabViewModelBase<District>> OpenDialogFunction => n => throw new NotImplementedException();
	}
}