using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

using System.Data.SQLite;
using System.IO;


namespace AGLatency
{
    public partial class PrimaryLocalHarden : Form
    {
        DataTable table = new DataTable("TestTable");
        DateTime startTime = new DateTime();
        Dictionary<int, Dictionary<string, List<DataPoint>>> dataDict = new Dictionary<int, Dictionary<string, List<DataPoint>>>();
        int current_dbid = -1;
        public PrimaryLocalHarden()
        {
            InitializeComponent();
            Init();


        }

        public void Init()
        {
            // First set the ChartArea.InnerPlotPosition property.
            chart1.ChartAreas["Default"].InnerPlotPosition.Auto = true;
            chart1.ChartAreas[1].InnerPlotPosition.Auto = true;

            chart1.Series[0].ChartType = SeriesChartType.Line;
            chart1.Series[1].ChartType = SeriesChartType.Line;
            //chart1.Series[2].ChartType = SeriesChartType.Line;

            //  chart1.ChartAreas[0].AxisX.LabelStyle.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Seconds;
            //  chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;
            //chart1.ChartAre 
            // Zoom into the X axis
            //chart1.ChartAreas[0].AxisX.ScaleView.Zoom(1,2);
            // Enable range selection and zooming end user interface
            chart1.ChartAreas[0].AxisX.ScaleView.MinSize = 10; //10 seconds
            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            //chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;

            //chart1.ChartAreas[0].CursorY.IsUserEnabled = true;
            //chart1.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            //chart1.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            //chart1.ChartAreas[0].AxisY.ScrollBar.IsPositionedInside = true;

            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisX.MinorGrid.Enabled = false;

            chart1.ChartAreas[1].AxisX.ScaleView.MinSize = 10; //10 seconds
            chart1.ChartAreas[1].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[1].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[1].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[1].AxisX.ScrollBar.IsPositionedInside = true;
            chart1.ChartAreas[1].AxisY.MajorGrid.Enabled = false;
            chart1.ChartAreas[1].AxisY.MinorGrid.Enabled = false;
            chart1.ChartAreas[1].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[1].AxisX.MinorGrid.Enabled = false;

            chart1.ChartAreas[2].AxisX.ScaleView.MinSize = 10; //10 seconds
            chart1.ChartAreas[2].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[2].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[2].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[2].AxisX.ScrollBar.IsPositionedInside = true;
            chart1.ChartAreas[2].AxisY.MajorGrid.Enabled = false;
            chart1.ChartAreas[2].AxisY.MinorGrid.Enabled = false;
            chart1.ChartAreas[2].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[2].AxisX.MinorGrid.Enabled = false;

            chart1.ChartAreas[3].AxisX.ScaleView.MinSize = 10; //10 seconds
            chart1.ChartAreas[3].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[3].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[3].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[3].AxisX.ScrollBar.IsPositionedInside = true;
            chart1.ChartAreas[3].AxisY.MajorGrid.Enabled = false;
            chart1.ChartAreas[3].AxisY.MinorGrid.Enabled = false;
            chart1.ChartAreas[3].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[3].AxisX.MinorGrid.Enabled = false;



            // Set the alignment properties so the "Volume" chart area will allign to "Default"
            chart1.ChartAreas[1].AlignmentOrientation = AreaAlignmentOrientations.Vertical;
            chart1.ChartAreas[1].AlignmentStyle = AreaAlignmentStyles.All;
            chart1.ChartAreas[1].AlignWithChartArea = "Default";

            chart1.ChartAreas[2].AlignmentOrientation = AreaAlignmentOrientations.Vertical;
            chart1.ChartAreas[2].AlignmentStyle = AreaAlignmentStyles.All;
            chart1.ChartAreas[2].AlignWithChartArea = "Default";

            chart1.ChartAreas[3].AlignmentOrientation = AreaAlignmentOrientations.Vertical;
            chart1.ChartAreas[3].AlignmentStyle = AreaAlignmentStyles.All;
            chart1.ChartAreas[3].AlignWithChartArea = "Default";


            chart1.ChartAreas[0].Position.Height = 20;
            chart1.ChartAreas[1].Position.Height = 20;
            chart1.ChartAreas[2].Position.Height = 20;
            chart1.ChartAreas[3].Position.Height = 20;

            chart1.ChartAreas[0].Position.Width = 80;
            chart1.ChartAreas[1].Position.Width = 80;
            chart1.ChartAreas[2].Position.Width = 80;
            chart1.ChartAreas[3].Position.Width = 80;


            chart1.ChartAreas[0].Position.Y = 0;
            chart1.ChartAreas[1].Position.Y = chart1.ChartAreas[0].Position.Bottom;
            chart1.ChartAreas[2].Position.Y = chart1.ChartAreas[1].Position.Bottom;
            chart1.ChartAreas[3].Position.Y = chart1.ChartAreas[2].Position.Bottom;


        }


        public enum LogBlockFlushCounter
        {
            log_block_avg_flush_time,
            log_block_total_flush_time,
            total_log_blocks_flushed,
            total_write_size
        }
        private void BindData()
        {
            if (current_dbid == -1) current_dbid = int.Parse(listBox1.Items[listBox1.Items.Count - 1].ToString());
            int dbid = current_dbid;

            var lst = dataDict[dbid][LogBlockFlushCounter.log_block_avg_flush_time.ToString()];
            var lst2 = dataDict[dbid][LogBlockFlushCounter.log_block_total_flush_time.ToString()];
            var lst3 = dataDict[dbid][LogBlockFlushCounter.total_log_blocks_flushed.ToString()];
            var lst4 = dataDict[dbid][LogBlockFlushCounter.total_write_size.ToString()];

            DateTime dt = new DateTime(2013, 1, 1);

            List<List<DataPoint>> allLists = new List<List<DataPoint>>();
            allLists.Add(lst);
            allLists.Add(lst2);
            allLists.Add(lst3);
            allLists.Add(lst4);
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.Series[2].Points.Clear();
            chart1.Series[3].Points.Clear();

            for (int i = 0; i < allLists.Count; i++)
            {
                var dataList = allLists[i];

                for (int j = 0; j < dataList.Count; j++)
                {
                    DataPoint dp = dataList[j];
                    // chart1.Series[0].Points.AddXY(dt, 10+ r.Next(100) / 10);
                    chart1.Series[i].Points.Add(dp);



                    var dt_tmp = dt.AddSeconds(dp.XValue);
                    chart1.Series[i].Points[chart1.Series[i].Points.Count - 1].AxisLabel = dt_tmp.ToString("HH:mm:ss");


                  


                }

            }

             




        }

        private void FillData()
        {
            /*
            var dict =   Latency.LogBlockLocalHarden.GetPerfPointData();

            foreach (int dbid in dict.Keys)
            {
                //Ignore dbid=1 which is master
                if (dbid == 1) continue;
                Dictionary<string, List<DataPoint>> counterMap = new Dictionary<string, List<DataPoint>>();

                counterMap.Add(LogBlockFlushCounter.log_block_avg_flush_time.ToString(), new List<DataPoint>());
                counterMap.Add(LogBlockFlushCounter.log_block_total_flush_time.ToString(), new List<DataPoint>());
                counterMap.Add(LogBlockFlushCounter.total_log_blocks_flushed.ToString(), new List<DataPoint>());
                counterMap.Add(LogBlockFlushCounter.total_write_size.ToString(), new List<DataPoint>());

                dataDict.Add(dbid, counterMap);
                listBox1.Items.Add(dbid);
            }

            foreach (KeyValuePair<int, List<Latency.LogBlockFlush_Sec>> kv in dict)
            {
                if (kv.Key == 1) continue;
                Dictionary<string, List<DataPoint>> di = dataDict[kv.Key];

                foreach (Latency.LogBlockFlush_Sec pfp in kv.Value)
                {
                    di[LogBlockFlushCounter.log_block_avg_flush_time.ToString()].Add(new DataPoint(pfp.secondDistance, pfp.Avg_Duration));
                    di[LogBlockFlushCounter.log_block_total_flush_time.ToString()].Add(new DataPoint(pfp.secondDistance, pfp.Sum_Duration));
                    di[LogBlockFlushCounter.total_log_blocks_flushed.ToString()].Add(new DataPoint(pfp.secondDistance, pfp.LogBlocks));
                    di[LogBlockFlushCounter.total_write_size.ToString()].Add(new DataPoint(pfp.secondDistance, pfp.Sum_write_size));
                }



            }


    */

        }

        private void PrimaryLocalHarden_Load(object sender, EventArgs e)
        {
            FillData();
            BindData();
            chart1.Invalidate();
        }

        private void ZoomReset()
        {
            chart1.ChartAreas["Default"].AxisX.ScaleView.ZoomReset(0);
            chart1.ChartAreas["Default"].AxisY.ScaleView.ZoomReset(0);

        }
        private void btnResetZoom_Click(object sender, EventArgs e)
        {
            ZoomReset();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null) return;
            if ((int)listBox1.SelectedItem != current_dbid)
            {
                current_dbid = (int)listBox1.SelectedItem;
                BindData();
                ZoomReset();
                chart1.Invalidate();
            }
        }
    }
}
