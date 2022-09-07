using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Complaints
{
	public class CreateInnerComplaintViewModel : EntityTabViewModelBase<Complaint>
	{
		private readonly IEmployeeService _employeeService;
		private readonly ISubdivisionRepository _subdivisionRepository;
		readonly IEntityAutocompleteSelectorFactory _employeeSelectorFactory;
        private readonly IFileDialogService _fileDialogService;
        private readonly IUserRepository _userRepository;
        private IList<ComplaintObject> _complaintObjectSource;
        private ComplaintObject _complaintObject;
        private readonly IList<ComplaintKind> _complaintKinds;

		public CreateInnerComplaintViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			IEntityAutocompleteSelectorFactory employeeSelectorFactory,
            IFileDialogService fileDialogService,
			IUserRepository userRepository
            ) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
            _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			Entity.ComplaintType = ComplaintType.Inner;
			Entity.SetStatus(ComplaintStatuses.Checking);

			_complaintKinds = complaintKindSource = UoW.GetAll<ComplaintKind>().Where(k => !k.IsArchive).ToList();

			TabName = "Новая внутреняя рекламация";
		}

		//так как диалог только для создания рекламации
		public bool CanEdit => PermissionResult.CanCreate;

		private Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null) {
					currentEmployee = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return currentEmployee;
			}
		}

		IList<ComplaintKind> complaintKindSource;
		public IList<ComplaintKind> ComplaintKindSource {
			get => complaintKindSource;
			set => SetField(ref complaintKindSource, value);
		}

		public virtual ComplaintObject ComplaintObject
		{
			get => _complaintObject;
			set
			{
				if(SetField(ref _complaintObject, value))
				{
					ComplaintKindSource = value == null ? _complaintKinds : _complaintKinds.Where(x => x.ComplaintObject == value).ToList();
				}
			}
		}

		public IEnumerable<ComplaintObject> ComplaintObjectSource =>
			_complaintObjectSource ?? (_complaintObjectSource = UoW.GetAll<ComplaintObject>().Where(x => !x.IsArchive).ToList());

		private GuiltyItemsViewModel guiltyItemsViewModel;
		public GuiltyItemsViewModel GuiltyItemsViewModel
		{
			get
			{
				if(guiltyItemsViewModel == null)
				{
					guiltyItemsViewModel =
						new GuiltyItemsViewModel(Entity, UoW, CommonServices, _subdivisionRepository, _employeeSelectorFactory);
				}

				return guiltyItemsViewModel;
			}
		}

        private ComplaintFilesViewModel filesViewModel;
        public ComplaintFilesViewModel FilesViewModel
        {
            get
            {
                if (filesViewModel == null)
                {
                    filesViewModel = new ComplaintFilesViewModel(Entity, UoW, _fileDialogService, CommonServices, _userRepository);
                }
                return filesViewModel;
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
