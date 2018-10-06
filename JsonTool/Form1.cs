using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JsonTool
{
    public partial class Form1 : Form
    {
        ArrayList citylist = new ArrayList();//城市的集合
        ArrayList parentlist = new ArrayList();//市级的集合
        ArrayList adminlist = new ArrayList();//省级的集合
        //ArrayList all = new ArrayList();

        Boolean newadmin = false;//用于判断是在同个省份中发现市级不一样的情况
        string admintemptext = "", parenttemptext = "", citytemptext = "";//用于存储合并后的各级字符串
        string admintext = "", parenttext = "", citytext = "";//用于存储readline获取的各级名字，用作判断省份市级是否有变动
        string cityname, parentname, adminname;//用于存储readline获取到的省市名字。

        Encoding textencoding;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String linetext;
            OpenFileDialog ofd = new OpenFileDialog();//保存文件窗体
            ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);//默认选择路径为文档路径
            ofd.Filter = "文本文档|*.txt|所有文件|*.*";//设置文件筛选格式
            if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName.Substring(ofd.FileName.LastIndexOf("\\") + 1).Length > 0)//如果选择好了打开文件的路径和名字
            {
                FileStream textStream = new FileStream(ofd.FileName, FileMode.Open, FileAccess.ReadWrite);//根据选择的文件参数，打开文件，获得文件流
                                                                                                          //FileStream textStream = new FileStream(@"G:\Users\wcedla\Desktop\qqqqqqqqqq.txt", FileMode.Open, FileAccess.ReadWrite);//根据选择的文件参数，打开文件，获得文件流
                byte[] bt = new byte[textStream.Length];//文件流的read方法需要用到的byte数组。
                textStream.Read(bt, 0, bt.Length);//将文本内容全部存储到byte数组
                textStream.Close();//关闭文本流
                textencoding = GetBytesEncoding(bt);//获取文本编码
                StreamReader streamReader = new StreamReader(ofd.FileName, textencoding);
                while ((linetext = streamReader.ReadLine()) != null && !string.IsNullOrWhiteSpace(linetext))//文件读到尾部了或者尾部是空行的时候停止循环
                {
                    cityname = linetext.Split('\t')[0];
                    adminname = linetext.Split('\t')[1];
                    parentname = linetext.Split('\t')[2];
                    if (!admintext.Equals(adminname))
                    {
                        if (!admintext.Equals(""))//排除开头不等的情况，不判断会写入空行，因为citylist还没有内容就开始连接了。
                        {
                            newadmin = true;//标志位，主要用于同省份不同市级的时候需要合并citylist
                            connectCity();//省份不同了，说明上一个省份已经读取完毕了，需要整合parentlist了，但是整合parentlist之前要整合citylist。
                            connectParent();
                        }
                        admintext = adminname;//不能一直判不同
                    }
                    else
                    {
                        newadmin = false;//保证只有省份不同时才是真
                    }
                    if (!parenttext.Equals(parentname))
                    {
                        if (!parenttext.Equals(""))//排除第一次读取的时候
                        {
                            if (!newadmin)//省份相同时遇到市级不同时需要整合citylist
                            {
                                connectCity();
                            }
                        }
                        parenttext = parentname;
                    }
                    if (!citytext.Equals(cityname))
                    {
                        citytemptext = "{\"cityname\":" + "\"" + cityname + "\"}";//一直加citylist
                        citytext = cityname;
                        citylist.Add(citytemptext);
                    }
                }
                connectCity();//读取完毕之后需要整合adminlist了，整合adminlist之前需要将上一次还没有整合的parentlist和citylist再整合一次
                connectParent();
                string resultstring = connectAdmin(); //获取最终合并字符串。
                streamReader.Close();
                SaveFileDialog sfd = new SaveFileDialog();//打开保存文件窗体           
                sfd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);//设置存储路径
                sfd.FileName = "生成文件.txt";//设置文件名
                sfd.Filter = "文本文档|*.txt|所有文件|*.*";//设置文件筛选格式
                DialogResult dialogResult = sfd.ShowDialog();//显示保存操作窗体
                if (dialogResult == DialogResult.OK && sfd.FileName.Substring(sfd.FileName.LastIndexOf("\\") + 1).Length > 0)//如果选择了确定保存
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.CreateNew, FileAccess.ReadWrite);
                    StreamWriter streamWriter = new StreamWriter(fs, textencoding);
                    streamWriter.Write(resultstring);
                    streamWriter.Close();
                    fs.Close();
                    MessageBox.Show("省份list剩余数" + parentlist.Count.ToString() + ",处理的省份数" + adminlist.Count.ToString());
                    MessageBox.Show("保存成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);//保存成功提示                                   
                }
            }
        }

        void connectCity()
        {
            string connectstring = "";
            for (int i = 0; i < citylist.Count; i++)
            {
                connectstring += citylist[i].ToString();
                if (i != citylist.Count - 1)//最后一个不能再加逗号了
                {
                    connectstring += ",";
                }
            }
            parenttemptext = "{\"parentname\":" + "\"" + parenttext + "\",\"citylist\":[" + connectstring + "]}";
            parentlist.Add(parenttemptext);
            citylist.Clear();
        }

        void connectParent()
        {
            string connectstring = "";
            for (int i = 0; i < parentlist.Count; i++)
            {
                connectstring += parentlist[i].ToString();
                if (i != parentlist.Count - 1)//最后一个不能再加逗号了
                {
                    connectstring += ",";
                }
            }
            admintemptext = "{\"adminname\":" + "\"" + admintext + "\"" + "," + "\"parentlist\":[" + connectstring + "]}";
            adminlist.Add(admintemptext);
            parentlist.Clear();
            citylist.Clear();
        }

        string connectAdmin()
        {
            string connectstring = "";
            for (int i = 0; i < adminlist.Count; i++)
            {
                connectstring += adminlist[i].ToString();
                if (i != adminlist.Count - 1)//最后一个不加逗号
                {
                    connectstring += ",";
                }
            }
            string result = "[" + connectstring + "]";
            Clipboard.SetDataObject(result);
            return result;
        }

        Encoding GetBytesEncoding(byte[] bs)//判断文本使用的编码
        {
            int len = bs.Length;
            if (len >= 3 && bs[0] == 0xEF && bs[1] == 0xBB && bs[2] == 0xBF)
            {
                return Encoding.UTF8;
            }
            int[] cs = { 7, 5, 4, 3, 2, 1, 0, 6, 14, 30, 62, 126 };
            for (int i = 0; i < len; i++)
            {
                int bits = -1;
                for (int j = 0; j < 6; j++)
                {
                    if (bs[i] >> cs[j] == cs[j + 6])
                    {
                        bits = j;
                        break;
                    }
                }
                if (bits == -1)
                {
                    return Encoding.Default;
                }
                while (bits-- > 0)
                {
                    i++;
                    if (i == len || bs[i] >> 6 != 2)
                    {
                        return Encoding.Default;
                    }
                }
            }
            return Encoding.UTF8;
        }
    }   
}
