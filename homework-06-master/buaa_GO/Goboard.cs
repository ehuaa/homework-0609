/**
 *  Go Applet
 *  1996.11		xinz	written in Java
 *  2001.3		xinz	port to C#
 *  2001.5.10	xinz	file parsing, back/forward
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Diagnostics; 

namespace Go_WinApp
{

	public enum StoneColor : byte
	{
		black = 0, white = 1
	}


	/**
	 * 棋盘类
	 */
	public class GoBoard : System.Windows.Forms.Form
	{
		string [] strLabels; // {"Z","Z","Z","Z","Z","Z","Z","Z","Z","Z","Z","Z","Z","Z","Z","Z","Z","Z","Z","Z"};

		int nSize;		                //棋盘的边范围是 19
		const int nBoardMargin = 10;	//棋盘的边界范围是10
		int nCoodStart = 4;
		const int	nBoardOffset = 20;
		int nEdgeLen = nBoardOffset + nBoardMargin;
		int nTotalGridWidth = 360 + 36;	//棋盘的总长度
		int nUnitGridWidth = 22;		//每一个格子所占的像素
		int nSeq = 0;
        Rectangle rGrid;		    //网格棋盘
		StoneColor m_colorToPlay;   //下一个棋子的颜色. 
		GoMove m_gmLastMove;	    //上一次下的棋子, 
		Boolean bDrawMark;	        //是否要绘制那个用来标记的spot. 
		Boolean m_fAnyKill;	        //是否有棋子被杀掉
		Spot [,] Grid;		        //格子的二维数组
		Pen penGrid, penStoneW, penStoneB,penMarkW, penMarkB;
		Brush brStar, brBoard, brBlack, brWhite, m_brMark;
	
        // ZZZZ ZZZZZZZZZ
        int nFFMove = 10;   //最大绘制十个子，棋谱中向前绘制十个子. 
        int nRewindMove = 10;  // 最大恢复十个子，棋谱中退回十个子; 

		GoTree	gameTree;

		/// <ZZZZZZZ>
		///    各种组件.
		/// </ZZZZZZZ>
		private System.ComponentModel.Container components;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button Rewind;
		private System.Windows.Forms.Button FForward;
		private System.Windows.Forms.Button Save;
		private System.Windows.Forms.Button Open;
		private System.Windows.Forms.Button Back;
		private System.Windows.Forms.Button Forward;

		public GoBoard(int nSize)
		{
			//
			// 初始化棋盘
			//
			InitializeComponent();

			//
			// ZZZZ: ZZZ ZZZ ZZZZZZZZZZZ ZZZZ ZZZZZ ZZZZZZZZZZZZZZZZZZZ ZZZZ
            //各种绘图
			//

			this.nSize = nSize;  //设定棋盘的范围.

			m_colorToPlay = StoneColor.black;

			Grid = new Spot[nSize,nSize];
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					Grid[i,j] = new Spot();
			penGrid = new Pen(Color.Brown, (float)0.5);
			penStoneW = new Pen(Color.WhiteSmoke, (float)1);
			penStoneB = new Pen(Color.Black,(float)1);
			penMarkW = new Pen(Color.Blue, (float) 1);
			penMarkB = new Pen(Color.Beige, (float) 1);

			brStar = new SolidBrush(Color.Black);
			brBoard = new SolidBrush(Color.Orange);
			brBlack = new SolidBrush(Color.Black);
			brWhite = new SolidBrush(Color.White);
			m_brMark = new SolidBrush(Color.Red);

			rGrid = new Rectangle(nEdgeLen, nEdgeLen,nTotalGridWidth, nTotalGridWidth);
			strLabels = new string [] {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t"};
			gameTree = new GoTree();
		}

		/// <ZZZZZZZ>
		///    ZZZZZZZZ ZZZZZZ ZZZ ZZZZZZZZ ZZZZZZZ - ZZ ZZZ ZZZZZZ
		///    ZZZ ZZZZZZZZ ZZ ZZZZ ZZZZZZ ZZZZ ZZZ ZZZZ ZZZZZZ.各种初始化
		/// </ZZZZZZZ>
		private void InitializeComponent()
		{
            this.Open = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.Rewind = new System.Windows.Forms.Button();
            this.Forward = new System.Windows.Forms.Button();
            this.Back = new System.Windows.Forms.Button();
            this.FForward = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Open
            // 
            this.Open.Location = new System.Drawing.Point(534, 95);
            this.Open.Name = "Open";
            this.Open.Size = new System.Drawing.Size(67, 25);
            this.Open.TabIndex = 2;
            this.Open.Text = "open";
            this.Open.Click += new System.EventHandler(this.Open_Click);
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(611, 95);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(67, 25);
            this.Save.TabIndex = 3;
            this.Save.Text = "save";
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // Rewind
            // 
            this.Rewind.Location = new System.Drawing.Point(611, 60);
            this.Rewind.Name = "Rewind";
            this.Rewind.Size = new System.Drawing.Size(67, 25);
            this.Rewind.TabIndex = 5;
            this.Rewind.Text = "<<";
            this.Rewind.Click += new System.EventHandler(this.Rewind_Click);
            // 
            // Forward
            // 
            this.Forward.Location = new System.Drawing.Point(534, 26);
            this.Forward.Name = "Forward";
            this.Forward.Size = new System.Drawing.Size(67, 25);
            this.Forward.TabIndex = 0;
            this.Forward.Text = ">";
            this.Forward.Click += new System.EventHandler(this.Forward_Click);
            // 
            // Back
            // 
            this.Back.Location = new System.Drawing.Point(611, 26);
            this.Back.Name = "Back";
            this.Back.Size = new System.Drawing.Size(67, 25);
            this.Back.TabIndex = 1;
            this.Back.Text = "<";
            this.Back.Click += new System.EventHandler(this.Back_Click);
            // 
            // FForward
            // 
            this.FForward.Location = new System.Drawing.Point(534, 60);
            this.FForward.Name = "FForward";
            this.FForward.Size = new System.Drawing.Size(67, 25);
            this.FForward.TabIndex = 4;
            this.FForward.Text = ">>";
            this.FForward.Click += new System.EventHandler(this.FForward_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(536, 138);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(144, 335);
            this.textBox1.TabIndex = 6;
            this.textBox1.Text = "please oepn a .sgf file to view, or just play on the board";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // GoBoard
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(689, 495);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.Rewind);
            this.Controls.Add(this.FForward);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.Open);
            this.Controls.Add(this.Back);
            this.Controls.Add(this.Forward);
            this.Name = "GoBoard";
            this.Text = "Go_WinForm";
            this.Load += new System.EventHandler(this.GoBoard_Load);
            this.Click += new System.EventHandler(this.GoBoard_Click);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintHandler);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpHandler);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		protected void textBox1_TextChanged (object sender, System.EventArgs e)
		{
			return;
		}

		private void PaintHandler(Object sender, PaintEventArgs e)
		{
			UpdateGoBoard(e);
		}

		protected void Save_Click (object sender, System.EventArgs e)
		{
			return;
		}

		protected void Open_Click (object sender, System.EventArgs e)
		{
			OpenFile();
			showGameInfo();
		}

		protected void Rewind_Click (object sender, System.EventArgs e)
		{
			gameTree.reset();
			resetBoard();
            showGameInfo();
		}

		protected void FForward_Click (object sender, System.EventArgs e)
		{
            if (gameTree != null)
            {
                int i = 0; 
                GoMove gm = null;
                for (gm = gameTree.doNext(); gm != null; gm = gameTree.doNext()) 
                {
                    playNext(ref gm);
                    if (i++ > nFFMove)
                        break; 
                }
            }
		}

		protected void Forward_Click (object sender, System.EventArgs e)
		{
			GoMove gm = gameTree.doNext();
			if (null != gm)
			{
				playNext(ref gm);
			}
		}

		private void showGameInfo()
		{
			//在textBox中显示游戏信息
			textBox1.Clear();
			textBox1.AppendText(gameTree.Info);
		}

		protected void Back_Click (object sender, System.EventArgs e)
		{
			GoMove gm = gameTree.doPrev();	//去上一步下的棋子，并将gametree向后回退一个棋子
            if (null != gm)
            {
                playPrev(gm);
            }
            else
            {
                resetBoard();
                showGameInfo(); 
            }
		}

		Boolean onBoard(int x, int y) 
		{
			return (x>=0 && x<nSize && y>=0 && y<nSize);
		}

		protected void GoBoard_Click (object sender, System.EventArgs e)
		{
			return;
		}

		private Point PointToGrid(int x, int y)
		{
			Point p= new Point(0,0);
			p.X = (x - rGrid.X + nUnitGridWidth/2) / nUnitGridWidth;
			p.Y = (y - rGrid.Y + nUnitGridWidth/2) / nUnitGridWidth;
			return p;
		}

		//ZZ ZZZ ZZ Z ZZZZZZZZ ZZZZZ (Z,Z) ZZ ZZZZZZ ZZZZZZ ZZZ ZZZZZZZZZ ZZ 
		//ZZ ZZZZZ ZZZZZ Z. (Z.Z. ZZZZZZ 1/3 ZZ ZZZZZZZZZZZZZZ
        //判定鼠标点击的点是否与格点距离足够近
		private Boolean closeEnough(Point p, int x, int y)
		{
			if (x < rGrid.X+nUnitGridWidth*p.X-nUnitGridWidth/3 ||
				x > rGrid.X+nUnitGridWidth*p.X+nUnitGridWidth/3 ||
				y < rGrid.Y+nUnitGridWidth*p.Y-nUnitGridWidth/3 ||
				y > rGrid.Y+nUnitGridWidth*p.Y+nUnitGridWidth/3)
			{
				return false;
			}
			else 
				return true;
		}
        /// <ZZZZZZZ>
        /// 
        /// </ZZZZZZZ>
        /// <ZZZZZ ZZZZ="ZZZZZZ"></ZZZZZ>
        /// <ZZZZZ ZZZZ="Z"></ZZZZZ>
		private void MouseUpHandler(Object sender,MouseEventArgs e)
		{
			Point p;
			GoMove	gmThisMove;

			p = PointToGrid(e.X,e.Y);
			if (!onBoard(p.X, p.Y) || !closeEnough(p,e.X, e.Y)|| Grid[p.X,p.Y].hasStone())
				return; //如果鼠标的位置不在面板上或不在格点上或该点已经有了一颗棋子，那么就返回不绘制点

			//ZZZZZ ZZZZ ZZ Z ZZZZZZ ZZZZ, ZZ ZZZZ ZZ ZZZ ZZZ ZZZZ ZZ ZZZ ZZZZ ZZZZ
			//将这个点的信息添加到gameTree中
            gmThisMove = new GoMove(p.X, p.Y, m_colorToPlay, 0);
			playNext(ref gmThisMove);
			gameTree.addMove(gmThisMove);
            /*GoMove gmNextMove = AI(gameTree,m_colorToPlay);
            playNext(gmNextMove);
            gameTree.addMove(gmThisMove);*/
		}

		public void playNext(ref GoMove gm) 
		{
			Point p = gm.Point;
			m_colorToPlay = gm.Color;	//将下一个的颜色设置为当前棋子的颜色

			//清空label和mark
			clearLabelsAndMarksOnBoard(); 
			
			if (m_gmLastMove != null)
				repaintOneSpotNow(m_gmLastMove.Point);

			bDrawMark = true;
			Grid[p.X,p.Y].setStone(gm.Color);
			m_gmLastMove = new GoMove(p.X, p.Y, gm.Color, nSeq++);
			//设置棋盘上的label和mark
			setLabelsOnBoard(gm);
			setMarksOnBoard(gm);
			
			doDeadGroup(nextTurn(m_colorToPlay));
			//判断是否有棋子被杀掉的情况. 
			if (m_fAnyKill)
				appendDeadGroup(ref gm, nextTurn(m_colorToPlay));
			else //如果第一种颜色没有被杀掉的，判断第二种颜色是否有被杀掉的情况
			{
				doDeadGroup(m_colorToPlay);
				if (m_fAnyKill)
					appendDeadGroup(ref gm, m_colorToPlay);
			}
			m_fAnyKill = false;
			
			optRepaint();

			//设置下一种颜色
			m_colorToPlay = nextTurn(m_colorToPlay);
			
			//ZZZZ ZZZ ZZZZZZZ, ZZ ZZZ
			textBox1.Clear();
			textBox1.AppendText(gm.Comment);
		}

		private void appendDeadGroup(ref GoMove gm, StoneColor c)
		{
			ArrayList a = new ArrayList();
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					if (Grid[i,j].isKilled())
					{
						Point pt = new Point(i,j);
						a.Add(pt);
						Grid[i,j].setNoKilled();
					}
			gm.DeadGroup = a;
			gm.DeadGroupColor = c;
		}

		public void resetBoard()
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					Grid[i,j].removeStone();
			m_gmLastMove = null;
			Invalidate(null);
		}

		/*
		 * ZZZZ ZZZ ZZZZ ZZ ZZZZ ZZZ ZZZZ ZZZZZZZZZ ZZ ZZZZ ZZZZZZ ZZZZ ZZZZ ZZ ZZZZZZ. 
		 * ZZZZ ZZ ZZ:
		 * 	1. ZZZZZZ ZZZ ZZZZZZZ ZZZZ ZZZZ ZZZ ZZZZZ
		 *  1.1 ZZZZ ZZZZZZ ZZZ "ZZZZZZZZ" ZZZZZZZZZZ
		 *	2. store the stones got killed by cur户rent move
		 *  3. ZZZZZZZZZZ ZZZ ZZZ "ZZZZZZZZ"
		 */
        //退一步棋的相关操作
		public void playPrev(GoMove gm)
		{
            Point P=gm.Point;
            
            Grid[P.X, P.Y].removeStone();
            m_gmLastMove = gameTree.peekPrev();
            if (m_gmLastMove != null)
            {
                Grid[m_gmLastMove.Point.X, m_gmLastMove.Point.Y].setUpdated();
            }
            if (gm.DeadGroup != null)
            {
                System.Collections.IEnumerator myEnumerator =gm.DeadGroup.GetEnumerator();
                while (myEnumerator.MoveNext())
                {
                    Point p;
                    p = (Point)myEnumerator.Current;
                    Grid[p.X, p.Y].setStone(m_colorToPlay);
                }
            }
            m_colorToPlay = nextTurn(m_colorToPlay);
            optRepaint();
            textBox1.Clear();
            textBox1.AppendText(gm.Comment);
            return;
        }


				
		
		Rectangle getUpdatedArea(int i, int j) 
		{
			int x = rGrid.X + i * nUnitGridWidth - nUnitGridWidth/2;
			int y = rGrid.Y + j * nUnitGridWidth - nUnitGridWidth/2;
			return new Rectangle(x,y, nUnitGridWidth, nUnitGridWidth);
		}

		/**
		 * ZZZZZZZZ ZZZ ZZZZZZZ ZZZZ, ZZZZ ZZZZZZZ ZZZ ZZZZZZZ ZZZZZ ZZ ZZZ ZZZZZ
		 */
        //重新绘制整个棋盘上的信息
		private void optRepaint()
		{
			Rectangle r = new Rectangle(0,0,0,0);
			Region	re;

			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					if (Grid[i,j].isUpdated()) 
					{
						r = getUpdatedArea(i,j);
						re = new Region(r);
						Invalidate(re);
					}
		}

		/*
		 * ZZZZZZZ ZZZZ ZZZ ZZZZ ZZ ZZZ ZZZZZ ZZZZZZZ 
		 */
        //将上一步棋上的hightlight spot拿掉
		void repaintOneSpotNow(Point p)
		{
			Grid[p.X, p.Y].setUpdated();
			bDrawMark = false;
			Rectangle r = getUpdatedArea(p.X, p.Y);
			Invalidate( new Region(r));
			Grid[p.X, p.Y].resetUpdated();
			bDrawMark = true;
		}

		//ZZZZ ZZ ZZZZZZZZ ZZ ZZZZZZZ ZZZ ZZZ ZZZZ ZZZZ ZZZZ.  
		void recordMove(Point p, StoneColor colorToPlay)
            //没有被使用过。。。
		{
			Grid[p.X,p.Y].setStone(colorToPlay);
			// ZZZZZZ ZZZZ ZZZZ.
			m_gmLastMove = new GoMove(p.X, p.Y, colorToPlay, nSeq++);
		}

		StoneColor nextTurn(StoneColor c) 
		{
			if (c == StoneColor.black)
				return StoneColor.white;
			else 
				return StoneColor.black;
		}

		/**
		 *	bury the dead stones in a group (same color). 
		 *	if a stone in one group is dead, the whole group is dead.
		*/
		void buryTheDead(int i, int j, StoneColor c) 
		{
			if (onBoard(i,j) && Grid[i,j].hasStone() && 
				Grid[i,j].color() == c) 
			{
				Grid[i,j].die();
				buryTheDead(i-1, j, c);
				buryTheDead(i+1, j, c);
				buryTheDead(i, j-1, c);
				buryTheDead(i, j+1, c);
			}
		}

		void cleanScanStatus()
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					Grid[i,j].clearScanned();
		}

		/**
		 * ZZZZZZ ZZZ ZZZZ ZZZZZ ZZZ ZZZZZ ZZZZ ZZZZ ZZZ ZZZZZ.
		 */
        //处理被杀掉的棋子组的部分
		void doDeadGroup(StoneColor c) 
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					if (Grid[i,j].hasStone() &&
						Grid[i,j].color() == c) 
					{
						if (calcLiberty(i,j,c) == 0)
						{
							buryTheDead(i,j,c);
							m_fAnyKill = true;
						}
						cleanScanStatus();
					}
		}


		/**
		 * ZZZZZZZZZ ZZZ ZZZZZZZ ZZ ZZZ ZZZZZ, ZZZZZZZZ ZZZZ ZZZ ZZZZZ.
		 */
        //计算一组棋子的气
		int calcLiberty(int x, int y, StoneColor c) 
		{
			int lib = 0; // ZZZZZZZ	
			
			if (!onBoard(x,y))
				return 0;
			if (Grid[x,y].isScanned())
				return 0;

			if (Grid[x,y].hasStone()) 
			{
				if (Grid[x,y].color() == c) 
				{
					//ZZZ ZZZZZZZZZZ ZZZ ZZZZZZZ ZZZZZ.
					Grid[x,y].setScanned();
					lib += calcLiberty(x-1, y, c);
					lib += calcLiberty(x+1, y, c);
					lib += calcLiberty(x, y-1, c);
					lib += calcLiberty(x, y+1, c);
				} 
				else 
					return 0;
			} 
			else 
			{// ZZZZ ZZ ZZZZZ ZZZ ZZZZZZZZZ. 
				lib ++;
				Grid[x,y].setScanned();
			}

			return lib;
		}


		/**
		 * ZZZZ ZZZ ZZZZ ZZZZ
		 */
        //标记上一个下的棋子
		void markLastMove(Graphics g) 
		{
			Brush brMark;
			if (m_gmLastMove.Color == StoneColor.white)
				brMark = brBlack;
			else 
				brMark = brWhite;
			Point p = m_gmLastMove.Point;
			g.FillRectangle( brMark,
				rGrid.X + (p.X) * nUnitGridWidth - (nUnitGridWidth-1)/8, 
				rGrid.Y + (p.Y) * nUnitGridWidth - (nUnitGridWidth-1)/8,
				3, 3);
		}

		private void clearLabelsAndMarksOnBoard()
		{
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
				{
					if (Grid[i,j].hasLabel())
						Grid[i,j].resetLabel();
					if (Grid[i,j].hasMark())
						Grid[i,j].resetMark();
				}

		}

		private void setLabelsOnBoard(GoMove gm)
		{
			short	nLabel = 0;
			Point p;
			if (null != gm.Labels)
			{
				int i = gm.Labels.Count;
				i = gm.Labels.Capacity;

				System.Collections.IEnumerator myEnumerator = gm.Labels.GetEnumerator();
				while (myEnumerator.MoveNext())
				{
					p = (Point)myEnumerator.Current;
					Grid[p.X,p.Y].setLabel(++nLabel);
				}
			}
		}

		private void setMarksOnBoard(GoMove gm)
		{
			Point p;
			if (null != gm.Labels)
			{
				System.Collections.IEnumerator myEnumerator = gm.Marks.GetEnumerator();
				while ( myEnumerator.MoveNext() )
				{
					p = (Point)myEnumerator.Current;
					Grid[p.X,p.Y].setMark();
				}
			}
		}

		private Point SwapXY(Point p)
		{
			Point pNew = new Point(p.Y,p.X);
			return pNew;
		}

		private void DrawBoard(Graphics g)
		{
			//ZZZZZ ZZZ ZZZ ZZZZ ZZZ ZZZZZZZZZZZ
            //绘制棋盘
			string[] strV= {"1","2","3","4","5","6","7","8","9","10","11","12","13","14","15","16","17","18","19"};
			string [] strH= {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T"};

			Point p1 = new Point(nEdgeLen,nEdgeLen);
			Point p2 = new Point(nTotalGridWidth+nEdgeLen,nEdgeLen);
			g.FillRectangle(brBoard,nBoardOffset,nBoardOffset,nTotalGridWidth+nBoardOffset,nTotalGridWidth+nBoardOffset);
			for (int i=0;i<nSize; i++)
			{
				g.DrawString(strV[i],this.Font, brBlack, 0, nCoodStart+ nBoardOffset + nUnitGridWidth*i);
				g.DrawString(strH[i],this.Font, brBlack, nBoardOffset + nCoodStart + nUnitGridWidth*i, 0);
				g.DrawLine(penGrid, p1, p2);
				g.DrawLine(penGrid, SwapXY(p1), SwapXY(p2));

				p1.Y += nUnitGridWidth;
				p2.Y += nUnitGridWidth;
			}
			//ZZZZ ZZZ ZZZZ ZZ ZZZ ZZZZZ
			Pen penHi = new Pen(Color.WhiteSmoke, (float)0.5);
			Pen penLow = new Pen(Color.Gray, (float)0.5);

			g.DrawLine(penHi, nBoardOffset, nBoardOffset, nTotalGridWidth+2*nBoardOffset, nBoardOffset);
			g.DrawLine(penHi, nBoardOffset, nBoardOffset, nBoardOffset, nTotalGridWidth+2*nBoardOffset);
			g.DrawLine(penLow, nTotalGridWidth+2*nBoardOffset,nTotalGridWidth+2*nBoardOffset, nBoardOffset+1, nTotalGridWidth+2*nBoardOffset);
			g.DrawLine(penLow, nTotalGridWidth+2*nBoardOffset,nTotalGridWidth+2*nBoardOffset, nTotalGridWidth+2*nBoardOffset, nBoardOffset+1);
		}

		void UpdateGoBoard(PaintEventArgs e)
		{
			DrawBoard(e.Graphics);
			
			//ZZZZ ZZZZ-ZZZZZ. 
			drawStars(e.Graphics);

			//绘制棋盘上的对象
			drawEverySpot(e.Graphics);
		}

		//ZZZZ ZZZ ZZZZ ZZ ZZZZZZZZ ZZZZZZZZ
		void drawStar(Graphics g, int row, int col) 
		{
			g.FillRectangle(brStar,
				rGrid.X + (row-1) * nUnitGridWidth - 1, 
				rGrid.Y + (col-1) * nUnitGridWidth - 1, 
				3, 
				3);
		}

		//ZZZZ 9 ZZZZZ ZZZ ZZZZZZZ ZZZZ 19Z19. 
		void  drawStars(Graphics g)
		{
			drawStar(g, 4, 4);
			drawStar(g, 4, 10);
			drawStar(g, 4, 16);
			drawStar(g, 10, 4);
			drawStar(g, 10, 10);
			drawStar(g, 10, 16);
			drawStar(g, 16, 4);
			drawStar(g, 16, 10);
			drawStar(g, 16, 16);
		}

		/**
		 * ZZZZ Z ZZZZZ (ZZZZZ/ZZZZZ) ZZ ZZZZZZZZ ZZZZZZZZ.
		 */
        //绘制棋子
		void drawStone(Graphics g, int row, int col, StoneColor c) 
		{
			Brush br;
			if (c == StoneColor.white)
				br = brWhite;
			else 
				br = brBlack;
			
			Rectangle r = new Rectangle(rGrid.X+ (row) * nUnitGridWidth - (nUnitGridWidth-1)/2, 
				rGrid.Y + (col) * nUnitGridWidth - (nUnitGridWidth-1)/2,
				nUnitGridWidth-1,
				nUnitGridWidth-1);

			g.FillEllipse(br, r);
		}

		void drawLabel(Graphics g, int x, int y, short nLabel) 
		{
			if (nLabel ==0)
				return;
			nLabel --;
			nLabel %= 18;			//ZZZZZZZZ ZZZZZ.

			//ZZZZZ ZZZ ZZ
			Rectangle r = new Rectangle(rGrid.X+ x * nUnitGridWidth - (nUnitGridWidth-1)/2, 
				rGrid.Y + y * nUnitGridWidth - (nUnitGridWidth-1)/2,
				nUnitGridWidth-1,
				nUnitGridWidth-1);

			g.FillEllipse(brBoard, r);

			g.DrawString(strLabels[nLabel],	//ZZZZZZ ZZZZZZ ZZZZ 1, ZZZ ZZZ ZZZZZZ ZZZZZZ ZZZZ 0.
				this.Font, 
				brBlack, 
				rGrid.X+ (x) * nUnitGridWidth - (nUnitGridWidth-1)/4, 
				rGrid.Y + (y) * nUnitGridWidth - (nUnitGridWidth-1)/2);
		}

		void drawMark(Graphics g, int x, int y)
		{
			g.FillRectangle( m_brMark,
				rGrid.X + x* nUnitGridWidth - (nUnitGridWidth-1)/8, 
				rGrid.Y + y * nUnitGridWidth - (nUnitGridWidth-1)/8,
				5, 5);
		}

		void drawEverySpot(Graphics g) 
		{
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
				{
					if (Grid[i,j].hasStone())
						drawStone(g, i, j, Grid[i,j].color());
					if (Grid[i,j].hasLabel())
						drawLabel(g, i, j, Grid[i,j].getLabel());
					if (Grid[i,j].hasMark())
						drawMark(g, i, j);
				}
			//如果可以绘制mark或者上一个棋子非空. 
			if (bDrawMark && m_gmLastMove != null)
				markLastMove(g);
		}

		//打开棋谱的函数
		private void OpenFile()
		{
			OpenFileDialog openDlg = new OpenFileDialog();
			openDlg.Filter  = "sgf files (*.sgf)|*.sgf|All Files (*.*)|*.*";
			openDlg.FileName = "" ;
			openDlg.DefaultExt = ".sgf";
			openDlg.CheckFileExists = true;
			openDlg.CheckPathExists = true;
			
			DialogResult res = openDlg.ShowDialog ();
			
			if(res == DialogResult.OK)
			{
				if( !(openDlg.FileName).EndsWith(".sgf") && !(openDlg.FileName).EndsWith(".SGF")) 
					MessageBox.Show("Unexpected file format","Super Go Format",MessageBoxButtons.OK);
				else
				{
					FileStream f = new FileStream(openDlg.FileName, FileMode.Open); 
					StreamReader r = new StreamReader(f);
					string s = r.ReadToEnd();
					gameTree = new GoTree(s);
					gameTree.reset();
                    resetBoard();
					r.Close(); 
					f.Close();
				}
			}		
		}

        private void GoBoard_Load(object sender, EventArgs e)
        {

        }	
	}

	public class GoTest
	{
		/// <ZZZZZZZ>
		/// ZZZ ZZZZ ZZZZZ ZZZZZ ZZZ ZZZ ZZZZZZZZZZZ.主函数
		/// </ZZZZZZZ>
        [STAThread]
		public static void Main(string[] args) 
		{
			Application.Run(new GoBoard(19));
		}
	}

	
	//ZZZZ ZZZZZZZZZZZZ ZZ ZZZ ZZZZ ZZ ZZZ ZZZZZ棋盘上的每个格点类
	public class Spot 
	{
		private Boolean bEmpty;
		private Boolean bKilled;
		private Stone s;
		private short	m_nLabel;
		private Boolean m_bMark;
		private Boolean bScanned;
		private Boolean bUpdated; //ZZ ZZZ ZZZZ ZZ ZZZZZZZ. (ZZZ ZZZZ/ZZZZ ZZZZZ/ZZZZZZ ZZZZZ)是否需要被更新
		/**
		 * ZZZZZZZZZZZ.初始化每个spot类
		 */
		public Spot() 
		{
			bEmpty = true;
			bScanned = false;
			bUpdated = false;
			bKilled = false;
		}
		
		public Boolean hasStone() { return !bEmpty;	}
		public Boolean isEmpty() {	return bEmpty;	}
		public Stone thisStone() {	return s;}
		public StoneColor color() {	return s.color;}

		public Boolean hasLabel() {return m_nLabel>0;}
		public Boolean hasMark() {return m_bMark;}
		public void setLabel(short l) {m_nLabel = l; bUpdated = true; }
		public void setMark() {m_bMark = true; bUpdated = true;}
		public void resetLabel() {m_nLabel = 0; bUpdated = true;}
		public void resetMark() {m_bMark = false; bUpdated = true;}
		public short	getLabel() {return m_nLabel;}

		public Boolean isScanned() { return bScanned;}
		public void setScanned() {	bScanned = true;}
		public void clearScanned() { bScanned = false; }

		public void setStone(StoneColor c) 
		{
			if (bEmpty) 
			{
				bEmpty = false;
				s.color = c;
				bUpdated = true;
			} // ZZZZ ZZZZZZ ZZZZZZZZ.如果该格点为空，那么设置该格点的颜色
		}

		/*
		 * ZZZZZZ Z ZZZZZ ZZZZ ZZZ ZZZZZZZZ删除这个格点上的棋子
		*/
		public void removeStone()
		{	//ZZZZZZ ZZZZZZ !ZZZZZZ
			bEmpty = true;
			bUpdated = true;
		}
				
		//ZZ ZZZZ ZZZZ ZZZZZ ZZZZZZZZZZZZZ ZZZZZZ ZZZ ZZZZ ZZZZZ Z.杀死这个格点，即将该格点上面的棋子清空
		public void die() 
		{
			bKilled = true;
			bEmpty = true;
			bUpdated = true;
		} 

		public Boolean isKilled() { return bKilled;}
		public void setNoKilled() { bKilled = false;}

		public void resetUpdated() { bUpdated = false; bKilled = false;}

		//ZZ ZZZ ZZZZZZZ ZZZZZZZZZ ZZZZ ZZZZZZ (ZZZZZZZ)? 判断是否需要被更新
		public Boolean isUpdated() 
		{ 
			if (bUpdated)
			{	//ZZZZ ZZ ZZZZ ZZZ ZZZZZZ ZZZZZZZZZ ZZZ ZZZ ZZZZ ZZZZZZ
				bUpdated = false;
				return true;
			} 
			else 
				return false;
		}

		// ZZZZZ Z ZZZZ ZZ ZZ ZZZZZZZZZ, ZZZZ ZZ ZZZZZZZZ ZZZ ZZZZZ ZZZZ.将该格点设置为需要被更新
		public void setUpdated() { bUpdated = true; }
	}

	/**
	 * Z ZZZZ ZZ Z ZZ ZZZZ.棋子的移动类
	 */
	public class GoMove 
	{
		StoneColor m_c;	//ZZZZZ/ZZZZZ
		Point m_pos;		//ZZZZZZZZZZZ ZZ ZZZ ZZZZ.
		int m_n;			//ZZZZZZZZ ZZ ZZZ ZZZZZZZZZ.
		String m_comment;	//ZZZZZZZZ.
		MoveResult m_mr;	//ZZZZ'Z ZZZZZZ. 

		ArrayList		m_alLabel; //ZZZ ZZZZZ ZZ ZZZZ ZZZZ. 
		ArrayList		m_alMark; //ZZZZ

		//ZZZ ZZZZZ ZZ ZZZZ ZZZZZZ ZZZZZZ ZZ ZZZZ ZZZZ
		//ZZ ZZZZZ ZZ ZZZ ZZZZ ZZZZZ (ZZ ZZZ ZZZZZZ ZZZZ ZZZZ ZZ ZZZZZZZ). 
		ArrayList		m_alDead;
		StoneColor	m_cDead;
		/**
		 * ZZZZZZZZZZZ.
		 */
		public GoMove(int x, int y, StoneColor sc, int seq) 
		{
			m_pos = new Point(x,y);
			m_c = sc;
			m_n = seq;
			m_mr = new MoveResult();
			m_alLabel = new ArrayList();
			m_alMark = new ArrayList();
		}

		public GoMove(String str, StoneColor c) 
		{
			char cx = str[0];
			char cy = str[1];
			m_pos = new Point(0,0);
			//ZZZ Z# ZZ ZZZ ZZZZZZZZZ - 
			m_pos.X = (int) ( (int)cx - (int)(char)'a');
			m_pos.Y = (int) ( (int)cy - (int)(char)'a');
			this.m_c = c;
			m_alLabel = new ArrayList();
			m_alMark = new ArrayList();
		}


		private Point	StrToPoint(String str)
		{
			Point p = new Point(0,0);
			char cx = str[0];
			char cy = str[1];
			//ZZZ Z# ZZ ZZZ ZZZZZZZZZ -将文件中的英文字母转化为坐标值 
			p.X = (int) ( (int)cx - (int)(char)'a');
			p.Y = (int) ( (int)cy - (int)(char)'a');
			return p;
		}


        public StoneColor Color
        { 
            get { return m_c; } 
        }

        public String Comment 
        {
            get
            {
                if (m_comment == null)
                    return string.Empty;
                else
                    return m_comment;
            }
            set
            {
                m_comment = value; 
            }
        }

		public int Seq
        {
            get { return m_n; }
            set {	m_n = value;}
        }

        public Point Point
        {
           get  { return m_pos; }
        }

        public MoveResult Result
        {
            get { return m_mr; }
            set { m_mr = value; }
        }
		
		public ArrayList DeadGroup
        {
            get { return m_alDead;}
            set {m_alDead = value;}
        }

        public StoneColor DeadGroupColor
        {
            get { return m_cDead; }
            set { m_cDead = value; }
        }
		
		public void addLabel(String str) {m_alLabel.Add(StrToPoint(str));}
		
		public void addMark(String str) {	m_alMark.Add(StrToPoint(str));}

        public ArrayList Labels
        {
            get { return m_alLabel; }
        }

        public ArrayList Marks
        {
            get { return m_alMark; }
        }
	}
	

	/**
	 * ZZZZZZZZZZ - ZZZ ZZZZZZ ZZ ZZZ 4 ZZZZZZZZZZZ ZZZZZZZZZ ZZZZ Z ZZZZ ZZ ZZZZZZ.
	 * 
	 */
	public class MoveResult 
	{
		public StoneColor color; 
		// 4 ZZZZZZZZ ZZZZZZ ZZZZZ ZZ ZZZZZZZZ. 
		public Boolean bUpKilled;
		public Boolean bDownKilled;
		public Boolean bLeftKilled;
		public Boolean bRightKilled;
		public Boolean bSuicide;	//ZZ ZZZ ZZZZ Z ZZZZZZZ?
		public MoveResult() 
		{
			bUpKilled = false;
			bDownKilled = false;
			bLeftKilled = false;
			bRightKilled = false;
			bSuicide = false;
		}
	}

	/**
	 * ZZZZZ. 
	 * ZZZZZ ZZZ ZZZ ZZZZZZZZ ZZZZZ, ZZZZZ ZZZ ZZZZZ. 
	 */
	public struct Stone 
	{
		public StoneColor color; 
	}

	/**
	 * Z ZZZZZZZZZ ZZ Z ZZ ZZZZ.
	 * ZZZZZZZZZZ: ZZZ ZZZZ ZZZZ ZZZZZZ ZZZ ZZ 0.棋子的属性类 
	 */
	public class GoVariation 
	{
		int m_id;			//ZZZZZZZZZ ZZ. 
		string m_name;	//ZZZZZZZZZ ZZZZ. (ZZZZ.5, ZZZ.9, "ZZZZZ ZZZZZZ", ZZZ).
		//ZZZZZZZZZZZZZ ZZZ;	//ZZZZZZZZZ ZZZZZZZZ ZZZZZ.	
		ArrayList m_moves; 
		int m_seq;			//ZZZZZZ ZZZ ZZZ ZZ ZZZZ ZZZZ. 
		int m_total;

		//ZZZZZZZZZZZ. 
		public GoVariation(int id)
		{
			m_id = id;
			m_moves = new ArrayList(10);
			m_seq = 0;
			m_total = 0;
		}

		public void addAMove(GoMove gm) 
		{
			gm.Seq = m_total ++;
            m_moves.Insert(m_seq,gm);
			m_seq++;
			//m_moves.Add(gm);
            
		}

		public void updateResult(GoMove gm) 
		{
		}

		public GoMove doNext()
		{
			if (m_seq < m_total) 
			{
				return (GoMove)m_moves[m_seq++];
			} 
			else 
				return null;
		}

		public GoMove doPrev()
		{
			if (m_seq > 0)
				return (GoMove)(m_moves[--m_seq]);
			else 
				return null;
		}

		/*
		 *  ZZZZ ZZZZZZ ZZZ ZZZZZZZZ ZZZZ, ZZ ZZZZZZ ZZZZZZ ZZ ZZZ ZZZZZZZZ.找到上一步的棋子
		 */
		public GoMove peekPrev()
		{
			if (m_seq > 0)
				return (GoMove)(m_moves[m_seq-1]);
			else 
				return null;
		}

		public void reset() {m_seq = 0;}
	}


	/**
	* ZZ: ZZZ ZZ ZZ Z ZZZZZZZZZ ZZZZZ ZZZZ ZZZ ZZZZZZZ ZZZZ. 
	* ZZZ: ZZZ ZZZZ ZZ Z ZZZZZZZZZ ZZZZZ ZZZZZZZ ZZZZ. 
	*/
	struct VarStartPoint
	{
		int m_id; 
		int m_seq;
	}

	struct GameInfo 
	{
		public string gameName;
		public string playerBlack;
		public string playerWhite;
		public string rankBlack;
		public string rankWhite;
		public string result;
		public string date;
		public string km;
		public string size;
		public string comment;
        public string handicap;
        public string gameEvent;
        public string location;
        public string time;             // ZZZZZ ZZZZZ ZZZZ ZZZZZ ZZ ZZZ ZZZZ. 
        public string unknown_ff;   //ZZZZZZZ ZZZZZZZZZZ. 
        public string unknown_gm;
        public string unknown_vw; 
	}

	public class KeyValuePair 
	{
		public string k; public ArrayList alV;

		private string	removeBackSlash(string strIn)
		{
			string strOut; 
			int		iSlash;

			strOut = string.Copy(strIn);
			if (strOut.Length < 2)
				return strOut;
			for (iSlash = strOut.Length-2; iSlash>=0; iSlash--)
			{
				if (strOut[iSlash] == '\\')		// && ZZZZZZ[ZZZZZZ+1] == ']')
				{
					strOut = strOut.Remove(iSlash,1);
					if (iSlash>0)
						iSlash --;	//ZZZZ ZZ ZZZZ ZZZZZZZZZ ZZZZZ ZZ ZZZZZZ ZZZ ZZZZZ
				}
			}
			return strOut;
		}

		public KeyValuePair(string k, string v)
		{
			this.k = string.Copy(k);
			string strOneVal;
			int		iBegin, iEnd;
		
			//ZZZZ ZZ ZZZZZ ZZZZ ZZZZZ
			alV = new ArrayList(1);

			//ZZZZZZZ ZZZZ ZZZ ZZZZZZZ Z[ZZZZZZZ]
			if (k.Equals("C"))
			{
				strOneVal = removeBackSlash(string.Copy(v));
				//ZZZ ZZZ ZZ '\'
				alV.Add(strOneVal);
				return;
			}

			iBegin = v.IndexOf("[");
			if (iBegin == -1)	//ZZZZZZ ZZZZZ
			{
				alV.Add(v);
				return; 
			}
			
			iBegin = 0;
			while (iBegin < v.Length && iBegin>=0)
			{
				iEnd = v.IndexOf("]", iBegin);
				if (iEnd > 0)
					strOneVal = v.Substring(iBegin, iEnd-iBegin);
				else 
					strOneVal = v.Substring(iBegin);	//ZZZ ZZZZ ZZZZZ
				alV.Add(strOneVal);
				iBegin = v.IndexOf("[", iBegin+1);
				if (iBegin > 0)
					iBegin ++;	//ZZZ ZZ ZZZ ZZZZZ ZZ ZZZZ ZZZZZ
			}
		}
	}

	/**
	 * ZZZ ZZZZZZ ZZ Z ZZ ZZZZ.
	 * ZZZZ ZZ ZZZ ZZZZ ZZ ZZZ ZZZZ ZZZZ, ZZ ZZZZZ Z ZZZZZ ZZ ZZZZZZZZZZ. 
	 */

	public class GoTree 
	{
		GameInfo _gi;		//ZZZZZ ZZZ ZZZZ'Z ZZZZZZZ ZZZZ.
		ArrayList _vars;		//ZZZZZZZZZZ. 
		int _currVarId;		//ZZ ZZ ZZZZZZZ ZZZZZZZZZ.
		int _currVarNum;
		GoVariation _currVar;		//ZZZZZZZ ZZZZZZZZZZZ.
		string	_stGameComment;

		// ZZZZZZZZZZZ - ZZZZZZ ZZZ ZZZZZZ ZZZZZ ZZ ZZZZZ ZZZZZZ根据棋谱建立棋子移动树
		public GoTree(string s)
		{
			_vars = new ArrayList(10);
			_currVarNum = 0;
			_currVarId = 0; 
			_currVar = new GoVariation(_currVarId);
			_vars.Add(_currVar);
			parseFile(s);
		}

		//	ZZZZZZZZZZZ - ZZZZZZ ZZ ZZZZZ ZZZZZZ根据用户的操作建立棋子移动树
		public GoTree()
		{
			_vars = new ArrayList(10);
			_currVarNum = 0;
			_currVarId = 0; 
			_currVar = new GoVariation(_currVarId);
			_vars.Add(_currVar);
		}

		public	string Info
		{
            get
            {
                return _gi.comment == null? string.Empty : _gi.comment;
            }
		}

		public void addMove(GoMove gm) 
		{
			_currVar.addAMove(gm);
		}

		/**
		 * ZZZZZ ZZZ ZZZZZ ZZZZ ZZ Z ZZZZZZ. 转换文件中的字符串
		 */
		Boolean parseFile(String goStr) 
		{
			int iBeg, iEnd=0; 
			while (iEnd < goStr.Length) 
			{
				if (iEnd > 0)
					iBeg = iEnd;
				else 
					iBeg = goStr.IndexOf(";", iEnd);
				iEnd = goStr.IndexOf(";", iBeg+1);
				if (iEnd < 0) //ZZ ZZZZ ";"
					iEnd = goStr.LastIndexOf(")", goStr.Length);		//ZZZ ZZZZ ZZZZZZZ ZZZZZZ ZZ ZZZZZZZZ ZZ ")"
				if (iBeg >= 0 && iEnd > iBeg) 
				{
					string section = goStr.Substring(iBeg+1, iEnd-iBeg-1);
					parseASection(section);
				} 
				else 
					break;
			}
			return true;
		}

        /// <ZZZZZZZ>
        /// ZZZZ ZZZ ZZZZZ ZZ ZZZ ZZ ZZZZZ ZZZZZZ,
        /// ZZZZZZZ ZZ'Z ZZZ "]" ZZZZ,  
        /// ZZ ZZZ ZZ ZZZZ "\]",  ZZ ZZZZ ZZZZZZZZ, ZZZ ZZZZZZ ZZZ ZZZ ZZZZ "]", ZZ ZZZ ZZ ZZZZZZ. 
        /// </ZZZZZZZ>
        /// <ZZZZZ ZZZZ="ZZZ"></ZZZZZ>
        /// <ZZZZZZZ></ZZZZZZZ>
        int findEndofValueStr(String sec)
        {
            int i = 0;
            //ZZ ZZZZZZ ZZ'ZZ ZZZZZZZZ ZZZZ ZZZZZZZ ZZZ ZZZZZ ZZZZZZ.
            while (i >= 0)
            {
                i = sec.IndexOf(']', i+1);
                if (i > 0 && sec[i - 1] != '\\')
                    return i;    //ZZZZZZ ZZZ ZZZZZ ZZ "]". 
            }

            //ZZ ZZ ZZZZ ZZ ZZZ ']', ZZZ'Z ZZZZ ZZZ ZZZ ZZZ ZZ ZZZZZZ. 
            return sec.Length - 1;		//ZZZZ ZZ ZZZ ZZZZZ ZZ ZZZ ZZZZ ZZZZ ZZ ZZZ ZZZZZZ
        }
        
        int findEndofValueStrOld(String sec)
		{
			int i = 0;
            //ZZ ZZZZZZ ZZ'ZZ ZZZZZZZZ ZZZZ ZZZZZZZ ZZZ ZZZZZ ZZZZZZ. 
			bool fOutside = false;
			
			for (i=0; i<sec.Length;i++)
			{
				if (sec[i] == ']')
				{
					if (i>1 && sec[i-1] != '\\') //ZZ ZZ
						fOutside = true;
				}
				else if (char.IsLetter(sec[i]) && fOutside && i>0)
					return i-1;
				else if (fOutside && sec[i] == '[')
					fOutside = false;
			}
			return sec.Length-1;		//ZZZZ ZZ ZZZ ZZZZZ ZZ ZZZ ZZZZ ZZZZ ZZ ZZZ ZZZZZZ
		}

        private string purgeCRLFSuffix(string inStr)
        {
            int iLast = inStr.Length - 1; //ZZZZZ ZZ ZZZ ZZ ZZZZZZ. 

            if (iLast <= 0)
                return inStr; 

            while ((inStr[iLast] == '\r' || inStr[iLast] == '\n' || inStr[iLast] == ' '))
            {
                iLast--; 
            }
            if ((iLast+1) != inStr.Length)
                return inStr.Substring(0, iLast+1);  //ZZZ 2ZZ ZZZZZZZZZ ZZ ZZZ ZZZZZZ
            else
                return inStr; 
        }
 

		/**
		 * ZZZZZ Z ZZZZZZZ ZZ ZZZ ZZZZZZ ZZZZZZ. 
		 * Z ZZZZZZZ ZZZ ZZZ ZZZZZZ "ZZ {ZZ}"
		 * Z ZZ (ZZZ ZZZZZ ZZZZ) ZZZ ZZZ ZZZZZZ "ZZZ ZZZZZ {ZZZZZ}"
		 * ZZZZ: Z ZZZ ZZZ ZZZZZZZZZ ZZZZ ZZZZZZZZ ZZZZZZ, Z.Z. ZZZZZZ, ZZZZZ:  Z[ZZ][ZZ]. 
		 * Z ZZZ ZZ ZZZZZZ 
		 * Z ZZZZZ ZZ Z ZZZZZZ ZZZZZZZZ ZZ [ ZZZ ].
		 * ZZZZ: ZZZZZZZZ ( Z[ZZZZZZZZ]) ZZZ ZZZZ ZZZ ']' ZZZZZZZZZ ZZZZZZ ZZZ ZZZZZZZZ, ZZZZZ ZZ ZZZZZZZ ZZ "\]"
		 * Z.Z.  Z[ZZZZZ ZZZZZ ZZ [4,Z\] ZZ ZZZZZ ZZZZZZ]
         * 
		 */
		Boolean parseASection(String sec) 
		{
			int iKey = 0;
			int iValue = 0;
			int iLastValue = 0;
			KeyValuePair kv;
			ArrayList Section = new ArrayList(10);
			
			try 
			{
				iKey = sec.IndexOf("[");
				if (iKey < 0)
				{
					return false;
				}
                sec = purgeCRLFSuffix(sec);
 
				iValue = findEndofValueStr(sec); //ZZZ.ZZZZZZZ("]", ZZZZ);
				iLastValue = sec.LastIndexOf("]");
				if (iValue <= 0 || iLastValue <= 1)
				{
					return false;
				}
				sec = sec.Substring(0,iLastValue+1);
				while (iKey > 0 && iValue > iKey)//ZZ ZZZZ ZZZZZ ZZ ZZZZZZZ
				{
					string key = sec.Substring(0,iKey);
					int iNonLetter = 0;
					while (!char.IsLetter(key[iNonLetter]) && iNonLetter < key.Length)
						iNonLetter ++;
					key = key.Substring(iNonLetter);
					//ZZZZ ZZ ZZZZ ZZZ ZZZZ ZZZZZZ ZZZZZZZ ZZ Z [] ZZZZ
					//ZZZZZZ = ZZZZZZZZZZZZZZZZZ(ZZZ);
					string strValue = sec.Substring(iKey+1, iValue-iKey-1);
					//ZZZ ZZ ZZZZ Z ZZZ/ZZZZZ ZZZZ
					kv = new KeyValuePair(key, strValue);
					Section.Add(kv);
					if (iValue >= sec.Length)
						break;
					sec = sec.Substring(iValue+1);
					iKey = sec.IndexOf("[");
					if (iKey > 0)
					{
						iValue = findEndofValueStr(sec); //ZZZ.ZZZZZZZ("]",ZZZZ);
					}
				}
			}
			catch
			{
                return false;
            }

			processASection(Section);
			return true;
		}


        /** 
         * ZZZZZZZ Z ZZZ ZZZ ZZZ ZZZZZZZZZZZZZ ZZZZZ
         * ZZ ZZZZZ ZZ Z ZZZZ, ZZ ZZZZZZ ZZZZZZZZZZZ.
         * ZZZZZZZZZZ, ZZZ ZZZZZZZ ZZZ ZZZZ ZZZZ ZZZ ZZZZ ZZ ZZZZ. 
         * 
         * ZZZZ: ZZ/ZZ ZZZ ZZZZZZZZZ ZZZ ZZZZ ZZZZZZZZ ZZZZZ ZZZZZZ, ZZZ Z ZZZZZ'Z ZZZZ ZZZ ZZZZZZZ ZZZ 
         */
        Boolean processASection(ArrayList arrKV) 
		{
			Boolean fMultipleMoves = false;   //ZZZZZZZ ZZZZ ZZZZZZZ ZZZ ZZZZZZZZ ZZZZZ. 
			GoMove gm = null; 
            
			string key, strValue;

			for (int i = 0;i<arrKV.Count;i++)
			{
				key = ((KeyValuePair)(arrKV[i])).k;
				for (int j=0; j<((KeyValuePair)(arrKV[i])).alV.Count; j++)
				{
					strValue = (string)(((KeyValuePair)(arrKV[i])).alV[j]);

                    if (key.Equals("B"))   //ZZZZZ ZZZZZ
                    {
                        Debug.Assert(gm == null);
                        gm = new GoMove(strValue, StoneColor.black);
                    }
                    else if (key.Equals("W"))  //ZZZZZ ZZZZZ
                    {
                        Debug.Assert(gm == null);
                        gm = new GoMove(strValue, StoneColor.white);
                    }
                    else if (key.Equals("C"))  //ZZZZZZZ
                    {
                        //ZZZZZ.ZZZZZZ(Z>0);
                        if (gm != null)
                            gm.Comment = strValue;
                        else	//ZZZ ZZ ZZ ZZZ ZZZZ ZZZZZZZ 
                            _gi.comment += strValue;
                    }
                    else if (key.Equals("L"))  //ZZZZZ
                    {
                        if (gm != null)
                            gm.addLabel(strValue);
                        else	//ZZZ ZZ ZZ ZZZ ZZZZ ZZZZZZZ 
                            _stGameComment += strValue;
                    }

                    else if (key.Equals("M"))  //ZZZZ
                    {
                        if (gm != null)
                            gm.addMark(strValue);
                        else	//ZZZ ZZ ZZ ZZZ ZZZZ ZZZZZZZ 
                            _gi.comment += strValue;
                    }
                    else if (key.Equals("AW"))		//ZZZ ZZZZZ ZZZZZ
                    {
                        fMultipleMoves = true;
                        gm = new GoMove(strValue, StoneColor.white);
                    }
                    else if (key.Equals("AB"))		//ZZZ ZZZZZ ZZZZZ
                    {
                        fMultipleMoves = true;
                        gm = new GoMove(strValue, StoneColor.black);
                    }
                    else if (key.Equals("HA"))
                        _gi.handicap = (strValue);
                    else if (key.Equals("BR"))
                        _gi.rankBlack = (strValue);
                    else if (key.Equals("PB"))
                        _gi.playerBlack = (strValue);
                    else if (key.Equals("PW"))
                        _gi.playerWhite = (strValue);
                    else if (key.Equals("WR"))
                        _gi.rankWhite = (strValue);
                    else if (key.Equals("DT"))
                        _gi.date = (strValue);
                    else if (key.Equals("KM"))
                        _gi.km = (strValue);
                    else if (key.Equals("RE"))
                        _gi.result = (strValue);
                    else if (key.Equals("SZ"))
                        _gi.size = (strValue);
                    else if (key.Equals("EV"))
                        _gi.gameEvent = (strValue);
                    else if (key.Equals("PC"))
                        _gi.location = (strValue);
                    else if (key.Equals("TM"))
                        _gi.time = (strValue);
                    else if (key.Equals("GN"))
                        _gi.gameName = strValue;

                    else if (key.Equals("FF"))
                        _gi.unknown_ff = (strValue);
                    else if (key.Equals("GM"))
                        _gi.unknown_gm = (strValue);
                    else if (key.Equals("VW"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("US"))
                        _gi.unknown_vw = (strValue);

                    else if (key.Equals("BS"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("WS"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("ID"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("KI"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("SO"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("TR"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("LB"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("RO"))
                        _gi.unknown_vw = (strValue);


                    //ZZZZ ZZZZZ
                    else
                        System.Diagnostics.Debug.Assert(false, "unhandle key: " + key + " "+ strValue);

                    //ZZZZZZZZZ ZZZ ZZZZ ZZZZZZ ZZZ ZZZZ ZZ ZZZZ ZZZZ (ZZ, ZZ) ZZZZ ZZZZZZZZ ZZZZZ. 
                    if (fMultipleMoves)
                    {
                        _currVar.addAMove(gm);
                    }
                }
            }

            //ZZZ ZZZ ZZZZ ZZ ZZZZZZZ ZZZZZZZZZ. 
            if (!fMultipleMoves && gm != null)
            {
                _currVar.addAMove(gm);
            }
			return true;
		} 

		public GoMove doPrev() 
		{
			return _currVar.doPrev();
		}

		public GoMove peekPrev() 
		{
			return _currVar.peekPrev();
		}

		public GoMove doNext() 
		{
			return _currVar.doNext();
		}

		public void updateResult(GoMove gm) 
		{
			_currVar.updateResult(gm);
		}
		
		public void reset()
		{
			_currVarNum = 0;
			_currVarId = 0; 
			_currVar.reset();
		}
		public void rewindToStart()
		{

		}
	} //ZZZ ZZ ZZZZZZ
}
