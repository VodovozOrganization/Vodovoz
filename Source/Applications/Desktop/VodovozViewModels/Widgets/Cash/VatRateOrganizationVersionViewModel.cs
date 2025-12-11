using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Controllers.Cash;

namespace Vodovoz.ViewModels.Widgets.Cash
{
	public class VatRateOrganizationVersionViewModel : EntityWidgetViewModelBase<Organization>
	{
		private readonly IVatRateVersionController _vatRateVersionController;

		public VatRateOrganizationVersionViewModel(
			Organization entity, 
			IVatRateVersionController vatRateVersionController,
			ICommonServices commonServices,
			ViewModelEEVMBuilder<VatRate> vatRateEevmBuilder,
			DialogViewModelBase parentDialog,
			bool isEditable = true) : base(entity, commonServices)
		{
			_vatRateVersionController = vatRateVersionController;
		}
	}
}
