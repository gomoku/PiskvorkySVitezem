using System;

namespace cz.msdn.Piskvorky.Definice
{
	/// <summary>
	/// Rozhran� definuj�c� funkci hrac�ho enginu.
	/// </summary>
	public interface IEngine
	{
		/// <summary>
		/// Nalezeni nejlepsiho tahu v dan� situaci.
		/// </summary>
		/// <param name="hraciPole">Hrac� pole jako dvourozm�rn� �tvercov� pole o velikosti HraciPlocha.VelikostPlochy</param>
		/// <param name="barvaNaTahu">Barva kamene, kter� je pr�v� na tahu.</param>
		/// <param name="zbyvajiciCasNaPartii">�as, kter� m� je�t� engine k dispozici na v�echny zb�vaj�c� tahy v partii. Engine, kter� v�echen �as vy�erp� jako prvn�, prohr�v� partii. Hodnotu zbyvajiciCasNaPartii dost�v� engine od programu-rozhod��ho z�pasu a pro v�sledek z�pasu je sm�rodatn� m��en� prov�d�n� po�adateli, proto by m�l program zach�zet s t�mto �asem s jistou malou toleranc�, kter� eliminuje p��padn� drobn� odchylky v m��en�.</param>
		/// <returns>Sou�adnice zvolen� enginem jako nejlep�� tah.</returns> 
		SouradnicePole NajdiNejlepsiTah(BarvaKamene[,] hraciPole, BarvaKamene barvaNaTahu, TimeSpan zbyvajiciCasNaPartii);
	}
}
