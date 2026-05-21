using System;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class OnlineOrderTemplatesJournalViewModel :
		EntityJournalViewModelBase<OnlineOrderTemplate, OnlineOrderTemplateViewModel, OnlineOrderTemplatesJournalNode>
	{
		private readonly OnlineOrderTemplatesJournalFilterViewModel _filterViewModel;
		private readonly IInteractiveService _interactiveService;
		private IPermissionResult _permissionResult;

		public OnlineOrderTemplatesJournalViewModel(
			OnlineOrderTemplatesJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService,
			Action<OnlineOrderTemplatesJournalFilterViewModel> filterParams = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_interactiveService = interactiveService;
			
			ConfigureFilter(filterParams);
			SearchEnabled = false;
		}

		protected override IQueryOver<OnlineOrderTemplate> ItemsQuery(IUnitOfWork uow)
		{
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			DeliverySchedule deliveryScheduleAlias = null;
			OnlineOrderTemplateWeekday weekdayAlias = null;
			OnlineOrderTemplatesJournalNode resultAlias = null;
			
			var query = uow.Session.QueryOver<OnlineOrderTemplate>()
				.Left.JoinAlias(t => t.CounterpartyId, () => counterpartyAlias)
				.Left.JoinAlias(t => t.DeliveryPointId, () => deliveryPointAlias)
				.Left.JoinAlias(t => t.DeliveryScheduleId, () => deliveryScheduleAlias)
				.Left.JoinAlias(t => t.Weekdays, () => weekdayAlias);

			if(_filterViewModel != null)
			{
				var templateId = _filterViewModel.TemplateId;
				if(templateId.HasValue)
				{
					query.Where(t => t.Id == templateId.Value);
				}

				if(_filterViewModel.Counterparty != null)
				{
					query.Where(t => t.CounterpartyId == _filterViewModel.Counterparty.Id);
				}

				if(_filterViewModel.DeliveryPoint != null)
				{
					query.Where(t => t.DeliveryPointId == _filterViewModel.DeliveryPoint.Id);
				}
				
				var paymentType = _filterViewModel.PaymentType;
				if(paymentType.HasValue)
				{
					query.Where(t => t.PaymentType == paymentType.Value);
				}
				
				var status = _filterViewModel.Status;
				if(status.HasValue)
				{
					if(_filterViewModel.Status == OnlineOrderTemplateStatus.Active)
					{
						query.Where(t => t.IsActive);
					}
					else
					{
						query.Where(t => !t.IsActive);
					}
				}

				if(!_filterViewModel.ShowArchived)
				{
					query.Where(t => !t.IsArchive);
				}
			}
			
			query.SelectList(list => list
				.SelectGroup(t => t.Id).WithAlias(() => resultAlias.Id)
				.Select(() => counterpartyAlias.FullName).WithAlias(() => resultAlias.CounterpartyName) //сделать проекцию имя + id
				.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.CompiledAddress) //сделать проекцию адрес + id
				.Select(() => deliveryScheduleAlias.Id).WithAlias(() => resultAlias.DeliveryTime) //сделать проекцию на время доставки
				.Select(t => t.IsSelfDelivery).WithAlias(() => resultAlias.IsSelfDelivery)
				.Select(t => t.ContactPhone).WithAlias(() => resultAlias.ContactPhone)
				.Select(t => t.DeliveryFrequency).WithAlias(() => resultAlias.DeliveryFrequency)
				.Select(t => t.PaymentType).WithAlias(() => resultAlias.PaymentType)
				.Select(t => t.IsActive).WithAlias(() => resultAlias.IsActive)
			)
			.TransformUsing(Transformers.AliasToBean<OnlineOrderTemplatesJournalNode>());
			
			return query;
		}

		protected override void CreateNodeActions()
		{
			_permissionResult = CurrentPermissionService.ValidateEntityPermission(typeof(OnlineOrderTemplate));
			
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateAddAction();
			CreateEditAction();
			CreateActiveInactiveAction();
			CreateArchiveAction();
		}

		private void CreateAddAction()
		{
			var canCreate = _permissionResult.CanCreate;
			
			var addAction = new JournalAction("Добавить",
				(selected) => canCreate,
				(selected) => VisibleCreateAction,
				(selected) => CreateEntityDialog(),
				"Insert"
			);
			NodeActionsList.Add(addAction);
		}

		private void CreateEditAction()
		{
			var canRead = _permissionResult.CanRead;
			
			var editAction = new JournalAction("Изменить",
				(selected) => canRead && selected.Any(),
				(selected) => VisibleEditAction,
				(selected) => selected.Cast<OnlineOrderTemplatesJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
		}

		private void CreateActiveInactiveAction()
		{
			var canUpdate = _permissionResult.CanUpdate;
			
			var activeInactiveAction = new JournalAction("Приостановить/Активировать",
				(selected) => canUpdate && selected.Any(),
				(selected) => true,
				(selected) =>
				{
					var changed = 0;

					foreach(OnlineOrderTemplatesJournalNode node in selected)
					{
						var template = UoW.GetById<OnlineOrderTemplate>(node.Id);
						template.UpdateState(!template.IsActive, template.IsArchive);
						UoW.Save(template);
						changed++;
					}

					if(changed > 0)
					{
						UoW.Commit();
					}
				});
			
			NodeActionsList.Add(activeInactiveAction);
		}

		private void CreateArchiveAction()
		{
			var canUpdate = _permissionResult.CanUpdate;
			
			var archiveAction = new JournalAction("Архивировать",
				(selected) => canUpdate && selected.Any(),
				(selected) => true,
				(selected) =>
				{
					if(!_interactiveService.Question("Архивирование нельзя будет отменить. Вы уверены?"))
					{
						return;
					}

					var changed = 0;

					foreach(OnlineOrderTemplatesJournalNode node in selected)
					{
						if(!node.IsArchive)
						{
							var template = UoW.GetById<OnlineOrderTemplate>(node.Id);
							template.UpdateState(false, true);
							UoW.Save(template);
							changed++;
						}
					}

					if(changed > 0)
					{
						UoW.Commit();
					}
				});
			
			NodeActionsList.Add(archiveAction);
		}

		private void ConfigureFilter(Action<OnlineOrderTemplatesJournalFilterViewModel> filterParams)
		{
			if(_filterViewModel is null) return;
			_filterViewModel.SetJournal(this);
			filterParams?.Invoke(_filterViewModel);
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}
		
		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}
	}
}
