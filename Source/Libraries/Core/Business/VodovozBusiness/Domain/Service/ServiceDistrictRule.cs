using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;

namespace VodovozBusiness.Domain.Service
{
	[Appellative(Gender = GrammaticalGender.Neuter,
	NominativePlural = "условия сервисной доставки",
	Nominative = "условие сервисной доставки")]
	[EntityPermission]
	[HistoryTrace]
	public abstract partial class ServiceDistrictRule : PropertyChangedBase, IDomainObject
	{
		private MasterServiceType _serviceType;
		private decimal _price;
		private ServiceDistrict _serviceDistrict;

		public virtual int Id { get; set; }

		public virtual MasterServiceType ServiceType
		{
			get => _serviceType;
			set => SetField(ref _serviceType, value);
		}

		public virtual decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
		}

		public virtual ServiceDistrict ServiceDistrict
		{
			get => _serviceDistrict;
			set => SetField(ref _serviceDistrict, value);
		}

		public abstract object Clone();
	}
}
