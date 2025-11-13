using QS.Extensions.Observable.Collections.List;

namespace Vodovoz.Core.Domain.Common
{
	public interface IHasAttachedFilesInformations<TFileInformation>
		where TFileInformation : FileInformation
	{
		IObservableList<TFileInformation> AttachedFileInformations { get; }
	}
}
