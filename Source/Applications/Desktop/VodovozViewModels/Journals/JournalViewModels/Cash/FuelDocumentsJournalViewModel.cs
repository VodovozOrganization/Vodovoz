using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using QS.Tdi;
using System;
using System.Linq;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.Journals.Nodes.Cash;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
	public class FuelDocumentsJournalViewModel : MultipleEntityJournalViewModelBase<FuelDocumentJournalNode>
	{
		private readonly IFuelRepository _fuelRepository;

		public FuelDocumentsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IFuelRepository fuelRepository,
			INavigationManager navigationManager) : base(unitOfWorkFactory, commonServices)
		{
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

			TabName = "Журнал учета топлива";

			var loader = new ThreadDataLoader<FuelDocumentJournalNode>(unitOfWorkFactory);
			loader.MergeInOrderBy(x => x.CreationDate, true);
			DataLoader = loader;

			RegisterIncomeInvoice();
			RegisterTransferDocument();
			RegisterWriteoffDocument();

			FinishJournalConfiguration();

			UpdateOnChanges(
				typeof(FuelIncomeInvoice),
				typeof(FuelIncomeInvoiceItem),
				typeof(FuelTransferDocument),
				typeof(FuelWriteoffDocument),
				typeof(FuelWriteoffDocumentItem));
		}

		public override string FooterInfo
		{
			get
			{
				var balance = _fuelRepository.GetAllFuelsBalance(UoW);
				string result = "";
				foreach(var item in balance)
				{
					result += $"{item.Key.Name}: {item.Value.ToString("0")} л., ";
				}
				result.Trim(' ', ',');
				return result;
			}
		}

		#region IncomeInvoice

		private IQueryOver<FuelIncomeInvoice> GetFuelIncomeQuery(IUnitOfWork uow)
		{
			FuelDocumentJournalNode resultAlias = null;
			FuelIncomeInvoice fuelIncomeInvoiceAlias = null;
			FuelIncomeInvoiceItem fuelIncomeInvoiceItemAlias = null;
			Employee authorAlias = null;
			Subdivision subdivisionToAlias = null;
			var fuelIncomeInvoiceQuery = uow.Session.QueryOver<FuelIncomeInvoice>(() => fuelIncomeInvoiceAlias)
				.Left.JoinQueryOver(() => fuelIncomeInvoiceAlias.Author, () => authorAlias)
				.Left.JoinQueryOver(() => fuelIncomeInvoiceAlias.Subdivision, () => subdivisionToAlias)
				.Left.JoinQueryOver(() => fuelIncomeInvoiceAlias.FuelIncomeInvoiceItems,
					() => fuelIncomeInvoiceItemAlias)
				.SelectList(list => list
					.SelectGroup(() => fuelIncomeInvoiceAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => fuelIncomeInvoiceAlias.СreationTime).WithAlias(() => resultAlias.CreationDate)
					.Select(() => fuelIncomeInvoiceAlias.Comment).WithAlias(() => resultAlias.Comment)
					.Select(Projections.Sum(Projections.Property(() => fuelIncomeInvoiceItemAlias.Liters)))
					.WithAlias(() => resultAlias.Liters)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)

					.Select(() => subdivisionToAlias.Name).WithAlias(() => resultAlias.SubdivisionTo)
				)
				.OrderBy(() => fuelIncomeInvoiceAlias.СreationTime).Desc()
				.TransformUsing(Transformers.AliasToBean<FuelDocumentJournalNode<FuelIncomeInvoice>>());

			fuelIncomeInvoiceQuery.Where(GetSearchCriterion(
				() => authorAlias.Name,
				() => authorAlias.LastName,
				() => authorAlias.Patronymic,
				() => fuelIncomeInvoiceAlias.Comment
			));

			return fuelIncomeInvoiceQuery;
		}

		private void RegisterIncomeInvoice()
		{
			var complaintConfig = RegisterEntity<FuelIncomeInvoice>(GetFuelIncomeQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => NavigationManager.OpenViewModel<FuelIncomeInvoiceViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel,
					//функция диалога открытия документа
					(FuelDocumentJournalNode node) => NavigationManager.OpenViewModel<FuelIncomeInvoiceViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
					//функция идентификации документа 
					(FuelDocumentJournalNode node) =>
					{
						return node.EntityType == typeof(FuelIncomeInvoice);
					},
					"Входящая накладная");

			//завершение конфигурации
			complaintConfig.FinishConfiguration();
		}

		#endregion IncomeInvoice

		#region TransferDocument

		private IQueryOver<FuelTransferDocument> GetTransferDocumentQuery(IUnitOfWork uow)
		{
			FuelDocumentJournalNode resultAlias = null;
			FuelTransferDocument fuelTransferAlias = null;
			Employee authorAlias = null;
			Subdivision subdivisionFromAlias = null;
			Subdivision subdivisionToAlias = null;
			var fuelTransferQuery = uow.Session.QueryOver<FuelTransferDocument>(() => fuelTransferAlias)
				.Left.JoinQueryOver(() => fuelTransferAlias.Author, () => authorAlias)
				.Left.JoinQueryOver(() => fuelTransferAlias.CashSubdivisionFrom, () => subdivisionFromAlias)
				.Left.JoinQueryOver(() => fuelTransferAlias.CashSubdivisionTo, () => subdivisionToAlias)
				.SelectList(list => list
					.Select(() => fuelTransferAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => fuelTransferAlias.CreationTime).WithAlias(() => resultAlias.CreationDate)
					.Select(() => fuelTransferAlias.Status).WithAlias(() => resultAlias.TransferDocumentStatus)
					.Select(() => fuelTransferAlias.TransferedLiters).WithAlias(() => resultAlias.Liters)
					.Select(() => fuelTransferAlias.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => fuelTransferAlias.SendTime).WithAlias(() => resultAlias.SendTime)
					.Select(() => fuelTransferAlias.ReceiveTime).WithAlias(() => resultAlias.ReceiveTime)

					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)

					.Select(() => subdivisionFromAlias.Name).WithAlias(() => resultAlias.SubdivisionFrom)
					.Select(() => subdivisionToAlias.Name).WithAlias(() => resultAlias.SubdivisionTo)
				)
				.OrderBy(() => fuelTransferAlias.CreationTime).Desc()
				.TransformUsing(Transformers.AliasToBean<FuelDocumentJournalNode<FuelTransferDocument>>());

			fuelTransferQuery.Where(GetSearchCriterion(
				() => authorAlias.Name,
				() => authorAlias.LastName,
				() => authorAlias.Patronymic,
				() => fuelTransferAlias.Comment));

			return fuelTransferQuery;
		}

		private void RegisterTransferDocument()
		{
			var complaintConfig = RegisterEntity<FuelTransferDocument>(GetTransferDocumentQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => NavigationManager.OpenViewModel<FuelTransferDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel,
					//функция диалога открытия документа
					(FuelDocumentJournalNode node) => NavigationManager.OpenViewModel<FuelTransferDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
					//функция идентификации документа 
					(FuelDocumentJournalNode node) =>
					{
						return node.EntityType == typeof(FuelTransferDocument);
					},
					"Перемещение");

			//завершение конфигурации
			complaintConfig.FinishConfiguration();
		}

		#endregion TransferDocument

		#region WriteoffDocument

		private IQueryOver<FuelWriteoffDocument> GetWriteoffDocumentQuery(IUnitOfWork uow)
		{
			FuelDocumentJournalNode resultAlias = null;
			FuelWriteoffDocument fuelWriteoffAlias = null;
			Employee cashierAlias = null;
			Employee employeeAlias = null;
			Subdivision subdivisionAlias = null;
			FinancialExpenseCategory financialExpenseCategoryAlias = null;
			FuelWriteoffDocumentItem fuelWriteoffItemAlias = null;
			var fuelWriteoffQuery = uow.Session.QueryOver<FuelWriteoffDocument>(() => fuelWriteoffAlias)
				.Left.JoinAlias(() => fuelWriteoffAlias.Cashier, () => cashierAlias)
				.Left.JoinAlias(() => fuelWriteoffAlias.Employee, () => employeeAlias)
				.Left.JoinAlias(() => fuelWriteoffAlias.CashSubdivision, () => subdivisionAlias)
				.JoinEntityAlias(
					() => financialExpenseCategoryAlias,
					() => fuelWriteoffAlias.ExpenseCategoryId == financialExpenseCategoryAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => fuelWriteoffAlias.FuelWriteoffDocumentItems, () => fuelWriteoffItemAlias)
				.SelectList(list => list
					.SelectGroup(() => fuelWriteoffAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => fuelWriteoffAlias.Date).WithAlias(() => resultAlias.CreationDate)
					.Select(() => fuelWriteoffAlias.Reason).WithAlias(() => resultAlias.Comment)

					.Select(() => cashierAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => cashierAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => cashierAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)

					.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmployeeName)
					.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
					.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)

					.Select(() => financialExpenseCategoryAlias.Title).WithAlias(() => resultAlias.ExpenseCategory)
					.Select(Projections.Sum(Projections.Property(() => fuelWriteoffItemAlias.Liters))).WithAlias(() => resultAlias.Liters)

					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.SubdivisionFrom))
				.OrderBy(() => fuelWriteoffAlias.Date).Desc()
				.TransformUsing(Transformers.AliasToBean<FuelDocumentJournalNode<FuelWriteoffDocument>>());

			fuelWriteoffQuery.Where(GetSearchCriterion(
				() => cashierAlias.Name,
				() => cashierAlias.LastName,
				() => cashierAlias.Patronymic,
				() => employeeAlias.Name,
				() => employeeAlias.LastName,
				() => employeeAlias.Patronymic,
				() => fuelWriteoffAlias.Reason));

			return fuelWriteoffQuery;
		}

		private void RegisterWriteoffDocument()
		{
			var complaintConfig = RegisterEntity<FuelWriteoffDocument>(GetWriteoffDocumentQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => NavigationManager.OpenViewModel<FuelWriteoffDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel,
					//функция диалога открытия документа
					(FuelDocumentJournalNode node) => NavigationManager.OpenViewModel<FuelWriteoffDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
					//функция идентификации документа 
					(FuelDocumentJournalNode node) =>
					{
						return node.EntityType == typeof(FuelWriteoffDocument);
					},
					"Акт выдачи топлива");

			//завершение конфигурации
			complaintConfig.FinishConfiguration();
		}

		#endregion WriteoffDocument

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateAddActions();
			CreateEditAction();
			CreateDefaultDeleteAction();
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<FuelDocumentJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					FuelDocumentJournalNode selectedNode = selectedNodes.First();
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
					var selectedNodes = selected.OfType<FuelDocumentJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					FuelDocumentJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode);
					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog)
					{
						HideJournal(TabParent);
					}
				}
			);
			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		private void CreateAddActions()
		{
			if(!EntityConfigs.Any())
			{
				return;
			}

			var totalCreateDialogConfigs = EntityConfigs
				.Where(x => x.Value.PermissionResult.CanCreate)
				.Sum(x => x.Value.EntityDocumentConfigurations
					.Select(y => y.GetCreateEntityDlgConfigs().Count())
					.Sum());

			if(EntityConfigs.Values.Count(x => x.PermissionResult.CanRead) > 1 || totalCreateDialogConfigs > 1)
			{
				var addParentNodeAction = new JournalAction("Добавить", (selected) => true, (selected) => true, (selected) => { });
				foreach(var entityConfig in EntityConfigs.Values)
				{
					foreach(var documentConfig in entityConfig.EntityDocumentConfigurations)
					{
						foreach(var createDlgConfig in documentConfig.GetCreateEntityDlgConfigs())
						{
							var childNodeAction = new JournalAction(createDlgConfig.Title,
								(selected) => entityConfig.PermissionResult.CanCreate,
								(selected) => entityConfig.PermissionResult.CanCreate,
								(selected) =>
								{
									createDlgConfig.OpenEntityDialogFunction.Invoke();

									if(documentConfig.JournalParameters.HideJournalForCreateDialog)
									{
										HideJournal(TabParent);
									}
								}
							);
							addParentNodeAction.ChildActionsList.Add(childNodeAction);
						}
					}
				}
				NodeActionsList.Add(addParentNodeAction);
			}
			else
			{
				var entityConfig = EntityConfigs.First().Value;
				var addAction = new JournalAction("Добавить",
					(selected) => entityConfig.PermissionResult.CanCreate,
					(selected) => entityConfig.PermissionResult.CanCreate,
					(selected) =>
					{
						var docConfig = entityConfig.EntityDocumentConfigurations.First();
						ITdiTab tab = docConfig.GetCreateEntityDlgConfigs().First().OpenEntityDialogFunction.Invoke();

						if(tab is ITdiDialog)
						{
							((ITdiDialog)tab).EntitySaved += Tab_EntitySaved;
						}

						if(docConfig.JournalParameters.HideJournalForCreateDialog)
						{
							HideJournal(TabParent);
						}
					},
					"Insert"
					);
				NodeActionsList.Add(addAction);
			};
		}
	}
}
