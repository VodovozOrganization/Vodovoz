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

		private PhoneEnumType phoneEnumType;
		public PhoneEnumType PhoneEnumType {
			get {
				phoneEnumType = Entity.PhoneEnumType;
				return phoneEnumType;
			}
			set {
				if(SetField(ref phoneEnumType, value)) {
					Entity.PhoneEnumType = value;
				}
			}
		}

	}
}
