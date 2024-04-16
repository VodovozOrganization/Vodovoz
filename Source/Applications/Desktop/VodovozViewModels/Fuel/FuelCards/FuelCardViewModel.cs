using FuelControl.Library.Services;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Fuel;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Fuel.FuelCards
{
	public class FuelCardViewModel : EntityTabViewModelBase<FuelCard>
	{
		private readonly ILogger<FuelCardViewModel> _logger;
		private readonly IFuelCardsGeneralInfoService _fuelCardsGeneralInfoService;

		public FuelCardViewModel(
			ILogger<FuelCardViewModel> logger,
			IFuelRepository fuelRepository,
			IFuelCardsGeneralInfoService fuelCardsGeneralInfoService,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(fuelRepository is null)
			{
				throw new ArgumentNullException(nameof(fuelRepository));
			}

			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fuelCardsGeneralInfoService = fuelCardsGeneralInfoService ?? throw new ArgumentNullException(nameof(fuelCardsGeneralInfoService));
			TabName = 
				UoWGeneric.IsNew
				? $"Диалог создания {Entity.GetType().GetClassUserFriendlyName().Genitive}"
				: $"{Entity.GetType().GetClassUserFriendlyName().Nominative.CapitalizeSentence()} №{Entity.Title}";

			SaveCommand = new DelegateCommand(() => Save(true));
			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
			GetCardIdCommand = new DelegateCommand(() => GetCardId(), () => CanGetCardId);

			ValidationContext.ServiceContainer.AddService(typeof(IUnitOfWorkFactory), unitOfWorkFactory);
			ValidationContext.ServiceContainer.AddService(typeof(IFuelRepository), fuelRepository);
		}

		public bool CanGetCardId => true;

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }
		public DelegateCommand GetCardIdCommand { get; }

		private string GetCardId()
		{
			return string.Empty;
		}
	}
}
