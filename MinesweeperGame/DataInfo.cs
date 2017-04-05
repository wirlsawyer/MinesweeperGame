using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperGame
{
    class DataInfo
    {
        public int Row = -1;
        public int Col = -1;
        public int SelectIndex = -1;


        public String Key = "";
        public int[] Total = new int[9];
        public int[] Fail = new int[9];

        public DataInfo(int row, int col, String key)
        {
            this.Row = row;
            this.Col = col;
            this.Key = key;

            for (int i = 0; i < 9; i++)
            {
                Total[i] = 0;
                Fail[i] = 0;
            }
        }

        public Point GetSmartPoint()
        {
            Point point = new Point();
            point.X = -1;
            point.Y = -1;

            double value = double.MaxValue;
            for (int i = 0; i < 9; i++)
            {
                if (i == 4) continue;
                if (Key.Substring(i, 1) != "?") continue;


                double fValue = 0;
                if (Total[i] != 0)
                {
                    fValue = GetFailRatio(i);
                }

                if (value > fValue)
                {
                    this.SelectIndex = i;
                    value = fValue;
                    switch (i)
                    {
                        case 0:
                            point.X = this.Row - 1;
                            point.Y = this.Col - 1;
                            break;
                        case 1:
                            point.X = this.Row;
                            point.Y = this.Col - 1 ;
                            break;
                        case 2:
                            point.X = this.Row + 1;
                            point.Y = this.Col - 1;
                            break;
                        case 3:
                            point.X = this.Row - 1;
                            point.Y = this.Col;
                            break;
                        case 5:
                            point.X = this.Row + 1;
                            point.Y = this.Col;
                            break;
                        case 6:
                            point.X = this.Row - 1;
                            point.Y = this.Col + 1;
                            break;
                        case 7:
                            point.X = this.Row;
                            point.Y = this.Col + 1;
                            break;
                        case 8:
                            point.X = this.Row + 1;
                            point.Y = this.Col + 1;
                            break;
                    }

                    
                }
            }

            return point;
        }


        public double GetFailRatio()
        {
            double total = 0;
            double fail = 0;

            for (int i = 0; i < 9; i++)
            {
                total += Total[i];
                fail += Fail[i];
            }

            if (total == 0) return 0;
            return fail / total;
        }

        public double GetFailRatio(int index)
        {
            if (Total[index] == 0) return 0;
            return ((double)Fail[index] / (double)Total[index]);
        }
    }
}
