using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using cz.msdn.Piskvorky.Definice;

namespace cz.msdn.Piskvorky.TestovaciAplikace
{
	/// <summary>
	/// Hlavní formuláø testovací aplikace
	/// </summary>
	public class HraciFormular : System.Windows.Forms.Form
	{

		private HraciPlocha plocha=new HraciPlocha();
		private IEngine engine;
		private BarvaKamene barvaEnginu;
		private BarvaKamene barvaCloveka;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button Start1;
		private System.Windows.Forms.Button Start2;
		private System.Windows.Forms.Label Zprava;
		private cz.msdn.Piskvorky.TestovaciAplikace.KlikaciPanelHraciPlocha panelPlocha;

		/// <summary>
		/// Vychozi konstruktor.
		/// </summary>
		public HraciFormular()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			panelPlocha.ProvedenTah+=new ProvedenTahEventHandler(this.HracProvedlTah);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panelPlocha = new cz.msdn.Piskvorky.TestovaciAplikace.KlikaciPanelHraciPlocha();
			this.Start1 = new System.Windows.Forms.Button();
			this.Start2 = new System.Windows.Forms.Button();
			this.Zprava = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// panelPlocha
			// 
			this.panelPlocha.Aktivni = false;
			this.panelPlocha.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.panelPlocha.Location = new System.Drawing.Point(8, 8);
			this.panelPlocha.Name = "panelPlocha";
			this.panelPlocha.Plocha = null;
			this.panelPlocha.Size = new System.Drawing.Size(256, 256);
			this.panelPlocha.TabIndex = 0;
			// 
			// Start1
			// 
			this.Start1.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.Start1.Location = new System.Drawing.Point(280, 8);
			this.Start1.Name = "Start1";
			this.Start1.Size = new System.Drawing.Size(136, 23);
			this.Start1.TabIndex = 1;
			this.Start1.Text = "Nová partie - zaèínám já";
			this.Start1.Click += new System.EventHandler(this.Start1_Click);
			// 
			// Start2
			// 
			this.Start2.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.Start2.Location = new System.Drawing.Point(280, 48);
			this.Start2.Name = "Start2";
			this.Start2.Size = new System.Drawing.Size(136, 23);
			this.Start2.TabIndex = 2;
			this.Start2.Text = "Nová partie - zaèínáš Ty";
			this.Start2.Click += new System.EventHandler(this.Start2_Click);
			// 
			// Zprava
			// 
			this.Zprava.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.Zprava.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(238)));
			this.Zprava.ForeColor = System.Drawing.Color.Red;
			this.Zprava.Location = new System.Drawing.Point(280, 88);
			this.Zprava.Name = "Zprava";
			this.Zprava.Size = new System.Drawing.Size(136, 40);
			this.Zprava.TabIndex = 3;
			// 
			// HraciFormular
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(424, 277);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.Zprava,
																		  this.Start2,
																		  this.Start1,
																		  this.panelPlocha});
			this.Name = "HraciFormular";
			this.Text = "Piškvorky.NET";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new HraciFormular());
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

		TimeSpan maxDobaPremysleni=TimeSpan.FromSeconds(5);
		
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
		
		private void HracProvedlTah()
		{
			panelPlocha.Aktivni=false;
			Zprava.Text="";
			SouradnicePole tah=panelPlocha.VybranePole;
			panelPlocha.VybranePole=null;
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
	}
}
