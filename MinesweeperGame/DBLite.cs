using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperGame
{
    class DBLite
    {
        const String TABLE_NAME1 = "data";
        const String TABLE_NAME2 = "step";

        static String _Path = "";
        static public void Create(String path)
        {
            if (!File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);
            }
            _Path = path;

            String sql = @"CREATE TABLE IF NOT EXISTS `" + TABLE_NAME1 + "` (";
            //sql += @"id INTEGER PRIMARY KEY AUTOINCREMENT";
            sql += @" Key TEXT PRIMARY KEY";
            sql += @", T0 INTEGER";
            sql += @", F0 INTEGER";
            sql += @", T1 INTEGER";
            sql += @", F1 INTEGER";
            sql += @", T2 INTEGER";
            sql += @", F2 INTEGER";
            sql += @", T3 INTEGER";
            sql += @", F3 INTEGER";
            //sql += @", T4 INTEGER";
            //sql += @", F4 INTEGER";
            sql += @", T5 INTEGER";
            sql += @", F5 INTEGER";
            sql += @", T6 INTEGER";
            sql += @", F6 INTEGER";
            sql += @", T7 INTEGER";
            sql += @", F7 INTEGER";
            sql += @", T8 INTEGER";
            sql += @", F8 INTEGER";
            sql += ")";

            ExecuteSQL(path, sql);

   
            sql = @"CREATE TABLE IF NOT EXISTS `" + TABLE_NAME2 + "` (";
            sql += @"id INTEGER PRIMARY KEY AUTOINCREMENT";
            sql += @", step INTEGER";
            sql += @", history INTEGER";
            sql += @", progress INTEGER";
            sql += @", data TEXT";
            sql += @", modify datetime DEFAULT  current_timestamp";
            sql += ")";

            ExecuteSQL(path, sql);
        }

        static public bool ExecuteSQL(String path, String sql)
        {
            try
            {

                using (SQLiteConnection conn = new SQLiteConnection("Data source=" + path))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery(); //using behind every write cmd

                    }
                    conn.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        static public void SaveData(DataInfo data, bool IsPass)
        {
            if (data == null) return;
            data.Total[data.SelectIndex]++;
            if (IsPass == false) data.Fail[data.SelectIndex]++;

            String sql = String.Format("REPLACE INTO `{0}` (Key, T0, F0, T1, F1, T2, F2, T3, F3, T5, F5, T6, F6, T7, F7, T8, F8) VALUES ('{1}'", TABLE_NAME1, data.Key);
            for (int i = 0; i < 9; i++)
            {
                if (i == 4) continue;
                sql += String.Format(", {0}, {1}", data.Total[i], data.Fail[i]);
            }
            sql += ")";

            ExecuteSQL(_Path, sql);
        }

        static public void SaveStep(int step, int historyCount, double progress, String data)
        {
            String sql = String.Format("INSERT INTO `{0}` (step, history, progress, data) VALUES ({1}, {2}, {3}, '{4}')", TABLE_NAME2, step.ToString(), historyCount, progress, data);
            ExecuteSQL(_Path, sql);
        }


        static public IDictionary<String, DataInfo> GetData(String path)
        {
            IDictionary<String, DataInfo> result = new Dictionary<String, DataInfo>();

            try
            {

                using (SQLiteConnection conn = new SQLiteConnection("Data source=" + path))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = "SELECT * FROM " + TABLE_NAME1;

                        SQLiteDataReader sqlite_datareader = cmd.ExecuteReader();

                        // 一筆一筆列出查詢的資料
                        while (sqlite_datareader.Read())
                        {
                            String key = sqlite_datareader["Key"].ToString();
                            DataInfo newInfo = new DataInfo(-1, -1, key);
                            for (int i=0; i<9; i++)
                            {
                                if (i == 4) continue;

                                newInfo.Total[i] = int.Parse(sqlite_datareader["T" + i].ToString());
                                newInfo.Fail[i] = int.Parse(sqlite_datareader["F" + i].ToString());

                            }
                            result.Add(key, newInfo);
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }
    }
}
