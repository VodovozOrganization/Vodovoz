using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Counterparties
{
	public class TagViewModel : EntityTabViewModelBase<Tag>
	{
		private readonly ILogger<TagViewModel> _logger;

		public TagViewModel(
			ILogger<TagViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_logger = logger;
		}

		public override bool Save(bool close)
		{
			_logger.LogInformation("Сохраняем  тег контрагента...");
			UoWGeneric.Save();
			_logger.LogInformation("Ok");
			return true;
		}
	}
}
