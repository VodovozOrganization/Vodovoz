using System;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Banks.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using Vodovoz.ViewModels.Journals.FilterViewModels.Banks;
using Vodovoz.ViewModels.Journals.JournalNodes.Banks;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Banks
{
	public class BanksJournalViewModel : JournalViewModelBase
	{
		private readonly BanksJournalFilterViewModel _filterViewModel;

		public BanksJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			BanksJournalFilterViewModel banksJournalViewModel,
			Action<BanksJournalFilterViewModel> filterConfig = null) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_filterViewModel = banksJournalViewModel ?? throw new ArgumentNullException(nameof(banksJournalViewModel));
			
			Title = "Банки";

			var loader = new ThreadDataLoader<BanksJournalNode>(UnitOfWorkFactory);
			loader.AddQuery(BanksQuery);
			DataLoader = loader;
			
			JournalFilter =  _filterViewModel;

			if(filterConfig != null)
			{
				_filterViewModel.ConfigureWithoutFiltering(filterConfig);
			}
		}

		private IQueryOver<Bank> BanksQuery(IUnitOfWork uow)
		{
			Account accountAlias = null;
			Bank bankAlias = null;
			BanksJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => bankAlias)
				.JoinEntityAlias(
					() => accountAlias,
					() => accountAlias.InBank.Id == bankAlias.Id,
					JoinType.LeftOuterJoin);

			if(_filterViewModel != null)
			{
				if(_filterViewModel.ExcludeBanksIds != null)
				{
					query.WhereRestrictionOn(b => b.Id).Not.IsInG(_filterViewModel.ExcludeBanksIds);
				}

				if(_filterViewModel.Account != null)
				{
					query.Where(() => accountAlias.Id == _filterViewModel.Account.Id);
				}
			}

			query.Where(GetSearchCriterion(
				() => bankAlias.Name,
				() => bankAlias.Bik
			));

			query.SelectList(list => list
					.SelectGroup(b => b.Id).WithAlias(() => resultAlias.Id)
					.Select(b => b.Name).WithAlias(() => resultAlias.Name)
					.Select(b => b.Bik).WithAlias(() => resultAlias.Bik)
					.Select(b => b.City).WithAlias(() => resultAlias.City)
				)
				.TransformUsing(Transformers.AliasToBean<BanksJournalNode>());
			
			return query;
		}
	}
}
