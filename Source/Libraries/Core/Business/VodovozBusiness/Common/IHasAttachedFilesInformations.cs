using QS.Extensions.Observable.Collections.List;
using VodovozBusiness.Domain.Common;

namespace VodovozBusiness.Common
{
	public interface IHasAttachedFilesInformations<TFileInformationType>
		where TFileInformationType : FileInformation
	{
		IObservableList<TFileInformationType> AttachedFileInformations { get; }
	}
}
