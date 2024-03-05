using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vodovoz.Settings.Logistics
{
	public interface ICarEventSettings
	{
		int CarEventStartNewPeriodDay { get; }
		int CompensationFromInsuranceByCourtId { get; }
	}
}
