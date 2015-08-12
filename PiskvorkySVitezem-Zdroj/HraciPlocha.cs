using System;
using cz.msdn.Piskvorky.Definice;

namespace cz.msdn.Piskvorky.TestovaciAplikace
{

	/// <summary>
	/// T��da HraciPlocha implementuje hraci plochu - pole kamenu a souvisejici operace
	/// </summary>
	public class HraciPlocha
	{
		/// <summary>
		/// Velikost �tvercov� hrac� plochy (19)
		/// </summary>
		public const int VelikostPlochy=19;

		/// <summary>
		/// Po�et kamen� v �ad�, kter� jsou t�eba k v�t�zstv� ve h�e (5)
		/// </summary>
		public const int PocetVRadeKVitezstvi=5;

		private BarvaKamene[,] hraciPole=new BarvaKamene[VelikostPlochy,VelikostPlochy];
		private SouradnicePole[] viteznaRadaPoli;

		/// <summary>
		/// Hrac� pole jako dvourozm�rn� pole typu BarvaKamene
		/// </summary>
		public BarvaKamene[,] HraciPole
		{
			get
			{
				//Nevrac�me p��mo pole, ale pouze kopii - pole je intern� z�le�itost� a nen� mo�n� ho zven�� m�nit
				return (BarvaKamene[,]) hraciPole.Clone();
			}
		}

		/// <summary>
		/// Um�st�n� hrac�ho kamene na hrac� plochu.
		/// </summary>
		/// <param name="souradnice">Sou�adnice, kam bude k�men um�st�n</param>
		/// <param name="barva">Barva kamene, kter� je t�eba um�stit</param>
		/// <returns>Vrac� true, pokud bylo pole neobsazen� a k�men byl um�st�n, jinak false</returns>
		public bool UmistiKamen(SouradnicePole souradnice, BarvaKamene barva)
		{
			if ((hraciPole[souradnice.Radek,souradnice.Sloupec]!=BarvaKamene.Zadny) || JeVitez)
				return false;
			else
			{
				hraciPole[souradnice.Radek,souradnice.Sloupec]=barva;
				ZkusNajitViteznouRaduPoli();
				return true;
			}
		}

		private void ZkusNajitViteznouRaduPoli()
		{			
			viteznaRadaPoli = null;
			ZkusNajitPrimeVitezneRadyPoli(TransformaceDesky.zadna);
			if(viteznaRadaPoli==null)
				ZkusNajitPrimeVitezneRadyPoli(TransformaceDesky.symetrie);
			if(viteznaRadaPoli==null)
				ZkusNajitSikmeVitezneRadyPoli(TransformaceDesky.zadna);
			if(viteznaRadaPoli==null)
				ZkusNajitSikmeVitezneRadyPoli(TransformaceDesky.rotace);
		}


		/// <summary>
		/// Zji�t�n�, zda m� ji� partie v�t�ze. Vr�t� true, pokud existuje souvisl� �ada 'PocetVRadeKVitezstvi', jinak false.
		/// </summary>
		public bool JeVitez
		{
			get
			{
				return (viteznaRadaPoli != null);
			}
		}


		/// <summary>
		/// Zji�t�n�, kdo v partii vyhr�l. Pokud je v�t�z, vr�t� jeho barvu kamen�, jinak vr�t� Zadny
		/// </summary>
		public BarvaKamene ViteznyHrac
		{
			get
			{
				if(viteznaRadaPoli==null)
					return BarvaKamene.Zadny;
				else
					return hraciPole[viteznaRadaPoli[0].Radek,viteznaRadaPoli[0].Sloupec];
			}
		}


		/// <summary>
		/// Zji�t�n� v�t�zn� �ady kamen� v t�to h�e
		/// </summary>
		public SouradnicePole[] ViteznaRadaPoli
		{
			get
			{
				return (SouradnicePole[]) viteznaRadaPoli.Clone();
			}
		}

		#region Implementa�n� detaily
		private enum TransformaceDesky {zadna=0, symetrie=1, rotace=2}
		private BarvaKamene HraciPoleZarazka(int radek, int sloupec, TransformaceDesky transformace)
		{
			int x=-1,y=-1;
			if	    (transformace== TransformaceDesky.zadna)
			{
				x=radek;y=sloupec;
			}
			else if (transformace == TransformaceDesky.symetrie)
			{
				x = sloupec; y=radek;
			}
			else if (transformace == TransformaceDesky.rotace)
			{
				x= (VelikostPlochy-1)-sloupec; y=radek;
			}
			
			if ((x>=0) && (x<HraciPlocha.VelikostPlochy) && (y>=0) && (y<HraciPlocha.VelikostPlochy))
				return hraciPole[x,y];
			else
				return BarvaKamene.Zadny;
		}

		private void ZkusNajitPrimeVitezneRadyPoli(TransformaceDesky transformace)
		{
			int rada=1;
			for(int y=0;y<HraciPlocha.VelikostPlochy;y++)
				for(int x=0;x<HraciPlocha.VelikostPlochy;x++)
				{
					if (HraciPoleZarazka(x,y,transformace)==BarvaKamene.Cerny && HraciPoleZarazka(x-1,y,transformace) == BarvaKamene.Cerny)
						rada++;
					else if(HraciPoleZarazka(x,y,transformace)==BarvaKamene.Bily && HraciPoleZarazka(x-1,y,transformace) == BarvaKamene.Bily)
						rada++;
					else if(HraciPoleZarazka(x,y,transformace)==BarvaKamene.Cerny && HraciPoleZarazka(x-1,y,transformace) != BarvaKamene.Cerny)
						rada=1;
					else if(HraciPoleZarazka(x,y,transformace)==BarvaKamene.Bily && HraciPoleZarazka(x-1,y,transformace) != BarvaKamene.Bily)
						rada=1;
					else 
						rada=0;
					if(rada==HraciPlocha.PocetVRadeKVitezstvi)	// v�t�zn� n-tice nalezena
					{
						viteznaRadaPoli = new SouradnicePole[HraciPlocha.PocetVRadeKVitezstvi];
						for(int i=x-HraciPlocha.PocetVRadeKVitezstvi+1;i<=x;i++)
							if(transformace==TransformaceDesky.zadna)
								viteznaRadaPoli[i-(x-HraciPlocha.PocetVRadeKVitezstvi+1)] = new SouradnicePole(i,y);
							else	//TransformaceDesky.symetrie
								viteznaRadaPoli[i-(x-HraciPlocha.PocetVRadeKVitezstvi+1)] = new SouradnicePole(y,i);
						return;
					}
				}
		}

		private void ZkusNajitSikmeVitezneRadyPoli(TransformaceDesky transformace)
		{
			int rada=1;
			for(int diagonala=-HraciPlocha.VelikostPlochy+1;diagonala<=HraciPlocha.VelikostPlochy-1;diagonala++)
				for(int x=0;x<HraciPlocha.VelikostPlochy;x++)
				{
					if		(HraciPoleZarazka(x+diagonala,x,transformace)==BarvaKamene.Cerny && HraciPoleZarazka(x+diagonala-1,x-1,transformace) == BarvaKamene.Cerny)
						rada++;
					else if (HraciPoleZarazka(x+diagonala,x,transformace)==BarvaKamene.Bily && HraciPoleZarazka(x+diagonala-1,x-1,transformace) == BarvaKamene.Bily)
						rada++;
					else if (HraciPoleZarazka(x+diagonala,x,transformace)==BarvaKamene.Cerny && HraciPoleZarazka(x+diagonala-1,x-1,transformace) != BarvaKamene.Cerny)
						rada = 1;
					else if (HraciPoleZarazka(x+diagonala,x,transformace)==BarvaKamene.Bily && HraciPoleZarazka(x+diagonala-1,x-1,transformace) != BarvaKamene.Bily)
						rada = 1;
					else rada = 0;
					if(rada==HraciPlocha.PocetVRadeKVitezstvi)
					{
						viteznaRadaPoli = new SouradnicePole[HraciPlocha.PocetVRadeKVitezstvi];
						for(int i=x-HraciPlocha.PocetVRadeKVitezstvi+1;i<=x;i++)
							if(transformace == TransformaceDesky.zadna)
								viteznaRadaPoli[i-(x-HraciPlocha.PocetVRadeKVitezstvi+1)] = new SouradnicePole(i+diagonala,i);
							else			// TransformaceDesky.rotace
								viteznaRadaPoli[i-(x-HraciPlocha.PocetVRadeKVitezstvi+1)] = new SouradnicePole(VelikostPlochy-1-i,i+diagonala);
						return;
					}
				}
		}

		#endregion
	}
}
