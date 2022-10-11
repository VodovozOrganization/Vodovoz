using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Retail;
using Vodovoz.ViewModels.Journals.FilterViewModels.Retail;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Retail;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Retail
{
    public class SalesChannelJournalViewModel : FilterableSingleEntityJournalViewModelBase<SalesChannel, SalesChannelViewModel, SalesChannelJournalNode, SalesChannelJournalFilterViewModel>
    {
        public SalesChannelJournalViewModel(SalesChannelJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
            : base(filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Журнал каналов сбыта";

            UpdateOnChanges(
                typeof(SalesChannel)
            );
        }

        protected override Func<IUnitOfWork, IQueryOver<SalesChannel>> ItemsSourceQueryFunction => (uow) =>
        {
            SalesChannelJournalNode salesChannelJournalNodeAlias = null;
            SalesChannel salesChannelAlias = null;

            var query = uow.Session.QueryOver<SalesChannel>(() => salesChannelAlias);

            query.Where(GetSearchCriterion(
                () => salesChannelAlias.Id,
                () => salesChannelAlias.Name
            ));

            var result = query.SelectList(list => list
                .Select(u => u.Id).WithAlias(() => salesChannelJournalNodeAlias.Id)
                .Select(u => u.Name).WithAlias(() => salesChannelJournalNodeAlias.Name))
                .TransformUsing(Transformers.AliasToBean<SalesChannelJournalNode>());

            return result;
        };

        protected override Func<SalesChannelViewModel> CreateDialogFunction => () => new SalesChannelViewModel(
               EntityUoWBuilder.ForCreate(),
               UnitOfWorkFactory,
               commonServices);

        protected override Func<SalesChannelJournalNode, SalesChannelViewModel> OpenDialogFunction => (node) => new SalesChannelViewModel(
               EntityUoWBuilder.ForOpen(node.Id),
               UnitOfWorkFactory,
               commonServices);
    }
}
