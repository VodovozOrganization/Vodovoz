using System;
using QS.DomainModel.UoW;

namespace Vodovoz.Services
{
	public interface ISubdivisionService
	{
		int GetOkkId();

		//FIXME исправить на нормальную проверку права этого подразделения
		//необходимо правильно хранить подразделения которым запрещен доступ к опредленным функциям системы
		int GetSubdivisionIdForRLAccept();
	}
}
