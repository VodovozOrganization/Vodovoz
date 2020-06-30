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
            TabName = $"Активация версии районов \"{Entity.Name}\"";
            
            ActivationInProgress = false;
            WasActivated = false;
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

        private bool wasActivated;
        public bool WasActivated {
            get => wasActivated;
            private set => SetField(ref wasActivated, value, () => WasActivated);
        }

        #region Commands

        private DelegateCommand activateDistrictsSetCommand;
        public DelegateCommand ActivateDistrictsSetCommand => activateDistrictsSetCommand ?? (activateDistrictsSetCommand = new DelegateCommand(
            () => {
                Task.Run(() => {
                    try {
                        ActivationInProgress = true;
                        
                        ReAssignDeliveryPoints();
                        ReAssignDriverDistirctPriorities();
                        
                        Entity.Status = DistrictsSetStatus.Active;
                        Entity.DateActivated = DateTime.Now;
                        UoW.Save(Entity);
                        if(ActiveDistrictsSet != null) {
                            ActiveDistrictsSet.Status = DistrictsSetStatus.Draft;
                            ActiveDistrictsSet.DateActivated = null;
                            UoW.Save(ActiveDistrictsSet);
                        }
                        WasActivated = true;
                        UoW.Commit();
                        ActivationStatus = "Активация завершена";
                        ActiveDistrictsSet = Entity;
                        OnPropertyChanged(nameof(DeletedPriorities));
                    }
                    catch (Exception ex) {
                        ActivationStatus = "Ошибка при активации версии районов";
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
            UoW.Session.CreateSQLQuery(query).SetTimeout(90).ExecuteUpdate();
        }

        private void ReAssignDriverDistirctPriorities()
        {
            ActivationStatus = "Переприсвоение приоритетов доставки водителей";
            
            var priorities = UoW.Session.QueryOver<DriverDistrictPriority>().List();
            foreach (var priority in priorities.ToList()) {
                if(priority.District.DistrictsSet.Id == Entity.Id)
                    continue;
                
                var newDistrict = Entity.Districts.FirstOrDefault(x => x.CopyOf == priority.District);
                if(newDistrict == null) {
                    DeletedPriorities.Add(priority);
                    UoW.Delete(priority);
                }
                else {
                    priority.District = newDistrict;
                    UoW.Save(priority);
                }
            }
        }
    }
}