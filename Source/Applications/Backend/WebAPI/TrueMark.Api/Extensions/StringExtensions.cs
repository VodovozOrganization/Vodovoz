using TrueMark.Contracts;

namespace TrueMark.Api.Extensions;

public static class ProductInstanceStatusEnumExtensions
{
	public static ProductInstanceStatusEnum? ToProductInstanceStatusEnum(this string statusString) => statusString switch
	{
		"EMITTED" => (ProductInstanceStatusEnum?)ProductInstanceStatusEnum.Emitted,
		"APPLIED" => (ProductInstanceStatusEnum?)ProductInstanceStatusEnum.Applied,
		"APPLIED_PAID" => (ProductInstanceStatusEnum?)ProductInstanceStatusEnum.AppliedPaid,
		"INTRODUCED" => (ProductInstanceStatusEnum?)ProductInstanceStatusEnum.Introduced,
		"WRITTEN_OFF" => (ProductInstanceStatusEnum?)ProductInstanceStatusEnum.WrittenOff,
		"RETIRED" => (ProductInstanceStatusEnum?)ProductInstanceStatusEnum.Retired,
		"WITHDRAWN" => (ProductInstanceStatusEnum?)ProductInstanceStatusEnum.Withdrawn,
		"DISAGGREGATION" => (ProductInstanceStatusEnum?)ProductInstanceStatusEnum.Disaggregation,
		"DISAGGREGATED" => (ProductInstanceStatusEnum?)ProductInstanceStatusEnum.Disaggregated,
		"APPLIED_NOT_PAID" => (ProductInstanceStatusEnum?)ProductInstanceStatusEnum.AppliedNotPaid,
		_ => null,
	};
}
