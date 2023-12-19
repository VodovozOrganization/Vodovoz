using System;
using Autofac;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Flyers;
using Vodovoz.ViewModels.ViewModels.Flyers;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Flyers
{
	public class FlyersJournalViewModel : SingleEntityJournalViewModelBase<Flyer, FlyerViewModel, FlyersJournalNode>
	{
		private ILifetimeScope _lifetimeScope;
		
		public FlyersJournalViewModel(
			ILifetimeScope lifetimeScope,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false
		) : base(uowFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			
			TabName = "Журнал рекламных листовок";
			
			UpdateOnChanges(
				typeof(Flyer),
				typeof(FlyerActionTime));
		}

		protected override Func<IUnitOfWork, IQueryOver<Flyer>> ItemsSourceQueryFunction => uow =>
		{
			Flyer flyerAlias = null;
			Nomenclature leafletNomenclatureAlias = null;
			FlyerActionTime flyerActionTimeAlias = null;
			FlyersJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => flyerAlias)
				.Left.JoinAlias(l => l.FlyerNomenclature, () => leafletNomenclatureAlias)
				.Left.JoinAlias(l => l.FlyerActionTimes, () => flyerActionTimeAlias);

			query.Where(GetSearchCriterion(
				() => leafletNomenclatureAlias.Name
			));
			
			var result = query.SelectList(list => list
					.Select(l => l.Id).WithAlias(() => resultAlias.Id)
					.Select(() => leafletNomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => flyerActionTimeAlias.StartDate).WithAlias(() => resultAlias.StartDate)
					.Select(() => flyerActionTimeAlias.EndDate).WithAlias(() => resultAlias.EndDate))
				.OrderBy(() => leafletNomenclatureAlias.Name).Asc
				.TransformUsing(Transformers.AliasToBean<FlyersJournalNode>());
			
			return result;
		};

		protected override Func<FlyerViewModel> CreateDialogFunction => () =>
			_lifetimeScope.Resolve<FlyerViewModel>(
				new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForCreate()));
		
		protected override Func<FlyersJournalNode, FlyerViewModel> OpenDialogFunction => n =>
			_lifetimeScope.Resolve<FlyerViewModel>(
				new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForOpen(n.Id)));

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
