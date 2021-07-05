using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using QS.ViewModels;
using Vodovoz.Domain.Security;
using Vodovoz.ViewModels.Journals.FilterViewModels.Security;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Security;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Security
{
    public class RegisteredRMJournalViewModel : FilterableSingleEntityJournalViewModelBase<RegisteredRM, RegisteredRMViewModel, RegisteredRMJournalNode, RegisteredRMJournalFilterViewModel>
    {
        public RegisteredRMJournalViewModel(
	        EntitiesJournalActionsViewModel journalActionsViewModel,
	        RegisteredRMJournalFilterViewModel filterViewModel,
	        IUnitOfWorkFactory unitOfWorkFactory,
	        ICommonServices commonServices)
            : base(journalActionsViewModel, filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Журнал зарегистрированных RM";

            UpdateOnChanges(
                typeof(RegisteredRM)
            );
        }

        protected override Func<IUnitOfWork, IQueryOver<RegisteredRM>> ItemsSourceQueryFunction => (uow) =>
        {
            RegisteredRMJournalNode registeredRMJournalNodeAlias = null;
            RegisteredRM registeredRMAlias = null;

            var query = uow.Session.QueryOver<RegisteredRM>(() => registeredRMAlias);

            query.Where(GetSearchCriterion(
                () => registeredRMAlias.Id,
                () => registeredRMAlias.Username,
                () => registeredRMAlias.Domain,
                () => registeredRMAlias.SID
            ));

            var result = query.SelectList(list => list
                .Select(u => u.Id).WithAlias(() => registeredRMJournalNodeAlias.Id)
                .Select(u => u.Username).WithAlias(() => registeredRMJournalNodeAlias.Username)
                .Select(u => u.Domain).WithAlias(() => registeredRMJournalNodeAlias.Domain)
                .Select(u => u.SID).WithAlias(() => registeredRMJournalNodeAlias.SID)
                .Select(u => u.IsActive).WithAlias(() => registeredRMJournalNodeAlias.IsActive))
                .TransformUsing(Transformers.AliasToBean<RegisteredRMJournalNode>());

            return result;
        };

        protected override Func<RegisteredRMViewModel> CreateDialogFunction => () => new RegisteredRMViewModel(
               EntityUoWBuilder.ForCreate(),
               UnitOfWorkFactory,
               CommonServices
           );

        protected override Func<JournalEntityNodeBase, RegisteredRMViewModel> OpenDialogFunction => node => new RegisteredRMViewModel(
               EntityUoWBuilder.ForOpen(node.Id),
               UnitOfWorkFactory,
               CommonServices);
    }
}
