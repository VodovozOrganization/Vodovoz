using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Sale
{
	public class DistrictCopyItem : PropertyChangedBase, IDomainObject
	{
		private District _district;
		private District _copiedToDistrict;
		public virtual int Id { get; set; }

		[Display(Name = "Район, который был скопирован")]
		public virtual District District
		{ 
			get => _district; 
			set => SetField(ref _district, value);
		}

		[Display(Name = "Копия района")]
		public virtual District CopiedToDistrict
		{
			get => _copiedToDistrict;
			set => SetField(ref _copiedToDistrict, value);
		}
	}
}
