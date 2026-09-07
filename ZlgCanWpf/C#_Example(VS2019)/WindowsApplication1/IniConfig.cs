using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WindowsApplication1
{
    public class IniConfig
    {
        public int _BDS_FrameType { get; set; }
        public int _BDS_FrameID { get; set; }
        public int _BDS_FrameFormat { get; set; }
        public int _BDS_DischangeState { get; set; }
        public int _BDS_MsgCounter { get; set; }
        public int _BBI_FrameType { get; set; }
        public int _BBI_FrameFormat { get; set; }
        public int _BBI_FrameID { get; set; }
        public int _ERD_FrameType { get; set; }
        public int _ERD_FrameID { get; set; }
        public int _ERD_FrameFormat { get; set; }
        public int _ESD_FrameType { get; set; }
        public int _ESD_FrameID { get; set; }
        public int _ESD_FrameFormat { get; set; }
        string seg = "Config";
        string path;
        public IniConfig(string _path)
        {
            this.path = _path;
        }
        public void ReadIniFlie()
        {
            _BDS_FrameType = Convert.ToInt32(IniReadKey(path, seg, "BDS_FrameType"));
            _BDS_FrameID = Convert.ToInt32(IniReadKey(path, seg, "BDS_FrameID"), 16);
            _BDS_FrameFormat = Convert.ToInt32(IniReadKey(path, seg, "BDS_FrameFormat"));
            _BDS_DischangeState = Convert.ToInt32(IniReadKey(path, seg, "BDS_DischargeState"));
            _BDS_MsgCounter = Convert.ToInt32(IniReadKey(path, seg, "BDS_MsgCounter"));
            _BBI_FrameType = Convert.ToInt32(IniReadKey(path, seg, "BBI_FrameType"));
            _BBI_FrameFormat = Convert.ToInt32(IniReadKey(path, seg, "BBI_FrameFormat"));
            _BBI_FrameID = Convert.ToInt32(IniReadKey(path, seg, "BBI_FrameID"), 16);
            _ERD_FrameType = Convert.ToInt32(IniReadKey(path, seg, "ERD_FrameType"));
            _ERD_FrameID = Convert.ToInt32(IniReadKey(path, seg, "ERD_FrameID"), 16);
            _ERD_FrameFormat = Convert.ToInt32(IniReadKey(path, seg, "ERD_FrameFormat"));
            _ESD_FrameType = Convert.ToInt32(IniReadKey(path, seg, "ESD_FrameType"));
            _ESD_FrameID = Convert.ToInt32(IniReadKey(path, seg, "ESD_FrameID"), 16);
            _ESD_FrameFormat = Convert.ToInt32(IniReadKey(path, seg, "ESD_FrameFormat"));

        }
        public void WriteIniFile()
        {
            IniWriteKey(path, seg, "BDS_FrameType", _BDS_FrameType.ToString());
            IniWriteKey(path, seg, "BDS_FrameID", _BDS_FrameID.ToString("X"));
            IniWriteKey(path, seg, "BDS_FrameFormat", _BDS_FrameFormat.ToString());
            IniWriteKey(path, seg, "BDS_DischangeState", _BDS_DischangeState.ToString());
            IniWriteKey(path, seg, "BDS_MsgCounter", _BDS_MsgCounter.ToString());

            IniWriteKey(path, seg, "BBI_FrameType", _BBI_FrameType.ToString());
            IniWriteKey(path, seg, "BBI_FrameFormat", _BBI_FrameFormat.ToString());
            IniWriteKey(path, seg, "BBI_frameID", _BBI_FrameID.ToString("X"));

            IniWriteKey(path, seg, "ERD_FrameType", _ERD_FrameType.ToString());
            IniWriteKey(path, seg, "ERD_FrameID", _ERD_FrameID.ToString("X"));
            IniWriteKey(path, seg, "ERD_FrameFormat", _ERD_FrameFormat.ToString());

            IniWriteKey(path, seg, "ESD_FrameType", _ESD_FrameType.ToString());
            IniWriteKey(path, seg, "ESD_FrameID", _ESD_FrameID.ToString("X"));
            IniWriteKey(path, seg, "ESD_FrameFormat", _ESD_FrameFormat.ToString());
        }

        public string IniReadKey(string iniFile, string Section, string key)//read from ini file
        {
            StringBuilder temp = new StringBuilder(2048);
            GetPrivateProfileString(Section, key, "", temp, 2048, iniFile);
            return temp.ToString();
        }
        public void IniWriteKey(string iniFile, string Section, string key, string val)//read from ini file
        {
            WritePrivateProfileString(Section, key, val, iniFile);
        }
        public string[] GetAllKeys(string section)
        {
            try
            {
                byte[] allSectionByte = new byte[4096];
                int length = GetPrivateProfileString(section, null, "", allSectionByte, 4096, this.path);
                string allSectionStr = Encoding.GetEncoding(Encoding.ASCII.CodePage).GetString(allSectionByte, 0, length);
                //用'|'代替‘\0’作为分隔符
                string allSectionStrEx = allSectionStr.Replace('\0', '|');
                //去除追后一个分割符"|"，不然会有一个空数据
                allSectionStrEx = allSectionStrEx.Substring(0, length - 1);
                //以'|'为分隔符分割allSectionStrEx到字符串数组
                string[] allSections = new string[length];
                char[] separator = { '|' };
                allSections = allSectionStrEx.Split(separator);
                return allSections;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        /// 写入INI文件
        /// </summary>
        /// <param name="section">节点名称[如[TypeName]]</param>
        /// <param name="key">键</param>
        /// <param name="val">值</param>
        /// <param name="filepath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <param name="key">键</param>
        /// <param name="def">值</param>
        /// <param name="retval">stringbulider对象</param>
        /// <param name="size">字节大小</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, byte[] retval, int size, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);
        /// <summary>获取节点下所有键</summary>
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileSection")]
        private static extern uint GetPrivateProfileSection(string lpAppName, byte[] lpReturnedString, int nSize, string lpFileName);
    }
}
