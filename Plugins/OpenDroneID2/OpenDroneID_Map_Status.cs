using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissionPlanner.Controls
{
    public partial class OpenDroneID_Map_Status : UserControl
    {

        public static OpenDroneID_Map_Status Instance;
        public OpenDroneID_UI _parent_ODID { get; set; } = null;

        public OpenDroneID_Map_Status()
        {
            Instance = this;
            

            InitializeComponent();
        }

        public void setStatusOK()
        {
            LED_ODID_Status.Color = Color.Green;
            LBL_ODID_OK.Text = "远程ID正常";
            LBL_ODID_reason.Text = "双击 -> 紧急";
        }

        public void setStatusAlert(string alertReason)
        {
            LED_ODID_Status.Color = Color.Red;
            LBL_ODID_OK.Text = "远程ID失败";
            LBL_ODID_reason.Text = alertReason;
        }

        public void setStatusEmergency(string alertReason)
        {
            LED_ODID_Status.Color = Color.Red;
            //LED_ODID_Status.Blink(200); 
            LBL_ODID_OK.Text = "远程ID紧急";
            LBL_ODID_reason.Text = alertReason;
        }

        private void OpenDroneID_Map_Status_DoubleClick(object sender, EventArgs e)
        {
            if (CustomMessageBox.Show("确定要声明远程ID紧急状态吗？", "远程ID紧急？", CustomMessageBox.MessageBoxButtons.YesNo) == CustomMessageBox.DialogResult.Yes)
            {
                try
                {
                    if (_parent_ODID != null)
                     _parent_ODID.setEmergencyFromMap();
                } catch
                {
                    Console.WriteLine("远程ID - 从地图设置紧急状态失败");
                }
            }
        }

    }

    
}
