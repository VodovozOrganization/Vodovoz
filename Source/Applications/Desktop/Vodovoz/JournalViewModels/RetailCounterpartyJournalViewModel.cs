using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using Vodovoz.Domain.Contacts;
using QS.DomainModel.UoW;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using QS.Project.Journal;
using Vodovoz.Domain.Retail;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using QS.Project.Domain;
using QS.Navigation;

namespace Vodovoz.JournalViewModels
{
	public class RetailCounterpartyJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<Counterparty, CounterpartyDlg, RetailCounterpartyJournalNode, CounterpartyJournalFilterViewModel>
	{
		private readonly bool _canOpenCloseDeliveries;

		public RetailCounterpartyJournalViewModel(
			CounterpartyJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			Action<CounterpartyJournalFilterViewModel> filterConfig = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices, navigation: navigationManager)
		{
			filterViewModel.Journal = this;

			TabName = "Журнал контрагентов";
			
			_canOpenCloseDeliveries =
				commonServices.CurrentPermissionService.ValidatePresetPermission("can_close_deliveries_for_counterparty");

			if(filterConfig != null)
			{
				FilterViewModel.ConfigureWithoutFiltering(filterConfig);
			}

			UpdateOnChanges(
				typeof(Counterparty),
				typeof(CounterpartyContract),
				typeof(Phone),
				typeof(Tag),
				typeof(DeliveryPoint));

			SearchEnabled = false;
		}

		protected override Func<IUnitOfWork, IQueryOver<Counterparty>> ItemsSourceQueryFunction => (uow) => {
			RetailCounterpartyJournalNode resultAlias = null;
			Counterparty counterpartyAlias = null;
			Counterparty counterpartyAliasForSubquery = null;
			CounterpartyContract contractAlias = null;
			Phone phoneAlias = null;
			Phone deliveryPointPhoneAlias = null;
			DeliveryPoint addressAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Tag tagAliasForSubquery = null;
			SalesChannel salesChannelAlias = null;

			var query = uow.Session.QueryOver<Counterparty>(() => counterpartyAlias);

			if (FilterViewModel != null && FilterViewModel.IsForRetail != null)
			{
				query.Where(c => c.IsForRetail == FilterViewModel.IsForRetail);

				if (FilterViewModel.SalesChannels.Any(x => x.Selected))
				{
					query.Left.JoinAlias(c => c.SalesChannels, () => salesChannelAlias);
					query.Where(() => salesChannelAlias.Id.IsIn(FilterViewModel.SalesChannels.Where(x => x.Selected).Select(x => x.Id).ToArray()));
				}
			}

			if (FilterViewModel != null && !FilterViewModel.RestrictIncludeArchive)
			{
				query.Where(c => !c.IsArchive);
			}

			if(!FilterViewModel.ShowLiquidating)
			{
				query.Where(c => !c.IsLiquidating);
			}

			if(FilterViewModel?.CounterpartyType != null)
			{
				query.Where(t => t.CounterpartyType == FilterViewModel.CounterpartyType);
			}

			if(FilterViewModel?.ReasonForLeaving != null)
			{
				query.Where(c => c.ReasonForLeaving == FilterViewModel.ReasonForLeaving);
			}

			if(FilterViewModel.IsNeedToSendBillByEdo)
			{
				query.Where(c => c.NeedSendBillByEdo);
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

			if(FilterViewModel?.CounterpartyId != null)
			{
				query.Where(c => c.Id == FilterViewModel.CounterpartyId);
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyContractNumber))
			{
				query.Where(Restrictions.Like(Projections.Property(() => contractAlias.Number),
					FilterViewModel.CounterpartyContractNumber,
					MatchMode.Anywhere));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyInn))
			{
				query.Where(c => c.INN == FilterViewModel.CounterpartyInn);
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
				query.JoinAlias(c => c.Tags, () => tagAliasForSubquery)
					 .Where(() => tagAliasForSubquery.Id == FilterViewModel.Tag.Id);

			query
				.Left.JoinAlias(c => c.Phones, () => phoneAlias, () => !phoneAlias.IsArchive)
				.Left.JoinAlias(() => counterpartyAlias.DeliveryPoints, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias, () => !deliveryPointPhoneAlias.IsArchive)
				.Left.JoinAlias(c => c.CounterpartyContracts, () => contractAlias);
			
			query.Where(GetSearchCriterion(
				() => counterpartyAlias.Name,
				() => counterpartyAlias.Id,
				() => counterpartyAlias.INN));

			var counterpartyResultQuery = query.SelectList(list => list
				.SelectGroup(c => c.Id).WithAlias(() => resultAlias.Id)
				.Select(c => c.Name).WithAlias(() => resultAlias.Name)
				.Select(c => c.INN).WithAlias(() => resultAlias.INN)
				.Select(c => c.IsArchive).WithAlias(() => resultAlias.IsArhive)
				.Select(c => c.IsLiquidating).WithAlias(() => resultAlias.IsLiquidating)
				.Select(contractsProjection).WithAlias(() => resultAlias.Contracts)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
					NHibernateUtil.String,
					Projections.Property(() => phoneAlias.Number),
					Projections.Constant("\n"))
					).WithAlias(() => resultAlias.Phones)			   
				.SelectSubQuery(addressSubquery).WithAlias(() => resultAlias.Addresses)
				.SelectSubQuery(tagsSubquery).WithAlias(() => resultAlias.Tags)
				)
				.TransformUsing(Transformers.AliasToBean<RetailCounterpartyJournalNode>());

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

			var query = uow.Session.QueryOver<Counterparty>(() => counterpartyAlias);

			if(FilterViewModel != null && FilterViewModel.IsForRetail != null)
			{
				query.Where(c => c.IsForRetail == FilterViewModel.IsForRetail);

				if (FilterViewModel.SalesChannels.Any(x => x.Selected))
				{
					query.Left.JoinAlias(c => c.SalesChannels, () => salesChannelAlias);
					query.Where(() => salesChannelAlias.Id.IsIn(FilterViewModel.SalesChannels.Where(x => x.Selected).Select(x => x.Id).ToArray()));
				}
			}

			if(FilterViewModel != null && !FilterViewModel.RestrictIncludeArchive)
			{
				query.Where(c => !c.IsArchive);
			}

			if(!FilterViewModel.ShowLiquidating)
			{
				query.Where(c => !c.IsLiquidating);
			}

			if(FilterViewModel?.CounterpartyType != null)
			{
				query.Where(t => t.CounterpartyType == FilterViewModel.CounterpartyType);
			}

			if(FilterViewModel?.ReasonForLeaving != null)
			{
				query.Where(c => c.ReasonForLeaving == FilterViewModel.ReasonForLeaving);
			}

			if(FilterViewModel.IsNeedToSendBillByEdo)
			{
				query.Where(c => c.NeedSendBillByEdo);
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
					.And(() => !counterpartyPhoneAlias.IsArchive)
					.Select(x => x.Id);

				query.Where(Subqueries.Exists(counterpartyPhonesSubquery.DetachedCriteria));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.DeliveryPointPhone))
			{
				query.Where(() => deliveryPointPhoneAlias.DigitsNumber == FilterViewModel.DeliveryPointPhone);
			}

			if(FilterViewModel?.CounterpartyId != null)
			{
				query.Where(c => c.Id == FilterViewModel.CounterpartyId);
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyContractNumber))
			{
				query.Where(Restrictions.Like(Projections.Property(() => contractAlias.Number),
					FilterViewModel.CounterpartyContractNumber,
					MatchMode.Anywhere));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyInn))
			{
				query.Where(c => c.INN == FilterViewModel.CounterpartyInn);
			}

			query.Where(FilterViewModel?.SearchByAddressViewModel?.GetSearchCriterion(
				() => deliveryPointAlias.CompiledAddress
			));

			if(FilterViewModel != null && FilterViewModel.Tag != null)
				query.JoinAlias(c => c.Tags, () => tagAliasForSubquery)
					 .Where(() => tagAliasForSubquery.Id == FilterViewModel.Tag.Id);

			query
				.Left.JoinAlias(c => c.Phones, () => phoneAlias, () => !phoneAlias.IsArchive)
				.Left.JoinAlias(() => counterpartyAlias.DeliveryPoints, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias, () => !deliveryPointPhoneAlias.IsArchive)
				.Left.JoinAlias(c => c.CounterpartyContracts, () => contractAlias);
			
			query.Where(GetSearchCriterion(
				() => counterpartyAlias.Name,
				() => counterpartyAlias.Id,
				() => counterpartyAlias.INN));

			var resultCountQuery = query
				.SelectList(list => list
					.Select(Projections.CountDistinct<Counterparty>(c => c.Id)))
				.SingleOrDefault<int>();
			
			return resultCountQuery;
		};

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateEditAction();
			CreateDefaultDeleteAction();
			CreateOpenCloseSupplyAction();
		}

		protected override Func<CounterpartyDlg> CreateDialogFunction => () => new CounterpartyDlg();

		protected override Func<RetailCounterpartyJournalNode, CounterpartyDlg> OpenDialogFunction => (node) => new CounterpartyDlg(node.Id);

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => {
					var selectedNodes = selected.OfType<RetailCounterpartyJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return false;
					}
					RetailCounterpartyJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<RetailCounterpartyJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return;
					}
					RetailCounterpartyJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), this);
					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog) {
						HideJournal(TabParent);
					}
				}
			);
			if(SelectionMode == JournalSelectionMode.None) {
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		private void CreateOpenCloseSupplyAction()
		{
			var openCloseSupplyAction = new JournalAction("Закрыть/открыть поставки",
				//sensetive
				(selected) => {
					var selectedNodes = selected.OfType<RetailCounterpartyJournalNode>();
					if(!_canOpenCloseDeliveries)
					{
						return false;
					}
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					RetailCounterpartyJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanUpdate;
				},
				//visible
				(selected) => true,
				//execute
				(selected) => {
					var selectedNodes = selected.OfType<RetailCounterpartyJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					RetailCounterpartyJournalNode selectedNode = selectedNodes.First();
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
	}
}
