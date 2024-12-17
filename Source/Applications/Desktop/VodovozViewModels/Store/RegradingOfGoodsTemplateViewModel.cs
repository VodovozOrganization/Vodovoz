using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModels.Store
{
	public class RegradingOfGoodsTemplateViewModel : EntityTabViewModelBase<RegradingOfGoodsTemplate>
	{
		private readonly ILogger<RegradingOfGoodsTemplateViewModel> _logger;
		private readonly IValidator _validator;

		public RegradingOfGoodsTemplateViewModel(
			ILogger<RegradingOfGoodsTemplateViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			RegradingOfGoodsTemplateItemsViewModel regradingOfGoodsTemplateItemsViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IValidator validator,
			INavigationManager navigation)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(regradingOfGoodsTemplateItemsViewModel is null)
			{
				throw new ArgumentNullException(nameof(regradingOfGoodsTemplateItemsViewModel));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));

			ItemsViewModel = regradingOfGoodsTemplateItemsViewModel;
			ItemsViewModel.SetUnitOfWork(UoW);
			ItemsViewModel.Items = Entity.Items;
			ItemsViewModel.ParentViewModel = this;

			SaveCommand = new DelegateCommand(SaveAndClose);
			CancelCommand = new DelegateCommand(() => Close(true, CloseSource.Cancel));
		}

		public RegradingOfGoodsTemplateItemsViewModel ItemsViewModel { get; }
		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }

		public override bool Save(bool close)
		{
			if(!_validator.Validate(Entity))
			{
				return false;
			}

			_logger.LogInformation("Сохраняем шаблон пересортицы...");
			UoWGeneric.Save();
			_logger.LogInformation("Ok.");
			return true;
		}
	}
}
