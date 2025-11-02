using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Persister.Entity;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.ViewModels.Logistic
{
	public sealed class DistrictsSetActivationViewModel : EntityTabViewModelBase<DistrictsSet>, ITDICloseControlTab, IAskSaveOnCloseViewModel
	{
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IDeliveryRepository _deliveryRepository;
		private bool _activationInProgress;
		private string _activationStatus;
		private DistrictsSet _activeDistrictsSet;
		private List<DriverDistrictPriority> _notCopiedPriorities;
		private bool _wasActivated;
		private readonly TimeSpan _queriesTimeout = TimeSpan.FromSeconds(240);

		public DistrictsSetActivationViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeRepository employeeRepository,
			IDeliveryRepository deliveryRepository,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			TabName = $"Активация версии районов \"{Entity.Name}\"";

			ActivationInProgress = false;
			WasActivated = false;
			NotCopiedPriorities = new List<DriverDistrictPriority>();
			ActiveDistrictsSet = UoW.Session.QueryOver<DistrictsSet>()
				.SetTimeout((int)_queriesTimeout.TotalSeconds)
				.Where(x => x.Status == DistrictsSetStatus.Active)
				.Take(1)
				.SingleOrDefault();
			_employeeRepository = employeeRepository;
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
		}

		public override bool HasChanges => false;
		public bool AskSaveOnClose => false;

		public DistrictsSet ActiveDistrictsSet
		{
			get => _activeDistrictsSet;
			set => SetField(ref _activeDistrictsSet, value);
		}

		public bool ActivationInProgress
		{
			get => _activationInProgress;
			set => SetField(ref _activationInProgress, value);
		}

		public string ActivationStatus
		{
			get => _activationStatus;
			set => SetField(ref _activationStatus, value);
		}

		public List<DriverDistrictPriority> NotCopiedPriorities
		{
			get => _notCopiedPriorities;
			set => SetField(ref _notCopiedPriorities, value);
		}

		public bool WasActivated
		{
			get => _wasActivated;
			private set => SetField(ref _wasActivated, value);
		}

		public bool CanClose()
		{
			if(ActivationInProgress)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Дождитесь окончания активации версии районов",
					"Активация версии районов");
			}
			return !ActivationInProgress;
		}

		public async Task ActivateAsync()
		{
			try
			{
				ActivationInProgress = true;

				var task = Task.Run(() =>
				{
					var unitOfWorkTitle = TabName;

					using(var localUoW = UnitOfWorkFactory.CreateWithoutRoot(unitOfWorkTitle))
					{
						var currentActiveDistrictSet = localUoW.GetById<DistrictsSet>(ActiveDistrictsSet.Id);
						var districtSetToActivate = localUoW.GetById<DistrictsSet>(Entity.Id);

						localUoW.Session.BeginTransaction();
						if(currentActiveDistrictSet != null)
						{
							currentActiveDistrictSet.Status = DistrictsSetStatus.Draft;
							localUoW.Save(currentActiveDistrictSet);
						}
						districtSetToActivate.Status = DistrictsSetStatus.Active;

						ReAssignDeliveryPoints(localUoW);
						TryToFindNearbyDistrict(localUoW, districtSetToActivate);
						ReAssignDriverDistrictPriorities(localUoW);

						districtSetToActivate.DateActivated = DateTime.Now;
						localUoW.Save(districtSetToActivate);
						localUoW.Commit();
					}
				});

				await task;

				WasActivated = true;
				ActivationStatus = "Активация завершена";
				OnPropertyChanged(nameof(NotCopiedPriorities));
				UoW.Session.Refresh(Entity);
				UoW.Session.Refresh(ActiveDistrictsSet);

				ActiveDistrictsSet = Entity;
			}
			catch
			{
				ActivationStatus = "Ошибка при активации версии районов. Попробуйте активировать версию ещё раз";
				throw;
			}
			finally
			{
				ActivationInProgress = false;
			}
		}

		private void ReAssignDeliveryPoints(IUnitOfWork uow)
		{
			ActivationStatus = "Переприсвоение районов точкам доставки...";

			var factory = uow.Session.SessionFactory;
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

			uow.Session.CreateSQLQuery(query)
				.SetTimeout((int)_queriesTimeout.TotalSeconds)
				.ExecuteUpdate();
		}

		private void TryToFindNearbyDistrict(IUnitOfWork uow, DistrictsSet districtsSet)
		{
			ActivationStatus = "Поиск районов для точек доставки без районов...";
			var deliveryPoints = uow.Session.QueryOver<DeliveryPoint>()
				.SetTimeout((int)_queriesTimeout.TotalSeconds)
				.Where(d => d.District == null)
				.And(x => x.Latitude != null)
				.And(x => x.Longitude != null)
				.List();

			foreach(var dp in deliveryPoints)
			{
				if(dp.FindAndAssociateDistrict(uow, _deliveryRepository, districtsSet))
				{
					uow.Save(dp);
				}
			}
		}

		private void ReAssignDriverDistrictPriorities(IUnitOfWork uow)
		{
			ActivationStatus = "Переприсвоение приоритетов доставки водителей...";
			NotCopiedPriorities.Clear();

			DriverDistrictPrioritySet districtPrioritySetAlias = null;

			var employeeForCurrentUser = _employeeRepository.GetEmployeeForCurrentUser(uow);

			var drivers = uow.Session.QueryOver<Employee>()
				.SetTimeout((int)_queriesTimeout.TotalSeconds)
				.Inner.JoinAlias(x => x.DriverDistrictPrioritySets, () => districtPrioritySetAlias)
				.TransformUsing(Transformers.DistinctRootEntity)
				.List<Employee>();

			foreach(var driver in drivers)
			{
				var currentActivePrioritySet = driver.DriverDistrictPrioritySets.SingleOrDefault(x => x.IsActive);
				if(currentActivePrioritySet == null || !currentActivePrioritySet.DriverDistrictPriorities.Any())
				{
					continue;
				}
				var newSet = DriverDistrictPriorityHelper.CopyPrioritySetWithActiveDistricts(
					uow,
					currentActivePrioritySet,
					out var notCopied
				);

				newSet.LastEditor = employeeForCurrentUser;
				newSet.Author = employeeForCurrentUser;
				newSet.IsCreatedAutomatically = true;
				NotCopiedPriorities.AddRange(notCopied);

				if(newSet.DriverDistrictPriorities.Any())
				{
					driver.AddDriverDistrictPrioritySet(newSet);
					driver.ActivateDriverDistrictPrioritySet(newSet, employeeForCurrentUser);
				}
			}
		}
	}
}
