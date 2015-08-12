using System;

namespace cz.msdn.Piskvorky.Definice
{

	/// <summary>
	/// Enumerace urèující obsazenost daného pole
	/// </summary>
	public enum BarvaKamene
	{
		/// <summary>
		/// Pole obsazené èerným kamenem, numerická hodnota 1
		/// </summary>
		Cerny = 1,
		/// <summary>
		/// Pole obsazené bílým kamenem, numerická hodnota -1
		/// </summary>
		Bily = -1,
		/// <summary>
		/// Neobsazené pole, numerická hodnota 0
		/// </summary>
		Zadny=0
	}


	/// <summary>
	/// Tøída pro vyjádøení souøadnic tahu jednoho z hráèù
	/// </summary>
	public class SouradnicePole
	{
		/// <summary>
		/// Velikost hrací plochy (ètverec o velikosti 19)
		/// </summary>
		public const int VelikostPlochy=19;

		/// <summary>
		/// Konstruktor pro vytvoøení a inicializaci souøadnic pole
		/// </summary>
		/// <param name="radek">Èíselný index øádku - 0 až 18</param>
		/// <param name="sloupec">Èíselný index sloupce - 0 až 18</param>
		public SouradnicePole(int radek, int sloupec)
		{
			if ((radek>=0) && (radek<VelikostPlochy))
				Radek=radek;
			else
				throw(new Exception("Hodnota 'radek' mimo pøípustné hranice"));
			if ((sloupec>=0) && (sloupec<VelikostPlochy))
				Sloupec=sloupec;
			else
				throw(new Exception("Hodnota 'sloupec' mimo pøípustné hranice"));
		}

		/// <summary>
		/// Èíselný index øádku - 0 až 18. Pouze ke ètení.
		/// </summary>
		public readonly int Radek;

		/// <summary>
		/// Èíselný index sloupce - 0 až 18. Pouze ke ètení.
		/// </summary>
		public readonly int Sloupec;
	}


}
