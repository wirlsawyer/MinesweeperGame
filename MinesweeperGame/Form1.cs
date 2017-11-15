using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinesweeperGame
{
    public partial class Form1 : Form
    {
        private Minesweeper _Minwper = new Minesweeper();
        const int LENGTH = 20;
        bool flag_BOMB = false;
        bool flag_IsGameOver = false;
        int lastX = -1;
        int lastY = -1;
        //Robote
        private Thread _thread = null;
        private Form1 _Me = null;
        IDictionary<String, DataInfo> _DBData = null;

        public Form1()
        {
            InitializeComponent();
            _Me = this;
            DBLite.Create(Application.StartupPath+"\\data.db");
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void AddList(String str)
        {
            listBox1.Items.Add(str);
        }

        static public void Run(Form form, Delegate method)
        {
            //EX: NSRunOnMainThread.Run(new Action(()=>func(param1, param2)));
            form.Invoke(method);
        }

        private void GameStart()
        {
            listBox1.Items.Clear();
            _DBData = DBLite.GetData(Application.StartupPath + "\\data.db");

            lastX = -1;
            lastY = -1;
            flag_BOMB = false;
            flag_IsGameOver = false;
            _Minwper.Init(16, 16, 40);
            Draw(flag_BOMB);

        }

        private void Draw(bool ShowBomb)
        {
            Bitmap canvas = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);

            using (Graphics g = Graphics.FromImage(canvas))
            {
                Pen pen = new Pen(Color.Black, 1);
                for (int i = 0; i < _Minwper.GetRowCount(); i++)
                {
                    for (int j = 0; j < _Minwper.GetColCount(); j++)
                    {
                        Brush brush = null;
                        String str = "";
                        switch (_Minwper.GetData(i, j))
                        {
                            case Minesweeper.BOUNDARY:
                                brush = new SolidBrush(Color.DarkGray);
                                break;
                            case Minesweeper.BOMB:
                                if (ShowBomb)
                                {
                                    brush = new SolidBrush(Color.FromArgb(255, 255, 0, 0));
                                    str = "B";
                                }
                                else
                                {
                                    brush = new SolidBrush(Color.FromArgb(255, 168, 178, 158));
                                }

                                break;
                            case Minesweeper.UNCLICK:
                                brush = new SolidBrush(Color.FromArgb(255, 168, 178, 158));
                                break;
                            default:
                                brush = new SolidBrush(Color.White);
                                str = _Minwper.GetData(i, j).ToString();
                                break;
                        }
                        g.FillRectangle(brush, i * LENGTH, j * LENGTH, LENGTH, LENGTH);
                        g.DrawRectangle(pen, i * LENGTH, j * LENGTH, LENGTH, LENGTH);

                        switch (_Minwper.GetMark(i, j))
                        {
                            case 0:
                                str = "";
                                break;
                            case 1:
                                str = "Ҏ";
                                break;
                            default:
                                break;
                        }
                        g.DrawString(str, this.Font, Brushes.Black, i * LENGTH + 5, j * LENGTH + 5);
                    }
                }


                if (_Minwper.GetLastX() != -1 && _Minwper.GetLastY() != -1)
                {
                    g.DrawString("҉", this.Font, Brushes.Black, _Minwper.GetLastX() * LENGTH + 8, _Minwper.GetLastY() * LENGTH + 5);
                }

                pen.Dispose();
            }

            Bitmap image = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
            pictureBox1.BackgroundImage = canvas; // 設置為背景層
            pictureBox1.Refresh();
            pictureBox1.CreateGraphics().DrawImage(canvas, 0, 0);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            int x = e.X / LENGTH;
            int y = e.Y / LENGTH;

            if (x >= 16 || y >= 16) return;

            switch (e.Button)
            {
                case MouseButtons.Right:
                    _Minwper.SetMark(x, y);
                    break;
                case MouseButtons.Left:

                    if (_Minwper.GetStep() == 0 )
                    {
                        if (_Minwper.IsBomb(x, y)) _Minwper.FirstChange(x, y);
                    }

                    if (_Minwper.IsBomb(x, y))
                    {
                        flag_BOMB = true;
                        flag_IsGameOver = true;
                    }
                    else
                    {
                        _Minwper.StepOn(x, y);
                    }

                    break;

            }

            Draw(flag_BOMB);
            Console.WriteLine(String.Format("Step:{0} X:{1} Y:{2}", _Minwper.GetStep(), x, y));

        }
        private void button1_Click(object sender, EventArgs e)
        {
            GameStart();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
            button2.Text = timer1.Enabled.ToString();
        }

        private void doloop()
        {
            Random crandom = new Random();
            crandom.Next();

            int x = crandom.Next() % (16);
            int y = crandom.Next() % (16);
            int index = -1;
            String key = "";
            double dirCost = -1;
            double totalCost = -1;


            while (flag_IsGameOver == false)
            {

                DataInfo info_safe = SafeNext();
                DataInfo info = GetNextDataInfo();
                if (info_safe != null)
                {
                    x = info_safe.Row;
                    y = info_safe.Col;
                    Console.WriteLine("Safe ({0}, {1})", x, y);
                    info = null;
                }
                else if (info != null)
                {
                    Point point = info.GetSmartPoint();
                    x = point.X;
                    y = point.Y;
                    index = info.SelectIndex;
                    key = info.Key;;
                    dirCost = info.GetFailRatio(index);
                    totalCost = info.GetFailRatio();
                }
                else
                {
                    crandom.Next();
                    x = crandom.Next() % (16);
                    y = crandom.Next() % (16);
                }


                if (_Minwper.GetStep() == 0)
                {
                    if (_Minwper.IsBomb(x, y)) _Minwper.FirstChange(x, y);
                }

                if (_Minwper.Progress() == 100)
                {
                    String tmp = "{\"Step" + (_Minwper.GetStep() + 1).ToString();
                    tmp += " Winner";
                    tmp += "\": {\"Key\":\"" + key + "\", ";
                    tmp += "\"Dir\":\"" + index + "\", ";
                    tmp += "\"DirCost\":\"" + dirCost + "\", ";
                    tmp += "\"TolCost\":\"" + totalCost + "\"";
                    tmp += "}}";
                    Form1.Run(_Me, new Action(() => AddList(tmp)));

                    tmp = "[";
                    for (int i = 0; i < listBox1.Items.Count; i++)
                    {
                        if (i == 0)
                        {
                            tmp += listBox1.Items[i];
                        }
                        else
                        {
                            tmp += "," + listBox1.Items[i];
                        }
                    }

                    tmp += "]";
                    DBLite.SaveStep(_Minwper.GetStep() + 1, _DBData.Count, _Minwper.Progress(), tmp);
                    flag_IsGameOver = true;
                    break;
                }

                if (_Minwper.IsBomb(x, y))
                {
                    flag_BOMB = true;
                    flag_IsGameOver = true;
                    DBLite.SaveData(info, false);

                    if (_DBData.ContainsKey(key))
                    {
                        _DBData[key].Total[index]++;
                        _DBData[key].Fail[index]++;
                    }
                    
                    String tmp = "{\"Step" + (_Minwper.GetStep()+1).ToString();
                    tmp += " GamvOver";
                    tmp += "\": {\"Key\":\"" + key + "\", ";
                    tmp += "\"Dir\":\"" + index + "\", ";
                    tmp += "\"DirCost\":\"" + dirCost + "\", ";
                    tmp += "\"TolCost\":\"" + totalCost + "\"";
                    tmp += "}}";
                    Form1.Run(_Me, new Action(() => AddList(tmp)));

                    //Form1.Run(_Me, new Action(() => AddList(String.Format("{\"Step{0} GameOver\":{\"Key\":\"{1}\", \"Dir:\":{2}, \"DirCost\":{3}, \"TolCost\":{4}}}", _Minwper.GetStep() + 1, key, index, dirCost, totalCost))));

                    tmp = "[";
                    for (int i = 0; i < listBox1.Items.Count; i++)
                    {
                        if (i == 0)
                        {
                            tmp += listBox1.Items[i];
                        }
                        else
                        {
                            tmp += ","+listBox1.Items[i];
                        }
                    }

                    tmp += "]";
                    //tmp = tmp.Replace("\"", "\\\"");
                    DBLite.SaveStep(_Minwper.GetStep()+1, _DBData.Count, _Minwper.Progress(), tmp);
                }
                else
                {
                    _Minwper.StepOn(x, y);
                    DBLite.SaveData(info, true);
                    if (_DBData.ContainsKey(key))
                    {
                        _DBData[key].Total[index]++;
                    }

                    String tmp = "{\"Step" + (_Minwper.GetStep()).ToString();
                    //tmp += " GamvOver";
                    tmp += "\": {\"Key\":\"" + key + "\", ";
                    tmp += "\"Dir\":\"" + index + "\", ";
                    tmp += "\"DirCost\":\"" + dirCost + "\", ";
                    tmp += "\"TolCost\":\"" + totalCost + "\"";
                    tmp += "}}";
                    Form1.Run(_Me, new Action(() => AddList(tmp)));
                    //Form1.Run(_Me, new Action(() => AddList(String.Format("{\"Step{0}\":{\"Key\":\"{1}\", \"Dir:\":{2}, \"DirCost\":{3}, \"TolCost\":{4}}}", _Minwper.GetStep() + 1, key, index, dirCost, totalCost))));
                }

                AutoMark();
                Form1.Run(_Me, new Action(() => Draw(flag_BOMB)));

                Thread.Sleep(5);
                //Console.WriteLine(String.Format("Step:{0} X:{1} Y:{2}", _Minwper.GetStep(), x, y));
            }

            //Console.WriteLine(String.Format("Step:{0} X:{1} Y:{2} GameOver", _Minwper.GetStep(), x, y));
        }



        private void AutoMark()
        {
            for (int i = 0; i < _Minwper.GetRowCount(); i++)
            {
                for (int j = 0; j < _Minwper.GetColCount(); j++)
                {
                    String key = GetRoundNumber(i, j);

                    if (key.Contains("?") == false) continue;
                    if (key.Substring(4, 1) == "?" || key.Substring(4, 1) == "-" || key.Substring(4, 1) == "M") continue;
                    if (int.Parse(key.Substring(4, 1)) == (key.Split('?').Length + key.Split('M').Length) - 2)
                    {
                        for (int k = -1; k <= 1; k++)
                        {
                            for (int z = -1; z <= 1; z++)
                            {
                                switch (_Minwper.GetData(k + i, z + j))
                                {
                                    case Minesweeper.BOMB:
                                    case Minesweeper.UNCLICK:
                                        if (_Minwper.GetMark(k + i, z + j) == 0) _Minwper.SetMark(k + i, z + j);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private DataInfo SafeNext()
        {
            DataInfo result = null;

            for (int i = 0; i < _Minwper.GetRowCount(); i++)
            {
                for (int j = 0; j < _Minwper.GetColCount(); j++)
                {
                    String key = GetRoundNumber(i, j);

                    if (key.Contains("?") == false) continue;
                    if (key.Substring(4, 1) == "?" || key.Substring(4, 1) == "-" || key.Substring(4, 1) == "M") continue;
                    if (int.Parse(key.Substring(4, 1)) ==  key.Split('M').Length - 1)
                    {
                        for (int k = -1; k <= 1; k++)
                        {
                            for (int z = -1; z <= 1; z++)
                            {
                                switch (_Minwper.GetData(k + i, z + j))
                                {
                                    case Minesweeper.UNCLICK:
                                        if (_Minwper.GetMark(k + i, z + j) == 0)
                                        {
                                            result = new DataInfo(k + i, z + j, key);
                                            return result;
                                        }
                                        break;
                                }

                            }
                        }
                    }
                }
            }

            return result;
        }

        private String GetRoundNumber(int row, int col)
        {

            String result = "";

            for (int j = -1; j <= 1; j++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    String tag = _Minwper.GetData(i + row, j + col).ToString();
                    switch (_Minwper.GetData(i + row, j + col))
                    {
                        case Minesweeper.BOUNDARY:
                            tag  = "-";
                            break;
                        case Minesweeper.BOMB:
                        case Minesweeper.UNCLICK:
                            tag  = "?";
                            break;
                        default:
                            tag =  _Minwper.GetData(i + row, j + col).ToString();
                            break;
                    }

                    if (_Minwper.GetMark(i + row, j + col) == 1) tag = "M";
                    result += tag;
                }
            }

            return result;
        }


        private void CopyDataInfo(DataInfo src, DataInfo dest)
        {
            for (int i = 0; i < 9; i++)
            {
                dest.Total[i] = src.Total[i];
                dest.Fail[i] = src.Fail[i];
            }
        }

        private DataInfo GetNextDataInfo()
        {
            
            List<DataInfo> list = new List<DataInfo>();

            for (int i = 0; i < _Minwper.GetRowCount(); i++)
            {
                for (int j = 0; j < _Minwper.GetColCount(); j++)
                {
                    String key = "";
                    switch (_Minwper.GetData(i, j))
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                            key = GetRoundNumber(i, j);
                            if (key.Contains("?"))
                            {
                                if (int.Parse(key.Substring(4, 1)) != (key.Split('?').Length + key.Split('M').Length) - 2)
                                {
                                    DataInfo newItem = new DataInfo(i, j, key);
                                    list.Add(newItem);
                                    if (_DBData.ContainsKey(key)) CopyDataInfo(_DBData[key], newItem);
                                }
                            }
                            
                            break;
                    }

                }
            }





            double fValue = double.MaxValue;
            DataInfo result = null;
            foreach (DataInfo info in list)
            {
                if (fValue > info.GetFailRatio())
                {
                    fValue = info.GetFailRatio();
                    result = info;
                }

            }

            if (result == null)
            {
                //Error
                return null;
            }

            return result;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_thread == null)
            {

            }
            else if (_thread.IsAlive)
            {
                return;
            }
            GameStart();
            _thread = new Thread(new ThreadStart(doloop));
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
           
            AutoMark();
        }
    }
}
