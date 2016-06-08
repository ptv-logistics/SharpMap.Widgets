using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using SharpMap.Rendering.Thematics;
using Ptv.Controls.Map;
using SharpMap.Styles;

namespace Ptv.Controls.Map.AddressMonitor
{
    public class AMStyle : ITheme
    {
        private Dictionary<string, string> m_BitmapMapping = new Dictionary<string, string>();
        private string m_IconColumn = String.Empty;
        private double m_MaxVisible = -1;
        private string m_NameColumn = "POIName";
        private BitmapCache bitmapCache = new BitmapCache();

        public AMStyle(string fileName, string bitmapBase)
        {
            InitStyle(fileName, bitmapBase, "");
        }

        public AMStyle(string fileName, string bitmapBase, string password, string nameColumn)
        {
            m_NameColumn = nameColumn;

            InitStyle(fileName, bitmapBase, password);
        }

        private string GetConnectionString(string fileName, string password)
        {
            string connstr = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName;
            if (!string.IsNullOrEmpty(password))
                connstr = connstr + ";Jet OLEDB:Database Password='" + password + "'";

            return connstr;
        }

        protected void InitStyle(string fileName, string bitmapBase, string password)
        {
            using (OleDbConnection conn = new OleDbConnection(GetConnectionString(fileName, password)))
            {
                string select = "Select ColumnName, LowBound, Imagefile from _FilterDefinitions where FilterName = 'Standard'";

                using (System.Data.OleDb.OleDbCommand command = new OleDbCommand(select, conn))
                {
                    conn.Open();
                    using (System.Data.OleDb.OleDbDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (m_IconColumn.Length == 0 && !dr.IsDBNull(0) && dr.GetString(0).Trim().ToUpper() != "NOT_USED")
                                m_IconColumn = dr.GetString(0);

                            if (dr.IsDBNull(1) || dr.IsDBNull(2))
                                continue;

                            string key = dr.GetString(1);
                            string value = dr.GetString(2);
                            if (key.Length > 0 && value.Length > 0)
                                bitmapCache.AddBitmap(key, bitmapBase + "\\" + value, System.Drawing.Color.Magenta);
                        }

                        dr.Close();
                    }
                    conn.Close();
                }
            }

            CalcMaxVisible(fileName, password);
        }

        public double MaxVisible
        {
            get
            {
                return m_MaxVisible;
            }
        }

        // gets the display threshold from _Filterproperties - the value can only be approximated to SharpMap
        private void CalcMaxVisible(string fileName, string password)
        {
            using (OleDbConnection conn = new OleDbConnection(GetConnectionString(fileName, password)))
            {
                string select = "Select DisplayMaxScale from _FilterProperties where FilterName = 'Standard'";

                using (OleDbCommand command = new OleDbCommand(select, conn))
                {
                    conn.Open();
                    object res = command.ExecuteScalar();
                    if (res is double)
                        m_MaxVisible = System.Convert.ToDouble(res) * 5000;
                    conn.Close();
                }
            }
        }

        #region ITheme Members

        public SharpMap.Styles.IStyle GetStyle(SharpMap.Data.FeatureDataRow attribute)
        {
            var style = new VectorStyle();

            if(m_IconColumn != "NOT_USED")
                style.Symbol = bitmapCache.GetBitmap(attribute[m_IconColumn] as string);

            return style;
        }

        #endregion
    }
}
