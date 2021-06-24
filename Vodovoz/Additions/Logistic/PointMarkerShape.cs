namespace Vodovoz.Additions.Logistic
{
	public enum PointMarkerShape
	{
		none = 0, 
		// < 6 бутылей
		circle,
		// 6 - 10 бутылей
		triangle,
		// 10 - 20 бутылей
		square,
		// 20 - 40 бутылей
		cross,
		// > 40 бутылей
		star,
		//без формы
		custom,
		// < 6 бутылей
		overduecircle,
		// 6 - 10 бутылей
		overduetriangle,
		// 10 - 20 бутылей
		overduesquare,
		// 20 - 40 бутылей
		overduecross,
		// > 40 бутылей
		overduestar
	}
}
