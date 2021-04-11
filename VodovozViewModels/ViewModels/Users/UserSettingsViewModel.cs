using System;
using QS.DomainModel.UoW;
using QS.EntityRepositories;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Journals.JournalViewModels.Organization;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Users
{
    public class UserSettingsViewModel : EntityTabViewModelBase<UserSettings>
    {
        private readonly IEmployeeService employeeService;
        private readonly ISubdivisionService subdivisionService;
        private int? subdivisionId;
        private string rulesName = "user_have_access_to_retail";
        
        public EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel> SubdivisionAutocompleteSelectorFactory { get; }
        public IEntityAutocompleteSelectorFactory CounterpartyAutocompleteSelectorFactory { get; }

        public UserSettingsViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
            IEmployeeService employeeService, ISubdivisionService subdivisionService, EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel> subdivisionAutocompleteSelectorFactory, IEntityAutocompleteSelectorFactory counterpartySelectorFactory)
            : base(uowBuilder, unitOfWorkFactory, commonServices)
        {
            this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService)); ;
            this.subdivisionService = subdivisionService ?? throw new ArgumentNullException(nameof(subdivisionService));
            SubdivisionAutocompleteSelectorFactory = subdivisionAutocompleteSelectorFactory;
            CounterpartyAutocompleteSelectorFactory = counterpartySelectorFactory;
            subdivisionId = this.employeeService.GetEmployeeForUser(UoW, CommonServices.UserService.CurrentUserId)?.Subdivision?.Id;
        }

        public bool IsUserFromOkk => subdivisionService.GetOkkId() == subdivisionId;

        public bool IsUserFromRetail => UoW.Session.QueryOver<HierarchicalPresetUserPermission>().Where(x =>
                        x.User.Id == Entity.User.Id && x.PermissionName == rulesName && x.Value).List().Count > 0 || 
                   UoW.Session.QueryOver<HierarchicalPresetSubdivisionPermission>().Where(x =>
                        x.Subdivision.Id == subdivisionId && x.PermissionName == rulesName && x.Value).List().Count > 0;
    }
}
