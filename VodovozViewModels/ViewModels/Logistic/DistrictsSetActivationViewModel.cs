using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate.Persister.Entity;
using NHibernate.Transform;
using NLog;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.Controllers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.ViewModels.Logistic
{
	public sealed class DistrictsSetActivationViewModel : EntityTabViewModelBase<DistrictsSet>, ITDICloseControlTab
	{
		public DistrictsSetActivationViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeRepository employeeRepository,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			TabName = $"Активация версии районов \"{Entity.Name}\"";
			
			ActivationInProgress = false;
			WasActivated = false;
			NotCopiedPriorities = new List<DriverDistrictPriority>();
			ActiveDistrictsSet = UoW.Session.QueryOver<DistrictsSet>()
				.Where(x => x.Status == DistrictsSetStatus.Active)
				.Take(1)
				.SingleOrDefault();
			this.employeeRepository = employeeRepository;
		}

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IEmployeeRepository employeeRepository;
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

		private List<DriverDistrictPriority> notCopiedPriorities;
		public List<DriverDistrictPriority> NotCopiedPriorities {
			get => notCopiedPriorities;
			set => SetField(ref notCopiedPriorities, value);
		}

		private bool wasActivated;
		public bool WasActivated {
			get => wasActivated;
			private set => SetField(ref wasActivated, value, () => WasActivated);
		}

		#region Commands

		private DelegateCommand activateDistrictsSetCommand;
		public DelegateCommand ActivateDistrictsSetCommand => activateDistrictsSetCommand ??
			(activateDistrictsSetCommand = new DelegateCommand(
				async () => {
					try {
						ActivationInProgress = true;

						var task = Task.Run(() => {
							if(ActiveDistrictsSet != null) {
								ActiveDistrictsSet.Status = DistrictsSetStatus.Draft;
								UoW.Save(ActiveDistrictsSet);
							}
							Entity.Status = DistrictsSetStatus.Active;
							UoW.Save(Entity);
							UoW.Commit();

							ReAssignDeliveryPoints();
							TryToFindNearbyDistrict();
							ReAssignDriverDistrictPriorities();
						});
						await task;

						ActiveDistrictsSet.DateActivated = null;
						Entity.DateActivated = DateTime.Now;
						UoW.Save(ActiveDistrictsSet);
						UoW.Save(Entity);
						UoW.Commit();
						
						WasActivated = true;
						ActivationStatus = "Активация завершена";
						OnPropertyChanged(nameof(NotCopiedPriorities));
						ActiveDistrictsSet = Entity;
					}
					catch(Exception ex) {
						logger.Error(ex, "Ошибка при активации версии районов");
						ActivationStatus = "Ошибка при активации версии районов. Попробуйте активировать версию ещё раз";
						
						if(ActiveDistrictsSet != null) {
							ActiveDistrictsSet.Status = DistrictsSetStatus.Active;
							UoW.Save(ActiveDistrictsSet);
						}
						Entity.Status = DistrictsSetStatus.Draft;
						UoW.Save(Entity);
						UoW.Commit();
					}
					finally {
						ActivationInProgress = false;
					}
				},
				() => !ActivationInProgress
			));

		#endregion

		public bool CanClose()
		{
			if(ActivationInProgress) {
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Дождитесь окончания активации версии районов", "Активация версии районов");
			}
			return !ActivationInProgress;
		}

		private void ReAssignDeliveryPoints()
		{
			ActivationStatus = "Переприсвоение районов точкам доставки...";
			
			var factory = UoW.Session.SessionFactory;
			var dpPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(DeliveryPoint));
			var districtPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(District));
			
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

		private void TryToFindNearbyDistrict()
		{
			ActivationStatus = "Поиск районов для точек доставки без районов...";
			var deliveryPoints = UoW.Session.QueryOver<DeliveryPoint>()
				.Where(d => d.District == null)
				.And(x => x.Latitude != null)
				.And(x => x.Longitude != null)
				.List();
			int batchCounter = 0;
			UoW.Session.SetBatchSize(500);
			
			foreach(var dp in deliveryPoints) {
				if(dp.FindAndAssociateDistrict(UoW)) {
					UoW.Save(dp);
					batchCounter++;
				}
				if(batchCounter == 500) {
					UoW.Commit();
					batchCounter = 0;
				}
			}
			UoW.Commit();
		}

		private void ReAssignDriverDistrictPriorities()
		{
			ActivationStatus = "Переприсвоение приоритетов доставки водителей...";
			NotCopiedPriorities.Clear();

			DriverDistrictPrioritySet districtPrioritySetAlias = null;

			var employeeForCurrentUser = employeeRepository.GetEmployeeForCurrentUser(UoW);

			var drivers = UoW.Session.QueryOver<Employee>()
				.Inner.JoinAlias(x => x.DriverDistrictPrioritySets, () => districtPrioritySetAlias)
				.TransformUsing(Transformers.DistinctRootEntity)
				.List<Employee>();

			foreach(var driver in drivers) {
				var currentActivePrioritySet = driver.DriverDistrictPrioritySets.SingleOrDefault(x => x.IsActive);
				if(currentActivePrioritySet == null || !currentActivePrioritySet.DriverDistrictPriorities.Any()) {
					continue;
				}
				var newSet = DriverDistrictPriorityHelper.CopyPrioritySetWithActiveDistricts(
					currentActivePrioritySet,
					out var notCopied
				);

				newSet.LastEditor = employeeForCurrentUser;
				newSet.Author = employeeForCurrentUser;
				newSet.IsCreatedAutomatically = true;
				NotCopiedPriorities.AddRange(notCopied);

				if(newSet.DriverDistrictPriorities.Any()) {
					driver.AddDriverDistrictPrioritySet(newSet);
					driver.ActivateDriverDistrictPrioritySet(newSet, employeeForCurrentUser);
				}
			}
		}
	}
}
