using System;

namespace TaxcomEdoApi.Library.Models
{
	public class WarrantMeta
	{
		//TODO найти иил придумать свой тип вместо WarrantType
		/*public WarrantMeta(WarrantType certificateDto)
		{
			this.RegistrationNumber = certificateDto.RegistrationNumber;
			this.Status = certificateDto.Status;
			this.DateTimeLastStatusUpdateSpecified = certificateDto.DateTimeLastStatusUpdateSpecified;
			this.DateTimeLastStatusUpdate = new DateTime?(certificateDto.DateTimeLastStatusUpdate);
			this.DefaultUse = certificateDto.DefaultUse;
		}*/

		public string RegistrationNumber { get; set; }

		public string Status { get; set; }

		public bool DateTimeLastStatusUpdateSpecified { get; set; }

		public DateTime? DateTimeLastStatusUpdate { get; set; }

		public bool DefaultUse { get; set; }
	}
}
