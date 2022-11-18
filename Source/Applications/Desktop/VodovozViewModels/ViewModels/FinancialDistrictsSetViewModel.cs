using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using GMap.NET;
using NetTopologySuite.Geometries;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Services;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels
{
	public class FinancialDistrictsSetViewModel : EntityTabViewModelBase<FinancialDistrictsSet>
	{
		private readonly IEntityDeleteWorker entityDeleteWorker;
		private readonly GeometryFactory geometryFactory;
		
		public bool CanEdit { get; }
		public bool CanEditDistrict { get; }
		public bool CanDeleteDistrict { get; }
		public bool CanCreateDistrict { get; }
		
		private FinancialDistrict selectedDistrict;
		public FinancialDistrict SelectedDistrict {
			get => selectedDistrict;
			set => SetField(ref selectedDistrict, value);
		}
		
		private bool isCreatingNewBorder;
		public bool IsCreatingNewBorder {
			get => isCreatingNewBorder;
			private set {
				if(value && SelectedDistrict == null)
					throw new ArgumentNullException(nameof(SelectedDistrict));
				SetField(ref isCreatingNewBorder, value);
			}
		}
		
		private GenericObservableList<PointLatLng> selectedDistrictBorderVertices = new GenericObservableList<PointLatLng>();
		public GenericObservableList<PointLatLng> SelectedDistrictBorderVertices {
			get => selectedDistrictBorderVertices;
			set => SetField(ref selectedDistrictBorderVertices, value);
		}
        
		private GenericObservableList<PointLatLng> newBorderVertices = new GenericObservableList<PointLatLng>();
		public GenericObservableList<PointLatLng> NewBorderVertices {
			get => newBorderVertices;
			set => SetField(ref newBorderVertices, value);
		}
		
		public FinancialDistrictsSetViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEntityDeleteWorker entityDeleteWorker,
			IEmployeeService employeeService
		) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			this.entityDeleteWorker = entityDeleteWorker ?? throw new ArgumentNullException(nameof(entityDeleteWorker));
			TabName = "Финансовые районы";
			
			if(uowBuilder.IsNewEntity) {
				Entity.Author = employeeService.GetEmployeeForUser(UoW, CurrentUser.Id);
				Entity.Status = DistrictsSetStatus.Draft;
			}
			
			var permissionResult = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(FinancialDistrict));
			CanEditDistrict = permissionResult.CanUpdate && Entity.Status != DistrictsSetStatus.Active;
			CanDeleteDistrict = permissionResult.CanDelete && Entity.Status != DistrictsSetStatus.Active;
			CanCreateDistrict = permissionResult.CanCreate && Entity.Status != DistrictsSetStatus.Active;
			
			var permissionRes = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(FinancialDistrictsSet));
			CanEdit = permissionRes.CanUpdate && Entity.Status != DistrictsSetStatus.Active;
			
			geometryFactory = new GeometryFactory(new PrecisionModel(), 3857);
		}

		#region Commands

		private DelegateCommand addDistrictCommand;
        public DelegateCommand AddDistrictCommand => addDistrictCommand ?? (addDistrictCommand = new DelegateCommand(
            () => {
                var newDistrict = new FinancialDistrict { Name = "Новый район", FinancialDistrictsSet = Entity };
                Entity.ObservableFinancialDistricts.Add(newDistrict);
                SelectedDistrict = newDistrict;
            }, 
            () => true
        ));

        private DelegateCommand removeDistrictCommand;
        public DelegateCommand RemoveDistrictCommand => removeDistrictCommand ?? (removeDistrictCommand = new DelegateCommand(
            () => {
                var distrToDel = selectedDistrict;
                Entity.ObservableFinancialDistricts.Remove(SelectedDistrict);

                if (distrToDel.Id == 0) return;
                
                if(entityDeleteWorker.DeleteObject<FinancialDistrict>(distrToDel.Id, UoW)) {
                    SelectedDistrict = null;
                }
                else {
                    Entity.ObservableFinancialDistricts.Add(distrToDel);
                    SelectedDistrict = distrToDel;
                }
            },
            () => SelectedDistrict != null
        ));
        
        private DelegateCommand createBorderCommand;
        public DelegateCommand CreateBorderCommand => createBorderCommand ?? (createBorderCommand = new DelegateCommand(
            () => {
                IsCreatingNewBorder = true;
                NewBorderVertices.Clear();
            },
            () => !IsCreatingNewBorder
        ));
        
        private DelegateCommand confirmNewBorderCommand;
        public DelegateCommand ConfirmNewBorderCommand => confirmNewBorderCommand ?? (confirmNewBorderCommand = new DelegateCommand(
            () => {
                
	            if(NewBorderVertices.Count < 3) return;
	            
                var closingPoint = NewBorderVertices[0];
                NewBorderVertices.Add(closingPoint);
                SelectedDistrictBorderVertices = new GenericObservableList<PointLatLng>(NewBorderVertices.ToList());
                NewBorderVertices.Clear();
                SelectedDistrict.Border = geometryFactory.CreatePolygon(SelectedDistrictBorderVertices.Select(p => new Coordinate(p.Lat,p.Lng)).ToArray());
                IsCreatingNewBorder = false;
            },
            () => IsCreatingNewBorder
        ));
        
        private DelegateCommand cancelNewBorderCommand;
        public DelegateCommand CancelNewBorderCommand => cancelNewBorderCommand ?? (cancelNewBorderCommand = new DelegateCommand(
            () => {
                NewBorderVertices.Clear();
                IsCreatingNewBorder = false;
            },
            () => IsCreatingNewBorder
        ));
        
        private DelegateCommand removeBorderCommand;
        public DelegateCommand RemoveBorderCommand => removeBorderCommand ?? (removeBorderCommand = new DelegateCommand(
            () => {
                SelectedDistrict.Border = null;
                SelectedDistrictBorderVertices.Clear();
                OnPropertyChanged(nameof(SelectedDistrict));
            },
            () => !IsCreatingNewBorder
        ));
        
        private DelegateCommand<PointLatLng> addNewVertexCommand;
        public DelegateCommand<PointLatLng> AddNewVertexCommand => addNewVertexCommand ?? (addNewVertexCommand = new DelegateCommand<PointLatLng>(
            point => {
                NewBorderVertices.Add(point);
            },
            point => IsCreatingNewBorder
        ));
        
        private DelegateCommand<PointLatLng> removeNewBorderVertexCommand;
        public DelegateCommand<PointLatLng> RemoveNewBorderVertexCommand => removeNewBorderVertexCommand ?? (removeNewBorderVertexCommand = new DelegateCommand<PointLatLng>(
            point => {
                NewBorderVertices.Remove(point);
            },
            point => IsCreatingNewBorder && !point.IsEmpty
        ));

		#endregion

		public void FillSelectedDistrictBorderVertices()
		{
			SelectedDistrictBorderVertices.Clear();
			
			if(SelectedDistrict?.Border?.Coordinates != null) {
				foreach(Coordinate coord in SelectedDistrict.Border.Coordinates) {
					SelectedDistrictBorderVertices.Add(new PointLatLng {
						Lat = coord.X,
						Lng = coord.Y
					});
				}
			}
		}
		
		public override bool Save(bool close)
		{
			if (Entity.Id == 0) {
				Entity.DateCreated = DateTime.Now;
			}

			if(base.Save(close)) {
				if(!CommonServices.InteractiveService.Question("Продолжить редактирование районов?", "Успешно сохранено"))
					Close(false, CloseSource.Save);
				return true;
			}
			
			return false;
		}

		public override void Close(bool askSave, CloseSource source)
		{
			if (askSave) {
				TabParent?.AskToCloseTab(this, source);
			}
			else {
				TabParent?.ForceCloseTab(this, source);
			}
		}

		public override bool HasChanges {
			get => base.HasChanges && (CanEditDistrict || CanEdit);
			set => base.HasChanges = value;
		}
	}
}
