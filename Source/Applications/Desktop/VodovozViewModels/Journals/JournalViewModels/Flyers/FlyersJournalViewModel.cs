using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalNodes.Flyers;
using Vodovoz.ViewModels.ViewModels.Flyers;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Flyers
{
	public class FlyersJournalViewModel : SingleEntityJournalViewModelBase<Flyer, FlyerViewModel, FlyersJournalNode>
	{
		private readonly INomenclatureJournalFactory _nomenclatureSelectorFactory;
		private readonly IFlyerRepository _flyerRepository;
		
		public FlyersJournalViewModel(
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			IFlyerRepository flyerRepository,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false
		) : base(uowFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			_nomenclatureSelectorFactory =
				nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));
			
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
			new FlyerViewModel(
				EntityUoWBuilder.ForCreate(),
				UnitOfWorkFactory,
				commonServices,
				_nomenclatureSelectorFactory,
				_flyerRepository);
		
		protected override Func<FlyersJournalNode, FlyerViewModel> OpenDialogFunction => n =>
			new FlyerViewModel(
				EntityUoWBuilder.ForOpen(n.Id),
				UnitOfWorkFactory,
				commonServices,
				_nomenclatureSelectorFactory,
				_flyerRepository);
	}
}
