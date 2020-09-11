using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure.Services;

namespace Vodovoz.ViewModels.Complaints
{
	public class CreateComplaintViewModel : EntityTabViewModelBase<Complaint>
	{
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory { get; }
		public IEmployeeService EmployeeService { get; }
		readonly IEntityAutocompleteSelectorFactory employeeSelectorFactory;

		public CreateComplaintViewModel(
			IEntityUoWBuilder uoWBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			IEntityAutocompleteSelectorFactory employeeSelectorFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory
			) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			this.employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			EmployeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			CounterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			NomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			Entity.ComplaintType = ComplaintType.Client;
			Entity.SetStatus(ComplaintStatuses.Checking);
			ConfigureEntityPropertyChanges();
			TabName = "Новая клиентская рекламация";
		}

		private Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null) {
					currentEmployee = EmployeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return currentEmployee;
			}
		}

		//так как диалог только для создания рекламации
		public bool CanEdit => PermissionResult.CanCreate;

		public bool CanSelectDeliveryPoint => Entity.Counterparty != null;

		private List<ComplaintSource> complaintSources;
		private readonly ISubdivisionRepository subdivisionRepository;

		public IEnumerable<ComplaintSource> ComplaintSources {
			get {
				if(complaintSources == null) {
					complaintSources = UoW.GetAll<ComplaintSource>().ToList();
				}
				return complaintSources;
			}
		}

		List<ComplaintKind> complaintKindSource;
		public IEnumerable<ComplaintKind> ComplaintKindSource {
			get {
				if(complaintKindSource == null)
					complaintKindSource = UoW.GetAll<ComplaintKind>().Where(k => !k.IsArchive).ToList();
				if(Entity.ComplaintKind != null && Entity.ComplaintKind.IsArchive)
					complaintKindSource.Add(UoW.GetById<ComplaintKind>(Entity.ComplaintKind.Id));

				return complaintKindSource;
			}
		}

		private GuiltyItemsViewModel guiltyItemsViewModel;
		public GuiltyItemsViewModel GuiltyItemsViewModel {
			get {
				if(guiltyItemsViewModel == null) {
					guiltyItemsViewModel = new GuiltyItemsViewModel(Entity, UoW, CommonServices, subdivisionRepository, employeeSelectorFactory);
				}

				return guiltyItemsViewModel;
			}
		}

		protected override void BeforeValidation()
		{
			if(UoW.IsNew) {
				Entity.CreatedBy = CurrentEmployee;
				Entity.CreationDate = DateTime.Now;
				Entity.PlannedCompletionDate = DateTime.Today;
			}
			Entity.ChangedBy = CurrentEmployee;
			Entity.ChangedDate = DateTime.Now;

			base.BeforeValidation();
		}

		void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(
				e => e.Counterparty,
				() => CanSelectDeliveryPoint
			);
		}
	}
}
