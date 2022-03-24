using System;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels;

namespace Vodovoz.Journals.JournalViewModels
{
    public class FinancialDistrictsSetsJournalViewModel 
        : FilterableSingleEntityJournalViewModelBase<FinancialDistrictsSet, 
                                                     FinancialDistrictsSetViewModel, 
                                                     FinancialDistrictsSetsJournalNode, 
                                                     FinancialDistrictsSetsJournalFilterViewModel>
    {
        private readonly IEmployeeService _employeeService;
        private readonly IEntityDeleteWorker _entityDeleteWorker;
		
        private readonly bool _canUpdate;
        private readonly bool _canCreate;
        private readonly bool _canActivateDistrictsSet;

        public FinancialDistrictsSetsJournalViewModel(
            FinancialDistrictsSetsJournalFilterViewModel filterViewModel,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
            IEmployeeService employeeService,
            IEntityDeleteWorker entityDeleteWorker,
			INavigationManager navigationManager = null,
            bool hideJournalForOpenDialog = false, 
            bool hideJournalForCreateDialog = false)
            : base(filterViewModel,
				unitOfWorkFactory,
				commonServices,
				navigationManager,
				hideJournalForOpenDialog,
				hideJournalForCreateDialog)
        {
            _entityDeleteWorker = entityDeleteWorker ?? throw new ArgumentNullException(nameof(entityDeleteWorker));
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			
            _canActivateDistrictsSet = commonServices.CurrentPermissionService.ValidatePresetPermission("can_activate_financial_districts_set");
            var permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(FinancialDistrictsSet));
            _canCreate = permissionResult.CanCreate;
            _canUpdate = permissionResult.CanUpdate;

            TabName = "Журнал версий финансовых районов";
            UpdateOnChanges(typeof(FinancialDistrictsSet));
        }
        
        protected override Func<IUnitOfWork, IQueryOver<FinancialDistrictsSet>> ItemsSourceQueryFunction => uow => {
			FinancialDistrictsSet districtsSetAlias = null;
			FinancialDistrictsSetsJournalNode resultAlias = null;
			Employee creatorAlias = null;
			
			var query = uow.Session.QueryOver<FinancialDistrictsSet>(() => districtsSetAlias)
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
				   .Select(() => creatorAlias.Name).WithAlias(() => resultAlias.AuthorName)
				   .Select(() => creatorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
				   .Select(() => creatorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
				)
				.TransformUsing(Transformers.AliasToBean<FinancialDistrictsSetsJournalNode>());
		};

		protected override Func<FinancialDistrictsSetViewModel> CreateDialogFunction => () =>
			new FinancialDistrictsSetViewModel(
				EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices, _entityDeleteWorker, _employeeService);

		protected override Func<FinancialDistrictsSetsJournalNode, FinancialDistrictsSetViewModel> OpenDialogFunction => node =>
			new FinancialDistrictsSetViewModel(
				EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices, _entityDeleteWorker, _employeeService);
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
			CreateCopyAction();
		}

		private void CreateCopyAction()
		{
			var copyAction = new JournalAction("Копировать",
				selectedItems => _canCreate && 
				                 selectedItems.OfType<FinancialDistrictsSetsJournalNode>().FirstOrDefault() != null,
				selected => true,
				selected => {
					var selectedNode = selected.OfType<FinancialDistrictsSetsJournalNode>().FirstOrDefault();
					
					if(selectedNode == null)
						return;
					
					var districtsSetToCopy = UoW.GetById<FinancialDistrictsSet>(selectedNode.Id);
					var alreadyCopiedDistrict = 
						UoW.Session.QueryOver<FinancialDistrict>()
								   .WhereRestrictionOn(x => x.CopyOf.Id)
								   .IsIn(districtsSetToCopy.FinancialDistricts.Select(x => x.Id).ToArray())
								   .Take(1)
								   .SingleOrDefault();
					
					if(alreadyCopiedDistrict != null) {
						commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
							$"Выбранная версия районов уже была скопирована\nКопия: {alreadyCopiedDistrict.FinancialDistrictsSet.Id} {alreadyCopiedDistrict.FinancialDistrictsSet.Name}");
						return;
					}
					
					if(commonServices.InteractiveService.Question($"Скопировать версию районов \"{selectedNode.Name}\"")) {
						var copy = districtsSetToCopy.Clone() as FinancialDistrictsSet;
						copy.Name += " - копия";
						copy.Author = _employeeService.GetEmployeeForUser(UoW, commonServices.UserService.CurrentUserId);
						copy.Status = DistrictsSetStatus.Draft;
						copy.DateCreated = DateTime.Now;
						
						UoW.Save(copy);
						UoW.Commit();
						commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Копирование завершено");
						Refresh();
					}
				}
			);
			NodeActionsList.Add(copyAction);
		}

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
						&& selectedItems.OfType<FinancialDistrictsSetsJournalNode>().FirstOrDefault()?.Status == DistrictsSetStatus.Draft,
					selectedItems => true,
					selectedItems => {
						var selectedNodes = selectedItems.OfType<FinancialDistrictsSetsJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						
						if(selectedNode == null)
							return;
						
						var activeFinancialDistrictsSet = UoW.Session.QueryOver<FinancialDistrictsSet>()
															.Where(x => x.Status == DistrictsSetStatus.Active)
															.Take(1)
															.SingleOrDefault();
						
						if(activeFinancialDistrictsSet?.DateCreated > selectedNode.DateCreated) {
							commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Нельзя активировать, так как дата создания выбранной версии меньше чем дата создания активной версии");
							return;
						}
						
						var selectedFinancialDistrictsSet = UoW.GetById<FinancialDistrictsSet>(selectedNode.Id);
						
						FinancialDistrictSetActivation(selectedFinancialDistrictsSet, activeFinancialDistrictsSet);
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
						selectedItems.OfType<FinancialDistrictsSetsJournalNode>().FirstOrDefault()?.Status == DistrictsSetStatus.Draft,
					selectedItems => true,
					selectedItems => {
						var selectedNodes = selectedItems.OfType<FinancialDistrictsSetsJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						
						if(selectedNode != null) {
							var districtsSet = UoW.GetById<FinancialDistrictsSet>(selectedNode.Id);
							
							if(districtsSet != null) {
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
						selectedItems.OfType<FinancialDistrictsSetsJournalNode>().FirstOrDefault()?.Status == DistrictsSetStatus.Closed,
					selectedItems => true,
					selectedItems => {
						var selectedNodes = selectedItems.OfType<FinancialDistrictsSetsJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						
						if(selectedNode != null) {
							var districtsSet = UoW.GetById<FinancialDistrictsSet>(selectedNode.Id);
							
							if(districtsSet != null) {
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

		private void FinancialDistrictSetActivation(FinancialDistrictsSet selectedDistrictsSet, 
			FinancialDistrictsSet activeFinancialDistrictSet)
		{
			if (activeFinancialDistrictSet != null)
			{
				activeFinancialDistrictSet.Status = DistrictsSetStatus.Draft;
				activeFinancialDistrictSet.DateActivated = null;
				UoW.Save(activeFinancialDistrictSet);
			}
			
			selectedDistrictsSet.Status = DistrictsSetStatus.Active;
			selectedDistrictsSet.DateActivated = DateTime.Now;
				
			UoW.Save(selectedDistrictsSet);
			UoW.Commit();
			Refresh();
		}
    }
}
