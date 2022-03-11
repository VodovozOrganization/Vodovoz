using System;

namespace Vodovoz.Domain.Organizations
{
	public interface IRoboatsEntity
	{
		RoboatsEntityType RoboatsEntityType { get; }
		int? RoboatsId { get; }
		string RoboatsAudiofile { get; set; }
		string NewRoboatsAudiofile { get; set; }
		Guid? FileId { get; set; }
	}
}

