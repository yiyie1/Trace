using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Collections.ObjectModel;
using Trace.Model;
using Trace.View.Result;

namespace DataBase
{
    class DatabaseConnection
    {
        static string m_connectionString = global::Trace.Properties.Settings.Default.zhongchao_DataConnectionString;

        public const String COMPENSATE_TABLE_NAME = "reftable";

        public static bool QueryUserTable(ref List<User> users)
        {
            bool ret = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SELECT [UserName], [UserPassword], [UserRole] FROM [dbo].[User]", connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine("{0} {1} {2}", reader.GetString(0), reader.GetString(1), reader.GetInt32(2));
                            User u = new User();
                            u.UserName = reader.GetString(0).Trim();
                            u.Password = reader.GetString(1).Trim();
                            int role = reader.GetInt32(2);
                            u.UserRole = role == 0 ? "管理员" : "操作员";

                            users.Add(u);
                        }

                        reader.Close();
                        ret = true;
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return ret;
        }

        #region 创建表格

        //创建参考参数表
        public static void CreateRefTable()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    connection.Open();

                    String strCreateRefTabel = "CREATE TABLE reftable ("
                        + "ref int)";

                    using (SqlCommand command = new SqlCommand(strCreateRefTabel, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static void CreateRefResultTables()
        {
            CreateReflectionResultTable("RefResult_1", 0, 999);
            DataBase.DatabaseConnection.CreateReflectionResultTable("RefResult_2", 1000, 1999);
            CreateReflectionResultTable("RefResult_3", 2000, 2047);
        }

        //创建反射光谱结果表
        public static void CreateReflectionResultTable(String tableName, int startResultIndex, int endResultIndex)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    connection.Open();

                    //创建反射率表1
                    String strCreateRefResultTable = "CREATE TABLE " + tableName + @" (ID int IDENTITY (1,1) PRIMARY KEY, 
                        TaskID int Foreign Key References Task(TaskID), 
                        TestID int, ";

                    for (int i = startResultIndex; i <= endResultIndex; i++)
                    {
                        strCreateRefResultTable += "data" + i.ToString() + " real,";
                    }

                    strCreateRefResultTable = strCreateRefResultTable.TrimEnd(',');
                    strCreateRefResultTable += ")";
                    
                    using (SqlCommand command = new SqlCommand(strCreateRefResultTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        #endregion

        #region 添加用户
        public static bool SaveUserPwdToDatabase(string userName, string password, int userRole)
        {
            bool ret = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    connection.Open();

                    bool bNewUser = true;
                    using (SqlCommand selCmd = new SqlCommand("SELECT [UserName] FROM [dbo].[User]", connection))
                    using (SqlDataReader reader = selCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine("{0}", reader.GetString(0));
                            if (reader.GetString(0).Trim() == userName)
                            {
                                bNewUser = false;
                                break;
                            }
                        }
                        reader.Close();
                    }

                    if (bNewUser)
                    {
                        string insertCmd = "INSERT INTO [dbo].[User] ([UserName], [UserPassword], [UserRole]) VALUES ('" + userName + "', '" + password + "', '" + userRole + "') ";
                        using (SqlCommand sqlcmd = new SqlCommand(insertCmd, connection))
                        {
                            int result = sqlcmd.ExecuteNonQuery();
                            Console.WriteLine("Inserted rows: {0}", result);
                            ret = true;
                        }
                    }
                    else
                    {
                        string updateCmd = "UPDATE dbo.[User] SET [UserPassword] = '" + password + "', [UserRole] = '" + userRole + "' WHERE [UserName] = '" + userName + "'";
                        using (SqlCommand sqlCmd = new SqlCommand(updateCmd, connection))
                        {
                            int result = sqlCmd.ExecuteNonQuery();
                            Console.WriteLine("Updated rows: {0}", result);
                            ret = true;
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return ret;
        }
        #endregion

        #region 删除用户
        public static bool DeleteUser(string userName)
        {
            bool ret = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    connection.Open();

                    bool bExistingUser = false;
                    using (SqlCommand selCmd = new SqlCommand("SELECT [UserName] FROM [dbo].[User]", connection))
                    using (SqlDataReader reader = selCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine("{0}", reader.GetString(0));
                            if (reader.GetString(0).Trim() == userName)
                            {
                                bExistingUser = true;
                                break;
                            }
                        }

                        reader.Close();
                    }

                    if(bExistingUser)
                    {
                        string insertCmd = "DELETE FROM [dbo].[User] WHERE [UserName] = '" + userName + "'";
                        using (SqlCommand sqlcmd = new SqlCommand(insertCmd, connection))
                        {
                            int result = sqlcmd.ExecuteNonQuery();
                            Console.WriteLine("Deleted rows: {0}", result);
                            ret = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show(Trace.Constants.NO_USER);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return ret;
        }
        #endregion

        #region 查询结果数据

        public static List<TestResult> QueryFromResult(List<SearchParam> searchParams, bool bDateEnabled, DateTime startDate, DateTime endDate)
        {
            List<TestResult> results = new List<TestResult>();
            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    string sqlResult = "SELECT * FROM Result WHERE ";
                    if (bDateEnabled)
                    {
                        sqlResult += " TestDate >= '" + startDate.ToString() + "' and TestDate <= '" + endDate.ToString() + "' "; ;
                    }
                    int count = searchParams.Count;
                    int index = 0;

                    foreach (SearchParam searchParam in searchParams)
                    {
                        if (index == 0 && bDateEnabled)
                        {
                            sqlResult += "And ";
                        }
                        sqlResult += (searchParam.Field + " " + searchParam.Compare + " '" + searchParam.Value + "' ");

                        index++;

                        //Append "and"/"or"
                        if (index < count)
                        {
                            sqlResult += (searchParam.LogicOperation + " ");
                        }
                    }

                    Console.WriteLine("Select string: {0}", sqlResult);

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(sqlResult, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int intFieldCount = reader.FieldCount;
                        object[] objValues = new object[intFieldCount];
                        while (reader.Read())
                        {
                            reader.GetValues(objValues);

                            TestResult res = createTestResultFromResultTable(objValues);

                            results.Add(res);
                        }

                        reader.Close();
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return results;
        }

        private static TestResult createTestResultFromResultTable(object[] objValues)
        {
            TestResult tr = new TestResult
            {
                IdInDatabase = Convert.ToInt32(objValues[0]),
                TestID = Convert.ToInt32(objValues[1]),
                ID = Convert.ToInt32(objValues[2]),
                PositionX = Convert.ToInt32(objValues[3]),
                PositionY = Convert.ToInt32(objValues[4]),
                MeasureAngle = Convert.ToInt32(objValues[5]),
                Stop = Convert.ToInt32(objValues[6]),
                ObserveAngle = objValues[7].ToString(),
                Illuminant = objValues[8].ToString(),
                IntegrationTime_ms = Convert.ToInt32(objValues[9]),
                Average = Convert.ToInt32(objValues[10]),
                Smooth = Convert.ToInt32(objValues[11]),
                UpperL = Math.Round(Convert.ToDouble(objValues[12]), 1),
                UpperC = Math.Round(Convert.ToDouble(objValues[13]), 1),
                UpperH = Math.Round(Convert.ToDouble(objValues[14]), 1),
                LowerA = Math.Round(Convert.ToDouble(objValues[15]), 1),
                LowerB = Math.Round(Convert.ToDouble(objValues[16]), 1),
                UpperY = Math.Round(Convert.ToDouble(objValues[17]), 1),
                LowerX = Math.Round(Convert.ToDouble(objValues[18]), 4),
                LowerY = Math.Round(Convert.ToDouble(objValues[19]), 4),
                UpperR = Math.Round(Convert.ToDouble(objValues[20]), 1),
                UpperG = Math.Round(Convert.ToDouble(objValues[21]), 1),
                UpperB = Math.Round(Convert.ToDouble(objValues[22]), 1),
            };

            return tr;
        }

        public static List<TestResult> QueryFromTaskAndResult(List<SearchParam> searchParams, bool bDateEnabled, DateTime startDate, DateTime endDate)
        {
            List<TestResult> results = new List<TestResult>();
            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    string sqlResult = "SELECT * FROM Result LEFT JOIN Task ON Task.TaskID=Result.TaskID WHERE ";
                    if(bDateEnabled)
                    {
                        sqlResult += " TestTime >= '" + startDate.ToString() + "' and TestTime <= '" + endDate.ToString() + "' "; ;
                    }
                    int count = searchParams.Count;
                    int index = 0;

                    foreach (SearchParam se in searchParams)
                    {
                        if(index == 0 && bDateEnabled)
                        {
                            sqlResult += "And ";
                        }
                        sqlResult += (se.Field + " " + se.Compare + " '" + se.Value + "' ");

                        index++;
                        //Append "and"/"or"
                        if (index < count)
                            sqlResult += (se.LogicOperation + " ");
                    }

                    Console.WriteLine("Select string: {0}", sqlResult);

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(sqlResult, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int intFieldCount = reader.FieldCount;
                        object[] objValues = new object[intFieldCount];
                        while (reader.Read())
                        {
                            reader.GetValues(objValues);

                            TestResult res = createTestResult(objValues);

                            results.Add(res);
                        }

                        reader.Close();
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return results;
        }

        private static TestResult createTestResult(object[] objValues)
        {
            TestResult tr = new TestResult
            {
                ID = Convert.ToInt32(objValues[1]),
                TestID = Convert.ToInt32(objValues[2]),
                PositionX = Convert.ToInt32(objValues[3]),
                PositionY = Convert.ToInt32(objValues[4]),
                MeasureAngle = Convert.ToInt32(objValues[5]),
                Stop = Convert.ToInt32(objValues[6]),
                IntegrationTime_ms = Convert.ToInt32(objValues[7]),
                Average = Convert.ToInt32(objValues[8]),
                Smooth = Convert.ToInt32(objValues[9]),
                UpperL = Math.Round(Convert.ToDouble(objValues[10]), 1),
                UpperC = Math.Round(Convert.ToDouble(objValues[11]), 1),
                UpperH = Math.Round(Convert.ToDouble(objValues[12]), 1),
                LowerA = Math.Round(Convert.ToDouble(objValues[13]), 1),
                LowerB = Math.Round(Convert.ToDouble(objValues[14]), 1),
                UpperY = Math.Round(Convert.ToDouble(objValues[15]), 1),
                LowerX = Math.Round(Convert.ToDouble(objValues[16]), 4),
                LowerY = Math.Round(Convert.ToDouble(objValues[17]), 4),
                UpperR = Math.Round(Convert.ToDouble(objValues[18]), 1),
                UpperG = Math.Round(Convert.ToDouble(objValues[19]), 1),
                UpperB = Math.Round(Convert.ToDouble(objValues[20]), 1),
                TaskName = objValues[22].ToString(),
                GroupName = objValues[23].ToString(),
                ReelNumber = objValues[24].ToString(),
                ObserveAngle = objValues[25].ToString(),
                Illuminant = objValues[26].ToString(),

            };

            return tr;
        }

        public static List<string> QueryFromTask(string field, bool bDateEnabled, DateTime startDate, DateTime endDate)
        {
            List<string> results = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    string sqlResult = "SELECT DISTINCT " + field + " FROM [dbo].[Task]";
                    if (bDateEnabled)
                    {
                        sqlResult += "WHERE TestTime >= '" + startDate.ToString() + "' and TestTime <= '" + endDate.ToString() + "'";
                    }

                    Console.WriteLine("Select string: {0}", sqlResult);

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(sqlResult, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(reader.GetString(0).Trim());
                        }

                        reader.Close();
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return results;
        }

        public static List<TestTask> QueryFromTask(List<SearchParam> searchParams)
        {
            List<TestTask> results = new List<TestTask>();
            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    string sqlResult = "SELECT * FROM [dbo].[Task] WHERE ";
                    int count = searchParams.Count;
                    int index = 0;

                    foreach (SearchParam se in searchParams)
                    {

                        sqlResult += (se.Field + " " + se.Compare + " '" + se.Value + "' ");

                        index++;
                        //Append "and"/"or"
                        if (index < count)
                            sqlResult += (se.LogicOperation + " ");
                    }

                    Console.WriteLine("Select string: {0}", sqlResult);

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(sqlResult, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int intFieldCount = reader.FieldCount;
                        object[] objValues = new object[intFieldCount];
                        while (reader.Read())
                        {
                            reader.GetValues(objValues);
                            TestTask tt = new TestTask
                            {
                                TaskID = Convert.ToInt32(objValues[0]),
                                TaskName = objValues[1].ToString().Trim(),
                                GroupName = objValues[2].ToString().Trim(),
                                ReelNumber = objValues[3].ToString().Trim(),
                                ObserveAngle = objValues[4].ToString().Trim(),
                                LightSource = objValues[5].ToString().Trim()
                            };
                            results.Add(tt);
                        }

                        reader.Close();
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return results;
        }

        public static DataTable ConvertDataReaderToDataTable(SqlDataReader reader)
        {
            try
            {
                DataTable objDataTable = new DataTable();
                int intFieldCount = reader.FieldCount;
                for (int intCounter = 0; intCounter < intFieldCount; ++intCounter)
                {
                    objDataTable.Columns.Add(reader.GetName(intCounter), reader.GetFieldType(intCounter));
                }
                objDataTable.BeginLoadData();

                object[] objValues = new object[intFieldCount];
                while (reader.Read())
                {
                    reader.GetValues(objValues);
                    objDataTable.LoadDataRow(objValues, true);
                }
                reader.Close();
                objDataTable.EndLoadData();

                return objDataTable;

            }
            catch (Exception ex)
            {
                throw new Exception("转换出错!", ex);
            }

        }
        #endregion

        #region 保存数据

        public static int SaveTaskToDatabase(TestTask task)
        {
            int taskID = -1;

            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    connection.Open();

                    string command = "INSERT INTO [dbo].[Task] ([TaskName], [TestGroup], [ReelNumber], [ObserveAngle], [LightSource], [TestTime]) VALUES ('" 
                        + task.TaskName + "', '" + task.GroupName + "', '" + task.ReelNumber + "', '" + task.ObserveAngle + "', '" + task.LightSource + "', getdate())";
                    using (SqlCommand sqlcmd = new SqlCommand(command, connection))
                    {
                        sqlcmd.ExecuteNonQuery();
                    }

                    command = "select  @@IDENTITY as 'TaskID'";
                    using (SqlCommand sqlcmd = new SqlCommand(command, connection))
                    {
                        SqlDataReader reader = sqlcmd.ExecuteReader();
                        while (reader.Read())
                        {
                            taskID = int.Parse(reader[0].ToString());
                            System.Diagnostics.Trace.WriteLine("SaveTaskToDatabase, taskID = " + taskID);
                        }

                        reader.Close();
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return taskID;
        }

        public static bool QueryRefResultTables(int idInDataBase, ref List<double> results)
        {
            bool ret1 = QueryRefResultTable(1, idInDataBase, ref results);
            bool ret2 = QueryRefResultTable(2, idInDataBase, ref results);
            bool ret3 = QueryRefResultTable(3, idInDataBase, ref results);

            return ret1 && ret2 && ret3;
        }

        public static bool QueryRefResultTables(int taskID, int testID, ref List<double> results)
        {

            bool ret1 = QueryRefResultTable(1, taskID, testID, ref results);
            bool ret2 = QueryRefResultTable(2, taskID, testID, ref results);
            bool ret3 = QueryRefResultTable(3, taskID, testID, ref results);

            return ret1 && ret2 && ret3;
        }

        public static bool QueryRefResultTable(int tableID, int idInDataBase, ref List<double> results)
        {
            bool ret = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    connection.Open();

                    string sqlStr = "SELECT ";
                    if (tableID == 1)
                    {
                        for (int i = 0; i <= 999; i++)
                        {
                            if (i == 999)
                            {
                                sqlStr += "RefResult_1.data" + i;
                            }
                            else
                            {
                                sqlStr += "RefResult_1.data" + i + ", ";
                            }
                        }
                    }
                    else if (tableID == 2)
                    {
                        for (int i = 1000; i <= 1999; i++)
                        {
                            if (i == 1999)
                            {
                                sqlStr += "RefResult_2.data" + i;
                            }
                            else
                            {
                                sqlStr += "RefResult_2.data" + i + ", ";
                            }
                        }
                    }
                    else if (tableID == 3)
                    {
                        for (int i = 2000; i <= 2047; i++)
                        {
                            if (i == 2047)
                            {
                                sqlStr += "RefResult_3.data" + i;
                            }
                            else
                            {
                                sqlStr += "RefResult_3.data" + i + ", ";
                            }
                        }
                    }

                    sqlStr += " FROM [dbo].[RefResult_" + tableID + "] " + "WHERE ID = " + idInDataBase;

                    using (SqlCommand command = new SqlCommand(sqlStr, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (tableID == 1)
                            {
                                for (int i = 0; i <= 999; i++)
                                {
                                    results.Add(Math.Round(Convert.ToDouble(reader[i]), 1));
                                }
                            }
                            else if (tableID == 2)
                            {
                                for (int i = 0; i <= 999; i++)
                                {
                                    results.Add(Math.Round(Convert.ToDouble(reader[i]), 1));
                                }
                            }
                            else if (tableID == 3)
                            {
                                for (int i = 0; i <= 47; i++)
                                {
                                    results.Add(Math.Round(Convert.ToDouble(reader[i]), 1));
                                }
                            }
                        }

                        reader.Close();
                        ret = true;
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return ret;
        }

        public static bool QueryRefResultTable(int tableID, int taskID, int testID, ref List<double> results)
        {
            bool ret = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    connection.Open();
                    //SELECT* FROM RefResult_1 LEFT JOIN Task ON Task.TaskID = RefResult_1.TaskID
                    string sqlStr = "SELECT ";
                    if(tableID == 1)
                    {
                        for (int i = 0; i <= 999; i++)
                        {
                            if(i == 999)
                            {
                                sqlStr += "RefResult_1.data" + i;
                            }
                            else
                            {
                                sqlStr += "RefResult_1.data" + i + ", ";
                            }
                        }
                    }
                    else if(tableID == 2)
                    {
                        for (int i = 1000; i <= 1999; i++)
                        {
                            if(i == 1999)
                            {
                                sqlStr += "RefResult_2.data" + i;
                            }
                            else
                            {
                                sqlStr += "RefResult_2.data" + i + ", ";
                            }
                        }
                    }
                    else if(tableID == 3)
                    {
                        for (int i = 2000; i <= 2047; i++)
                        {
                            if(i == 2047)
                            {
                                sqlStr += "RefResult_3.data" + i;
                            }
                            else
                            {
                                sqlStr += "RefResult_3.data" + i + ", ";
                            }
                        }
                    }
                    
                    sqlStr += " FROM [dbo].[RefResult_" +
                        tableID + "] LEFT JOIN [dbo].[Result] ON Result.TaskID = RefResult_" + tableID + ".TaskID and Result.TestID = RefResult_" + tableID 
                        + ".TestID WHERE [dbo].[Result].TaskID = " + taskID + " and Result.TestID = " + testID;

                    //Console.WriteLine("{0}", sqlStr);
                    using (SqlCommand command = new SqlCommand(sqlStr, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if(tableID == 1)
                            {
                                for (int i = 0; i <= 999; i++)
                                {
                                    results.Add(Math.Round(Convert.ToDouble(reader[i]), 1));
                                }
                            }
                            else if(tableID == 2)
                            {
                                for (int i = 0; i <= 999; i++)
                                {
                                    results.Add(Math.Round(Convert.ToDouble(reader[i]), 1));
                                }
                            }
                            else if(tableID == 3)
                            {
                                for (int i = 0; i <= 47; i++)
                                {
                                    results.Add(Math.Round(Convert.ToDouble(reader[i]), 1));
                                }
                            }
                        }

                        reader.Close();
                        ret = true;
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return ret;
        }

        public static bool SaveRefsToDataBase(ObservableCollection<TestResult> results, int taskID)
        {
            bool ret1 = SaveRefToDatabase(results, taskID, "RefResult_1", 0, 999);
            bool ret2 = SaveRefToDatabase(results, taskID, "RefResult_2", 1000, 1999);
            bool ret3 = SaveRefToDatabase(results, taskID, "RefResult_3", 2000, 2047);

            return ret1 && ret2 && ret3;
        }

        public static bool SaveRefToDatabase(ObservableCollection<TestResult> results, int taskID, String tableName, int startRefIndex, int endRefIndex)
        {
            bool ret = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    connection.Open();

                    foreach(TestResult testResult in results)
                    {
                        string command = "INSERT INTO [dbo].[" + tableName + "] VALUES (" + taskID + ","
                            + testResult.ID + ",";
                        for (int i = startRefIndex; i <= endRefIndex; i++)
                        {
                            command += testResult.EnergyArray[i] + ",";
                        }

                        command = command.TrimEnd(',');
                        command += ")";

                        SqlCommand sqlcmd = new SqlCommand(command, connection);
                        int result = sqlcmd.ExecuteNonQuery();
                        ret = result > 0;
                        System.Diagnostics.Trace.WriteLine("SaveRefToDatabase" + startRefIndex + "-" + endRefIndex
                            + "Success:" + ret.ToString());
                    }

                    connection.Close();
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return ret;
        }

        public static bool SaveResultToDatabase(ObservableCollection<TestResult> results, int taskID)
        {
            bool ret = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    connection.Open();

                    foreach(TestResult testResult in results)
                    {
                        string command = "INSERT INTO [dbo].[Result] VALUES ("
                            + taskID + "," + testResult.ID + "," + testResult.PositionX + ","
                            + testResult.PositionY + "," + testResult.MeasureAngle + ","
                        + testResult.Stop + ",'" + testResult.ObserveAngle + "','" + testResult.Illuminant + "'," + testResult.IntegrationTime_ms + ","
                        + testResult.Average + "," + testResult.Smooth + ","+ testResult.UpperL + ","
                        + testResult.UpperC + "," + testResult.UpperH + "," + testResult.LowerA + ","
                        + testResult.LowerB + "," + testResult.UpperY + "," + testResult.LowerX + ","
                        + testResult.LowerY  + "," + testResult.UpperR + "," + testResult.UpperG + ","
                        + testResult.UpperB + ", getdate())";
                        SqlCommand sqlcmd = new SqlCommand(command, connection);
                        int result = sqlcmd.ExecuteNonQuery();
                        Console.WriteLine("Inserted rows: {0}", result);
                        ret = true;
                    }

                    connection.Close();
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return ret;
        }

        public static bool SaveCompenParamsToDatabase(List<string> compensationParams)
        {
            bool ret = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    connection.Open();

                    //插入1-1000
                    String command = GetInsertCompenParamSqlCmd(compensationParams, 0, 999);
                    SqlCommand sqlcmd = new SqlCommand(command, connection);
                    int result1 = sqlcmd.ExecuteNonQuery();
                    Console.WriteLine("Inserted rows(1-1000): {0}", command);

                    //插入1001-2000
                    command = GetInsertCompenParamSqlCmd(compensationParams, 1000, 1999);
                    sqlcmd = new SqlCommand(command, connection);
                    int result2 = sqlcmd.ExecuteNonQuery();
                    Console.WriteLine("Inserted rows(1000-2000): {0}", command);

                    //插入2001-2048
                    command = GetInsertCompenParamSqlCmd(compensationParams, 2000, 2047);
                    sqlcmd = new SqlCommand(command, connection);
                    int result3 = sqlcmd.ExecuteNonQuery();
                    Console.WriteLine("Inserted rows(2001-2048): {0}", command);

                    bool ret1 = Convert.ToBoolean(result1);
                    bool ret2 = Convert.ToBoolean(result2);
                    bool ret3 = Convert.ToBoolean(result3);

                    ret = ret1 && ret2 && ret3;

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return ret;
        }

        #endregion

        #region 查询数据

        //插入补偿参数至数据库
        private static String GetInsertCompenParamSqlCmd(List<string> compensationParams, int startIndex, int endIndex)
        {
            string command = "INSERT INTO [dbo].[reftable] ([ref]) VALUES";
            for (int i = startIndex; i <= endIndex; i++)
            {
                command += " (" + compensationParams[i] + "),";
            }
            command = command.TrimEnd(',');

            return command;
        }

        public static bool GetCompensateParamFromDatabase(ref List<string> list)
        {
            bool ret = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SELECT [REF]  FROM [dbo].[reftable]", connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(reader[0].ToString());
                        }

                        reader.Close();
                        Console.WriteLine("GetCompensateParamFromDatabase params count = {0}", list.Count);
                        ret = true;
                    }

                    connection.Close();
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return ret;
        }

        #endregion

        public static bool DeleteDataFromTable(String tableName)
        {
            bool ret = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(m_connectionString))
                {
                    connection.Open();

                    string command = "DELETE FROM [dbo].[" + tableName + "]";
                    using (SqlCommand sqlcmd = new SqlCommand(command, connection))
                    {
                        int result = sqlcmd.ExecuteNonQuery();
                        Console.WriteLine("DELETE DATA FROM DATA: {0}", tableName);
                        ret = Convert.ToBoolean(result);
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return ret;
        }
    }
}
