using System;

namespace cz.msdn.Piskvorky.Definice
{

	/// <summary>
	/// Enumerace ur�uj�c� obsazenost dan�ho pole
	/// </summary>
	public enum BarvaKamene
	{
		/// <summary>
		/// Pole obsazen� �ern�m kamenem, numerick� hodnota 1
		/// </summary>
		Cerny = 1,
		/// <summary>
		/// Pole obsazen� b�l�m kamenem, numerick� hodnota -1
		/// </summary>
		Bily = -1,
		/// <summary>
		/// Neobsazen� pole, numerick� hodnota 0
		/// </summary>
		Zadny=0
	}


	/// <summary>
	/// T��da pro vyj�d�en� sou�adnic tahu jednoho z hr���
	/// </summary>
	public class SouradnicePole
	{
		/// <summary>
		/// Velikost hrac� plochy (�tverec o velikosti 19)
		/// </summary>
		public const int VelikostPlochy=19;

		/// <summary>
		/// Konstruktor pro vytvo�en� a inicializaci sou�adnic pole
		/// </summary>
		/// <param name="radek">��seln� index ��dku - 0 a� 18</param>
		/// <param name="sloupec">��seln� index sloupce - 0 a� 18</param>
		public SouradnicePole(int radek, int sloupec)
		{
			if ((radek>=0) && (radek<VelikostPlochy))
				Radek=radek;
			else
				throw(new Exception("Hodnota 'radek' mimo p��pustn� hranice"));
			if ((sloupec>=0) && (sloupec<VelikostPlochy))
				Sloupec=sloupec;
			else
				throw(new Exception("Hodnota 'sloupec' mimo p��pustn� hranice"));
		}

		/// <summary>
		/// ��seln� index ��dku - 0 a� 18. Pouze ke �ten�.
		/// </summary>
		public readonly int Radek;

		/// <summary>
		/// ��seln� index sloupce - 0 a� 18. Pouze ke �ten�.
		/// </summary>
		public readonly int Sloupec;
	}


}
