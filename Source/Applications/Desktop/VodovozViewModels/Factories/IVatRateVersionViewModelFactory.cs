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
		/// <summary>
		///  Создать ВМ выбора версии ставки НДС для номенклатуры
		/// </summary>
		/// <param name="nomenclature">Номенклатура</param>
		/// <param name="parentDialog">Родительское окно</param>
		/// <param name="vatRateEevmBuilder">evvm билдер</param>
		/// <param name="unitOfWorkFactory">UoW</param>
		/// <param name="isEditable">Редактируема ли</param>
		/// <returns>VM</returns>
		VatRateNomenclatureVersionViewModel CreateVatRateVersionViewModel(Nomenclature nomenclature, 
			DialogViewModelBase parentDialog, 
			ViewModelEEVMBuilder<VatRate> vatRateEevmBuilder,
			IUnitOfWorkFactory  unitOfWorkFactory,
			bool isEditable = true);
		
		/// <summary>
		///  Создать ВМ выбора версии ставки НДС для организации
		/// </summary>
		/// <param name="organization">Организация</param>
		/// <param name="parentDialog">Родительское окно</param>
		/// <param name="vatRateEevmBuilder">evvm билдер</param>
		/// <param name="unitOfWorkFactory">UoW</param>
		/// <param name="isEditable">Редактируема ли</param>
		/// <returns>VM</returns>
		VatRateOrganizationVersionViewModel CreateVatRateVersionViewModel(Organization organization, 
			DialogViewModelBase parentDialog,
			ViewModelEEVMBuilder<VatRate> vatRateEevmBuilder,
			IUnitOfWorkFactory  unitOfWorkFactory,
			bool isEditable = true);
	}
}
