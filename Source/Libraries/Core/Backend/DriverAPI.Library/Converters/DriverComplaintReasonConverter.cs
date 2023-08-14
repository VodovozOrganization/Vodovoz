using DriverAPI.Library.DTOs;
using Vodovoz.Domain.Complaints;

namespace DriverAPI.Library.Converters
{
	/// <summary>
	/// Конвертер причин рекламаций водителей
	/// </summary>
	public class DriverComplaintReasonConverter
	{
		/// <summary>
		/// Метод конвертации в DTO
		/// </summary>
		/// <param name="driverComplaint">Причина екламации водителя ДВ</param>
		/// <returns></returns>
		public DriverComplaintReasonDto ConvertToAPIDriverComplaintReason(DriverComplaintReason driverComplaint)
		{
			return new DriverComplaintReasonDto()
			{
				Id = driverComplaint.Id,
				Name = driverComplaint.Name
			};
		}
	}
}
