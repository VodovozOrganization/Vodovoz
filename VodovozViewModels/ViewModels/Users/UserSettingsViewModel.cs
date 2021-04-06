using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Journals.JournalViewModels.Organization;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Users
{
    public class UserSettingsViewModel : EntityTabViewModelBase<UserSettings>
    {
        private readonly IEmployeeService employeeService;
        private readonly ISubdivisionService subdivisionService;
        public EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel> SubdivisionAutocompleteSelectorFactory { get; }

        public UserSettingsViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
            IEmployeeService employeeService, ISubdivisionService subdivisionService, EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel> subdivisionAutocompleteSelectorFactory)
            : base(uowBuilder, unitOfWorkFactory, commonServices)
        {
            this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService)); ;
            this.subdivisionService = subdivisionService ?? throw new ArgumentNullException(nameof(subdivisionService));
            SubdivisionAutocompleteSelectorFactory = subdivisionAutocompleteSelectorFactory;
        }

        public bool IsUserFromOkk => subdivisionService.GetOkkId() == employeeService.GetEmployeeForUser(UoW, CommonServices.UserService.CurrentUserId)?.Subdivision?.Id;
    }
}
