using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Data;
using SharpMap.Data.Providers;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using Tools;

namespace Ptv.Controls.Map.AddressMonitor
{
    /// <summary>
    /// The AMProvider provider is used for rendering point data from an AddressMonitor compatible datasource.
    /// The code
    /// </summary>
    /// <remarks>
    /// <para>The data source will need to have two int32-type columns, xColumn and yColumn that contains the coordinates of the point,
    /// and an integer-type column containing a unique identifier for each row.</para>
    /// <para>To get good performance, make sure you have applied indexes on ID, xColumn and yColumns in your datasource table.</para>
    /// </remarks>
    public class AMProvider : IProvider, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the AMProvider
        /// </summary>
        /// <param name="ConnectionStr"></param>
        /// <param name="tablename"></param>
        /// <param name="OID_ColumnName"></param>
        /// <param name="xColumn"></param>
        /// <param name="yColumn"></param>
        public AMProvider(string ConnectionStr, string tablename, string OID_ColumnName, string xColumn, string yColumn)
        {
            this.Table = tablename;
            this.XColumn = xColumn;
            this.YColumn = yColumn;
            this.ObjectIdColumn = OID_ColumnName;
            this.ConnectionString = ConnectionStr;
        }

        private string _Table;

        /// <summary>
        /// Data table name
        /// </summary>
        public string Table
        {
            get { return _Table; }
            set { _Table = value; }
        }


        private string _ObjectIdColumn;

        /// <summary>
        /// Name of column that contains the Object ID
        /// </summary>
        public string ObjectIdColumn
        {
            get { return _ObjectIdColumn; }
            set { _ObjectIdColumn = value; }
        }

        private string _XColumn;

        /// <summary>
        /// Name of column that contains X coordinate
        /// </summary>
        public string XColumn
        {
            get { return _XColumn; }
            set { _XColumn = value; }
        }

        private string _YColumn;

        /// <summary>
        /// Name of column that contains Y coordinate
        /// </summary>
        public string YColumn
        {
            get { return _YColumn; }
            set { _YColumn = value; }
        }

        private string _ConnectionString;
        /// <summary>
        /// Connectionstring
        /// </summary>
        public string ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }
        }

        #region IProvider Members

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>Total number of features</returns>
        public int GetFeatureCount()
        {
            int count = 0;
            using (var conn = new OleDbConnection(_ConnectionString))
            {
                string strSQL = "Select Count(*) FROM " + this.Table;
                if (!string.IsNullOrEmpty(_defintionQuery)) //If a definition query has been specified, add this as a filter on the query
                    strSQL += " WHERE " + _defintionQuery;

                using (var command = new OleDbCommand(strSQL, conn))
                {
                    conn.Open();
                    count = (int)command.ExecuteScalar();
                    conn.Close();
                }
            }
            return count;
        }

        private string _defintionQuery;

        /// <summary>
        /// Definition query used for limiting dataset
        /// </summary>
        public string DefinitionQuery
        {
            get { return _defintionQuery; }
            set { _defintionQuery = value; }
        }

        /// <summary>
        /// Returns a datarow based on a RowID
        /// </summary>
        /// <param name="RowID"></param>
        /// <returns>datarow</returns>
        public FeatureDataRow GetFeature(uint RowID)
        {
            using (var conn = new OleDbConnection(_ConnectionString))
            {
                string strSQL = "select * from " + this.Table + " WHERE " + this.ObjectIdColumn + "=" + RowID.ToString();

                using (var adapter = new OleDbDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    var ds = new DataSet();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            var fdr = fdt.NewRow();
                            foreach (DataColumn col in ds.Tables[0].Columns)
                                fdr[col.ColumnName] = dr[col];
                            if (dr[this.XColumn] != DBNull.Value && dr[this.YColumn] != DBNull.Value)
                                fdr.Geometry = new Point(Convert.ToDouble(dr[this.XColumn]), Convert.ToDouble(dr[this.YColumn]));
                            return fdr;
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        public string ConnectionID
        {
            get { return _ConnectionString; }
        }

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public void Open()
        {
            //Don't really do anything. OleDb's ConnectionPooling takes over here
            _IsOpen = true;
        }
        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            //Don't really do anything. OleDb's ConnectionPooling takes over here
            _IsOpen = false;
        }

        private bool _IsOpen;

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public bool IsOpen
        {
            get { return _IsOpen; }
        }

        private int _SRID = -1;
        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public int SRID
        {
            get { return _SRID; }
            set { _SRID = value; }
        }

        #endregion

        #region Disposers and finalizers
        private bool disposed = false;

        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                }
                disposed = true;
            }
        }

        public Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            bbox = GeoTools.Wgs2SphereMercator(bbox, true);
            var result = new Collection<IGeometry>();
            using (var conn = new OleDbConnection(_ConnectionString))
            {
                string strSQL = "Select " + this.XColumn + ", " + this.YColumn + " FROM " + this.Table + " WHERE ";
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += _defintionQuery + " AND ";
                //Limit to the points within the boundingbox
                strSQL += this.XColumn + " BETWEEN " + Convert.ToInt32(bbox.Left()).ToString() + " AND " + Convert.ToInt32(bbox.Right()).ToString() + " AND " +
                    this.YColumn + " BETWEEN " + Convert.ToInt32(bbox.Bottom()).ToString() + " AND " + Convert.ToInt32(bbox.Top()).ToString();

                using (var command = new OleDbCommand(strSQL, conn))
                {
                    conn.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value && dr[1] != DBNull.Value)
                                result.Add(new Point(GeoTools.SphereMercator2Wgs(
                                    new Coordinate(Convert.ToDouble(dr[0]), Convert.ToDouble(dr[1])), true)));
                        }
                    }
                    conn.Close();
                }
            }

            return result;
        }

        public Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            bbox = GeoTools.Wgs2SphereMercator(bbox, true);
            Collection<uint> objectlist = new Collection<uint>();
            using (var conn = new OleDbConnection(_ConnectionString))
            {
                string strSQL = "Select " + this.ObjectIdColumn + " FROM " + this.Table + " WHERE ";
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += _defintionQuery + " AND ";
                //Limit to the points within the boundingbox
                strSQL += this.XColumn + " BETWEEN " + Convert.ToInt32(bbox.Left()).ToString() + " AND " + Convert.ToInt32(bbox.Right()).ToString() + " AND " + this.YColumn +
                    " BETWEEN " + Convert.ToInt32(bbox.Bottom()).ToString() + " AND " + Convert.ToInt32(bbox.Top()).ToString();

                using (var command = new OleDbCommand(strSQL, conn))
                {
                    conn.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                            if (dr[0] != DBNull.Value)
                                objectlist.Add((uint)(int)dr[0]);
                    }
                    conn.Close();
                }
            }
            return objectlist;
        }

        IGeometry IProvider.GetGeometryByID(uint oid)
        {
            using (var conn = new OleDbConnection(_ConnectionString))
            {
                string strSQL = "Select " + this.XColumn + ", " + this.YColumn + " FROM " + this.Table + " WHERE " + this.ObjectIdColumn + "=" + oid.ToString();
                using (var command = new OleDbCommand(strSQL, conn))
                {
                    conn.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            //If the read row is OK, create a point geometry from the XColumn and YColumn and return it
                            if (dr[0] != DBNull.Value && dr[1] != DBNull.Value)
                                return new Point(GeoTools.SphereMercator2Wgs(new Coordinate(Convert.ToDouble(dr[0]), Convert.ToDouble(dr[1])), true));
                        }
                    }
                    conn.Close();
                }
            }
            return null;
        }

        public void ExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            throw new NotImplementedException();
        }

        public void ExecuteIntersectionQuery(Envelope bbox, FeatureDataSet ds)
        {
            bbox = GeoTools.Wgs2SphereMercator(bbox, true);

            using (var conn = new OleDbConnection(_ConnectionString))
            {
                string strSQL = "Select * FROM " + this.Table + " WHERE ";
                if (!String.IsNullOrEmpty(_defintionQuery)) //If a definition query has been specified, add this as a filter on the query
                    strSQL += _defintionQuery + " AND ";
                //Limit to the points within the boundingbox
                strSQL += this.XColumn + " BETWEEN " + Convert.ToInt32(bbox.Left()).ToString() + " AND " + Convert.ToInt32(bbox.Right()).ToString() + " AND " + this.YColumn +
                    " BETWEEN " + Convert.ToInt32(bbox.Bottom()).ToString() + " AND " + Convert.ToInt32(bbox.Top()).ToString() +
                    " ORDER BY " + this.YColumn + " DESC"; // makes them appear nicely ordered on a map

                using (var adapter = new OleDbDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    var ds2 = new DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (DataColumn col in ds2.Tables[0].Columns)
                            fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds2.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds2.Tables[0].Columns)
                                fdr[col.ColumnName] = dr[col];
                            if (dr[this.XColumn] != DBNull.Value && dr[this.YColumn] != DBNull.Value)
                                fdr.Geometry = new Point(GeoTools.SphereMercator2Wgs(new Coordinate(Convert.ToDouble(dr[this.XColumn]), Convert.ToDouble(dr[this.YColumn])), true));
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        Envelope IProvider.GetExtents()
        {
            using (var conn = new OleDbConnection(_ConnectionString))
            {
                string strSQL = "Select Min(" + this.XColumn + ") as MinX, Max(" + this.XColumn + ") As MaxX, " +
                                       "Min(" + this.YColumn + ") As minY, Max(" + this.YColumn + ") As MaxY FROM " + this.Table;
                if (!String.IsNullOrEmpty(_defintionQuery)) //If a definition query has been specified, add this as a filter on the query
                    strSQL += " WHERE " + _defintionQuery;

                using (var command = new OleDbCommand(strSQL, conn))
                {
                    conn.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            // If the read row is OK, create a point geometry from the XColumn and YColumn and return it
                            if (dr[0] != DBNull.Value && dr[1] != DBNull.Value && dr[2] != DBNull.Value && dr[3] != DBNull.Value)
                                return GeoTools.SphereMercator2Wgs(
                                    new Envelope(Convert.ToDouble(dr[0]), Convert.ToDouble(dr[1]), 
                                    Convert.ToDouble(dr[2]), Convert.ToDouble(dr[3])), true);
                        }
                    }
                    conn.Close();
                }
            }
            return null;
        }
        #endregion
    }
}
