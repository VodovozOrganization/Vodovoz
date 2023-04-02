using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Parameters;

namespace Vodovoz.ViewModels.ViewModels.Settings
{
	public class GeneralSettingsViewModel : TabViewModelBase
	{
		private readonly IGeneralSettingsParametersProvider _generalSettingsParametersProvider;
		private readonly ICommonServices _commonServices;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private const int _routeListPrintedFormPhonesLimitSymbols = 500;

		private string _routeListPrintedFormPhones;
		private bool _canAddForwardersToLargus;
		private DelegateCommand _saveRouteListPrintedFormPhonesCommand;
		private DelegateCommand _saveCanAddForwardersToLargusCommand;
		private DelegateCommand _saveOrderAutoCommentCommand;
		private DelegateCommand _showAutoCommentInfoCommand;
		private string _orderAutoComment;
		private GenericObservableList<Subdivision> _observableSubdivisions = new GenericObservableList<Subdivision>();
		private Subdivision _selectedSubdivision;

		public GeneralSettingsViewModel(
			IGeneralSettingsParametersProvider generalSettingsParametersProvider,
			ICommonServices commonServices,
			RoboatsSettingsViewModel roboatsSettingsViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation = null) : base(commonServices?.InteractiveService, navigation)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			RoboatsSettingsViewModel = roboatsSettingsViewModel ?? throw new ArgumentNullException(nameof(roboatsSettingsViewModel));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_generalSettingsParametersProvider =
				generalSettingsParametersProvider ?? throw new ArgumentNullException(nameof(generalSettingsParametersProvider));

			TabName = "Общие настройки";

			RouteListPrintedFormPhones = _generalSettingsParametersProvider.GetRouteListPrintedFormPhones;
			CanAddForwardersToLargus = _generalSettingsParametersProvider.GetCanAddForwardersToLargus;
			CanEditRouteListPrintedFormPhones =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_route_List_printed_form_phones");
			CanEditCanAddForwardersToLargus =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_can_add_forwarders_to_largus");
			CanEditOrderAutoComment =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("сan_edit_order_auto_comment_setting");

			CanEditComplaintWithoutDriverSubdivisions = CanEditRouteListPrintedFormPhones;

			OrderAutoComment = _generalSettingsParametersProvider.OrderAutoComment;

			AddSubdivisionCommand = new DelegateCommand(AddSubdivision);
			RemoveSubdivisionCommand = new DelegateCommand(RemoveSubdivision, () => CanRemoveSubdivision);
			SaveSubdivisionsCommand = new DelegateCommand(SaveSubdivisions);
			ShowSubdivisionsToInformComplaintHasNoDriverInfoCommand = new DelegateCommand(ShowSubdivisionsToInformComplaintHasNoDriverInfo);

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot())
			{
				unitOfWork.Session.DefaultReadOnly = true;

				var subdivisionIdToRetrieve = _generalSettingsParametersProvider.SubdivisionsToInformComplaintHasNoDriver;

				var retrievedSubdivisions = unitOfWork.Session.Query<Subdivision>()
					.Where(subdivision => subdivisionIdToRetrieve.Contains(subdivision.Id))
					.ToList();

				foreach(var subdivision in retrievedSubdivisions)
				{
					ObservableSubdivisions.Add(subdivision);
				}
			}
		}

		#region RouteListPrintedFormPhones

		public bool CanEditRouteListPrintedFormPhones { get; }

		public string RouteListPrintedFormPhones
		{
			get => _routeListPrintedFormPhones;
			set => SetField(ref _routeListPrintedFormPhones, value);
		}

		public DelegateCommand SaveRouteListPrintedFormPhonesCommand => _saveRouteListPrintedFormPhonesCommand
			?? (_saveRouteListPrintedFormPhonesCommand = new DelegateCommand(SaveRouteListPrintedFormPhones)
			);

		private void SaveRouteListPrintedFormPhones()
		{
			if(!ValidateRouteListPrintedFormPhones())
			{
				return;
			}

			_generalSettingsParametersProvider.UpdateRouteListPrintedFormPhones(RouteListPrintedFormPhones);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
		}

		private bool ValidateRouteListPrintedFormPhones()
		{
			if(string.IsNullOrWhiteSpace(RouteListPrintedFormPhones))
			{
				ShowWarningMessage("Строка с телефонами для печатной формы МЛ не может быть пуста!");
				return false;
			}

			if(RouteListPrintedFormPhones != null && RouteListPrintedFormPhones.Length > _routeListPrintedFormPhonesLimitSymbols)
			{
				ShowWarningMessage(
					$"Строка с телефонами для печатной формы МЛ не может превышать {_routeListPrintedFormPhonesLimitSymbols} символов!");
				return false;
			}

			return true;
		}

		#endregion

		#region CanAddForwardersToLargus

		public bool CanEditCanAddForwardersToLargus { get; }

		public bool CanAddForwardersToLargus
		{
			get => _canAddForwardersToLargus;
			set => SetField(ref _canAddForwardersToLargus, value);
		}

		public DelegateCommand SaveCanAddForwardersToLargusCommand => _saveCanAddForwardersToLargusCommand
			?? (_saveCanAddForwardersToLargusCommand = new DelegateCommand(() =>
				{
					_generalSettingsParametersProvider.UpdateCanAddForwardersToLargus(CanAddForwardersToLargus);
					_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
				})
			);

		public RoboatsSettingsViewModel RoboatsSettingsViewModel { get; }

		#endregion

		#region OrderAutoComment

		public string OrderAutoComment
		{
			get => _orderAutoComment;
			set => SetField(ref _orderAutoComment, value);
		}

		public bool CanEditOrderAutoComment { get; }

		public DelegateCommand SaveOrderAutoCommentCommand =>
			_saveOrderAutoCommentCommand ?? (_saveOrderAutoCommentCommand = new DelegateCommand(() =>
			{
				_generalSettingsParametersProvider.UpdateOrderAutoComment(OrderAutoComment);
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
			}));

		public DelegateCommand ShowAutoCommentInfoCommand =>
			_showAutoCommentInfoCommand ?? (_showAutoCommentInfoCommand = new DelegateCommand(() =>
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Info,
					"Если в заказе стоит бесконтактная доставка и доставляется промонабор для новых клиентов (в наборе не стоит галочка \"для многократного использования\"),\n" +
					"то в начало комментария к заказу добавляется текст из настройки."
					);
			}));

		#endregion

		#region Complaints

		public bool CanEditComplaintWithoutDriverSubdivisions { get; }

		public GenericObservableList<Subdivision> ObservableSubdivisions
		{
			get => _observableSubdivisions;
			set => SetField(ref _observableSubdivisions, value);
		}

		[PropertyChangedAlso(nameof(CanRemoveSubdivision))]
		public Subdivision SelectedSubdivision
		{
			get => _selectedSubdivision;
			set => SetField(ref _selectedSubdivision, value);
		}

		private List<int> _subdivisionsToAdd = new List<int>();
		private List<int> _subdivisionsToRemoves = new List<int>();

		public DelegateCommand AddSubdivisionCommand { get; }
		public DelegateCommand RemoveSubdivisionCommand { get; }
		public DelegateCommand SaveSubdivisionsCommand { get; }
		public DelegateCommand ShowSubdivisionsToInformComplaintHasNoDriverInfoCommand { get; }

		public bool CanRemoveSubdivision => CanEditComplaintWithoutDriverSubdivisions && SelectedSubdivision != null;

		private void AddSubdivision()
		{
			var page = NavigationManager.OpenViewModel<SubdivisionsJournalViewModel>(this);
			page.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			page.ViewModel.OnEntitySelectedResult += OnSubdivisionsToAddSelected;
		}

		private void OnSubdivisionsToAddSelected(object sender, JournalSelectedNodesEventArgs e)
		{
			if(!(sender is SubdivisionsJournalViewModel viewModel))
			{
				return;
			}

			var selectedIds = e.SelectedNodes.Select(x => x.Id);

			if(!selectedIds.Any())
			{
				return;
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot())
			{
				unitOfWork.Session.DefaultReadOnly = true;

				foreach(var id in selectedIds)
				{
					if(!_subdivisionsToAdd.Contains(id)
						&& !_subdivisionsToRemoves.Contains(id))
					{
						_subdivisionsToAdd.Add(id);
					}

					if(_subdivisionsToRemoves.Contains(id))
					{
						_subdivisionsToRemoves.Remove(id);
					}
				}

				var subdivisionIdToRetrieve = _subdivisionsToAdd.Except(ObservableSubdivisions.Select(x => x.Id));

				var retrievedSubdivisions = unitOfWork.Session.Query<Subdivision>()
					.Where(subdivision => subdivisionIdToRetrieve.Contains(subdivision.Id))
					.ToList();

				foreach(var subdivision in retrievedSubdivisions)
				{
					ObservableSubdivisions.Add(subdivision);
				}
			}

			viewModel.OnEntitySelectedResult -= OnSubdivisionsToAddSelected;
		}

		private void RemoveSubdivision()
		{
			var currentlySelected = SelectedSubdivision.Id;
			var itemToRemove = ObservableSubdivisions.FirstOrDefault(x => x.Id == currentlySelected);

			if(itemToRemove is null)
			{
				return;
			}

			if(!_subdivisionsToRemoves.Contains(currentlySelected)
				&& !_subdivisionsToAdd.Contains(currentlySelected))
			{
				_subdivisionsToRemoves.Add(currentlySelected);
			}

			if(_subdivisionsToAdd.Contains(currentlySelected))
			{
				_subdivisionsToAdd.Remove(currentlySelected);
			}

			ObservableSubdivisions.Remove(itemToRemove);
		}

		private void SaveSubdivisions()
		{
			_generalSettingsParametersProvider.UpdateSubdivisionsToInformComplaintHasNoDriver(
				_generalSettingsParametersProvider.SubdivisionsToInformComplaintHasNoDriver
					.Concat(_subdivisionsToAdd)
					.Except(_subdivisionsToRemoves)
					.Distinct()
					.ToArray());

			_subdivisionsToAdd.Clear();
			_subdivisionsToRemoves.Clear();
		}

		private void ShowSubdivisionsToInformComplaintHasNoDriverInfo()
		{
			_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Info,
					"Сотрудники данных отделов будут проинформированы о незаполненном водителе при закрытии рекламации. " +
					"Если отдел есть в списке ответственных и итог работы по сотрудникам: Вина доказана.");
		}

		#endregion
	}
}
