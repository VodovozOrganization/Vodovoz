using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vodovoz.Core.Application.Entity;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Presentation.ViewModels.Employees.Journals
{
	public class InnerPhonesJournalViewModel : JournalViewModelBase
	{
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IPermissionResult _permissionResult;

		public InnerPhonesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			ICurrentPermissionService currentPermissionService,
			INavigationManager navigation) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));

			Title = "Внутренние телефоны";

			_permissionResult = _currentPermissionService.ValidateEntityPermission(typeof(InnerPhone));
			if(!_permissionResult.CanRead)
			{
				throw new AbortCreatingPageException("Отсутствуют права на просмотр внутренних телефонов", "");
			}

			DataLoader = new AnyDataLoader<InnerPhoneJournalNode>(GetItems);
			CreateNodeActions();
			UpdateOnChanges(typeof(InnerPhone));
		}

		protected IList<InnerPhoneJournalNode> GetItems(CancellationToken token)
		{
			InnerPhoneJournalNode resultAlias = null;

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var query = uow.Session.QueryOver<InnerPhone>()
					.SelectList(list => list
						.Select(x => x.PhoneNumber).WithAlias(() => resultAlias.Number)
						.Select(x => x.Description).WithAlias(() => resultAlias.Description)
					)
					.OrderBy(x => x.PhoneNumber).Asc
					.TransformUsing(Transformers.AliasToBean<InnerPhoneJournalNode>());
					
				query.Where(GetSearchCriterion<InnerPhone>(
					x => x.PhoneNumber,
					x => x.Description
				));

				return query.List<InnerPhoneJournalNode>();
			}
		}

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();

			bool canCreate = _permissionResult.CanCreate;
			bool canEdit = _permissionResult.CanUpdate;
			bool canDelete = _permissionResult.CanDelete;

			var addAction = new JournalAction("Добавить",
					(selected) => canCreate,
					(selected) => true,
					(selected) => CreateEntityDialog(),
					"Insert"
					);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
					(selected) => canEdit && selected.Count() == 1,
					(selected) => true,
					(selected) => EditEntityDialog(selected.Cast<InnerPhoneJournalNode>().FirstOrDefault())
					);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}

			var deleteAction = new JournalAction("Удалить",
					(selected) => false,
					(selected) => false,
					(selected) => DeleteEntities(selected.Cast<InnerPhoneJournalNode>()),
					"Delete"
					);
			NodeActionsList.Add(deleteAction);
		}

		protected virtual void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<InnerPhoneViewModel, IEntityIdentifier>(this, EntityIdentifier.NewEntity());
		}

		protected virtual void EditEntityDialog(InnerPhoneJournalNode node)
		{
			if(node == null)
			{
				return;
			}
			NavigationManager.OpenViewModel<InnerPhoneViewModel, IEntityIdentifier>(this, EntityIdentifier.OpenEntity(node.Number));
		}

		protected virtual void DeleteEntities(IEnumerable<InnerPhoneJournalNode> nodes)
		{
			throw new NotSupportedException("Не поддерживается удаление внутренних телефонов. " +
				"Необходима доработка сервиса удаления до возможности удаления сущностей не имеющих Int Id");
		}
	}
}
