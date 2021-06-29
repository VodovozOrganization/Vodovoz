using Gamma.Utilities;
using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.ViewModels.Reports.DeliveryAnalytics
{
	public class WaveNode: PropertyChangedBase
	{
		private bool selected;
		public virtual bool Selected {
			get => selected;
			set => SetField(ref selected, value);
		}

		private WaveNodes _waveNode;
		public virtual WaveNodes WaveNodes {
			get => _waveNode;
			set => SetField(ref _waveNode, value);
		}

		public override string ToString()
		{
			return _waveNode.GetEnumTitle();
		}
	}
}