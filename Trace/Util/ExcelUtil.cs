using System;
using System.Windows.Controls;
using Trace.Model;

namespace ExcelUtil
{
    class ExcelUtil
    {
        public static bool ExportToExcel(ListView listViewData)
        {
            try
            {
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.DefaultExt = "xls";
                sfd.Filter = "Excel文件(*.xls)|*.xls";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DoExport(listViewData, sfd.FileName);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                return false;
            }

            return true;
        }

        public static bool ExportToExcel_MainWindowNew(ListView listViewData)
        {
            try
            {
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.DefaultExt = "xls";
                sfd.Filter = "Excel文件(*.xls)|*.xls";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DoExport_MainWindowNew(listViewData, sfd.FileName);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                return false;
            }

            return true;
        }

        private static void DoExport(ListView listViewData, string strFileName)
        {
            Microsoft.Office.Interop.Excel.Application xla = new Microsoft.Office.Interop.Excel.Application();

            xla.Visible = false;
            xla.DisplayAlerts = false;
            xla.AlertBeforeOverwriting = false;

            Microsoft.Office.Interop.Excel.Workbook wb = xla.Workbooks.Add(Microsoft.Office.Interop.Excel.XlSheetType.xlWorksheet);

            Microsoft.Office.Interop.Excel.Worksheet ws = (Microsoft.Office.Interop.Excel.Worksheet)xla.ActiveSheet;

            GridViewColumnCollection cols = (listViewData.View as GridView).Columns;
            for (int col = 1; col < cols.Count; col++)
            {
                ws.Cells[1, col] = cols[col].Header.ToString();
            }

            int i = 2;
            foreach (TestResult res in listViewData.Items)
            {
                ws.Cells[i, 1] = res.TaskName;
                ws.Cells[i, 2] = res.GroupName;
                ws.Cells[i, 3] = res.ReelNumber;
                ws.Cells[i, 4] = res.ObserveAngle;
                ws.Cells[i, 5] = res.Illuminant;
                ws.Cells[i, 6] = res.PositionX;
                ws.Cells[i, 7] = res.PositionY;
                ws.Cells[i, 8] = res.MeasureAngle;
                ws.Cells[i, 9] = res.Stop;
                ws.Cells[i, 10] = res.IntegrationTime_ms;
                ws.Cells[i, 11] = res.Average;
                ws.Cells[i, 12] = res.Smooth;
                ws.Cells[i, 13] = res.UpperL;
                ws.Cells[i, 14] = res.UpperC;
                ws.Cells[i, 15] = res.UpperH;
                ws.Cells[i, 16] = res.LowerA;
                ws.Cells[i, 17] = res.LowerB;
                ws.Cells[i, 18] = res.UpperY;
                ws.Cells[i, 19] = res.LowerX;
                ws.Cells[i, 20] = res.LowerY;
                ws.Cells[i, 21] = res.UpperR;
                ws.Cells[i, 22] = res.UpperG;
                ws.Cells[i, 23] = res.UpperB;
                i++;
            }

            wb.SaveAs(strFileName, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);

            wb.Close();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(wb);

            xla.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(xla);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private static void DoExport_MainWindowNew(ListView listViewData, string strFileName)
        {
            Microsoft.Office.Interop.Excel.Application xla = new Microsoft.Office.Interop.Excel.Application();

            xla.Visible = false;
            xla.DisplayAlerts = false;
            xla.AlertBeforeOverwriting = false;

            Microsoft.Office.Interop.Excel.Workbook wb = xla.Workbooks.Add(Microsoft.Office.Interop.Excel.XlSheetType.xlWorksheet);

            Microsoft.Office.Interop.Excel.Worksheet ws = (Microsoft.Office.Interop.Excel.Worksheet)xla.ActiveSheet;

            GridViewColumnCollection cols = (listViewData.View as GridView).Columns;
            for (int col = 1; col < cols.Count; col++)
            {
                ws.Cells[1, col] = cols[col].Header.ToString();
            }

            int i = 2;
            foreach (TestResult res in listViewData.Items)
            {
                ws.Cells[i, 1] = res.ObserveAngle;
                ws.Cells[i, 2] = res.Illuminant;
                ws.Cells[i, 3] = res.PositionX;
                ws.Cells[i, 4] = res.PositionY;
                ws.Cells[i, 5] = res.MeasureAngle;
                ws.Cells[i, 6] = res.Stop;
                ws.Cells[i, 7] = res.IntegrationTime_ms;
                ws.Cells[i, 8] = res.Average;
                ws.Cells[i, 9] = res.Smooth;
                ws.Cells[i, 10] = res.UpperL;
                ws.Cells[i, 11] = res.UpperC;
                ws.Cells[i, 12] = res.UpperH;
                ws.Cells[i, 13] = res.LowerA;
                ws.Cells[i, 14] = res.LowerB;
                ws.Cells[i, 15] = res.UpperY;
                ws.Cells[i, 16] = res.LowerX;
                ws.Cells[i, 17] = res.LowerY;
                ws.Cells[i, 18] = res.UpperR;
                ws.Cells[i, 19] = res.UpperG;
                ws.Cells[i, 20] = res.UpperB;
                i++;
            }

            wb.SaveAs(strFileName, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);

            wb.Close();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(wb);

            xla.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(xla);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        //isSaveSelData代表是否只保存isSelected为true的结果
        public static bool ExportToExcel_MainWindow(ListView listViewData, bool isSaveSelData)
        {
            try
            {
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.DefaultExt = "xls";
                sfd.Filter = "Excel文件(*.xls)|*.xls";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DoExport_MainWindow(listViewData, sfd.FileName, isSaveSelData);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                return false;
            }

            return true;
        }

        //主窗口的保存函数
        private static void DoExport_MainWindow(ListView listViewData, string strFileName, bool isSaveSelData)
        {
            Microsoft.Office.Interop.Excel.Application xla = new Microsoft.Office.Interop.Excel.Application();

            xla.Visible = false;
            xla.DisplayAlerts = false;
            xla.AlertBeforeOverwriting = false;

            Microsoft.Office.Interop.Excel.Workbook wb = xla.Workbooks.Add(Microsoft.Office.Interop.Excel.XlSheetType.xlWorksheet);

            Microsoft.Office.Interop.Excel.Worksheet ws = (Microsoft.Office.Interop.Excel.Worksheet)xla.ActiveSheet;

            GridViewColumnCollection cols = (listViewData.View as GridView).Columns;
            for (int col = 1; col < cols.Count; col++)
            {
                ws.Cells[1, col] = cols[col].Header.ToString();
            }

            int i = 2;
            for (int index = listViewData.Items.Count - 1; index >= 0; index--)
            {
                TestResult res = (TestResult)listViewData.Items[index];

                if (isSaveSelData)
                {
                    if (!res.IsSelected)
                    {
                        continue;
                    }
                }

                ws.Cells[i, 1] = res.desc;
                ws.Cells[i, 2] = res.PositionX;
                ws.Cells[i, 3] = res.PositionY;
                ws.Cells[i, 4] = res.MeasureAngle;
                ws.Cells[i, 5] = res.Stop;
                ws.Cells[i, 6] = res.ObserveAngle;
                ws.Cells[i, 7] = res.Illuminant;
                ws.Cells[i, 8] = res.IntegrationTime_ms;
                ws.Cells[i, 9] = res.Average;
                ws.Cells[i, 10] = res.Smooth;
                ws.Cells[i, 11] = res.UpperL;
                ws.Cells[i, 12] = res.UpperC;
                ws.Cells[i, 13] = res.UpperH;
                ws.Cells[i, 14] = res.LowerA;
                ws.Cells[i, 15] = res.LowerB;
                ws.Cells[i, 16] = res.UpperY;
                ws.Cells[i, 17] = res.LowerX;
                ws.Cells[i, 18] = res.LowerY;
                ws.Cells[i, 19] = res.UpperR;
                ws.Cells[i, 20] = res.UpperG;
                ws.Cells[i, 21] = res.UpperB;
                i++;
            }

            wb.SaveAs(strFileName, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, 
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);

            wb.Close();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(wb);

            xla.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(xla);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
