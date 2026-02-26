namespace MissionPlanner
{
    partial class NMEA_GPS_Connection
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.CB_auto_connect = new System.Windows.Forms.CheckBox();
            this.CMB_baudrate = new System.Windows.Forms.ComboBox();
            this.CMB_serialport = new System.Windows.Forms.ComboBox();
            this.BUT_connect = new MissionPlanner.Controls.MyButton();
            this.LBL_gpsStatus = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.CHK_use_home_operator_location = new System.Windows.Forms.CheckBox();
            this.LBL_use_home_operator_location_info = new System.Windows.Forms.Label();
            this.LBL_manual_override_info = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // CB_auto_connect
            // 
            this.CB_auto_connect.AutoSize = true;
            this.CB_auto_connect.Location = new System.Drawing.Point(303, 42);
            this.CB_auto_connect.Name = "CB_auto_connect";
            this.CB_auto_connect.Size = new System.Drawing.Size(48, 17);
            this.CB_auto_connect.TabIndex = 46;
            this.CB_auto_connect.Text = "自动";
            this.CB_auto_connect.UseVisualStyleBackColor = true;
            this.CB_auto_connect.CheckedChanged += new System.EventHandler(this.CB_auto_connect_CheckedChanged);
            // 
            // CMB_baudrate
            // 
            this.CMB_baudrate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CMB_baudrate.FormattingEnabled = true;
            this.CMB_baudrate.Items.AddRange(new object[] {
            "4800",
            "9600",
            "14400",
            "19200",
            "28800",
            "38400",
            "57600",
            "115200"});
            this.CMB_baudrate.Location = new System.Drawing.Point(133, 18);
            this.CMB_baudrate.Name = "CMB_baudrate";
            this.CMB_baudrate.Size = new System.Drawing.Size(97, 21);
            this.CMB_baudrate.TabIndex = 45;
            // 
            // CMB_serialport
            // 
            this.CMB_serialport.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CMB_serialport.FormattingEnabled = true;
            this.CMB_serialport.Location = new System.Drawing.Point(6, 19);
            this.CMB_serialport.Name = "CMB_serialport";
            this.CMB_serialport.Size = new System.Drawing.Size(121, 21);
            this.CMB_serialport.TabIndex = 44;
            this.CMB_serialport.Enter += new System.EventHandler(this.CMB_serialport_Enter);
            // 
            // BUT_connect
            // 
            this.BUT_connect.Location = new System.Drawing.Point(236, 19);
            this.BUT_connect.Name = "BUT_connect";
            this.BUT_connect.Size = new System.Drawing.Size(115, 20);
            this.BUT_connect.TabIndex = 43;
            this.BUT_connect.Text = "连接基站定位";
            this.BUT_connect.UseVisualStyleBackColor = true;
            this.BUT_connect.Click += new System.EventHandler(this.BUT_connect_Click);
            // 
            // LBL_gpsStatus
            // 
            this.LBL_gpsStatus.AutoSize = false;
            this.LBL_gpsStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LBL_gpsStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LBL_gpsStatus.Location = new System.Drawing.Point(3, 43);
            this.LBL_gpsStatus.Name = "LBL_gpsStatus";
            this.LBL_gpsStatus.Size = new System.Drawing.Size(295, 36);
            this.LBL_gpsStatus.TabIndex = 42;
            this.LBL_gpsStatus.Text = "尚未启动                                                                  " +
    "                              ";
            this.LBL_gpsStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LBL_gpsStatus.DoubleClick += new System.EventHandler(this.LBL_gpsStatus_DoubleClick);
            this.LBL_gpsStatus.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LBL_gpsStatus_MouseUp);

            // 
            // CHK_use_home_operator_location
            // 
            this.CHK_use_home_operator_location.AutoSize = true;
            this.CHK_use_home_operator_location.Location = new System.Drawing.Point(6, 60);
            this.CHK_use_home_operator_location.Name = "CHK_use_home_operator_location";
            this.CHK_use_home_operator_location.Size = new System.Drawing.Size(155, 17);
            this.CHK_use_home_operator_location.TabIndex = 47;
            this.CHK_use_home_operator_location.Text = "使用起飞点作为飞手位置";
            this.CHK_use_home_operator_location.UseVisualStyleBackColor = true;
            this.CHK_use_home_operator_location.CheckedChanged += new System.EventHandler(this.CHK_use_home_operator_location_CheckedChanged);

            // 
            // LBL_use_home_operator_location_info
            // 
            this.LBL_use_home_operator_location_info.AutoSize = true;
            this.LBL_use_home_operator_location_info.Location = new System.Drawing.Point(6, 77);
            this.LBL_use_home_operator_location_info.Name = "LBL_use_home_operator_location_info";
            this.LBL_use_home_operator_location_info.Size = new System.Drawing.Size(347, 26);
            this.LBL_use_home_operator_location_info.TabIndex = 48;
            this.LBL_use_home_operator_location_info.Text = "功能逻辑：勾选后，将无人机 Home 点(起飞位置)坐标映射为飞手(地面站)实时位置。\r\n适用于飞手基本保持在起飞位置、不发生明显移动的场景，可省去连接地面站GPS硬件。";

            // 
            // LBL_manual_override_info
            // 
            this.LBL_manual_override_info.AutoSize = true;
            this.LBL_manual_override_info.Location = new System.Drawing.Point(6, 103);
            this.LBL_manual_override_info.Name = "LBL_manual_override_info";
            this.LBL_manual_override_info.Size = new System.Drawing.Size(338, 26);
            this.LBL_manual_override_info.TabIndex = 49;
            this.LBL_manual_override_info.Text = "如当前环境无 GPS 信号/无法硬件定位，可右键“地面站定位”状态栏弹出坐标输入框，\r\n手动输入经度、纬度和高度，以人工指定虚拟坐标用于远程ID信息上报及相关功能。";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.LBL_manual_override_info);
            this.groupBox1.Controls.Add(this.LBL_use_home_operator_location_info);
            this.groupBox1.Controls.Add(this.CHK_use_home_operator_location);
            this.groupBox1.Controls.Add(this.BUT_connect);
            this.groupBox1.Controls.Add(this.CB_auto_connect);
            this.groupBox1.Controls.Add(this.LBL_gpsStatus);
            this.groupBox1.Controls.Add(this.CMB_baudrate);
            this.groupBox1.Controls.Add(this.CMB_serialport);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(360, 132);
            this.groupBox1.TabIndex = 47;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "地面站定位";
            // 
            // NMEA_GPS_Connection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "NMEA_GPS_Connection";
            this.Size = new System.Drawing.Size(369, 140);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox CB_auto_connect;
        private System.Windows.Forms.ComboBox CMB_baudrate;
        private System.Windows.Forms.ComboBox CMB_serialport;
        private Controls.MyButton BUT_connect;
        private System.Windows.Forms.Label LBL_gpsStatus;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox CHK_use_home_operator_location;
        private System.Windows.Forms.Label LBL_use_home_operator_location_info;
        private System.Windows.Forms.Label LBL_manual_override_info;
    }
}
