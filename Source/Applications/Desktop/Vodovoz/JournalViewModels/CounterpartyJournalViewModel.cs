using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Retail;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Dialogs.Counterparties;

namespace Vodovoz.JournalViewModels
{
	public class CounterpartyJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<Counterparty, CounterpartyDlg, CounterpartyJournalNode, CounterpartyJournalFilterViewModel>
	{
		private readonly bool _userHaveAccessToRetail;
		private readonly bool _canOpenCloseDeliveries;

		public CounterpartyJournalViewModel(
			CounterpartyJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			Action<CounterpartyJournalFilterViewModel> filterConfiguration = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices, navigation: navigationManager)
		{
			filterViewModel.Journal = this;

			TabName = "Журнал контрагентов";

			_userHaveAccessToRetail = commonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_to_retail");
			_canOpenCloseDeliveries =
				commonServices.CurrentPermissionService.ValidatePresetPermission("can_close_deliveries_for_counterparty");

			if(filterConfiguration != null)
			{
				FilterViewModel.ConfigureWithoutFiltering(filterConfiguration);
			}

			UpdateOnChanges(
				typeof(Counterparty),
				typeof(CounterpartyContract),
				typeof(Phone),
				typeof(Tag),
				typeof(DeliveryPoint)
			);

			SearchEnabled = false;
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateCustomSelectAction();
			CreateDefaultAddActions();
			CreateCustomEditAction();
			CreateDefaultDeleteAction();
			CreateOpenCloseSupplyAction();
		}

		private void CreateCustomEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => {
					var selectedNodes = selected.OfType<CounterpartyJournalNode>();
					if (selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					CounterpartyJournalNode selectedNode = selectedNodes.First();
					if (!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				(selected) => selected.All(x => (x as CounterpartyJournalNode).Sensitive),
				(selected) => {
					if(!selected.All(x => (x as CounterpartyJournalNode).Sensitive))
					{
						return;
					}
					var selectedNodes = selected.OfType<CounterpartyJournalNode>();
					if (selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					CounterpartyJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), this);
					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog)
					{
						HideJournal(TabParent);
					}
				}
			);
			if (SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		protected virtual void CreateCustomSelectAction()
		{
			var selectAction = new JournalAction("Выбрать",
				(selected) => selected.Any() && selected.All(x => (x as CounterpartyJournalNode).Sensitive),
				(selected) => SelectionMode != JournalSelectionMode.None,
				(selected) => { if (selected.All(x => (x as CounterpartyJournalNode).Sensitive)) { OnItemsSelected(selected); } }
			);
			if (SelectionMode == JournalSelectionMode.Single || SelectionMode == JournalSelectionMode.Multiple)
			{
				RowActivatedAction = selectAction;
			}
			NodeActionsList.Add(selectAction);
		}

		private void CreateOpenCloseSupplyAction()
		{
			var openCloseSupplyAction = new JournalAction("Закрыть/открыть поставки",
				//sensetive
				(selected) => {
					var selectedNodes = selected.OfType<CounterpartyJournalNode>();
					if(!_canOpenCloseDeliveries)
					{
						return false;
					}
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					CounterpartyJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanUpdate;
				},
				//visible
				(selected) => selected.All(x => (x as CounterpartyJournalNode).Sensitive),
				//execute
				(selected) => {
					if(!selected.All(x => (x as CounterpartyJournalNode).Sensitive))
					{
						return;
					}
					var selectedNodes = selected.OfType<CounterpartyJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					CounterpartyJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					var openClosePage = Startup.MainWin.NavigationManager.OpenViewModel<CloseSupplyToCounterpartyViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(selectedNode.Id));
				}
			);
			NodeActionsList.Add(openCloseSupplyAction);
		}

		protected override Func<IUnitOfWork, IQueryOver<Counterparty>> ItemsSourceQueryFunction => (uow) => {
			CounterpartyJournalNode resultAlias = null;
			Counterparty counterpartyAlias = null;
			Counterparty counterpartyAliasForSubquery = null;
			CounterpartyContract contractAlias = null;
			Phone phoneAlias = null;
			Phone deliveryPointPhoneAlias = null;
			DeliveryPoint addressAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Tag tagAliasForSubquery = null;
			SalesChannel salesChannelAlias = null;
			CounterpartyClassification counterpartyClassificationAlias = null;

			var counterpartyClassificationLastCalculationId = GetCounterpartyClassificationLastCalculationId(uow);

			var query = uow.Session.QueryOver<Counterparty>(() => counterpartyAlias);

			if (FilterViewModel != null && FilterViewModel.IsForRetail != null)
			{
				if (FilterViewModel.SalesChannels.Any(x => x.Selected))
				{
					query.Left.JoinAlias(c => c.SalesChannels, () => salesChannelAlias);
					query.Where(() => salesChannelAlias.Id.IsIn(FilterViewModel.SalesChannels.Where(x => x.Selected).Select(x => x.Id).ToArray()));
				}
			}

			if (FilterViewModel != null && !FilterViewModel.RestrictIncludeArchive) {
				query.Where(c => !c.IsArchive);
			}
			
			if(FilterViewModel.RestrictedRevenueStatuses.Any())
			{
				query.WhereRestrictionOn(() => counterpartyAlias.RevenueStatus).IsIn(FilterViewModel.RestrictedRevenueStatuses.ToArray());
			}
			else
			{
				query.Where(() => counterpartyAlias.RevenueStatus == null);
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyName))
			{
				query.Where(Restrictions.InsensitiveLike(Projections.Property(() => counterpartyAlias.Name),
					$"%{FilterViewModel.CounterpartyName}%"));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyPhone))
			{
				Phone counterpartyPhoneAlias = null;

				var counterpartyPhonesSubquery = QueryOver.Of<Phone>(() => counterpartyPhoneAlias)
					.Where(() => counterpartyPhoneAlias.Counterparty.Id == counterpartyAlias.Id)
					.And(() => counterpartyPhoneAlias.DigitsNumber == FilterViewModel.CounterpartyPhone)
					.Select(x => x.Id);

				query.Where(Subqueries.Exists(counterpartyPhonesSubquery.DetachedCriteria));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.DeliveryPointPhone))
			{
				query.Where(() => deliveryPointPhoneAlias.DigitsNumber == FilterViewModel.DeliveryPointPhone);
			}

			if(FilterViewModel?.CounterpartyType != null) {
				query.Where(t => t.CounterpartyType == FilterViewModel.CounterpartyType);
			}

			if(FilterViewModel?.ReasonForLeaving != null)
			{
				query.Where(c => c.ReasonForLeaving == FilterViewModel.ReasonForLeaving);
			}

			if(FilterViewModel?.IsNeedToSendBillByEdo == true)
			{
				query.Where(c => c.NeedSendBillByEdo);
			}

			if(FilterViewModel?.CounterpartyId != null)
			{
				query.Where(c => c.Id == FilterViewModel.CounterpartyId);
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel.CounterpartyContractNumber))
			{
				query.Where(Restrictions.Like(Projections.Property(() => contractAlias.Number),
					FilterViewModel.CounterpartyContractNumber,
					MatchMode.Anywhere));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyInn))
			{
				query.Where(c => c.INN == FilterViewModel.CounterpartyInn);
			}

			if(FilterViewModel?.CounterpartyClassification != null)
			{
				switch(FilterViewModel.CounterpartyClassification)
				{
					case (CounterpartyCompositeClassification.AX):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.A
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.X);
						break;
					case (CounterpartyCompositeClassification.AY):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.A
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.Y);
						break;
					case (CounterpartyCompositeClassification.AZ):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.A
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.Z);
						break;
					case (CounterpartyCompositeClassification.BX):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.B
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.X);
						break;
					case (CounterpartyCompositeClassification.BY):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.B
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.Y);
						break;
					case (CounterpartyCompositeClassification.BZ):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.B
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.Z);
						break;
					case (CounterpartyCompositeClassification.CX):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.C
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.X);
						break;
					case (CounterpartyCompositeClassification.CY):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.C
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.Y);
						break;
					case (CounterpartyCompositeClassification.CZ):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.C
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.Z);
						break;
					case (CounterpartyCompositeClassification.New):
						query.Where(() => counterpartyClassificationAlias.Id == null);
						break;
					default:
						throw new ArgumentException("Выбран неизвестный тип классификации контрагента");
				}
			}

			if(FilterViewModel != null
				&& FilterViewModel.ClientCameFrom != null
				&& !FilterViewModel.ClientCameFromIsEmpty)
			{
				query.Where(c => c.CameFrom.Id == FilterViewModel.ClientCameFrom.Id);
			}

			if(FilterViewModel != null && FilterViewModel.ClientCameFromIsEmpty)
			{
				query.Where(c => c.CameFrom == null);
			}

			query.Where(FilterViewModel?.SearchByAddressViewModel?.GetSearchCriterion(
				() => deliveryPointAlias.CompiledAddress
			));

			var contractsProjection = Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
					NHibernateUtil.String,
					Projections.Property(() => contractAlias.Number),
					Projections.Constant("\n"));

			var addressSubquery = QueryOver.Of<DeliveryPoint>(() => addressAlias)
				.Where(d => d.Counterparty.Id == counterpartyAlias.Id)
				.Where(() => addressAlias.IsActive)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(?1 SEPARATOR ?2)"),
					NHibernateUtil.String,
					Projections.Property(() => addressAlias.CompiledAddress),
					Projections.Constant("\n")));

			var tagsSubquery = QueryOver.Of<Counterparty>(() => counterpartyAliasForSubquery)
				.Where(() => counterpartyAlias.Id == counterpartyAliasForSubquery.Id)
				.JoinAlias(c => c.Tags, () => tagAliasForSubquery)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( CONCAT(' <span foreground=\"', ?1, '\"> ♥</span>', ?2) SEPARATOR '\n')"),
					NHibernateUtil.String,
					Projections.Property(() => tagAliasForSubquery.ColorText),
					Projections.Property(() => tagAliasForSubquery.Name)
				));

			if(FilterViewModel != null && FilterViewModel.Tag != null)
			{
				query.JoinAlias(c => c.Tags, () => tagAliasForSubquery)
					 .Where(() => tagAliasForSubquery.Id == FilterViewModel.Tag.Id);
			}

			if(FilterViewModel != null && FilterViewModel.IsForSalesDepartment != null)
			{
				query.Where(() => counterpartyAlias.IsForSalesDepartment == FilterViewModel.IsForSalesDepartment);
			}

			query
				.Left.JoinAlias(c => c.Phones, () => phoneAlias)
				.Left.JoinAlias(() => counterpartyAlias.DeliveryPoints, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias)
				.Left.JoinAlias(c => c.CounterpartyContracts, () => contractAlias)
				.JoinEntityAlias(
						() => counterpartyClassificationAlias,
						() => counterpartyAlias.Id == counterpartyClassificationAlias.CounterpartyId
							&& counterpartyClassificationAlias.ClassificationCalculationSettingsId == counterpartyClassificationLastCalculationId,
						JoinType.LeftOuterJoin);

			query.Where(GetSearchCriterion(
				() => counterpartyAlias.Name,
				() => counterpartyAlias.Id,
				() => counterpartyAlias.INN));

			var counterpartyResultQuery = query
				.SelectList(list => list
					.SelectGroup(c => c.Id).WithAlias(() => resultAlias.Id)
					.Select(c => c.Name).WithAlias(() => resultAlias.Name)
					.Select(c => c.INN).WithAlias(() => resultAlias.INN)
					.Select(c => c.IsArchive).WithAlias(() => resultAlias.IsArhive)
					.Select(c => c.RevenueStatus).WithAlias(() => resultAlias.RevenueStatus)
					.Select(contractsProjection).WithAlias(() => resultAlias.Contracts)
					.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
						NHibernateUtil.String,
						Projections.Property(() => phoneAlias.Number),
						Projections.Constant("\n"))
					).WithAlias(() => resultAlias.Phones)
					.Select(
						Projections.Conditional(
							Restrictions.Or(
								Restrictions.Eq(Projections.Constant(true), _userHaveAccessToRetail),
								Restrictions.Not(Restrictions.Eq(Projections.Property(() => counterpartyAlias.IsForRetail), true))
							),
							Projections.Constant(true),
							Projections.Constant(false)
						)).WithAlias(() => resultAlias.Sensitive
					)
					.SelectSubQuery(addressSubquery).WithAlias(() => resultAlias.Addresses)
					.SelectSubQuery(tagsSubquery).WithAlias(() => resultAlias.Tags)
					.Select(() => counterpartyClassificationAlias.ClassificationByBottlesCount).WithAlias(() => resultAlias.ClassificationByBottlesCount)
					.Select(() => counterpartyClassificationAlias.ClassificationByOrdersCount).WithAlias(() => resultAlias.ClassificationByOrdersCount)
				)
				.TransformUsing(Transformers.AliasToBean<CounterpartyJournalNode>());

			return counterpartyResultQuery;
		};
		
		protected override Func<IUnitOfWork, int> ItemsCountFunction => (uow) => {
			Counterparty counterpartyAlias = null;
			Counterparty counterpartyAliasForSubquery = null;
			CounterpartyContract contractAlias = null;
			Phone phoneAlias = null;
			Phone deliveryPointPhoneAlias = null;
			DeliveryPoint addressAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Tag tagAliasForSubquery = null;
			SalesChannel salesChannelAlias = null;
			CounterpartyClassification counterpartyClassificationAlias = null;

			var counterpartyClassificationLastCalculationId = GetCounterpartyClassificationLastCalculationId(uow);

			var query = uow.Session.QueryOver<Counterparty>(() => counterpartyAlias);

			if (FilterViewModel != null && FilterViewModel.IsForRetail != null)
			{
				if (FilterViewModel.SalesChannels.Any(x => x.Selected))
				{
					query.Left.JoinAlias(c => c.SalesChannels, () => salesChannelAlias);
					query.Where(() => salesChannelAlias.Id.IsIn(FilterViewModel.SalesChannels.Where(x => x.Selected).Select(x => x.Id).ToArray()));
				}
			}

			if (FilterViewModel != null && !FilterViewModel.RestrictIncludeArchive) {
				query.Where(c => !c.IsArchive);
			}

			if(FilterViewModel.RestrictedRevenueStatuses.Any())
			{
				query.WhereRestrictionOn(() => counterpartyAlias.RevenueStatus).IsIn(FilterViewModel.RestrictedRevenueStatuses.ToArray());
			}
			else
			{
				query.Where(() => counterpartyAlias.RevenueStatus == null);
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyName))
			{
				query.Where(Restrictions.InsensitiveLike(Projections.Property(() => counterpartyAlias.Name),
					$"%{FilterViewModel.CounterpartyName}%"));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyPhone))
			{
				Phone counterpartyPhoneAlias = null;

				var counterpartyPhonesSubquery = QueryOver.Of<Phone>(() => counterpartyPhoneAlias)
					.Where(() => counterpartyPhoneAlias.Counterparty.Id == counterpartyAlias.Id)
					.And(() => counterpartyPhoneAlias.DigitsNumber == FilterViewModel.CounterpartyPhone)
					.Select(x => x.Id);

				query.Where(Subqueries.Exists(counterpartyPhonesSubquery.DetachedCriteria));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.DeliveryPointPhone))
			{
				query.Where(() => deliveryPointPhoneAlias.DigitsNumber == FilterViewModel.DeliveryPointPhone);
			}

			if(FilterViewModel?.CounterpartyType != null) {
				query.Where(t => t.CounterpartyType == FilterViewModel.CounterpartyType);
			}

			if(FilterViewModel?.ReasonForLeaving != null)
			{
				query.Where(c => c.ReasonForLeaving == FilterViewModel.ReasonForLeaving);
			}

			if(FilterViewModel?.IsNeedToSendBillByEdo == true)
			{
				query.Where(c => c.NeedSendBillByEdo);
			}

			if(FilterViewModel?.CounterpartyId != null)
			{
				query.Where(c => c.Id == FilterViewModel.CounterpartyId);
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel.CounterpartyContractNumber))
			{
				query.Where(Restrictions.Like(Projections.Property(() => contractAlias.Number),
					FilterViewModel.CounterpartyContractNumber,
					MatchMode.Anywhere));
			}

			if(FilterViewModel?.CounterpartyInn != null)
			{
				query.Where(c => c.INN == FilterViewModel.CounterpartyInn);
			}

			if(FilterViewModel?.CounterpartyClassification != null)
			{
				switch(FilterViewModel.CounterpartyClassification)
				{
					case (CounterpartyCompositeClassification.AX):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.A
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.X);
						break;
					case (CounterpartyCompositeClassification.AY):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.A
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.Y);
						break;
					case (CounterpartyCompositeClassification.AZ):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.A
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.Z);
						break;
					case (CounterpartyCompositeClassification.BX):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.B
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.X);
						break;
					case (CounterpartyCompositeClassification.BY):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.B
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.Y);
						break;
					case (CounterpartyCompositeClassification.BZ):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.B
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.Z);
						break;
					case (CounterpartyCompositeClassification.CX):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.C
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.X);
						break;
					case (CounterpartyCompositeClassification.CY):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.C
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.Y);
						break;
					case (CounterpartyCompositeClassification.CZ):
						query.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == CounterpartyClassificationByBottlesCount.C
							&& counterpartyClassificationAlias.ClassificationByOrdersCount == CounterpartyClassificationByOrdersCount.Z);
						break;
					case (CounterpartyCompositeClassification.New):
						query.Where(() => counterpartyClassificationAlias.Id == null);
						break;
					default:
						throw new ArgumentException("Выбран неизвестный тип классификации контрагента");
				}
			}

			query.Where(FilterViewModel?.SearchByAddressViewModel?.GetSearchCriterion(
				() => deliveryPointAlias.CompiledAddress
			));

			if(FilterViewModel != null && FilterViewModel.Tag != null)
			{
				query.JoinAlias(c => c.Tags, () => tagAliasForSubquery)
					 .Where(() => tagAliasForSubquery.Id == FilterViewModel.Tag.Id);
			}

			if(FilterViewModel != null && FilterViewModel.IsForSalesDepartment != null)
			{
				query.Where(() => counterpartyAlias.IsForSalesDepartment == FilterViewModel.IsForSalesDepartment);
			}
			
			query.Where(GetSearchCriterion(
				() => counterpartyAlias.Name,
				() => counterpartyAlias.Id,
				() => counterpartyAlias.INN));

			query
				.Left.JoinAlias(c => c.Phones, () => phoneAlias)
				.Left.JoinAlias(() => counterpartyAlias.DeliveryPoints, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias)
				.Left.JoinAlias(c => c.CounterpartyContracts, () => contractAlias)
				.JoinEntityAlias(
						() => counterpartyClassificationAlias,
						() => counterpartyAlias.Id == counterpartyClassificationAlias.CounterpartyId
							&& counterpartyClassificationAlias.ClassificationCalculationSettingsId == counterpartyClassificationLastCalculationId,
						JoinType.LeftOuterJoin);

			var resultCountQuery = query
				.SelectList(list => list
					.Select(Projections.CountDistinct<Counterparty>(c => c.Id)))
				.SingleOrDefault<int>();

			return resultCountQuery;
		};

		protected override Func<CounterpartyDlg> CreateDialogFunction => () => new CounterpartyDlg();

		protected override Func<CounterpartyJournalNode, CounterpartyDlg> OpenDialogFunction => (node) => new CounterpartyDlg(node.Id);

		private int GetCounterpartyClassificationLastCalculationId(IUnitOfWork uow) => uow.GetAll<CounterpartyClassification>()
				.Select(c => c.ClassificationCalculationSettingsId)
				.OrderByDescending(c => c)
				.FirstOrDefault();

	}
}
