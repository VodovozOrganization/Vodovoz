using QS.Extensions.Observable.Collections.List;

namespace Vodovoz.Core.Domain.Common
{
	public interface IHasAttachedFilesInformations<TFileInformationType>
		where TFileInformationType : FileInformation
	{
		IObservableList<TFileInformationType> AttachedFileInformations { get; }
	}
}
