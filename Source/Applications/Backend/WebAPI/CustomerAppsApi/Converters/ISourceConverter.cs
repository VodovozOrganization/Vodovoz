using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Goods.NomenclatureOnlineParameters;

namespace CustomerAppsApi.Converters
{
	public interface ISourceConverter
	{
		NomenclatureOnlineParameterType ConvertToNomenclatureOnlineParameterType(Source source);
	}
}
