using System.Collections.Generic;

namespace Vodovoz.PermissionExtensions
{
	public interface IPermissionExtensionStore
	{
		IList<IPermissionExtension> PermissionExtensions { get; }
	}
}