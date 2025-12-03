using System;
using System.Windows.Input;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Repositories;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	/// <summary>
	/// VM создания/редактирования ставки НДС
	/// </summary>
	public class VatRateViewModel: EntityTabViewModelBase<VatRate>, IAskSaveOnCloseViewModel
	{
		private readonly IGenericRepository<VatRate> _vatRateRepository;

		public VatRateViewModel(IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			INavigationManager navigation, 
			IGenericRepository<VatRate> vatRateRepository) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_vatRateRepository = vatRateRepository ?? throw new ArgumentNullException(nameof(vatRateRepository));
			
			TabName = IsNew ? "Новая ставка НДС" : $"Ставка НДС №{Entity.Id}";
			
			SaveCommand = new DelegateCommand(() => SaveAndClose());
			CancelCommand = new DelegateCommand(() => Close(AskSaveOnClose, CloseSource.Cancel));
		}

		public ICommand SaveCommand { get; }
		public ICommand CancelCommand { get; }

		public bool IsNew => Entity.Id == 0;
		public bool CanEdit => PermissionResult.CanCreate && IsNew || PermissionResult.CanUpdate;
		public bool AskSaveOnClose => CanEdit;
		
		protected override bool BeforeSave()
		{
			var duplicate = _vatRateRepository
				.GetFirstOrDefault(UoW, x => x.VatRateValue == Entity.VatRateValue && x.Id != Entity.Id);

			if(duplicate != null)
			{
				CommonServices.InteractiveService.ShowMessage(
					QS.Dialog.ImportanceLevel.Warning,
					$"Ставка НДС с значением \"{Entity.VatRateValue}\"% уже существует."
				);
				return false;
			}

			return base.BeforeSave();
		}
	}
}
