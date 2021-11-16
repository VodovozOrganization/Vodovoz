using System;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class DiscountReasonViewModel : EntityTabViewModelBase<DiscountReason>
	{
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private readonly IProductGroupJournalFactory _productGroupJournalFactory;
		private object _selectedProductGroup;
		private DelegateCommand _addProductGroupCommand;
		private DelegateCommand _removeProductGroupCommand;
		
		public DiscountReasonViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IDiscountReasonRepository discountReasonRepository,
			IProductGroupJournalFactory productGroupJournalFactory)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_productGroupJournalFactory = productGroupJournalFactory ?? throw new ArgumentNullException(nameof(productGroupJournalFactory));
			TabName = UoWGeneric.IsNew ? "Новое основание для скидки" : $"Основание для скидки \"{Entity.Name}\"";
		}

		public bool IsProductGroupSelected => SelectedProductGroup != null;
		public bool CanChangeDiscountReasonName => Entity.Id == 0;

		public object SelectedProductGroup
		{
			get => _selectedProductGroup;
			set
			{
				if(SetField(ref _selectedProductGroup, value))
				{
					OnPropertyChanged(nameof(IsProductGroupSelected));
				}
			} 
		}

		public DelegateCommand AddProductGroupCommand => _addProductGroupCommand ?? (_addProductGroupCommand = new DelegateCommand(
				() =>
				{
					var journalViewModel = _productGroupJournalFactory.CreateProductGroupAutocompleteSelector();
					journalViewModel.OnEntitySelectedResult += (s, ea) =>
					{
						var selectedNode = ea.SelectedNodes.FirstOrDefault();
						if(selectedNode == null)
						{
							return;
						}

						Entity.AddProductGroup(UoW.GetById<ProductGroup>(selectedNode.Id));
					};
					TabParent.AddSlaveTab(this, journalViewModel);
				}
			)
		);
		
		public DelegateCommand RemoveProductGroupCommand => _removeProductGroupCommand ?? (_removeProductGroupCommand = new DelegateCommand(
				() =>
				{
					Entity.RemoveProductGroup(_selectedProductGroup as ProductGroup);
				}
			)
		);

		public override bool Save(bool close)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				if(_discountReasonRepository.ExistsActiveDiscountReasonWithName(
					uow, Entity.Id, Entity.Name, out var activeDiscountReasonWithSameName))
				{
					CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
						"Уже существует основание для скидки с таким названием.\n" +
						"Сохранение текущего основания невозможно.\n" +
						"Существующее основание:\n" +
						$"Код: {activeDiscountReasonWithSameName.Id}\n" +
						$"Название: {activeDiscountReasonWithSameName.Name}");
					return false;
				}
			}
			return base.Save(close);
		}
	}
}
