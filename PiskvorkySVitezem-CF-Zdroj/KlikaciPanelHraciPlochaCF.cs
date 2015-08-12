using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using cz.msdn.Piskvorky.Definice;
using System.Diagnostics;

namespace cz.msdn.Piskvorky.TestovaciAplikace
{
	/// <summary>
	/// Ovládací prvek pro vizualizaci zakladnich vlastnosti tridy HraciPlocha
	/// </summary>
	public class KlikaciPanelHraciPlochaCF : System.Windows.Forms.Control
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Základní konstruktor pro panel hrací plochy
		/// </summary>
		public KlikaciPanelHraciPlochaCF()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// KlikaciPanelHraciPlocha
			// 
			this.Size = new System.Drawing.Size(256, 256);
			this.Resize += new System.EventHandler(this.PanelHraciPlocha_Resize);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelHraciPlocha_Paint);
		}
		#endregion

		private bool m_aktivni=false;

		/// <summary>
		/// Zda lze kliknutim umistit kamen
		/// </summary>
		public bool Aktivni
		{
			get
			{return m_aktivni;}
			set
			{
				m_aktivni=value;
				if(value)
					this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.StisknutiMysi);
				else
					this.MouseDown -= new System.Windows.Forms.MouseEventHandler(this.StisknutiMysi);
			}
		}
		
		private HraciPlocha m_plocha=null;

		/// <summary>
		/// Hrací plocha, kterou tento panel zobrazuje.
		/// </summary>
		public HraciPlocha Plocha
		{
			get
			{
				return m_plocha;
			}
			set
			{
				m_plocha=value;
				Invalidate();
			}
		}

		public BarvaKamene BarvaCloveka=BarvaKamene.Zadny;

		/// <summary>
		/// Aktualizace zobrazení kamene
		/// </summary>
		/// <param name="s">Souøadnice kamene</param>
		public void AktualizujKamen(SouradnicePole s)
		{
			double ctv=(Height<Width) ? (double) (Height-1)/SouradnicePole.VelikostPlochy : (double) (Width-1)/SouradnicePole.VelikostPlochy;
			this.Invalidate(new Rectangle((int)(ctv*s.Sloupec), (int)(ctv*s.Radek),(int) ctv, (int) ctv)); //prekresleni prvku
		}

		private void PanelHraciPlocha_Paint(object sender, PaintEventArgs e)
		{
			Graphics g=e.Graphics;
			g.Clear(SystemColors.Control);
			double ctv=(Height<Width) ? (double) (Height-1)/SouradnicePole.VelikostPlochy : (double) (Width-1)/SouradnicePole.VelikostPlochy;
			int velkyCtv=(int) (ctv*SouradnicePole.VelikostPlochy);
			g.FillRectangle(new SolidBrush(Color.CadetBlue),0,0,velkyCtv,velkyCtv);
			Pen p=new Pen(Color.Black);
			for(int i=0;i<=SouradnicePole.VelikostPlochy;i++)
			{
				g.DrawLine(p,(int) (ctv*i),0,(int) (ctv*i), (int) (ctv*SouradnicePole.VelikostPlochy));
				g.DrawLine(p,0,(int) (ctv*i),(int) (ctv*SouradnicePole.VelikostPlochy),(int) (ctv*i));
			}
			if(Plocha==null) return;
			if(Plocha.JeVitez)
			{
				SolidBrush b=new SolidBrush(Color.LawnGreen);
				foreach(SouradnicePole s in Plocha.ViteznaRadaPoli)
				{
					g.FillRectangle(b,(int)(ctv*s.Sloupec+1), (int)(ctv*s.Radek+1), (int)(ctv*s.Sloupec+ctv)-(int)(ctv*s.Sloupec+1)-1, (int)(ctv*s.Radek+ctv)-(int)(ctv*s.Radek+1)-1);
				}
			}
			BarvaKamene[,] pole=Plocha.HraciPole;
			int prumer=(int) (ctv*0.6);
			double mezera=(ctv-prumer)/2;
			SolidBrush bWhite=new SolidBrush(Color.White);
			SolidBrush bBlack=new SolidBrush(Color.Black);
			for(int i=0;i<SouradnicePole.VelikostPlochy;i++)
				for(int j=0;j<SouradnicePole.VelikostPlochy;j++)
				{	
					switch(pole[i,j])       
					{         
						case BarvaKamene.Bily:
							g.FillEllipse(bWhite,(int)(ctv*j+mezera), (int) (ctv*i+mezera),prumer,prumer);
							break;         
						case BarvaKamene.Cerny:   
							g.FillEllipse(bBlack,(int)(ctv*j+mezera), (int) (ctv*i+mezera),prumer,prumer);
							break;         
					}
				}
		}

		private void PanelHraciPlocha_Resize(object sender, System.EventArgs e)
		{
			this.Invalidate();
		}

		private void StisknutiMysi(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			double ctv=(Height<Width) ? (double) (Height-1)/SouradnicePole.VelikostPlochy : (double) (Width-1)/SouradnicePole.VelikostPlochy;
			int radek=(int) (((double)e.Y)/ctv);
			int sloupec=(int) (((double)e.X)/ctv);
			if((radek>=0)&&(radek<HraciPlocha.VelikostPlochy)&&(sloupec>=0)&&(sloupec<HraciPlocha.VelikostPlochy)&&(Plocha.HraciPole[radek,sloupec]==BarvaKamene.Zadny))
				OnProvedenTah(new SouradnicePole(radek,sloupec));
		}

		/// <summary>
		/// Provedeni tahu kliknutim mysi na policku
		/// </summary>
		public event ProvedenTahEventHandler ProvedenTah;
		
		protected void OnProvedenTah(SouradnicePole s)
		{
			if(ProvedenTah!=null) ProvedenTah(s);
		}
	}

	public delegate void ProvedenTahEventHandler(SouradnicePole s);
}
