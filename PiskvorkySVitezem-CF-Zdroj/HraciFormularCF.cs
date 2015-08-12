using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Data;
using cz.msdn.Piskvorky.Definice;
using System.Threading;

namespace cz.msdn.Piskvorky.TestovaciAplikace
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class HraciFormularCF : System.Windows.Forms.Form
	{
		
		private HraciPlocha plocha=new HraciPlocha();
		private IEngine engine;
		private BarvaKamene barvaEnginu;
		private BarvaKamene barvaCloveka;
		private System.Windows.Forms.Label Zprava;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem Start1;
		private System.Windows.Forms.MenuItem Start2;
		private cz.msdn.Piskvorky.TestovaciAplikace.KlikaciPanelHraciPlochaCF panelPlocha;


		public HraciFormularCF()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			panelPlocha=new KlikaciPanelHraciPlochaCF();
			panelPlocha.Location = new System.Drawing.Point(10,10);
			panelPlocha.Plocha = null;
			panelPlocha.Size = new System.Drawing.Size(220, 220);
			panelPlocha.ProvedenTah+=new ProvedenTahEventHandler(this.HracProvedlTah);
			this.Controls.Add(panelPlocha);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			base.Dispose( disposing );
		}
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Zprava = new System.Windows.Forms.Label();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.Start1 = new System.Windows.Forms.MenuItem();
			this.Start2 = new System.Windows.Forms.MenuItem();
			// 
			// Zprava
			// 
			this.Zprava.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold);
			this.Zprava.ForeColor = System.Drawing.Color.Red;
			this.Zprava.Location = new System.Drawing.Point(24, 232);
			this.Zprava.Size = new System.Drawing.Size(176, 24);
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.Add(this.menuItem1);
			// 
			// menuItem1
			// 
			this.menuItem1.MenuItems.Add(this.Start1);
			this.menuItem1.MenuItems.Add(this.Start2);
			this.menuItem1.Text = "Nová partie";
			// 
			// Start1
			// 
			this.Start1.Text = "Zaèínám já";
			this.Start1.Click += new System.EventHandler(this.Start1_Click);
			// 
			// Start2
			// 
			this.Start2.Text = "Zaèínáš Ty";
			this.Start2.Click += new System.EventHandler(this.Start2_Click);
			// 
			// HraciFormularCF
			// 
			this.Controls.Add(this.Zprava);
			this.Menu = this.mainMenu1;
			this.Text = "Piškvorky.NET";
			this.Load += new System.EventHandler(this.HraciFormularCF_Load);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>

		static void Main() 
		{
			Application.Run(new HraciFormularCF());
		}

		private void StartInit()
		{
			plocha=new HraciPlocha();
			engine=new cz.msdn.Piskvorky.EngineA30732();
			panelPlocha.Plocha=plocha;
			panelPlocha.BarvaCloveka=this.barvaCloveka;
		}
		
		private void Start1_Click(object sender, System.EventArgs e)
		{
			barvaEnginu=BarvaKamene.Cerny;
			barvaCloveka=BarvaKamene.Bily;
			StartInit();
			ProvedTahEnginu();
		}

		private void Start2_Click(object sender, System.EventArgs e)
		{
			panelPlocha.Aktivni=false;
			barvaEnginu=BarvaKamene.Bily;
			barvaCloveka=BarvaKamene.Cerny;
			StartInit();
			panelPlocha.Aktivni=true;		
			Zprava.Text="Jsi na tahu.";
		}

		private void ProvedTahEnginu()
		{
			panelPlocha.Aktivni=false;
			Zprava.Text="Pøemýšlím.";
			Start1.Enabled=false;
			Start2.Enabled=false;
			ThreadStart ts=new ThreadStart(ProvedTahEnginuAsync);
			Thread t=new Thread(ts);
			t.Priority=ThreadPriority.BelowNormal;
			t.Start();
		}

		TimeSpan maxDobaPremysleni=TimeSpan.FromSeconds(1);
		
		private void ProvedTahEnginuAsync()
		{
			SouradnicePole tah=engine.NajdiNejlepsiTah(plocha.HraciPole, barvaEnginu, maxDobaPremysleni);
			plocha.UmistiKamen(tah,barvaEnginu);
			panelPlocha.AktualizujKamen(tah);
			if(plocha.JeVitez)
			{
				Zprava.Text="Vyhrál jsem.";
				panelPlocha.Aktivni=false;
				panelPlocha.Invalidate();
			}
			else
			{
				Zprava.Text="Jsi na tahu.";
				panelPlocha.Aktivni=true;
			}
			Start1.Enabled=true;
			Start2.Enabled=true;
		}
		
		private void HracProvedlTah(SouradnicePole tah)
		{
			panelPlocha.Aktivni=false;
			Zprava.Text="";
			plocha.UmistiKamen(tah,barvaCloveka);
			panelPlocha.AktualizujKamen(tah);
			if(plocha.JeVitez)
			{
				Zprava.Text="Vyhrál jsi.";
				panelPlocha.Aktivni=false;
				panelPlocha.Invalidate();
			}
			else
				ProvedTahEnginu();
		}

		private void HraciFormularCF_Load(object sender, System.EventArgs e)
		{
		
		}

	}
}
