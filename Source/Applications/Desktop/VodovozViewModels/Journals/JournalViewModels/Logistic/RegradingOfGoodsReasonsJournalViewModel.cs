using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class RegradingOfGoodsReasonsJournalViewModel : SingleEntityJournalViewModelBase<RegradingOfGoodsReason, RegradingOfGoodsReasonViewModel,
		RegradingOfGoodsReasonsJournalNode>
	{
		public RegradingOfGoodsReasonsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Причины пересортицы";

			UpdateOnChanges(typeof(RegradingOfGoodsReason));
		}

		protected override Func<IUnitOfWork, IQueryOver<RegradingOfGoodsReason>> ItemsSourceQueryFunction => (uow) =>
		{
			RegradingOfGoodsReason regradingOfGoodsReasonsAlias = null;
			RegradingOfGoodsReasonsJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => regradingOfGoodsReasonsAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => regradingOfGoodsReasonsAlias.Id,
				() => regradingOfGoodsReasonsAlias.Name)
			);

			itemsQuery.SelectList(list => list
					.Select(() => regradingOfGoodsReasonsAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => regradingOfGoodsReasonsAlias.Name).WithAlias(() => resultAlias.Name)
				)
				.TransformUsing(Transformers.AliasToBean<RegradingOfGoodsReasonsJournalNode>());

			return itemsQuery;
		};

		protected override Func<RegradingOfGoodsReasonViewModel> CreateDialogFunction => () =>
			new RegradingOfGoodsReasonViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<RegradingOfGoodsReasonsJournalNode, RegradingOfGoodsReasonViewModel> OpenDialogFunction =>
			(node) => new RegradingOfGoodsReasonViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
