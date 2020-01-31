using Vodovoz.Domain.Contacts;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;

namespace Vodovoz.ViewModels
{
	public class PhoneTypeViewModel : EntityTabViewModelBase<PhoneType>
	{
		public PhoneTypeViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Типы телефонов";
		}

		private PhoneAdditionalType phoneAdditionalType;
		public PhoneAdditionalType PhoneAdditionalType {
			get {
				phoneAdditionalType = Entity.PhoneAdditionalType;
				return phoneAdditionalType;
			}
			set {
				if(SetField(ref phoneAdditionalType, value)) {
					Entity.PhoneAdditionalType = value;
				}
			}
		}

	}
}
