using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using iNervCore.DB;
using System.Data.SQLite;
using iNervCore.UTIL;

namespace iNervCore.DB
{
    class CSqlite : CBaseDB
    {
        SQLiteConnection pConn = null;
        //OleDbDataReader objReader = null;

        public override void CreateDatabase(string strPath, string strPwd = "")
        {
            try
            {
                SQLiteConnection.CreateFile(strPath);
                Open(strPath);
                Close();
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.DATA, "Sqlite::CreateDatabase() - " + e.Message);
            }
        }

        public override bool Open(string sPath, string strPwd = "")
        {
            sConquery = sPath;
            return Open();
        }

        public override bool Open(string ip, int port, string db, string uid, string pwd)
        {
            return false;
        }

        protected override bool Open()
        {
            if (sConquery == "")
                return false;

            string sConnStr = @"Data Source=" + sConquery + ";Version=3;";
            try
            {
                if(pConn != null)
                {
                    Close();
                }

                pConn = new SQLiteConnection(sConnStr);
                pConn.Open();
            }
            catch(Exception e)
            {
                CLog.LOG(LOG_TYPE.DATA, "Sqlite::Open(" + sConnStr + ") - " + e.Message);
                return false;
            }
            return true;
        }
        
        public override void ClearData()
        {
            try
            {
                if (pDs != null)
                {
                    pDs.Clear();
                    pDs.Dispose();
                    pDs = null;
                }
            }
            catch (Exception)
            {
            }
        }
        public override void Close()
        {
            if (pConn == null)
                return;

            try
            {
                pConn.Close();
                pConn.Dispose();
                pConn = null;
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.DATA, "Sqlite::Close() - " + e.Message);
            }
        }

        public override bool ExecQuery(string query)
        {
            if (pConn == null) 
                return false;

            try
            {
                SQLiteCommand command = new SQLiteCommand(query, pConn);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.DATA, "Sqlite::ExecQuery(" + query + ") - " + e.Message);
                return false;
            }

            return true;
        }

        public override int LoadData(string query)
        {
            if (pConn == null)
                return 0;

            try
            {
                ClearData();
                pDs = new DataSet();

                SQLiteDataAdapter adp = new SQLiteDataAdapter(query, pConn);
                adp.Fill(pDs);

                return pDs.Tables[0].Rows.Count;
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.DATA, "Sqlite::LoadData(" + query + ") - " + e.Message);
                Close();
                return -1;
            }
        }

        public override bool ExistTable(string sTable)
        {
            if (pConn == null)
                return false;

            try
            {
                string query = "SELECT name FROM sqlite_master WHERE type = 'table' AND name='" + sTable + "';";
                int nCnt = LoadData(query);
                if (nCnt <= 0)
                    return false;
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.DATA, "Sqlite::ExistTable(" + sTable + ") - " + e.Message);
                return false;
            }
            return true;
        }

        public override bool ExistColumn(string sTable, string sColumn)
        {
            if (pConn == null)
                return false;

            try
            {
                string query = "PRAGMA table_info(" + sTable + ");";
                int nCnt = LoadData(query);
                if (nCnt <= 0)
                    return false;

                foreach(DataRow row in DATA.Rows)
                {
                    //UTIL.PR("ExistColumn : " + row["name"].ToString());
                    if (row["name"].ToString() == sColumn)
                        return true;
                }
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.DATA, "Sqlite::ExistColumn(" + sTable + ") - " + e.Message);
                return false;
            }

            return false;
        }

        public override bool ModifyColumn(string sTable, string sColumn, string sType)
        {
            if (pConn == null)
                return false;

            try
            {
                int nCnt = LoadData("PRAGMA table_info(" + sTable + ");");
                if (nCnt <= 0)
                    return false;

                string sQuery = "CREATE TABLE " + sTable + "(";
                string sQueryInsertColumns = "";
                string sPrimaryKey = ",PRIMARY KEY(";
                bool bPrimaryKey = false;
                foreach(DataRow row in DATA.Rows)
                {
                    if(row["name"].ToString() != sColumn)
                        sQuery += row["name"].ToString() + " " + row["type"].ToString();
                    else
                        sQuery += sColumn + " " + sType;

                    if (row["notnull"].ToString() == "1")   //NOT NULL 체크
                        sQuery += " NOT NULL ";

                    if (row["pk"].ToString() == "1")        //PRIMARY KEY 체크
                    {
                        bPrimaryKey = true;
                        sPrimaryKey += row["name"].ToString() + ",";
                    }

                    if (row["dflt_value"].ToString() != null && row["dflt_value"].ToString() != "") //기본값 체크
                    {
                        if (row["type"].ToString().StartsWith("TEXT"))
                            sQuery += "DEFAULT '" + row["dflt_value"].ToString() + "'";
                        else
                            sQuery += "DEFAULT " + row["dflt_value"].ToString();
                    }

                    sQueryInsertColumns += row["name"].ToString();

                    if (row != DATA.Rows[DATA.Rows.Count -1])
                    {
                        sQuery += ", ";
                        sQueryInsertColumns += ", ";
                    }
                }
                if(bPrimaryKey)
                {
                    sPrimaryKey = sPrimaryKey.Substring(0, sPrimaryKey.Length - 1);
                    sQuery += sPrimaryKey + ")";
                }
                sQuery += ");";

                if (!ExecQuery("ALTER TABLE " + sTable + " RENAME TO old_" + sTable))
                    return false;
                if( !ExecQuery(sQuery) )
                {
                    // 새로운 테이블 생성 실패 시 원상복구
                    ExecQuery("ALTER TABLE old_" + sTable + " RENAME TO " + sTable);
                    return false;
                }
                sQuery = "INSERT INTO " + sTable + "(" + sQueryInsertColumns + ")" 
                          +"SELECT " + sQueryInsertColumns + " FROM old_" + sTable;
                if( !ExecQuery(sQuery) )
                {
                    // 새로운 테이블로 데이터 밀어넣기 실패 시 모두 원상복구
                    ExecQuery("DROP TABLE " + sTable);
                    ExecQuery("ALTER TABLE old_" + sTable + " RENAME TO " + sTable);
                    return false;
                }

                ExecQuery("DROP TABLE old_" + sTable);
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.DATA, "Sqlite::AlterTable(" + sTable + ") - " + e.Message);
                return false;
            }

            return true;
        }

        public override bool DropColumn(string sTable, string sColumn)
        {
            if (pConn == null)
                return false;

            try
            {
                int nCnt = LoadData("PRAGMA table_info(" + sTable + ");");
                if (nCnt <= 0)
                    return false;

                string sQuery = "CREATE TABLE " + sTable + "(";
                string sQueryInsertColumns = "";
                string sPrimaryKey = ",PRIMARY KEY(";
                bool bPrimaryKey = false;
                foreach (DataRow row in DATA.Rows)
                {
                    if (row["name"].ToString() != sColumn)
                        sQuery += row["name"].ToString() + " " + row["type"].ToString();
                    else
                        continue;

                    if (row["notnull"].ToString() == "1")   //NOT NULL 체크
                        sQuery += " NOT NULL ";

                    if (row["pk"].ToString() == "1")        //PRIMARY KEY 체크
                    {
                        bPrimaryKey = true;
                        sPrimaryKey += row["name"].ToString() + ",";
                    }

                    if (row["dflt_value"].ToString() != null && row["dflt_value"].ToString() != "") //기본값 체크
                    {
                        if (row["type"].ToString().StartsWith("TEXT"))
                            sQuery += "DEFAULT '" + row["dflt_value"].ToString() + "'";
                        else
                            sQuery += "DEFAULT " + row["dflt_value"].ToString();
                    }

                    sQueryInsertColumns += row["name"].ToString();

                    if (row != DATA.Rows[DATA.Rows.Count - 1])
                    {
                        sQuery += ", ";
                        sQueryInsertColumns += ", ";
                    }
                }

                if(sQuery.EndsWith(", "))
                {
                    sQuery = sQuery.Substring(0, sQuery.Length - 2);
                    sQueryInsertColumns = sQueryInsertColumns.Substring(0, sQueryInsertColumns.Length - 2);
                }

                if (bPrimaryKey)
                {
                    sPrimaryKey = sPrimaryKey.Substring(0, sPrimaryKey.Length - 1);
                    sQuery += sPrimaryKey + ")";
                }
                sQuery += ");";

                ExecQuery("ALTER TABLE " + sTable + " RENAME TO old_" + sTable);
                if (!ExecQuery(sQuery))
                {
                    // 새로운 테이블 생성 실패 시 원상복구
                    ExecQuery("ALTER TABLE old_" + sTable + " RENAME TO " + sTable);
                    return false;
                }

                sQuery = "INSERT INTO " + sTable + "(" + sQueryInsertColumns + ")"
                          + "SELECT " + sQueryInsertColumns + " FROM old_" + sTable;

                if (!ExecQuery(sQuery))
                {
                    // 새로운 테이블로 데이터 밀어넣기 실패 시 모두 원상복구
                    ExecQuery("DROP TABLE " + sTable);
                    ExecQuery("ALTER TABLE old_" + sTable + " RENAME TO " + sTable);
                    return false;
                }
                ExecQuery("DROP TABLE old_" + sTable);
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.DATA, "Sqlite::AlterTable(" + sTable + ") - " + e.Message);
                return false;
            }

            return true;
        }

        public override bool AddColumn(string sTable, string sColumn, string sColType)
        {
            if (pConn == null)
                return false;

            string sType;
            string sSize;

            try
            {
                string sQuery = "ALTER TABLE " + sTable + " ADD COLUMN " + sColumn + " ";// + sType + ";";

                sType = sColType.Substring(0, 1);
                sSize = sColType.Substring(1);
                switch (sType)
                {
                    case "T":
                        sType = "TEXT";
                        if (sSize != "")
                            sSize = "(" + sSize + ")";
                        break;
                    case "I":
                        sType = "INTEGER";
                        sSize = "";
                        break;
                    case "L":
                        sType = "INTEGER";
                        sSize = "";
                        break;
                    case "M":
                        sType = "TEXT";
                        if (sSize != "")
                            sSize = "(" + sSize + ")";
                        break;
                }
                sQuery += sType + sSize + ";";

                if (!ExecQuery(sQuery))
                {   
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void BuildTable(string sDBName, string[] srTableInfo)
        {
            if (!Open(sDBName))
            {
                CreateDatabase(sDBName);
            }

            if (ExistTable(srTableInfo[0]))
            {
                return;
            }

            int i = 0;
            string squery = "CREATE TABLE IF NOT EXISTS " + srTableInfo[0] + "(";
            string sType;
            string sSize;
            for (i = 1; i < srTableInfo.Length; i += 2)
            {
                squery += srTableInfo[i] + " ";
                sType = srTableInfo[i + 1].Substring(0, 1);
                sSize = srTableInfo[i + 1].Substring(1);
                switch (sType)
                {
                    case "T":
                        sType = "TEXT";
                        if (sSize != "")
                            sSize = "(" + sSize + ")";
                        break;
                    case "I":
                        sType = "INTEGER";
                        sSize = "";
                        break;
                    case "L":
                        sType = "INTEGER";
                        sSize = "";
                        break;
                    case "M":
                        sType = "TEXT";
                        if (sSize != "")
                            sSize = "(" + sSize + ")";
                        break;
                }
                if (i + 1 < srTableInfo.Length - 1)
                    squery += sType + sSize + ", ";
                else
                    squery += sType + sSize + " ";
            }
            squery += ");";
            if (!ExecQuery(squery))
            {
                CLog.LOG(LOG_TYPE.ERR, "BuildTable Erro: DB-" + sDBName + " SQL: " + squery);
                //UTIL.PR("TABLE 생성 실패: " + sDBName + "-" + srTableInfo[0]);
                //UTIL.PR("query: " + squery);

                //UTIL.WRITE_LOG(LOG_TYPE.SET, "CREATE TABLE FAILED");
            }
            Close();
        }
    }
}
