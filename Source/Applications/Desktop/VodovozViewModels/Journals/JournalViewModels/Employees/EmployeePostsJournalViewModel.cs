using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Employees
{
    public class EmployeePostsJournalViewModel : SingleEntityJournalViewModelBase<EmployeePost, EmployeePostViewModel, EmployeePostJournalNode>
    {
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;

        public EmployeePostsJournalViewModel(
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices)
            : base(unitOfWorkFactory, commonServices)
        {
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

            TabName = "Журнал должностей";
        }

        protected override Func<IUnitOfWork, IQueryOver<EmployeePost>> ItemsSourceQueryFunction => uow => {
            EmployeePostJournalNode resultAlias = null;
            EmployeePost postNameAlias = null;

            var query = uow.Session.QueryOver<EmployeePost>(() => postNameAlias);

            query.Where(GetSearchCriterion(
                () => postNameAlias.Id,
                () => postNameAlias.Name));

            return query
                .SelectList(list => list
                    .Select(x => postNameAlias.Id).WithAlias(() => resultAlias.Id)
                    .Select(x => postNameAlias.Name).WithAlias(() => resultAlias.EmployeePostName))
                    .TransformUsing(Transformers.AliasToBean<EmployeePostJournalNode>());
        };

        protected override Func<EmployeePostViewModel> CreateDialogFunction => () => new EmployeePostViewModel(
            EntityUoWBuilder.ForCreate(),
			_unitOfWorkFactory,
            _commonServices);

        protected override Func<EmployeePostJournalNode, EmployeePostViewModel> OpenDialogFunction => (node) => new EmployeePostViewModel(
            EntityUoWBuilder.ForOpen(node.Id),
			_unitOfWorkFactory,
            _commonServices
        );

        protected override void CreateNodeActions()
        {
            NodeActionsList.Clear();
            CreateDefaultSelectAction();
            CreateDefaultAddActions();
            CreateDefaultEditAction();
        }
    }
}
