using System;
using cz.msdn.Piskvorky.Definice;

namespace cz.msdn.Piskvorky {

	public class EngineA30732: IEngine {

		class Tprior {
			public int pv,ps0,ps1,ps2,ps3; //ohodnocen� ve 4 sm�rech a jejich sou�et
			public int i;       //v kter�m je seznamu dobr�ch tah�
			public int nxt,pre; //dal�� a p�edchoz� prvky seznamu dobr�ch tah�
		};

		struct Tsquare {
			public int z;       //0=pr�zndn�, 1=b�l�, 2=�ern�, 3=hranice okna
			public Tprior[] h;	//ohodnocen� pro oba hr��e
			public int x,y;	    //sou�adnice
		};

		int 
			player, //hr�� na tahu, 0=b�l�, 1=�ern�
			moves,  //po�et tah�
			width,  //���ka hrac�ho pole
			height, //v��ka hrac�ho pole
			height2;//height+2
		int[] diroff= new int[9]; //vzd�lenosti na sousedn� pol��ka v hrac�m poli
					 
		//---------------------------------------------------------------------------
		const int  //konstanty pro ohodnocovac� funkci
			H10=2,  H11=6,   H12=10,
			H20=23, H21=158, H22=175,
			H30=256,H31=511, H4=2047;
		int[,] priority={{0,1,1},{H10,H11,H12},{H20,H21,H22},{H30,H31,0},{H4,0,0}};

		int[] sum={0,0};	//celkov� sou�ty priorit v�ech pol� pro oba hr��e
		int dpth=0,depth; //hloubka rekurze
		int[] D={7,5,4};

		const int McurMoves=384, MwinMoves=400;
		int[] 
			curMoves= new int[McurMoves],  //buffer na zpracov�van� tahy
			winMoves1= new int[MwinMoves], //buffer na v�hern� kombinace
			winEval= new int[MwinMoves];

		int[,] 
			goodMoves= new int[4,2],	//seznamy pol��ek s velk�m ohodnocen�m; pro oba hr��e
			winMove= new int[2,2];		//m�sto, kde u� se d� vyhr�t; pro oba hr��e
		
		int 
			UwinMoves,
			lastMove,
			bestMove;  //v�sledn� tah po��ta�e

		Tsquare[] board;  //hrac� plocha
		int boardk; //konec hrac� plochy
		Random generator;
		//---------------------------------------------------------------------------
		int max(int a,int b){
			return (a>b) ? a:b;	
		}
		int abs(int x) {
			return x>=0 ? x : -x;
		} 
		int distance(int p1, int p2) {
			return max( abs(board[p1].x - board[p2].x), 
				abs(board[p1].y - board[p2].y) );
		}
		//---------------------------------------------------------------------------
		public SouradnicePole NajdiNejlepsiTah(BarvaKamene[,] hraciPole, BarvaKamene barvaNaTahu, TimeSpan zbyvajiciCasNaPartii) {
			int p,x,y;

			//nastav hloubku p�em��len� podle zb�vaj�c�ho �asu
			depth= 4+(int)zbyvajiciCasNaPartii.TotalSeconds/60;
			//p�i prvn�m tahu zjisti, jestli jsem b�l� nebo �ern�
			if(moves==0) {
				player=0;
				if(barvaNaTahu==BarvaKamene.Bily) player=1;
			}
			//najdi v hrac�m poli posledn� tah soupe�e
			p=6*height2+1;
			for(x=0; x<width; x++){
				for(y=0; y<height; y++){
					switch(hraciPole[y,x]){
					case BarvaKamene.Bily:
						if(board[p].z!=1){
							doMove(p);
							goto mujTah;
						}
						break;
					case BarvaKamene.Cerny:
						if(board[p].z!=2){
							doMove(p);
							goto mujTah;
						}
						break;
					}
					p++;
				}
				p+=2;
			}
			//tah soupe�e nenalezen => j� za��n�m		
			player=0;
			if(barvaNaTahu==BarvaKamene.Cerny) player=1;
			//prove� sv�j tah
			mujTah:
			computer1();
			return new SouradnicePole(board[lastMove].y, board[lastMove].x);
		}
		//---------------------------------------------------------------------------

		public EngineA30732() {
			int x,y,k;
			int p;
			Tprior pr;
	
			generator = new Random();
			//alokuj hrac� plochu
			width=height=SouradnicePole.VelikostPlochy;
			height2=height+2;
			board= new Tsquare[(width+12)*(height2)]; //jednorozm�rn� pole !
			boardk= (width+6)*height2;
			//offsety pro pohyb do v�ech osmi sm�r�
			diroff[0]=1;            
			diroff[4]=-diroff[0];
			diroff[1]=(1+height2);
			diroff[5]=-diroff[1];
			diroff[2]=height2;
			diroff[6]=-diroff[2];
			diroff[3]=(-1+height2);
			diroff[7]=-diroff[3];
			diroff[8]=0;
	
			//vynuluj pole
			p=0;
			for(x=-5; x<=width+6; x++){
				for(y=0; y<=height+1; y++){
					board[p].z= (x<1 || y<1 || x>width || y>height) ? 3:0;
					board[p].x= x-1;
					board[p].y= y-1;
					board[p].h= new Tprior[2];
					for(k=0;k<2;k++) {
						board[p].h[k]= pr = new Tprior();
						pr.i=0;
						pr.pv=4;
						pr.ps0=pr.ps1=pr.ps2=pr.ps3=1;
					}
					p++;
				}
			}
			moves=0;
			//vytvo� pomocnou tabulku pro zrychlen� ohodnocovac� funkce
			gen();
		}
		//---------------------------------------------------------------------------
		//ud�l� tah na pole p
		bool doMove(int p) {
			if(board[p].z!=0) return false;
			board[p].z= player+1;
			player=1-player;
			//zvy� po��tadlo tah�
			moves++;
			//p�epo��tej ohodnocen�
			evaluate(p);
			lastMove=p;
			return true;
		}
		//---------------------------------------------------------------------------
		short[,] K=new short[2,262144]; //ohodnocen� pro v�echny kombinace 9 pol�
		static int[] comb=new int[10];
		static int ind;
		static int[] n=new int[4];
		//---------------------------------------------------------------------------
		void gen2(int pos) {
			int pb,pe,a1,a2;
			int n1,n2,n3;
			int s;

			if(pos==9){
				a1=a2=0;
				if(comb[4]==0){
					n1=n[1]; n2=n[2]; n3=n[3];
					pb=0;
					pe=4;
					while(pe!=9){
						if(n3==0){
							if(n2==0){
								s=0;
								if(comb[pb]==0 && comb[pe+1]<2 && pb!=4){
									s++;
									if(comb[pe]==0 && pe!=4) s++;
								}
								int pri= priority[n1,s];
								if(a1<pri) a1=pri;
							}
							if(n1==0){
								s=0;
								if(comb[pb]==0 && (comb[pe+1]&1)==0 && pb!=4){
									s++;
									if(comb[pe]==0 && pe!=4) s++;
								}
								int pri= priority[n2,s];
								if(a2<pri) a2=pri;
							}
						}
						switch(comb[++pe]) {
						case 1: n1++; break;
						case 2: n2++; break;
						case 3: n3++; break;
						}
						switch(comb[pb++]){
						case 1: n1--; break;
						case 2: n2--; break;
						case 3: n3--; break;
						}
					}
				}
				K[0,ind]= (short)a1;
				K[1,ind]= (short)a2;
				ind++;
			}else{
				//vygeneruj postupn� v�echny kombinace 
				for(int z=0; z<4; z++){
					comb[pos]=z;
					gen2(pos+1);
				}
			}
		}

		void gen1(int pos) {
			if(pos==5) gen2(pos);
			else{
				for(int z=0; z<4; z++){
					comb[pos]=z;
					n[z]++;
					gen1(pos+1);
					n[z]--;
				}
			}
		}

		void gen() {
			ind=0;
			gen1(0);
		}
		//---------------------------------------------------------------------------
		//p�epo��t� ohodnocen� pol� do vzd�lenosti 4 od pol��ka p0
		void evaluate(int p0) {
			int i,k,m,s,h;
			Tprior pr;
			int p,q,qk,pe,pk1;
			int ind;
			int pattern;

			//zapln�n� pole odstra� ze seznamu a dej mu nulovou prioritu
			if(board[p0].z!=0){
				for(k=0; k<2; k++){
					pr= board[p0].h[k];
					if(pr.pv!=0){
						if(pr.i!=0){
							board[pr.nxt].h[k].pre= pr.pre;
							if(pr.pre!=0) board[pr.pre].h[k].nxt= pr.nxt;
							else goodMoves[pr.i,k]= pr.nxt;
							pr.i=0;
						}
						sum[k]-= pr.pv;
						pr.pv= pr.ps0= pr.ps1= pr.ps2= pr.ps3= 0;
					}
				}
			}
			//zpracuj v�echny 4 sm�ry
			for(i=0; i<4; i++){
				s=diroff[i];
				pk1=p0;
				pk1+= s*5;
				pe=p0;
				p=p0;
				for(m=4; m>0; m--){
					p-=s;
					if(board[p].z==3){
						pe += s*m;
						p+=s;
						break;
					}
				}
				pattern=0;
				qk=pe;
				qk-= s*9;
				for(q=pe; q!=qk; q-=s){
					pattern*=4;
					pattern+= board[q].z;
				}
				while(board[p].z!=3){
					if(board[p].z==0){
						for(k=0; k<2; k++){ //pro oba hr��e
							pr= board[p].h[k];
							//oprav prioritu v jednom sm�ru
							h= K[k,pattern];
							switch(i) {
							case 0:
								m=pr.ps0; pr.ps0=h;
								break;
							case 1:
								m=pr.ps1; pr.ps1=h;
								break;
							case 2:
								m=pr.ps2; pr.ps2=h;
								break;
							case 3:
								m=pr.ps3; pr.ps3=h;
								break;
							}
							m=h-m;
							if(m!=0){
								sum[k]+=m;
								pr.pv+=m;
								//podle ohodnocen� ur�i seznam
								ind=0;
								if(pr.pv >= H21){
									ind++;
									if(pr.pv >= 2*H21){
										ind++;
										if(pr.pv >= H4) ind++;
									}
								}
								//p�eho� pol��ko do jin�ho seznamu
								if(ind!=pr.i){
									//odpoj
									if(pr.i!=0){
										board[pr.nxt].h[k].pre= pr.pre;
										if(pr.pre!=0) board[pr.pre].h[k].nxt= pr.nxt;
										else goodMoves[pr.i,k]= pr.nxt;
									}
									//p�ipoj
									if((pr.i=ind)!=0){
										q= pr.nxt= goodMoves[ind,k];
										goodMoves[ind,k]= board[q].h[k].pre= p;
										pr.pre= 0;
									}
								}
							}
						}
					}
					p+=s;
					if(p==pk1) break;
					//rotuj pattern vpravo; zleva na�ti dal�� pol��ko
					pe+=s;
					pattern>>=2;
					pattern+= board[pe].z << 16;
				}
			}
		}
		//---------------------------------------------------------------------------
		//hlavn� rekurzivn� funkce
		//zjist�, jestli prohraju nebo vyhraju
		//p�i dpth==0 nastav� prom�nnou tah
		int alfabeta(int player1, int UcurMoves, int logWin, int last, int strike) {
			int p,q,t, defendMoves1,defendMoves2,UwinMoves0;
			int y,m;
			int i,j,s;
			int pr,hr;
			int mustDefend,mustAttack;

			//kdy� u� jsou �ty�i v �ad�, tak t�hni bez rozm��len�
			p=goodMoves[3,player1];
			if(p!=0){
				if(logWin!=0 && (strike&1)!=0) winMoves1[UwinMoves++]=p;
				return 1000-dpth; //vyhr�l jsem :)
			}
			int player2=1-player1;
			p=goodMoves[3,player2];
			if(p!=0){
				board[p].z=player1+1;
				evaluate(p);
				if((strike&1)!=0)
					y= -alfabeta(player2,UcurMoves,logWin,last,2);
				else
					y= -alfabeta(player2,UcurMoves,logWin,last,1);
				board[p].z=0;
				evaluate(p);
				if(logWin!=0 && y!=0 && ((y>0) == ((strike&1)!=0))) winMoves1[UwinMoves++]=p;
				return y;
			}

			//nejd��ve najdi v�echny dobr� tahy a p�ekop�ruj je do statick�ho pole
			int Utahy0=UcurMoves;
			if((strike&1)==0) hr=player2; else hr=player1;
			mustDefend= mustAttack= 0;
			p=goodMoves[2,player1];
			if(p!=0){
				mustAttack++;
				do{
					//u� m�m t�i v �ad� => m�l bych vyhr�t
					if(logWin==0 && board[p].h[player1].pv >= H31){
						if(dpth==0) bestMove=p;
						return 999-dpth;
					}
					if(UcurMoves==McurMoves) break;
					pr=board[p].h[hr].pv;
					for(q=UcurMoves++; q>Utahy0 && 
						board[curMoves[q-1]].h[hr].pv < pr; q--){
						curMoves[q] = curMoves[q-1];
					}
					curMoves[q] = p;
					p=board[p].h[player1].nxt;
				}while(p!=0);
			}
			defendMoves1=UcurMoves;
			for(p=goodMoves[2,player2]; p!=0; p=board[p].h[player2].nxt){
				//soupe� m� t�i v �ad� => mus�m se br�nit
				if(board[p].h[player2].pv >= H30+H21){
					if(mustDefend==0) mustDefend=1;
					if(board[p].h[player2].pv >= H31) mustDefend=2;
				}else{
					if(mustAttack!=0) continue;
				}
				if(UcurMoves==McurMoves) break;
				pr=board[p].h[hr].pv;
				for(q=UcurMoves++; q>defendMoves1 && 
					board[curMoves[q-1]].h[hr].pv < pr; q--){
					curMoves[q] = curMoves[q-1];
				}
				curMoves[q] = p;
			}
			defendMoves2=UcurMoves;

			if(dpth<depth){
				//d�vej se jen na okol� posledn�ho tahu
				if(strike<2 && last!=0){
					for(i=0; i<8; i++){
						s=diroff[i];
						p=last; 
						p+=s;
						for(j=0; j<4 && (board[p].z!=3);  j++, p+=s){
							if( (strike&1)==0 && board[p].h[player2].i==1
								&& (mustAttack==0 || board[p].h[player2].pv >= H30)
								|| board[p].h[player1].i==1 && 
								(mustDefend==0 || board[p].h[player1].pv >= H30) ){
								if(UcurMoves<McurMoves){
									pr=board[p].h[hr].pv;
									for(q=UcurMoves++; q>defendMoves2 && 
										board[curMoves[q-1]].h[hr].pv < pr; q--){
										curMoves[q] = curMoves[q-1];
									}
									curMoves[q] = p;
								}
							}
						}
					}
				}else{
					//obrana
					if(strike==2 && mustDefend<2){
						for(p=goodMoves[1,player2]; p!=0; p=board[p].h[player2].nxt){
							if(UcurMoves==McurMoves) break;
							if(  (last==0 || distance(p,last) < D[mustDefend])
								&& (mustAttack==0 || board[p].h[player2].pv >= H30)
								){
								if(UcurMoves==McurMoves) break;
								pr=board[p].h[hr].pv;
								for(q=UcurMoves++; q>defendMoves2 && 
									board[curMoves[q-1]].h[hr].pv < pr; q--){
									curMoves[q] = curMoves[q-1];
								}
								curMoves[q] = p;
							}
						}
						defendMoves2=UcurMoves;
					}
					//�tok
					for(p=goodMoves[1,player1]; p!=0; p=board[p].h[player1].nxt){
						if(UcurMoves==McurMoves) break;
						if( (last==0 || distance(p,last) < 7)
							&& (mustDefend==0 || board[p].h[player1].pv >= H30)
							){
							if(UcurMoves==McurMoves) break;
							pr=board[p].h[hr].pv;
							for(q=UcurMoves++; q>defendMoves2 && 
								board[curMoves[q-1]].h[hr].pv < pr; q--){
								curMoves[q] = curMoves[q-1];
							}
							curMoves[q] = p;
						}
					}
				}
			}

			if(Utahy0==UcurMoves)
				return 0; //nelze nikde za�to�it nebo u� jsem moc hluboko

      //dobr� tahy jsou v poli curMoves => ohodno� je a vyber nejlep��
			UwinMoves0=UwinMoves;
			m=-0x7ffe;
			for(t=Utahy0; t<UcurMoves; t++){
				dpth++;
				p=curMoves[t];
				//prove� tah
				board[p].z=player1+1;
				evaluate(p);
				//rekurze
				if((strike&1)!=0){
					if(t>=defendMoves2 || t<defendMoves1)
						//�to�n� tah, aktualizuj m�sto posledn�ho �toku
						y= -alfabeta(player2,UcurMoves,logWin,p,0);
					else
						//obrann� tah, nem�n�m pol��ko posledn�ho �toku
						//soupe� z�skal tah nav�c a m��e se br�nit, kde bude cht�t
						y= -alfabeta(player2,UcurMoves,logWin,last,2);
				}else{
					y= -alfabeta(player2,UcurMoves,logWin,last,1);
				}
				//sma�, co jsi p�idal
				board[p].z=0;
				evaluate(p);
				dpth--;
				if(y>0){
					//vyhraju
					if(dpth==0) bestMove=p;
					if(logWin!=0 && (strike&1)!=0) winMoves1[UwinMoves++]=p;
					return y;
				}
				if(y==0){
					if((strike&1)==0){
						//ubr�n�m se
						UwinMoves=UwinMoves0;
						if(dpth==0) bestMove=p;
						return y;
					}
					m=y;
				}else if(y>=m){
					//asi prohraju, mus�m zkou�et dal�� mo�n� tahy
					if(logWin!=0 && (strike&1)==0){ winMoves1[UwinMoves++]=p; logWin=0; }
					if(dpth==0)
						//vyber tah, kter�m prohraju co nejpozd�ji
						if(y>m || board[p].h[player2].pv > board[bestMove].h[player2].pv) 
							bestMove=p;
					m=y;
				}
			}
			return m;
		}
		//---------------------------------------------------------------------------
		int try4(int player1, int last) {
			int i,j,s;
			int p,p2=0,y=0;

			p=goodMoves[3,player1];
			if(p!=0){
				winMoves1[UwinMoves++]=p;
				return p; //vyhr�l jsem
			}
			int player2=1-player1;

			for(i=0; i<8; i++){
				s=diroff[i];
				p=last; 
				p+=s;
				for(j=0;  j<4 && (board[p].z!=3);  j++, p+=s){
					if(board[p].h[player1].pv>=H30){
						//�tok
						board[p].z=player1+1;
						evaluate(p);
						if(goodMoves[3,player2]==0){
							p2=goodMoves[3,player1];
							if(p2!=0){
								//obrana - jen jedin� mo�nost
								board[p2].z=2-player1;
								evaluate(p2);
								//rekurze
								y=try4(player1, p);
								board[p2].z=0;
								evaluate(p2);
							}
						}
						board[p].z=0;
						evaluate(p);
						if(y!=0){
							winMoves1[UwinMoves++]=p2;
							winMoves1[UwinMoves++]=p;
							return p;
						}
					}
				}
			}
			return 0;
		}
		//---------------------------------------------------------------------------
		//zkou�ej jen vynucen� tahy, kdy �to�n�k d�l� jen �tve�ice
		//hloubka rekurze nen� omezena 
		int try4(int player1) {
			int p,p2,y=0,t;
			int j;

			UwinMoves=0;
			t=0;
			for(j=1; j<=2; j++){
				for(p=goodMoves[j,player1]; p!=0; p=board[p].h[player1].nxt){
					if(board[p].h[player1].pv>=H30){
						if(t==McurMoves) break;
						curMoves[t++]=p;
					}
				}
			}
			for(t--; t>=0; t--){
				p=curMoves[t];
				board[p].z=player1+1;
				evaluate(p);
				if(goodMoves[3,1-player1]==0){
					p2=goodMoves[3,player1];
					if(p2!=0){
						board[p2].z= 2-player1;
						evaluate(p2);
						y=try4(player1, p);
						board[p2].z=0;
						evaluate(p2);
					}
					board[p].z=0;
					evaluate(p);
					if(y!=0){
						winMoves1[UwinMoves++]=p2;
						winMoves1[UwinMoves++]=p;
						return p;
					}
				}
			}
			return 0;
		}
		//---------------------------------------------------------------------------
		int alfabeta(int strike, int player1, int logWin, int last) {
			/* int y=0;
			 if(depth>5 && player==1){
				 depth-=5;
				 y=alfabeta(player1,curMoves,logWin,last,strike);
				 setDepth();
			 }
			 if(!y) y=*/
			return alfabeta(player1,0,logWin,last,strike);
		}
		//---------------------------------------------------------------------------
		//zjisti ohodnocen� pol��ka p0 pro hr��e player1
		int getEval(int player1, int p0) {
			int i,s,y,c1,c2,n;
			int p;

			y=0;
			//pod�vej se na okoln� pol��ka
			c1=c2=0;
			for(i=0; i<8; i++){
				s=diroff[i];
				p=p0;
				p+=s;
				if(board[p].z==player1+1) c1++;
				if(board[p].z==2-player1) c2++;
			}
			n=0;
			if(board[p0].h[player1].ps0<2) n++;
			if(board[p0].h[player1].ps1<2) n++;
			if(board[p0].h[player1].ps2<2) n++;
			if(board[p0].h[player1].ps3<2) n++;
			if(n>2) y-=8;
			if(c1+c2==0) y-=20;
			if(c2==0 && c1>0 && board[p0].h[player1].pv>9){
				y+= (c1+1)*5;
			}
			if(board[p0].h[1-player1].pv<5){
				n=0;
				if(board[p0].h[player1].ps0>=H12) n++;
				if(board[p0].h[player1].ps1>=H12) n++;
				if(board[p0].h[player1].ps2>=H12) n++;
				if(board[p0].h[player1].ps3>=H12) n++;
				y+=15;
				if(n>1) y+=n*64;
			}
			return y + board[p0].h[player1].pv;
		}
		//---------------------------------------------------------------------------
		int getEval(int p) {
			int a,b;
			a=getEval(0,p);
			b=getEval(1,p);
			//zkombinuj ohodnocen� obou hr���
			return a>b ? a+b/2 : a/2+b;
		}
		//---------------------------------------------------------------------------
		//obrana, zkou�ej t�hnout na pol��ka z pole winMoves1
		int defend(int player1) {
			int p,t;
			int m,mv,mh,y,yh,Nwins,i,j;
			int player2=1-player1;
			int th,thm=0;

			dpth++;
			Nwins= UwinMoves;
			//vypo�ti ohodnocen� v�ech pol��ek v seznamu
			for(t=UwinMoves-1,th=Nwins-1; t!=-1; t--,th--){
				winEval[th]= getEval(player2,winMoves1[t]);
			}

			mh=m=-0x7ffe;
			for(i=0;  Nwins>0 && i<20;  i++){
				//vyber pol��ko s nejv�t��m ohodnocen�m
				mv=-0x7ffe;
				for(th=Nwins-1; th!=-1; th--){
					if(winEval[th]>mv){ thm=th; mv=winEval[th]; }
				}
				if(mv < 25) break;
				//vyjmi ho ze seznamu
				j = thm;
				p= winMoves1[j];
				Nwins--;
				winMoves1[j]=winMoves1[Nwins];

				board[p].z= player1+1;
				evaluate(p);
				y = -alfabeta(3,player2,0,0);
				board[p].z=0;
				evaluate(p);
				yh= winEval[thm] + y*20;
				if(yh>mh){
					m=y;
					mh=yh;
					bestMove=p;
					if(y>0 || y==0 && winMove[player,player1]==0) break;
				}
				winEval[thm] = winEval[Nwins];
			}
			if(m<0){
				//kdy� m�m prohr�t, zkus�m je�t� za�to�it (n�kdy to pom��e)
				t=0;
				for(p=goodMoves[1,player1]; p!=0; p=board[p].h[player1].nxt){
					if(board[p].h[player1].pv>=H30){
						if(t==MwinMoves) break;
						winMoves1[t++]=p;
					}
				}
				for(t--; t>=0; t--){
					p=winMoves1[t];
					board[p].z=player1+1;
					evaluate(p);
					y = -alfabeta(3,player2,0,0);
					board[p].z=0;
					evaluate(p);
					if(y>m){
						m=y;
						bestMove=p;
						if(y>=0) break;
					}
				}
			}
			dpth--;
			return m;
		}
		//---------------------------------------------------------------------------
		//najde pol��ko s nejv�t��m ohodnocen�m
		//kdy� je ohodnocen� moc mal�, vr�t� 0
		int findMax(int player1) {
			int p,t;
			int m,r;
			int i,k;

			m=-1;
			t=0;
			for(i=2; i>0 && t==0; i--)
				for(k=0; k<2; k++)
					for(p=goodMoves[i,k]; p!=0; p=board[p].h[k].nxt){
						r= getEval(p);
						if(r>m){
							m=r;
							t=p;
						}
					}
			return t;
		}
		//---------------------------------------------------------------------------
		//zjisti, jak� bude celkov� ohodnocen� po n�kolika taz�ch
		int lookAhead(int player1) {
			int p;
			int y;

			if(goodMoves[3,player1]!=0) return 500; //byla nalezena v�hra
			int player2=1-player1;
			p=goodMoves[3,player2];
			if(p==0 && dpth<4) p=findMax(player1);
			if(p==0){
				return (sum[player1]-sum[player2])/3;
			}
			dpth++;
			board[p].z=player1+1;
			evaluate(p);
			y= -lookAhead(player2);
			board[p].z=0;
			evaluate(p);
			dpth--;
			return y;
		}
		//---------------------------------------------------------------------------
		void computer1() {
			int p;
			int Nresults=0;
			int m,y=0,rnd;
			int r;
			int player1=player, player2=1-player1;

			//prvn� tah bude uprost�ed hrac� plochy
			if(moves==0){
				doMove((width/2+6)*height2 + height/2+1);
				return;
			}
			//druh� tah dej n�hodn� na n�kter� sousedn� pol��ko
			if(moves==1){
				for(;;) //prvn� tah mohl b�t na okraji nebo v rohu !
					switch(generator.Next(0,4)){
					case 0: if(doMove(lastMove+1)) return;
						break;
					case 1: if(doMove(lastMove-1)) return;
						break;
					case 2: if(doMove(lastMove+height2)) return;
						break;
					case 3: if(doMove(lastMove-height2)) return;
						break;
					}
			}
			lastMove=-1;

			//kdy� u� jsou �ty�i v �ad�, tak t�hni bez rozm��len�
			if(doMove(goodMoves[3,player1])) return; //pr�v� jsem vyhr�l
			if(doMove(goodMoves[3,player2])) return; //mus�m se br�nit

			//zkou�ej d�lat v�echny mo�n� �tve�ice
			bestMove=0;
			if(doMove(try4(player1))) return; //ur�it� vyhraju

			//co kdy� soupe� bude d�lat jen �tve�ice 
			p=try4(player2);
			if(p!=0){
				//soupe� m��e vyhr�t => mus�m se br�nit 	
				winMove[player1,player2]=p;
				y=1;
			}else{
				bestMove=0;
				if(winMove[player1,player1]!=0){
				 //v p�edchoz�m tahu byla nalezena v�hra
				 y= alfabeta(1,player1,0,winMove[player1,player1]);
				 if(y<=0) winMove[player1,player1]= 0; //soupe�i se poda�ilo se ubr�nit
				}
				//zjisti, zda u� m��u vyhr�t
				if(bestMove==0){
					y= alfabeta(3,player1,0,0);
				}
				if(y>0 && bestMove!=0){
					//pravd�podobn� vyhraju
					doMove(bestMove);
					//zapamatuj si toto m�sto, abys v p��t�m tahu nehr�l n�kde jinde
					winMove[player1,player1]=bestMove;
					return;
				}
				//zjisti, zda soupe� nem��e vyhr�t
				y=0;
				if(winMove[player1,player2]!=0){
					UwinMoves=0;
					y= alfabeta(1,player2,1,winMove[player1,player2]);
					if(y<=0){
						winMove[player1,player2]=0; //soupe� mohl vyhr�t, ale pokazil to
					}
				}
				if(y<=0){
					UwinMoves=0;
					y= alfabeta(3,player2,1,0);
					if(y>0) winMove[player1,player2]=bestMove; //asi prohraju
				}
			}
			bestMove=0;

			if(y>0){
			 //obrana
				if(UwinMoves>0){
					//zkou�ej se br�nit jen na pol��k�ch, kde byla nalezena v�hra soupe�e
					defend(player1);
				}
				if(bestMove==0){
					//zkou�ej se br�nit kdekoli
					alfabeta(2,player1,0,0);
				}
			}

			if(bestMove==0 && moves>9){
				//prohled�v�n� do hloubky nenalezlo v�hern� tahy
				m=-0x7ffffffe;
				for(p=0; p<boardk; p++) {
					if(board[p].z==0 && (board[p].h[0].pv>10 || board[p].h[1].pv>10)) {
						r= getEval(p);
						board[p].z=player1+1;
						evaluate(p);
						r-= lookAhead(player2);
						board[p].z=0;
						evaluate(p);
						if(r>m) {
							m=r;
							bestMove=p;
							Nresults=1;
						}
						else if(r>m-20) {
							Nresults++;
							if(generator.Next(0,Nresults)==0) bestMove=p;
						}
					}
				}
			}
			if(bestMove==0) {
				//vyber pol��ko s nejv�t��m ohodnocen�m
				m=-1;
				for(p=0; p<boardk; p++) {
					if(board[p].z==0) {
						r=getEval(p);
						if(r>m) m=r;
					}
				}
				//n�hodn� zvol pol��ko, kter� m� ohodnocen� o trochu men�� ne� nejlep��
				rnd= m/12;
				if(rnd>30) rnd=30;
				Nresults=0;
				for(p=0; p<boardk; p++) {
					if(board[p].z==0) {
						if(getEval(p) >= m-rnd) {
							Nresults++;
							if(generator.Next(0,Nresults)==0) bestMove=p;
						}
					}
				}
		
			}
			//kone�n� prove� sv�j tah 
			doMove(bestMove);
		}
		//---------------------------------------------------------------------------
	}
}