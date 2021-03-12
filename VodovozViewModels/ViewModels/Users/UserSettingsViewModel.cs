using System;
using NLog;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Users
{
    public class UserSettingsViewModel : EntityTabViewModelBase<UserSettings>
    {
        private readonly IEmployeeService employeeService;
        private readonly ISubdivisionService subdivisionService;
        private readonly ICommonServices commonServices;

        public UserSettingsViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory,
           ICommonServices commonServices, IEmployeeService employeeService, ISubdivisionService subdivisionService) : base(uowBuilder, unitOfWorkFactory, commonServices)
        {
            this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService)); ;
            this.subdivisionService = subdivisionService ?? throw new ArgumentNullException(nameof(subdivisionService));
            this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices)); ;
        }

        public bool IsUserFromOkk => subdivisionService.GetOkkId() == employeeService.GetEmployeeForUser(UoW, commonServices.UserService.CurrentUserId)?.Subdivision?.Id;
    }
}
