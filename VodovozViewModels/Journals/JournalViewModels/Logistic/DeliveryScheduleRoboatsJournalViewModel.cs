using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.Factories;
using Vodovoz.ViewModels.Dialogs.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class DeliveryScheduleRoboatsJournalViewModel : SingleEntityJournalViewModelBase<DeliverySchedule, DeliveryScheduleViewModel, DeliveryScheduleJournalNode>
	{
		private readonly IDeliveryScheduleRepository _deliveryScheduleRepository;
		private readonly RoboatsViewModelFactory _roboatsViewModelFactory;

		public DeliveryScheduleRoboatsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false)
		: base(unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			Title = "Графики доставки";

			NotifyConfiguration.Instance.BatchSubscribeOnEntity(OnEntityChanged, typeof(DeliverySchedule));
		}

		public void OnEntityChanged(EntityChangeEvent[] changeEvents)
		{
			Refresh();
		}

		protected override void CreateNodeActions()
		{
			RowActivatedAction = new JournalAction("Выбрать", x => true, x => true, Select);
		}

		private void Select(object[] selectedNodes)
		{
			OnItemsSelected(selectedNodes, false);
		}

		protected override Func<IUnitOfWork, IQueryOver<DeliverySchedule>> ItemsSourceQueryFunction => (uow) =>
		{
			DeliverySchedule deliveryScheduleAlias = null;
			DeliveryScheduleJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => deliveryScheduleAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => deliveryScheduleAlias.Id,
				() => deliveryScheduleAlias.Name,
				() => deliveryScheduleAlias.From,
				() => deliveryScheduleAlias.To)
			);

			itemsQuery.SelectList(list => list
					.Select(() => deliveryScheduleAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => deliveryScheduleAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => deliveryScheduleAlias.From).WithAlias(() => resultAlias.TimeFrom)
					.Select(() => deliveryScheduleAlias.To).WithAlias(() => resultAlias.TimeTo)
					.Select(() => deliveryScheduleAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
					.Select(() => deliveryScheduleAlias.RoboatsAudiofile).WithAlias(() => resultAlias.RoboatsAudioFileName)
				)
				.OrderByAlias(() => deliveryScheduleAlias.RoboatsId).Desc
				.TransformUsing(Transformers.AliasToBean(typeof(DeliveryScheduleJournalNode)));

			return itemsQuery;
		};

		protected override Func<DeliveryScheduleViewModel> CreateDialogFunction => () => throw new NotSupportedException();

		protected override Func<DeliveryScheduleJournalNode, DeliveryScheduleViewModel> OpenDialogFunction => (node) => throw new NotSupportedException();
	}
}
