namespace WindowsApplication1
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBox_devtype = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBox_Time1 = new System.Windows.Forms.TextBox();
            this.textBox_AccMask = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBox_Mode = new System.Windows.Forms.ComboBox();
            this.comboBox_Filter = new System.Windows.Forms.ComboBox();
            this.textBox_Time0 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_AccCode = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_CANIndex = new System.Windows.Forms.ComboBox();
            this.comboBox_DevIndex = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.button_StartCAN = new System.Windows.Forms.Button();
            this.button_StopCAN = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.CBoxBDSFrameFormat = new System.Windows.Forms.ComboBox();
            this.CBoxBDSFrameType = new System.Windows.Forms.ComboBox();
            this.CBoxMsgCounter = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.CBoxVCUDischarge = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.CBoxDischarge = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_BDSData = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.TxtBoxBDSFrameID = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.button_Send = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.listBox_Info = new System.Windows.Forms.ListBox();
            this.timer_rec = new System.Windows.Forms.Timer(this.components);
            this.button_Clear = new System.Windows.Forms.Button();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.CBoxBBIFrameFormat = new System.Windows.Forms.ComboBox();
            this.CBoxBBIFrameType = new System.Windows.Forms.ComboBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.textBox_BBIData = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.TxtBoxBBIFrameID = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.CBoxERDFrameFormat = new System.Windows.Forms.ComboBox();
            this.CBoxERDFrameType = new System.Windows.Forms.ComboBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.textBox_ERDData = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.TxtBoxERDFrameID = new System.Windows.Forms.TextBox();
            this.label24 = new System.Windows.Forms.Label();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.Rec_Btn = new System.Windows.Forms.Button();
            this.CBoxESDFrameFormat = new System.Windows.Forms.ComboBox();
            this.CBoxESDFrameType = new System.Windows.Forms.ComboBox();
            this.label25 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.textBox_ESDData = new System.Windows.Forms.TextBox();
            this.label27 = new System.Windows.Forms.Label();
            this.TxtBoxESDFrameID = new System.Windows.Forms.TextBox();
            this.label28 = new System.Windows.Forms.Label();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            this.panel1.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.groupBox11.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBox_devtype);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.comboBox_CANIndex);
            this.groupBox1.Controls.Add(this.comboBox_DevIndex);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(18, 18);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(719, 235);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "设备参数";
            // 
            // comboBox_devtype
            // 
            this.comboBox_devtype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_devtype.FormattingEnabled = true;
            this.comboBox_devtype.Items.AddRange(new object[] {
            "3",
            "4"});
            this.comboBox_devtype.Location = new System.Drawing.Point(76, 34);
            this.comboBox_devtype.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_devtype.MaxDropDownItems = 15;
            this.comboBox_devtype.Name = "comboBox_devtype";
            this.comboBox_devtype.Size = new System.Drawing.Size(180, 26);
            this.comboBox_devtype.TabIndex = 5;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(12, 40);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(53, 18);
            this.label14.TabIndex = 4;
            this.label14.Text = "类型:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.textBox_Time1);
            this.groupBox2.Controls.Add(this.textBox_AccMask);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.comboBox_Mode);
            this.groupBox2.Controls.Add(this.comboBox_Filter);
            this.groupBox2.Controls.Add(this.textBox_Time0);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.textBox_AccCode);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(15, 75);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(620, 116);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "初始化CAN参数";
            // 
            // textBox_Time1
            // 
            this.textBox_Time1.Enabled = false;
            this.textBox_Time1.Location = new System.Drawing.Point(327, 69);
            this.textBox_Time1.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_Time1.Name = "textBox_Time1";
            this.textBox_Time1.Size = new System.Drawing.Size(40, 28);
            this.textBox_Time1.TabIndex = 1;
            // 
            // textBox_AccMask
            // 
            this.textBox_AccMask.Location = new System.Drawing.Point(111, 69);
            this.textBox_AccMask.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_AccMask.Name = "textBox_AccMask";
            this.textBox_AccMask.Size = new System.Drawing.Size(103, 28);
            this.textBox_AccMask.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(228, 78);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(98, 18);
            this.label6.TabIndex = 0;
            this.label6.Text = "定时器1:0x";
            // 
            // comboBox_Mode
            // 
            this.comboBox_Mode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Mode.FormattingEnabled = true;
            this.comboBox_Mode.Items.AddRange(new object[] {
            "正常",
            "只听",
            "自测"});
            this.comboBox_Mode.Location = new System.Drawing.Point(476, 72);
            this.comboBox_Mode.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_Mode.Name = "comboBox_Mode";
            this.comboBox_Mode.Size = new System.Drawing.Size(103, 26);
            this.comboBox_Mode.TabIndex = 1;
            // 
            // comboBox_Filter
            // 
            this.comboBox_Filter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Filter.FormattingEnabled = true;
            this.comboBox_Filter.Items.AddRange(new object[] {
            "接收全部类型",
            "只接收标准帧",
            "只接收扩展帧"});
            this.comboBox_Filter.Location = new System.Drawing.Point(476, 32);
            this.comboBox_Filter.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_Filter.Name = "comboBox_Filter";
            this.comboBox_Filter.Size = new System.Drawing.Size(103, 26);
            this.comboBox_Filter.TabIndex = 1;
            // 
            // textBox_Time0
            // 
            this.textBox_Time0.Enabled = false;
            this.textBox_Time0.Location = new System.Drawing.Point(327, 28);
            this.textBox_Time0.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_Time0.Name = "textBox_Time0";
            this.textBox_Time0.Size = new System.Drawing.Size(40, 28);
            this.textBox_Time0.TabIndex = 1;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(420, 78);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 18);
            this.label8.TabIndex = 0;
            this.label8.Text = "模式:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(384, 38);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 18);
            this.label7.TabIndex = 0;
            this.label7.Text = "滤波方式:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 78);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 18);
            this.label5.TabIndex = 0;
            this.label5.Text = "屏蔽码:0x";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(228, 38);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 18);
            this.label4.TabIndex = 0;
            this.label4.Text = "定时器0:0x";
            // 
            // textBox_AccCode
            // 
            this.textBox_AccCode.Location = new System.Drawing.Point(111, 28);
            this.textBox_AccCode.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_AccCode.Name = "textBox_AccCode";
            this.textBox_AccCode.Size = new System.Drawing.Size(103, 28);
            this.textBox_AccCode.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 38);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 18);
            this.label3.TabIndex = 0;
            this.label3.Text = "验收码:0x";
            // 
            // comboBox_CANIndex
            // 
            this.comboBox_CANIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_CANIndex.FormattingEnabled = true;
            this.comboBox_CANIndex.Items.AddRange(new object[] {
            "0",
            "1"});
            this.comboBox_CANIndex.Location = new System.Drawing.Point(552, 34);
            this.comboBox_CANIndex.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_CANIndex.Name = "comboBox_CANIndex";
            this.comboBox_CANIndex.Size = new System.Drawing.Size(68, 26);
            this.comboBox_CANIndex.TabIndex = 1;
            // 
            // comboBox_DevIndex
            // 
            this.comboBox_DevIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_DevIndex.FormattingEnabled = true;
            this.comboBox_DevIndex.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3"});
            this.comboBox_DevIndex.Location = new System.Drawing.Point(357, 34);
            this.comboBox_DevIndex.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_DevIndex.Name = "comboBox_DevIndex";
            this.comboBox_DevIndex.Size = new System.Drawing.Size(60, 26);
            this.comboBox_DevIndex.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(446, 40);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 18);
            this.label2.TabIndex = 0;
            this.label2.Text = "第几路CAN:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(278, 38);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "索引号:";
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(26, 27);
            this.buttonConnect.Margin = new System.Windows.Forms.Padding(4);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(161, 35);
            this.buttonConnect.TabIndex = 2;
            this.buttonConnect.Text = "连接";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // button_StartCAN
            // 
            this.button_StartCAN.Location = new System.Drawing.Point(26, 81);
            this.button_StartCAN.Margin = new System.Windows.Forms.Padding(4);
            this.button_StartCAN.Name = "button_StartCAN";
            this.button_StartCAN.Size = new System.Drawing.Size(161, 34);
            this.button_StartCAN.TabIndex = 5;
            this.button_StartCAN.Text = "启动CAN";
            this.button_StartCAN.UseVisualStyleBackColor = true;
            this.button_StartCAN.Click += new System.EventHandler(this.button_StartCAN_Click);
            // 
            // button_StopCAN
            // 
            this.button_StopCAN.Location = new System.Drawing.Point(26, 134);
            this.button_StopCAN.Margin = new System.Windows.Forms.Padding(4);
            this.button_StopCAN.Name = "button_StopCAN";
            this.button_StopCAN.Size = new System.Drawing.Size(161, 36);
            this.button_StopCAN.TabIndex = 5;
            this.button_StopCAN.Text = "复位CAN";
            this.button_StopCAN.UseVisualStyleBackColor = true;
            this.button_StopCAN.Click += new System.EventHandler(this.button_StopCAN_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.CBoxBDSFrameFormat);
            this.groupBox3.Controls.Add(this.CBoxBDSFrameType);
            this.groupBox3.Controls.Add(this.CBoxMsgCounter);
            this.groupBox3.Controls.Add(this.label16);
            this.groupBox3.Controls.Add(this.CBoxVCUDischarge);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.CBoxDischarge);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.textBox_BDSData);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.TxtBoxBDSFrameID);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Location = new System.Drawing.Point(11, 7);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(700, 192);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "BDS";
            // 
            // CBoxBDSFrameFormat
            // 
            this.CBoxBDSFrameFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBoxBDSFrameFormat.FormattingEnabled = true;
            this.CBoxBDSFrameFormat.Items.AddRange(new object[] {
            "数据帧",
            "远程帧"});
            this.CBoxBDSFrameFormat.Location = new System.Drawing.Point(299, 23);
            this.CBoxBDSFrameFormat.Margin = new System.Windows.Forms.Padding(4);
            this.CBoxBDSFrameFormat.Name = "CBoxBDSFrameFormat";
            this.CBoxBDSFrameFormat.Size = new System.Drawing.Size(103, 26);
            this.CBoxBDSFrameFormat.TabIndex = 1;
            // 
            // CBoxBDSFrameType
            // 
            this.CBoxBDSFrameType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBoxBDSFrameType.FormattingEnabled = true;
            this.CBoxBDSFrameType.Items.AddRange(new object[] {
            "标准帧",
            "扩展帧"});
            this.CBoxBDSFrameType.Location = new System.Drawing.Point(104, 21);
            this.CBoxBDSFrameType.Margin = new System.Windows.Forms.Padding(4);
            this.CBoxBDSFrameType.Name = "CBoxBDSFrameType";
            this.CBoxBDSFrameType.Size = new System.Drawing.Size(103, 26);
            this.CBoxBDSFrameType.TabIndex = 1;
            // 
            // CBoxMsgCounter
            // 
            this.CBoxMsgCounter.FormattingEnabled = true;
            this.CBoxMsgCounter.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15"});
            this.CBoxMsgCounter.Location = new System.Drawing.Point(192, 122);
            this.CBoxMsgCounter.Name = "CBoxMsgCounter";
            this.CBoxMsgCounter.Size = new System.Drawing.Size(177, 26);
            this.CBoxMsgCounter.TabIndex = 5;
            // 
            // label16
            // 
            this.label16.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label16.Location = new System.Drawing.Point(-6, 125);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(194, 23);
            this.label16.TabIndex = 2;
            this.label16.Text = "消息计数器(bit56-59)";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CBoxVCUDischarge
            // 
            this.CBoxVCUDischarge.Enabled = false;
            this.CBoxVCUDischarge.FormattingEnabled = true;
            this.CBoxVCUDischarge.Items.AddRange(new object[] {
            "0",
            "1"});
            this.CBoxVCUDischarge.Location = new System.Drawing.Point(192, 88);
            this.CBoxVCUDischarge.Name = "CBoxVCUDischarge";
            this.CBoxVCUDischarge.Size = new System.Drawing.Size(177, 26);
            this.CBoxVCUDischarge.TabIndex = 4;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(220, 26);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(71, 18);
            this.label11.TabIndex = 0;
            this.label11.Text = "帧格式:";
            // 
            // CBoxDischarge
            // 
            this.CBoxDischarge.FormattingEnabled = true;
            this.CBoxDischarge.Items.AddRange(new object[] {
            "默认状态(0)",
            "用户启用放电(1)",
            "用户停止放电(2)",
            "预留(3)"});
            this.CBoxDischarge.Location = new System.Drawing.Point(192, 54);
            this.CBoxDischarge.Name = "CBoxDischarge";
            this.CBoxDischarge.Size = new System.Drawing.Size(177, 26);
            this.CBoxDischarge.TabIndex = 3;
            this.CBoxDischarge.SelectedIndexChanged += new System.EventHandler(this.CBoxDischargeChanged);
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(36, 90);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(150, 23);
            this.label15.TabIndex = 1;
            this.label15.Text = "VCU放电(bit12)";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(30, 27);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(71, 18);
            this.label10.TabIndex = 0;
            this.label10.Text = "帧类型:";
            // 
            // textBox_BDSData
            // 
            this.textBox_BDSData.Location = new System.Drawing.Point(192, 155);
            this.textBox_BDSData.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_BDSData.Name = "textBox_BDSData";
            this.textBox_BDSData.Size = new System.Drawing.Size(365, 28);
            this.textBox_BDSData.TabIndex = 1;
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(120, 155);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(64, 28);
            this.label13.TabIndex = 0;
            this.label13.Text = "数据:";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label9
            // 
            this.label9.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.Location = new System.Drawing.Point(7, 62);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(181, 18);
            this.label9.TabIndex = 0;
            this.label9.Text = "放电状态(bit0-1)";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TxtBoxBDSFrameID
            // 
            this.TxtBoxBDSFrameID.Location = new System.Drawing.Point(491, 21);
            this.TxtBoxBDSFrameID.Margin = new System.Windows.Forms.Padding(4);
            this.TxtBoxBDSFrameID.Name = "TxtBoxBDSFrameID";
            this.TxtBoxBDSFrameID.Size = new System.Drawing.Size(129, 28);
            this.TxtBoxBDSFrameID.TabIndex = 1;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(410, 26);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(71, 18);
            this.label12.TabIndex = 0;
            this.label12.Text = "帧ID:0x";
            // 
            // button_Send
            // 
            this.button_Send.Location = new System.Drawing.Point(563, 60);
            this.button_Send.Margin = new System.Windows.Forms.Padding(4);
            this.button_Send.Name = "button_Send";
            this.button_Send.Size = new System.Drawing.Size(129, 44);
            this.button_Send.TabIndex = 5;
            this.button_Send.Text = "发送";
            this.button_Send.UseVisualStyleBackColor = true;
            this.button_Send.Click += new System.EventHandler(this.button_Send_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.listBox_Info);
            this.groupBox4.Location = new System.Drawing.Point(18, 616);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox4.Size = new System.Drawing.Size(800, 323);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "BDS  BBI信息";
            // 
            // listBox_Info
            // 
            this.listBox_Info.FormattingEnabled = true;
            this.listBox_Info.ItemHeight = 18;
            this.listBox_Info.Location = new System.Drawing.Point(8, 24);
            this.listBox_Info.Margin = new System.Windows.Forms.Padding(4);
            this.listBox_Info.Name = "listBox_Info";
            this.listBox_Info.Size = new System.Drawing.Size(784, 292);
            this.listBox_Info.TabIndex = 0;
            // 
            // timer_rec
            // 
            this.timer_rec.Tick += new System.EventHandler(this.Timer_rec_Tick);
            // 
            // button_Clear
            // 
            this.button_Clear.Location = new System.Drawing.Point(26, 186);
            this.button_Clear.Margin = new System.Windows.Forms.Padding(4);
            this.button_Clear.Name = "button_Clear";
            this.button_Clear.Size = new System.Drawing.Size(161, 39);
            this.button_Clear.TabIndex = 8;
            this.button_Clear.Text = "清空列表";
            this.button_Clear.UseVisualStyleBackColor = true;
            this.button_Clear.Click += new System.EventHandler(this.button_Clear_Click);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.buttonConnect);
            this.groupBox6.Controls.Add(this.button_StartCAN);
            this.groupBox6.Controls.Add(this.button_Clear);
            this.groupBox6.Controls.Add(this.button_StopCAN);
            this.groupBox6.Location = new System.Drawing.Point(1397, 18);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(234, 235);
            this.groupBox6.TabIndex = 10;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "groupBox6";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.CBoxBBIFrameFormat);
            this.groupBox5.Controls.Add(this.CBoxBBIFrameType);
            this.groupBox5.Controls.Add(this.label18);
            this.groupBox5.Controls.Add(this.button_Send);
            this.groupBox5.Controls.Add(this.label20);
            this.groupBox5.Controls.Add(this.textBox_BBIData);
            this.groupBox5.Controls.Add(this.label21);
            this.groupBox5.Controls.Add(this.TxtBoxBBIFrameID);
            this.groupBox5.Controls.Add(this.label23);
            this.groupBox5.Location = new System.Drawing.Point(13, 207);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox5.Size = new System.Drawing.Size(700, 119);
            this.groupBox5.TabIndex = 11;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "BBI";
            // 
            // CBoxBBIFrameFormat
            // 
            this.CBoxBBIFrameFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBoxBBIFrameFormat.FormattingEnabled = true;
            this.CBoxBBIFrameFormat.Items.AddRange(new object[] {
            "数据帧",
            "远程帧"});
            this.CBoxBBIFrameFormat.Location = new System.Drawing.Point(299, 23);
            this.CBoxBBIFrameFormat.Margin = new System.Windows.Forms.Padding(4);
            this.CBoxBBIFrameFormat.Name = "CBoxBBIFrameFormat";
            this.CBoxBBIFrameFormat.Size = new System.Drawing.Size(103, 26);
            this.CBoxBBIFrameFormat.TabIndex = 1;
            // 
            // CBoxBBIFrameType
            // 
            this.CBoxBBIFrameType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBoxBBIFrameType.FormattingEnabled = true;
            this.CBoxBBIFrameType.Items.AddRange(new object[] {
            "标准帧",
            "扩展帧"});
            this.CBoxBBIFrameType.Location = new System.Drawing.Point(104, 21);
            this.CBoxBBIFrameType.Margin = new System.Windows.Forms.Padding(4);
            this.CBoxBBIFrameType.Name = "CBoxBBIFrameType";
            this.CBoxBBIFrameType.Size = new System.Drawing.Size(103, 26);
            this.CBoxBBIFrameType.TabIndex = 1;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(220, 26);
            this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(71, 18);
            this.label18.TabIndex = 0;
            this.label18.Text = "帧格式:";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(30, 27);
            this.label20.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(71, 18);
            this.label20.TabIndex = 0;
            this.label20.Text = "帧类型:";
            // 
            // textBox_BBIData
            // 
            this.textBox_BBIData.Location = new System.Drawing.Point(190, 70);
            this.textBox_BBIData.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_BBIData.Name = "textBox_BBIData";
            this.textBox_BBIData.Size = new System.Drawing.Size(365, 28);
            this.textBox_BBIData.TabIndex = 1;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(129, 73);
            this.label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(53, 18);
            this.label21.TabIndex = 0;
            this.label21.Text = "数据:";
            // 
            // TxtBoxBBIFrameID
            // 
            this.TxtBoxBBIFrameID.Location = new System.Drawing.Point(491, 21);
            this.TxtBoxBBIFrameID.Margin = new System.Windows.Forms.Padding(4);
            this.TxtBoxBBIFrameID.Name = "TxtBoxBBIFrameID";
            this.TxtBoxBBIFrameID.Size = new System.Drawing.Size(129, 28);
            this.TxtBoxBBIFrameID.TabIndex = 1;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(410, 26);
            this.label23.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(71, 18);
            this.label23.TabIndex = 0;
            this.label23.Text = "帧ID:0x";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.CBoxERDFrameFormat);
            this.groupBox7.Controls.Add(this.CBoxERDFrameType);
            this.groupBox7.Controls.Add(this.label17);
            this.groupBox7.Controls.Add(this.label19);
            this.groupBox7.Controls.Add(this.textBox_ERDData);
            this.groupBox7.Controls.Add(this.label22);
            this.groupBox7.Controls.Add(this.TxtBoxERDFrameID);
            this.groupBox7.Controls.Add(this.label24);
            this.groupBox7.Location = new System.Drawing.Point(752, 18);
            this.groupBox7.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox7.Size = new System.Drawing.Size(612, 119);
            this.groupBox7.TabIndex = 12;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "ERD";
            // 
            // CBoxERDFrameFormat
            // 
            this.CBoxERDFrameFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBoxERDFrameFormat.FormattingEnabled = true;
            this.CBoxERDFrameFormat.Items.AddRange(new object[] {
            "数据帧",
            "远程帧"});
            this.CBoxERDFrameFormat.Location = new System.Drawing.Point(299, 23);
            this.CBoxERDFrameFormat.Margin = new System.Windows.Forms.Padding(4);
            this.CBoxERDFrameFormat.Name = "CBoxERDFrameFormat";
            this.CBoxERDFrameFormat.Size = new System.Drawing.Size(103, 26);
            this.CBoxERDFrameFormat.TabIndex = 1;
            // 
            // CBoxERDFrameType
            // 
            this.CBoxERDFrameType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBoxERDFrameType.FormattingEnabled = true;
            this.CBoxERDFrameType.Items.AddRange(new object[] {
            "标准帧",
            "扩展帧"});
            this.CBoxERDFrameType.Location = new System.Drawing.Point(104, 21);
            this.CBoxERDFrameType.Margin = new System.Windows.Forms.Padding(4);
            this.CBoxERDFrameType.Name = "CBoxERDFrameType";
            this.CBoxERDFrameType.Size = new System.Drawing.Size(103, 26);
            this.CBoxERDFrameType.TabIndex = 1;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(220, 26);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(71, 18);
            this.label17.TabIndex = 0;
            this.label17.Text = "帧格式:";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(30, 27);
            this.label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(71, 18);
            this.label19.TabIndex = 0;
            this.label19.Text = "帧类型:";
            // 
            // textBox_ERDData
            // 
            this.textBox_ERDData.Location = new System.Drawing.Point(104, 70);
            this.textBox_ERDData.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_ERDData.Name = "textBox_ERDData";
            this.textBox_ERDData.Size = new System.Drawing.Size(377, 28);
            this.textBox_ERDData.TabIndex = 1;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(43, 75);
            this.label22.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(53, 18);
            this.label22.TabIndex = 0;
            this.label22.Text = "数据:";
            // 
            // TxtBoxERDFrameID
            // 
            this.TxtBoxERDFrameID.Location = new System.Drawing.Point(491, 21);
            this.TxtBoxERDFrameID.Margin = new System.Windows.Forms.Padding(4);
            this.TxtBoxERDFrameID.Name = "TxtBoxERDFrameID";
            this.TxtBoxERDFrameID.Size = new System.Drawing.Size(90, 28);
            this.TxtBoxERDFrameID.TabIndex = 1;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(410, 26);
            this.label24.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(71, 18);
            this.label24.TabIndex = 0;
            this.label24.Text = "帧ID:0x";
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.dataGridView1);
            this.groupBox8.Location = new System.Drawing.Point(825, 498);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(565, 441);
            this.groupBox8.TabIndex = 13;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "信息";
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(6, 19);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.RowTemplate.Height = 30;
            this.dataGridView1.Size = new System.Drawing.Size(553, 415);
            this.dataGridView1.TabIndex = 0;
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.dataGridView2);
            this.groupBox9.Location = new System.Drawing.Point(1397, 498);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(234, 441);
            this.groupBox9.TabIndex = 14;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "设备故障状态";
            // 
            // dataGridView2
            // 
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Location = new System.Drawing.Point(6, 19);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.RowHeadersVisible = false;
            this.dataGridView2.RowHeadersWidth = 62;
            this.dataGridView2.RowTemplate.Height = 30;
            this.dataGridView2.Size = new System.Drawing.Size(222, 415);
            this.dataGridView2.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox5);
            this.panel1.Location = new System.Drawing.Point(18, 260);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(719, 339);
            this.panel1.TabIndex = 15;
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.Rec_Btn);
            this.groupBox10.Controls.Add(this.CBoxESDFrameFormat);
            this.groupBox10.Controls.Add(this.CBoxESDFrameType);
            this.groupBox10.Controls.Add(this.label25);
            this.groupBox10.Controls.Add(this.label26);
            this.groupBox10.Controls.Add(this.textBox_ESDData);
            this.groupBox10.Controls.Add(this.label27);
            this.groupBox10.Controls.Add(this.TxtBoxESDFrameID);
            this.groupBox10.Controls.Add(this.label28);
            this.groupBox10.Location = new System.Drawing.Point(752, 145);
            this.groupBox10.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox10.Size = new System.Drawing.Size(612, 108);
            this.groupBox10.TabIndex = 16;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "ESD";
            // 
            // Rec_Btn
            // 
            this.Rec_Btn.Location = new System.Drawing.Point(489, 68);
            this.Rec_Btn.Margin = new System.Windows.Forms.Padding(4);
            this.Rec_Btn.Name = "Rec_Btn";
            this.Rec_Btn.Size = new System.Drawing.Size(92, 29);
            this.Rec_Btn.TabIndex = 6;
            this.Rec_Btn.Text = "发送";
            this.Rec_Btn.UseVisualStyleBackColor = true;
            this.Rec_Btn.Click += new System.EventHandler(this.Rec_Btn_Click);
            // 
            // CBoxESDFrameFormat
            // 
            this.CBoxESDFrameFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBoxESDFrameFormat.FormattingEnabled = true;
            this.CBoxESDFrameFormat.Items.AddRange(new object[] {
            "数据帧",
            "远程帧"});
            this.CBoxESDFrameFormat.Location = new System.Drawing.Point(299, 23);
            this.CBoxESDFrameFormat.Margin = new System.Windows.Forms.Padding(4);
            this.CBoxESDFrameFormat.Name = "CBoxESDFrameFormat";
            this.CBoxESDFrameFormat.Size = new System.Drawing.Size(103, 26);
            this.CBoxESDFrameFormat.TabIndex = 1;
            // 
            // CBoxESDFrameType
            // 
            this.CBoxESDFrameType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBoxESDFrameType.FormattingEnabled = true;
            this.CBoxESDFrameType.Items.AddRange(new object[] {
            "标准帧",
            "扩展帧"});
            this.CBoxESDFrameType.Location = new System.Drawing.Point(104, 21);
            this.CBoxESDFrameType.Margin = new System.Windows.Forms.Padding(4);
            this.CBoxESDFrameType.Name = "CBoxESDFrameType";
            this.CBoxESDFrameType.Size = new System.Drawing.Size(103, 26);
            this.CBoxESDFrameType.TabIndex = 1;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(220, 26);
            this.label25.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(71, 18);
            this.label25.TabIndex = 0;
            this.label25.Text = "帧格式:";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(30, 27);
            this.label26.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(71, 18);
            this.label26.TabIndex = 0;
            this.label26.Text = "帧类型:";
            // 
            // textBox_ESDData
            // 
            this.textBox_ESDData.Location = new System.Drawing.Point(104, 70);
            this.textBox_ESDData.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_ESDData.Name = "textBox_ESDData";
            this.textBox_ESDData.Size = new System.Drawing.Size(377, 28);
            this.textBox_ESDData.TabIndex = 1;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(43, 75);
            this.label27.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(53, 18);
            this.label27.TabIndex = 0;
            this.label27.Text = "数据:";
            // 
            // TxtBoxESDFrameID
            // 
            this.TxtBoxESDFrameID.Location = new System.Drawing.Point(491, 21);
            this.TxtBoxESDFrameID.Margin = new System.Windows.Forms.Padding(4);
            this.TxtBoxESDFrameID.Name = "TxtBoxESDFrameID";
            this.TxtBoxESDFrameID.Size = new System.Drawing.Size(90, 28);
            this.TxtBoxESDFrameID.TabIndex = 1;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(410, 26);
            this.label28.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(71, 18);
            this.label28.TabIndex = 0;
            this.label28.Text = "帧ID:0x";
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.listBox1);
            this.groupBox11.Location = new System.Drawing.Point(752, 268);
            this.groupBox11.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox11.Size = new System.Drawing.Size(879, 223);
            this.groupBox11.TabIndex = 17;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "ERD  ESD信息";
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 18;
            this.listBox1.Location = new System.Drawing.Point(8, 24);
            this.listBox1.Margin = new System.Windows.Forms.Padding(4);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(858, 184);
            this.listBox1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1643, 974);
            this.Controls.Add(this.groupBox11);
            this.Controls.Add(this.groupBox10);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox9);
            this.Controls.Add(this.groupBox8);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "USB CAN";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox9.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            this.panel1.ResumeLayout(false);
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.groupBox11.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.ComboBox comboBox_CANIndex;
        private System.Windows.Forms.ComboBox comboBox_DevIndex;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_Time1;
        private System.Windows.Forms.TextBox textBox_AccMask;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_Time0;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_AccCode;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_Mode;
        private System.Windows.Forms.ComboBox comboBox_Filter;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button_StartCAN;
        private System.Windows.Forms.Button button_StopCAN;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox CBoxBDSFrameFormat;
        private System.Windows.Forms.ComboBox CBoxBDSFrameType;
        private System.Windows.Forms.Button button_Send;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBox_BDSData;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox TxtBoxBDSFrameID;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ListBox listBox_Info;
        private System.Windows.Forms.Timer timer_rec;
        private System.Windows.Forms.ComboBox comboBox_devtype;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button button_Clear;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox CBoxMsgCounter;
        private System.Windows.Forms.ComboBox CBoxVCUDischarge;
        private System.Windows.Forms.ComboBox CBoxDischarge;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.ComboBox CBoxBBIFrameFormat;
        private System.Windows.Forms.ComboBox CBoxBBIFrameType;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox textBox_BBIData;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox TxtBoxBBIFrameID;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.ComboBox CBoxERDFrameFormat;
        private System.Windows.Forms.ComboBox CBoxERDFrameType;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox textBox_ERDData;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox TxtBoxERDFrameID;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.DataGridView dataGridView2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.ComboBox CBoxESDFrameFormat;
        private System.Windows.Forms.ComboBox CBoxESDFrameType;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.TextBox textBox_ESDData;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TextBox TxtBoxESDFrameID;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.GroupBox groupBox11;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button Rec_Btn;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}

