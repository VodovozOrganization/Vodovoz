using QS.DomainModel.UoW;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Widgets.Cash;

namespace Vodovoz.ViewModels.Factories
{
	public interface IVatRateVersionViewModelFactory
	{
		VatRateNomenclatureVersionViewModel CreateVatRateVersionViewModel(Nomenclature nomenclature, 
			DialogViewModelBase parentDialog, 
			ViewModelEEVMBuilder<VatRate> vatRateEevmBuilder,
			IUnitOfWorkFactory  unitOfWorkFactory,
			bool isEditable = true);
		
		VatRateOrganizationVersionViewModel CreateVatRateVersionViewModel(Organization organization, 
			DialogViewModelBase parentDialog,
			ViewModelEEVMBuilder<VatRate> vatRateEevmBuilder,
			IUnitOfWorkFactory  unitOfWorkFactory,
			bool isEditable = true);
	}
}
