using TrueMark.Contracts;

namespace TrueMark.Api.Extensions
{
	/// <summary>
	/// Расширение для конвертации строки в Enum
	/// </summary>
	public static class ProductInstanceStatusEnumExtensions
	{
		/// <summary>
		/// Конвертация строки в ProductInstanceStatusEnum
		/// </summary>
		/// <param name="statusString"></param>
		/// <returns></returns>
		public static ProductInstanceStatusEnum? ToProductInstanceStatusEnum(this string statusString)
		{
			switch(statusString)
			{
				case "EMITTED": return ProductInstanceStatusEnum.Emitted;
				case "APPLIED": return ProductInstanceStatusEnum.Applied;
				case "APPLIED_PAID": return ProductInstanceStatusEnum.AppliedPaid;
				case "INTRODUCED": return ProductInstanceStatusEnum.Introduced;
				case "WRITTEN_OFF": return ProductInstanceStatusEnum.WrittenOff;
				case "RETIRED": return ProductInstanceStatusEnum.Retired;
				case "WITHDRAWN": return ProductInstanceStatusEnum.Withdrawn;
				case "DISAGGREGATION": return ProductInstanceStatusEnum.Disaggregation;
				case "DISAGGREGATED": return ProductInstanceStatusEnum.Disaggregated;
				case "APPLIED_NOT_PAID": return ProductInstanceStatusEnum.AppliedNotPaid;
				default: return null;
			}
		}
	}
}
