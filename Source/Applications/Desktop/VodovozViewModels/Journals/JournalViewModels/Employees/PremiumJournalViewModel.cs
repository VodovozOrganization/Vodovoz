using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Deletion;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using QS.Project.Journal.DataLoader;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Employees;
using QS.Project.DB;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Employees
{
	public class PremiumJournalViewModel : FilterableMultipleEntityJournalViewModelBase<PremiumJournalNode, PremiumJournalFilterViewModel>
	{
		private readonly ICommonServices _commonServices;
		private readonly IEmployeeService _employeeService;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IPremiumTemplateJournalFactory _premiumTemplateJournalFactory;
		public PremiumJournalViewModel(
			PremiumJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			IEmployeeJournalFactory employeeJournalFactory,
			IPremiumTemplateJournalFactory premiumTemplateJournalFactory,
			Action<PremiumJournalFilterViewModel> filterConfig = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_premiumTemplateJournalFactory = premiumTemplateJournalFactory
				?? throw new ArgumentNullException(nameof(premiumTemplateJournalFactory));

			TabName = "Журнал премий";

			RegisterPremiums();
			RegisterPremiumsRaskatGAZelle();

			FilterViewModel.JournalViewModel = this;

			var threadLoader = DataLoader as ThreadDataLoader<PremiumJournalNode>;
			threadLoader.MergeInOrderBy(x => x.Id, true);

			FinishJournalConfiguration();

			UpdateOnChanges(
				typeof(Premium),
				typeof(PremiumRaskatGAZelle),
				typeof(PremiumItem)
			);

			if(filterConfig != null)
			{
				FilterViewModel.SetAndRefilterAtOnce(filterConfig);
			}
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
			CreateCustomDeleteAction();
		}

		private void RegisterPremiums()
		{
			var premiumsConfig = RegisterEntity<Premium>(GetPremiumsQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => new PremiumViewModel(
						EntityUoWBuilder.ForCreate(),
						UnitOfWorkFactory,
						_commonServices,
						_employeeService,
						_employeeJournalFactory,
						_premiumTemplateJournalFactory
					),
					//функция диалога открытия документа
					(PremiumJournalNode node) => new PremiumViewModel(
						EntityUoWBuilder.ForOpen(node.Id),
						UnitOfWorkFactory,
						_commonServices,
						_employeeService,
						_employeeJournalFactory,
						_premiumTemplateJournalFactory
					),
					//функция идентификации документа 
					(PremiumJournalNode node) => node.EntityType == typeof(Premium),
					"Премия",
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = false }
				);

			//завершение конфигурации
			premiumsConfig.FinishConfiguration();
		}

		private void RegisterPremiumsRaskatGAZelle()
		{
			var premiumRaskatGAZelleConfig =
				RegisterEntity<PremiumRaskatGAZelle>(GetPremiumsRaskatGAZelleQuery)
				.AddDocumentConfigurationWithoutCreation(
				//функция диалога открытия документа
				(PremiumJournalNode node) => new PremiumRaskatGAZelleViewModel(
					EntityUoWBuilder.ForOpen(node.Id),
					UnitOfWorkFactory,
					_commonServices
				),
				//функция идентификации документа 
				(PremiumJournalNode node) => node.EntityType == typeof(PremiumRaskatGAZelle),
				new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = false }
				);

			//завершение конфигурации
			premiumRaskatGAZelleConfig.FinishConfiguration();
		}

		private IQueryOver<Premium> GetPremiumsQuery(IUnitOfWork uow)
		{
			PremiumJournalNode resultAlias = null;
			Premium premiumAlias = null;
			PremiumItem premiumItemAlias = null;
			Employee employeeAlias = null;

			var query = uow.Session.QueryOver<Premium>(() => premiumAlias)
				.JoinAlias(f => f.Items, () => premiumItemAlias)
				.JoinAlias(() => premiumItemAlias.Employee, () => employeeAlias);

			if(FilterViewModel.Subdivision != null)
			{
				query.Where(() => employeeAlias.Subdivision.Id == FilterViewModel.Subdivision.Id);
			}

			if(FilterViewModel.StartDate.HasValue)
			{
				query.Where(() => premiumAlias.Date >= FilterViewModel.StartDate.Value);
			}

			if(FilterViewModel.EndDate.HasValue)
			{
				query.Where(() => premiumAlias.Date <= FilterViewModel.EndDate.Value);
			}

			var employeeProjection = CustomProjections.Concat_WS(
				" ",
				() => employeeAlias.LastName,
				() => employeeAlias.Name,
				() => employeeAlias.Patronymic
			);

			query.Where(
					GetSearchCriterion(
						() => premiumAlias.Id,
						() => premiumAlias.PremiumReasonString,
						() => employeeProjection,
						() => premiumAlias.TotalMoney
				)
			);

			var resultQuery = query
				.SelectList(list => list
					.SelectGroup(() => premiumAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => premiumAlias.Date).WithAlias(() => resultAlias.Date)
					.Select(CustomProjections.GroupConcat(
						employeeProjection, 
						false,
						employeeProjection,
						OrderByDirection.Asc,
						"\n")
					).WithAlias(() => resultAlias.EmployeesName)
					.Select(() => premiumAlias.PremiumReasonString).WithAlias(() => resultAlias.PremiumReason)
					.Select(() => premiumAlias.TotalMoney).WithAlias(() => resultAlias.PremiumSum)
				).OrderBy(o => o.Date).Desc
			.TransformUsing(Transformers.AliasToBean<PremiumJournalNode<Premium>>());

			return resultQuery;
		}

		private IQueryOver<PremiumRaskatGAZelle> GetPremiumsRaskatGAZelleQuery(IUnitOfWork uow)
		{
			PremiumJournalNode resultAlias = null;
			PremiumRaskatGAZelle premiumRaskatGAZelleAlias = null;
			PremiumItem premiumItemAlias = null;
			Employee employeeAlias = null;

			var query = uow.Session.QueryOver<PremiumRaskatGAZelle>(() => premiumRaskatGAZelleAlias)
				.JoinAlias(f => f.Items, () => premiumItemAlias)
				.JoinAlias(() => premiumItemAlias.Employee, () => employeeAlias);

			if(FilterViewModel.Subdivision != null)
			{
				query.Where(() => employeeAlias.Subdivision.Id == FilterViewModel.Subdivision.Id);
			}

			if(FilterViewModel.StartDate.HasValue)
			{
				query.Where(() => premiumRaskatGAZelleAlias.Date >= FilterViewModel.StartDate.Value);
			}

			if(FilterViewModel.EndDate.HasValue)
			{
				query.Where(() => premiumRaskatGAZelleAlias.Date <= FilterViewModel.EndDate.Value);
			}

			var employeeProjection = CustomProjections.Concat_WS(
				" ",
				() => employeeAlias.LastName,
				() => employeeAlias.Name,
				() => employeeAlias.Patronymic
			);

			query.Where(
				GetSearchCriterion(
					() => premiumRaskatGAZelleAlias.Id,
					() => premiumRaskatGAZelleAlias.PremiumReasonString,
					() => employeeProjection,
					() => premiumRaskatGAZelleAlias.TotalMoney
				)
			);

			var resultQuery = query
				.SelectList(list => list
					.Select(() => premiumRaskatGAZelleAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => premiumRaskatGAZelleAlias.Date).WithAlias(() => resultAlias.Date)
					.Select(employeeProjection).WithAlias(() => resultAlias.EmployeesName)
					.Select(() => premiumRaskatGAZelleAlias.PremiumReasonString).WithAlias(() => resultAlias.PremiumReason)
					.Select(() => premiumRaskatGAZelleAlias.TotalMoney).WithAlias(() => resultAlias.PremiumSum)
				).OrderBy(o => o.Date).Desc
				.TransformUsing(Transformers.AliasToBean<PremiumJournalNode<PremiumRaskatGAZelle>>());

			return resultQuery;
		}

		private void CreateCustomDeleteAction()
		{
			var deleteAction = new JournalAction("Удалить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<PremiumJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					PremiumJournalNode selectedNode = selectedNodes.First();
					if(selectedNode.EntityType == typeof(PremiumRaskatGAZelle))
					{
						return false;
					}
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanDelete;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<PremiumJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					PremiumJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					if(config.PermissionResult.CanDelete)
					{
						DeleteHelper.DeleteEntity(selectedNode.EntityType, selectedNode.Id);
					}
				},
				"Delete"
			);
			NodeActionsList.Add(deleteAction);
		}
	}
}
