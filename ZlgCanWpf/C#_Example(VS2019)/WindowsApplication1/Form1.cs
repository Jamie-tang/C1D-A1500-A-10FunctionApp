using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Runtime.ConstrainedExecution;
using static System.Windows.Forms.AxHost;

/*------------兼容ZLG的数据类型---------------------------------*/

//1.ZLGCAN系列接口卡信息的数据类型。
//public struct VCI_BOARD_INFO 
//{ 
//    public UInt16 hw_Version;
//    public UInt16 fw_Version;
//    public UInt16 dr_Version;
//    public UInt16 in_Version;
//    public UInt16 irq_Num;
//    public byte   can_Num;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst=20)] public byte []str_Serial_Num;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
//    public byte[] str_hw_Type;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
//    public byte[] Reserved;
//}

//以下为简易定义与调用方式，在项目属性->生成->勾选使用不安全代码即可
unsafe public struct VCI_BOARD_INFO//使用不安全代码
{
    public UInt16 hw_Version;
    public UInt16 fw_Version;
    public UInt16 dr_Version;
    public UInt16 in_Version;
    public UInt16 irq_Num;
    public byte can_Num;

    public fixed byte str_Serial_Num[20];
    public fixed byte str_hw_Type[40];
    public fixed byte Reserved[8];
}

/////////////////////////////////////////////////////
//2.定义CAN信息帧的数据类型。
unsafe public struct VCI_CAN_OBJ  //使用不安全代码
{
    public uint ID;
    public uint TimeStamp;        //时间标识
    public byte TimeFlag;         //是否使用时间标识
    public byte SendType;         //发送标志。保留，未用
    public byte RemoteFlag;       //是否是远程帧
    public byte ExternFlag;       //是否是扩展帧
    public byte DataLen;          //数据长度
    public fixed byte Data[8];    //数据
    public fixed byte Reserved[3];//保留位

}

//3.定义初始化CAN的数据类型
public struct VCI_INIT_CONFIG 
{
    public UInt32 AccCode;
    public UInt32 AccMask;
    public UInt32 Reserved;
    public byte Filter;   //0或1接收所有帧。2标准帧滤波，3是扩展帧滤波。
    public byte Timing0;  //波特率参数，具体配置，请查看二次开发库函数说明书。
    public byte Timing1;
    public byte Mode;     //模式，0表示正常模式，1表示只听模式,2自测模式
}

/*------------其他数据结构描述---------------------------------*/
//4.USB-CAN总线适配器板卡信息的数据类型1，该类型为VCI_FindUsbDevice函数的返回参数。
public struct VCI_BOARD_INFO1
{
    public UInt16 hw_Version;
    public UInt16 fw_Version;
    public UInt16 dr_Version;
    public UInt16 in_Version;
    public UInt16 irq_Num;
    public byte can_Num;
    public byte Reserved;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)] public byte []str_Serial_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] str_hw_Type;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] str_Usb_Serial;
}

/*------------数据结构描述完成---------------------------------*/

public struct CHGDESIPANDPORT 
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public byte[] szpwd;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] szdesip;
    public Int32 desport;

    public void Init()
    {
        szpwd = new byte[10];
        szdesip = new byte[20];
    }
}


namespace WindowsApplication1
{
    public partial class Form1 : Form
    {
        const int DEV_USBCAN = 3;
        const int DEV_USBCAN2 = 4;

        ulong BDSData = 0;
         /// <summary>
        /// 
        /// </summary>
        /// <param name="DeviceType"></param>
        /// <param name="DeviceInd"></param>
        /// <param name="Reserved"></param>
        /// <returns></returns>
        /*------------兼容ZLG的函数描述---------------------------------*/
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);
        
        /*------------其他函数描述---------------------------------*/

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ConnectDevice(UInt32 DevType,UInt32 DevIndex);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_UsbDeviceReset(UInt32 DevType,UInt32 DevIndex,UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_FindUsbDevice2(ref VCI_BOARD_INFO pInfo);
        /*------------函数描述结束---------------------------------*/

        enum CANCommunicationState
        {
            CAN_COMM_STATE_FAILED = 0,
            CAN_COMM_STATE_SUCCEED = 1,
            CAN_COMM_STATE_NOTFOUNT = -1,
        }

        static UInt32 m_devtype = 4;//USBCAN2

        Int32 m_bOpen = 0;
        UInt32 m_canind = 0;
        UInt32 m_devind = 0;
        VCI_CAN_OBJ[] m_recobj = new VCI_CAN_OBJ[1000];
        UInt32[] m_arrdevtype = new UInt32[20];
        IniConfig iniConfig;
        private float X; private float Y;
        private string[]  ViewList= 
            {
                "充电器版本",             //1
                "最大放电电流",          //2
                "最低放电电流",           //3
                "最高放电电压",           //4
                "充电器请求放电指令",    //5
                "电子锁状态",    //6
                "充电器状态",//7
                "累计放电量(kwh)",//8
                "累计放电时间(min)",//9
                "实时对外放电功率(kw)",//10
                "充电器内部温度(℃)",//11
                "ESD消息计数器"//12
            };

        private string[] ProtectionList =
        {
            "输出OCP",
            "输出SCP",
            "OTP",
            "输出OVP",
            "输出UVP",
            "输入OVP",
            "输入UVP",
            "锁枪故障",
            "CAN通讯中断",
            "充电器内部故障"
        };

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string iniPath = Application.StartupPath + "\\Config.ini";
            iniConfig = new IniConfig(iniPath);
            iniConfig.ReadIniFlie();
            getAllInitData();
            //
            comboBox_DevIndex.SelectedIndex = 0;
            comboBox_DevIndex.SelectedIndex = 0;
            comboBox_CANIndex.SelectedIndex = 0;
            textBox_AccCode.Text = "00000000";
            textBox_AccMask.Text = "FFFFFFFF";
            textBox_Time0.Text = "01";
            textBox_Time1.Text = "1C";
            comboBox_Filter.SelectedIndex = 0;              //接收所有类型
            comboBox_Mode.SelectedIndex = 2;                //还回测试模式
            getBDSData();
            //
            Int32 curindex = 0;
            comboBox_devtype.Items.Clear();

            curindex = comboBox_devtype.Items.Add("DEV_USBCAN");
            m_arrdevtype[curindex] =  DEV_USBCAN;
            //comboBox_devtype.Items[2] = "VCI_USBCAN1";
            //m_arrdevtype[2]=  VCI_USBCAN1 ;

            curindex = comboBox_devtype.Items.Add("DEV_USBCAN2");
            m_arrdevtype[curindex] = DEV_USBCAN2 ;
            //comboBox_devtype.Items[3] = "VCI_USBCAN2";
            //m_arrdevtype[3]=  VCI_USBCAN2 ;

             comboBox_devtype.SelectedIndex = 1;
             comboBox_devtype.MaxDropDownItems = comboBox_devtype.Items.Count;

            DataTable dt = new DataTable();
            dt.Columns.Add("Item");
            dt.Columns.Add("Read Data");
            dt.Columns.Add("Raw Data");

            for (int i = 0; i < ViewList.Length; i++)
            {
                dt.Rows.Add(ViewList[i]);
            }
            dataGridView1.DataSource = dt;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("状态信息");
            dt2.Columns.Add("Bit");
            for (int i = 0; i < ProtectionList.Length; i++)
            {
                dt2.Rows.Add(ProtectionList[i]);
            }
            dataGridView2.DataSource = dt2;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            for (int i = 0; i < 2; i++)
            {
                dataGridView2.AutoResizeColumn(i);
            }
            dataGridView2.AutoResizeRows();
            X = this.Width;//获取窗体的宽度
            Y = this.Height;//获取窗体的高度
            setTag(this);//调用方法
            this.Resize += new EventHandler(MainFormResize);//窗体调整大小时引发事件
            //for (int i = 0; i < dataGridView1.Rows.Count; i++)
            //{
            //    dataGridView1.RowTemplate.Height = (DataView.Size.Height / dataGridView1.Rows.Count);
            //}
            //for (int i = 0; i < dataGridView2.Rows.Count; i++)
            //{
            //    dataGridView2.RowTemplate.Height = Status.Size.Height / dataGridView1.Rows.Count;
            //}
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable; // 禁用排序
            }
            foreach (DataGridViewColumn column in dataGridView2.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable; // 禁用排序
            }
        }
        private void MainFormResize(object sender, EventArgs e)
        {
            float newx = (this.Width) / X; //窗体宽度缩放比例
            float newy = this.Height / Y;//窗体高度缩放比例
            setControls(newx, newy, this);//随窗体改变控件大小
                                          //this.Text = this.Width.ToString() + " " + this.Height.ToString();//窗体标题栏文本
            //for (int i = 0; i < dataGridView1.Rows.Count; i++)
            //{
            //    dataGridView1.Rows[i].Height = DataView.Size.Height / dataGridView1.Rows.Count;
            //}
        }
        /// <summary>
        /// 遍历所有控件 用于Resize
        /// </summary>
        /// <param name="cons"></param>
        private void setTag(Control cons)
        {
            //遍历窗体中的控件
            foreach (Control con in cons.Controls)
            {
                con.Tag = con.Width + ":" + con.Height + ":" + con.Left + ":" + con.Top + ":" + con.Font.Size;
                if (con.Controls.Count > 0)
                    setTag(con);
            }
        }

        /// <summary>
        /// 根据窗体大小调整控件大小
        /// </summary>
        /// <param name="newx">x轴</param>
        /// <param name="newy">y轴</param>
        /// <param name="cons">各控件</param>
        private void setControls(float newx, float newy, Control cons)
        {
            try
            {
                //遍历窗体中的控件，重新设置控件的值
                foreach (Control con in cons.Controls)
                {
                    string[] mytag = con.Tag.ToString().Split(new char[] { ':' });//获取控件的Tag属性值，并分割后存储字符串数组
                    float a = Convert.ToSingle(mytag[0]) * newx;//根据窗体缩放比例确定控件的值，宽度
                    con.Width = (int)a;//宽度
                    a = Convert.ToSingle(mytag[1]) * newy;//高度
                    con.Height = (int)(a);
                    a = Convert.ToSingle(mytag[2]) * newx;//左边距离
                    con.Left = (int)(a);
                    a = Convert.ToSingle(mytag[3]) * newy;//上边缘距离
                    con.Top = (int)(a);
                    Single currentSize = Convert.ToSingle(mytag[4]) * newy;//字体大小
                    con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                    if (con.Controls.Count > 0)
                    {
                        setControls(newx, newy, con);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_bOpen==1)
            {
                VCI_CloseDevice(m_devtype, m_devind);
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {

            //以下两行为多卡同机测试代码，用来获取序列号与对应的设备索引号，单卡可以不使用。
            VCI_BOARD_INFO[] vbi2 = new VCI_BOARD_INFO[50];
            uint num1 = VCI_FindUsbDevice2(ref vbi2[0]);


            if (m_bOpen==1)
            {
                VCI_CloseDevice(m_devtype, m_devind);
                m_bOpen = 0;
            }
            else
            {
                m_devtype = m_arrdevtype[comboBox_devtype.SelectedIndex];

                m_devind=(UInt32)comboBox_DevIndex.SelectedIndex;
                m_canind = (UInt32)comboBox_CANIndex.SelectedIndex;
                int openSucessed = 0;
                openSucessed = (int)VCI_OpenDevice(m_devtype, m_devind, 0);
                if (openSucessed == 0)
                {
                    MessageBox.Show("打开设备失败,请检查设备类型和设备索引号是否正确", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //return;
                }

                //m_bOpen = 1;
                VCI_INIT_CONFIG config=new VCI_INIT_CONFIG();
                config.AccCode=System.Convert.ToUInt32("0x" + textBox_AccCode.Text,16);
                config.AccMask = System.Convert.ToUInt32("0x" + textBox_AccMask.Text, 16);
                config.Timing0 = System.Convert.ToByte("0x" + textBox_Time0.Text, 16);
                config.Timing1 = System.Convert.ToByte("0x" + textBox_Time1.Text, 16);
                config.Filter = (Byte)(comboBox_Filter.SelectedIndex+1);
                config.Mode = (Byte)comboBox_Mode.SelectedIndex;
                switch (VCI_InitCAN(m_devtype, m_devind, m_canind, ref config))
                { 
                    case 0:
                        MessageBox.Show("初始化CAN通道失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        m_bOpen = 0;
                        break;
                    case 1:
                        MessageBox.Show("初始化CAN通道成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        m_bOpen = 1;
                        break;
                    default:
                        MessageBox.Show("USB-CAN设备不存在或已掉线", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        m_bOpen = -1;
                        break;
                }

            }
            buttonConnect.Text = m_bOpen==1?"断开":"连接";
            timer_rec.Enabled = true;//m_bOpen==1?true:false;
        }

        unsafe private void Timer_rec_Tick(object sender, EventArgs e)
        {

            ////////////////////////////////////////////////////////
            //if (res == 0xFFFFFFFF) res = 0;//当设备未初始化时，返回0xFFFFFFFF，不进行列表显示。
            String str = "";
            //for (UInt32 i = 0; i < res; i++)
            //{
                //VCI_CAN_OBJ obj = (VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(VCI_CAN_OBJ))), typeof(VCI_CAN_OBJ));
                str = "接收到数据: ";
            str += "  帧ID:0x";// + System.Convert.ToString(m_recobj[i].ID, 16);
                str += "  帧格式:";
                //if (m_recobj[i].RemoteFlag == 0)
                    str += "数据帧 ";
                //else
                //    str += "远程帧 ";
                //if (m_recobj[i].ExternFlag == 0)
                //    str += "标准帧 ";
                //else
                //    str += "扩展帧 ";

                //////////////////////////////////////////
                //if (m_recobj[i].RemoteFlag == 0)
                //{
                //    str += "数据: ";
                //    byte len = (byte)(m_recobj[i].DataLen % 9);
                //    byte j = 0;
                //    fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
                //    {
                //        if (j++ < len)
                //            str += " " + System.Convert.ToString(m_recobj1->Data[0], 16);
                //        if (j++ < len)
                //            str += " " + System.Convert.ToString(m_recobj1->Data[1], 16);
                //        if (j++ < len)
                //            str += " " + System.Convert.ToString(m_recobj1->Data[2], 16);
                //        if (j++ < len)
                //            str += " " + System.Convert.ToString(m_recobj1->Data[3], 16);
                //        if (j++ < len)
                //            str += " " + System.Convert.ToString(m_recobj1->Data[4], 16);
                //        if (j++ < len)
                //            str += " " + System.Convert.ToString(m_recobj1->Data[5], 16);
                //        if (j++ < len)
                //            str += " " + System.Convert.ToString(m_recobj1->Data[6], 16);
                //        if (j++ < len)
                //            str += " " + System.Convert.ToString(m_recobj1->Data[7], 16);
                //    }
                //}

                listBox_Info.Items.Add(str);
                listBox_Info.SelectedIndex = listBox_Info.Items.Count - 1;
            //}
            //Marshal.FreeHGlobal(ptArray[0]);
            //Marshal.FreeHGlobal(pt);
        }

        private void button_StartCAN_Click(object sender, EventArgs e)
        {
            if (m_bOpen == 0)
                return;
            VCI_StartCAN(m_devtype, m_devind, m_canind);
        }

        private void button_StopCAN_Click(object sender, EventArgs e)
        {
            if (m_bOpen == 0)
                return;
            VCI_ResetCAN(m_devtype, m_devind, m_canind);
        }

        unsafe private void button_Send_Click(object sender, EventArgs e)
        {
            if(m_bOpen != 1)
            {
                MessageBox.Show("USB-CAN设备未连接或已断开请先连接设备", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
            //sendobj.Init();
            sendobj.RemoteFlag = (byte)CBoxBDSFrameFormat.SelectedIndex;
            sendobj.ExternFlag = (byte)CBoxBDSFrameType.SelectedIndex;
            sendobj.ID = System.Convert.ToUInt32(TxtBoxBDSFrameID.Text,16);
            int len = textBox_BDSData.Text.Length / 2;
            sendobj.DataLen =System.Convert.ToByte(len);
            String strdata = textBox_BDSData.Text;
            for (int i = 0; i < len; i++)
            {
                string hexValue = strdata.Substring(i * 2, 2); 
                sendobj.Data[i] = Convert.ToByte(hexValue, 16);
            }

            if (VCI_Transmit(m_devtype,m_devind,m_canind,ref sendobj,1)==0)
            {
                MessageBox.Show("发送失败", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void button_Clear_Click(object sender, EventArgs e)
        {
            listBox_Info.Items.Clear();
        }

        private void CBoxDischargeChanged(object sender, EventArgs e)
        {
            getBDSData();
        }

        void getBDSData()
        {
            int index = CBoxDischarge.SelectedIndex;
            if (CBoxDischarge.SelectedIndex == 1)
            {
                CBoxVCUDischarge.SelectedIndex = 1;
            }
            else if (CBoxDischarge.SelectedIndex == 2)
            {
                CBoxVCUDischarge.SelectedIndex = 0;
            }
            BDSData = ((ulong)index) +
                            (((ulong)0x3F0) << 2) +
                            (((ulong)CBoxVCUDischarge.SelectedIndex) << 12) +
                            (((ulong)0x02) << 13) +
                            (((ulong)0x01) << 15) +
                            (((ulong)0xFFFF) << 16) +
                            (((ulong)0x3FF) << 32) +
                            (((ulong)0x3FF) << 42) +
                            (((ulong)0xF) << 52) +
                            (((ulong)CBoxMsgCounter.SelectedIndex) << 56) +
                            (((ulong)0x000F) << 60);
            //ulong a = (((ulong)0x02) << 13) + 
            //                (((ulong)0x01) << 15) + 
            //                (((ulong)0xFFFF) << 16) + 
            //                (((ulong)0x3FF) << 32) + 
            //                (((ulong)0x3FF) << 42);
            //ulong b = (((ulong)0xF) << 52) + 
            //                (((ulong)CBoxMsgCounter.SelectedIndex) << 56) +
            //                (((ulong)0x000F) << 60);
            //BDSData = BDSData +a + b;
            textBox_BDSData.Text = BDSData.ToString("X16");
        }

        void getAllInitData()
        {
            CBoxBDSFrameType.SelectedIndex = iniConfig._BDS_FrameType;
            CBoxBDSFrameFormat.SelectedIndex = iniConfig._BDS_FrameFormat;
            TxtBoxBDSFrameID.Text = $"{iniConfig._BDS_FrameID:X8}";
            CBoxDischarge.SelectedIndex = iniConfig._BDS_DischangeState;
            CBoxMsgCounter.SelectedIndex = iniConfig._BDS_MsgCounter;

            CBoxBBIFrameType.SelectedIndex = iniConfig._BBI_FrameType;
            CBoxBBIFrameFormat.SelectedIndex = iniConfig._BBI_FrameFormat;
            TxtBoxBBIFrameID.Text = $"{iniConfig._BBI_FrameID:X8}";

            CBoxERDFrameType.SelectedIndex = iniConfig._ERD_FrameType;
            CBoxERDFrameFormat.SelectedIndex = iniConfig._ERD_FrameFormat;
            TxtBoxERDFrameID.Text = $"{iniConfig._ERD_FrameID:X8}";

            CBoxESDFrameType.SelectedIndex = iniConfig._ESD_FrameType;
            CBoxESDFrameFormat.SelectedIndex = iniConfig._ESD_FrameFormat;
            TxtBoxESDFrameID.Text = $"{iniConfig._ESD_FrameID:X8}";

        }

        unsafe private void Rec_Btn_Click(object sender, EventArgs e)
        {
            /*******************************ERD************************************/
            UInt32 res = new UInt32();
            m_recobj[0].TimeFlag = 1;
            m_recobj[0].ID = Convert.ToUInt32(TxtBoxERDFrameID.Text, 16);
            m_recobj[0].ExternFlag = (byte)CBoxERDFrameType.SelectedIndex;
            m_recobj[0].RemoteFlag = (byte)CBoxERDFrameFormat.SelectedIndex;

            res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
            string[] DecodeRawDataStr = new string[ViewList.Length], rawDataStr = new string[ViewList.Length];
            string[] ProtectionStatusStr = new string[ProtectionList.Length];
            UInt64 rawData;
            fixed (byte* p = m_recobj[0].Data)
            {
                rawData = *(ulong*)p;
            }
            //ERD 版本号BIT 0-15
            UInt16 Ver = (UInt16)(rawData & 0xFFFF);
            rawDataStr[0] = $"{Ver:X4}";
            DecodeRawDataStr[0] = $"{Ver:X4}";

            //ERD 最大放电电流BIT 16-31
            UInt16 MaxOutputCurrent = (UInt16)((rawData >> 16) & 0xFFFF);
            rawDataStr[1] = $"{MaxOutputCurrent:X4}";
            DecodeRawDataStr[1] = $"{(MaxOutputCurrent / 10):f3}";

            //ERD 最低放电电流BIT 32-47
            UInt16 MinOutputVolt = (UInt16)((rawData >> 32) & 0xFFFF);
            rawDataStr[2] = $"{MinOutputVolt:X4}";
            DecodeRawDataStr[2] = $"{(MinOutputVolt / 10):f3}";

            //ERD 最高放电电压BIT 48-63
            UInt16 MaxOutputVolt = (UInt16)((rawData >> 48) & 0xFFFF);
            rawDataStr[3] = $"{MaxOutputVolt:X4}";
            DecodeRawDataStr[3] = $"{(MaxOutputVolt / 10):f3}";
            /*****************************ERD end***************************************/
            /**********************************************************************/
            /*****************************ESD消息***********************************/
            res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
            rawData = 0;
            fixed (byte* p = m_recobj[0].Data)
            {
                rawData = *(ulong*)p;
            }
            //充电器请求放电指令 ESD BIT 0-1
            var commandDescriptions = new Dictionary<UInt16, string>
            {
                { 0x00, "低压自检" },
                { 0x01, "请求放电" },
                { 0x02, "请求停机下电" },
                { 0x03, "预留" }
            };
            UInt16 requestDischargeCmd = (UInt16)(rawData & 0x03);
            rawDataStr[4] = $"{requestDischargeCmd:X2}";
            DecodeRawDataStr[4] = commandDescriptions.TryGetValue(requestDischargeCmd, out var description)
                ? description
                : "未知";

            // 电子锁状态    ESD BIT 2-3
            var lockStateDescriptions = new Dictionary<UInt16, string>
            {
                { 0x00, "电子锁开锁" },
                { 0x01, "电子锁闭锁" },
                { 0x02, "电子锁故障" },
                { 0x03, "预留" }
            };
            UInt16 lockState = (UInt16)((rawData >> 2) & 0x03);
            rawDataStr[5] = $"{lockState:X2}";
            DecodeRawDataStr[5] = commandDescriptions.TryGetValue(requestDischargeCmd, out var desc)
                ? desc
                : "未知";

            //充电器状态 ESD BIT 4-6
            var dischargeStateDescriptions = new Dictionary<UInt16, string>
            {
                { 0x00, "初始化" },
                { 0x01, "待机" },
                { 0x02, "暂停" },
                { 0x03, "220V输出工作模式"},
                { 0x04, "380V输出停止模式"},
                { 0x05, "停机下电"},
                { 0x06, "告警"},
                { 0x07, "故障"}
            };
            UInt16 dischargeState = (UInt16)((rawData >> 4) & 0x07);
            rawDataStr[6] = $"{dischargeState:X2}";
            DecodeRawDataStr[6] = commandDescriptions.TryGetValue(requestDischargeCmd, out var des)
                ? des
                : "未知";

            //累计放电量  ESD BIT 7-16
            UInt16 KWh = (UInt16)((rawData >> 7) & 0x3FF);
            rawDataStr[7] = $"{KWh:X4}";
            DecodeRawDataStr[7] = $"{(KWh / 10):f3}";

            //设备故障状态 ESD BIT 17-29
            int chargerState = (UInt16)((rawData >> 17) & 0x1FFF);
            int flag = 0x01;

            for (int i = 0; i < ProtectionList.Length; i++) //有效标志位为10位
            {
                dataGridView2.Rows[i].Cells[1].Value = (chargerState & (flag << i)) >> i;
                //a = (state1.Flag & (flag << i));
                //b = (MduState & (flag << i));
                if (0x01 != ((chargerState & (flag << i)) >> i))
                {
                    dataGridView2.Rows[i].Cells[0].Style.BackColor = Color.LightBlue;
                }
                else
                {
                    dataGridView2.Rows[i].Cells[0].Style.BackColor = Color.Red;
                }
            }

            //累计放电时间 ESD BIT 30-41
            UInt16 cumulativeDischargeTime = (UInt16)((rawData >> 30) & 0xFFF);
            rawDataStr[8] = $"{cumulativeDischargeTime:X4}";
            DecodeRawDataStr[8] = $"{(cumulativeDischargeTime)}Min(s)";

            //实时对外放电功率 ESD BIT 42-51
            UInt16 Read_timeExDischargePower = (UInt16)((rawData >> 30) & 0xFFF);
            rawDataStr[9] = $"{Read_timeExDischargePower:X4}";
            DecodeRawDataStr[9] = $"{(Read_timeExDischargePower / 10):f3}KW";

            //充电枪内部温度 ESD BIT 52-59
            byte innerTemp = (byte)((rawData >> 52) & 0xFF);
            rawDataStr[10] = $"{innerTemp:X4}";
            innerTemp = (byte)((innerTemp >> 7) == 1 ? (innerTemp - 0x100) : innerTemp); //温度正负判断
            DecodeRawDataStr[10] = $"{innerTemp}";

            //ESD消息计数器 ESD BIT 60-63
            UInt16 MsgCounter = (UInt16)((rawData >> 60) & 0xF);
            rawDataStr[11] = $"{MsgCounter:X4}";
            DecodeRawDataStr[11] = $"{MsgCounter}";
        }
    }
}