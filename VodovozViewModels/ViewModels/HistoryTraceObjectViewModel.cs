using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vodovoz.ViewModels.ViewModels
{
    public class HistoryTraceObjectViewModel : DialogTabViewModelBase
    {
        public HistoryTraceObjectViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigation) 
            : base(unitOfWorkFactory, interactiveService, navigation)
        {
        }
    }
}
