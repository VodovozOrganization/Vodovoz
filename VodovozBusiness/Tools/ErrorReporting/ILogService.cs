using System;
namespace Vodovoz.Tools
{
	public interface ILogService
	{
		string GetLog(int? rowCount = null);
	}
}
