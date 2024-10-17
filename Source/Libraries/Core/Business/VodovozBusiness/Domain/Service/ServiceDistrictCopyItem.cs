using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace VodovozBusiness.Domain.Service
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "записи о наличии копий сервисных районов",
		Nominative = "запись о наличии копии сервисного района")]
	public class ServiceDistrictCopyItem : PropertyChangedBase, IDomainObject
	{
		private ServiceDistrict _serviceDistrict;
		private ServiceDistrict _copiedToServiceDistrict;
		public virtual int Id { get; set; }

		[Display(Name = "Район, который был скопирован")]
		public virtual ServiceDistrict ServiceDistrict
		{
			get => _serviceDistrict;
			set => SetField(ref _serviceDistrict, value);
		}

		[Display(Name = "Копия района")]
		public virtual ServiceDistrict CopiedToServiceDistrict
		{
			get => _copiedToServiceDistrict;
			set => SetField(ref _copiedToServiceDistrict, value);
		}

		public virtual string Title => $"Район {ServiceDistrict?.Title}({ServiceDistrict?.Id}) скопирован в {CopiedToServiceDistrict?.Title}({CopiedToServiceDistrict?.Id})";
	}
}
