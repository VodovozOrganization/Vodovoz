using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.Validation;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.ViewModels.Settings
{
	public class EmployeeFixedPricesViewModel : ViewModelBase, IDisposable
	{
		private const string _processUpdateFixedPricesWasCancelled = "Процесс обновления фиксы сотрудников ВВ был отменён";
		private const string _updatingEmployeeFixedPrices = "Обновление фиксы сотрудников ВВ...";
		private readonly ILogger<EmployeeFixedPricesViewModel> _logger;
		private readonly IInteractiveMessage _interactiveMessage;
		private readonly INavigationManager _navigationManager;
		private readonly IValidator _validator;
		private readonly IValidationViewFactory _validationResultViewFactory;
		private readonly INomenclatureFixedPriceController _nomenclatureFixedPriceController;
		private readonly IUnitOfWork _uow;
		private INamedDomainObject _selectedNomenclature;
		private NomenclatureFixedPrice _selectedFixedPrice;
		private DialogViewModelBase _parentViewModel;

		public EmployeeFixedPricesViewModel(
			ILogger<EmployeeFixedPricesViewModel> logger,
			IInteractiveMessage interactiveMessage,
			INavigationManager navigationManager,
			IUnitOfWorkFactory unitOfWorkFactory,
			IValidator validator,
			DialogViewModelBase parentViewModel,
			IValidationViewFactory validationResultViewFactory,
			INomenclatureFixedPriceController nomenclatureFixedPriceController,
			ICurrentPermissionService currentPermissionService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_uow = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory)))
				.CreateWithoutRoot(_updatingEmployeeFixedPrices);
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
			_parentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
			_validationResultViewFactory =
				validationResultViewFactory ?? throw new ArgumentNullException(nameof(validationResultViewFactory));
			_nomenclatureFixedPriceController =
				nomenclatureFixedPriceController ?? throw new ArgumentNullException(nameof(nomenclatureFixedPriceController));

			CanChangeEmployeesFixedPrices =
				(currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService)))
				.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.SettingsPermissions.CanEditEmployeeFixedPrices);
			InitializeEmployeesFixedPricesSettings();
		}

		public bool CanChangeEmployeesFixedPrices { get; }

		public INamedDomainObject SelectedNomenclature
		{
			get => _selectedNomenclature;
			set
			{
				if(SetField(ref _selectedNomenclature, value))
				{
					OnPropertyChanged(nameof(CanRemoveNomenclature));
					OnPropertyChanged(nameof(CanAddFixedPrice));
				}
			}
		}
		
		public NomenclatureFixedPrice SelectedFixedPrice
		{
			get => _selectedFixedPrice;
			set
			{
				if(SetField(ref _selectedFixedPrice, value))
				{
					OnPropertyChanged(nameof(CanRemoveFixedPrice));
				}
			}
		}
		
		public DelegateCommand SaveEmployeesFixedPricesCommand { get; private set; }
		public DelegateCommand AddNomenclatureForFixedPriceCommand { get; private set; }
		public DelegateCommand RemoveNomenclatureForFixedPriceCommand { get; private set; }
		public DelegateCommand RemoveFixedPriceCommand { get; private set; }
		public DelegateCommand AddFixedPriceCommand { get; private set; }
		public IList<INamedDomainObject> Nomenclatures { get; private set; }
		public IDictionary<int, IList<NomenclatureFixedPrice>> FixedPrices { get; } =
			new Dictionary<int, IList<NomenclatureFixedPrice>>();
		
		public bool CanRemoveNomenclature => CanChangeEmployeesFixedPrices && SelectedNomenclature != null;
		public bool CanRemoveFixedPrice => CanChangeEmployeesFixedPrices && SelectedFixedPrice != null;
		public bool CanAddFixedPrice => CanChangeEmployeesFixedPrices && SelectedNomenclature != null;
		private IList<NomenclatureFixedPrice> DeletedFixedPrices { get; set; }
		
		private void InitializeEmployeesFixedPricesSettings()
		{
			SaveEmployeesFixedPricesCommand = new DelegateCommand(SaveEmployeesFixedPrices);
			AddNomenclatureForFixedPriceCommand = new DelegateCommand(AddNomenclatureForFixedPrice);
			RemoveNomenclatureForFixedPriceCommand = new DelegateCommand(RemoveNomenclatureForFixedPrice);
			AddFixedPriceCommand = new DelegateCommand(AddFixedPrice);
			RemoveFixedPriceCommand = new DelegateCommand(RemoveSelectedFixedPrice);
			
			GetNomenclaturesWithFixedPrices();
			DeletedFixedPrices = new List<NomenclatureFixedPrice>();
		}

		private void SaveEmployeesFixedPrices()
		{
			var allFixedPrices = FixedPrices.Values.SelectMany(x => x).ToArray();

			var validationResults = ValidateEmployeeFixedPrices(allFixedPrices);

			if(validationResults.Any())
			{
				var view = _validationResultViewFactory.CreateValidationView(validationResults);
				view.ShowModal();
				return;
			}
			
			UpdateAllEmployeeFixedPrices(allFixedPrices);
		}

		private ICollection<ValidationResult> ValidateEmployeeFixedPrices(NomenclatureFixedPrice[] allFixedPrices)
		{
			var validationResults = new List<ValidationResult>();

			foreach(var nomenclature in Nomenclatures)
			{
				FixedPrices.TryGetValue(nomenclature.Id, out var fixedPrices);
				
				if(fixedPrices is null || !fixedPrices.Any())
				{
					validationResults.Add(new ValidationResult($"Номенклатура {nomenclature.Name} не содержит фиксы"));
				}
			}
			
			foreach(var fixedPrice in allFixedPrices)
			{
				if(!_validator.Validate(fixedPrice, new ValidationContext(fixedPrice), false))
				{
					validationResults.AddRange(_validator.Results);
				}
			}

			return validationResults;
		}

		private void UpdateAllEmployeeFixedPrices(NomenclatureFixedPrice[] fixedPrices)
		{
			_logger.LogInformation("Запущен процесс обновления фиксы сотрудников ВВ");

			IPage<ProgressWindowViewModel> progressWindow = null;
			var cts = new CancellationTokenSource();
			var isProgressWindowClosed = false;

			void OnProgressWindowClosed(object sender, PageClosedEventArgs args)
			{
				cts.Cancel();
				isProgressWindowClosed = true;
			}

			try
			{
				progressWindow = _navigationManager.OpenViewModel<ProgressWindowViewModel>(_parentViewModel);
				progressWindow.PageClosed += OnProgressWindowClosed;
				var progressBarDisplayable = progressWindow.ViewModel.Progress;
				progressBarDisplayable.Update(_updatingEmployeeFixedPrices);
				
				_nomenclatureFixedPriceController.UpdateAllEmployeeFixedPrices(_uow, fixedPrices, DeletedFixedPrices, cts.Token);
				progressBarDisplayable.Update("Сохраняем данные...");
				_uow.Commit();
				
				_logger.LogInformation("Процесс обновления фиксы сотрудников ВВ завершён");
				DeletedFixedPrices.Clear();
			}
			catch(OperationCanceledException)
			{
				_logger.LogDebug(_processUpdateFixedPricesWasCancelled);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"{ProcessUpdateFixedPricesWasCancelled}: {ExceptionMessage}",
					_processUpdateFixedPricesWasCancelled,
					e.Message);
				_interactiveMessage.ShowMessage(ImportanceLevel.Error, $"{_processUpdateFixedPricesWasCancelled}: {e.Message}");
			}
			finally
			{
				if(progressWindow != null)
				{
					progressWindow.PageClosed -= OnProgressWindowClosed;
					if(!isProgressWindowClosed)
					{
						_navigationManager.ForceClosePage(progressWindow);
					}
				}
			}
		}

		private void AddNomenclatureForFixedPrice()
		{
			_navigationManager.OpenViewModel<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
				_parentViewModel,
				f =>
				{
					f.RestrictCategory = NomenclatureCategory.water;
					f.RestrictedExcludedIds = FixedPrices.Keys.ToArray();
				},
				OpenPageOptions.AsSlave,
				vm =>
				{
					vm.SelectionMode = JournalSelectionMode.Multiple;
					vm.OnSelectResult += OnSelectResult;
					return;

					void OnSelectResult(object sender, JournalSelectedEventArgs e)
					{
						var selectedNodes = e.GetSelectedObjects<NomenclatureJournalNode>().ToArray();

						if(!selectedNodes.Any())
						{
							return;
						}

						foreach(var node in selectedNodes)
						{
							var namedDomainObject = new NamedDomainObjectNode
							{
								Id = node.Id,
								Name = node.Name
							};
							
							Nomenclatures.Add(namedDomainObject);
							
							FixedPrices.Add(
								node.Id,
								new GenericObservableList<NomenclatureFixedPrice>
								{
									NomenclatureFixedPrice.CreateEmployeeFixedPrice(namedDomainObject)
								});
						}
					}
				});
		}
		
		private void RemoveNomenclatureForFixedPrice()
		{
			if(!FixedPrices.TryGetValue(SelectedNomenclature.Id, out var fixedPrices))
			{
				return;
			}

			foreach(var fixedPrice in fixedPrices)
			{
				UpdateRemovedFixedPrices(fixedPrice);
			}

			FixedPrices.Remove(SelectedNomenclature.Id);
			Nomenclatures.Remove(SelectedNomenclature);
		}

		private void AddFixedPrice()
		{
			var fixedPrice = NomenclatureFixedPrice.CreateEmployeeFixedPrice(SelectedNomenclature);
			
			if(!FixedPrices.TryGetValue(fixedPrice.Nomenclature.Id, out var fixedPrices))
			{
				return;
			}

			fixedPrices.Add(fixedPrice);
		}

		private void RemoveSelectedFixedPrice()
		{
			if(!FixedPrices.TryGetValue(SelectedFixedPrice.Nomenclature.Id, out var fixedPrices))
			{
				return;
			}
			
			UpdateRemovedFixedPrices(SelectedFixedPrice);
			fixedPrices.Remove(SelectedFixedPrice);
		}

		private void UpdateRemovedFixedPrices(NomenclatureFixedPrice fixedPrice)
		{
			if(fixedPrice.Id > 0)
			{
				DeletedFixedPrices.Add(fixedPrice);
			}
			else
			{
				_uow.Session.Evict(fixedPrice);
			}
		}
		
		private void GetNomenclaturesWithFixedPrices()
		{
			Nomenclatures = new GenericObservableList<INamedDomainObject>();
			
			var savedFixedPrices = _nomenclatureFixedPriceController.GetEmployeesNomenclatureFixedPrices(_uow);
			
			foreach(var item in savedFixedPrices)
			{
				var namedDomainObj = new NamedDomainObjectNode
				{
					Id = item.Nomenclature.Id,
					Name = item.Nomenclature.Name
				};

				if(!FixedPrices.TryGetValue(namedDomainObj.Id, out var fixedPrices))
				{
					var observableFixedPrices = new GenericObservableList<NomenclatureFixedPrice>
					{
						item
					};
					
					FixedPrices.Add(namedDomainObj.Id, observableFixedPrices);
					Nomenclatures.Add(namedDomainObj);
				}
				else
				{
					fixedPrices.Add(item);
				}
			}
		}

		public void Dispose()
		{
			_parentViewModel = null;
			_uow.Dispose();
		}
	}
}
