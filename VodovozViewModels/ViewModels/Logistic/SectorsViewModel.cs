using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Sectors;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Sectors;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class SectorsViewModel : EntityTabViewModelBase<SectorVersion>
	{
		private readonly ICommonServices _commonServices;
		private readonly IEntityDeleteWorker _entityDeleteWorker;
		private readonly GeometryFactory _geometryFactory;
		private readonly ISectorsRepository _sectorRepository;
		private readonly int _personellId;
		private List<Sector> _sectors;
		private readonly List<SectorVersion> _sectorVersions;
		private readonly List<SectorDeliveryRuleVersion> _sectorDeliveryRuleVersions;
		private readonly List<SectorWeekDayRulesVersion> _sectorWeekDeliveryRuleVersions;
		private List<SectorVersion> _sectorVersionsInSession;
		private List<SectorDeliveryRuleVersion> _sectorDeliveryRuleVersionsInSession;
		private List<SectorWeekDayRulesVersion> _sectorWeekDeliveryRuleVersionsInSession;
		private List<DeliveryPointSectorVersion> _deliveryPointSectorVersions;

		public SectorsViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			IEntityDeleteWorker entityDeleteWorker,
			IEmployeeRepository employeeRepository,
			ISectorsRepository sectorRepository,
			INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_entityDeleteWorker = entityDeleteWorker ?? throw new ArgumentNullException(nameof(entityDeleteWorker));
			_commonServices = commonServices;
			_personellId = employeeRepository.GetEmployeeForCurrentUser(UoW).Id;
			TabName = "Районы с графиками доставки";
			
			if(Entity.Id == 0) {
				Entity.Author = employeeRepository.GetEmployeeForCurrentUser(UoW);
				Entity.Status = SectorsSetStatus.Draft;
				Entity.LastEditor = employeeRepository.GetEmployeeForCurrentUser(UoW);
			}
			
			var permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Sector));
			
			var permissionRes = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(SectorVersion));
			
			_geometryFactory = new GeometryFactory(new PrecisionModel(), 3857);
		}

		#region Свойства

		public List<Sector> Sectors
		{
			get => _sectors;
			set => _sectors = value;
		}

		public List<SectorVersion> SectorVersions => _sectorVersions;
		

		public List<SectorVersion> SectorVersionsInSession
		{
			get => _sectorVersionsInSession;
			set => _sectorVersionsInSession = value;
		}

		public List<SectorDeliveryRuleVersion> SectorDeliveryRuleVersions => _sectorDeliveryRuleVersions;

		public List<SectorDeliveryRuleVersion> SectorDeliveryRuleVersionsInSession
		{
			get => _sectorDeliveryRuleVersionsInSession;
			set => _sectorDeliveryRuleVersionsInSession = value;
		}

		public List<SectorWeekDayRulesVersion> SectorWeekDeliveryRuleVersions => _sectorWeekDeliveryRuleVersions;

		public List<SectorWeekDayRulesVersion> SectorWeekDeliveryRuleVersionsInSession
		{
			get => _sectorWeekDeliveryRuleVersionsInSession;
			set => _sectorWeekDeliveryRuleVersionsInSession = value;
		}

		public List<DeliveryPointSectorVersion> DeliveryPointSectorVersions => _deliveryPointSectorVersions;

		#endregion

		#region Операции над секторами

		public void AddSectors(DateTime createTime)
		{
			var sector = new Sector{DateCreated = createTime};
			Sectors.Add(sector);
		}
		
		public void RemoveSectors(Sector sector) => Sectors.Remove(sector);

		public bool CheckAddSectorInSession(Sector sector) => Sectors.Contains(sector);

		#endregion

		#region Операции над версиями секторов(основные характеристики)

		public void AddSectorVersion(SectorVersion sectorVersion)
		{
			if(sectorVersion.Status == SectorsSetStatus.Active)
			{
				var version = SectorVersions.SingleOrDefault(x => x.Id == sectorVersion.Id);
				var newVersion = version?.Clone() as SectorVersion;

				SectorVersionsInSession.Add(newVersion);
			}
		}

		public bool CheckSectorVersion(SectorVersion sectorVersion) => SectorVersions.SingleOrDefault(x => x.Id == sectorVersion.Id)?.Status == SectorsSetStatus.Active;

		public void RemoveSectorVersion(SectorVersion sectorVersion) => SectorVersionsInSession.Remove(sectorVersion);

		#endregion

		#region Операции над дополнительными характеристиками версии сектора

		public void AddRulesDelivery()
		{
			var deliveryRuleVersion = new SectorDeliveryRuleVersion();
			SectorDeliveryRuleVersionsInSession.Add(deliveryRuleVersion);
		}

		public void CopyRulesDelivery(SectorDeliveryRuleVersion deliveryRuleVersion)
		{
			var deliveryRuleVersionClone = deliveryRuleVersion.Clone() as SectorDeliveryRuleVersion;
			SectorDeliveryRuleVersionsInSession.Add(deliveryRuleVersionClone);
		}

		public void RemoveRulesDelivery(SectorDeliveryRuleVersion deliveryRuleVersion)
		{
			SectorDeliveryRuleVersionsInSession.Remove(deliveryRuleVersion);
		}

		public void AddWeekRuleDelivery()
		{
			var weekRuleDelivery = new SectorWeekDayRulesVersion();
			SectorWeekDeliveryRuleVersionsInSession.Add(weekRuleDelivery);
		}

		public void CopyWeekRuleDelivery(SectorWeekDayRulesVersion weekDayRulesVersion)
		{
			var weekRuleDelivery = weekDayRulesVersion.Clone() as SectorWeekDayRulesVersion;
			SectorWeekDeliveryRuleVersionsInSession.Add(weekRuleDelivery);
		}

		public void RemoveWeekRuleDelivery(SectorWeekDayRulesVersion weekDayRulesVersion) =>
			SectorWeekDeliveryRuleVersionsInSession.Remove(weekDayRulesVersion);
		
		#endregion

		#region Сводка

		

		#endregion

		#region На активацию

		public void OnActivation(SectorVersion sectorVersion)
		{
			sectorVersion.Status = SectorsSetStatus.OnActivation;
			var draftSectors = SectorVersions.Concat(SectorVersionsInSession).Where(x => x.Status == SectorsSetStatus.OnActivation
			                                                                             && x.LastEditor.Id == _personellId && x.Sector.Id == sectorVersion.Sector.Id).ToList();
			for(int i = 0; i < draftSectors.Count; i++)
			{
				draftSectors[i].Status = SectorsSetStatus.Draft;
			}
			
			SaveUow();
		}

		#endregion

		#region Вернуть в черновик

		public void DraftSectors(SectorVersion sectorVersion)
		{
			sectorVersion.Status = SectorsSetStatus.Draft;
			
			SaveUow();
		}

		#endregion

		#region Активировать

		public void ActivateSectors(SectorVersion sectorVersion)
		{
			var activeSectorVersion = SectorVersions.Concat(SectorVersionsInSession)
				.SingleOrDefault(x => x.Status == SectorsSetStatus.Active && x.Sector.Id == sectorVersion.Sector.Id);
			if(activeSectorVersion != null)
			{
				activeSectorVersion.Status = SectorsSetStatus.Closed;
				if(!sectorVersion.Polygon.EqualsExact(activeSectorVersion.Polygon))
				{
					DeliveryPointSectorVersions.ForEach(x => { x.FindAndAssociateDistrict(UoW, _sectorRepository); });
				}
			}
			sectorVersion.Status = SectorsSetStatus.Active;
			
			SaveUow();
		}

		#endregion
		#region Сохранение

		private void SaveUow()
		{
			UoW.Save();
			SectorVersionsInSession.Clear();
			SectorWeekDeliveryRuleVersionsInSession.Clear();
			SectorDeliveryRuleVersionsInSession.Clear();
		}

		#endregion
	}
}