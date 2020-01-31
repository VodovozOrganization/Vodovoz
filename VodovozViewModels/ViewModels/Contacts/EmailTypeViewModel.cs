using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.ViewModels
{
	public class EmailTypeViewModel : EntityTabViewModelBase<EmailType>
	{
		public EmailTypeViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Типы e-mail адресов";
		}

		private EmailAdditionalType emailAdditionalType;
		public EmailAdditionalType EmailAdditionalType {
			get {
				emailAdditionalType = Entity.EmailAdditionalType;
				return emailAdditionalType;
			}
			set {
				if(SetField(ref emailAdditionalType, value)) {
					Entity.EmailAdditionalType = value;
				}
			}
		}

	}
}
