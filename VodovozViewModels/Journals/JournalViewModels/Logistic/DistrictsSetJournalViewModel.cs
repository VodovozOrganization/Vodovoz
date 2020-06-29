using System;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.JournalNodes;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Journals.JournalViewModels
{
	public sealed class DistrictsSetJournalViewModel : FilterableSingleEntityJournalViewModelBase<DistrictsSet, DistrictsSetViewModel, DistrictsSetJournalNode, DistrictsSetJournalFilterViewModel>
	{
		public DistrictsSetJournalViewModel(
			DistrictsSetJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeRepository employeeRepository,
			IEntityDeleteWorker entityDeleteWorker,
			bool hideJournalForOpenDialog = false, 
			bool hideJournalForCreateDialog = false)
			: base(filterViewModel, unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			this.entityDeleteWorker = entityDeleteWorker ?? throw new ArgumentNullException(nameof(entityDeleteWorker));
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			
			TabName = "Журнал версий районов";
			EnableDeleteButton = false;
			UpdateOnChanges(typeof(DistrictsSet));
		}

		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly IEmployeeRepository employeeRepository;
		private readonly IEntityDeleteWorker entityDeleteWorker;
		
		public bool EnableDeleteButton { get; set; }

		protected override Func<IUnitOfWork, IQueryOver<DistrictsSet>> ItemsSourceQueryFunction => uow => {
			DistrictsSet districtsSetAlias = null;
			DistrictsSetJournalNode resultAlias = null;
			Employee creatorAlias = null;
			
			var query = uow.Session.QueryOver<DistrictsSet>(() => districtsSetAlias)
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
				   .Select(() => creatorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
				)
				.TransformUsing(Transformers.AliasToBean<DistrictsSetJournalNode>());
		};

		protected override Func<DistrictsSetViewModel> CreateDialogFunction => () =>
			new DistrictsSetViewModel(EntityUoWBuilder.ForCreate(), unitOfWorkFactory, commonServices, entityDeleteWorker, employeeRepository);

		protected override Func<DistrictsSetJournalNode, DistrictsSetViewModel> OpenDialogFunction => node =>
			new DistrictsSetViewModel(EntityUoWBuilder.ForOpen(node.Id), unitOfWorkFactory, commonServices, entityDeleteWorker, employeeRepository);
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
			CreateCopyAction();
			
			if(EnableDeleteButton)
				CreateDefaultDeleteAction();
		}

		private void CreateCopyAction()
		{
			var copyAction = new JournalAction("Копировать",
				selectedItems => selectedItems.OfType<DistrictsSetJournalNode>().FirstOrDefault() != null,
				selected => true,
				selected => {
					var selectedNode = selected.OfType<DistrictsSetJournalNode>().FirstOrDefault();
					if(selectedNode == null)
						return;
					if(commonServices.InteractiveService.Question($"Скопировать версию районов \"{selectedNode.Name}\"")) {
						var districtsSetToCopy = UoW.GetById<DistrictsSet>(selectedNode.Id);
						var copy = districtsSetToCopy.Clone() as DistrictsSet;
						copy.Name += " - копия";
						copy.Author = employeeRepository.GetEmployeeForCurrentUser(UoW);
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
			
			PopupActionsList.Add(
				new JournalAction(
					"Активировать",
					selectedItems => {
						var selectedNodes = selectedItems.OfType<DistrictsSetJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						return selectedNode?.Status == DistrictsSetStatus.Draft;
					},
					selectedItems => true,
					selectedItems => {
						var selectedNodes = selectedItems.OfType<DistrictsSetJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null) {
							TabParent.AddSlaveTab(this, 
								new DistrictsSetActivationViewModel(EntityUoWBuilder.ForOpen(selectedNode.Id), QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory, commonServices)
							);
						}
					}
				)
			);
			
			PopupActionsList.Add(
				new JournalAction(
					"Закрыть",
					selectedItems => {
						var selectedNodes = selectedItems.Cast<DistrictsSetJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						return selectedNode?.Status == DistrictsSetStatus.Draft;
					},
					selectedItems => true,
					selectedItems => {
						var selectedNodes = selectedItems.Cast<DistrictsSetJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null) {
							var districtsSet = UoW.GetById<DistrictsSet>(selectedNode.Id);
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
			
			PopupActionsList.Add(
				new JournalAction(
					"В черновик",
					selectedItems => {
						var selectedNodes = selectedItems.Cast<DistrictsSetJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						return selectedNode?.Status == DistrictsSetStatus.Closed;
					},
					selectedItems => true,
					selectedItems => {
						var selectedNodes = selectedItems.Cast<DistrictsSetJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null) {
							var districtsSet = UoW.GetById<DistrictsSet>(selectedNode.Id);
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
		
	}
}
