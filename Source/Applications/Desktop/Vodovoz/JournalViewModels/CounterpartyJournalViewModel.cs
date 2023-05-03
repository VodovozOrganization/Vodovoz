﻿using System;
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

namespace Vodovoz.JournalViewModels
{
	public class CounterpartyJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<Counterparty, CounterpartyDlg, CounterpartyJournalNode, CounterpartyJournalFilterViewModel>
	{
		private readonly bool _userHaveAccessToRetail;

		public CounterpartyJournalViewModel(
			CounterpartyJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал контрагентов";

			_userHaveAccessToRetail = commonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_to_retail");

			UpdateOnChanges(
				typeof(Counterparty),
				typeof(CounterpartyContract),
				typeof(Phone),
				typeof(Tag),
				typeof(DeliveryPoint)
			);
		}

        protected override void CreateNodeActions()
        {
			NodeActionsList.Clear();
			CreateCustomSelectAction();
			CreateDefaultAddActions();
			CreateCustomEditAction();
			CreateDefaultDeleteAction();
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
					if (!selected.All(x => (x as CounterpartyJournalNode).Sensitive))
					{
						return;
					}
					var selectedNodes = selected.OfType<CounterpartyJournalNode>();
					if (selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					CounterpartyJournalNode selectedNode = selectedNodes.First();
					if (!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), this);
					if (foundDocumentConfig.JournalParameters.HideJournalForOpenDialog)
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

			if(!String.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyName))
			{
				query.Where(Restrictions.InsensitiveLike(Projections.Property(() => counterpartyAlias.Name),
					$"%{FilterViewModel.CounterpartyName}%"));
			}

			if(!String.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyPhone))
			{
				Phone counterpartyPhoneAlias = null;

				var counterpartyPhonesSubquery = QueryOver.Of<Phone>(() => counterpartyPhoneAlias)
					.Where(() => counterpartyPhoneAlias.Counterparty.Id == counterpartyAlias.Id)
					.And(() => counterpartyPhoneAlias.DigitsNumber == FilterViewModel.CounterpartyPhone)
					.Select(x => x.Id);

				query.Where(Subqueries.Exists(counterpartyPhonesSubquery.DetachedCriteria));
			}

			if(!String.IsNullOrWhiteSpace(FilterViewModel?.DeliveryPointPhone))
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

			var contractsSubquery = QueryOver.Of<CounterpartyContract>(() => contractAlias)
				.Left.JoinAlias(c => c.Counterparty, () => counterpartyAliasForSubquery)
				.Where(() => counterpartyAlias.Id == counterpartyAliasForSubquery.Id)
				.Select(Projections.SqlFunction(
											new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( CONCAT(?2,' - ',?1) SEPARATOR ?3)"),
											NHibernateUtil.String,
											Projections.Property(() => contractAlias.ContractSubNumber),
											Projections.Property(() => counterpartyAliasForSubquery.VodovozInternalId),
											Projections.Constant("\n")));

			var addressSubquery = QueryOver.Of<DeliveryPoint>(() => addressAlias)
				.Where(d => d.Counterparty.Id == counterpartyAlias.Id)
				.Where(() => addressAlias.IsActive)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
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

			if(FilterViewModel != null && FilterViewModel.IsForSalesDepartment != null)
			{
				query.Where(() => counterpartyAlias.IsForSalesDepartment == FilterViewModel.IsForSalesDepartment);
			}

			query
				.Left.JoinAlias(c => c.Phones, () => phoneAlias)
				.Left.JoinAlias(() => counterpartyAlias.DeliveryPoints, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias);

			var searchHealperNew = new TempAdapters.SearchHelper(Search);

			var idParam = new TempAdapters.SearchParameter(() => counterpartyAlias.Id, TempAdapters.SearchParametrType.Id);
			var vodovozInternalIdParam = new TempAdapters.SearchParameter(() => counterpartyAlias.VodovozInternalId, TempAdapters.SearchParametrType.VodovozInternalId);
			var nameParam = new TempAdapters.SearchParameter(() => counterpartyAlias.Name, TempAdapters.SearchParametrType.Name);
			var INNParam = new TempAdapters.SearchParameter(() => counterpartyAlias.INN, TempAdapters.SearchParametrType.INN);
			var digitNumberParam = new TempAdapters.SearchParameter(() => phoneAlias.DigitsNumber, TempAdapters.SearchParametrType.DigitsNumber);
			var deliveryPointPhoneParam = new TempAdapters.SearchParameter(() => deliveryPointPhoneAlias.DigitsNumber, TempAdapters.SearchParametrType.DigitsNumber);
			var compiledAdressParam = new TempAdapters.SearchParameter(() => deliveryPointAlias.CompiledAddress, TempAdapters.SearchParametrType.CompiledAddress);

			query.Where(searchHealperNew.GetSearchCriterionNew(
				idParam,
				vodovozInternalIdParam,
				nameParam,
				INNParam,
				digitNumberParam,
				deliveryPointPhoneParam,
				compiledAdressParam
			));

			var counterpartyResultQuery = query
				.SelectList(list => list
					.SelectGroup(c => c.Id).WithAlias(() => resultAlias.Id)
					.SelectGroup(c => c.VodovozInternalId).WithAlias(() => resultAlias.InternalId)
					.Select(c => c.Name).WithAlias(() => resultAlias.Name)
					.Select(c => c.INN).WithAlias(() => resultAlias.INN)
					.Select(c => c.IsArchive).WithAlias(() => resultAlias.IsArhive)
					.SelectSubQuery(contractsSubquery).WithAlias(() => resultAlias.Contracts)
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

			if(!String.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyName))
			{
				query.Where(Restrictions.InsensitiveLike(Projections.Property(() => counterpartyAlias.Name),
					$"%{FilterViewModel.CounterpartyName}%"));
			}

			if(!String.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyPhone))
			{
				Phone counterpartyPhoneAlias = null;

				var counterpartyPhonesSubquery = QueryOver.Of<Phone>(() => counterpartyPhoneAlias)
					.Where(() => counterpartyPhoneAlias.Counterparty.Id == counterpartyAlias.Id)
					.And(() => counterpartyPhoneAlias.DigitsNumber == FilterViewModel.CounterpartyPhone)
					.Select(x => x.Id);

				query.Where(Subqueries.Exists(counterpartyPhonesSubquery.DetachedCriteria));
			}

			if(!String.IsNullOrWhiteSpace(FilterViewModel?.DeliveryPointPhone))
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

			var contractsSubquery = QueryOver.Of<CounterpartyContract>(() => contractAlias)
				.Left.JoinAlias(c => c.Counterparty, () => counterpartyAliasForSubquery)
				.Where(() => counterpartyAlias.Id == counterpartyAliasForSubquery.Id)
				.Select(Projections.SqlFunction(
											new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( CONCAT(?2,' - ',?1) SEPARATOR ?3)"),
											NHibernateUtil.String,
											Projections.Property(() => contractAlias.ContractSubNumber),
											Projections.Property(() => counterpartyAliasForSubquery.VodovozInternalId),
											Projections.Constant("\n")));

			var addressSubquery = QueryOver.Of<DeliveryPoint>(() => addressAlias)
				.Where(d => d.Counterparty.Id == counterpartyAlias.Id)
				.Where(() => addressAlias.IsActive)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
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

			if(FilterViewModel != null && FilterViewModel.IsForSalesDepartment != null)
			{
				query.Where(() => counterpartyAlias.IsForSalesDepartment == FilterViewModel.IsForSalesDepartment);
			}
			
			var searchHealperNew = new TempAdapters.SearchHelper(Search);

			var idParam = new TempAdapters.SearchParameter(() => counterpartyAlias.Id, TempAdapters.SearchParametrType.Id);
			var vodovozInternalIdParam = new TempAdapters.SearchParameter(() => counterpartyAlias.VodovozInternalId, TempAdapters.SearchParametrType.VodovozInternalId);
			var nameParam = new TempAdapters.SearchParameter(() => counterpartyAlias.Name, TempAdapters.SearchParametrType.Name);
			var INNParam = new TempAdapters.SearchParameter(() => counterpartyAlias.INN, TempAdapters.SearchParametrType.INN);
			var digitNumberParam = new TempAdapters.SearchParameter(() => phoneAlias.DigitsNumber, TempAdapters.SearchParametrType.DigitsNumber);
			var deliveryPointPhoneParam = new TempAdapters.SearchParameter(() => deliveryPointPhoneAlias.DigitsNumber, TempAdapters.SearchParametrType.DigitsNumber);
			var compiledAdressParam = new TempAdapters.SearchParameter(() => deliveryPointAlias.CompiledAddress, TempAdapters.SearchParametrType.CompiledAddress);

			query.Where(searchHealperNew.GetSearchCriterionNew(
				idParam,
				vodovozInternalIdParam,
				nameParam,
				INNParam,
				digitNumberParam,
				deliveryPointPhoneParam,
				compiledAdressParam
			));

			query
				.Left.JoinAlias(c => c.Phones, () => phoneAlias)
				.Left.JoinAlias(() => counterpartyAlias.DeliveryPoints, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias);

			var resultCountQuery = query
				.SelectList(list => list
					.Select(Projections.CountDistinct<Counterparty>(c => c.Id)))
				.SingleOrDefault<int>();

			return resultCountQuery;
		};

		protected override Func<CounterpartyDlg> CreateDialogFunction => () => new CounterpartyDlg();

		protected override Func<CounterpartyJournalNode, CounterpartyDlg> OpenDialogFunction => (node) => new CounterpartyDlg(node.Id);
	}
}
