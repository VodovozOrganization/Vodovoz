using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate.Persister.Entity;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.Logistic
{
    public sealed class DistrictsSetActivationViewModel : EntityTabViewModelBase<DistrictsSet>, ITDICloseControlTab
    {
        public DistrictsSetActivationViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
            INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
        {
            TabName = $"Активация набора районов \"{Entity.Name}\"";
            
            ActivationInProgress = false;
            WasSuccesfullyActivated = false;
            DeletedPriorities = new List<DriverDistrictPriority>();
            ActiveDistrictsSet = UoW.Session.QueryOver<DistrictsSet>()
                .Where(x => x.Status == DistrictsSetStatus.Active)
                .Take(1)
                .SingleOrDefault();
            
        }

        private DistrictsSet activeDistrictsSet;
        public DistrictsSet ActiveDistrictsSet {
            get => activeDistrictsSet;
            set => SetField(ref activeDistrictsSet, value, () => ActiveDistrictsSet);
        }
        
        private bool activationInProgress;
        public bool ActivationInProgress {
            get => activationInProgress;
            set => SetField(ref activationInProgress, value, () => ActivationInProgress);
        }
        
        private string activationStatus;
        public string ActivationStatus {
            get => activationStatus;
            set => SetField(ref activationStatus, value, () => ActivationStatus);
        }

        private List<DriverDistrictPriority> deletedPriorities;
        public List<DriverDistrictPriority> DeletedPriorities {
            get => deletedPriorities;
            set => SetField(ref deletedPriorities, value, () => DeletedPriorities);
        }
        
        public bool WasSuccesfullyActivated;

        #region Commands

        private DelegateCommand activateDistrictsSetCommand;
        public DelegateCommand ActivateDistrictsSetCommand => activateDistrictsSetCommand ?? (activateDistrictsSetCommand = new DelegateCommand(
            () => {
                Task.Run(() => {
                    try {
                        ActivationInProgress = true;
                        
                        ReAssignDriverDistirctPriorities();
                        ReAssignDeliveryPoints();
                        
                        Entity.Status = DistrictsSetStatus.Active;
                        Entity.DateActivated = DateTime.Now;
                        UoW.Save(Entity);
                        if(ActiveDistrictsSet != null) {
                            ActiveDistrictsSet.Status = DistrictsSetStatus.Draft;
                            UoW.Save(ActiveDistrictsSet);
                        }
                        UoW.Commit();
                        ActivationStatus = "Активация завершена";
                        ActiveDistrictsSet = Entity;
                        WasSuccesfullyActivated = true;
                    }
                    catch (Exception ex) {
                        ActivationStatus = "Ошибка при активации набора района";
                    }
                    finally {
                        ActivationInProgress = false;
                    }
                });
            }, () => true
        ));

        #endregion
        
        public bool CanClose()
        {
            return !ActivationInProgress;
        }
        
        private void ReAssignDeliveryPoints()
        {
            ActivationStatus = "Переприсвоение районов точкам доставки...";
            
            var factory = UoW.Session.SessionFactory;
            var dpPersister = factory.GetClassMetadata(typeof(DeliveryPoint)) as AbstractEntityPersister;
            var districtPersister = factory.GetClassMetadata(typeof(District)) as AbstractEntityPersister;
            
            var districtColumn = dpPersister.GetPropertyColumnNames(nameof(DeliveryPoint.District)).First();
            var latColumn = dpPersister.GetPropertyColumnNames(nameof(DeliveryPoint.Latitude)).First();
            var longColumn = dpPersister.GetPropertyColumnNames(nameof(DeliveryPoint.Longitude)).First();
            var borderColumn = districtPersister.GetPropertyColumnNames(nameof(District.DistrictBorder)).First();
            var districtsSetColumn = districtPersister.GetPropertyColumnNames(nameof(District.DistrictsSet)).First();

            var query = $"UPDATE {dpPersister.TableName} dp SET {districtColumn} = "
                + $"(SELECT districts.{districtPersister.KeyColumnNames.First()} FROM {districtPersister.TableName} AS districts"
                + $" WHERE districts.{districtsSetColumn} = {Entity.Id} AND ST_WITHIN(PointFromText(CONCAT('POINT(', dp.{latColumn} ,' ', dp.{longColumn} ,')')), districts.{borderColumn}) LIMIT 1);";
            UoW.Session.CreateSQLQuery(query).ExecuteUpdate();
        }

        private void ReAssignDriverDistirctPriorities()
        {
            ActivationStatus = "Переприсвоение приоритетов доставки водителей";
            
            foreach (var priority in UoW.Session.QueryOver<DriverDistrictPriority>().Future()) {
                if(priority.District.DistrictsSet.Id == Entity.Id)
                    continue;
                
                var newDistrict = Entity.Districts.FirstOrDefault(x => x.DistrictName == priority.District.DistrictName);
                if(newDistrict == null) {
                    DeletedPriorities.Add(priority);
                    UoW.Delete(priority);
                }
                else {
                    priority.District = newDistrict;
                    UoW.Save(priority);
                }
            }
            OnPropertyChanged(nameof(DeletedPriorities));
        }
    }
}