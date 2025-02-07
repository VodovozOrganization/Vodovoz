using VodovozBusiness.Domain.Cash.CashRequest;

namespace Vodovoz.Application.FileStorage
{
	public interface ICashlessRequestCommentFileStorageService : IEntityFileStorageService<CashlessRequestComment>
	{
		string BucketNameWithPrefix { get; }
	}
}
