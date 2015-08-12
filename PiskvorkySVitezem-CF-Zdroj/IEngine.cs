using System;

namespace cz.msdn.Piskvorky.Definice
{
	/// <summary>
	/// Rozhraní definující funkci hracího enginu.
	/// </summary>
	public interface IEngine
	{
		/// <summary>
		/// Nalezeni nejlepsiho tahu v dané situaci.
		/// </summary>
		/// <param name="hraciPole">Hrací pole jako dvourozmìrné ètvercové pole o velikosti HraciPlocha.VelikostPlochy</param>
		/// <param name="barvaNaTahu">Barva kamene, který je právì na tahu.</param>
		/// <param name="zbyvajiciCasNaPartii">Èas, který má ještì engine k dispozici na všechny zbývající tahy v partii. Engine, který všechen èas vyèerpá jako první, prohrává partii. Hodnotu zbyvajiciCasNaPartii dostává engine od programu-rozhodèího zápasu a pro výsledek zápasu je smìrodatné mìøení provádìné poøadateli, proto by mìl program zacházet s tímto èasem s jistou malou tolerancí, která eliminuje pøípadné drobné odchylky v mìøení.</param>
		/// <returns>Souøadnice zvolené enginem jako nejlepší tah.</returns> 
		SouradnicePole NajdiNejlepsiTah(BarvaKamene[,] hraciPole, BarvaKamene barvaNaTahu, TimeSpan zbyvajiciCasNaPartii);
	}
}
