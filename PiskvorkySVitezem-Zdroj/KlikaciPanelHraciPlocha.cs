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
	public class KlikaciPanelHraciPlocha : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Základní konstruktor pro panel hrací plochy
		/// </summary>
		public KlikaciPanelHraciPlocha()
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
			this.Name = "KlikaciPanelHraciPlocha";
			this.Size = new System.Drawing.Size(256, 256);
			this.Resize += new System.EventHandler(this.PanelHraciPlocha_Resize);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelHraciPlocha_Paint);
			this.MouseLeave += new System.EventHandler(this.OpusteniMysi);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.StisknutiMysi);

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
				if(m_aktivni)
					this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PohybMysi);
				else
					this.MouseMove -= new System.Windows.Forms.MouseEventHandler(this.PohybMysi);
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
			g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			g.Clear(SystemColors.Control);
			double ctv=(Height<Width) ? (double) (Height-1)/SouradnicePole.VelikostPlochy : (double) (Width-1)/SouradnicePole.VelikostPlochy;
			int velkyCtv=(int) (ctv*SouradnicePole.VelikostPlochy);
			g.FillRectangle(Brushes.CadetBlue,0,0,velkyCtv,velkyCtv);
			Pen p=new Pen(Color.Black,1);
			for(int i=0;i<=SouradnicePole.VelikostPlochy;i++)
			{
				g.DrawLine(p,(int) (ctv*i),0,(int) (ctv*i), (int) (ctv*SouradnicePole.VelikostPlochy));
				g.DrawLine(p,0,(int) (ctv*i),(int) (ctv*SouradnicePole.VelikostPlochy),(int) (ctv*i));
			}
			if(Plocha==null) return;
			if(Plocha.JeVitez)
			{
				foreach(SouradnicePole s in Plocha.ViteznaRadaPoli)
				{
					g.FillRectangle(Brushes.LawnGreen,(int)(ctv*s.Sloupec+1), (int)(ctv*s.Radek+1), (int)(ctv*s.Sloupec+ctv)-(int)(ctv*s.Sloupec+1)-1, (int)(ctv*s.Radek+ctv)-(int)(ctv*s.Radek+1)-1);
				}
			}
			BarvaKamene[,] pole=Plocha.HraciPole;
			float prumer=(float) (ctv*0.7);
			double mezera=(ctv-prumer)/2;
			for(int i=0;i<SouradnicePole.VelikostPlochy;i++)
				for(int j=0;j<SouradnicePole.VelikostPlochy;j++)
				{	
					switch(pole[i,j])       
					{         
						case BarvaKamene.Bily:
							g.FillEllipse(Brushes.White,(int)(ctv*j+mezera), (int) (ctv*i+mezera),prumer,prumer);
							break;         
						case BarvaKamene.Cerny:   
							g.FillEllipse(Brushes.Black,(int)(ctv*j+mezera), (int) (ctv*i+mezera),prumer,prumer);
							break;         
					}
				}
			if(VybranePole!=null)
			{
				Brush b=(BarvaCloveka==BarvaKamene.Bily) ? Brushes.White : Brushes.Black;
				g.FillRectangle(Brushes.Yellow,(int)(ctv*VybranePole.Sloupec+1), (int)(ctv*VybranePole.Radek+1), (int)(ctv*VybranePole.Sloupec+ctv)-(int)(ctv*VybranePole.Sloupec+1)-1, (int)(ctv*VybranePole.Radek+ctv)-(int)(ctv*VybranePole.Radek+1)-1);
				g.FillEllipse(b,(int)(ctv*VybranePole.Sloupec+mezera), (int) (ctv*VybranePole.Radek+mezera),prumer,prumer);
			}
		}

		private void PanelHraciPlocha_Resize(object sender, System.EventArgs e)
		{
			this.Invalidate();
		}

		private SouradnicePole m_vybranePole=null;

		/// <summary>
		/// Souradnice prave vybraneho hraciho pole.
		/// </summary>
		public SouradnicePole VybranePole
		{
			get {return m_vybranePole;}
			set
			{
				SouradnicePole puvodni=m_vybranePole;
				if((puvodni!=null)&&((value==null)||(puvodni.Radek!=value.Radek)||(puvodni.Sloupec!=value.Sloupec)))
				{
					m_vybranePole=null;
					this.AktualizujKamen(puvodni);
				}
				if((value!=null)&&((puvodni==null)||(puvodni.Radek!=value.Radek)||(puvodni.Sloupec!=value.Sloupec))&&(Plocha.HraciPole[value.Radek,value.Sloupec]==BarvaKamene.Zadny))
				{
					m_vybranePole=value;
					this.AktualizujKamen(value);
				}
			}
		}

		private void PohybMysi(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			double ctv=(Height<Width) ? (double) (Height-1)/SouradnicePole.VelikostPlochy : (double) (Width-1)/SouradnicePole.VelikostPlochy;
			int radek=(int) (((double)e.Y)/ctv);
			int sloupec=(int) (((double)e.X)/ctv);
			if((radek>=0)&&(radek<HraciPlocha.VelikostPlochy)&&(sloupec>=0)&&(sloupec<HraciPlocha.VelikostPlochy))
				VybranePole=new SouradnicePole(radek,sloupec);
			else
				VybranePole=null;
		}

		private void OpusteniMysi(object sender, System.EventArgs e)
		{
			if(this.VybranePole!=null) VybranePole=null;
		}

		private void StisknutiMysi(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if((e.Button==MouseButtons.Left)&&(VybranePole!=null))
			{
				OnProvedenTah();
			}
		}

		/// <summary>
		/// Provedeni tahu kliknutim mysi na policku
		/// </summary>
		public event ProvedenTahEventHandler ProvedenTah;
		
		protected void OnProvedenTah()
		{
			if(ProvedenTah!=null) ProvedenTah();
		}
	}

	public delegate void ProvedenTahEventHandler();
}
