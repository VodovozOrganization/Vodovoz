using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Cash.Transfer.Journal
{
	public class TransferDocumentsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<DocumentNode, TransferDocumentsJournalFilterViewModel>
	{
		private readonly IDictionary<Type, IPermissionResult> _domainObjectsPermissions;

		private readonly ICashRepository _cashRepository;
		private readonly ICurrentPermissionService _currentPermissionService;

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		public TransferDocumentsJournalViewModel(
			TransferDocumentsJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ICashRepository cashRepository,
			ICurrentPermissionService currentPermissionService)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал перемещения д/с";

			DomainObjectsTypes = new[]
{
				typeof(IncomeCashTransferDocument),
				typeof(CommonCashTransferDocument),
			};

			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));

			_domainObjectsPermissions = InitializePermissionsMatrix(DomainObjectsTypes);

			UseSlider = false;

			Filter.HidenByDefault = true;

			UpdateOnChanges(DomainObjectsTypes);

			RegisterDocuments();

			UpdateAllEntityPermissions();

			CreateNodeActions();
			CreatePopupActions();

			DataLoader.DynamicLoadingEnabled = false;
			DataLoader.ItemsListUpdated += OnItemsUpdated;
		}

		private IDictionary<Type, IPermissionResult> InitializePermissionsMatrix(IEnumerable<Type> types)
		{
			var result = new Dictionary<Type, IPermissionResult>();

			foreach(var domainObject in types)
			{
				result.Add(domainObject, _currentPermissionService.ValidateEntityPermission(domainObject));
			}

			return result;
		}

		public Type[] DomainObjectsTypes { get; }

		private void OnItemsUpdated(object sender, EventArgs e)
		{
			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Items));

			OnPropertyChanged(nameof(FooterInfo));
		}

		public override string FooterInfo => $"{base.FooterInfo}. В сейфе инкасcатора: {_cashRepository.GetCashInTransferring(UoW):C}";

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateAddActions();
			CreateOpenDocumentAction();
		}

		private void CreateOpenDocumentAction()
		{
			var editAction = new JournalAction(
				"Открыть документ",
				(selected) =>
				{
					var selectedNodes = selected.OfType<DocumentNode>();

					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}

					DocumentNode selectedNode = selectedNodes.First();

					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}

					var config = EntityConfigs[selectedNode.EntityType];

					return config.PermissionResult.CanUpdate;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<DocumentNode>();

					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}

					DocumentNode selectedNode = selectedNodes.First();

					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}

					var config = EntityConfigs[selectedNode.EntityType];

					var foundDocumentConfig = config.EntityDocumentConfigurations
						.FirstOrDefault(x => x.IsIdentified(selectedNode));

					foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode);

					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog)
					{
						HideJournal(TabParent);
					}
				});

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}

			NodeActionsList.Add(editAction);
		}

		private void RegisterDocuments()
		{
			RegisterEntity(GetCommonTransferDocumentQuery)
				.AddDocumentConfiguration(
				() => null,
				(node) => NavigationManager
					.OpenViewModel<CommonCashTransferDocumentViewModel, IEntityUoWBuilder>(
					this,
					EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
				(node) => node.EntityType == typeof(CommonCashTransferDocument))
				.FinishConfiguration();


			RegisterEntity(GetIncomeTransferDocumentQuery)
				.AddDocumentConfiguration(
				() => null,
				(node) => NavigationManager
					.OpenViewModel<IncomeCashTransferDocumentViewModel, IEntityUoWBuilder>(
					this,
					EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
				(node) => node.EntityType == typeof(IncomeCashTransferDocument))
				.FinishConfiguration();

			var dataLoader = DataLoader as ThreadDataLoader<DocumentNode>;
			dataLoader.MergeInOrderBy(node => node.CreationDate, true);
		}

		private void CreateAddActions()
		{
			var addParentNodeAction = new JournalAction("Добавить", (selected) => true, (selected) => true, (selected) => { });

			foreach(var documentType in DomainObjectsTypes)
			{
				var incomeCreateNodeAction = new JournalAction(
				documentType.GetClassUserFriendlyName().Accusative.CapitalizeSentence(),
				(selected) => _domainObjectsPermissions[documentType].CanCreate,
				(selected) => _domainObjectsPermissions[documentType].CanCreate,
				(selected) =>
				{
					if(documentType == typeof(CommonCashTransferDocument))
					{
						NavigationManager.OpenViewModel<CommonCashTransferDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
					}

					if(documentType == typeof(IncomeCashTransferDocument))
					{
						NavigationManager.OpenViewModel<IncomeCashTransferDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
					}
				});

				addParentNodeAction.ChildActionsList.Add(incomeCreateNodeAction);
			}

			NodeActionsList.Add(addParentNodeAction);
		}

		#region Queries
		private IQueryOver<IncomeCashTransferDocument> GetIncomeTransferDocumentQuery(IUnitOfWork unitOfWork)
		{
			DocumentNode resultAlias = null;

			IncomeCashTransferDocument incomeTransferDocumentAlias = null;
			Employee authorAlias = null;
			Employee cashierSenderAlias = null;
			Employee cashierReceiverAlias = null;
			Subdivision subdivisionFromAlias = null;
			Subdivision subdivisionToAlias = null;

			var query = unitOfWork.Session.QueryOver(() => incomeTransferDocumentAlias);

			if(FilterViewModel.CashTransferDocumentStatus != null)
			{
				query.Where(ctd => ctd.Status == FilterViewModel.CashTransferDocumentStatus);
			}

			query.Where(GetSearchCriterion(
					() => incomeTransferDocumentAlias.Id,
					() => cashierSenderAlias.Name,
					() => cashierSenderAlias.LastName,
					() => cashierSenderAlias.Patronymic,
					() => incomeTransferDocumentAlias.CreationDate,
					() => cashierReceiverAlias.Name,
					() => cashierReceiverAlias.LastName,
					() => cashierReceiverAlias.Patronymic,
					() => incomeTransferDocumentAlias.TransferedSum,
					() => incomeTransferDocumentAlias.Comment));

			query.Left.JoinAlias(() => incomeTransferDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => incomeTransferDocumentAlias.CashierSender, () => cashierSenderAlias)
				.Left.JoinAlias(() => incomeTransferDocumentAlias.CashierReceiver, () => cashierReceiverAlias)
				.Left.JoinAlias(() => incomeTransferDocumentAlias.CashSubdivisionFrom, () => subdivisionFromAlias)
				.Left.JoinAlias(() => incomeTransferDocumentAlias.CashSubdivisionTo, () => subdivisionToAlias)
				.SelectList(list => list
					.Select(() => typeof(IncomeCashTransferDocument)).WithAlias(() => resultAlias.EntityType)
					.Select(() => incomeTransferDocumentAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => incomeTransferDocumentAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
					.Select(() => incomeTransferDocumentAlias.TransferedSum).WithAlias(() => resultAlias.TransferedSum)
					.Select(() => incomeTransferDocumentAlias.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => incomeTransferDocumentAlias.Status).WithAlias(() => resultAlias.Status)
					.Select(() => incomeTransferDocumentAlias.SendTime).WithAlias(() => resultAlias.SendTime)
					.Select(() => incomeTransferDocumentAlias.ReceiveTime).WithAlias(() => resultAlias.ReceiveTime)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => cashierSenderAlias.Name).WithAlias(() => resultAlias.CashierSenderName)
					.Select(() => cashierSenderAlias.LastName).WithAlias(() => resultAlias.CashierSenderSurname)
					.Select(() => cashierSenderAlias.Patronymic).WithAlias(() => resultAlias.CashierSenderPatronymic)
					.Select(() => cashierReceiverAlias.Name).WithAlias(() => resultAlias.CashierReceiverName)
					.Select(() => cashierReceiverAlias.LastName).WithAlias(() => resultAlias.CashierReceiverSurname)
					.Select(() => cashierReceiverAlias.Patronymic).WithAlias(() => resultAlias.CashierReceiverPatronymic)
					.Select(() => subdivisionFromAlias.Name).WithAlias(() => resultAlias.SubdivisionFrom)
					.Select(() => subdivisionToAlias.Name).WithAlias(() => resultAlias.SubdivisionTo));

			return query.OrderBy(x => x.CreationDate).Desc
				.TransformUsing(Transformers.AliasToBean<DocumentNode>());
		}

		public IQueryOver<CommonCashTransferDocument> GetCommonTransferDocumentQuery(IUnitOfWork unitOfWork)
		{
			DocumentNode resultAlias = null;

			CommonCashTransferDocument commonTransferDocumentAlias = null;
			Employee authorAlias = null;
			Employee cashierSenderAlias = null;
			Employee cashierReceiverAlias = null;
			Subdivision subdivisionFromAlias = null;
			Subdivision subdivisionToAlias = null;

			var query = unitOfWork.Session.QueryOver(() => commonTransferDocumentAlias);

			if(FilterViewModel.CashTransferDocumentStatus != null)
			{
				query.Where(ctd => ctd.Status == FilterViewModel.CashTransferDocumentStatus);
			}

			query.Where(GetSearchCriterion(
					() => commonTransferDocumentAlias.Id,
					() => cashierSenderAlias.Name,
					() => cashierSenderAlias.LastName,
					() => cashierSenderAlias.Patronymic,
					() => commonTransferDocumentAlias.CreationDate,
					() => cashierReceiverAlias.Name,
					() => cashierReceiverAlias.LastName,
					() => cashierReceiverAlias.Patronymic,
					() => commonTransferDocumentAlias.TransferedSum,
					() => commonTransferDocumentAlias.Comment));

			query.Left.JoinAlias(() => commonTransferDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => commonTransferDocumentAlias.CashierSender, () => cashierSenderAlias)
				.Left.JoinAlias(() => commonTransferDocumentAlias.CashierReceiver, () => cashierReceiverAlias)
				.Left.JoinAlias(() => commonTransferDocumentAlias.CashSubdivisionFrom, () => subdivisionFromAlias)
				.Left.JoinAlias(() => commonTransferDocumentAlias.CashSubdivisionTo, () => subdivisionToAlias)
				.SelectList(list => list
					.Select(() => typeof(CommonCashTransferDocument)).WithAlias(() => resultAlias.EntityType)
					.Select(() => commonTransferDocumentAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => commonTransferDocumentAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
					.Select(() => commonTransferDocumentAlias.TransferedSum).WithAlias(() => resultAlias.TransferedSum)
					.Select(() => commonTransferDocumentAlias.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => commonTransferDocumentAlias.Status).WithAlias(() => resultAlias.Status)
					.Select(() => commonTransferDocumentAlias.SendTime).WithAlias(() => resultAlias.SendTime)
					.Select(() => commonTransferDocumentAlias.ReceiveTime).WithAlias(() => resultAlias.ReceiveTime)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => cashierSenderAlias.Name).WithAlias(() => resultAlias.CashierSenderName)
					.Select(() => cashierSenderAlias.LastName).WithAlias(() => resultAlias.CashierSenderSurname)
					.Select(() => cashierSenderAlias.Patronymic).WithAlias(() => resultAlias.CashierSenderPatronymic)
					.Select(() => cashierReceiverAlias.Name).WithAlias(() => resultAlias.CashierReceiverName)
					.Select(() => cashierReceiverAlias.LastName).WithAlias(() => resultAlias.CashierReceiverSurname)
					.Select(() => cashierReceiverAlias.Patronymic).WithAlias(() => resultAlias.CashierReceiverPatronymic)
					.Select(() => subdivisionFromAlias.Name).WithAlias(() => resultAlias.SubdivisionFrom)
					.Select(() => subdivisionToAlias.Name).WithAlias(() => resultAlias.SubdivisionTo));

			return query.OrderBy(x => x.CreationDate).Desc
				.TransformUsing(Transformers.AliasToBean<DocumentNode>());
		}

		#endregion Queries
	}
}
