using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.Orders
{
    public class NomenclaturePlanViewModel : EntityTabViewModelBase<Nomenclature>
    {
        public NomenclaturePlanViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory uowFactory, ICommonServices commonServices)
            : base(uowBuilder, uowFactory, commonServices)
        {
        }

        private DelegateCommand saveCommand = null;

        public DelegateCommand SaveCommand =>
            saveCommand ?? (saveCommand = new DelegateCommand(
                    () => { Save(true); },
                    () => true
                )
            );
    }
}