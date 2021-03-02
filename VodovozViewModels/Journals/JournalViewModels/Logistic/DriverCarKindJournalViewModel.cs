using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Journals.JournalViewModels
{
    public sealed class DriverCarKindJournalViewModel : FilterableSingleEntityJournalViewModelBase
        <DriverCarKind, DriverCarKindViewModel, DriverCarKindNode, DriverCarKindJournalFilterViewModel>
    {
        public DriverCarKindJournalViewModel(
            DriverCarKindJournalFilterViewModel filterViewModel,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices)
            : base(filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Виды наёмных автомобилей";
            UpdateOnChanges(typeof(DriverCarKind));
        }

        protected override Func<IUnitOfWork, IQueryOver<DriverCarKind>> ItemsSourceQueryFunction => uow => {
            DriverCarKindNode resultNode = null;
            DriverCarKind driverCarKindAlias = null;

            var query = uow.Session.QueryOver<DriverCarKind>(() => driverCarKindAlias);

            if(FilterViewModel != null) {
                if(!FilterViewModel.IncludeArchive) {
                    query.Where(c => !c.IsArchive);
                }
            }

            query.Where(GetSearchCriterion(
                () => driverCarKindAlias.Id,
                () => driverCarKindAlias.Name
            ));

            var result = query.SelectList(list => list
                    .Select(c => c.Id).WithAlias(() => resultNode.Id)
                    .Select(c => c.Name).WithAlias(() => resultNode.Name))
                .TransformUsing(Transformers.AliasToBean<DriverCarKindNode>());

            return result;
        };

        protected override Func<DriverCarKindViewModel> CreateDialogFunction => () => new DriverCarKindViewModel(
            EntityUoWBuilder.ForCreate(),
            UnitOfWorkFactory,
            commonServices
        );

        protected override Func<DriverCarKindNode, DriverCarKindViewModel> OpenDialogFunction => node => new DriverCarKindViewModel(
            EntityUoWBuilder.ForOpen(node.Id),
            UnitOfWorkFactory,
            commonServices
        );
    }
}
