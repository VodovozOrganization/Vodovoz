using System;

namespace VodovozInfrastructure.Events
{
	public class PercentEventArgs : EventArgs
	{
		public int Percent { get; }

		public PercentEventArgs(int percent)
		{
			Percent = percent;
		}
	}
}
