using QS.DomainModel.Entity;

namespace VodovozBusiness.Domain.Goods.NomenclaturesOnlineParameters
{
	public class SlangWord : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int? _robotMiaParametersId;
		private string _word;

		public int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		public int? RobotMiaParametersId
		{
			get => _robotMiaParametersId;
			set => SetField(ref _robotMiaParametersId, value);
		}

		public string Word
		{
			get => _word;
			set => SetField(ref _word, value);
		}
	}
}
