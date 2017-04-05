using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperGame
{
    class Minesweeper
    {
        public const int BOUNDARY = -8;
        public const int UNCLICK = -1;
        public const int BOMB = 99;

        int _Row = 0;
        int _Col = 0;

        int[,] _Data;
        int[,] _Mark;
        int _Bomb_count = 0;
        int _Step = 0;

        int _LastX = -1;
        int _LastY = -1;
   
        public void Init(int row, int col, int bomb)
        {
            _Step = 0;
            _LastX = -1;
            _LastY = -1;

            _Row = row;
            _Col = col;

            _Data = new int[row + 2, col + 2];
            _Mark = new int[row + 2, col + 2];
            _Bomb_count = bomb;

            for (int i = 0; i < row + 2; i++)
            {
                for (int j = 0; j < col + 2; j++)
                {
                    if (i == 0 || j == 0 || i == row + 1 || j == col + 1)
                    {
                        //設定邊界
                        _Data[i,j] = BOUNDARY;
                        _Mark[i, j] = BOUNDARY;
                    }
                    else
                    {
                        //設定盤面初始內容
                        _Data[i, j] = UNCLICK;
                        _Mark[i, j] = 0;
                    }
                }
            }


            //隨機產生 bombs 個炸彈
            Random crandom = new Random();
            crandom.Next();
            while (_Bomb_count > 0)
            {
                int temp1 = crandom.Next() % (row) + 1;
                int temp2 = crandom.Next() % (col) + 1;
                //當發生重複指定炸彈的狀況時，將 real_bombs-1
                if (_Data[temp1, temp2] != BOMB)
                {
                    _Bomb_count--;
                    _Data[temp1, temp2] = BOMB;
                }
            }
            _Bomb_count = bomb;
        }

       
        private void StepOn(int row, int col, int iLevel)
        {
           
            if (iLevel == 0)
            {
                _Step++;
                _LastX = row;
                _LastY = col;
            }
            iLevel++;
            int x = row + 1;
            int y = col + 1;
            int i, j, count;
            count = 0;
            //統計該位置周邊的炸彈總數
            for (i = -1; i <= 1; i++)
            {
                for (j = -1; j <= 1; j++)
                {
                    if (_Data[i + x, j + y] == BOMB)
                    {
                        count = count + 1;
                    }
                }
            }

            //若周邊炸彈數為 0 時，遞迴呼叫展開
            //每一輪呼叫的順序為 1 4 6
            // 2 7
            // 3 5 8
            //否則 將周邊炸彈總數儲存在該座標的陣列中
            if (count == 0)
            {
                _Data[x, y] = 0;
                for (j = -1; j <= 1; j++)
                {
                    for (i = -1; i <= 1; i++)
                    {
                        //取消下兩行註解 可顯示目前處理的座標位置
                        //cout << i+x <<" " << j+y << endl;
                        //system("pause");
                        if (_Data[i + x, j + y] == UNCLICK)
                        {
                            //取消下兩行註解 可動態展示遞迴展開的狀況
                            // display();
                            // system("pause");
                            StepOn(i + row, j + col, iLevel);
                        }
                    }
                }
            }
            else
            {
                _Data[x, y] = count;
                _Mark[x, y] = -1;
            }
        }
        public void StepOn(int row, int col)
        {
            StepOn(row, col, 0);
        }
        public void FirstChange(int row, int col)
        {
            
            //隨機產生 bombs 個炸彈
            Random crandom = new Random();
            crandom.Next();
            while (true)
            {
                int temp1 = crandom.Next() % (_Row) + 1;
                int temp2 = crandom.Next() % (_Col) + 1;
                //當發生重複指定炸彈的狀況時，將 real_bombs-1
                if (_Data[temp1, temp2] != BOMB)
                {
                    _Data[temp1, temp2] = BOMB;
                    break;
                }
            }

            _Data[row + 1, col + 1] = UNCLICK;

        }

        public double Progress()
        {
            double unCheckCount = 0;
            for (int i = 1; i < _Row + 1; i++)
            {
                for (int j = 1; j < _Col + 1; j++)
                {
                    if (_Data[i, j] == BOMB || _Data[i, j] == UNCLICK)
                    {
                        unCheckCount++;
                    }
                }
            }

            if (unCheckCount == _Bomb_count)
            {
                return 100;
            }

            double total = _Row * _Col;
            double radio = (total - unCheckCount) / total * 100;

            return radio;
        }
        public bool IsBomb(int row, int col)
        {
            _LastX = row;
            _LastY = col;
            return GetData(row, col) == BOMB;
        }

        public int GetStep()
        {
            return _Step;
        }


        public int[,] GetData()
        {
            return _Data;
        }
        public int GetData(int row, int col)
        {
            return _Data[row+1, col+1];
        }

        public int GetMark(int row, int col)
        {
            return _Mark[row + 1, col + 1];
        }

        public void SetMark(int row, int col)
        {
            _Mark[row + 1, col + 1]++;
            _Mark[row + 1, col + 1] %= 2;
        }

        public int GetRowCount()
        {
            return _Row;
        }

        public int GetColCount()
        {
            return _Col;
        }

        public int GetLastX() {
            return _LastX;
        }

        public int GetLastY()
        {
            return _LastY;
        }
    }
}
