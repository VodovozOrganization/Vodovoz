using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using Vodovoz.ViewModels.ViewModels.Edo;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Edo
{
	public class EdoNotificationSettingJournalViewModel
		: EntityJournalViewModelBase<EdoNotificationSetting,
		EdoNotificationSettingViewModel,
		EdoNotificationSettingJournalNode>
	{
		public EdoNotificationSettingJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService
			)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService)
		{
			TabName = "Журнал настроек уведомлений для ЭДО";

			UpdateOnChanges(typeof(EdoNotificationSetting));

			var canChangeEdoNotificationSettings = currentPermissionService.ValidatePresetPermission(
				Core.Domain.Permissions.EdoPermissions.CanChangeEdoNotificationSettings);

			if(!canChangeEdoNotificationSettings)
			{
				AbortOpening("Недостаточно прав для просмотра настроек уведомлений для ЭДО");
			}
		}

		protected override IQueryOver<EdoNotificationSetting> ItemsQuery(IUnitOfWork uow)
		{
			EdoNotificationSetting edoNotificationSettingAlias = null;
			EdoNotificationSettingJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => edoNotificationSettingAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => edoNotificationSettingAlias.Id,
				() => edoNotificationSettingAlias.EdoNotificationType,
				() => edoNotificationSettingAlias.Template)
			);

			itemsQuery.SelectList(list => list
					.Select(() => edoNotificationSettingAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => edoNotificationSettingAlias.EdoNotificationType).WithAlias(() => resultAlias.EdoNotificationType)
					.Select(() => edoNotificationSettingAlias.Emails).WithAlias(() => resultAlias.Emails)
					.Select(() => edoNotificationSettingAlias.BitrixDialogs).WithAlias(() => resultAlias.BitrixDialogs)
					.Select(() => edoNotificationSettingAlias.Template).WithAlias(() => resultAlias.Template)
					.Select(() => edoNotificationSettingAlias.NotificationDisabled).WithAlias(() => resultAlias.NotificationDisabled)
					.Select(() => edoNotificationSettingAlias.AllowDuplicateNotifications).WithAlias(() => resultAlias.AllowDuplicateNotifications)
				)
				.TransformUsing(Transformers.AliasToBean<EdoNotificationSettingJournalNode>());

			return itemsQuery;
		}
	}
}
