using System;
using QS.Project.Journal;
using Vodovoz.Domain.Client;
using Vodovoz.JournalNodes;
using Vodovoz.Filters.ViewModels;
using QS.DomainModel.Config;
using QS.Services;
using NHibernate;
using QSContacts;
using NHibernate.Criterion;
using NHibernate.Transform;
using NHibernate.Dialect.Function;

namespace Vodovoz.JournalViewModels
{
	public class CounterpartyJournalViewModel : SingleEntityJournalViewModelBase<Counterparty, CounterpartyDlg, CounterpartyJournalNode>
	{
		private CounterpartyJournalFilterViewModel filterViewModel;
		public CounterpartyJournalFilterViewModel FilterViewModel {
			get { return filterViewModel; }
			set {
				filterViewModel = value;
				Filter = filterViewModel;
			}
		}

		public CounterpartyJournalViewModel(CounterpartyJournalFilterViewModel filterViewModel, IEntityConfigurationProvider entityConfigurationProvider, ICommonServices commonServices) : base(filter, entityConfigurationProvider, commonServices)
		{
			TabName = "Журнал контрагентов";
			RegisterAliasPropertiesToSearch(
				() => counterpartyAlias.Id,
				() => counterpartyAlias.VodovozInternalId,
				() => counterpartyAlias.Name,
				() => counterpartyAlias.INN,
				() => phoneAlias.DigitsNumber
			);
			FilterViewModel = filterViewModel;
		}

		CounterpartyJournalNode resultAlias = null;
		Counterparty counterpartyAlias = null;
		Counterparty counterpartyAliasForSubquery = null;
		CounterpartyContract contractAlias = null;
		Phone phoneAlias = null;
		DeliveryPoint addressAlias = null;
		Tag tagAliasForSubquery = null;

		protected override Func<IQueryOver<Counterparty>> ItemsSourceQueryFunction => () => {
			var query = UoW.Session.QueryOver<Counterparty>(() => counterpartyAlias);

			if(FilterViewModel != null && !FilterViewModel.RestrictIncludeArchive) {
				query.Where(c => !c.IsArchive);
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

			var counterpartyResultQuery = query
				.JoinAlias(c => c.Phones, () => phoneAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   .SelectGroup(c => c.Id).WithAlias(() => resultAlias.Id)
				   .SelectGroup(c => c.VodovozInternalId).WithAlias(() => resultAlias.InternalId)
				   .Select(c => c.Name).WithAlias(() => resultAlias.Name)
				   .Select(c => c.INN).WithAlias(() => resultAlias.INN)
				   .Select(c => c.IsArchive).WithAlias(() => resultAlias.IsArhive)
				   .SelectSubQuery(contractsSubquery).WithAlias(() => resultAlias.Contracts)
			   .Select(Projections.SqlFunction(
				   new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
				   NHibernateUtil.String,
					   Projections.Property(() => phoneAlias.Number),
				   Projections.Constant("\n"))
				   ).WithAlias(() => resultAlias.Phones)
			   .Select(Projections.SqlFunction(
					   new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
					   NHibernateUtil.String,
					   Projections.Property(() => phoneAlias.DigitsNumber),
					   Projections.Constant("\n"))
				   ).WithAlias(() => resultAlias.PhonesDigits)
					.SelectSubQuery(addressSubquery).WithAlias(() => resultAlias.Addresses)
					.SelectSubQuery(tagsSubquery).WithAlias(() => resultAlias.Tags)
				)
				.TransformUsing(Transformers.AliasToBean<CounterpartyJournalNode>());

			return counterpartyResultQuery;
		};

		protected override Func<CounterpartyDlg> CreateDialogFunction => () => new CounterpartyDlg();

		protected override Func<CounterpartyJournalNode, CounterpartyDlg> OpenDialogFunction => (node) => new CounterpartyDlg(node.Id);
	}
}
