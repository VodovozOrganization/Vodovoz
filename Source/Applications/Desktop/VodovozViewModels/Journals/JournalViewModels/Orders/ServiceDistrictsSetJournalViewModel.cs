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
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.JournalNodes;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Settings.Delivery;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Orders;
using VodovozBusiness.Domain.Service;

namespace Vodovoz.Journals.JournalViewModels
{
	public sealed class ServiceDistrictsSetJournalViewModel : EntityJournalViewModelBase<ServiceDistrictsSet, ServiceDistrictsSetViewModel, ServiceDistrictsSetJournalNode>
	{
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IUserService _userService;
		private readonly IInteractiveService _interactiveService;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly bool _canUpdate;
		private readonly bool _canCreate;
		private readonly bool _canActivateServiceDistrictsSet;

		private int? _diffSourceDistrictSetVersionId = null;
		private int? _diffTargetDistrictSetVersionId = null;

		public ServiceDistrictsSetJournalViewModel(
			ServiceDistrictsSetJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			IEmployeeRepository employeeRepository,
			IDeliveryRulesSettings deliveryRulesSettings,
			IUserService userService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService)
			: base
			(
				unitOfWorkFactory,
				interactiveService,
				navigationManager,
				deleteEntityService,
				currentPermissionService
			)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_deliveryRulesSettings =
				deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));

			_canActivateServiceDistrictsSet = CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.ServiceDistrictsSetPermissions.CanActivateServiceDistrictsSet);
			var permissionResult = CurrentPermissionService.ValidateEntityPermission(typeof(ServiceDistrictsSet));
			_canCreate = permissionResult.CanCreate;
			_canUpdate = permissionResult.CanUpdate;

			if(filterViewModel != null)
			{
				JournalFilter = filterViewModel;
				filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			}

			VisibleDeleteAction = _userService.GetCurrentUser().IsAdmin;

			TabName = "Журнал версий сервисных районов";

			UseSlider = true;

			UpdateOnChanges(typeof(ServiceDistrictsSet));
			CreatePopupActions();
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override IQueryOver<ServiceDistrictsSet> ItemsQuery(IUnitOfWork unitOfWork)
		{
			ServiceDistrictsSet serviceDistrictsSetAlias = null;
			ServiceDistrictsSetJournalNode resultAlias = null;
			Employee creatorAlias = null;

			var query = unitOfWork.Session.QueryOver<ServiceDistrictsSet>(() => serviceDistrictsSetAlias)
				.Left.JoinAlias(() => serviceDistrictsSetAlias.Author, () => creatorAlias);

			query.Where(GetSearchCriterion(
				() => serviceDistrictsSetAlias.Name,
				() => serviceDistrictsSetAlias.Id
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
				.TransformUsing(Transformers.AliasToBean<ServiceDistrictsSetJournalNode>());
		}

		#region NodeActions

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();

			CreateCopyAction();
		}

		private void CreateCopyAction()
		{
			var copyAction = new JournalAction("Копировать",
				selectedItems => _canCreate && selectedItems.OfType<ServiceDistrictsSetJournalNode>().FirstOrDefault() != null,
				selected => true,
				selected =>
				{
					var selectedNode = selected.OfType<ServiceDistrictsSetJournalNode>().FirstOrDefault();

					if(selectedNode == null)
					{
						return;
					}

					var serviceDistrictsSetToCopy = UoW.GetById<ServiceDistrictsSet>(selectedNode.Id);

					var districtsToCopy = serviceDistrictsSetToCopy.ServiceDistricts.Select(x => x.Id).ToList();

					var alreadyCopiedDistrictSets = UoW.GetAll<ServiceDistrict>()
						.Where(d => districtsToCopy.Contains(d.CopyOf.Id))
						.Select(d => d.ServiceDistrictsSet)
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

					var copy = (ServiceDistrictsSet)serviceDistrictsSetToCopy.Clone();
					copy.Name += " - копия";

					if(copy.Name.Length > ServiceDistrictsSet.NameMaxLength)
					{
						copy.Name = copy.Name.Remove(ServiceDistrictsSet.NameMaxLength);
					}

					copy.Author = _employeeRepository.GetEmployeeForCurrentUser(UoW);
					copy.Status = ServiceDistrictsSetStatus.Draft;
					copy.DateCreated = DateTime.Now;

					UoW.Save(copy);
					UoW.Commit();

					_interactiveService.ShowMessage(ImportanceLevel.Info, "Копирование завершено");

					Refresh();
				}
			);
			NodeActionsList.Add(copyAction);
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
					selectedItems => _canActivateServiceDistrictsSet && _canUpdate
						&& selectedItems.OfType<ServiceDistrictsSetJournalNode>().FirstOrDefault()?.Status == ServiceDistrictsSetStatus.Draft,
					selectedItems => true,
					selectedItems =>
					{
						var selectedNodes = selectedItems.OfType<ServiceDistrictsSetJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode == null)
						{
							return;
						}

						var activeServiceDistrictsSet = UoW.Session.QueryOver<ServiceDistrictsSet>()
							.Where(x => x.Status == ServiceDistrictsSetStatus.Active).Take(1)
							.SingleOrDefault();

						if(activeServiceDistrictsSet?.DateCreated > selectedNode.DateCreated)
						{
							_interactiveService.ShowMessage(ImportanceLevel.Warning, "Нельзя активировать, так как дата создания выбранной версии меньше чем дата создания активной версии");
							return;
						}

						NavigationManager.OpenViewModel<ServiceDistrictsSetActivationViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(selectedNode.Id));
					}
				)
			);
		}

		private void CreateCloseNodeAction()
		{
			PopupActionsList.Add(
				new JournalAction(
					"Закрыть",
					selectedItems => _canUpdate &&
						selectedItems.OfType<ServiceDistrictsSetJournalNode>().FirstOrDefault()?.Status == ServiceDistrictsSetStatus.Draft,
					selectedItems => true,
					selectedItems =>
					{
						var selectedNodes = selectedItems.OfType<ServiceDistrictsSetJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null)
						{
							var serviceDistrictsSet = UoW.GetById<ServiceDistrictsSet>(selectedNode.Id);
							if(serviceDistrictsSet != null)
							{
								serviceDistrictsSet.Status = ServiceDistrictsSetStatus.Closed;
								serviceDistrictsSet.DateClosed = DateTime.Now;
								serviceDistrictsSet.DateActivated = null;
								UoW.Save(serviceDistrictsSet);
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
						selectedItems.OfType<ServiceDistrictsSetJournalNode>().FirstOrDefault()?.Status == ServiceDistrictsSetStatus.Closed,
					selectedItems => true,
					selectedItems =>
					{
						var selectedNodes = selectedItems.OfType<ServiceDistrictsSetJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null)
						{
							var serviceDistrictsSet = UoW.GetById<ServiceDistrictsSet>(selectedNode.Id);
							if(serviceDistrictsSet != null)
							{
								serviceDistrictsSet.Status = ServiceDistrictsSetStatus.Draft;
								serviceDistrictsSet.DateClosed = null;
								serviceDistrictsSet.DateActivated = null;
								UoW.Save(serviceDistrictsSet);
								UoW.Commit();
								Refresh();
							}
						}
					}
				)
			);
		}

		#endregion PopupActions

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
	}
}
