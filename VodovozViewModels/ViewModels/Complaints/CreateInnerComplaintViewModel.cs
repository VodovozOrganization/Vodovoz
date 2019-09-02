using System;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure.Services;

namespace Vodovoz.ViewModels.Complaints
{
	public class CreateInnerComplaintViewModel : EntityTabViewModelBase<Complaint>
	{
		private readonly IEmployeeService employeeService;
		private readonly ISubdivisionRepository subdivisionRepository;

		public CreateInnerComplaintViewModel(
			IEntityConstructorParam ctorParam,
			IEmployeeService employeeService,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices
			) : base(ctorParam, commonServices)
		{
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			Entity.ComplaintType = ComplaintType.Inner;
			Entity.SetStatus(ComplaintStatuses.Checking);
			TabName = "Новая внутреняя жалоба";
		}

		//так как диалог только для создания жалобы
		public bool CanEdit => PermissionResult.CanCreate;

		private Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null) {
					currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return currentEmployee;
			}
		}

		private GuiltyItemsViewModel guiltyItemsViewModel;
		public GuiltyItemsViewModel GuiltyItemsViewModel {
			get {
				if(guiltyItemsViewModel == null) {
					guiltyItemsViewModel = new GuiltyItemsViewModel(Entity, UoW, CommonServices, subdivisionRepository);
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
	}
}
