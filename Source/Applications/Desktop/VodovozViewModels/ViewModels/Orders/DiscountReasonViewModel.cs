using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Extensions;
using Vodovoz.ViewModels.Goods.ProductGroups;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Widgets;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class DiscountReasonViewModel : DialogTabViewModelBase, IAskSaveOnCloseViewModel
	{
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private ILifetimeScope _lifetimeScope;
		private readonly ICommonServices _commonServices;
		private IEnumerable<object> _selectedNomenclatures = Array.Empty<object>();
		private IEnumerable<object> _selectedProductGroups = Array.Empty<object>();
		private IObservableList<PromotionalSet> _promotionalSets;
		private IPermissionResult _permissionResult;

		private int _currentPage;
		private bool _hasOrderMinSum;
		private bool _discountInfoTabActive;
		private bool _promoCodeSettingsTabActive;
		private bool _hasPromoCodeDurationTime;
		private bool _selectedAllCategories;

		public DiscountReasonViewModel(
			ILifetimeScope lifetimeScope,
			IEntityViewModelContext viewModelContext,
			ICommonServices commonServices,
			IDiscountReasonRepository discountReasonRepository,
			INavigationManager navigationManager,
			AddOrRemoveIDomainObjectViewModel addOrRemovePromoSetsViewModel)
			: base(viewModelContext?.UowFactory, commonServices?.InteractiveService, navigationManager)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			
			InitializeEntity(viewModelContext);
			SetPermissions();
			InitializeCommands();
			InitializeDiscountTypes();
			InitializeNomenclatureCategoriesList();
			InitializeHasOrderMinSum();
			InitializeHasPromoCodeDurationTime();
			InitializeViewModels(addOrRemovePromoSetsViewModel);
		}

		private void InitializeViewModels(AddOrRemoveIDomainObjectViewModel addOrRemovePromoSetsViewModel)
		{
			AddOrRemovePromoSetsViewModel = addOrRemovePromoSetsViewModel ?? throw new ArgumentNullException(nameof(addOrRemovePromoSetsViewModel));
			AddOrRemovePromoSetsViewModel.Configure(
				typeof(PromotionalSet),
				CanEditPromotionalSets,
				"Промонаборы:",
				UoW,
				(DialogViewModelBase)this,
				_promotionalSets);
		}

		public DiscountReasonBase Entity { get; private set; }
		public AddOrRemoveIDomainObjectViewModel AddOrRemovePromoSetsViewModel { get; private set; }
		public bool IsNewEntity => Entity.Id == 0;
		public bool AskSaveOnClose => CanEditDiscountReason;
		public bool CanArchive => CanEditDiscountReason && !IsArchive;
		public bool CanEditDiscountReason => IsNewEntity && _permissionResult.CanCreate;
		public bool CanEditPromotionalSets => CanEditDiscountReason && Entity.DiscountReasonType != DiscountReasonType.Discount;
		public bool CanRemoveNomenclatures => IsNomenclaturesSelected && CanEditDiscountReason;
		public bool IsNomenclaturesSelected => SelectedNomenclatures != null && SelectedNomenclatures.Any();
		public bool CanRemoveProductGroup => IsProductGroupsSelected && CanEditDiscountReason;
		public bool IsProductGroupsSelected => SelectedProductGroups != null && SelectedProductGroups.Any();
		public bool CanChangeDiscountReasonName => CanEditDiscountReason;
		public IDictionary<DiscountType, UseDiscountType?> DiscountTypeUses { get; private set; }

		public string EntityName
		{
			get => Entity.Name;
			set
			{
				Entity.Name = value;
				OnPropertyChanged();
			}
		}
		
		public bool IsArchive
		{
			get => Entity.IsArchive;
			set
			{
				Entity.IsArchive = value;
				OnPropertyChanged();
			}
		}
		
		public bool IsPremiumDiscount
		{
			get => Entity.IsPremiumDiscount;
			set
			{
				Entity.IsPremiumDiscount = value;
				OnPropertyChanged();
			}
		}
		
		public bool IsPresent
		{
			get => Entity.IsPresent;
			set
			{
				Entity.IsPresent = value;
				OnPropertyChanged();
			}
		}
		
		public decimal DiscountValue
		{
			get => Entity.Value;
			set
			{
				Entity.Value = value;
				OnPropertyChanged();
			}
		}

		public DiscountReasonType SelectedDiscountReasonType
		{
			get => Entity.DiscountReasonType;
			set
			{
				if((int)Entity.DiscountReasonType == (int)value)
				{
					return;
				}
				
				if(Entity.Id > 0)
				{
					throw new InvalidOperationException(
						"Нельзя менять тип основания скидки у сохраненной сущности. Изменение доступно только для новых!");
				}

				//у обычной скидки не может быть промонаборов
				if(value == DiscountReasonType.Discount)
				{
					Entity.PromoSetsClear();
				}

				switch(value)
				{
					case DiscountReasonType.Discount:
						Entity = DiscountReason.Create(Entity, _promotionalSets);
						break;
					case DiscountReasonType.FirstOnlineOrderDiscount:
						Entity = FirstOnlineOrderDiscount.Create(Entity, _promotionalSets);
						break;
					case DiscountReasonType.PromoCode:
						Entity = PromoCodeDiscount.Create(Entity, _promotionalSets);
						break;
					case DiscountReasonType.AutoOrder:
						Entity = AutoOrderDiscount.Create(Entity, _promotionalSets);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(value), value, "Неизвестное значение типа скидки");
				}

				UpdateDiscountTypes();
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsPromoCode));
				OnPropertyChanged(nameof(CanShowApplicabilitiesByTypes));
				AddOrRemovePromoSetsViewModel.SetCanEdit(CanEditPromotionalSets);
			}
		}

		public DateTime? StartDate
		{
			get => Entity is PromoCodeDiscount promoCode ? promoCode.StartDate : null;
			set
			{
				if(!(Entity is PromoCodeDiscount promoCode))
				{
					throw new InvalidOperationException("Установка даты начала промокода, доступно только для скидки с типом Промокод");
				}

				promoCode.StartDate = value;
				OnPropertyChanged();
			}
		}
		
		public DateTime? EndDate
		{
			get => Entity is PromoCodeDiscount promoCode ? promoCode.EndDate : null;
			set
			{
				if(!(Entity is PromoCodeDiscount promoCode))
				{
					throw new InvalidOperationException("Установка даты окончания промокода, доступно только для скидки с типом Промокод");
				}

				promoCode.EndDate = value;
				OnPropertyChanged();
			}
		}
		
		public TimeSpan? StartTime
		{
			get => Entity is PromoCodeDiscount promoCode ? promoCode.StartTime : null;
			set
			{
				if(!(Entity is PromoCodeDiscount promoCode))
				{
					throw new InvalidOperationException("Установка времени начала действия промокода, доступно только для скидки с типом Промокод");
				}

				promoCode.StartTime = value;
				OnPropertyChanged();
			}
		}
		
		public TimeSpan? EndTime
		{
			get => Entity is PromoCodeDiscount promoCode ? promoCode.EndTime : null;
			set
			{
				if(!(Entity is PromoCodeDiscount promoCode))
				{
					throw new InvalidOperationException("Установка времени окончания действия промокода, доступно только для скидки с типом Промокод");
				}

				promoCode.EndTime = value;
				OnPropertyChanged();
			}
		}
		
		public decimal OrderMinSum
		{
			get => Entity is PromoCodeDiscount promoCode ? promoCode.OrderMinSum : 0m;
			set
			{
				if(!(Entity is PromoCodeDiscount promoCode))
				{
					throw new InvalidOperationException("Установка минимальной суммы заказа, доступно только для скидки с типом Промокод");
				}

				promoCode.OrderMinSum = value;
				OnPropertyChanged();
			}
		}
		
		public bool IsOneTimePromoCode
		{
			get => Entity is PromoCodeDiscount promoCode && promoCode.IsOneTimePromoCode;
			set
			{
				if(!(Entity is PromoCodeDiscount promoCode))
				{
					throw new InvalidOperationException("Установка одноразового промокода, доступно только для скидки с типом Промокод");
				}

				promoCode.IsOneTimePromoCode = value;
				OnPropertyChanged();
			}
		}
		
		public DiscountUnits DiscountValueType
		{
			get => Entity.ValueType;
			set
			{
				Entity.ValueType = value;
				OnPropertyChanged();
			}
		}

		public IEnumerable<object> SelectedNomenclatures
		{
			get => _selectedNomenclatures;
			set
			{
				if(SetField(ref _selectedNomenclatures, value))
				{
					OnPropertyChanged(nameof(IsNomenclaturesSelected));
					OnPropertyChanged(nameof(CanRemoveNomenclatures));
				}
			} 
		}
		
		public IEnumerable<object> SelectedProductGroups
		{
			get => _selectedProductGroups;
			set
			{
				if(SetField(ref _selectedProductGroups, value))
				{
					OnPropertyChanged(nameof(IsProductGroupsSelected));
					OnPropertyChanged(nameof(CanRemoveProductGroup));
				}
			} 
		}
		
		public bool SelectedAllCategories
		{
			get => _selectedAllCategories;
			set
			{
				if(SetField(ref _selectedAllCategories, value))
				{
					UpdateSelectedCategories(_selectedAllCategories);
				}
			}
		}

		public IList<SelectableNomenclatureCategoryNode> SelectableNomenclatureCategoryNodes { get; private set; }
		
		public ICommand SaveCommand { get; private set; }
		public ICommand CloseCommand { get; private set; }
		public ICommand AddProductGroupCommand { get; private set; }
		public ICommand RemoveProductGroupCommand { get; private set; }
		public ICommand AddNomenclatureCommand { get; private set; }
		public ICommand RemoveNomenclatureCommand { get; private set; }

		public int CurrentPage
		{
			get => _currentPage;
			set => SetField(ref _currentPage, value);
		}

		public bool HasOrderMinSum
		{
			get => _hasOrderMinSum;
			set
			{
				if(SetField(ref _hasOrderMinSum, value) && !value)
				{
					if(Entity is PromoCodeDiscount promoCode)
					{
						promoCode.ResetOrderMinSum();
					}
				}
			}
		}

		public bool HasPromoCodeDurationTime
		{
			get => _hasPromoCodeDurationTime;
			set
			{
				if(SetField(ref _hasPromoCodeDurationTime, value) && !value)
				{
					if(Entity is PromoCodeDiscount promoCode)
					{
						promoCode.ResetTimeDuration();
					}
				}
			}
		}

		public bool CanEditPromoCode { get; private set; }
		public bool CanChangePromoCodeName => IsNewEntity && CanEditPromoCode;
		public bool IsPromoCode => Entity.DiscountReasonType == DiscountReasonType.PromoCode;
		public bool CanShowApplicabilitiesByTypes => Entity.DiscountReasonType != DiscountReasonType.Discount;
		public bool DiscountInfoTabActive
		{
			get => _discountInfoTabActive;
			set
			{
				if(SetField(ref _discountInfoTabActive, value) && value)
				{
					CurrentPage = 0;
				}
			}
		}
		
		public bool PromoCodeSettingsTabActive
		{
			get => _promoCodeSettingsTabActive;
			set
			{
				if(SetField(ref _promoCodeSettingsTabActive, value) && value)
				{
					CurrentPage = 1;
				}
			}
		}

		public void UpdateNomenclatureCategories(SelectableNomenclatureCategoryNode selectedCategory) =>
			Entity.UpdateNomenclatureCategories(selectedCategory);
		
		private void InitializeEntity(IEntityViewModelContext viewModelContext)
		{
			if(viewModelContext is null)
			{
				throw new ArgumentNullException(nameof(viewModelContext));
			}
			
			if(!viewModelContext.EntityId.HasValue)
			{
				Entity = new DiscountReason();
			}
			else
			{
				Entity = (DiscountReasonBase)UoW.GetById(viewModelContext.EntityType, viewModelContext.EntityId.Value);
			}

			_promotionalSets = Entity.PromoSets;
			TabName = IsNewEntity ? "Новое основание для скидки" : $"Основание для скидки \"{Entity.Name}\"";
		}
		
		private void SetPermissions()
		{
			_permissionResult = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DiscountReason));
			CanEditPromoCode = _commonServices.CurrentPermissionService.ValidatePresetPermission(
				Vodovoz.Permissions.DiscountReasonPermissions.CanEditPromoCode)
				&& CanEditDiscountReason;
		}
		
		private void InitializeCommands()
		{
			SaveCommand = new DelegateCommand(Save);
			CloseCommand = new DelegateCommand(()=> Close(false, CloseSource.Cancel));
			AddProductGroupCommand = new DelegateCommand(AddProductGroup);
			RemoveProductGroupCommand = new DelegateCommand(RemoveProductGroup);
			AddNomenclatureCommand = new DelegateCommand(AddNomenclature);
			RemoveNomenclatureCommand = new DelegateCommand(RemoveNomenclatures);
		}
		
		private void UpdateSelectedCategories(bool value)
		{
			foreach(var node in SelectableNomenclatureCategoryNodes)
			{
				node.IsSelected = value;
				UpdateNomenclatureCategories(node);
			}
		}
		
		private void AddNomenclature()
		{
			NavigationManager.OpenViewModel<NomenclaturesJournalViewModel>(this,
				OpenPageOptions.AsSlave,
				vm =>
				{
					vm.SelectionMode = JournalSelectionMode.Multiple;
					vm.OnSelectResult += OnNomenclaturesSelected;
				});
		}

		private void OnNomenclaturesSelected(object sender, JournalSelectedEventArgs ea)
		{
			(sender as NomenclaturesJournalViewModel).OnSelectResult -= OnNomenclaturesSelected;
			
			var selectedNodes = ea.SelectedObjects
				.Cast<INamedDomainObject>()
				.ToList();

			if(!selectedNodes.Any())
			{
				return;
			}

			Entity.AddNomenclatures(selectedNodes);
		}

		private void RemoveNomenclatures()
		{
			Entity.RemoveNomenclatures(_selectedNomenclatures);
		}

		private void AddProductGroup()
		{
			NavigationManager.OpenViewModel<ProductGroupsJournalViewModel, Action<ProductGroupsJournalFilterViewModel>>(
				this,
				filter =>
				{
					filter.IsGroupSelectionMode = true;
				},
				OpenPageOptions.AsSlave,
				vm =>
				{
					vm.SelectionMode = JournalSelectionMode.Multiple;
					vm.OnSelectResult += OnProductGroupsSelected;
				});
		}

		private void RemoveProductGroup()
		{
			Entity.RemoveProductGroups(_selectedProductGroups);
		}

		private void OnProductGroupsSelected(object sender, JournalSelectedEventArgs e)
		{
			(sender as ProductGroupsJournalViewModel).OnSelectResult -= OnProductGroupsSelected;
			
			var selectedGroups = e.SelectedObjects
				.Cast<INamedDomainObject>()
				.ToList();

			if(!selectedGroups.Any())
			{
				return;
			}

			Entity.AddProductGroups(selectedGroups);
		}
		
		private void InitializeNomenclatureCategoriesList()
		{
			SelectableNomenclatureCategoryNodes = new GenericObservableList<SelectableNomenclatureCategoryNode>();
			var discountNomenclatureCategories = UoW.GetAll<DiscountReasonNomenclatureCategory>().ToList();
			
			foreach(var category in discountNomenclatureCategories)
			{
				SelectableNomenclatureCategoryNodes.Add(
					SelectableNomenclatureCategoryNode.Create(
						category,
						Entity.NomenclatureCategories.Contains(category)));
			}
		}
		
		private void InitializeDiscountTypes()
		{
			UpdateDiscountTypes();
		}

		private void UpdateDiscountTypes()
		{
			var discountTypeUses = new Dictionary<DiscountType, UseDiscountType?>();

			if(Entity.DiscountApplicabilities.Any())
			{
				foreach(var discountApplicability in Entity.DiscountApplicabilities)
				{
					discountTypeUses.Add(discountApplicability.DiscountType, discountApplicability.UseDiscountType);
				}
			}
			else if(Entity.DiscountReasonType != DiscountReasonType.Discount)
			{
				foreach(DiscountType discountType in Enum.GetValues(typeof(DiscountType)))
				{
					if((int)discountType != (int)Entity.DiscountReasonType)
					{
						discountTypeUses.Add(discountType, null);
					}
				}
			}

			DiscountTypeUses = discountTypeUses;
		}

		private void InitializeHasOrderMinSum()
		{
			HasOrderMinSum = Entity is PromoCodeDiscount promoCode && promoCode.HasOrderMinSum;
		}

		private void InitializeHasPromoCodeDurationTime()
		{
			HasPromoCodeDurationTime = Entity is PromoCodeDiscount promoCode && promoCode.HasOrderMinSum;
		}

		private new void Save()
		{
			if(!Validate())
			{
				return;
			}
			
			Entity.UpdateDiscountApplicabilities(DiscountTypeUses);
			UoW.Save(Entity);
			UoW.Commit();
			
			Close(false, CloseSource.Save);
		}
		
		private bool Validate()
		{
			var validationDiscountTypeUsesResult = ValidateDiscountTypeUses().ToList();
			if(validationDiscountTypeUsesResult.Any())
			{
				var sb = new StringBuilder();
				
				foreach(var validationResult in validationDiscountTypeUsesResult)
				{
					sb.AppendLine(validationResult.ErrorMessage);	
				}

				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, sb.ToString());
				return false;
			}

			return _commonServices.ValidationService.Validate(Entity, new ValidationContext(Entity));
		}

		private IEnumerable<ValidationResult> ValidateDiscountTypeUses()
		{
			if(Entity.DiscountReasonType == DiscountReasonType.Discount)
			{
				if(Entity.DiscountApplicabilities.Any())
				{
					throw new InvalidOperationException(
						"Что-то пошло не так. У обычной скидки не может быть списка применимостей по типам!");
				}
			}
			else
			{
				foreach(var keyPairValue in DiscountTypeUses)
				{
					if(!keyPairValue.Value.HasValue)
					{
						yield return new ValidationResult($"Выберите тип использования для {keyPairValue.Key.GetEnumDisplayName()}");
					}
				}
			}
		}

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
