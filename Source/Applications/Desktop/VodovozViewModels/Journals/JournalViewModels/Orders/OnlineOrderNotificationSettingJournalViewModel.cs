using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class OnlineOrderNotificationSettingJournalViewModel : SingleEntityJournalViewModelBase<OnlineOrderNotificationSetting,
		OnlineOrderNotificationSettingViewModel,
		OnlineOrderNotificationSettingJournalNode>
	{
		public OnlineOrderNotificationSettingJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал настроек уведомлений для онлайн заказов";

			UpdateOnChanges(typeof(OnlineOrderNotificationSetting));
		}

		protected override Func<IUnitOfWork, IQueryOver<OnlineOrderNotificationSetting>> ItemsSourceQueryFunction => (uow) =>
		{
			OnlineOrderNotificationSetting onlineOrderNotificationSettingAlias = null;
			OnlineOrderNotificationSettingJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => onlineOrderNotificationSettingAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => onlineOrderNotificationSettingAlias.Id,
				() => onlineOrderNotificationSettingAlias.ExternalOrderStatus,
				() => onlineOrderNotificationSettingAlias.NotificationText)
			);

			itemsQuery.SelectList(list => list
					.Select(() => onlineOrderNotificationSettingAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => onlineOrderNotificationSettingAlias.ExternalOrderStatus).WithAlias(() => resultAlias.ExternalOrderStatus)
					.Select(() => onlineOrderNotificationSettingAlias.NotificationText).WithAlias(() => resultAlias.NotificationText)
				)
				.TransformUsing(Transformers.AliasToBean<OnlineOrderNotificationSettingJournalNode>());

			return itemsQuery;
		};

		protected override Func<OnlineOrderNotificationSettingViewModel> CreateDialogFunction => () =>
			new OnlineOrderNotificationSettingViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<OnlineOrderNotificationSettingJournalNode, OnlineOrderNotificationSettingViewModel> OpenDialogFunction =>
			(node) => new OnlineOrderNotificationSettingViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
