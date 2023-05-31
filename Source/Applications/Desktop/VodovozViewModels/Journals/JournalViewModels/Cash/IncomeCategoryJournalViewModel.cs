using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Cash;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
	public class IncomeCategoryJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<
			IncomeCategory,
			IncomeCategoryViewModel,
			IncomeCategoryJournalNode,
			IncomeCategoryJournalFilterViewModel
		>
	{
		private readonly IFileChooserProvider _fileChooserProvider;
		private readonly INavigationManager _navigationManager;

		public IncomeCategoryJournalViewModel(
			IncomeCategoryJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IFileChooserProvider fileChooserProvider,
			INavigationManager navigationManager,
			Action<IncomeCategoryJournalFilterViewModel> filterConfigurationAction = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_fileChooserProvider = fileChooserProvider ?? throw new ArgumentNullException(nameof(fileChooserProvider));
			_navigationManager = navigationManager;
			TabName = "Категории прихода";

			if(filterConfigurationAction != null)
			{
				filterViewModel.SetAndRefilterAtOnce(filterConfigurationAction);
			}

			UpdateOnChanges(
				typeof(IncomeCategory),
				typeof(Subdivision)
			);
		}

		protected override Func<IUnitOfWork, IQueryOver<IncomeCategory>> ItemsSourceQueryFunction => (uow) =>
		{
			IncomeCategoryJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<IncomeCategory>();

			IncomeCategory level1Alias = null;
			IncomeCategory level2Alias = null;
			IncomeCategory level3Alias = null;
			IncomeCategory level4Alias = null;
			IncomeCategory level5Alias = null;
			Subdivision subdivisionAlias = null;
			// При цепочке связи:
			// 5 <- 4 <- 3 <- 2 <- 1
			// Уровни распределяются как
			// lvl1 - 5
			// lvl2 - 4|5
			// lvl3 - 3|4|5
			// lvl4 - 2|3|4|5
			// lvl5 - 1|2|3|4|5
			query = uow.Session.QueryOver<IncomeCategory>(() => level1Alias)
				.Left.JoinAlias(() => level1Alias.Parent, () => level2Alias)
				.Left.JoinAlias(() => level2Alias.Parent, () => level3Alias)
				.Left.JoinAlias(() => level3Alias.Parent, () => level4Alias)
				.Left.JoinAlias(() => level4Alias.Parent, () => level5Alias)
				.Left.JoinAlias(() => level1Alias.Subdivision, () => subdivisionAlias);

			if(!FilterViewModel.ShowArchive)
				query.Where(x => !x.IsArchive);
			switch(FilterViewModel.Level)
			{
				case LevelsFilter.Level1:
					query.Where(Restrictions.IsNull(Projections.Property(() => level2Alias.Id)));
					break;
				case LevelsFilter.Level2:
					query.Where(Restrictions.IsNull(Projections.Property(() => level3Alias.Id)));
					break;
				case LevelsFilter.Level3:
					query.Where(Restrictions.IsNull(Projections.Property(() => level4Alias.Id)));
					break;
				case LevelsFilter.Level4:
					query.Where(Restrictions.IsNull(Projections.Property(() => level5Alias.Id)));
					break;
			}

			query.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(() => level1Alias.Name).WithAlias(() => resultAlias.Level5)
					.Select(() => level2Alias.Name).WithAlias(() => resultAlias.Level4)
					.Select(() => level3Alias.Name).WithAlias(() => resultAlias.Level3)
					.Select(() => level4Alias.Name).WithAlias(() => resultAlias.Level2)
					.Select(() => level5Alias.Name).WithAlias(() => resultAlias.Level1)
					.Select(() => FilterViewModel.Level).WithAlias(() => resultAlias.LevelFilter)
					.Select(() => subdivisionAlias.ShortName).WithAlias(() => resultAlias.Subdivision)
					.Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive)
				).TransformUsing(Transformers.AliasToBean<IncomeCategoryJournalNode>())
				.OrderBy(x => x.Name);

			query.Where(
				GetSearchCriterion(
					() => level5Alias.Name,
					() => level4Alias.Name,
					() => level3Alias.Name,
					() => level2Alias.Name,
					() => level1Alias.Name,
					() => level5Alias.Id,
					() => level4Alias.Id,
					() => level3Alias.Id,
					() => level2Alias.Id,
					() => level1Alias.Id
				)
			);
			return query;
		};

		protected override Func<IncomeCategoryViewModel> CreateDialogFunction => () =>
			_navigationManager.OpenViewModel<IncomeCategoryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel;

		protected override Func<IncomeCategoryJournalNode, IncomeCategoryViewModel> OpenDialogFunction => node =>
			_navigationManager.OpenViewModel<IncomeCategoryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id)).ViewModel;

		protected override void CreatePopupActions()
		{
			base.CreatePopupActions();
			NodeActionsList.Add(new JournalAction(
				"Экспорт",
				x => true,
				x => true,
				selectedItems =>
				{
					var selectedNodes = selectedItems.Cast<IncomeCategoryJournalNode>();
					StringBuilder CSVbuilder = new StringBuilder();
					foreach(IncomeCategoryJournalNode incomeCategoryJournalNode in Items)
					{
						CSVbuilder.Append(incomeCategoryJournalNode.Level1 + ", ");
						CSVbuilder.Append(incomeCategoryJournalNode.Level2 + ", ");
						CSVbuilder.Append(incomeCategoryJournalNode.Level3 + ", ");
						CSVbuilder.Append(incomeCategoryJournalNode.Level4 + ", ");
						CSVbuilder.Append(incomeCategoryJournalNode.Level5 + ", ");
						CSVbuilder.Append(incomeCategoryJournalNode.Subdivision + "\n");
					}

					var fileChooserPath = _fileChooserProvider.GetExportFilePath($"Категории прихода {DateTime.Now.ToShortDateString()}");
					var res = CSVbuilder.ToString();
					if(fileChooserPath == "") return;
					Stream fileStream = new FileStream(fileChooserPath, FileMode.Create);
					using(StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.GetEncoding("Windows-1251")))
					{
						writer.Write("\"sep=,\"\n");
						writer.Write(res.ToString());
					}
					_fileChooserProvider.CloseWindow();
				})
			);

			PopupActionsList.Add(new JournalAction(
				"Архивировать",
				x => true,
				x => true,
				selectedItems =>
				{
					var selectedNodes = selectedItems.Cast<IncomeCategoryJournalNode>();
					var selectedNode = selectedNodes.FirstOrDefault();
					if(selectedNode != null)
					{
						selectedNode.IsArchive = true;
						using(var uow = UnitOfWorkFactory.CreateForRoot<IncomeCategory>(selectedNode.Id))
						{
							uow.Root.SetIsArchiveRecursively(true);
							uow.Save();
							uow.Commit();
						}
					}
				})
			);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateAddActions();
			CreateEditAction();
			CreateDefaultDeleteAction();
		}

		protected void CreateAddActions()
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
									createDlgConfig.OpenEntityDialogFunction();
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
						ITdiTab tab = docConfig.GetCreateEntityDlgConfigs().First().OpenEntityDialogFunction();

						if(tab is ITdiDialog)
							((ITdiDialog)tab).EntitySaved += Tab_EntitySaved;

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

		protected void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<IncomeCategoryJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					IncomeCategoryJournalNode selectedNode = selectedNodes.First();
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
					var selectedNodes = selected.OfType<IncomeCategoryJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					IncomeCategoryJournalNode selectedNode = selectedNodes.First();
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
	}
}
