using System;
using QS.Project.Filter;
using QS.Project.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Services;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.ViewModels.Journals.FilterViewModels
{
    public class CashRequestJournalFilterViewModel: FilterViewModelBase<CashRequestJournalFilterViewModel>
    {
        private Employee author;
        public virtual Employee Author {
            get => author;
            set => UpdateFilterField(ref author, value);
        }

        private Employee accountableEmployee;
        public virtual Employee AccountableEmployee {
            get => accountableEmployee;
            set => UpdateFilterField(ref accountableEmployee, value);
        }

        private DateTime? startDate;
        public virtual DateTime? StartDate {
            get => startDate;
            set => UpdateFilterField(ref startDate, value);
        }

        private DateTime? endDate; //= DateTime.Now;
        public virtual DateTime? EndDate {
            get => endDate;
            set => UpdateFilterField(ref endDate, value);
        }
        
        private CashRequest.States? state;
        public virtual CashRequest.States? State {
            get => state;
            set => UpdateFilterField(ref state, value);
        }

        public UserRole GetUserRole()
        {
            int userId = ServicesConfig.CommonServices.UserService.CurrentUserId;
            
            if (CashRequestViewModel.checkRole("role_financier_cash_request", userId)){
                return UserRole.Financier;
            } else if (CashRequestViewModel.checkRole("role_coordinator_cash_request", userId)){
                return UserRole.Coordinator;
            } else if (CashRequestViewModel.checkRole("role_сashier", userId)) {
                return UserRole.Cashier;
            } else {
                //Тут неважно что вернется роль создатель, тк кк нет условий на скрытие фильтров для создателя заявки
                return UserRole.Other;
            }
        }
    }
}