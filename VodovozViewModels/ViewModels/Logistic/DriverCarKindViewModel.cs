using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Logistic
{
    public class DriverCarKindViewModel : EntityTabViewModelBase<DriverCarKind>
    {
        public DriverCarKindViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) 
            : base(uowBuilder, unitOfWorkFactory, commonServices)
        {
            
        }
    }
}
