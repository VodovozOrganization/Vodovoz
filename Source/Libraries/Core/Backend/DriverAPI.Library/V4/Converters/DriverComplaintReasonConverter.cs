using DriverApi.Contracts.V4;
using Vodovoz.Domain.Complaints;

namespace DriverAPI.Library.V4.Converters
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
