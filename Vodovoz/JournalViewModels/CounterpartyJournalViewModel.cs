using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Contacts;
using QS.DomainModel.UoW;
using QS.Project.Journal.Search;
using QS.Project.Journal.Search.Criterion;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using Vodovoz.SearchModel;
using Vodovoz.SearchViewModels;

namespace Vodovoz.JournalViewModels
{
	public class CounterpartyJournalViewModel : FilterableSingleEntityJournalViewModelBase<Counterparty, CounterpartyDlg, CounterpartyJournalNode, CounterpartyJournalFilterViewModel, SolrCriterionSearchModel>
	{
		public CounterpartyJournalViewModel(
			CounterpartyJournalFilterViewModel filterViewModel, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			SingleEntrySolrCriterionSearchViewModel searchViewModel) 
		: base(filterViewModel, unitOfWorkFactory, commonServices, searchViewModel)
		{
			TabName = "Журнал контрагентов";
			UpdateOnChanges(
				typeof(Counterparty),
				typeof(CounterpartyContract),
				typeof(Phone),
				typeof(Tag),
				typeof(DeliveryPoint)
			);

			CriterionSearchModel.AddSolrSearchBy<Counterparty>(x => x.Id);
			CriterionSearchModel.AddSolrSearchBy<Counterparty>(x => x.Name);
			CriterionSearchModel.AddSolrSearchBy<Counterparty>(x => x.FullName);
			CriterionSearchModel.AddSolrSearchBy<Counterparty>(x => x.INN);
			CriterionSearchModel.AddSolrSearchBy<Employee>(x => x.Id);
			CriterionSearchModel.AddSolrSearchBy<Employee>(x => x.Name);
			CriterionSearchModel.AddSolrSearchBy<Employee>(x => x.LastName);

		}

		protected override Func<IUnitOfWork, IQueryOver<Counterparty>> ItemsSourceQueryFunction => (uow) => {
			CounterpartyJournalNode resultAlias = null;
			Counterparty counterpartyAlias = null;
			Counterparty counterpartyAliasForSubquery = null;
			CounterpartyContract contractAlias = null;
			Phone phoneAlias = null;
			DeliveryPoint addressAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Tag tagAliasForSubquery = null;

			var query = uow.Session.QueryOver<Counterparty>(() => counterpartyAlias);

			if(FilterViewModel != null && !FilterViewModel.RestrictIncludeArchive) {
				query.Where(c => !c.IsArchive);
			}

			if(FilterViewModel?.CounterpartyType != null) {
				query.Where(t => t.CounterpartyType == FilterViewModel.CounterpartyType);
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

			query
				.Left.JoinAlias(c => c.Phones, () => phoneAlias)
				.Left.JoinAlias(() => counterpartyAlias.DeliveryPoints, () => deliveryPointAlias);

			query.Where(CriterionSearchModel.ConfigureSearch()
				.AddSearchBy(() => counterpartyAlias.Id)
				.AddSearchBy(() => counterpartyAlias.Name)
				.AddSearchBy(() => counterpartyAlias.INN)
				//.AddSearchBy(() => phoneAlias.DigitsNumber)
				//.AddSearchBy(() => deliveryPointAlias.CompiledAddress)
				.GetSearchCriterion()
			);

			var counterpartyResultQuery = query.SelectList(list => list
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
					.SelectSubQuery(addressSubquery).WithAlias(() => resultAlias.Addresses)
					.SelectSubQuery(tagsSubquery).WithAlias(() => resultAlias.Tags)
				)
				.TransformUsing(Transformers.AliasToBean<CounterpartyJournalNode>())
				;

			return counterpartyResultQuery;
		};

		protected override Func<CounterpartyDlg> CreateDialogFunction => () => new CounterpartyDlg();

		protected override Func<CounterpartyJournalNode, CounterpartyDlg> OpenDialogFunction => (node) => new CounterpartyDlg(node.Id);
	}
}
