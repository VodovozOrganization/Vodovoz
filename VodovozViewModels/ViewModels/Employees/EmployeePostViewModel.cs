using System;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure.Services;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Employees
{
    public class EmployeePostViewModel : EntityTabViewModelBase<EmployeePost>
    {
        public EmployeePostViewModel(
            IEntityUoWBuilder uowBuilder,
            IUnitOfWorkFactory uowFactory,
            ICommonServices commonServices
            ) : base(uowBuilder, uowFactory, commonServices) { }

        public string PostName
        {
            get => Entity.Name;
            set
            {
                Entity.Name = value;
                OnPropertyChanged(nameof(PostName));
            }
        }

        public bool CanEdit => PermissionResult.CanUpdate;
    }
}