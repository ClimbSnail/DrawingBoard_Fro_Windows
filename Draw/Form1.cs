using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Drawing.Drawing2D;


namespace Draw
{
    public partial class Form1 : Form
    {
        Pen p = new Pen(Color.Black, 5); //画笔
      //public Bitmap pbmap;       
        Graphics g;             //声明区间对象
        Color color;            //储存调色板颜色
        int x1, y1, x2, y2;   //坐标点
        int font=2;             //储存字体大小
        int tool;               //记录tool工具的选择
        int move = 0;               // 鼠标移动标志
        int  flag = 0 ;              //画由两点确认的图形时 两点先后的标志
        int timeflag = 0;
        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            
            button2.ForeColor = Color.Red ;
            color = Color.Blue;   //初始让color为黑色
            for (int i = 2; i <= 72 ; i+=2 )  //初始化添加字体选择下拉表框
            {
                comboBox2.Items.Add(i.ToString()); 
            }
            comboBox2.Text = "2";  //字体表初始为1

            pictureBox1.BackColor = Color.White; 

            init();                 //初始化功能按钮
            toolStripButton1.Checked = true;  //默认为画笔按钮开启
            tool = 1;               //工具
        }

        private void button1_Click(object sender, EventArgs e)      //搜索串口开关
        {
            SearchAndAdd(serialPort1, comboBox1);               //搜索串口
        }

        private void button2_Click(object sender, EventArgs e)  //打开串口
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Close();
                }
                catch { }
                button2.Text = "打开";
                button2.ForeColor = Color.Red ;   //将"关闭串口"的按钮字体变为红色
            }
            else
            {
                try
                {
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.Open();
                    button2.Text = "关闭";
                    button2.ForeColor = Color.Green;   //将"打开串口"的按钮字体变为绿色
                }
                catch
                {
                    MessageBox.Show("连接失败","错误");  //错误事弹出提示框
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);//必须手动添加事件处理程序
//            this.toolStripStatusLabel2.Text = System.DateTime.Now.ToString(); //获取时间

        }
        private void SearchAndAdd(SerialPort MyPort, ComboBox MyBox)   //搜索串口
        {
            timer1.Stop();
            string Buffer;                                              //缓存
            MyBox.Items.Clear();                                        //清空ComboBox内容
            for (int i = 1; i < 10; i++)                                //循环
            {
                try                                                     //核心原理是依靠try和catch完成遍历
                {
                    Buffer = "COM" + i.ToString();
                    MyPort.PortName = Buffer;
                    MyPort.Open();                                      //如果失败，后面的代码不会执行
                    MyBox.Items.Add(Buffer);                            //打开成功，添加至下俩列表
                    MyPort.Close();                                     //关闭
                }
                catch
                {
                  }
            }
            timer1.Start();
        }
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)//串口数据接收事件
        {
            timer1.Stop();
            //string str = serialPort1.ReadExisting();//字符串方式读
            ////int temp = Convert.ToInt32(str);
            int temp;
            //int.TryParse(str,out temp);
            string str = serialPort1.ReadExisting();//字符串方式读
            if (int.TryParse(str, out temp))
            {
                temp = Convert.ToInt32(str);
            }
            x1 = temp / 1000;

            if (x1 == 0)
            {
                font = temp / 100;
                int color_num = temp % 100 / 10;
                switch ( color_num )
                {
                    case 1: color = Color.Red; break;
                    case 2: color = Color.Green; break;
                    case 3: color = Color.Blue; break;
                    case 4: color = Color.Brown; break;
                    case 5: color = Color.Yellow; break;
                } 
                tool = temp % 1000;                
            }
            else
            {
                y1 = temp % 1000;
                p.Width = font; //画笔粗细

                switch (tool)                          //选择画图工具
                {
                    case 1:
                        g.FillEllipse(Brushes.White, new Rectangle(x1, y1, font, font)); //画一个白色圆
                        break;
                    case 0:                                                         //只画一个小点
                        g.FillEllipse(Brushes.Red, new Rectangle(x1, y1, font, font));
                        x2 = x1; y2 = y1;
                        break;
                    case 2:                                                     //画直线
                        this.pictureBox1.Image = null;
                        //if (fork == 0) { x2 = x1; y2 = y1; fork = 1; }              //两点先后的标志
                        //else { fork = 0; g.DrawLine(p, x2, y2, x1, y1); }
                        break;
                    case 3:                                                  //画矩形
                        if ( flag == 0) { x2 = x1; y2 = y1; flag = 1; }              //两点先后的标志
                        else { flag = 0; g.DrawRectangle(p, x2, y2, x1, y1); }
                        break;
                    case 4:                                             //画圆形
                        if ( flag == 0) { x2 = x1; y2 = y1; flag = 1; }              //两点先后的标志
                        else { flag = 0; g.DrawEllipse(p, x2, y2, x1, y1); }
                        break;
                    case 5:                                                            //橡皮擦 
                        g.FillEllipse(Brushes.White, new Rectangle(x1, y1, font, font)); //画一个白色圆
                        break;
                }
            }
            timeflag = 0 ;
            timer1.Start();
//           else
//            { //如果接收模式为数值接收
            //byte data;
            //data = (byte)serialPort1.ReadByte();//此处需要强制类型转换，将(int)类型数据转换为(byte类型数据，不必考虑是否会丢失数据
            //string str = Convert.ToString(data, 16).ToUpper();//转换为大写十六进制字符串
//            textBox1.AppendText("0x" + (str.Length == 1 ? "0" + str : str) + " ");//空位补“0”   
               //}
        }

        private void 新建图形ToolStripMenuItem_Click(object sender, EventArgs e)  //新建图形按钮
        {
            pictureBox1.Refresh();
            pictureBox1.BackColor = Color.White;
            g = this.pictureBox1.CreateGraphics();
        //    pbmap = new Bitmap(pictureBox1.Width, pictureBox1.Height); 
        }

        private void 保存文件ToolStripMenuItem_Click(object sender, EventArgs e)   //保存文件
        {

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            //saveFileDialog1.Filter = "保存(*.bmp)|*.bmp";
            //saveFileDialog1.FilterIndex = 2;
            //saveFileDialog1.RestoreDirectory = true;
            //if (DialogResult.OK == saveFileDialog1.ShowDialog())
            //{
            //    if (pictureBox1.Image != null)
            //    {
            //        Bitmap bmp = new Bitmap(pictureBox1.Image);
            //        g = Graphics.FromImage((Image)bmp);
            //        pictureBox1.Image = (Image)bmp;
            //        pictureBox1.Image.Save(saveFileDialog1.FileName);
            //        bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
            //    }
            //    else
            //    {
            //        MessageBox.Show("已保存");
            //    }
            //}
            
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            g = Graphics.FromImage((Image)bmp);
            g.DrawRectangle(p, 0, 0, 100, 100);
            pictureBox1.Image = (Image)bmp;
            saveFileDialog1.Title = "save the pictrue";
            saveFileDialog1.Filter = "jpg图片(*.jpg)|*.jpg|bmp图片(*.bmp)|*.bmp";
            saveFileDialog1.InitialDirectory = "f:\\";

            if ((saveFileDialog1.FileName != null) && (saveFileDialog1.ShowDialog() == DialogResult.OK))
            {
                bmp.Save("C:\\Picture1.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                bmp.Dispose();
                pictureBox1.Image.Save(saveFileDialog1.FileName);
            }
            else
            {
                return;

            }
        }

        private void 打开文件ToolStripMenuItem_Click(object sender, EventArgs e) //打开文件
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();  //声明一个打开对象
            openFileDialog1.Multiselect = false;
            //if (openFileDialog1.ShowDialog() == DialogResult.OK)
            //{
            //    //修改窗口标题  
            //    this.Text = "MyDraw\t" + openFileDialog1.FileName;
            //    editFileName = openFileDialog1.FileName;
            //    theImage = Image.FromFile(openFileDialog1.FileName);
            //    Graphics g = this.CreateGraphics();
            //    g.DrawImage(theImage, this.ClientRectangle);
            //    ig = Graphics.FromImage(theImage);
            //    ig.DrawImage(theImage, this.ClientRectangle);
            //    //ToolBar可以使用了  
            //    toolStrip1.Enabled = true;
            //}  
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)  //鼠标按下
        {
            x1 = e.X; y1 = e.Y; 
            move = 1;
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)  //鼠标松开
        {
                move = 0;       //让鼠标移动停止
                x2 = e.X; y2 = e.Y;
                g = pictureBox1.CreateGraphics();
                Pen p2 = new Pen(color, font);
                switch (tool)
                {
                    case 2:
                        g.DrawLine(p2, x1, y1, x2, y2);         //画直线
                        break;
                    case 3:
                        g.DrawRectangle(p2, x1, y1, x2 - x1, y2 - y1); //画矩形
                        break;
                    case 4:
                        g.DrawEllipse(p2, x1, y1, x2 - x1, y2 - y1);//画填充椭圆的方法，x坐标、y坐标、宽、高
                        break;
                }
            
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            timer1.Stop();
            Pen p2 = new Pen(color , font);
            g = pictureBox1.CreateGraphics();
        if (move==1 && tool == 1)    //鼠标左击后到松开有效，且为画笔时才作图
            {
                g.FillEllipse(Brushes.Blue , new Rectangle(x1-font/2, y1-font/2, font, font)); //只画圆心
                x1 = e.X;
                y1 = e.Y;
            }
        
        if (move == 1 && tool == 5)    //鼠标左击后到松开有效，且为橡皮擦才作图
        {
            g.DrawLine( new Pen(Color.White,font) , x1, y1, e.X , e.Y );
            x1 = e.X;
            y1 = e.Y;
            p2.Color = Color.White; 
        }
          
         if (timeflag == 1)
        {
            x2 = x1;
            y2 = y1;
        }
        else {
            if( x1 != 0 )
            g.DrawLine(p2, x2, y2, x1, y1);
            x2 = x1;
            y2 = y1;
        }
        timeflag = 0;
        timer1.Start();
         }
        private void timer1_Tick(object sender, EventArgs e)  //定时器中断函数
        {
            timer1.Stop();    //定时器关闭
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
            }
            if ( timeflag == 1 )
            x2 = 0;
            try
            {
                serialPort1.Open();
            }
            catch { }
            timeflag = 1;     
            timer1.Start();     //定时器开启
            
        }
        private void init()         //复位按键
        {
            toolStripButton1.Checked = false;   //各按键复位
            toolStripButton2.Checked = false;
            toolStripButton3.Checked = false;
            toolStripButton4.Checked = false;
            toolStripButton5.Checked = false;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)  //按下画笔
        {
            init();
            toolStripButton1.Checked = true;
            tool = 1;       
        }
        public void SetCursor(Bitmap cursor, Point hotPoint)        //设置鼠标函数
        {
            int hotX = hotPoint.X;
            int hotY = hotPoint.Y;
            Bitmap myNewCursor = new Bitmap(cursor.Width * 2 - hotX, cursor.Height * 2 - hotY);
            Graphics g = Graphics.FromImage(myNewCursor);
            g.Clear(Color.FromArgb(0, 0, 0, 0));
            g.DrawImage(cursor, cursor.Width - hotX, cursor.Height - hotY, cursor.Width,
            cursor.Height);

            this.Cursor = new Cursor(myNewCursor.GetHicon());

            g.Dispose();
            myNewCursor.Dispose();
        }  
        private void toolStripButton2_Click(object sender, EventArgs e)//按下直线
        {
            init();
            toolStripButton2.Checked = true;
            tool = 2;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)//按下矩形
        {
            init();
            toolStripButton3.Checked = true;
            tool = 3;
            MessageBox.Show("先选择矩形左上角的点，再选择椭圆的右下角的点", "友情提示");
        }

        private void toolStripButton4_Click(object sender, EventArgs e)//按下椭圆
        {
            init();
            toolStripButton4.Checked = true;
            tool = 4;
            MessageBox.Show("先选择椭圆左上角的点，再选择椭圆的右下角的点", "友情提示");
        }

        private void toolStripButton5_Click(object sender, EventArgs e)//按下橡皮擦
        {
            init();
            toolStripButton5.Checked = true;
            tool = 5;
        }
        

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string str = comboBox2.Text;
            font = 0;
            for (int i = 0; i < str.Length; i++)   //字体大小
                font = font * 10 + str[i] - 48;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.pictureBox1.Image = null;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort1.PortName = comboBox1.Text;
        }
    }
}
