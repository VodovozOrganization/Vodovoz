using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Store;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Store;

namespace Vodovoz.Journals
{
    public class MovementWagonJournalViewModel : FilterableSingleEntityJournalViewModelBase<MovementWagon, MovementWagonViewModel, MovementWagonJournalNode, MovementWagonJournalFilterViewModel>
    {
        public MovementWagonJournalViewModel(MovementWagonJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) 
            : base(filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Журнал фур";
        }

        protected override Func<IUnitOfWork, IQueryOver<MovementWagon>> ItemsSourceQueryFunction => (uow) =>
        {
            MovementWagonJournalNode movementWagonJournalNodeAlias = null;
            MovementWagon movementWagonAlias = null;

            var query = uow.Session.QueryOver<MovementWagon>(() => movementWagonAlias);

            query.Where(GetSearchCriterion(
                () => movementWagonAlias.Id,
                () => movementWagonAlias.Name
            ));

            var result = query.SelectList(list => list
                .Select(u => u.Id).WithAlias(() => movementWagonJournalNodeAlias.Id)
                .Select(u => u.Name).WithAlias(() => movementWagonJournalNodeAlias.Name))
                .TransformUsing(Transformers.AliasToBean<MovementWagonJournalNode>());

            return result;
        };

        protected override Func<MovementWagonViewModel> CreateDialogFunction => () => new MovementWagonViewModel(
               EntityUoWBuilder.ForCreate(),
               UnitOfWorkFactory,
               commonServices
           );

        protected override Func<MovementWagonJournalNode, MovementWagonViewModel> OpenDialogFunction => (node) => new MovementWagonViewModel(
               EntityUoWBuilder.ForOpen(node.Id),
               UnitOfWorkFactory,
               commonServices
           );
    }
}
