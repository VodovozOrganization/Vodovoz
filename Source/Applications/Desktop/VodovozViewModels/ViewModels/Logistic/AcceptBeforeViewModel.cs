using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Logistic
{
    public class AcceptBeforeViewModel : EntityTabViewModelBase<AcceptBefore>
    {
        public AcceptBeforeViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
        {
            TabName = "Время приема до";
        }

        private int? hours;
        public int? Hours {
            get => hours;
            set {
                if(SetField(ref hours, value, () => Hours)) {
                    Entity.Time = new TimeSpan(value ?? 0, Minutes ?? 0, 0);
                }
            }
        }

        private int? minutes;
        public int? Minutes {
            get => minutes;
            set {
                if(SetField(ref minutes, value, () => Minutes))
                    Entity.Time = new TimeSpan(Hours ?? 0, value ?? 0, 0);
            }
        }
    }
}