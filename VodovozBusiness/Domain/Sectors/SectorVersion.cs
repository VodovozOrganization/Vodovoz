using System;
using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.Domain.Sectors
{
	[HistoryTrace]
	public class SectorVersion : PropertyChangedBase, IDomainObject, ICloneable
	{
		public int Id { get; set; }
		
		private Employee _author;
		[Display(Name = "Автор")]
		public virtual Employee Author {
			get => _author;
			set => SetField(ref _author, value);
		}

		private Employee _lastEditor;
		public virtual Employee LastEditor {
			get => _lastEditor;
			set => SetField(ref _lastEditor, value);
		}

		private DateTime _startDate;
		[Display(Name = "Время активации")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		private DateTime? _endDate;
		[Display(Name = "Время закрытия")]
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		private Sector _sector;
		public virtual Sector Sector
		{
			get => _sector;
			set => SetField(ref _sector, value);
		}
		
		private string _sectorName;
		[Display(Name = "Название района")]
		public virtual string SectorName {
			get => _sectorName;
			set => SetField(ref _sectorName, value);
		}
		
		private TariffZone tariffZone;
		[Display(Name = "Тарифная зона")]
		public virtual TariffZone TariffZone {
			get => tariffZone;
			set => SetField(ref tariffZone, value);
		}

		private Geometry _polygon;
		[Display(Name = "Граница")]
		public virtual Geometry Polygon {
			get => _polygon;
			set => SetField(ref _polygon, value);
		}
		
		private WageSector _wageSector;
		[Display(Name = "Группа района для расчёта ЗП")]
		public virtual WageSector WageSector {
			get => _wageSector;
			set => SetField(ref _wageSector, value);
		}
		
		private GeographicGroup _geographicGroup;
		[Display(Name = "Часть города")]
		public virtual GeographicGroup GeographicGroup {
			get => _geographicGroup;
			set => SetField(ref _geographicGroup, value);
		}

		private SectorsSetStatus _status;
		public virtual SectorsSetStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}
		
		private int _minBottles;
		[Display(Name = "Минимальное количество бутылей")]
		public virtual int MinBottles {
			get => _minBottles;
			set => SetField(ref _minBottles, value);
		}

		private decimal _waterPrice;
		[Display(Name = "Цена на воду")]
		public virtual decimal WaterPrice {
			get => _waterPrice;
			set => SetField(ref _waterPrice, value);
		}

		private SectorWaterPrice _priceType;
		[Display(Name = "Вид цены")]
		public virtual SectorWaterPrice PriceType {
			get => _priceType;
			set {
				SetField(ref _priceType, value);
				if(WaterPrice != 0 && PriceType != SectorWaterPrice.FixForDistrict)
					WaterPrice = 0;
			}
		}
		
		private bool _isArchive;
		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
			get => _isArchive;
			set => SetField(ref _isArchive, value);
			
		}

		public object Clone()
		{
			var wageSector = new WageSector{Name = WageSector.Name, IsArchive = WageSector.IsArchive};
			
			var geographicGroup = new GeographicGroup {Name = GeographicGroup.Name};
			geographicGroup.SetСoordinates(GeographicGroup.BaseLatitude, GeographicGroup.BaseLongitude);

			var polygonString = JsonConvert.SerializeObject(Polygon);
			var copyPolygon = JsonConvert.DeserializeObject<Geometry>(polygonString);

			var tariff = new TariffZone {Name = TariffZone.Name};

			var sector = Sector.Clone() as Sector;
			return new SectorVersion
			{
				Status = SectorsSetStatus.Draft,
				SectorName = SectorName,
				StartDate = StartDate,
				EndDate = EndDate,
				WageSector = wageSector,
				GeographicGroup = geographicGroup,
				Polygon = copyPolygon,
				TariffZone = tariff,
				Sector = sector
			};
		}
	}
}