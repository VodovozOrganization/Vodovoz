using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.JournalNodes;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Settings.Delivery;
using Vodovoz.ViewModels.Logistic;
using static Vodovoz.ViewModels.Logistic.DistrictSetDiffReportConfirmationViewModel;

namespace Vodovoz.Journals.JournalViewModels
{
	public sealed class DistrictsSetJournalViewModel : EntityJournalViewModelBase<DistrictsSet, DistrictsSetViewModel, DistrictsSetJournalNode>
	{
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IUserService _userService;
		private readonly bool _сanChangeOnlineDeliveriesToday;
		private readonly IInteractiveService _interactiveService;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly bool _canUpdate;
		private readonly bool _canCreate;
		private readonly bool _canActivateDistrictsSet;

		private int? _diffSourceDistrictSetVersionId = null;
		private int? _diffTargetDistrictSetVersionId = null;

		public DistrictsSetJournalViewModel(
			DistrictsSetJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			IEmployeeRepository employeeRepository,
			IDeliveryRulesSettings deliveryRulesSettings,
			IUserService userService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService)
			: base(
				  unitOfWorkFactory,
				  interactiveService,
				  navigationManager,
				  deleteEntityService,
				  currentPermissionService)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_deliveryRulesSettings =
				deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));

			_canActivateDistrictsSet = CurrentPermissionService.ValidatePresetPermission("can_activate_districts_set");
			var permissionResult = CurrentPermissionService.ValidateEntityPermission(typeof(DistrictsSet));
			_canCreate = permissionResult.CanCreate;
			_canUpdate = permissionResult.CanUpdate;
			_сanChangeOnlineDeliveriesToday =
				CurrentPermissionService.ValidatePresetPermission("can_change_online_deliveries_today");

			if(filterViewModel != null)
			{
				JournalFilter = filterViewModel;
				filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			}

			VisibleDeleteAction = _userService.GetCurrentUser().IsAdmin;

			TabName = "Журнал версий районов";

			UseSlider = true;

			UpdateOnChanges(typeof(DistrictsSet));
			SetIsStoppedOnlineDeliveriesToday();
			CreatePopupActions();
		}

		private bool IsStoppedOnlineDeliveriesToday { get; set; }

		public bool CanGenerateDiffReport => DiffSourceDistrictSetVersionId != null && DiffTargetDistrictSetVersionId != null;

		[PropertyChangedAlso(nameof(CanGenerateDiffReport))]
		public int? DiffSourceDistrictSetVersionId
		{
			get => _diffSourceDistrictSetVersionId;
			set => SetField(ref _diffSourceDistrictSetVersionId, value);
		}

		[PropertyChangedAlso(nameof(CanGenerateDiffReport))]
		public int? DiffTargetDistrictSetVersionId
		{
			get => _diffTargetDistrictSetVersionId;
			set => SetField(ref _diffTargetDistrictSetVersionId, value);
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override IQueryOver<DistrictsSet> ItemsQuery(IUnitOfWork unitOfWork)
		{
			DistrictsSet districtsSetAlias = null;
			DistrictsSetJournalNode resultAlias = null;
			Employee creatorAlias = null;

			var query = unitOfWork.Session.QueryOver<DistrictsSet>(() => districtsSetAlias)
				.Left.JoinAlias(() => districtsSetAlias.Author, () => creatorAlias);

			query.Where(GetSearchCriterion(
				() => districtsSetAlias.Name,
				() => districtsSetAlias.Id
			));

			return query
				.SelectList(list => list
				   .Select(x => x.Id).WithAlias(() => resultAlias.Id)
				   .Select(x => x.Name).WithAlias(() => resultAlias.Name)
				   .Select(x => x.Status).WithAlias(() => resultAlias.Status)
				   .Select(x => x.DateCreated).WithAlias(() => resultAlias.DateCreated)
				   .Select(x => x.DateActivated).WithAlias(() => resultAlias.DateActivated)
				   .Select(x => x.DateClosed).WithAlias(() => resultAlias.DateClosed)
				   .Select(x => x.Comment).WithAlias(() => resultAlias.Comment)
				   .Select(() => creatorAlias.Name).WithAlias(() => resultAlias.AuthorName)
				   .Select(() => creatorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
				   .Select(() => creatorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic))
				.TransformUsing(Transformers.AliasToBean<DistrictsSetJournalNode>());
		}

		#region NodeActions

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();

			CreateCopyAction();

			CreateStartOnlineDeliveriesTodayAction();
			CreateStopOnlineDeliveriesTodayAction();
		}

		private void CreateCopyAction()
		{
			var copyAction = new JournalAction("Копировать",
				selectedItems => _canCreate && selectedItems.OfType<DistrictsSetJournalNode>().FirstOrDefault() != null,
				selected => true,
				selected =>
				{
					var selectedNode = selected.OfType<DistrictsSetJournalNode>().FirstOrDefault();

					if(selectedNode == null)
					{
						return;
					}

					var districtsSetToCopy = UoW.GetById<DistrictsSet>(selectedNode.Id);

					var districtsToCopy = districtsSetToCopy.Districts.Select(x => x.Id).ToList();

					var alreadyCopiedDistrictSets = UoW.GetAll<District>()
						.Where(d => districtsToCopy.Contains(d.CopyOf.Id))
						.Select(d => d.DistrictsSet)
						.Distinct()
						.ToList();

					var questionMessageBuilder = new StringBuilder();

					if(alreadyCopiedDistrictSets.Count > 0)
					{
						questionMessageBuilder.AppendLine("Выбранная версия районов уже была скопирована\n\nКопии:\n");

						foreach(var distrtictSet in alreadyCopiedDistrictSets)
						{
							questionMessageBuilder.AppendLine($"Код: ({distrtictSet.Id}) {distrtictSet.Name}");
						}

						questionMessageBuilder.AppendLine();
					}

					questionMessageBuilder.AppendLine($"Скопировать версию районов \"{selectedNode.Name}\"?");

					if(!_interactiveService.Question(questionMessageBuilder.ToString()))
					{
						return;
					}

					var copy = (DistrictsSet)districtsSetToCopy.Clone();
					copy.Name += " - копия";

					if(copy.Name.Length > DistrictsSet.NameMaxLength)
					{
						copy.Name = copy.Name.Remove(DistrictsSet.NameMaxLength);
					}

					copy.Author = _employeeRepository.GetEmployeeForCurrentUser(UoW);
					copy.Status = DistrictsSetStatus.Draft;
					copy.DateCreated = DateTime.Now;

					UoW.Save(copy);
					UoW.Commit();

					_interactiveService.ShowMessage(ImportanceLevel.Info, "Копирование завершено");

					Refresh();
				}
			);
			NodeActionsList.Add(copyAction);
		}

		private void CreateStartOnlineDeliveriesTodayAction()
		{
			var startOnlinesAction = new JournalAction("Запустить онлайны",
				selectedItems => true,
				selected => _сanChangeOnlineDeliveriesToday && IsStoppedOnlineDeliveriesToday,
				selected =>
				{
					_deliveryRulesSettings.UpdateOnlineDeliveriesTodayParameter("false");
					SetIsStoppedOnlineDeliveriesToday();
					UpdateJournalActions?.Invoke();
				}
			);
			NodeActionsList.Add(startOnlinesAction);
		}

		private void CreateStopOnlineDeliveriesTodayAction()
		{
			var stopOnlinesAction = new JournalAction("Остановить онлайны",
				selectedItems => true,
				selected => _сanChangeOnlineDeliveriesToday && !IsStoppedOnlineDeliveriesToday,
				selected =>
				{
					_deliveryRulesSettings.UpdateOnlineDeliveriesTodayParameter("true");
					SetIsStoppedOnlineDeliveriesToday();
					UpdateJournalActions?.Invoke();
				}
			);
			NodeActionsList.Add(stopOnlinesAction);
		}

		private void SetIsStoppedOnlineDeliveriesToday()
		{
			IsStoppedOnlineDeliveriesToday = _deliveryRulesSettings.IsStoppedOnlineDeliveriesToday;
		}

		#endregion NodeActions

		#region PopupActions

		protected override void CreatePopupActions()
		{
			PopupActionsList.Clear();
			CreateActivateNodeAction();
			CreateCloseNodeAction();
			CreateToDraftNodeAction();
		}

		private void CreateActivateNodeAction()
		{
			PopupActionsList.Add(
				new JournalAction(
					"Активировать",
					selectedItems => _canActivateDistrictsSet && _canUpdate
						&& selectedItems.OfType<DistrictsSetJournalNode>().FirstOrDefault()?.Status == DistrictsSetStatus.Draft,
					selectedItems => true,
					selectedItems =>
					{
						var selectedNodes = selectedItems.OfType<DistrictsSetJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode == null)
						{
							return;
						}

						var activeDistrictsSet = UoW.Session.QueryOver<DistrictsSet>().Where(x => x.Status == DistrictsSetStatus.Active).Take(1).SingleOrDefault();
						if(activeDistrictsSet?.DateCreated > selectedNode.DateCreated)
						{
							_interactiveService.ShowMessage(ImportanceLevel.Warning, "Нельзя активировать, так как дата создания выбранной версии меньше чем дата создания активной версии");
							return;
						}
						var selectedDistrictsSet = UoW.GetById<DistrictsSet>(selectedNode.Id);
						if(selectedDistrictsSet.Districts.Any(x => x.CopyOf == null)
							&& !_interactiveService.Question("Для выбранной версии невозможно перенести все приоритеты работы водителей\nПродолжить?"))
						{
							return;
						}

						NavigationManager.OpenViewModel<DistrictsSetActivationViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(selectedNode.Id));
					}
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Выбрать для сравнения",
					selectedItems => true,
					selectedItems => true,
					selectedItems =>
					{
						_diffSourceDistrictSetVersionId = selectedItems.Cast<DistrictsSetJournalNode>().First().Id;
					}));

			PopupActionsList.Add(
				new JournalAction(
					"Сравнить с выбранной версией",
					selectedItems => _diffSourceDistrictSetVersionId != null,
					selectedItems => true,
					selectedItems =>
					{
						_diffTargetDistrictSetVersionId = selectedItems.Cast<DistrictsSetJournalNode>().First().Id;

						NavigationManager.OpenViewModel<DistrictSetDiffReportConfirmationViewModel>(this, OpenPageOptions.AsSlave, vm =>
						{
							var versions = Items
								.Cast<DistrictsSetJournalNode>()
								.Where(x => x.Id == _diffSourceDistrictSetVersionId
									|| x.Id == _diffTargetDistrictSetVersionId)
								.OrderBy(x => x.DateCreated);

							vm.SourceDistrictSetId = versions.First().Id;
							vm.SourceDistrictSetName = $"[{versions.First().Id}] {versions.First().Name}";
							vm.TargetDistrictSetId = versions.Last().Id;
							vm.TargetDistrictSetName = $"[{versions.Last().Id}] {versions.Last().Name}";

							void OnPageClosed(object sender, DistrictSetDiffReportConfirmationClosedArgs e)
							{
								if(!e.Canceled)
								{
									_diffSourceDistrictSetVersionId = null;
								}
								_diffTargetDistrictSetVersionId = null;
								vm.Closed -= OnPageClosed;
							}

							vm.Closed += OnPageClosed;
						});
					}));
		}

		private void CreateCloseNodeAction()
		{
			PopupActionsList.Add(
				new JournalAction(
					"Закрыть",
					selectedItems => _canUpdate &&
						selectedItems.OfType<DistrictsSetJournalNode>().FirstOrDefault()?.Status == DistrictsSetStatus.Draft,
					selectedItems => true,
					selectedItems =>
					{
						var selectedNodes = selectedItems.OfType<DistrictsSetJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null)
						{
							var districtsSet = UoW.GetById<DistrictsSet>(selectedNode.Id);
							if(districtsSet != null)
							{
								districtsSet.Status = DistrictsSetStatus.Closed;
								districtsSet.DateClosed = DateTime.Now;
								districtsSet.DateActivated = null;
								UoW.Save(districtsSet);
								UoW.Commit();
								Refresh();
							}
						}
					}
				)
			);
		}

		private void CreateToDraftNodeAction()
		{
			PopupActionsList.Add(
				new JournalAction(
					"В черновик",
					selectedItems => _canUpdate &&
						selectedItems.OfType<DistrictsSetJournalNode>().FirstOrDefault()?.Status == DistrictsSetStatus.Closed,
					selectedItems => true,
					selectedItems =>
					{
						var selectedNodes = selectedItems.OfType<DistrictsSetJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null)
						{
							var districtsSet = UoW.GetById<DistrictsSet>(selectedNode.Id);
							if(districtsSet != null)
							{
								districtsSet.Status = DistrictsSetStatus.Draft;
								districtsSet.DateClosed = null;
								districtsSet.DateActivated = null;
								UoW.Save(districtsSet);
								UoW.Commit();
								Refresh();
							}
						}
					}
				)
			);
		}

		#endregion PopupActions
	}
}
