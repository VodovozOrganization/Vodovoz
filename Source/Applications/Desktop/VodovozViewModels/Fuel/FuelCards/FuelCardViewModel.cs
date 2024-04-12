using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Fuel;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Fuel.FuelCards
{
	public class FuelCardViewModel : EntityTabViewModelBase<FuelCard>
	{
		private readonly ILogger<FuelCardViewModel> _logger;

		public FuelCardViewModel(
			ILogger<FuelCardViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));

			TabName = 
				UoWGeneric.IsNew
				? $"Диалог создания {Entity.GetType().GetClassUserFriendlyName().Genitive}"
				: $"{Entity.GetType().GetClassUserFriendlyName().Nominative.CapitalizeSentence()} \"{Entity.Title}\"";

			SaveCommand = new DelegateCommand(() => Save());
			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
		}

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }

		public override bool Save(bool close)
		{
			return base.Save(close);
		}
	}
}
