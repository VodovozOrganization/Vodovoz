using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class TariffZoneJournalViewModel : SingleEntityJournalViewModelBase <TariffZone, TariffZoneViewModel, TariffZoneJournalNode>
	{
		public TariffZoneJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал тарифных зон";

			UpdateOnChanges(typeof(TariffZone));
		}

		protected override Func<IUnitOfWork, IQueryOver<TariffZone>> ItemsSourceQueryFunction => (uow) =>
		{
			TariffZone tariffZoneAlias = null;
			TariffZoneJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => tariffZoneAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => tariffZoneAlias.Id,
				() => tariffZoneAlias.Name)
			);

			itemsQuery.SelectList(list => list
					.Select(() => tariffZoneAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => tariffZoneAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => tariffZoneAlias.IsFastDeliveryAvailable).WithAlias(() => resultAlias.IsFastDeliveryAvailable)
					.Select(() => tariffZoneAlias.FastDeliveryTimeFrom).WithAlias(() => resultAlias.FastDeliveryTimeFrom)
					.Select(() => tariffZoneAlias.FastDeliveryTimeTo).WithAlias(() => resultAlias.FastDeliveryTimeTo)
				)
				.OrderBy(() => tariffZoneAlias.Name).Asc
				.TransformUsing(Transformers.AliasToBean<TariffZoneJournalNode>());

			return itemsQuery;
		};

		protected override Func<TariffZoneViewModel> CreateDialogFunction => () =>
			new TariffZoneViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<TariffZoneJournalNode, TariffZoneViewModel> OpenDialogFunction =>
			(node) => new TariffZoneViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
