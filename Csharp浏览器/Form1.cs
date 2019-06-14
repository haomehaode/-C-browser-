using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO.Ports;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;



namespace Csharp浏览器
{
    public partial class Form1 : Form
    {


        #region 初始化变量
        //添加到收藏夹
        [DllImport("User32.DLL")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        [DllImport("User32.DLL")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        public int IDM_ADDFAVORITES = 2261;
        public uint WM_COMMAND = 0x0111;
        //整理收藏夹
        [DllImport("shdocvw.dll")]
        public static extern IntPtr DoOrganizeFavDlg(IntPtr hWnd, string lpszRootFolder);
        //查看源文件
        public int IDM_VIEWSOURCE = 2139;
        //获取收藏夹路径
        string favorfolder = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);        //获取系统收藏夹路径 
        #endregion
        public Form1()
        {
            InitializeComponent(); 
            tabPage1.Text = "";
            ListMenuItem( new DirectoryInfo(favorfolder));  
        }   
        #region 收藏夹
        public void ListMenuItem( FileSystemInfo info)    //生成收藏夹菜单的函数，递归使用  
        {
            if (!info.Exists) 
                return;
            DirectoryInfo dir = info as DirectoryInfo;
            //不是目录   
            if (dir == null)
                return;
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            ToolStripMenuItem[] ShouCangsMenuItem = new ToolStripMenuItem[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i] as FileInfo;
                ShouCangsMenuItem[i] = new System.Windows.Forms.ToolStripMenuItem();
                //是文件   
                if (file != null)
                {
                    if (file.Extension == ".url")
                    {
                        string str = file.Name;             //获取收藏夹的文件名（都是URL文件）  
                        str = str.Remove(str.Length - 4);     //去掉.url后缀名  

                        ShouCangsMenuItem[i].Text = str;          //然后赋值给菜单文本  

                        StreamReader sr = file.OpenText();//获取文件输入流  


                        List<string> src = new List<string>();

                        string source = null;
                        while ((source = sr.ReadLine()) != null)
                        {
                            src.Add(source);//文件所有行添加到List<string>中  
                        }

                        ShouCangsMenuItem[i].Tag = src;

                        foreach (string stri in src)
                        {
                            if (stri != null)
                            {
                                if (stri.StartsWith("URL="))
                                {
                                    ShouCangsMenuItem[i].ToolTipText = stri.Remove(0, 4);//ToolTipText  
                                }
                            }
                        }

                        toolStripButton8.DropDownItems.Add(ShouCangsMenuItem[i]);   //生成的子菜单添加到上一级菜单  
                        ShouCangsMenuItem[i].Click += new EventHandler(ShouCangsMenuItem_Click);          //为生成的子菜单添加单击消息（可写一个消息处理函数，这里就不例出来了）  
                    }
                }
                //对于子目录，进行递归调用   
                else
                {
                    DirectoryInfo Direct = files[i] as DirectoryInfo;
                    ShouCangsMenuItem[i].Text = files[i].Name;
                    ShouCangsMenuItem[i].ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
                    ShouCangsMenuItem[i].ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.SizeToFit;
                    toolStripButton8.DropDownItems.Add(ShouCangsMenuItem[i]);       //生成的子菜单添加到上一级菜单  
                    ListMenuItem(new DirectoryInfo(Direct.FullName));   //递归使用，生成子菜单  
                }
            }
        }

        private void ShouCangsMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            string[] tar = ((List<string>)item.Tag).ToArray();
            for (int i = 0; i < tar.Length; i++)
            {
                if (tar[i] != null)
                {
                    if (tar[i].StartsWith("URL="))
                    {
                        getCurrentBrowser().Navigate(tar[i].Remove(0, 4));
                    }
                }
            }  
        }  
        #endregion



        /// <summary>
        /// 程序加载时发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            webBrowser1.Navigate("http://www.baidu.com"); 
            webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
        }
        /// <summary>
        /// 重掉函数得到标题
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser myBrowser = (WebBrowser)tabControl1.SelectedTab.Controls[0];
            tabControl1.SelectedTab.Text = myBrowser.DocumentTitle;
            url.Text = myBrowser.Url.ToString();
        }

        /// <summary>
        /// 导航发生改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            url.Text = getCurrentBrowser().Url.ToString();
            WebBrowser myBrowser = (WebBrowser)sender;
            string NewUrl = ((WebBrowser)sender).StatusText;
            tabControl1.SelectedTab.Text = NewUrl;
            TabPage TabPageTemp = new TabPage();
            WebBrowser tempBrowser = new WebBrowser();
            tempBrowser.ScriptErrorsSuppressed = true;
            tempBrowser.Navigate(NewUrl);
            tempBrowser.Dock = DockStyle.Fill;
            TabPageTemp.Controls.Add(tempBrowser);
            url.Text = myBrowser.Url.ToString();
            tempBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted); 
        }
        /// <summary>
        /// 创建新窗口时发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webBrowser1_NewWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            string NewUrl = ((WebBrowser)sender).StatusText;
            url.Text = NewUrl;
            TabPage TabPageTemp = new TabPage();
            WebBrowser tempBrowser = new WebBrowser();
            tempBrowser.NewWindow += new CancelEventHandler(webBrowser1_NewWindow);
            tempBrowser.ScriptErrorsSuppressed = true;
            tempBrowser.Navigate(NewUrl);
            tempBrowser.Dock = DockStyle.Fill;
            TabPageTemp.Controls.Add(tempBrowser);
            this.tabControl1.TabPages.Add(TabPageTemp);
            this.tabControl1.SelectedTab = TabPageTemp;
            tempBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
        }

        private WebBrowser getCurrentBrowser()
        {
            WebBrowser currentBrowser = (WebBrowser)tabControl1.SelectedTab.Controls[0];
            return currentBrowser;
        }
        #region 文件
        /// <summary>
        /// 新建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 新建MToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage myPage = new TabPage();
            myPage.ImageIndex = 0;
            WebBrowser tempBrowser = new WebBrowser();
            tempBrowser.NewWindow += new CancelEventHandler(webBrowser1_NewWindow);    
            tempBrowser.GoHome();
            myPage.Text = tempBrowser.DocumentTitle;
            tempBrowser.Dock = DockStyle.Fill;
            tempBrowser.ScriptErrorsSuppressed = true;
            myPage.Controls.Add(tempBrowser);
            tabControl1.TabPages.Add(myPage);
            this.tabControl1.SelectedIndex = tabControl1.TabCount - 1;
            tempBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
            
        }
        /// <summary>
        /// 打开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 打开OToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                string urlFileName = open.FileName;
                getCurrentBrowser().Navigate(urlFileName);
            }
        }
        /// <summary>
        /// 另存为
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 另存为AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().ShowSaveAsDialog();
        }
        /// <summary>
        /// 页面设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 页面设置UToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().ShowPageSetupDialog();
        }
        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 打印ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().ShowPrintDialog();
        }
        /// <summary>
        /// 打印预览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 打印预览ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().ShowPrintPreviewDialog();
        }
        /// <summary>
        /// 关闭当前页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 关闭页面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabControl tb = (TabControl)tabControl1;
            int i = tb.SelectedIndex;
            tb.TabPages.RemoveAt(i);
        }
        /// <summary>
        /// 关闭浏览器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 全部关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        
        #endregion

        #region 工具
        /// <summary>
        /// internet选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void internet选项OToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process pro = new Process();
            pro.StartInfo = new ProcessStartInfo("rundll32.exe", "shell32.dll,Control_RunDLL inetcpl.cpl");
            pro.Start();
        }
        #endregion
      
        #region 查看
        /// <summary>
        /// 查看网站源文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 网站源文件WToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IntPtr vHandle = webBrowser1.Handle;

            vHandle = FindWindowEx(vHandle, IntPtr.Zero, "Shell Embedding", null);
            vHandle = FindWindowEx(vHandle, IntPtr.Zero, "Shell DocObject View", null);
            vHandle = FindWindowEx(vHandle, IntPtr.Zero, "Internet Explorer_Server", null);
            SendMessage(vHandle, WM_COMMAND, IDM_VIEWSOURCE, (int)Handle);
            //WebBrowser myBrowser = (WebBrowser)tabControl1.SelectedTab.Controls[0];
            //string b = myBrowser.Document.Body.InnerHtml;
            //MessageBox.Show(b);
        }
        
        #endregion

        #region 选项卡功能
        /// <summary>
        /// 选项卡切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                WebBrowser myBrowser = (WebBrowser)tabControl1.SelectedTab.Controls[0];
                if (myBrowser.Url != null)
                {
                    url.Text = myBrowser.Url.ToString();
                    tabControl1.SelectedTab.Text = myBrowser.DocumentTitle;
                }
            }
            catch (Exception)
            {

                url.Text = "about:blank";
            }

        }
        /// <summary>
        /// 双击选项卡删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_DoubleClick(object sender, EventArgs e)
        {
            TabControl tb = (TabControl)(sender);
            

            int i = tb.SelectedIndex;
   
            tb.TabPages.RemoveAt(i);
        } 
        #endregion

        #region 收藏
        /// <summary>
        /// 添加收藏夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 添加到收藏夹AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                WebBrowser myBrowser = (WebBrowser)tabControl1.SelectedTab.Controls[0];
                IntPtr vHandle = myBrowser.Handle;
                vHandle = FindWindowEx(vHandle, IntPtr.Zero, "Shell Embedding", null);
                vHandle = FindWindowEx(vHandle, IntPtr.Zero, "Shell DocObject View", null);
                vHandle = FindWindowEx(vHandle, IntPtr.Zero, "Internet Explorer_Server", null);
                SendMessage(vHandle, WM_COMMAND, IDM_ADDFAVORITES, (int)Handle);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            List<ToolStripMenuItem> lc = new List<ToolStripMenuItem>();
            foreach (ToolStripMenuItem ca in toolStripButton8.DropDownItems)
            {
                lc.Add(ca);
            }
            foreach (ToolStripMenuItem cb in lc)
            {
                toolStripButton8.DropDownItems.Remove(cb);
              
            }     
            ListMenuItem(new DirectoryInfo(favorfolder));  
        }
        /// <summary>
        /// 整理收藏夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 整理收藏夹OToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SHDocVw.ShellUIHelper helper = new SHDocVw.ShellUIHelper();
                object o = null;
                helper.ShowBrowserUI("OrganizeFavorites", ref o);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }


        } 
        #endregion

        #region 导航栏
        /// <summary>
        /// 新建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            TabPage myPage = new TabPage();
            WebBrowser tempBrowser = new WebBrowser();
            tempBrowser.NewWindow += new CancelEventHandler(webBrowser1_NewWindow);    
            tempBrowser.GoHome();
            myPage.Text = tempBrowser.DocumentTitle;
            tempBrowser.Dock = DockStyle.Fill;
            tempBrowser.ScriptErrorsSuppressed = true;
            myPage.Controls.Add(tempBrowser);
            tabControl1.TabPages.Add(myPage);
            this.tabControl1.SelectedIndex = tabControl1.TabCount - 1;
            tempBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
        }
        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (getCurrentBrowser().CanGoBack)
            {
                getCurrentBrowser().GoBack();
            }
            else
            {
                MessageBox.Show("已达到最早历史记录");
            }
        }
        /// <summary>
        /// 前进
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (getCurrentBrowser().CanGoForward)
            {
                getCurrentBrowser().GoForward();
            }
            else
            {
                MessageBox.Show("已达到最后历史记录");
            }
        }
        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Refresh();
        }
        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Stop();
        }
        /// <summary>
        /// 主页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().GoHome();
        }
        /// <summary>
        /// 收藏夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            try
            {
                SHDocVw.ShellUIHelper helper = new SHDocVw.ShellUIHelper();
                object o = null;
                helper.ShowBrowserUI("OrganizeFavorites", ref o);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

        }
        /// <summary>
        /// 转到
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Navigate(url.Text);
        }
        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            TabPage myPage = new TabPage();
            WebBrowser tempBrowser = new WebBrowser();
            tempBrowser.Navigate("http://www.baidu.com/s?wd=" + toolStripComboBox2.Text.ToString());
            tempBrowser.Dock = DockStyle.Fill;
            tempBrowser.ScriptErrorsSuppressed = true;
            myPage.Controls.Add(tempBrowser);
            tabControl1.TabPages.Add(myPage);
            this.tabControl1.SelectedIndex = tabControl1.TabCount - 1;
            tempBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
        }
        #endregion



    }
}
