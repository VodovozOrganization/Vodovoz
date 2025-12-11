using QS.Services;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Widgets.Cash;
using VodovozBusiness.Controllers.Cash;

namespace Vodovoz.ViewModels.Factories
{
	public class VatRateVersionViewModelFactory : IVatRateVersionViewModelFactory
	{
		private readonly ICommonServices _commonServices;

		public VatRateVersionViewModelFactory(ICommonServices commonServices)
		{
			_commonServices = commonServices;
		}
		
		public VatRateNomenclatureVersionViewModel CreateVatRateVersionViewModel(Nomenclature nomenclature, DialogViewModelBase parentDialog, ViewModelEEVMBuilder<VatRate> vatRateEevmBuilder, bool isEditable = true)
		{
			return new VatRateNomenclatureVersionViewModel(
				nomenclature,
				new VatRateVersionController(nomenclature, null),
				_commonServices,
				vatRateEevmBuilder,
				parentDialog,
				isEditable);
		}

		public VatRateOrganizationVersionViewModel CreateVatRateVersionViewModel(Organization organization, DialogViewModelBase parentDialog, ViewModelEEVMBuilder<VatRate> vatRateEevmBuilder, bool isEditable = true)
		{
			return new VatRateOrganizationVersionViewModel(
				organization,
				new VatRateVersionController(null, organization),
				_commonServices,
				vatRateEevmBuilder,
				parentDialog,
				isEditable);
		}
	}
}
