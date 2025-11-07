using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class OnlineOrderNotificationSettingJournalViewModel : EntityJournalViewModelBase<OnlineOrderNotificationSetting,
		OnlineOrderNotificationSettingViewModel,
		OnlineOrderNotificationSettingJournalNode>
	{
		public OnlineOrderNotificationSettingJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService
			)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService)
		{
			TabName = "Журнал настроек уведомлений для онлайн заказов";

			UpdateOnChanges(typeof(OnlineOrderNotificationSetting));

			var canChangeOnlineOrderNotificationSettings = currentPermissionService.ValidatePresetPermission(
				Core.Domain.Permissions.PushNotificationPermissions.CanChangeOnlineOrderNotificationSettings);

			if(!canChangeOnlineOrderNotificationSettings)
			{
				AbortOpening("Недостаточно прав для просмотра настроек уведомлений для онлайн заказов");
			}
		}

		protected override IQueryOver<OnlineOrderNotificationSetting> ItemsQuery(IUnitOfWork uow)
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
		}
	}
}
