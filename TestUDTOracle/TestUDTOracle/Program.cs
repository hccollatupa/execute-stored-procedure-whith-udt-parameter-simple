using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Oracle.DataAccess.Client;
using System.Configuration;
using Oracle.DataAccess.Types;

namespace TestUDTOracle
{
    class Program
    {
        static void Main(string[] args)
        {
            ProductInfoList productInfoList = new ProductInfoList();
            ProductInfo[] productInfoArray = new ProductInfo[] {
                new ProductInfo(){ Id = 1, Description = "Producto 1", Price = 1.3 },
                new ProductInfo(){ Id = 2, Description = "Producto 2", Price = 2.4 }
            };
            productInfoList.Values = productInfoArray;

            string connectionString = ConfigurationManager.ConnectionStrings["connectionStrings"].ConnectionString;

            OracleConnection con = new OracleConnection();
            con.ConnectionString = connectionString;
            con.Open();

            OracleCommand cmd = con.CreateCommand();

            cmd.CommandText = "PKG_TEST_01.CREATE_PRODUCT";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            OracleParameter Oparameter1 = cmd.CreateParameter();
            Oparameter1.Direction = ParameterDirection.Input;
            Oparameter1.ParameterName = "P_PRODUCTS";
            Oparameter1.OracleDbType = OracleDbType.Array;
            Oparameter1.UdtTypeName = "PRODUCT_TABLE_UDT";
            Oparameter1.Value = productInfoList;
            cmd.Parameters.Add(Oparameter1);
            cmd.ExecuteNonQuery();

            Console.WriteLine("Presione una tecla para finalizar...");
            Console.ReadLine();
        }
    }

    [OracleCustomTypeMapping("PRODUCT_TABLE_UDT")]
    public class ProductInfoList : CustomCollectionTypeBase<ProductInfoList, ProductInfo>
    {
    }

    [OracleCustomTypeMapping("PRODUCT_UDT")]
    public class ProductInfo : CustomTypeBase<ProductInfo>
    {
        [OracleObjectMapping("ID")]
        public long Id;
        [OracleObjectMapping("DESCRIPTION")]
        public string Description;
        [OracleObjectMapping("PRICE")]
        public double Price;

        public override void FromCustomObject(OracleConnection connection, IntPtr pointerUdt)
        {
            OracleUdt.SetValue(connection, pointerUdt, "ID", Id);
            OracleUdt.SetValue(connection, pointerUdt, "DESCRIPTION", Description);
            OracleUdt.SetValue(connection, pointerUdt, "PRICE", Price);
        }

        public override void ToCustomObject(OracleConnection connection, IntPtr pointerUdt)
        {
            Id = (long)OracleUdt.GetValue(connection, pointerUdt, "ID");
            Description = (string)OracleUdt.GetValue(connection, pointerUdt, "DESCRIPTION");
            Price = (long)OracleUdt.GetValue(connection, pointerUdt, "PRICE");
        }
    }

    #region Helpers UDT Oracle
    public abstract class CustomCollectionTypeBase<TType, TValue> : CustomTypeBase<TType>, IOracleArrayTypeFactory where TType : CustomTypeBase<TType>, new()
    {
        [OracleArrayMapping()]
        public TValue[] Values;

        public override void FromCustomObject(OracleConnection connection, IntPtr pointerUdt)
        {
            OracleUdt.SetValue(connection, pointerUdt, 0, Values);
        }

        public override void ToCustomObject(OracleConnection connection, IntPtr pointerUdt)
        {
            Values = (TValue[])OracleUdt.GetValue(connection, pointerUdt, 0);
        }

        public Array CreateArray(int elementCount)
        {
            return new TValue[elementCount];
        }

        public Array CreateStatusArray(int elementCount)
        {
            return new OracleUdtStatus[elementCount];
        }
    }

    public abstract class CustomTypeBase<T> : IOracleCustomType, IOracleCustomTypeFactory, INullable where T : CustomTypeBase<T>, new()
    {
        private bool _isNull;

        public IOracleCustomType CreateObject()
        {
            return new T();
        }

        public abstract void FromCustomObject(OracleConnection connection, IntPtr pointerUdt);

        public abstract void ToCustomObject(OracleConnection connection, IntPtr pointerUdt);

        public bool IsNull
        {
            get { return this._isNull; }
        }

        public static T Null
        {
            get { return new T { _isNull = true }; }
        }
    }
    #endregion
}