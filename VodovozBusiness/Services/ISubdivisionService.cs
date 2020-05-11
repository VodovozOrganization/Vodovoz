namespace Vodovoz.Services
{
	public interface ISubdivisionService
	{
		int GetOkkId();

		//FIXME исправить на нормальную проверку права этого подразделения
		//необходимо правильно хранить подразделения которым запрещен доступ к опредленным функциям системы
		int GetSubdivisionIdForRLAccept();

		/// <summary>
		/// Возвращает Id родительского подразделения 'Веселый Водовоз'
		/// </summary>
		/// <returns>Id подразделения 'Веселый Водовоз'</returns>
		int GetParentVodovozSubdivisionId();
	}
}
