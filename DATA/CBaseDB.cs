using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace iNervCore.DB
{
    abstract class CBaseDB
    {
        protected DataSet pDs = null;
        protected string sConquery = string.Empty;

        public virtual DataTable DATA
        {
            get
            {
                if (pDs == null)
                    return null;
                else
                    return pDs.Tables[0];
            }
        }

        public virtual int COUNT
        {
            get
            {
                if (pDs == null)
                    return 0;
                else
                    return pDs.Tables[0].Rows.Count;
            }
        }   

        public abstract void CreateDatabase(string strPath, string strPwd = "");
        public abstract bool Open(string sPath, string strPwd = "");
        public abstract bool Open(string ip, int port, string db, string uid, string pwd);
        protected abstract bool Open();
        public abstract void ClearData();
        public abstract void Close();
        public abstract bool ExecQuery(string query);
        public abstract int LoadData(string query);
        public abstract bool ExistTable(string sTableName);

        public abstract bool ExistColumn(string sTable, string sColumn);

        public abstract bool ModifyColumn(string sTable, string sColumn, string sType);

        public abstract bool DropColumn(string sTable, string sColumn);

        public abstract bool AddColumn(string sTable, string sColumn, string sType);
    }
}
