using System;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
    public class EmployeePostsJournalFactory : IEntityAutocompleteSelectorFactory
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly ICommonServices commonServices;

        public EmployeePostsJournalFactory(IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
        }

        public Type EntityType => typeof(EmployeePost);


        public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
        {
            return new EmployeePostsJournalViewModel(unitOfWorkFactory, commonServices);
        }

        public IEntitySelector CreateSelector(bool multipleSelect = false)
        {
            return CreateAutocompleteSelector(multipleSelect);
        }
    }
}