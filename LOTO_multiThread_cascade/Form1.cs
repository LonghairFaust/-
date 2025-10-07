using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices; // 这是使用c#时,要调用动态库DLL时候要引入的  
using System.Text;
using System.Threading;               // for multithread
using System.Threading.Tasks;
using System.Windows.Forms;



namespace OpenSource_LOTO_A02
{
    public partial class Form1 : Form
    {       

        // 声明需要的变量,实例中四台设备命令公用了 (如果要单独发就分开)
        public static byte g_CtrlByte0 = 0; // 记录IO控制位 设备1A
        public static byte g_CtrlByte1 = 0; // 记录IO控制位 设备1B
        public static byte g_CtrlByte0_CD = 0;// 20250515记录IO控制位 设备1C
        public static byte g_CtrlByte1_CD = 0;// 20250515记录IO控制位 设备1D

        public static byte g_2ndOSC_CtrlByte0 = 0; // 记录IO控制位 设备2A
        public static byte g_2ndOSC_CtrlByte1 = 0; // 记录IO控制位 设备2B
        public static byte g_2ndOSC_CtrlByteC = 0; // 记录IO控制位 设备2C
        public static byte g_2ndOSC_CtrlByteD = 0; // 记录IO控制位 设备2D

        public static byte g_3rdOSC_CtrlByte0 = 0; // 记录IO控制位 设备3A
        public static byte g_3rdOSC_CtrlByte1 = 0; // 记录IO控制位 设备3B
        public static byte g_3rdOSC_CtrlByteC = 0; // 记录IO控制位 设备3C
        public static byte g_3rdOSC_CtrlByteD = 0; // 记录IO控制位 设备3D

        public static byte g_4thOSC_CtrlByte0 = 0; // 记录IO控制位 设备4A
        public static byte g_4thOSC_CtrlByte1 = 0; // 记录IO控制位 设备4B
        public static byte g_4thOSC_CtrlByteC = 0; // 记录IO控制位 设备4C
        public static byte g_4thOSC_CtrlByteD = 0; // 记录IO控制位 设备4D

        private LOTOReadThread LOTOReadThreadObject = null;  //20230726 jiangtao.lv 实例化一个线程类的对象。
        private Thread ReadThread = null;                    //20230726 jiangtao.lv 建立一个线程专门用于数据的读取和处理。
        
        public int m_DevModelNum = 7; //默认6为OSCA02，2为OSC2002，7为OSCH02或者OSCF4

        public Form1()
        {
             

            InitializeComponent();

            but_StopDevice.Visible = false;
           
    
            groupCoupling.Enabled = false;
        
            butRead.Enabled = false;
            But_Read.Enabled = false;
            butDispaly.Enabled = false;
            groupBoxCHBset.Enabled = false;
 
            comboBox_CHs.SelectedIndex = 0;
            ScanFreq = new System.Timers.Timer(500); // 扫频定时器初始化
            ScanFreq.Elapsed += new System.Timers.ElapsedEventHandler(ScanFreqStartHandler);
            ScanFreq.Enabled = false;

            processTimer = new System.Threading.Timer(ProcessTimer_Tick, null, 0, 100);


            saveQueue = new ConcurrentQueue<byte[]>();//测试

        }

        private void comboBox_CHs_SelectedIndexChanged(object sender, EventArgs e) // 多少台设备级联
        {
            // 1台， 2台，3台， 4台. 
            globleVariables.g_OSCcnt = comboBox_CHs.SelectedIndex + 1;
      
            Array.Clear(globleVariables.g_chADataArray, 0, globleVariables.g_chADataArray.Length);
            Array.Clear(globleVariables.g_chBDataArray, 0, globleVariables.g_chBDataArray.Length);
            Array.Clear(globleVariables.g_chCDataArray, 0, globleVariables.g_chCDataArray.Length);
            Array.Clear(globleVariables.g_chDDataArray, 0, globleVariables.g_chDDataArray.Length);
            Array.Clear(globleVariables.g_ch5DataArray, 0, globleVariables.g_ch5DataArray.Length);
            Array.Clear(globleVariables.g_ch6DataArray, 0, globleVariables.g_ch6DataArray.Length);
            Array.Clear(globleVariables.g_ch7DataArray, 0, globleVariables.g_ch7DataArray.Length);
            Array.Clear(globleVariables.g_ch8DataArray, 0, globleVariables.g_ch8DataArray.Length);
            Array.Clear(globleVariables.g_ch9DataArray, 0, globleVariables.g_ch9DataArray.Length);
            Array.Clear(globleVariables.g_ch10DataArray, 0, globleVariables.g_ch10DataArray.Length);
            Array.Clear(globleVariables.g_ch11DataArray, 0, globleVariables.g_ch11DataArray.Length);
            Array.Clear(globleVariables.g_ch12DataArray, 0, globleVariables.g_ch12DataArray.Length);
            Array.Clear(globleVariables.g_ch13DataArray, 0, globleVariables.g_ch13DataArray.Length);
            Array.Clear(globleVariables.g_ch14DataArray, 0, globleVariables.g_ch14DataArray.Length);
            Array.Clear(globleVariables.g_ch15DataArray, 0, globleVariables.g_ch15DataArray.Length);
            Array.Clear(globleVariables.g_ch16DataArray, 0, globleVariables.g_ch16DataArray.Length);

            LayoutRefresh();//根据通道数布局波形显示区的像素高度和位置

            pictureBox_chA.Invalidate();
            pictureBox_chB.Invalidate();
            pictureBox_chC.Invalidate();
            pictureBox_chD.Invalidate();
            pictureBox_ch5.Invalidate();
            pictureBox_ch6.Invalidate();
            pictureBox_ch7.Invalidate();
            pictureBox_ch8.Invalidate();
            pictureBox_ch9.Invalidate();
            pictureBox_ch10.Invalidate();
            pictureBox_ch11.Invalidate();
            pictureBox_ch12.Invalidate();
            pictureBox_ch13.Invalidate();
            pictureBox_ch14.Invalidate();
            pictureBox_ch15.Invalidate();
            pictureBox_ch16.Invalidate();
        }

        private void groupBox_Triger_Enter(object sender, EventArgs e)
        {

        }

        private void TrigerInit() // 20201001 部分新购买的硬件可能需要这些命令, 旧版本2020年10月之前购买的可能不需要调用此方法;
        {
            // 设备1

            // 硬件默认初始化命令，关闭外触发
            g_CtrlByte1 &= 0xdf;
            g_CtrlByte1 |= 0x00;

            lock (globleVariables.g_lockIO)
            {
                DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);

                // 设置触发位置在缓冲区50%(中间位置)
                DLL_1.USBCtrlTrans(0x18, 0xff, 1);
                DLL_1.USBCtrlTrans(0x17, 0x7f, 1);
            }

            // 设备2
            if (globleVariables.g_OSCcnt >= 2) //级联了设备2
            {
                // 硬件默认初始化命令，关闭外触发
                g_2ndOSC_CtrlByte1 &= 0xdf;
                g_2ndOSC_CtrlByte1 |= 0x00;
                lock (globleVariables.g_lockIO2)
                {
                    DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                    // 设置触发位置在缓冲区50%(中间位置)
                    DLL_2.USBCtrlTrans(0x18, 0xff, 1);
                    DLL_2.USBCtrlTrans(0x17, 0x7f, 1);
                }
            }

            // 设备3
            if (globleVariables.g_OSCcnt >= 3) //级联了设备3
            {
                // 硬件默认初始化命令，关闭外触发
                g_3rdOSC_CtrlByte1 &= 0xdf;
                g_3rdOSC_CtrlByte1 |= 0x00;
                lock (globleVariables.g_lockIO3)
                {
                    DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                    // 设置触发位置在缓冲区50%(中间位置)
                    DLL_3.USBCtrlTrans(0x18, 0xff, 1);
                    DLL_3.USBCtrlTrans(0x17, 0x7f, 1);
                }
            }

            // 设备4
            if (globleVariables.g_OSCcnt >= 4) //级联了设备4
            {
                // 硬件默认初始化命令，关闭外触发
                g_4thOSC_CtrlByte1 &= 0xdf;
                g_4thOSC_CtrlByte1 |= 0x00;
                lock (globleVariables.g_lockIO4)
                {
                    DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                    // 设置触发位置在缓冲区50%(中间位置)
                    DLL_4.USBCtrlTrans(0x18, 0xff, 1);
                    DLL_4.USBCtrlTrans(0x17, 0x7f, 1);
                }
            }
        }
        
        private void but_StartDevice_Click(object sender, EventArgs e)
        {
            DLL_1.SpecifyDevIdx(m_DevModelNum);        // 1: 设置当前设备的编号
            DLL_2.SpecifyDevIdx(m_DevModelNum);
            DLL_3.SpecifyDevIdx(m_DevModelNum);
            DLL_4.SpecifyDevIdx(m_DevModelNum);


            // 2：打开设备，在使用设备的功能之前先打开设备，并且只打开成功一次就可以了。

            Int32 res = 0;

            if (globleVariables.g_OSCcnt == 1)//只有一台，不级联
            {
                // 2.1. 打开设备
                res = DLL_1.DeviceOpen(); // 主设备编号1
                if (0 != res) // USB设备打开失败
                {
                    DLL_1.DeviceClose();
                    MessageBox.Show(this, "DeviceOpen Failed (0)!");
                    return;
                }
            }

            // 2.2. 打开第二个设备
            else if (globleVariables.g_OSCcnt >= 2)
            {
                // 2.1. 打开第一个设备
                DLL_1.SetMoreDeviceConcatenation(1); // 主设备
                res = DLL_1.DeviceOpenWithID(1); // 主设备编号1
                if (0 != res) // USB设备打开失败
                {
                    DLL_1.DeviceClose();
                    MessageBox.Show(this, "DeviceOpen Failed (1)!");
                    return;
                }

                DLL_2.SetMoreDeviceConcatenation(2); // 辅设备
                res = DLL_2.DeviceOpenWithID(2); // 第二台辅设备编号为2
                if (0 != res) // USB设备打开失败
                {
                    DLL_1.SetMoreDeviceConcatenation(0);
                    DLL_1.DeviceClose();
                    DLL_2.DeviceClose();
                    MessageBox.Show(this, "DeviceOpen Failed (2)!");
                    return;
                }
            }

            // 2.3. 打开第三个设备
            if (globleVariables.g_OSCcnt >= 3)
            {
                DLL_3.SetMoreDeviceConcatenation(2); // 辅设备
                res = DLL_3.DeviceOpenWithID(3); // 第三台辅设备编号为3
                if (0 != res) // USB设备打开失败
                {
                    DLL_1.SetMoreDeviceConcatenation(0);
                    DLL_1.DeviceClose();
                    DLL_2.SetMoreDeviceConcatenation(0);
                    DLL_2.DeviceClose();
                    DLL_3.DeviceClose();
                    MessageBox.Show(this, "DeviceOpen Failed (3)!");
                    return;
                }
            }

            // 2.4. 打开第四个设备
            if (globleVariables.g_OSCcnt >= 4)
            {
                DLL_4.SetMoreDeviceConcatenation(2); // 辅设备
                res = DLL_4.DeviceOpenWithID(4); // 第四台辅设备编号为4
                if (0 != res) // USB设备打开失败
                {
                    DLL_1.SetMoreDeviceConcatenation(0);
                    DLL_1.DeviceClose();
                    DLL_2.SetMoreDeviceConcatenation(0);
                    DLL_2.DeviceClose();
                    DLL_3.SetMoreDeviceConcatenation(0);
                    DLL_3.DeviceClose();
                    DLL_4.DeviceClose();
                    MessageBox.Show(this, "DeviceOpen Failed (4)!");
                    return;
                }
            }

            // 3 获取数据缓冲区首指针。数据缓冲区开辟的大小为20M字节，可以使用多大受setinfo函数限定。数据格式每个字节代表一个采集电压数据排列为：A通道，B通道，A通道，B通道，A通道，B通道...

            // 3.1 第一个设备
            globleVariables.g_pBuffer = DLL_1.GetBuffer4Wr(-1); //AB通道的数据缓存区
            if ((IntPtr)0 == globleVariables.g_pBuffer)
            {
                MessageBox.Show(this, "GetBuffer Failed (1)!");
                return;
            }
            globleVariables.g_pBufferCD = DLL_1.GetBuffer4Wr(1000); //20250515 获取CD数据缓冲区首指针。数据缓冲区开辟的大小为20M字节，可以使用多大受setinfo函数限定。数据格式每个字节代表一个采集电压数据排列为：C通道，D通道，C通道，D通道，C通道，D通道...
            if ((IntPtr)0 == globleVariables.g_pBufferCD)
            {
                MessageBox.Show(this, "GetBufferCD Failed (1)!");
                return;
            }
            // 3.2 第二个设备
            globleVariables.g_pBuffer_2nd = DLL_2.GetBuffer4Wr(-1);//第二个设备的AB通道，也就是5,6通道的数据缓存区
            if ((IntPtr)0 == globleVariables.g_pBuffer_2nd)
            {
                MessageBox.Show(this, "GetBuffer Failed (2)!");
                return;
            }
            globleVariables.g_pBufferCD2 = DLL_2.GetBuffer4Wr(1000); //20250515 获取CD数据缓冲区首指针。数据缓冲区开辟的大小为20M字节，可以使用多大受setinfo函数限定。数据格式每个字节代表一个采集电压数据排列为：C通道，D通道，C通道，D通道，C通道，D通道...
            if ((IntPtr)0 == globleVariables.g_pBufferCD2)
            {
                MessageBox.Show(this, "GetBufferCD2 Failed (1)!");
                return;
            }
            // 3.3 第三个设备
            globleVariables.g_pBuffer_3rd = DLL_3.GetBuffer4Wr(-1);
            if ((IntPtr)0 == globleVariables.g_pBuffer_3rd)
            {
                MessageBox.Show(this, "GetBuffer Failed (3)!");
                return;
            }
            globleVariables.g_pBufferCD3 = DLL_3.GetBuffer4Wr(1000); //20250515 获取CD数据缓冲区首指针。数据缓冲区开辟的大小为20M字节，可以使用多大受setinfo函数限定。数据格式每个字节代表一个采集电压数据排列为：C通道，D通道，C通道，D通道，C通道，D通道...
            if ((IntPtr)0 == globleVariables.g_pBufferCD3)
            {
                MessageBox.Show(this, "GetBufferCD3 Failed (1)!");
                return;
            }
            // 3.4 第四个设备
            globleVariables.g_pBuffer_4th = DLL_4.GetBuffer4Wr(-1);
            if ((IntPtr)0 == globleVariables.g_pBuffer_4th)
            {
                MessageBox.Show(this, "GetBuffer Failed (4)!");
                return;
            }
            globleVariables.g_pBufferCD4 = DLL_4.GetBuffer4Wr(1000); //20250515 获取CD数据缓冲区首指针。数据缓冲区开辟的大小为20M字节，可以使用多大受setinfo函数限定。数据格式每个字节代表一个采集电压数据排列为：C通道，D通道，C通道，D通道，C通道，D通道...
            if ((IntPtr)0 == globleVariables.g_pBufferCD4)
            {
                MessageBox.Show(this, "GetBufferCD4 Failed (1)!");
                return;
            }
            // 4.
            TrigerInit(); // 初始化硬件触发, 如果触发出问题, 请注释掉这个方法不调用.

            // 5.
            DLL_1.SetInfo(1, 0, 0x11, 0, 0, 64 * 1024 * 2); // 设置使用的缓冲区为128K字节,即每个通道64K字节
            DLL_2.SetInfo(1, 0, 0x11, 0, 0, 64 * 1024 * 2);
            DLL_3.SetInfo(1, 0, 0x11, 0, 0, 64 * 1024 * 2);
            DLL_4.SetInfo(1, 0, 0x11, 0, 0, 64 * 1024 * 2);

            but_StartDevice.Visible = false;
            comboBox_CHs.Enabled = false;
            but_StopDevice.Visible= true;         
      
            groupCoupling.Enabled = true;
        
            butRead.Enabled = true;

            groupBoxCHBset.Enabled = true;
       

            //做一些初始化工作
            radio_CHA_DC.Checked = true;
            radio_CHB_DC.Checked = true;
            groupBoxSPS.Enabled = true;
            groupBoxCHA.Enabled = true;
            groupBoxCHB.Enabled = true;
            groupBoxCHC.Enabled = true;
            groupBoxCHD.Enabled = true;
            checkBox_TriggerOn.Enabled = true;
            but_SingleRead.Enabled = true;
            but_CycleRead.Enabled = true;
            butGetCalibration.Enabled = true;
            groupBoxTrigSet.Enabled = true;
    
            radioBut_CHB_ON.Checked = true;

            butGetCalibration_Click(sender, e);////20230726 jiangtao.lv获取示波器的校准数据用来校准采集到的原始字节和真实电压之间的映射关系

            double TriggerLevel = globleVariables.dataTransform((byte)track_TriggerLevel.Value, globleVariables.g_CurrentZero0, globleVariables.g_CurrentScale_ch0, globleVariables.RangeV);//将设备1的原始字节数据转换成真正的电压值
            label_Tvalue.Text = TriggerLevel.ToString("0.00");

            ActivateSamplingRate();//发送采样率命令实施采样率切换
            ActivateRangeCHA();//发送命令实施通道A的电压测试范围设置
            ActivateRangeCHB();//发送命令实施通道B的电压测试范围设置
            ActivateRangeCHC();//发送命令实施通道C的电压测试范围设置
            ActivateRangeCHD();//发送命令实施通道D的电压测试范围设置
            ActivateTriggerONoff();//发送命令，是否开启触发功能
            ActivateTriggerEdge();//发送命令让示波器设备1设置执行触发边沿。            

        }

        private void but_StopDevice_Click(object sender, EventArgs e)
        {
            DLL_1.SetMoreDeviceConcatenation(0);
            DLL_1.DeviceClose();   //关闭设备

            DLL_2.SetMoreDeviceConcatenation(0);
            DLL_2.DeviceClose();   //关闭设备

            DLL_3.SetMoreDeviceConcatenation(0);
            DLL_3.DeviceClose();   //关闭设备

            DLL_4.SetMoreDeviceConcatenation(0);
            DLL_4.DeviceClose();   //关闭设备


            but_StartDevice.Visible = true;
            comboBox_CHs.Enabled = true;
            but_StopDevice.Visible = false;
           
     
            groupCoupling.Enabled = false;
        
            butRead.Enabled = false; 
            groupBoxSPS.Enabled = false;
            groupBoxCHA.Enabled = false;
            groupBoxCHB.Enabled = false;
            groupBoxCHC.Enabled = false;
            groupBoxCHD.Enabled = false;
            checkBox_TriggerOn.Enabled = false;
            but_SingleRead.Enabled = false;
            but_CycleRead.Enabled = false;
            butGetCalibration.Enabled = false;
            groupBoxTrigSet.Enabled = false;
            groupBoxCHBset.Enabled = false;
        } 

        private void radio_CHA_DC_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_CHA_DC.Checked)
            {
                // 设备1
                g_CtrlByte0 &= 0xef; // 设置chA为DC耦合
                g_CtrlByte0 |= 0x10; // 设置chA为DC耦合
                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x94, g_CtrlByte0, 1); // 设备1 设置chA为DC耦合
                }
                // 第二个设备
                if (globleVariables.g_OSCcnt >= 2)
                {
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x94, g_CtrlByte0, 1); // 设备2 设置chC为DC耦合
                    }
                }
                // 第3个设备
                if (globleVariables.g_OSCcnt >= 3)
                {
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x94, g_CtrlByte0, 1); // 设备3 设置ch5为DC耦合
                    }
                }
                // 第4个设备
                if (globleVariables.g_OSCcnt >= 4)
                {
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x94, g_CtrlByte0, 1); // 设备4 设置ch6为DC耦合
                    }
                }
            }
        }

        private void radio_CHA_AC_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_CHA_AC.Checked)
            {
                g_CtrlByte0 &= 0xef;                // 设置chA为AC耦合
                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x94, g_CtrlByte0, 1); // 设备1 设置chA为AC耦合
                }
                // 第二个设备
                if (globleVariables.g_OSCcnt >= 2)
                {
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x94, g_CtrlByte0, 1); // 设备2 设置chC为AC耦合
                    }
                }
                // 第3个设备
                if (globleVariables.g_OSCcnt >= 3)
                {
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x94, g_CtrlByte0, 1); // 设备3 设置ch5为AC耦合
                    }
                }
                // 第4个设备
                if (globleVariables.g_OSCcnt >= 4)
                {
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x94, g_CtrlByte0, 1); // 设备4 设置ch7为AC耦合
                    }
                }
            }
        }

        private void radio_CHB_DC_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_CHB_DC.Checked)
            {
                g_CtrlByte1 &= 0xef; // 设置chB为DC耦合
                g_CtrlByte1 |= 0x10; // 设置chB为DC耦合

                g_2ndOSC_CtrlByte1 &= 0xef; // 设置chB为DC耦合.设备4
                g_2ndOSC_CtrlByte1 |= 0x10; // 设置chB为DC耦合.设备4

                g_3rdOSC_CtrlByte1 &= 0xef; // 设置chB为DC耦合.设备4
                g_3rdOSC_CtrlByte1 |= 0x10; // 设置chB为DC耦合.设备4

                g_4thOSC_CtrlByte1 &= 0xef; // 设置chB为DC耦合.设备4
                g_4thOSC_CtrlByte1 |= 0x10; // 设置chB为DC耦合.设备4



                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1); // 设备1  设置chB为DC耦合 
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1); // 设备2  设置chD为DC耦合 
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1); // 设备3  设置ch6为DC耦合 
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1); // 设备4  设置ch8为DC耦合 
                    }
                }
            }
        }

        private void radio_CHB_AC_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_CHB_AC.Checked)
            {
                g_CtrlByte1 &= 0xef; // 设置chB为AC耦合
                g_CtrlByte1 |= 0x00; // 设置chB为AC耦合


                g_2ndOSC_CtrlByte1 &= 0xef; // 设置chB为AC耦合.设备4
                g_2ndOSC_CtrlByte1 |= 0x00; // 设置chB为AC耦合.设备4

                g_3rdOSC_CtrlByte1 &= 0xef; // 设置chB为AC耦合.设备4
                g_3rdOSC_CtrlByte1 |= 0x00; // 设置chB为AC耦合.设备4

                g_4thOSC_CtrlByte1 &= 0xef; // 设置chB为AC耦合.设备4
                g_4thOSC_CtrlByte1 |= 0x00; // 设置chB为AC耦合.设备4

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1); // 设备1  设置chB为AC耦合
                }
                lock (globleVariables.g_lockIO2)
                {
                    DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1); // 设备2  设置chD为AC耦合 
                }
                lock (globleVariables.g_lockIO3)
                {
                    DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1); // 设备3  设置ch6为AC耦合 
                }
                lock (globleVariables.g_lockIO4)
                {
                    DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1); // 设备4  设置ch8为AC耦合 
                }
            }
        }

        public void LayoutRefresh()//根据通道数布局波形显示区的像素高度和位置
        {
            int interval = 1; //统一的边界间隔
            transformer_tab.Width = groupBox2.Left - transformer_tab.Left;            

            int WidthAll = tabPageBasic.Width/2;
            int HeightALL = transformer_tab.Height - 6 * interval;

            if ((transformer_tab.Width <= 0) || (HeightALL <= 0))  {   return;  }

            int Height4 = (int)(HeightALL / 2);
            int Height8 = (int)(HeightALL / 4);
            int Height12 = (int)(HeightALL / 6);
            int Height16 = (int)(HeightALL / 8);

    
            pictureBox_ch5.Top = 2000;
            pictureBox_ch6.Top = 2000;
            pictureBox_ch7.Top = 2000;
            pictureBox_ch8.Top = 2000;
            pictureBox_ch9.Top = 2000;
            pictureBox_ch10.Top = 2000;
            pictureBox_ch11.Top = 2000;
            pictureBox_ch12.Top = 2000;
            pictureBox_ch13.Top = 2000;
            pictureBox_ch14.Top = 2000;
            pictureBox_ch15.Top = 2000;
            pictureBox_ch16.Top = 2000;

            pictureBox_chA.Width = WidthAll; //20250516
            pictureBox_chB.Width = WidthAll; //20250516
            pictureBox_chC.Width = WidthAll; //20250516
            pictureBox_chD.Width = WidthAll; //20250516
            pictureBox_ch5.Width = WidthAll; //20250516
            pictureBox_ch6.Width = WidthAll; //20250516
            pictureBox_ch7.Width = WidthAll; //20250516
            pictureBox_ch8.Width = WidthAll; //20250516
            pictureBox_ch9.Width = WidthAll; //20250516
            pictureBox_ch10.Width = WidthAll; //20250516
            pictureBox_ch11.Width = WidthAll; //20250516
            pictureBox_ch12.Width = WidthAll; //20250516
            pictureBox_ch13.Width = WidthAll; //20250516
            pictureBox_ch14.Width = WidthAll; //20250516
            pictureBox_ch15.Width = WidthAll; //20250516
            pictureBox_ch16.Width = WidthAll; //20250516

           if (globleVariables.g_OSCcnt == 1)///只有一台，不级联，4个通道 //20250515       
            {
                pictureBox_chA.Top = transformer_tab.Top ;
                pictureBox_chA.Height = Height4;
                pictureBox_chB.Top = pictureBox_chA.Top;
                pictureBox_chB.Height = Height4;
                pictureBox_chC.Top = pictureBox_chB.Bottom;
                pictureBox_chC.Height = Height4;
                pictureBox_chD.Top = pictureBox_chC.Top;
                pictureBox_chD.Height = Height4;

                pictureBox_chB.Left = pictureBox_chA.Right + interval;
                pictureBox_chD.Left = pictureBox_chC.Right + interval;
            }
            else if (globleVariables.g_OSCcnt == 2)//2级联，8个通道 //20250515   
            {
                pictureBox_chA.Top = transformer_tab.Top;
                pictureBox_chA.Height = Height8;
                pictureBox_chB.Top = pictureBox_chA.Top;
                pictureBox_chB.Height = Height8;
                pictureBox_chC.Top = pictureBox_chB.Bottom;
                pictureBox_chC.Height = Height8;
                pictureBox_chD.Top = pictureBox_chC.Top;
                pictureBox_chD.Height = Height8;

                pictureBox_chB.Left = pictureBox_chA.Right + interval;
                pictureBox_chD.Left = pictureBox_chC.Right + interval;

                pictureBox_ch5.Top = pictureBox_chD.Bottom;
                pictureBox_ch5.Height = Height8;
                pictureBox_ch6.Top = pictureBox_ch5.Top;
                pictureBox_ch6.Height = Height8;
                pictureBox_ch7.Top = pictureBox_ch5.Bottom;
                pictureBox_ch7.Height = Height8;
                pictureBox_ch8.Top = pictureBox_ch7.Top;
                pictureBox_ch8.Height = Height8;

                pictureBox_ch6.Left = pictureBox_ch5.Right + interval;
                pictureBox_ch8.Left = pictureBox_ch7.Right + interval;
            }
            else if (globleVariables.g_OSCcnt == 3)//3级联
            {
                pictureBox_chA.Top = transformer_tab.Top;
                pictureBox_chA.Height = Height12;
                pictureBox_chB.Top = pictureBox_chA.Top;
                pictureBox_chB.Height = Height12;
                pictureBox_chC.Top = pictureBox_chB.Bottom;
                pictureBox_chC.Height = Height12;
                pictureBox_chD.Top = pictureBox_chC.Top;
                pictureBox_chD.Height = Height12;

                pictureBox_chB.Left = pictureBox_chA.Right + interval;
                pictureBox_chD.Left = pictureBox_chC.Right + interval;

                pictureBox_ch5.Top = pictureBox_chD.Bottom;
                pictureBox_ch5.Height = Height12;
                pictureBox_ch6.Top = pictureBox_ch5.Top;
                pictureBox_ch6.Height = Height12;
                pictureBox_ch7.Top = pictureBox_ch5.Bottom;
                pictureBox_ch7.Height = Height12;
                pictureBox_ch8.Top = pictureBox_ch7.Top;
                pictureBox_ch8.Height = Height12;

                pictureBox_ch6.Left = pictureBox_ch5.Right + interval;
                pictureBox_ch8.Left = pictureBox_ch7.Right + interval;

                pictureBox_ch9.Top = pictureBox_ch8.Bottom;
                pictureBox_ch9.Height = Height12;
                pictureBox_ch10.Top = pictureBox_ch9.Top;
                pictureBox_ch10.Height = Height12;
                pictureBox_ch11.Top = pictureBox_ch9.Bottom;
                pictureBox_ch11.Height = Height12;
                pictureBox_ch12.Top = pictureBox_ch11.Top;
                pictureBox_ch12.Height = Height12;

                pictureBox_ch10.Left = pictureBox_ch9.Right + interval;
                pictureBox_ch12.Left = pictureBox_ch11.Right + interval;
            }
            else if (globleVariables.g_OSCcnt == 4)//4级联
            {
                pictureBox_chA.Top = transformer_tab.Top;
                pictureBox_chA.Height = Height16;
                pictureBox_chB.Top = pictureBox_chA.Top;
                pictureBox_chB.Height = Height16;
                pictureBox_chC.Top = pictureBox_chB.Bottom;
                pictureBox_chC.Height = Height16;
                pictureBox_chD.Top = pictureBox_chC.Top;
                pictureBox_chD.Height = Height16;

                pictureBox_chB.Left = pictureBox_chA.Right + interval;
                pictureBox_chD.Left = pictureBox_chC.Right + interval;

                pictureBox_ch5.Top = pictureBox_chD.Bottom;
                pictureBox_ch5.Height = Height16;
                pictureBox_ch6.Top = pictureBox_ch5.Top;
                pictureBox_ch6.Height = Height16;
                pictureBox_ch7.Top = pictureBox_ch5.Bottom;
                pictureBox_ch7.Height = Height16;
                pictureBox_ch8.Top = pictureBox_ch7.Top;
                pictureBox_ch8.Height = Height16;

                pictureBox_ch6.Left = pictureBox_ch5.Right + interval;
                pictureBox_ch8.Left = pictureBox_ch7.Right + interval;

                pictureBox_ch9.Top = pictureBox_ch8.Bottom;
                pictureBox_ch9.Height = Height16;
                pictureBox_ch10.Top = pictureBox_ch9.Top;
                pictureBox_ch10.Height = Height16;
                pictureBox_ch11.Top = pictureBox_ch9.Bottom;
                pictureBox_ch11.Height = Height16;
                pictureBox_ch12.Top = pictureBox_ch11.Top;
                pictureBox_ch12.Height = Height16;

                pictureBox_ch10.Left = pictureBox_ch9.Right + interval;
                pictureBox_ch12.Left = pictureBox_ch11.Right + interval;

                pictureBox_ch13.Top = pictureBox_ch12.Bottom;
                pictureBox_ch13.Height = Height16;
                pictureBox_ch14.Top = pictureBox_ch13.Top;
                pictureBox_ch14.Height = Height16;
                pictureBox_ch15.Top = pictureBox_ch13.Bottom;
                pictureBox_ch15.Height = Height16;
                pictureBox_ch16.Top = pictureBox_ch15.Top;
                pictureBox_ch16.Height = Height16;

                pictureBox_ch14.Left = pictureBox_ch13.Right + interval;
                pictureBox_ch16.Left = pictureBox_ch15.Right + interval;

            }
            label_CHA.Left = pictureBox_chA.Left + interval;
            label_CHA.Top = pictureBox_chA.Top + interval;
            label_PPvalue_A.Top = label_CHA.Top;

            label_CHB.Left = pictureBox_chB.Left + interval;
            label_CHB.Top = pictureBox_chB.Top + interval;
            label_PPvalue_B.Top = label_CHB.Top;

            label_CHC.Left = pictureBox_chC.Left + interval;
            label_CHC.Top = pictureBox_chC.Top + interval;
            label_PPvalue_C.Top = label_CHC.Top;

            label_CHD.Left = pictureBox_chD.Left + interval;
            label_CHD.Top = pictureBox_chD.Top + interval;
            label_PPvalue_D.Top = label_CHD.Top;

            label_CH5.Left = pictureBox_ch5.Left + interval;
            label_CH5.Top = pictureBox_ch5.Top + interval;
            label_PPvalue_5.Top = label_CH5.Top;

            label_CH6.Left = pictureBox_ch6.Left + interval;
            label_CH6.Top = pictureBox_ch6.Top + interval;
            label_PPvalue_6.Top = label_CH6.Top;

            label_CH7.Left = pictureBox_ch7.Left + interval;
            label_CH7.Top = pictureBox_ch7.Top + interval;  

            label_CH8.Left = pictureBox_ch8.Left + interval;
            label_CH8.Top = pictureBox_ch8.Top + interval;/**/     

            label_CH9.Left = pictureBox_ch9.Left + interval;
            label_CH9.Top = pictureBox_ch9.Top + interval;/**/
            label_PPvalue_9.Top = label_CH9.Top;

            label_CH10.Left = pictureBox_ch10.Left + interval;
            label_CH10.Top = pictureBox_ch10.Top + interval; 
            label_PPvalue_10.Top = label_CH10.Top;

            label_CH11.Left = pictureBox_ch11.Left + interval;
            label_CH11.Top = pictureBox_ch11.Top + interval; 

            label_CH12.Left = pictureBox_ch12.Left + interval;
            label_CH12.Top = pictureBox_ch12.Top + interval;   

            label_CH13.Left = pictureBox_ch13.Left + interval;
            label_CH13.Top = pictureBox_ch13.Top + interval;
            label_PPvalue_13.Top = label_CH13.Top;

            label_CH14.Left = pictureBox_ch14.Left + interval;
            label_CH14.Top = pictureBox_ch14.Top + interval;
            label_PPvalue_14.Top = label_CH14.Top;

            label_CH15.Left = pictureBox_ch15.Left + interval;
            label_CH15.Top = pictureBox_ch15.Top + interval;
            label_CH16.Left = pictureBox_ch16.Left + interval;
            label_CH16.Top = pictureBox_ch16.Top + interval;
        }      
 

        private void radioBut_CHB_ON_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBut_CHB_ON.Checked) // 通道B开启 
            {
                g_CtrlByte1 &= 0xfe;
                g_CtrlByte1 |= 0x01;

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1); // 设备1 B通道
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    g_2ndOSC_CtrlByte1 &= 0xfe; // 通道B开启 .设备2
                    g_2ndOSC_CtrlByte1 |= 0x01; // 通道B开启 .设备2
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1); // 设备2 D通道
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {               
                    g_3rdOSC_CtrlByte1 &= 0xfe; // 通道B开启 .设备3
                    g_3rdOSC_CtrlByte1 |= 0x01; // 通道B开启 .设备3
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1); // 设备3 6通道
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {                
                    g_4thOSC_CtrlByte1 &= 0xfe; // 通道B开启 .设备4
                    g_4thOSC_CtrlByte1 |= 0x01; // 通道B开启 .设备4
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1); // 设备4 8通道
                    }
                }
            }
        }

        private void radioBut_CHB_OFF_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBut_CHB_OFF.Checked) // 通道B关闭
            {
                g_CtrlByte1 &= 0xfe;
                g_CtrlByte1 |= 0x00;

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1); // 设备1 B通道
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    g_2ndOSC_CtrlByte1 &= 0xfe; // 通道B关闭 .设备2
                    g_2ndOSC_CtrlByte1 |= 0x00; // 通道B关闭 .设备2
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1); // 设备2 D通道
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    g_3rdOSC_CtrlByte1 &= 0xfe; // 通道B关闭 .设备3
                    g_3rdOSC_CtrlByte1 |= 0x00; // 通道B关闭 .设备3
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1); // 设备3 6通道
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    g_4thOSC_CtrlByte1 &= 0xfe; // 通道B关闭 .设备4
                    g_4thOSC_CtrlByte1 |= 0x00; // 通道B关闭 .设备4
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1); // 设备4 8通道
                    }
                }
            }
        }
        public void ActivateRangeCHA()//发送采样率命令实施通道A的电压测试范围设置
        {
            globleVariables.RangeV = 10;

            if (comboRangeA.SelectedIndex == 0) // chA 输入量程设置为：-8V ~ +8V
            {
                g_CtrlByte1 &= 0xF7;
                g_CtrlByte1 |= 0x08;

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x22, 0x00, 1); // 设备1 chA输入量程设置为：-8V ~ +8V 
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    g_2ndOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：-8V ~ +8V .设备2
                    g_2ndOSC_CtrlByte1 |= 0x08; // chA 输入量程设置为：-8V ~ +8V .设备2
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x22, 0x00, 1); // 设备2 chC
                        DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    g_3rdOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：-8V ~ +8V .设备3
                    g_3rdOSC_CtrlByte1 |= 0x08; // chA 输入量程设置为：-8V ~ +8V .设备3
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x22, 0x00, 1); // 设备3 ch5
                        DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    g_4thOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：-8V ~ +8V .设备4
                    g_4thOSC_CtrlByte1 |= 0x08; // chA 输入量程设置为：-8V ~ +8V .设备4
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x22, 0x00, 1); // 设备4 ch7
                        DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                    }
                }
                globleVariables.g_CurrentZero0 = globleVariables.g_VbiasZero02v;
                globleVariables.g_CurrentScale_ch0 = globleVariables.g_VbiasScale_2V_ch0;

                globleVariables.g_2ndOSC_CurrentZero0 = globleVariables.g_2ndOSC_VbiasZero02v;
                globleVariables.g_2ndOSC_CurrentScale_ch0 = globleVariables.g_2ndOSC_VbiasScale_2V_ch0;
                globleVariables.g_3rdOSC_CurrentZero0 = globleVariables.g_3rdOSC_VbiasZero02v;
                globleVariables.g_3rdOSC_CurrentScale_ch0 = globleVariables.g_3rdOSC_VbiasScale_2V_ch0;
                globleVariables.g_4thOSC_CurrentZero0 = globleVariables.g_4thOSC_VbiasZero02v;
                globleVariables.g_4thOSC_CurrentScale_ch0 = globleVariables.g_4thOSC_VbiasScale_2V_ch0;

                globleVariables.RangeV = 20;
                globleVariables.RangeV_2 = 20;
                globleVariables.RangeV_3 = 20;
                globleVariables.RangeV_4 = 20;
            }


            if (comboRangeA.SelectedIndex == 1) // chA 输入量程设置为：-5V ~ +5V
            {
                g_CtrlByte1 &= 0xF7;
                g_CtrlByte1 |= 0x08;

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x22, 0x02, 1); // 设备1 chA
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    g_2ndOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：-5V ~ +5V .设备2
                    g_2ndOSC_CtrlByte1 |= 0x08; // chA 输入量程设置为：-5V ~ +5V .设备2
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x22, 0x02, 1); // 设备2 chC
                        DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    g_3rdOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：-5V ~ +5V .设备3
                    g_3rdOSC_CtrlByte1 |= 0x08; // chA 输入量程设置为：-5V ~ +5V .设备3
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x22, 0x02, 1); // 设备3 ch5
                        DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    g_4thOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：-5V ~ +5V .设备4
                    g_4thOSC_CtrlByte1 |= 0x08; // chA 输入量程设置为：-5V ~ +5V .设备4
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x22, 0x02, 1); // 设备4 ch7
                        DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                    }
                }
                globleVariables.g_CurrentZero0 = globleVariables.g_VbiasZero01v;
                globleVariables.g_CurrentScale_ch0 = globleVariables.g_VbiasScale_1V_ch0;

                globleVariables.g_2ndOSC_CurrentZero0 = globleVariables.g_2ndOSC_VbiasZero01v;
                globleVariables.g_2ndOSC_CurrentScale_ch0 = globleVariables.g_2ndOSC_VbiasScale_1V_ch0;
                globleVariables.g_3rdOSC_CurrentZero0 = globleVariables.g_3rdOSC_VbiasZero01v;
                globleVariables.g_3rdOSC_CurrentScale_ch0 = globleVariables.g_3rdOSC_VbiasScale_1V_ch0;
                globleVariables.g_4thOSC_CurrentZero0 = globleVariables.g_4thOSC_VbiasZero01v;
                globleVariables.g_4thOSC_CurrentScale_ch0 = globleVariables.g_4thOSC_VbiasScale_1V_ch0;

                globleVariables.RangeV = 10;
                globleVariables.RangeV_2 = 10;
                globleVariables.RangeV_3 = 10;
                globleVariables.RangeV_4 = 10;
            }
            else if (comboRangeA.SelectedIndex == 2) // // chA 输入量程设置为：+-2.5v
            {
                g_CtrlByte1 &= 0xF7;
                g_CtrlByte1 |= 0x08;

                g_2ndOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-2.5v .设备2
                g_2ndOSC_CtrlByte1 |= 0x08; // chA 输入量程设置为：+-2.5v .设备2

                g_3rdOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-2.5v .设备3
                g_3rdOSC_CtrlByte1 |= 0x08; // chA 输入量程设置为：+-2.5v .设备3

                g_4thOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-2.5v .设备4
                g_4thOSC_CtrlByte1 |= 0x08; // chA 输入量程设置为：+-2.5v .设备4

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x22, 0x04, 1); // 设备1 chA
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                }
                lock (globleVariables.g_lockIO2)
                {
                    DLL_2.USBCtrlTrans(0x22, 0x04, 1); // 设备2 chC
                    DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                }
                lock (globleVariables.g_lockIO3)
                {
                    DLL_3.USBCtrlTrans(0x22, 0x04, 1); // 设备3 ch5
                    DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                }
                lock (globleVariables.g_lockIO4)
                {
                    DLL_4.USBCtrlTrans(0x22, 0x04, 1); // 设备4 ch7
                    DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                }
                globleVariables.g_CurrentZero0 = globleVariables.g_VbiasZero0500mv;
                globleVariables.g_CurrentScale_ch0 = globleVariables.g_VbiasScale_500mV_ch0;
                globleVariables.g_2ndOSC_CurrentZero0 = globleVariables.g_2ndOSC_VbiasZero0500mv;
                globleVariables.g_2ndOSC_CurrentScale_ch0 = globleVariables.g_2ndOSC_VbiasScale_500mV_ch0;
                globleVariables.g_3rdOSC_CurrentZero0 = globleVariables.g_3rdOSC_VbiasZero0500mv;
                globleVariables.g_3rdOSC_CurrentScale_ch0 = globleVariables.g_3rdOSC_VbiasScale_500mV_ch0;
                globleVariables.g_4thOSC_CurrentZero0 = globleVariables.g_4thOSC_VbiasZero0500mv;
                globleVariables.g_4thOSC_CurrentScale_ch0 = globleVariables.g_4thOSC_VbiasScale_500mV_ch0;

                globleVariables.RangeV = 5;
                globleVariables.RangeV_2 = 5;
                globleVariables.RangeV_3 = 5;
                globleVariables.RangeV_4 = 5;
            }
            else if (comboRangeA.SelectedIndex == 3) // // chA 输入量程设置为：+-1v
            {
                g_CtrlByte1 &= 0xF7;
                g_CtrlByte1 |= 0x08;

                g_2ndOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-1v .设备2
                g_2ndOSC_CtrlByte1 |= 0x08; // chA 输入量程设置为：+-1v .设备2

                g_3rdOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-1v .设备3
                g_3rdOSC_CtrlByte1 |= 0x08; // chA 输入量程设置为：+-1v .设备3

                g_4thOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-1v .设备4
                g_4thOSC_CtrlByte1 |= 0x08; // chA 输入量程设置为：+-1v .设备4

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x22, 0x06, 1); // 设备1 chA
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                }
                lock (globleVariables.g_lockIO2)
                {
                    DLL_2.USBCtrlTrans(0x22, 0x06, 1); // 设备2 chC
                    DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                }
                lock (globleVariables.g_lockIO3)
                {
                    DLL_3.USBCtrlTrans(0x22, 0x06, 1); // 设备3 ch5
                    DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                }
                lock (globleVariables.g_lockIO4)
                {
                    DLL_4.USBCtrlTrans(0x22, 0x06, 1); // 设备4 ch7
                    DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                }
      
                globleVariables.g_CurrentZero0 = globleVariables.g_VbiasZero0200mv;
                globleVariables.g_CurrentScale_ch0 = globleVariables.g_VbiasScale_200mV_ch0;
                globleVariables.g_2ndOSC_CurrentZero0 = globleVariables.g_2ndOSC_VbiasZero0200mv;
                globleVariables.g_2ndOSC_CurrentScale_ch0 = globleVariables.g_2ndOSC_VbiasScale_200mV_ch0;
                globleVariables.g_3rdOSC_CurrentZero0 = globleVariables.g_3rdOSC_VbiasZero0200mv;
                globleVariables.g_3rdOSC_CurrentScale_ch0 = globleVariables.g_3rdOSC_VbiasScale_200mV_ch0;
                globleVariables.g_4thOSC_CurrentZero0 = globleVariables.g_4thOSC_VbiasZero0200mv;
                globleVariables.g_4thOSC_CurrentScale_ch0 = globleVariables.g_4thOSC_VbiasScale_200mV_ch0;

                globleVariables.RangeV = 2;
                globleVariables.RangeV_2 = 2;
                globleVariables.RangeV_3 = 2;
                globleVariables.RangeV_4 = 2;
            }
            else if (comboRangeA.SelectedIndex == 4) // // chA 输入量程设置为：+-500mv
            {                
                    g_CtrlByte1 &= 0xF7;

                    lock (globleVariables.g_lockIO)
                    {
                        DLL_1.USBCtrlTrans(0x22, 0x02, 1); // 设备1 chA
                        DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                    }
                    if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                    {
                        g_2ndOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-500mv .设备2   
                        lock (globleVariables.g_lockIO2)
                        {
                            DLL_2.USBCtrlTrans(0x22, 0x02, 1); // 设备2 chC
                            DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                        }
                    }
                    if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                    { 
                        g_3rdOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-500mv .设备3  
                        lock (globleVariables.g_lockIO3)
                        {
                            DLL_3.USBCtrlTrans(0x22, 0x02, 1); // 设备3 ch5
                            DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                        }
                    }
                    if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                    {
                        g_4thOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-500mv .设备4
                        lock (globleVariables.g_lockIO4)
                        {
                            DLL_4.USBCtrlTrans(0x22, 0x02, 1); // 设备4 ch7
                            DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                        }
                    }
                    globleVariables.g_CurrentZero0 = globleVariables.g_VbiasZero0100mv;
                    globleVariables.g_CurrentScale_ch0 = globleVariables.g_VbiasScale_100mV_ch0;

                    globleVariables.g_2ndOSC_CurrentZero0 = globleVariables.g_2ndOSC_VbiasZero0100mv;
                    globleVariables.g_2ndOSC_CurrentScale_ch0 = globleVariables.g_2ndOSC_VbiasScale_100mV_ch0;
                    globleVariables.g_3rdOSC_CurrentZero0 = globleVariables.g_3rdOSC_VbiasZero0100mv;
                    globleVariables.g_3rdOSC_CurrentScale_ch0 = globleVariables.g_3rdOSC_VbiasScale_100mV_ch0;
                    globleVariables.g_4thOSC_CurrentZero0 = globleVariables.g_4thOSC_VbiasZero0100mv;
                    globleVariables.g_4thOSC_CurrentScale_ch0 = globleVariables.g_4thOSC_VbiasScale_100mV_ch0;

                    globleVariables.RangeV = 1;
                    globleVariables.RangeV_2 = 1;
                    globleVariables.RangeV_3 = 1;
                    globleVariables.RangeV_4 = 1;
            }
            else if (comboRangeA.SelectedIndex == 5) // chA 输入量程设置为：+-250mv
            {
                g_CtrlByte1 &= 0xF7;

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x22, 0x04, 1); // 设备1 chA
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    g_2ndOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-250mv .设备2    
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x22, 0x04, 1); // 设备2 chC
                        DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    g_3rdOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-250mv .设备3  
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x22, 0x04, 1); // 设备3 ch5
                        DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    g_4thOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-250mv .设备4
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x22, 0x04, 1); // 设备4 ch7
                        DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                    }
                }
                globleVariables.g_CurrentZero0 = globleVariables.g_VbiasZero050mv;
                globleVariables.g_CurrentScale_ch0 = globleVariables.g_VbiasScale_50mV_ch0;

                globleVariables.g_2ndOSC_CurrentZero0 = globleVariables.g_2ndOSC_VbiasZero050mv;
                globleVariables.g_2ndOSC_CurrentScale_ch0 = globleVariables.g_2ndOSC_VbiasScale_50mV_ch0;
                globleVariables.g_3rdOSC_CurrentZero0 = globleVariables.g_3rdOSC_VbiasZero050mv;
                globleVariables.g_3rdOSC_CurrentScale_ch0 = globleVariables.g_3rdOSC_VbiasScale_50mV_ch0;
                globleVariables.g_4thOSC_CurrentZero0 = globleVariables.g_4thOSC_VbiasZero050mv;
                globleVariables.g_4thOSC_CurrentScale_ch0 = globleVariables.g_4thOSC_VbiasScale_50mV_ch0;

                globleVariables.RangeV = 0.5;
                globleVariables.RangeV_2 = 0.5;
                globleVariables.RangeV_3 = 0.5;
                globleVariables.RangeV_4 = 0.5;
            }
            else if (comboRangeA.SelectedIndex == 6) // // chA 输入量程设置为：+-100mv
            {
                g_CtrlByte1 &= 0xF7;
                g_2ndOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-100mv .设备2               

                g_3rdOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-100mv .设备3            

                g_4thOSC_CtrlByte1 &= 0xF7; // chA 输入量程设置为：+-100mv .设备4

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x22, 0x06, 1); // 设备1 chA
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x22, 0x06, 1); // 设备2 chC
                        DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x22, 0x06, 1); // 设备3 ch5
                        DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x22, 0x06, 1); // 设备4 ch7
                        DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                    }
                }
                globleVariables.g_CurrentZero0 = globleVariables.g_VbiasZero020mv;
                globleVariables.g_CurrentScale_ch0 = globleVariables.g_VbiasScale_20mV_ch0;

                globleVariables.g_2ndOSC_CurrentZero0 = globleVariables.g_2ndOSC_VbiasZero020mv;
                globleVariables.g_2ndOSC_CurrentScale_ch0 = globleVariables.g_2ndOSC_VbiasScale_20mV_ch0;
                globleVariables.g_3rdOSC_CurrentZero0 = globleVariables.g_3rdOSC_VbiasZero020mv;
                globleVariables.g_3rdOSC_CurrentScale_ch0 = globleVariables.g_3rdOSC_VbiasScale_20mV_ch0;
                globleVariables.g_4thOSC_CurrentZero0 = globleVariables.g_4thOSC_VbiasZero020mv;
                globleVariables.g_4thOSC_CurrentScale_ch0 = globleVariables.g_4thOSC_VbiasScale_20mV_ch0;

                globleVariables.RangeV = 0.2;
                globleVariables.RangeV_2 = 0.2;
                globleVariables.RangeV_3 = 0.2;
                globleVariables.RangeV_4 = 0.2;
            }        
        }

        public void ActivateRangeCHB()//发送采样率命令实施通道A的电压测试范围设置
        {
            globleVariables.RangeV_B = 10;  

            if (comboRangeB.SelectedIndex == 0) // chB 输入量程设置为：-8V ~ +8V
            {
                g_CtrlByte1 &= 0xF9;                  

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x23, 0x00, 1); // 设备1 chB
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    g_2ndOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备2 
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x23, 0x00, 1); // 设备2 chD
                        DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    g_3rdOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备3   
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x23, 0x00, 1); // 设备3 ch6
                        DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    g_4thOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备4
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x23, 0x00, 1); // 设备4 ch8
                        DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                    }
                }
                globleVariables.g_CurrentZero1 = globleVariables.g_VbiasZero11v;
                globleVariables.g_CurrentScale_ch1 = globleVariables.g_VbiasScale_1V_ch1;

                globleVariables.g_2ndOSC_CurrentZero1 = globleVariables.g_2ndOSC_VbiasZero11v;
                globleVariables.g_2ndOSC_CurrentScale_ch1 = globleVariables.g_2ndOSC_VbiasScale_1V_ch1;
                globleVariables.g_3rdOSC_CurrentZero1 = globleVariables.g_3rdOSC_VbiasZero11v;
                globleVariables.g_3rdOSC_CurrentScale_ch1 = globleVariables.g_3rdOSC_VbiasScale_1V_ch1;
                globleVariables.g_4thOSC_CurrentZero1 = globleVariables.g_4thOSC_VbiasZero11v;
                globleVariables.g_4thOSC_CurrentScale_ch1 = globleVariables.g_4thOSC_VbiasScale_1V_ch1;

                globleVariables.RangeV_B = 20;
                globleVariables.RangeV_2B = 20;
                globleVariables.RangeV_3B = 20;
                globleVariables.RangeV_4B = 20;
            }

            if (comboRangeB.SelectedIndex == 1) // chB 输入量程设置为：-5V ~ +5V
            {
                    g_CtrlByte1 &= 0xF9;
                    g_CtrlByte1 |= 0x02;

                    lock (globleVariables.g_lockIO)
                    {
                        DLL_1.USBCtrlTrans(0x23, 0x00, 1); // 设备1, chB
                        DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                    }
                    if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                    {
                        g_2ndOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备2
                        g_2ndOSC_CtrlByte1 |= 0x02; // chB 输入量程设置为： .设备2
                        lock (globleVariables.g_lockIO2)
                        {
                            DLL_2.USBCtrlTrans(0x23, 0x00, 1); // 设备2 ,chD
                            DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                        }
                    }
                    if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                    {
                        g_3rdOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备3
                        g_3rdOSC_CtrlByte1 |= 0x02; // chB 输入量程设置为： .设备3
                        lock (globleVariables.g_lockIO3)
                        {
                            DLL_3.USBCtrlTrans(0x23, 0x00, 1); // 设备3, ch6
                            DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                        }
                    }
                    if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                    {
                        g_4thOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备4
                        g_4thOSC_CtrlByte1 |= 0x02; // chB 输入量程设置为： .设备4
                        lock (globleVariables.g_lockIO4)
                        {
                            DLL_4.USBCtrlTrans(0x23, 0x00, 1); // 设备4 ,ch8
                            DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                        }
                    }
                    globleVariables.g_CurrentZero1 = globleVariables.g_VbiasZero11v;
                    globleVariables.g_CurrentScale_ch1 = globleVariables.g_VbiasScale_1V_ch1;

                    globleVariables.g_2ndOSC_CurrentZero1 = globleVariables.g_2ndOSC_VbiasZero11v;
                    globleVariables.g_2ndOSC_CurrentScale_ch1 = globleVariables.g_2ndOSC_VbiasScale_1V_ch1;
                    globleVariables.g_3rdOSC_CurrentZero1 = globleVariables.g_3rdOSC_VbiasZero11v;
                    globleVariables.g_3rdOSC_CurrentScale_ch1 = globleVariables.g_3rdOSC_VbiasScale_1V_ch1;
                    globleVariables.g_4thOSC_CurrentZero1 = globleVariables.g_4thOSC_VbiasZero11v;
                    globleVariables.g_4thOSC_CurrentScale_ch1 = globleVariables.g_4thOSC_VbiasScale_1V_ch1;

                    globleVariables.RangeV_B  = 10;
                    globleVariables.RangeV_2B = 10;
                    globleVariables.RangeV_3B = 10;
                    globleVariables.RangeV_4B = 10;
            }
            else if (comboRangeB.SelectedIndex == 2) // chB 输入量程设置为：+-2.5v
            {
                g_CtrlByte1 &= 0xF9;
                g_CtrlByte1 |= 0x04;

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x23, 0x00, 1); // 设备1 chB
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    g_2ndOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备2
                    g_2ndOSC_CtrlByte1 |= 0x04; // chB 输入量程设置为： .设备2
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x23, 0x00, 1); // 设备2 chD
                        DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    g_3rdOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备3
                    g_3rdOSC_CtrlByte1 |= 0x04; // chB 输入量程设置为： .设备3
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x23, 0x00, 1); // 设备3 ch6
                        DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    g_4thOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备4
                    g_4thOSC_CtrlByte1 |= 0x04; // chB 输入量程设置为： .设备4
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x23, 0x00, 1); // 设备4 ch8
                        DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                    }
                }
                globleVariables.g_CurrentZero1 = globleVariables.g_VbiasZero1500mv;
                globleVariables.g_CurrentScale_ch1 = globleVariables.g_VbiasScale_500mV_ch1;

                globleVariables.g_2ndOSC_CurrentZero1 = globleVariables.g_2ndOSC_VbiasZero1500mv;
                globleVariables.g_2ndOSC_CurrentScale_ch1 = globleVariables.g_2ndOSC_VbiasScale_500mV_ch1;
                globleVariables.g_3rdOSC_CurrentZero1 = globleVariables.g_3rdOSC_VbiasZero1500mv;
                globleVariables.g_3rdOSC_CurrentScale_ch1 = globleVariables.g_3rdOSC_VbiasScale_500mV_ch1;
                globleVariables.g_4thOSC_CurrentZero1 = globleVariables.g_4thOSC_VbiasZero1500mv;
                globleVariables.g_4thOSC_CurrentScale_ch1 = globleVariables.g_4thOSC_VbiasScale_500mV_ch1;

                globleVariables.RangeV_B  = 5;
                globleVariables.RangeV_2B = 5;
                globleVariables.RangeV_3B = 5;
                globleVariables.RangeV_4B = 5;
            }
            else if (comboRangeB.SelectedIndex == 3) // chB 输入量程设置为：+-1v
            {
                g_CtrlByte1 &= 0xF9;
                g_CtrlByte1 |= 0x06;

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x23, 0x00, 1); // 设备1 chB
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    g_2ndOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备2
                    g_2ndOSC_CtrlByte1 |= 0x06; // chB 输入量程设置为： .设备2
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x23, 0x00, 1); // 设备2 chD
                        DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    g_3rdOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备3
                    g_3rdOSC_CtrlByte1 |= 0x06; // chB 输入量程设置为： .设备3
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x23, 0x00, 1); // 设备3 ch6
                        DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    g_4thOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备4
                    g_4thOSC_CtrlByte1 |= 0x06; // chB 输入量程设置为： .设备4
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x23, 0x00, 1); // 设备4 ch8
                        DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                    }
                }
                globleVariables.g_CurrentZero1 = globleVariables.g_VbiasZero1200mv;
                globleVariables.g_CurrentScale_ch1 = globleVariables.g_VbiasScale_200mV_ch1;

                globleVariables.g_2ndOSC_CurrentZero1 = globleVariables.g_2ndOSC_VbiasZero1200mv;
                globleVariables.g_2ndOSC_CurrentScale_ch1 = globleVariables.g_2ndOSC_VbiasScale_200mV_ch1;
                globleVariables.g_3rdOSC_CurrentZero1 = globleVariables.g_3rdOSC_VbiasZero1200mv;
                globleVariables.g_3rdOSC_CurrentScale_ch1 = globleVariables.g_3rdOSC_VbiasScale_200mV_ch1;
                globleVariables.g_4thOSC_CurrentZero1 = globleVariables.g_4thOSC_VbiasZero1200mv;
                globleVariables.g_4thOSC_CurrentScale_ch1 = globleVariables.g_4thOSC_VbiasScale_200mV_ch1;

                globleVariables.RangeV_B  = 2;
                globleVariables.RangeV_2B = 2;
                globleVariables.RangeV_3B = 2;
                globleVariables.RangeV_4B = 2;
            }
            else if (comboRangeB.SelectedIndex == 4) //  chB 输入量程设置为：+-500mv
            {
                g_CtrlByte1 &= 0xF9;
                g_CtrlByte1 |= 0x02; // 放大两倍
 

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x23, 0x40, 1); // 设备1 chB
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    g_2ndOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备2
                    g_2ndOSC_CtrlByte1 |= 0x02; // chB 输入量程设置为： .设备2
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x23, 0x40, 1); // 设备2 chD
                        DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    g_3rdOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备3
                    g_3rdOSC_CtrlByte1 |= 0x02; // chB 输入量程设置为： .设备3
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x23, 0x40, 1); // 设备3 ch6
                        DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    g_4thOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备4
                    g_4thOSC_CtrlByte1 |= 0x02; // chB 输入量程设置为： .设备4
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x23, 0x40, 1); // 设备4 ch8
                        DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                    }
                }
                globleVariables.g_CurrentZero1 = globleVariables.g_VbiasZero1100mv;
                globleVariables.g_CurrentScale_ch1 = globleVariables.g_VbiasScale_100mV_ch1;

                globleVariables.g_2ndOSC_CurrentZero1 = globleVariables.g_2ndOSC_VbiasZero1100mv;
                globleVariables.g_2ndOSC_CurrentScale_ch1 = globleVariables.g_2ndOSC_VbiasScale_100mV_ch1;
                globleVariables.g_3rdOSC_CurrentZero1 = globleVariables.g_3rdOSC_VbiasZero1100mv;
                globleVariables.g_3rdOSC_CurrentScale_ch1 = globleVariables.g_3rdOSC_VbiasScale_100mV_ch1;
                globleVariables.g_4thOSC_CurrentZero1 = globleVariables.g_4thOSC_VbiasZero1100mv;
                globleVariables.g_4thOSC_CurrentScale_ch1 = globleVariables.g_4thOSC_VbiasScale_100mV_ch1;

                globleVariables.RangeV_B  = 1;
                globleVariables.RangeV_2B = 1;
                globleVariables.RangeV_3B = 1;
                globleVariables.RangeV_4B = 1;
            }
            else if (comboRangeB.SelectedIndex == 5) //  chB 输入量程设置为：+-250mv
            {
                g_CtrlByte1 &= 0xF9;
                g_CtrlByte1 |= 0x04; // 放大4倍 

                g_2ndOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备2
                g_2ndOSC_CtrlByte1 |= 0x04; // chB 输入量程设置为： .设备2

                g_3rdOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备3
                g_3rdOSC_CtrlByte1 |= 0x04; // chB 输入量程设置为： .设备3

                g_4thOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备4
                g_4thOSC_CtrlByte1 |= 0x04; // chB 输入量程设置为： .设备4

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x23, 0x40, 1); // 设备1 chB
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x23, 0x40, 1); // 设备2 chD
                        DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x23, 0x40, 1); // 设备3 ch6
                        DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x23, 0x40, 1); // 设备4 ch8
                        DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                    }
                }
                globleVariables.g_CurrentZero1 = globleVariables.g_VbiasZero150mv;
                globleVariables.g_CurrentScale_ch1 = globleVariables.g_VbiasScale_50mV_ch1;

                globleVariables.g_2ndOSC_CurrentZero1 = globleVariables.g_2ndOSC_VbiasZero150mv;
                globleVariables.g_2ndOSC_CurrentScale_ch1 = globleVariables.g_2ndOSC_VbiasScale_50mV_ch1;
                globleVariables.g_3rdOSC_CurrentZero1 = globleVariables.g_3rdOSC_VbiasZero150mv;
                globleVariables.g_3rdOSC_CurrentScale_ch1 = globleVariables.g_3rdOSC_VbiasScale_50mV_ch1;
                globleVariables.g_4thOSC_CurrentZero1 = globleVariables.g_4thOSC_VbiasZero150mv;
                globleVariables.g_4thOSC_CurrentScale_ch1 = globleVariables.g_4thOSC_VbiasScale_50mV_ch1;

                globleVariables.RangeV_B  = 0.5;
                globleVariables.RangeV_2B = 0.5;
                globleVariables.RangeV_3B = 0.5;
                globleVariables.RangeV_4B = 0.5;
            }
            else if (comboRangeB.SelectedIndex == 6) //  chB 输入量程设置为：+-100mv
            {
                g_CtrlByte1 &= 0xF9;
                g_CtrlByte1 |= 0x06;

                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x23, 0x40, 1); // 设备1 chB
                    DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    g_2ndOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备2
                    g_2ndOSC_CtrlByte1 |= 0x06; // chB 输入量程设置为： .设备2
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x23, 0x40, 1); // 设备2 chD
                        DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    g_3rdOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备3
                    g_3rdOSC_CtrlByte1 |= 0x06; // chB 输入量程设置为： .设备3
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x23, 0x40, 1); // 设备3 ch6
                        DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    g_4thOSC_CtrlByte1 &= 0xF9; // chB 输入量程设置为： .设备4
                    g_4thOSC_CtrlByte1 |= 0x06; // chB 输入量程设置为： .设备4
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x23, 0x40, 1); // 设备4 ch8
                        DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);
                    }
                }
                globleVariables.g_CurrentZero1 = globleVariables.g_VbiasZero120mv;
                globleVariables.g_CurrentScale_ch1 = globleVariables.g_VbiasScale_20mV_ch1;

                globleVariables.g_2ndOSC_CurrentZero1 = globleVariables.g_2ndOSC_VbiasZero120mv;
                globleVariables.g_2ndOSC_CurrentScale_ch1 = globleVariables.g_2ndOSC_VbiasScale_20mV_ch1;
                globleVariables.g_3rdOSC_CurrentZero1 = globleVariables.g_3rdOSC_VbiasZero120mv;
                globleVariables.g_3rdOSC_CurrentScale_ch1 = globleVariables.g_3rdOSC_VbiasScale_20mV_ch1;
                globleVariables.g_4thOSC_CurrentZero1 = globleVariables.g_4thOSC_VbiasZero120mv;
                globleVariables.g_4thOSC_CurrentScale_ch1 = globleVariables.g_4thOSC_VbiasScale_20mV_ch1;

                globleVariables.RangeV_B  = 0.2;
                globleVariables.RangeV_2B = 0.2;
                globleVariables.RangeV_3B = 0.2;
                globleVariables.RangeV_4B = 0.2;
            }
        }
        public void ActivateRangeCHC()//发送采样率命令实施通道C的电压测试范围设置20250515
        {
            if (comboRangeB.SelectedIndex == 0) // chC 输入量程设置为：-8V ~ +8V
            {
                g_CtrlByte0_CD &= 0xf0;
                g_CtrlByte0_CD |= 0x08;
                ushort a = (ushort)((g_CtrlByte1_CD << 8) + g_CtrlByte0_CD);
                lock (globleVariables.g_lockIO)  {   DLL_1.USBCtrlTrans(0x48, a, 1);     }//20250515 设定C通道的输入量程设置为：-8V ~ +8V 
           
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2) { DLL_2.USBCtrlTrans(0x48, a, 1); }//20250515 设定7通道的输入量程设置为：-8V ~ +8V 
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3) { DLL_3.USBCtrlTrans(0x48, a, 1); }//20250515 设定11通道的输入量程设置为：-8V ~ +8V 
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4) { DLL_4.USBCtrlTrans(0x48, a, 1); }//20250515 设定15通道的输入量程设置为：-8V ~ +8V 
                }
                globleVariables.RangeV_C = 20;      //第1个设备的通道C电压量程范围
                globleVariables.g_CurrentZeroC = globleVariables.g_VbiasZeroC2v;
                globleVariables.g_CurrentScale_chC = globleVariables.g_VbiasScale_2V_chC;
            }
            if (comboRangeB.SelectedIndex == 1) // chC输入量程设置为：-5V ~ +5V
            {
                g_CtrlByte0_CD &= 0xf0; //20240820
                g_CtrlByte0_CD |= 0x09;
                ushort a = (ushort)((g_CtrlByte1_CD << 8) + g_CtrlByte0_CD);
                lock (globleVariables.g_lockIO)  { DLL_1.USBCtrlTrans(0x48, a, 1);   }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2) { DLL_2.USBCtrlTrans(0x48, a, 1); }//20250515 设定7通道的输入量程设置为：-5V ~ +5V 
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3) { DLL_3.USBCtrlTrans(0x48, a, 1); }//20250515 设定11通道的输入量程设置为：-5V ~ +5V 
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4) { DLL_4.USBCtrlTrans(0x48, a, 1); }//20250515 设定15通道的输入量程设置为：-5V ~ +5V 
                }
                globleVariables.RangeV_C = 10;      //第1个设备的通道C电压量程范围
                globleVariables.g_CurrentZeroC = globleVariables.g_VbiasZeroC1v;
                globleVariables.g_CurrentScale_chC = globleVariables.g_VbiasScale_1V_chC;
            }
            else if (comboRangeB.SelectedIndex == 2) // chC 输入量程设置为：+-2.5v
            {
                g_CtrlByte0_CD &= 0xf0; //20240820
                g_CtrlByte0_CD |= 0x0A;
                ushort a = (ushort)((g_CtrlByte1_CD << 8) + g_CtrlByte0_CD);

                lock (globleVariables.g_lockIO)  {   DLL_1.USBCtrlTrans(0x48, a, 1);  }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2) { DLL_2.USBCtrlTrans(0x48, a, 1); }//20250515 设定7通道的输入量程设置为：-2.5V ~ +2.5V 
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3) { DLL_3.USBCtrlTrans(0x48, a, 1); }//20250515 设定11通道的输入量程设置为：-2.5V ~ +2.5V 
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4) { DLL_4.USBCtrlTrans(0x48, a, 1); }//20250515 设定15通道的输入量程设置为：-2.5V ~ +2.5V 
                }
                globleVariables.RangeV_C = 5;      //第1个设备的通道C电压量程范围
                globleVariables.g_CurrentZeroC = globleVariables.g_VbiasZeroC500mv;
                globleVariables.g_CurrentScale_chC = globleVariables.g_VbiasScale_500mV_chC;
            }
            else if (comboRangeB.SelectedIndex == 3) // chB 输入量程设置为：+-1v
            {
                g_CtrlByte0_CD &= 0xf0;
                g_CtrlByte0_CD |= 0x0B;
                ushort a = (ushort)((g_CtrlByte1_CD << 8) + g_CtrlByte0_CD);

                lock (globleVariables.g_lockIO)   {   DLL_1.USBCtrlTrans(0x48, a, 1); }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2) { DLL_2.USBCtrlTrans(0x48, a, 1); }//20250515 设定7通道的输入量程设置为：-1 ~ +1V 
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3) { DLL_3.USBCtrlTrans(0x48, a, 1); }//20250515 设定11通道的输入量程设置为：-1 ~ +1V 
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4) { DLL_4.USBCtrlTrans(0x48, a, 1); }//20250515 设定15通道的输入量程设置为：-1 ~ +1V 
                } 
                globleVariables.RangeV_C = 2;      //第1个设备的通道C电压量程范围
                globleVariables.g_CurrentZeroC = globleVariables.g_VbiasZeroC200mv;
                globleVariables.g_CurrentScale_chC = globleVariables.g_VbiasScale_200mV_chC;
            }
            else if (comboRangeB.SelectedIndex == 4) //  chB 输入量程设置为：+-500mv
            {
                g_CtrlByte0_CD &= 0xf0;
                g_CtrlByte0_CD |= 0x01;
                ushort a = (ushort)((g_CtrlByte1_CD << 8) + g_CtrlByte0_CD);

                lock (globleVariables.g_lockIO)   {  DLL_1.USBCtrlTrans(0x48, a, 1);  }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2) { DLL_2.USBCtrlTrans(0x48, a, 1); }//20250515 设定7通道的输入量程设置为：-0.5~ +0.5V 
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3) { DLL_3.USBCtrlTrans(0x48, a, 1); }//20250515 设定11通道的输入量程设置为：-0.5~ +0.5V 
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4) { DLL_4.USBCtrlTrans(0x48, a, 1); }//20250515 设定15通道的输入量程设置为：-0.5~ +0.5V 
                }
                globleVariables.RangeV_C = 1;      //第1个设备的通道C电压量程范围
                globleVariables.g_CurrentZeroC = globleVariables.g_VbiasZeroC100mv;
                globleVariables.g_CurrentScale_chC = globleVariables.g_VbiasScale_100mV_chC;
            }
            else if (comboRangeB.SelectedIndex == 5) //  chB 输入量程设置为：+-250mv
            {
                g_CtrlByte0_CD &= 0xf0;
                g_CtrlByte0_CD |= 0x02;
                ushort a = (ushort)((g_CtrlByte1_CD << 8) + g_CtrlByte0_CD);
                lock (globleVariables.g_lockIO) { DLL_1.USBCtrlTrans(0x48, a, 1);  }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2) { DLL_2.USBCtrlTrans(0x48, a, 1); }//20250515 设定7通道的输入量程设置为：-0.25~ +0.25V 
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3) { DLL_3.USBCtrlTrans(0x48, a, 1); }//20250515 设定11通道的输入量程设置为：-0.25~ +0.25V 
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4) { DLL_4.USBCtrlTrans(0x48, a, 1); }//20250515 设定15通道的输入量程设置为：-0.25~ +0.25V 
                }
                globleVariables.RangeV_C = 0.5;      //第1个设备的通道C电压量程范围
                globleVariables.g_CurrentZeroC = globleVariables.g_VbiasZeroC50mv;
                globleVariables.g_CurrentScale_chC = globleVariables.g_VbiasScale_50mV_chC;
            }
            else if (comboRangeB.SelectedIndex == 6) //  chB 输入量程设置为：+-100mv
            {
                g_CtrlByte0_CD &= 0xf0;
                g_CtrlByte0_CD |= 0x03;
                ushort a = (ushort)((g_CtrlByte1_CD << 8) + g_CtrlByte0_CD);
                lock (globleVariables.g_lockIO)   {  DLL_1.USBCtrlTrans(0x48, a, 1);   }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2) { DLL_2.USBCtrlTrans(0x48, a, 1); }//20250515 设定7通道的输入量程设置为：-0.1~ +0.1V 
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3) { DLL_3.USBCtrlTrans(0x48, a, 1); }//20250515 设定11通道的输入量程设置为：-0.1~ +0.1V 
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4) { DLL_4.USBCtrlTrans(0x48, a, 1); }//20250515 设定15通道的输入量程设置为：-0.1~ +0.1V 
                }
                globleVariables.RangeV_C = 0.2;      //第1个设备的通道C电压量程范围
                globleVariables.g_CurrentZeroC = globleVariables.g_VbiasZeroC20mv;
                globleVariables.g_CurrentScale_chC = globleVariables.g_VbiasScale_20mV_chC;
            }
        }

        public void ActivateRangeCHD()//发送采样率命令实施通道D的电压测试范围设置20250515
        {
            if (comboRangeB.SelectedIndex == 0) // chD 输入量程设置为：-8V ~ +8V
            {
                g_CtrlByte0_CD &= 0x1F;
                g_CtrlByte0_CD |= 0x00;
                g_CtrlByte1_CD &= 0xFE;
                g_CtrlByte1_CD |= 0x01;
                ushort a = (ushort)((g_CtrlByte1_CD << 8) + g_CtrlByte0_CD);

                lock (globleVariables.g_lockIO)  {  DLL_1.USBCtrlTrans(0x48, a, 1); }//20250515 设定D通道的输入量程设置为：-8V ~ +8V 
               
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2) { DLL_2.USBCtrlTrans(0x48, a, 1); }//20250515 设定8通道的输入量程设置为：-8V ~ +8V 
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3) { DLL_3.USBCtrlTrans(0x48, a, 1); }//20250515 设定12通道的输入量程设置为：-8V ~ +8V 
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4) { DLL_4.USBCtrlTrans(0x48, a, 1); }//20250515 设定16通道的输入量程设置为：-8V ~ +8V 
                }
                globleVariables.RangeV_D = 20;      //第1个设备的通道C电压量程范围
                globleVariables.g_CurrentZeroD = globleVariables.g_VbiasZeroD2v;
                globleVariables.g_CurrentScale_chD = globleVariables.g_VbiasScale_2V_chD;
            }
            if (comboRangeB.SelectedIndex == 1) // chB 输入量程设置为：-5V ~ +5V
            {
                g_CtrlByte0_CD &= 0x1F;
                g_CtrlByte0_CD |= 0x20;
                g_CtrlByte1_CD &= 0xFE;
                g_CtrlByte1_CD |= 0x01;
                ushort a = (ushort)((g_CtrlByte1_CD << 8) + g_CtrlByte0_CD);

                lock (globleVariables.g_lockIO) { DLL_1.USBCtrlTrans(0x48, a, 1); }//20250515 设定D通道的输入量程设置为：-5V ~ +5V 

                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2) { DLL_2.USBCtrlTrans(0x48, a, 1); }//20250515 设定8通道的输入量程设置为：-5V ~ +5V 
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3) { DLL_3.USBCtrlTrans(0x48, a, 1); }//20250515 设定12通道的输入量程设置为：-5V ~ +5V 
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4) { DLL_4.USBCtrlTrans(0x48, a, 1); }//20250515 设定16通道的输入量程设置为：-5V ~ +5V 
                }
                    globleVariables.RangeV_D = 10;      //第1个设备的通道C电压量程范围
                    globleVariables.g_CurrentZeroD = globleVariables.g_VbiasZeroD1v;
                    globleVariables.g_CurrentScale_chD = globleVariables.g_VbiasScale_1V_chD;
               }
            else if (comboRangeB.SelectedIndex == 2) // chB 输入量程设置为：+-2.5v
            {
                g_CtrlByte0_CD &= 0x1F;
                g_CtrlByte0_CD |= 0x40;
                g_CtrlByte1_CD &= 0xFE;
                g_CtrlByte1_CD |= 0x01;
                ushort a = (ushort)((g_CtrlByte1_CD << 8) + g_CtrlByte0_CD);

                lock (globleVariables.g_lockIO) { DLL_1.USBCtrlTrans(0x48, a, 1); }//20250515 设定D通道的输入量程设置为：-2.5V ~ +2.5V 

                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2) { DLL_2.USBCtrlTrans(0x48, a, 1); }//20250515 设定8通道的输入量程设置为：-2.5V ~ +2.5V 
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3) { DLL_3.USBCtrlTrans(0x48, a, 1); }//20250515 设定12通道的输入量程设置为：-2.5V ~ +2.5V 
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4) { DLL_4.USBCtrlTrans(0x48, a, 1); }//20250515 设定16通道的输入量程设置为：-2.5V ~ +2.5V 
                }
                globleVariables.RangeV_D = 5;      //第1个设备的通道C电压量程范围
                globleVariables.g_CurrentZeroD = globleVariables.g_VbiasZeroD500mv;
                globleVariables.g_CurrentScale_chD = globleVariables.g_VbiasScale_500mV_chD;
            }
            else if (comboRangeB.SelectedIndex == 3) // chB 输入量程设置为：+-1v
            {
                g_CtrlByte0_CD &= 0x1F;
                g_CtrlByte0_CD |= 0x60;
                g_CtrlByte1_CD &= 0xFE;
                g_CtrlByte1_CD |= 0x01;
                ushort a = (ushort)((g_CtrlByte1_CD << 8) + g_CtrlByte0_CD);

                lock (globleVariables.g_lockIO) { DLL_1.USBCtrlTrans(0x48, a, 1); }//20250515 设定D通道的输入量程设置为：-1V ~ +1V 

                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2) { DLL_2.USBCtrlTrans(0x48, a, 1); }//20250515 设定8通道的输入量程设置为：-1V ~ +1V 
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3) { DLL_3.USBCtrlTrans(0x48, a, 1); }//20250515 设定12通道的输入量程设置为：-1V ~ +1V 
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4) { DLL_4.USBCtrlTrans(0x48, a, 1); }//20250515 设定16通道的输入量程设置为：-1V ~ +1V 
                }
                globleVariables.RangeV_D = 2;      //第1个设备的通道C电压量程范围
                globleVariables.g_CurrentZeroD = globleVariables.g_VbiasZeroD200mv;
                globleVariables.g_CurrentScale_chD = globleVariables.g_VbiasScale_200mV_chD;
            }
            else if (comboRangeB.SelectedIndex == 4) //  chB 输入量程设置为：+-500mv
            {
                g_CtrlByte0_CD &= 0x1F;
                g_CtrlByte0_CD |= 0x20;
                g_CtrlByte1_CD &= 0xFE;
                g_CtrlByte1_CD |= 0x00;
                ushort a = (ushort)((g_CtrlByte1_CD << 8) + g_CtrlByte0_CD);

                lock (globleVariables.g_lockIO) { DLL_1.USBCtrlTrans(0x48, a, 1); }//20250515 设定D通道的输入量程设置为：-0.5V ~ +0.5V 

                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2) { DLL_2.USBCtrlTrans(0x48, a, 1); }//20250515 设定8通道的输入量程设置为：-0.5V ~ +0.5V 
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3) { DLL_3.USBCtrlTrans(0x48, a, 1); }//20250515 设定12通道的输入量程设置为：-0.5V ~ +0.5V 
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4) { DLL_4.USBCtrlTrans(0x48, a, 1); }//20250515 设定16通道的输入量程设置为：--0.5V ~ +0.5V 
                }
                globleVariables.RangeV_D = 1;      //第1个设备的通道C电压量程范围
                globleVariables.g_CurrentZeroD = globleVariables.g_VbiasZeroD100mv;
                globleVariables.g_CurrentScale_chD = globleVariables.g_VbiasScale_100mV_chD;
            }
            else if (comboRangeB.SelectedIndex == 5) //  chB 输入量程设置为：+-250mv
            {
                g_CtrlByte0_CD &= 0x1F;
                g_CtrlByte0_CD |= 0x40;
                g_CtrlByte1_CD &= 0xFE;
                g_CtrlByte1_CD |= 0x00;
                ushort a = (ushort)((g_CtrlByte1_CD << 8) + g_CtrlByte0_CD);

                lock (globleVariables.g_lockIO) { DLL_1.USBCtrlTrans(0x48, a, 1); }//20250515 设定D通道的输入量程设置为：-0.25V ~ +0.25V 

                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2) { DLL_2.USBCtrlTrans(0x48, a, 1); }//20250515 设定8通道的输入量程设置为：-0.25V ~ +0.25V 
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3) { DLL_3.USBCtrlTrans(0x48, a, 1); }//20250515 设定12通道的输入量程设置为：-0.25V ~ +0.25V 
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4) { DLL_4.USBCtrlTrans(0x48, a, 1); }//20250515 设定16通道的输入量程设置为：--0.25V ~ +0.25V 
                }
                globleVariables.RangeV_D = 0.5;      //第1个设备的通道C电压量程范围
                globleVariables.g_CurrentZeroD = globleVariables.g_VbiasZeroD50mv;
                globleVariables.g_CurrentScale_chD = globleVariables.g_VbiasScale_50mV_chD;
            }
            else if (comboRangeB.SelectedIndex == 6) //  chB 输入量程设置为：+-100mv
            {
                g_CtrlByte0_CD &= 0x1F;
                g_CtrlByte0_CD |= 0x60;
                g_CtrlByte1_CD &= 0xFE;
                g_CtrlByte1_CD |= 0x00;
                ushort a = (ushort)((g_CtrlByte1_CD << 8) + g_CtrlByte0_CD);

                lock (globleVariables.g_lockIO) { DLL_1.USBCtrlTrans(0x48, a, 1); }//20250515 设定D通道的输入量程设置为：-0.1V ~ +0.1V 

                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2) { DLL_2.USBCtrlTrans(0x48, a, 1); }//20250515 设定8通道的输入量程设置为：-0.1V ~ +0.1V 
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3) { DLL_3.USBCtrlTrans(0x48, a, 1); }//20250515 设定12通道的输入量程设置为：-0.1V ~ +0.1V 
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4) { DLL_4.USBCtrlTrans(0x48, a, 1); }//20250515 设定16通道的输入量程设置为：--0.1V ~ +0.1V 
                }
                globleVariables.RangeV_D = 0.2;      //第1个设备的通道C电压量程范围
                globleVariables.g_CurrentZeroD = globleVariables.g_VbiasZeroD20mv;
                globleVariables.g_CurrentScale_chD = globleVariables.g_VbiasScale_20mV_chD;
            }
        }
        
        private void butRead_Click(object sender, EventArgs e)
        {
            DLL_1.ResetPipe(); //20230726 jiangtao.lv
            DLL_1.ResetPipe(); //20230726 jiangtao.lv
            DLL_1.ResetPipe(); //20230726 jiangtao.lv
            DLL_1.USBCtrlTransSimple((Int32)0x33); // 设备1 开始AD采集

            if (globleVariables.g_OSCcnt >= 2)  //级联了设备2
            {

                DLL_2.ResetPipe(); //20230726 jiangtao.lv
                DLL_2.ResetPipe(); //20230726 jiangtao.lv
                DLL_2.ResetPipe(); //20230726 jiangtao.lv   
            }
            if (globleVariables.g_OSCcnt >= 3)  //级联了设备3
            {
                DLL_3.ResetPipe(); //20230726 jiangtao.lv
                DLL_3.ResetPipe(); //20230726 jiangtao.lv
                DLL_3.ResetPipe(); //20230726 jiangtao.lv 
            }

            if (globleVariables.g_OSCcnt >= 4)  //级联了设备4
            {
                DLL_4.ResetPipe(); //20230726 jiangtao.lv
                DLL_4.ResetPipe(); //20230726 jiangtao.lv
                DLL_4.ResetPipe(); //20230726 jiangtao.lv 
            }
   
            But_Read.Enabled = true;
            Thread.Sleep(1000);
        }



        

        private void But_Read_Click(object sender, EventArgs e)
        {

            int ii = DLL_1.USBCtrlTransSimple((Int32)0x50); // 设备1  查询是否AD采集和存储完，如果采集存储结束，返回值是33       
            
            if(33 != ii)   
            {
                MessageBox.Show(this, "Haven't ready (1)!");
                return;            
            }

            // 第二个设备
           if (globleVariables.g_OSCcnt >= 2)
            {
                ii = DLL_2.USBCtrlTransSimple((Int32)0x50); // 查询是否AD采集和存储完，如果采集存储结束，返回值是33       
                if (33 != ii)
                {
                    MessageBox.Show(this, "Haven't ready (2)!");
                    return; 
                }
            }

            // 第三个设备
            if (globleVariables.g_OSCcnt >= 3)
            {
                ii = DLL_3.USBCtrlTransSimple((Int32)0x50); // 查询是否AD采集和存储完，如果采集存储结束，返回值是33       
                if (33 != ii)
                {
                    MessageBox.Show(this, "Haven't ready (3)!");
                    return;
                }
            }

            // 第四个设备
            if (globleVariables.g_OSCcnt >= 4)
            {
                ii = DLL_4.USBCtrlTransSimple((Int32)0x50); // 查询是否AD采集和存储完，如果采集存储结束，返回值是33       
                if (33 != ii)
                {
                    MessageBox.Show(this, "Haven't ready (4)!");
                    return;
                }
            }

            int res = DLL_1.AiReadBulkData(64 * 1024 * 2, 1, 2000, globleVariables.g_pBuffer, 0, 0); // 开始获取数据，读取128K的原始数据
            if (0 != res) // 读取失败
            {
                MessageBox.Show(this, "AiReadBulkData Failed (1)!");
                return;
            }

            // 第二个设备
            if (globleVariables.g_OSCcnt >= 2)
            {
                res = DLL_2.AiReadBulkData(64 * 1024 * 2, 1, 2000, globleVariables.g_pBuffer_2nd, 0, 0); // 开始获取数据，读取128K的原始数据
                if (0 != res) // 读取失败
                {
                    MessageBox.Show(this, "AiReadBulkData Failed (2)!");
                    return;
                }
            }
            // 第三个设备
            if (globleVariables.g_OSCcnt >= 3)
            {
                res = DLL_3.AiReadBulkData(64 * 1024 * 2, 1, 2000, globleVariables.g_pBuffer_3rd, 0, 0); // 开始获取数据，读取128K的原始数据
                if (0 != res) // 读取失败
                {
                    MessageBox.Show(this, "AiReadBulkData Failed (3)!");
                    return;
                }
            }
            // 第四个设备
            if (globleVariables.g_OSCcnt >= 4)
            {
                res = DLL_4.AiReadBulkData(64 * 1024 * 2, 1, 2000, globleVariables.g_pBuffer_4th, 0, 0); // 开始获取数据，读取128K的原始数据
                if (0 != res) // 读取失败
                {
                    MessageBox.Show(this, "AiReadBulkData Failed (4)!");
                    return;
                }
            }

            Thread.Sleep(100);
            butDispaly.Enabled = true;
        }

        private void butDisplay_Click(object sender, EventArgs e)
        {
            
        }

        

        private void pictureBox_chA_Paint(object sender, PaintEventArgs e) // 绘制A通道的波形曲线
        {
            Graphics g = e.Graphics; 
            Pen PanelPen = new Pen(Color.Red, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_chA.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_chA.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值            
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置            


            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_chADataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_chADataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }


            if (isSaving) // 由 but_saveData 控制的标志位
            {
                if (isSaving)
                {
                    // A 通道
                    byte[] copyA = new byte[globleVariables.DataCount];
                    Array.Copy(globleVariables.g_chADataArray, copyA, globleVariables.DataCount);
                    _ = CsvHelper.SaveBytesToCsvByPointerAsync(copyA, "chA", saveFolderPath);

                    // B 通道
                    byte[] copyB = new byte[globleVariables.DataCount];
                    Array.Copy(globleVariables.g_chBDataArray, copyB, globleVariables.DataCount);
                    _ = CsvHelper.SaveBytesToCsvByPointerAsync(copyB, "chB", saveFolderPath);

                    // C 通道
                    byte[] copyC = new byte[globleVariables.DataCount];
                    Array.Copy(globleVariables.g_chCDataArray, copyC, globleVariables.DataCount);
                    _ = CsvHelper.SaveBytesToCsvByPointerAsync(copyC, "chC", saveFolderPath);

                    // D 通道
                    byte[] copyD = new byte[globleVariables.DataCount];
                    Array.Copy(globleVariables.g_chDDataArray, copyD, globleVariables.DataCount);
                    _ = CsvHelper.SaveBytesToCsvByPointerAsync(copyD, "chD", saveFolderPath);
                }
            }

        }

        private void pictureBox1_chB_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.Blue, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_chB.Width;   // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_chB.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                       // 原始数据的最小值
            int DataMax = 255;                     // 原始数据的最大值
            int startXpos = 0;                     // 绘图区x轴的像素起始位置
            int startYpos = 0;                     // 绘图区y轴的像素起始位置  

            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素


            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_chBDataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_chBDataArray[i + 1] - DataMin) * step_y; // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }

            if (isSaving) // 由 but_saveData 控制的标志位
            {
                byte[] copy = new byte[globleVariables.DataCount];
                Array.Copy(globleVariables.g_chBDataArray, copy, globleVariables.DataCount);

                // 异步保存一帧数据到 chA.csv
                _ = CsvHelper.SaveBytesToCsvByPointerAsync(copy, "chB", saveFolderPath);
            }

        }

        private void pictureBox_chC_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.Red, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_chC.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_chC.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置  
            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_chCDataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_chCDataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }
        }

        private void pictureBox_chD_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.Blue, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_chD.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_chD.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置 
            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_chDDataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_chDDataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }
        }

        private void pictureBox_ch5_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.Red, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_ch5.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_ch5.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置 

            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_ch5DataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_ch5DataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }

            if (isSaving)
            {
                // 通道 5
                byte[] copy5 = new byte[globleVariables.DataCount];
                Array.Copy(globleVariables.g_ch5DataArray, copy5, globleVariables.DataCount);
                _ = CsvHelper.SaveBytesToCsvByPointerAsync(copy5, "ch5", saveFolderPath);

                // 通道 6
                byte[] copy6 = new byte[globleVariables.DataCount];
                Array.Copy(globleVariables.g_ch6DataArray, copy6, globleVariables.DataCount);
                _ = CsvHelper.SaveBytesToCsvByPointerAsync(copy6, "ch6", saveFolderPath);

                // 通道 7
                byte[] copy7 = new byte[globleVariables.DataCount];
                Array.Copy(globleVariables.g_ch7DataArray, copy7, globleVariables.DataCount);
                _ = CsvHelper.SaveBytesToCsvByPointerAsync(copy7, "ch7", saveFolderPath);

                // 通道 8
                byte[] copy8 = new byte[globleVariables.DataCount];
                Array.Copy(globleVariables.g_ch8DataArray, copy8, globleVariables.DataCount);
                _ = CsvHelper.SaveBytesToCsvByPointerAsync(copy8, "ch8", saveFolderPath);
            }

        }

        private void pictureBox_ch6_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.Blue, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_ch6.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_ch6.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置  

            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_ch6DataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_ch6DataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }
        }

        private void pictureBox_ch7_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.Red, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_ch7.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_ch7.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置 
            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_ch7DataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_ch7DataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }
        }

        private void pictureBox_ch8_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.Blue, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_ch8.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_ch8.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置   
            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_ch8DataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_ch8DataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }
        }

        private void butGetCalibration_Click(object sender, EventArgs e)////20230726 jiangtao.lv获取示波器的校准数据用来校准采集到的原始字节和真实电压之间的映射关系
        {
            lock (globleVariables.g_lockIO)
            {
                globleVariables.g_VbiasZero02v = (DLL_1.USBCtrlTrans(0x90, 0x82, 1));
                globleVariables.g_VbiasZero12v = (DLL_1.USBCtrlTrans(0x90, 0x72, 1));
                globleVariables.g_VbiasZero01v = (DLL_1.USBCtrlTrans(0x90, 0x01, 1));
                globleVariables.g_VbiasZero11v = (DLL_1.USBCtrlTrans(0x90, 0x02, 1));
                globleVariables.g_VbiasZero0500mv = DLL_1.USBCtrlTrans(0x90, 0x0E, 1);
                globleVariables.g_VbiasZero1500mv = DLL_1.USBCtrlTrans(0x90, 0x0F, 1);
                globleVariables.g_VbiasZero0200mv = DLL_1.USBCtrlTrans(0x90, 0x14, 1);
                globleVariables.g_VbiasZero1200mv = DLL_1.USBCtrlTrans(0x90, 0x15, 1);
                globleVariables.g_VbiasZero0100mv = DLL_1.USBCtrlTrans(0x90, 0x12, 1);     
                globleVariables.g_VbiasZero1100mv = DLL_1.USBCtrlTrans(0x90, 0x13, 1);
                globleVariables.g_VbiasZero050mv = DLL_1.USBCtrlTrans(0x90, 0x10, 1);
                globleVariables.g_VbiasZero150mv = DLL_1.USBCtrlTrans(0x90, 0x11, 1);
                globleVariables.g_VbiasZero120mv = DLL_1.USBCtrlTrans(0x90, 0xA1, 1);
                globleVariables.g_VbiasZero020mv = DLL_1.USBCtrlTrans(0x90, 0xA0, 1);
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                globleVariables.g_VbiasScale_2V_ch0 = (float)(DLL_1.USBCtrlTrans(0x90, 0xC2, 1)) * (float)2.0 / 255; // 20201110 2V新增
                globleVariables.g_VbiasScale_1V_ch0 = (float)(DLL_1.USBCtrlTrans(0x90, 0x03, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_200mV_ch0 = (float)(DLL_1.USBCtrlTrans(0x90, 0x06, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_2V_ch1 = (float)(DLL_1.USBCtrlTrans(0x90, 0xD2, 1)) * (float)2.0 / 255; // 20201110 2V新增
                globleVariables.g_VbiasScale_1V_ch1 = (float)(DLL_1.USBCtrlTrans(0x90, 0x04, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_200mV_ch1 = (float)(DLL_1.USBCtrlTrans(0x90, 0x07, 1)) * (float)2.0 / 255;
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                globleVariables.g_VbiasScale_500mV_ch0 = (float)(DLL_1.USBCtrlTrans(0x90, 0x08, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_100mV_ch0 = (float)(DLL_1.USBCtrlTrans(0x90, 0x09, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_50mV_ch0 = (float)(DLL_1.USBCtrlTrans(0x90, 0x0A, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_20mV_ch0 = (float)(DLL_1.USBCtrlTrans(0x90, 0x2A, 1)) * (float)2.0 / 255;

                globleVariables.g_VbiasScale_500mV_ch1 = (float)(DLL_1.USBCtrlTrans(0x90, 0x0B, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_100mV_ch1 = (float)(DLL_1.USBCtrlTrans(0x90, 0x0C, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_50mV_ch1 = (float)(DLL_1.USBCtrlTrans(0x90, 0x0D, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_20mV_ch1 = (float)(DLL_1.USBCtrlTrans(0x90, 0x2D, 1)) * (float)2.0 / 255;
            }
            lock (globleVariables.g_lockIO) //20250515 CD通道的零电压和放大倍数校准数据
            {
                globleVariables.g_VbiasScale_5V_chC = (float)(DLL_1.USBCtrlTrans(0x91, 0xC5, 1)) * (float)2.0 / 255; // 20240508
                globleVariables.g_VbiasScale_5V_chD = (float)(DLL_1.USBCtrlTrans(0x91, 0xD5, 1)) * (float)2.0 / 255; // 20240508
                globleVariables.g_VbiasZeroC5v = (DLL_1.USBCtrlTrans(0x91, 0x85, 1)); // 20240508
                globleVariables.g_VbiasZeroD5v = (DLL_1.USBCtrlTrans(0x91, 0x75, 1)); // 20240508
                globleVariables.g_VbiasScale_2V_chC = (float)(DLL_1.USBCtrlTrans(0x91, 0xC2, 1)) * (float)2.0 / 255; // 20201110 2V新增
                globleVariables.g_VbiasScale_1V_chC = (float)(DLL_1.USBCtrlTrans(0x91, 0x03, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_200mV_chC = (float)(DLL_1.USBCtrlTrans(0x91, 0x06, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_2V_chD = (float)(DLL_1.USBCtrlTrans(0x91, 0xD2, 1)) * (float)2.0 / 255; // 20201110 2V新增
                globleVariables.g_VbiasScale_1V_chD = (float)(DLL_1.USBCtrlTrans(0x91, 0x04, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_200mV_chD = (float)(DLL_1.USBCtrlTrans(0x91, 0x07, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_500mV_chC = (float)(DLL_1.USBCtrlTrans(0x91, 0x08, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_100mV_chC = (float)(DLL_1.USBCtrlTrans(0x91, 0x09, 1)  ) * (float)2.0 / 255;// (float)(DLL_1.USBCtrlTrans(0x90, 0x09, 1) ) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_50mV_chC = (float)(DLL_1.USBCtrlTrans(0x91, 0x0A, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_20mV_chC = (float)(DLL_1.USBCtrlTrans(0x91, 0x2A, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_500mV_chD = (float)(DLL_1.USBCtrlTrans(0x91, 0x0B, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_100mV_chD = (float)(DLL_1.USBCtrlTrans(0x91, 0x0C, 1)  ) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_50mV_chD = (float)(DLL_1.USBCtrlTrans(0x91, 0x0D, 1)) * (float)2.0 / 255;
                globleVariables.g_VbiasScale_20mV_chD = (float)(DLL_1.USBCtrlTrans(0x91, 0x2D, 1)) * (float)2.0 / 255;

                globleVariables.g_VbiasZeroC2v = (DLL_1.USBCtrlTrans(0x91, 0x82, 1));
                globleVariables.g_VbiasZeroD2v = (DLL_1.USBCtrlTrans(0x91, 0x72, 1));
                globleVariables.g_VbiasZeroC1v = (DLL_1.USBCtrlTrans(0x91, 0x01, 1));
                globleVariables.g_VbiasZeroD1v = (DLL_1.USBCtrlTrans(0x91, 0x02, 1));
                globleVariables.g_VbiasZeroC500mv = DLL_1.USBCtrlTrans(0x91, 0x0E, 1);
                globleVariables.g_VbiasZeroD500mv = DLL_1.USBCtrlTrans(0x91, 0x0F, 1);
                globleVariables.g_VbiasZeroC50mv = DLL_1.USBCtrlTrans(0x91, 0x10, 1);
                globleVariables.g_VbiasZeroC20mv = DLL_1.USBCtrlTrans(0x91, 0xA0, 1);
                globleVariables.g_VbiasZeroD50mv = DLL_1.USBCtrlTrans(0x91, 0x11, 1);
                globleVariables.g_VbiasZeroD20mv = DLL_1.USBCtrlTrans(0x91, 0xA1, 1);
                globleVariables.g_VbiasZeroC100mv = DLL_1.USBCtrlTrans(0x91, 0x12, 1);
                globleVariables.g_VbiasZeroC200mv = DLL_1.USBCtrlTrans(0x91, 0x14, 1);
                globleVariables.g_VbiasZeroD100mv = DLL_1.USBCtrlTrans(0x91, 0x13, 1);
                globleVariables.g_VbiasZeroD200mv = DLL_1.USBCtrlTrans(0x91, 0x15, 1); // 20190311 新增 nk
            }
            // 打开第二个设备
            if (globleVariables.g_OSCcnt >= 2)
            {
                lock (globleVariables.g_lockIO2)
                {
                    globleVariables.g_2ndOSC_VbiasZero02v = (DLL_2.USBCtrlTrans(0x90, 0x82, 1));
                    globleVariables.g_2ndOSC_VbiasZero12v = (DLL_2.USBCtrlTrans(0x90, 0x72, 1));
                    globleVariables.g_2ndOSC_VbiasZero01v = (DLL_2.USBCtrlTrans(0x90, 0x01, 1));
                    globleVariables.g_2ndOSC_VbiasZero11v = (DLL_2.USBCtrlTrans(0x90, 0x02, 1));
                    globleVariables.g_2ndOSC_VbiasZero0500mv = DLL_2.USBCtrlTrans(0x90, 0x0E, 1);
                    globleVariables.g_2ndOSC_VbiasZero1500mv = DLL_2.USBCtrlTrans(0x90, 0x0F, 1);
                    globleVariables.g_2ndOSC_VbiasZero0200mv = DLL_2.USBCtrlTrans(0x90, 0x14, 1);
                    globleVariables.g_2ndOSC_VbiasZero1200mv = DLL_2.USBCtrlTrans(0x90, 0x15, 1);
                    globleVariables.g_2ndOSC_VbiasZero0100mv = DLL_2.USBCtrlTrans(0x90, 0x12, 1);
                    globleVariables.g_2ndOSC_VbiasZero1100mv = DLL_2.USBCtrlTrans(0x90, 0x13, 1);
                    globleVariables.g_2ndOSC_VbiasZero050mv = DLL_2.USBCtrlTrans(0x90, 0x10, 1);
                    globleVariables.g_2ndOSC_VbiasZero150mv = DLL_2.USBCtrlTrans(0x90, 0x11, 1);
                    globleVariables.g_2ndOSC_VbiasZero120mv = DLL_2.USBCtrlTrans(0x90, 0xA1, 1);
                    globleVariables.g_2ndOSC_VbiasZero020mv = DLL_2.USBCtrlTrans(0x90, 0xA0, 1);
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    globleVariables.g_2ndOSC_VbiasScale_2V_ch0 = (float)(DLL_2.USBCtrlTrans(0x90, 0xC2, 1)) * (float)2.0 / 255; // 20201110 2V新增
                    globleVariables.g_2ndOSC_VbiasScale_1V_ch0 = (float)(DLL_2.USBCtrlTrans(0x90, 0x03, 1)) * (float)2.0 / 255;
                    globleVariables.g_2ndOSC_VbiasScale_200mV_ch0 = (float)(DLL_2.USBCtrlTrans(0x90, 0x06, 1)) * (float)2.0 / 255;
                    globleVariables.g_2ndOSC_VbiasScale_2V_ch1 = (float)(DLL_2.USBCtrlTrans(0x90, 0xD2, 1)) * (float)2.0 / 255; // 20201110 2V新增
                    globleVariables.g_2ndOSC_VbiasScale_1V_ch1 = (float)(DLL_2.USBCtrlTrans(0x90, 0x04, 1)) * (float)2.0 / 255;
                    globleVariables.g_2ndOSC_VbiasScale_200mV_ch1 = (float)(DLL_2.USBCtrlTrans(0x90, 0x07, 1)) * (float)2.0 / 255;
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    globleVariables.g_2ndOSC_VbiasScale_500mV_ch0 = (float)(DLL_2.USBCtrlTrans(0x90, 0x08, 1)) * (float)2.0 / 255;
                    globleVariables.g_2ndOSC_VbiasScale_100mV_ch0 = (float)(DLL_2.USBCtrlTrans(0x90, 0x09, 1)) * (float)2.0 / 255;
                    globleVariables.g_2ndOSC_VbiasScale_50mV_ch0 = (float)(DLL_2.USBCtrlTrans(0x90, 0x0A, 1)) * (float)2.0 / 255;
                    globleVariables.g_2ndOSC_VbiasScale_20mV_ch0 = (float)(DLL_2.USBCtrlTrans(0x90, 0x2A, 1)) * (float)2.0 / 255;

                    globleVariables.g_2ndOSC_VbiasScale_500mV_ch1 = (float)(DLL_2.USBCtrlTrans(0x90, 0x0B, 1)) * (float)2.0 / 255;
                    globleVariables.g_2ndOSC_VbiasScale_100mV_ch1 = (float)(DLL_2.USBCtrlTrans(0x90, 0x0C, 1)) * (float)2.0 / 255;
                    globleVariables.g_2ndOSC_VbiasScale_50mV_ch1 = (float)(DLL_2.USBCtrlTrans(0x90, 0x0D, 1)) * (float)2.0 / 255;
                    globleVariables.g_2ndOSC_VbiasScale_20mV_ch1 = (float)(DLL_2.USBCtrlTrans(0x90, 0x2D, 1)) * (float)2.0 / 255;
                }
            }
            // 打开第3个设备
            if (globleVariables.g_OSCcnt >= 3)
            {
                lock (globleVariables.g_lockIO3)
                {
                    globleVariables.g_3rdOSC_VbiasZero02v = (DLL_3.USBCtrlTrans(0x90, 0x82, 1));
                    globleVariables.g_3rdOSC_VbiasZero12v = (DLL_3.USBCtrlTrans(0x90, 0x72, 1));
                    globleVariables.g_3rdOSC_VbiasZero01v = (DLL_3.USBCtrlTrans(0x90, 0x01, 1));
                    globleVariables.g_3rdOSC_VbiasZero11v = (DLL_3.USBCtrlTrans(0x90, 0x02, 1));
                    globleVariables.g_3rdOSC_VbiasZero0500mv = DLL_3.USBCtrlTrans(0x90, 0x0E, 1);
                    globleVariables.g_3rdOSC_VbiasZero1500mv = DLL_3.USBCtrlTrans(0x90, 0x0F, 1);
                    globleVariables.g_3rdOSC_VbiasZero0200mv = DLL_3.USBCtrlTrans(0x90, 0x14, 1);
                    globleVariables.g_3rdOSC_VbiasZero1200mv = DLL_3.USBCtrlTrans(0x90, 0x15, 1);
                    globleVariables.g_3rdOSC_VbiasZero0100mv = DLL_3.USBCtrlTrans(0x90, 0x12, 1);
                    globleVariables.g_3rdOSC_VbiasZero1100mv = DLL_3.USBCtrlTrans(0x90, 0x13, 1);
                    globleVariables.g_3rdOSC_VbiasZero050mv = DLL_3.USBCtrlTrans(0x90, 0x10, 1);
                    globleVariables.g_3rdOSC_VbiasZero150mv = DLL_3.USBCtrlTrans(0x90, 0x11, 1);
                    globleVariables.g_3rdOSC_VbiasZero120mv = DLL_3.USBCtrlTrans(0x90, 0xA1, 1);
                    globleVariables.g_3rdOSC_VbiasZero020mv = DLL_3.USBCtrlTrans(0x90, 0xA0, 1);
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    globleVariables.g_3rdOSC_VbiasScale_2V_ch0 = (float)(DLL_3.USBCtrlTrans(0x90, 0xC2, 1)) * (float)2.0 / 255; // 20201110 2V新增
                    globleVariables.g_3rdOSC_VbiasScale_1V_ch0 = (float)(DLL_3.USBCtrlTrans(0x90, 0x03, 1)) * (float)2.0 / 255;
                    globleVariables.g_3rdOSC_VbiasScale_200mV_ch0 = (float)(DLL_3.USBCtrlTrans(0x90, 0x06, 1)) * (float)2.0 / 255;
                    globleVariables.g_3rdOSC_VbiasScale_2V_ch1 = (float)(DLL_3.USBCtrlTrans(0x90, 0xD2, 1)) * (float)2.0 / 255; // 20201110 2V新增
                    globleVariables.g_3rdOSC_VbiasScale_1V_ch1 = (float)(DLL_3.USBCtrlTrans(0x90, 0x04, 1)) * (float)2.0 / 255;
                    globleVariables.g_3rdOSC_VbiasScale_200mV_ch1 = (float)(DLL_3.USBCtrlTrans(0x90, 0x07, 1)) * (float)2.0 / 255;
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    globleVariables.g_3rdOSC_VbiasScale_500mV_ch0 = (float)(DLL_3.USBCtrlTrans(0x90, 0x08, 1)) * (float)2.0 / 255;
                    globleVariables.g_3rdOSC_VbiasScale_100mV_ch0 = (float)(DLL_3.USBCtrlTrans(0x90, 0x09, 1)) * (float)2.0 / 255;
                    globleVariables.g_3rdOSC_VbiasScale_50mV_ch0 = (float)(DLL_3.USBCtrlTrans(0x90, 0x0A, 1)) * (float)2.0 / 255;
                    globleVariables.g_3rdOSC_VbiasScale_20mV_ch0 = (float)(DLL_3.USBCtrlTrans(0x90, 0x2A, 1)) * (float)2.0 / 255;

                    globleVariables.g_3rdOSC_VbiasScale_500mV_ch1 = (float)(DLL_3.USBCtrlTrans(0x90, 0x0B, 1)) * (float)2.0 / 255;
                    globleVariables.g_3rdOSC_VbiasScale_100mV_ch1 = (float)(DLL_3.USBCtrlTrans(0x90, 0x0C, 1)) * (float)2.0 / 255;
                    globleVariables.g_3rdOSC_VbiasScale_50mV_ch1 = (float)(DLL_3.USBCtrlTrans(0x90, 0x0D, 1)) * (float)2.0 / 255;
                    globleVariables.g_3rdOSC_VbiasScale_20mV_ch1 = (float)(DLL_3.USBCtrlTrans(0x90, 0x2D, 1)) * (float)2.0 / 255;
                }
            }
            // 打开第4个设备
            if (globleVariables.g_OSCcnt >= 4)
            {
                lock (globleVariables.g_lockIO4)
                {
                    globleVariables.g_4thOSC_VbiasZero02v = (DLL_4.USBCtrlTrans(0x90, 0x82, 1));
                    globleVariables.g_4thOSC_VbiasZero12v = (DLL_4.USBCtrlTrans(0x90, 0x72, 1));
                    globleVariables.g_4thOSC_VbiasZero01v = (DLL_4.USBCtrlTrans(0x90, 0x01, 1));
                    globleVariables.g_4thOSC_VbiasZero11v = (DLL_4.USBCtrlTrans(0x90, 0x02, 1));
                    globleVariables.g_4thOSC_VbiasZero0500mv = DLL_4.USBCtrlTrans(0x90, 0x0E, 1);
                    globleVariables.g_4thOSC_VbiasZero1500mv = DLL_4.USBCtrlTrans(0x90, 0x0F, 1);
                    globleVariables.g_4thOSC_VbiasZero0200mv = DLL_4.USBCtrlTrans(0x90, 0x14, 1);
                    globleVariables.g_4thOSC_VbiasZero1200mv = DLL_4.USBCtrlTrans(0x90, 0x15, 1);
                    globleVariables.g_4thOSC_VbiasZero0100mv = DLL_4.USBCtrlTrans(0x90, 0x12, 1);
                    globleVariables.g_4thOSC_VbiasZero1100mv = DLL_4.USBCtrlTrans(0x90, 0x13, 1);
                    globleVariables.g_4thOSC_VbiasZero050mv = DLL_4.USBCtrlTrans(0x90, 0x10, 1);
                    globleVariables.g_4thOSC_VbiasZero150mv = DLL_4.USBCtrlTrans(0x90, 0x11, 1);
                    globleVariables.g_4thOSC_VbiasZero120mv = DLL_4.USBCtrlTrans(0x90, 0xA1, 1);
                    globleVariables.g_4thOSC_VbiasZero020mv = DLL_4.USBCtrlTrans(0x90, 0xA0, 1);
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    globleVariables.g_4thOSC_VbiasScale_2V_ch0 = (float)(DLL_4.USBCtrlTrans(0x90, 0xC2, 1)) * (float)2.0 / 255; // 20201110 2V新增
                    globleVariables.g_4thOSC_VbiasScale_1V_ch0 = (float)(DLL_4.USBCtrlTrans(0x90, 0x03, 1)) * (float)2.0 / 255;
                    globleVariables.g_4thOSC_VbiasScale_200mV_ch0 = (float)(DLL_4.USBCtrlTrans(0x90, 0x06, 1)) * (float)2.0 / 255;
                    globleVariables.g_4thOSC_VbiasScale_2V_ch1 = (float)(DLL_4.USBCtrlTrans(0x90, 0xD2, 1)) * (float)2.0 / 255; // 20201110 2V新增
                    globleVariables.g_4thOSC_VbiasScale_1V_ch1 = (float)(DLL_4.USBCtrlTrans(0x90, 0x04, 1)) * (float)2.0 / 255;
                    globleVariables.g_4thOSC_VbiasScale_200mV_ch1 = (float)(DLL_4.USBCtrlTrans(0x90, 0x07, 1)) * (float)2.0 / 255;
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    globleVariables.g_4thOSC_VbiasScale_500mV_ch0 = (float)(DLL_4.USBCtrlTrans(0x90, 0x08, 1)) * (float)2.0 / 255;
                    globleVariables.g_4thOSC_VbiasScale_100mV_ch0 = (float)(DLL_4.USBCtrlTrans(0x90, 0x09, 1)) * (float)2.0 / 255;
                    globleVariables.g_4thOSC_VbiasScale_50mV_ch0 = (float)(DLL_4.USBCtrlTrans(0x90, 0x0A, 1)) * (float)2.0 / 255;
                    globleVariables.g_4thOSC_VbiasScale_20mV_ch0 = (float)(DLL_4.USBCtrlTrans(0x90, 0x2A, 1)) * (float)2.0 / 255;

                    globleVariables.g_4thOSC_VbiasScale_500mV_ch1 = (float)(DLL_4.USBCtrlTrans(0x90, 0x0B, 1)) * (float)2.0 / 255;
                    globleVariables.g_4thOSC_VbiasScale_100mV_ch1 = (float)(DLL_4.USBCtrlTrans(0x90, 0x0C, 1)) * (float)2.0 / 255;
                    globleVariables.g_4thOSC_VbiasScale_50mV_ch1 = (float)(DLL_4.USBCtrlTrans(0x90, 0x0D, 1)) * (float)2.0 / 255;
                    globleVariables.g_4thOSC_VbiasScale_20mV_ch1 = (float)(DLL_4.USBCtrlTrans(0x90, 0x2D, 1)) * (float)2.0 / 255;
                }
            }
            richTex_Calibration.Text = "chA: " +
                                       globleVariables.g_VbiasZero02v.ToString() + " " +
                                       globleVariables.g_VbiasZero01v.ToString() + " " +
                                       globleVariables.g_VbiasZero0500mv.ToString() + " " +
                                       globleVariables.g_VbiasZero0200mv.ToString() + " " +
                                       globleVariables.g_VbiasZero0100mv.ToString() + " " +
                                       globleVariables.g_VbiasZero050mv.ToString() + " " +
                                       globleVariables.g_VbiasZero020mv.ToString() + " " +
                                       globleVariables.g_VbiasScale_2V_ch0.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_1V_ch0.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_500mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_200mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_100mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_50mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_20mV_ch0.ToString("0.00") + "\n" +
                                       "chB: " +
                                       globleVariables.g_VbiasZero12v.ToString() + " " +
                                       globleVariables.g_VbiasZero11v.ToString() + " " +
                                       globleVariables.g_VbiasZero1500mv.ToString() + " " +
                                       globleVariables.g_VbiasZero1200mv.ToString() + " " +
                                       globleVariables.g_VbiasZero1100mv.ToString() + " " +
                                       globleVariables.g_VbiasZero150mv.ToString() + " " +
                                       globleVariables.g_VbiasZero120mv.ToString() + " " +
                                       globleVariables.g_VbiasScale_2V_ch1.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_1V_ch1.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_500mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_200mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_100mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_50mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_20mV_ch1.ToString("0.00") + "\n" +
                                           "chC: " +
                                       globleVariables.g_VbiasZeroC2v.ToString() + " " +
                                       globleVariables.g_VbiasZeroC1v.ToString() + " " +
                                       globleVariables.g_VbiasZeroC500mv.ToString() + " " +
                                       globleVariables.g_VbiasZeroC200mv.ToString() + " " +
                                       globleVariables.g_VbiasZeroC100mv.ToString() + " " +
                                       globleVariables.g_VbiasZeroC50mv.ToString() + " " +
                                       globleVariables.g_VbiasZeroC20mv.ToString() + " " +
                                       globleVariables.g_VbiasScale_2V_chC.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_1V_chC.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_500mV_chC.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_200mV_chC.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_100mV_chC.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_50mV_chC.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_20mV_chC.ToString("0.00") + "\n" +
                                           "chD: " +
                                       globleVariables.g_VbiasZeroD2v.ToString() + " " +
                                       globleVariables.g_VbiasZeroD1v.ToString() + " " +
                                       globleVariables.g_VbiasZeroD500mv.ToString() + " " +
                                       globleVariables.g_VbiasZeroD200mv.ToString() + " " +
                                       globleVariables.g_VbiasZeroD100mv.ToString() + " " +
                                       globleVariables.g_VbiasZeroD50mv.ToString() + " " +
                                       globleVariables.g_VbiasZeroD20mv.ToString() + " " +
                                       globleVariables.g_VbiasScale_2V_chD.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_1V_chD.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_500mV_chD.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_200mV_chD.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_100mV_chD.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_50mV_chD.ToString("0.00") + " " +
                                       globleVariables.g_VbiasScale_20mV_chD.ToString("0.00");


            // 打开第2个设备
            if (globleVariables.g_OSCcnt >= 2)
            {
                richTex_Calibration.Text += "\n" + "ch5: " +
                                       globleVariables.g_2ndOSC_VbiasZero02v.ToString() + " " +
                                       globleVariables.g_2ndOSC_VbiasZero01v.ToString() + " " +
                                       globleVariables.g_2ndOSC_VbiasZero0500mv.ToString() + " " +
                                       globleVariables.g_2ndOSC_VbiasZero0200mv.ToString() + " " +
                                       globleVariables.g_2ndOSC_VbiasZero0100mv.ToString() + " " +
                                       globleVariables.g_2ndOSC_VbiasZero050mv.ToString() + " " +
                                       globleVariables.g_2ndOSC_VbiasZero020mv.ToString() + " " +
                                       globleVariables.g_2ndOSC_VbiasScale_2V_ch0.ToString("0.00") + " " +
                                       globleVariables.g_2ndOSC_VbiasScale_1V_ch0.ToString("0.00") + " " +
                                       globleVariables.g_2ndOSC_VbiasScale_500mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_2ndOSC_VbiasScale_200mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_2ndOSC_VbiasScale_100mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_2ndOSC_VbiasScale_50mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_2ndOSC_VbiasScale_20mV_ch0.ToString("0.00") + "\n" +
                                       "ch6: " +
                                       globleVariables.g_2ndOSC_VbiasZero12v.ToString() + " " +
                                       globleVariables.g_2ndOSC_VbiasZero11v.ToString() + " " +
                                       globleVariables.g_2ndOSC_VbiasZero1500mv.ToString() + " " +
                                       globleVariables.g_2ndOSC_VbiasZero1200mv.ToString() + " " +
                                       globleVariables.g_2ndOSC_VbiasZero1100mv.ToString() + " " +
                                       globleVariables.g_2ndOSC_VbiasZero150mv.ToString() + " " +
                                       globleVariables.g_2ndOSC_VbiasZero120mv.ToString() + " " +
                                       globleVariables.g_2ndOSC_VbiasScale_2V_ch1.ToString("0.00") + " " +
                                       globleVariables.g_2ndOSC_VbiasScale_1V_ch1.ToString("0.00") + " " +
                                       globleVariables.g_2ndOSC_VbiasScale_500mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_2ndOSC_VbiasScale_200mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_2ndOSC_VbiasScale_100mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_2ndOSC_VbiasScale_50mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_2ndOSC_VbiasScale_20mV_ch1.ToString("0.00");
            }
            
            // 打开第3个设备
            if (globleVariables.g_OSCcnt >= 3)
            {
                richTex_Calibration.Text += "\n" + "ch9: " +
                                       globleVariables.g_3rdOSC_VbiasZero02v.ToString() + " " +
                                       globleVariables.g_3rdOSC_VbiasZero01v.ToString() + " " +
                                       globleVariables.g_3rdOSC_VbiasZero0500mv.ToString() + " " +
                                       globleVariables.g_3rdOSC_VbiasZero0200mv.ToString() + " " +
                                       globleVariables.g_3rdOSC_VbiasZero0100mv.ToString() + " " +
                                       globleVariables.g_3rdOSC_VbiasZero050mv.ToString() + " " +
                                       globleVariables.g_3rdOSC_VbiasZero020mv.ToString() + " " +
                                       globleVariables.g_3rdOSC_VbiasScale_2V_ch0.ToString("0.00") + " " +
                                       globleVariables.g_3rdOSC_VbiasScale_1V_ch0.ToString("0.00") + " " +
                                       globleVariables.g_3rdOSC_VbiasScale_500mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_3rdOSC_VbiasScale_200mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_3rdOSC_VbiasScale_100mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_3rdOSC_VbiasScale_50mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_3rdOSC_VbiasScale_20mV_ch0.ToString("0.00") + "\n" +
                                       "ch10: " +
                                       globleVariables.g_3rdOSC_VbiasZero12v.ToString() + " " +
                                       globleVariables.g_3rdOSC_VbiasZero11v.ToString() + " " +
                                       globleVariables.g_3rdOSC_VbiasZero1500mv.ToString() + " " +
                                       globleVariables.g_3rdOSC_VbiasZero1200mv.ToString() + " " +
                                       globleVariables.g_3rdOSC_VbiasZero1100mv.ToString() + " " +
                                       globleVariables.g_3rdOSC_VbiasZero150mv.ToString() + " " +
                                       globleVariables.g_3rdOSC_VbiasZero120mv.ToString() + " " +
                                       globleVariables.g_3rdOSC_VbiasScale_2V_ch1.ToString("0.00") + " " +
                                       globleVariables.g_3rdOSC_VbiasScale_1V_ch1.ToString("0.00") + " " +
                                       globleVariables.g_3rdOSC_VbiasScale_500mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_3rdOSC_VbiasScale_200mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_3rdOSC_VbiasScale_100mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_3rdOSC_VbiasScale_50mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_3rdOSC_VbiasScale_20mV_ch1.ToString("0.00");
            }
            
            // 打开第4个设备
            if (globleVariables.g_OSCcnt >= 4)
            {
                richTex_Calibration.Text += "\n" + "ch13: " +
                                       globleVariables.g_4thOSC_VbiasZero02v.ToString() + " " +
                                       globleVariables.g_4thOSC_VbiasZero01v.ToString() + " " +
                                       globleVariables.g_4thOSC_VbiasZero0500mv.ToString() + " " +
                                       globleVariables.g_4thOSC_VbiasZero0200mv.ToString() + " " +
                                       globleVariables.g_4thOSC_VbiasZero0100mv.ToString() + " " +
                                       globleVariables.g_4thOSC_VbiasZero050mv.ToString() + " " +
                                       globleVariables.g_4thOSC_VbiasZero020mv.ToString() + " " +
                                       globleVariables.g_4thOSC_VbiasScale_2V_ch0.ToString("0.00") + " " +
                                       globleVariables.g_4thOSC_VbiasScale_1V_ch0.ToString("0.00") + " " +
                                       globleVariables.g_4thOSC_VbiasScale_500mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_4thOSC_VbiasScale_200mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_4thOSC_VbiasScale_100mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_4thOSC_VbiasScale_50mV_ch0.ToString("0.00") + " " +
                                       globleVariables.g_4thOSC_VbiasScale_20mV_ch0.ToString("0.00") + "\n" +
                                       "ch14: " +
                                       globleVariables.g_4thOSC_VbiasZero12v.ToString() + " " +
                                       globleVariables.g_4thOSC_VbiasZero11v.ToString() + " " +
                                       globleVariables.g_4thOSC_VbiasZero1500mv.ToString() + " " +
                                       globleVariables.g_4thOSC_VbiasZero1200mv.ToString() + " " +
                                       globleVariables.g_4thOSC_VbiasZero1100mv.ToString() + " " +
                                       globleVariables.g_4thOSC_VbiasZero150mv.ToString() + " " +
                                       globleVariables.g_4thOSC_VbiasZero120mv.ToString() + " " +
                                       globleVariables.g_4thOSC_VbiasScale_2V_ch1.ToString("0.00") + " " +
                                       globleVariables.g_4thOSC_VbiasScale_1V_ch1.ToString("0.00") + " " +
                                       globleVariables.g_4thOSC_VbiasScale_500mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_4thOSC_VbiasScale_200mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_4thOSC_VbiasScale_100mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_4thOSC_VbiasScale_50mV_ch1.ToString("0.00") + " " +
                                       globleVariables.g_4thOSC_VbiasScale_20mV_ch1.ToString("0.00");
            }
                               
        }

        private void Form1_Load(object sender, EventArgs e)//20230726主窗口LOAD时就把数据处理线程跑起来
        {
            but_CycleRead.Text = "循环采集";
            globleVariables.bCycleReadFlg = false;                   //默认是不执行循环采集操作的
            globleVariables.CycleReadCnt = 0xffffffff;               //默认的循环次数是一直循环，只要大于10000就是一直循环

            tex_DataOffset.Text = globleVariables.DataOffset.ToString();
            textDataQuatity.Text = globleVariables.DataCount.ToString();//显示多少数据量（每通道）
            LayoutRefresh();  //根据通道数布局波形显示区的像素高度和位置

            LOTOReadThreadObject = new LOTOReadThread(this);       //实例化我们建立的线程的类
            ReadThread = new Thread(LOTOReadThreadObject.DoWork);  //生成一个线程，用类里的一个函数作为线程循环体
            ReadThread.IsBackground = true;                        //让这个线程在后台运行

            comboSampleRate.SelectedIndex = 2;  //采样率默认 781K
            comboRangeA.SelectedIndex = 1;      //通道A电压范围默认+-5V
            comboRangeB.SelectedIndex = 1;      //通道B电压范围默认+-5V 
            comboRangeC.SelectedIndex = 1;      //通道C电压范围默认+-5V 
            comboRangeD.SelectedIndex = 1;      //通道D电压范围默认+-5V 

            combo_TriggerEdge.SelectedIndex = 0;//上升沿触发

            groupBoxSPS.Enabled = false;
            groupBoxCHA.Enabled = false;
            groupBoxCHB.Enabled = false;
            groupBoxCHC.Enabled = false;
            groupBoxCHD.Enabled = false;
            checkBox_TriggerOn.Enabled = false;
            but_SingleRead.Enabled = false;
            but_CycleRead.Enabled = false;
            butGetCalibration.Enabled = false;
            groupBoxTrigSet.Enabled = false;
            groupBoxCHBset.Enabled = false;
            group_Trig.Visible = false;

            ReadThread.Start();  //采集线程运行

      
         }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)//20230726主窗口关闭时就把线程停止和释放掉
        {
            if (LOTOReadThreadObject != null)
            {
                LOTOReadThreadObject.RequestStop();
            }
            Thread.Sleep(15);
            // Abort thread

            if (ReadThread != null)
            {
                if ((ReadThread.ThreadState & ThreadState.Suspended) == ThreadState.Suspended)
                {
                    ReadThread.Resume();
                }
                ReadThread.Abort();
            }
        }

        private void ToggleCycleRead()
        {
            if (globleVariables.bCycleReadFlg == false)
            {
                //20230726 一个状态标志，是否是在循环采集中，默认为FALSE，如果是正在进行循环采集，则为TRUE。
                globleVariables.bCycleReadFlg = true;
                but_CycleRead.Text = "停止循环";
                but_SingleRead.Enabled = false;
                checkBox_TriggerOn.Enabled = false;
                comboSampleRate.Enabled = false;
            }
            else
            {
                globleVariables.bCycleReadFlg = false;
                but_CycleRead.Text = "循环采集";
                but_SingleRead.Enabled = true;
                checkBox_TriggerOn.Enabled = true;
                comboSampleRate.Enabled = true;
            }
        }

        private void but_CycleRead_Click(object sender, EventArgs e)
        {
            ToggleCycleRead();
        }

        private void but_SingleRead_Click(object sender, EventArgs e)
        {            
            if (globleVariables.CycleReadCnt == 1 )  //如果已经在单次采集过程中了，直接返回
            {
                return;
            }
            if (globleVariables.bCycleReadFlg)       //如果已经在循环采集过程中了，直接返回
            {
                return;
            }
            globleVariables.bCycleReadFlg = true;   //开启循环
            globleVariables.CycleReadCnt = 1;       //设置循环一次
        }

        private void butDataOffset_Set_Click(object sender, EventArgs e)
        {
            globleVariables.DataOffset = Convert.ToInt32(tex_DataOffset.Text);
            if (globleVariables.DataOffset<0)
            {
                globleVariables.DataOffset = 0;
                tex_DataOffset.Text = "0";
            }
            else if (globleVariables.DataOffset > 1024*64)
            {
                globleVariables.DataOffset = 1024 * 64;
                tex_DataOffset.Text = "65536";
            }
        }

        private void butDataQuatitySet_Click(object sender, EventArgs e)//显示多少数据量（每通道）
        {
            globleVariables.DataCount = Convert.ToInt32(textDataQuatity.Text);
            if (globleVariables.DataCount <= 5)
            {
                globleVariables.DataCount = 5;
                textDataQuatity.Text = "5";
            }
            else if (globleVariables.DataCount > 1024 * 64 - globleVariables.DataOffset)
            {
                globleVariables.DataCount = 1024 * 64 - globleVariables.DataOffset;
                textDataQuatity.Text = globleVariables.DataCount.ToString();
            }
        }

        public void ActivateSamplingRate()//发送采样率命令实施采样率切换
        {
            if (comboSampleRate.SelectedIndex == 0)//100M采样
            {
                g_CtrlByte0 &= 0xf0;                // 设置100M Hz 采样率
                g_CtrlByte0 |= 0x00;
            }
            else if (comboSampleRate.SelectedIndex == 1)//12.5M采样
            {
                g_CtrlByte0 &= 0xf0;               // 设置12.5M Hz 采样率
                g_CtrlByte0 |= 0x08;
            }
            else if (comboSampleRate.SelectedIndex == 2)//781k采样
            {
                g_CtrlByte0 &= 0xf0;                // 设置781K Hz 采样率
                g_CtrlByte0 |= 0x0c;
            }
            else if (comboSampleRate.SelectedIndex == 3)//49k采样
            {
                g_CtrlByte0 &= 0xf0;               // 设置49K Hz 采样率
                g_CtrlByte0 |= 0x0e;
            }
            lock (globleVariables.g_lockIO)
            {
                DLL_1.USBCtrlTrans(0x94, g_CtrlByte0, 1); // 设备1
            }
            // 第二个设备
            if (globleVariables.g_OSCcnt >= 2)
            {
                lock (globleVariables.g_lockIO2)
                {
                    DLL_2.USBCtrlTrans(0x94, g_CtrlByte0, 1); // 设备2
                }
            }
            // 第3个设备
            if (globleVariables.g_OSCcnt >= 3)
            {
                lock (globleVariables.g_lockIO3)
                {
                    DLL_3.USBCtrlTrans(0x94, g_CtrlByte0, 1); // 设备3
                }
            }
            // 第4个设备
            if (globleVariables.g_OSCcnt >= 4)
            {
                lock (globleVariables.g_lockIO4)
                {
                    DLL_4.USBCtrlTrans(0x94, g_CtrlByte0, 1); // 设备4
                }
            }
        }
        private void comboSampleRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActivateSamplingRate();//发送采样率命令实施采样率切换
        }

        private void comboRangeA_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActivateRangeCHA();//发送命令实施通道A的电压测试范围设置
        }
        private void comboRangeB_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActivateRangeCHB();//发送命令实施通道A的电压测试范围设置
        }
        private void comboRangeC_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActivateRangeCHC();//发送命令实施通道C的电压测试范围设置
        }
        private void comboRangeD_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActivateRangeCHD();//发送命令实施通道D的电压测试范围设置   
        }
        private void checkBox_TriggerOn_CheckedChanged(object sender, EventArgs e)
        {
            ActivateTriggerONoff();//发送命令，是否开启触发功能
            Thread.Sleep(100);
        }
        public void  ActivateTriggerONoff()//发送命令，是否开启触发功能
        {
             if (checkBox_TriggerOn.Checked == false)//关闭触发
             {
                  g_CtrlByte1 &= 0xdf;
                  g_CtrlByte1 |= 0x20;
                  lock (globleVariables.g_lockIO)
                  {
                     DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);    //开启设备1的外触发   
                     DLL_1.USBCtrlTrans(0xE7, 0x00, 1); // 设备1
                  }
                  if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                  {
                      g_2ndOSC_CtrlByte1 &= 0xdf;                     
                      lock (globleVariables.g_lockIO2)
                      {
                          DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);    //关闭设备2的外触发 
                          DLL_2.USBCtrlTrans(0xE7, 0x00, 1); // 设备2关闭触发
                      }
                  }
                  if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                  {
                      g_3rdOSC_CtrlByte1 &= 0xdf;
                      lock (globleVariables.g_lockIO3)
                      {
                          DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);    //关闭设备3的外触发 
                          DLL_3.USBCtrlTrans(0xE7, 0x00, 1); // 设备3关闭触发
                      }
                  }
                  if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                  {
                      g_4thOSC_CtrlByte1 &= 0xdf;
                      lock (globleVariables.g_lockIO4)
                      {
                          DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);    //关闭设备4的外触发 
                          DLL_4.USBCtrlTrans(0xE7, 0x00, 1); // 设备4关闭触发
                      }
                  }
                   group_Trig.Visible = false;
              }
              else //开启触发
              {
                  byte TriggerLevel = (byte)track_TriggerLevel.Value;

                 
                  g_CtrlByte1 &= 0xdf;
                  lock (globleVariables.g_lockIO)
                  {
                      DLL_1.USBCtrlTrans(0x24, g_CtrlByte1, 1);    //关闭设备1的外触发 
                      DLL_1.USBCtrlTrans(0x16, TriggerLevel, 1);  // 设备1 设置触发数据
                      DLL_1.USBCtrlTrans(0xE7, 0x01, 1); // 设备1开启触发
                  }
                  if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                  {
                      g_2ndOSC_CtrlByte1 &= 0xdf;
                      g_2ndOSC_CtrlByte1 |= 0x20;
                      lock (globleVariables.g_lockIO2)
                      {
                          DLL_2.USBCtrlTrans(0x24, g_2ndOSC_CtrlByte1, 1);    //开启设备2的外触发  
                          DLL_2.USBCtrlTrans(0x16, TriggerLevel, 1);  // 设备1 设置触发数据
                          DLL_2.USBCtrlTrans(0xE7, 0x01, 1); // 设备2开启触发
                      }
                  }
                  if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                  {
                      g_3rdOSC_CtrlByte1 &= 0xdf;
                      g_3rdOSC_CtrlByte1 |= 0x20;
                      lock (globleVariables.g_lockIO3)
                      {
                          DLL_3.USBCtrlTrans(0x24, g_3rdOSC_CtrlByte1, 1);    //开启设备3的外触发   
                          DLL_3.USBCtrlTrans(0x16, TriggerLevel, 1);  // 设备1 设置触发数据
                          DLL_3.USBCtrlTrans(0xE7, 0x01, 1); // 设备3开启触发
                      }
                  }
                  if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                  {
                      g_4thOSC_CtrlByte1 &= 0xdf;
                      g_4thOSC_CtrlByte1 |= 0x20;
                      lock (globleVariables.g_lockIO4)
                      {
                          DLL_4.USBCtrlTrans(0x24, g_4thOSC_CtrlByte1, 1);    //开启设备4的外触发  
                          DLL_4.USBCtrlTrans(0x16, TriggerLevel, 1);  // 设备1 设置触发数据
                          DLL_4.USBCtrlTrans(0xE7, 0x01, 1); // 设备4开启触发
                      }
                  }
                  group_Trig.Visible = true;
              }
        }


        private void combo_TriggerEdge_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActivateTriggerEdge();//发送命令让示波器设备1设置执行触发边沿。
        }

        public void ActivateTriggerEdge()//发送命令让示波器设备1设置执行触发边沿。
        {
            if (combo_TriggerEdge.SelectedIndex == 1)//上升沿触发
            {
                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0xC5, 0x00, 1); // 设备1 上升沿，或者设置LED绿色灯灭
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0xC5, 0x00, 1); // 设备2 上升沿，或者设置LED绿色灯灭
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0xC5, 0x00, 1); // 设备3 上升沿，或者设置LED绿色灯灭
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0xC5, 0x00, 1); // 设备4 上升沿，或者设置LED绿色灯灭
                    }
                }
            }
            else if (combo_TriggerEdge.SelectedIndex == 0)//下降沿触发
            {
                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0xC5, 0x01, 1); // 设备1 下降沿设置，或者设置LED绿色灯亮
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0xC5, 0x01, 1); // 设备2 上升沿，或者设置LED绿色灯灭
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0xC5, 0x01, 1); // 设备3 上升沿，或者设置LED绿色灯灭
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0xC5, 0x01, 1); // 设备4 上升沿，或者设置LED绿色灯灭
                    }
                }
            }
        }

        private void track_TriggerLevel_MouseUp(object sender, MouseEventArgs e)
        {
            byte TriggerLevel = (byte)track_TriggerLevel.Value;
            lock (globleVariables.g_lockIO)
            {
                DLL_1.USBCtrlTrans(0x16, TriggerLevel, 1);  // 设备1 设置触发数据
            }
        }

        private void track_TriggerLevel_ValueChanged(object sender, EventArgs e)
        {
            double TriggerLevel = globleVariables.dataTransform((byte)track_TriggerLevel.Value, globleVariables.g_CurrentZero0, globleVariables.g_CurrentScale_ch0, globleVariables.RangeV);//将设备1的原始字节数据转换成真正的电压值
            label_Tvalue.Text = TriggerLevel.ToString("0.00");
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
           LayoutRefresh();//根据通道数布局波形显示区的像素高度和位置
        }

        private void checkBox_TrigHiSense_CheckedChanged(object sender, EventArgs e)
        {
            byte TriggerLevel = (byte)track_TriggerLevel.Value;

            if (checkBox_TrigHiSense.Checked == true)//触发使用了高灵敏度
            {
                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x2B, 1, 1); ////设备1 设置高灵敏度触发
                    DLL_1.USBCtrlTrans(0x16, TriggerLevel, 1);  // 设备1 设置触发数据
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x2B, 1, 1); //设备2 设置高灵敏度触发
                        DLL_2.USBCtrlTrans(0x16, TriggerLevel, 1);  // 设备2 设置触发数据
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x2B, 1, 1); //设备3 设置高灵敏度触发
                        DLL_3.USBCtrlTrans(0x16, TriggerLevel, 1);  // 设备13设置触发数据
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x2B, 1, 1); //设备4设置高灵敏度触发
                        DLL_4.USBCtrlTrans(0x16, TriggerLevel, 1);  // 设备4设置触发数据
                    }
                }
            }
            else
            {
                lock (globleVariables.g_lockIO)
                {
                    DLL_1.USBCtrlTrans(0x2B, 0, 1); //
                    DLL_1.USBCtrlTrans(0x16, TriggerLevel, 1);  // 设备1 设置触发数据
                }
                if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                {
                    lock (globleVariables.g_lockIO2)
                    {
                        DLL_2.USBCtrlTrans(0x2B, 0, 1); //设备2 设置低灵敏度触发
                        DLL_2.USBCtrlTrans(0x16, TriggerLevel, 1);  // 设备2 设置触发数据
                    }
                }
                if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                {
                    lock (globleVariables.g_lockIO3)
                    {
                        DLL_3.USBCtrlTrans(0x2B, 0, 1); //设备3 设置低灵敏度触发
                        DLL_3.USBCtrlTrans(0x16, TriggerLevel, 1);  // 设备13设置触发数据
                    }
                }
                if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                {
                    lock (globleVariables.g_lockIO4)
                    {
                        DLL_4.USBCtrlTrans(0x2B, 0, 1); //设备4设置低灵敏度触发
                        DLL_4.USBCtrlTrans(0x16, TriggerLevel, 1);  // 设备4设置触发数据
                    }
                }
            }
            
        }

        private void groupBoxSPS_Enter(object sender, EventArgs e)
        {

        }

        private void trackBarTrigPosition_ValueChanged(object sender, EventArgs e)//水平触发位置设定
        {
            textBoxTrigPosition.Text = trackBarTrigPosition.Value.ToString();
        }

        private void butTrigPositionSet_Click(object sender, EventArgs e)//执行水平触发位置设定
        {
            if (textBoxTrigPosition.Text == "")
            {
                return;
            }
            int va = Convert.ToInt32(textBoxTrigPosition.Text);
            if(va <0)
            {
               va = 0;
               textBoxTrigPosition.Text = "0";
            }
            else if(va >65535)
            {
               va = 65535;
               textBoxTrigPosition.Text = "65535";
            }
            trackBarTrigPosition.Value = va;

            UInt16 PreTrigValue = (UInt16)(trackBarTrigPosition.Value );
            byte Low8 = (byte)PreTrigValue;
            byte High8 = (byte)(PreTrigValue >> 8);

            lock (globleVariables.g_lockIO)
            {
                DLL_1.USBCtrlTrans(0x18, (byte)Low8, 1);
                DLL_1.USBCtrlTrans(0x17, (byte)High8, 1);
            }
            if (globleVariables.g_OSCcnt >= 2) //级联了设备2
            {
                lock (globleVariables.g_lockIO2)
                {
                    DLL_2.USBCtrlTrans(0x18, (byte)Low8, 1);
                    DLL_2.USBCtrlTrans(0x17, (byte)High8, 1);
                }
            }
            if (globleVariables.g_OSCcnt >= 3) //级联了设备3
            {
                lock (globleVariables.g_lockIO3)
                {
                    DLL_3.USBCtrlTrans(0x18, (byte)Low8, 1);
                    DLL_3.USBCtrlTrans(0x17, (byte)High8, 1);
                }
            }
            if (globleVariables.g_OSCcnt >= 4) //级联了设备4
            {
                lock (globleVariables.g_lockIO4)
                {
                    DLL_4.USBCtrlTrans(0x18, (byte)Low8, 1);
                    DLL_4.USBCtrlTrans(0x17, (byte)High8, 1);
                }
            }
        }

        private void pictureBox_ch9_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.DarkBlue, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_chA.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_chA.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值            
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置 

            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_ch9DataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_ch9DataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }
            if (isSaving)
            {
                // 通道 9
                byte[] copy9 = new byte[globleVariables.DataCount];
                Array.Copy(globleVariables.g_ch9DataArray, copy9, globleVariables.DataCount);
                _ = CsvHelper.SaveBytesToCsvByPointerAsync(copy9, "ch9", saveFolderPath);

                // 通道 10
                byte[] copy10 = new byte[globleVariables.DataCount];
                Array.Copy(globleVariables.g_ch10DataArray, copy10, globleVariables.DataCount);
                _ = CsvHelper.SaveBytesToCsvByPointerAsync(copy10, "ch10", saveFolderPath);

                // 通道 11
                byte[] copy11 = new byte[globleVariables.DataCount];
                Array.Copy(globleVariables.g_ch11DataArray, copy11, globleVariables.DataCount);
                _ = CsvHelper.SaveBytesToCsvByPointerAsync(copy11, "ch11", saveFolderPath);

                // 通道 12
                byte[] copy12 = new byte[globleVariables.DataCount];
                Array.Copy(globleVariables.g_ch12DataArray, copy12, globleVariables.DataCount);
                _ = CsvHelper.SaveBytesToCsvByPointerAsync(copy12, "ch12", saveFolderPath);

            }

        }

        private void pictureBox_ch10_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.DarkGreen, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_chA.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_chA.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值            
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置 

            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_ch10DataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_ch10DataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }
        }

        private void pictureBox_ch11_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.Purple, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_chA.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_chA.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值            
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置 

            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_ch11DataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_ch11DataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }
        }

        private void pictureBox_ch12_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.RosyBrown, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_chA.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_chA.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值            
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置 

            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_ch12DataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_ch12DataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }
        }

        private void pictureBox_ch13_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.RoyalBlue, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_chA.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_chA.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值            
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置 

            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_ch13DataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_ch13DataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }
            if (isSaving)
            {

                // 通道 13
                byte[] copy13 = new byte[globleVariables.DataCount];
                Array.Copy(globleVariables.g_ch13DataArray, copy13, globleVariables.DataCount);
                _ = CsvHelper.SaveBytesToCsvByPointerAsync(copy13, "ch13", saveFolderPath);

                // 通道 14
                byte[] copy14 = new byte[globleVariables.DataCount];
                Array.Copy(globleVariables.g_ch14DataArray, copy14, globleVariables.DataCount);
                _ = CsvHelper.SaveBytesToCsvByPointerAsync(copy14, "ch14", saveFolderPath);

                // 通道 15
                byte[] copy15 = new byte[globleVariables.DataCount];
                Array.Copy(globleVariables.g_ch15DataArray, copy15, globleVariables.DataCount);
                _ = CsvHelper.SaveBytesToCsvByPointerAsync(copy15, "ch15", saveFolderPath);

                // 通道 16
                byte[] copy16 = new byte[globleVariables.DataCount];
                Array.Copy(globleVariables.g_ch16DataArray, copy16, globleVariables.DataCount);
                _ = CsvHelper.SaveBytesToCsvByPointerAsync(copy16, "ch16", saveFolderPath);
            }

        }

        private void pictureBox_ch14_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.DeepPink, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_chA.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_chA.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值            
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置 

            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_ch14DataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_ch14DataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }
        }

        private void pictureBox_ch15_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.Black, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_chA.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_chA.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值            
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置 

            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_ch15DataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_ch15DataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }
        }

        private void pictureBox_ch16_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen PanelPen = new Pen(Color.DarkGray, 1);

            float xpos = 0;                        // 绘图区X轴的像素起始位置
            int xLength = pictureBox_chA.Width;    // 绘图区X轴的像素长度
            float ypos1 = 0;                       // y轴的像素位置
            float ypos2 = 0;                       // y轴的像素位置
            int yLength = pictureBox_chA.Height;  // 绘图区X轴的像素长度
            int DataMin = 0;                      // 原始数据的最小值
            int DataMax = 255;                    // 原始数据的最大值            
            int startXpos = 0;                    // 绘图区x轴的像素起始位置
            int startYpos = 0;                    // 绘图区y轴的像素起始位置 

            float step_x = (float)xLength / (float)globleVariables.DataCount;                 // 画一个数据步进多少个屏幕像素
            float step_y = (float)(yLength) / (float)(DataMax - DataMin);  // 原始数据每增加一个分辨率，纵向画多少个屏幕像素

            for (int i = 0; i < globleVariables.DataCount - 1; i++)
            {
                xpos = startXpos + i * step_x;
                ypos1 = startYpos + yLength - (globleVariables.g_ch16DataArray[i] - DataMin) * step_y;     // y的坐标是从上向下的，所以要做个减法
                ypos2 = startYpos + yLength - (globleVariables.g_ch16DataArray[i + 1] - DataMin) * step_y;   // y的坐标是从上向下的，所以要做个减法
                g.DrawLine(PanelPen, xpos, ypos1, xpos + step_x, ypos2);
            }
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {

        }

        int waveform = 1;

        private void butSine_Click(object sender, EventArgs e)
        {
            DLL_1.USBCtrlTrans(0xC7, 0x01, 1);
            DLL_1.USBCtrlTransSimple((Int32)0x60);
            // 参数使能
            DLL_1.USBCtrlTransSimple((Int32)0x74);
            waveform = 1;
        }

        private void butTri_Click(object sender, EventArgs e)
        {
            DLL_1.USBCtrlTrans(0xC7, 0x01, 1);
            DLL_1.USBCtrlTransSimple((Int32)0x61);
            // 参数使能
            DLL_1.USBCtrlTransSimple((Int32)0x74);
            waveform = 2;
        }

        private void butSquare_Click(object sender, EventArgs e)
        {
            DLL_1.USBCtrlTrans(0xC7, 0x0, 1);
            DLL_1.USBCtrlTransSimple((Int32)0x62);
            // 参数使能
            DLL_1.USBCtrlTransSimple((Int32)0x74);
            waveform = 3;
        }

        private void but_StartSignal_Click(object sender, EventArgs e)
        {
            // DE-15 IO 设置为输出状态
            // 将示波器 DE-15 可扩展功能模块接口，中部分IO设置为输出状态；用于满足信号发生器模块的需要
            {
                DLL_1.USBCtrlTrans(0x84, 0x00, 1);
                DLL_1.USBCtrlTrans(0x84, 0x01, 1);
                DLL_1.USBCtrlTrans(0x84, 0x02, 1);
                DLL_1.USBCtrlTrans(0x84, 0x03, 1);
            }
        }

        private uint m_FreOut = 1000;      // DDS信号发生器,常规波形输出频率;   默认1kHz
        // 扫频
        private int m_LowFreq = 0;         // 默认的扫频频率最低值
        private int m_HighFreq = 1000000;  // 默认的扫频频率最大值
        private int m_TimeSpan = 500;      // 默认扫频步进的时间间隔
        private int m_FreqSpan = 100;      // 默认扫频的步进量
        private bool m_PN = true;          // 默认扫频的方向(从小频率到大频率 【递增】 )

        private int Flatrate = 0;          // 扫频时所用到的变量(步进记数)

        private System.Timers.Timer ScanFreq; // 扫频定时器

        private void butSetFreq_Click(object sender, EventArgs e)
        {
            decimal ndcmalFre = this.numUpDownSignalFre.Value;
            uint.TryParse(ndcmalFre.ToString(), out m_FreOut); // 获取用户输入的信号发生器的频率

            if (m_FreOut < 1)
            {
                m_FreOut = 1;
            }
            if (m_FreOut > 13000000) // 模块最大可以发生13M的频率
            {
                m_FreOut = 13000000; // 13M
            }

            // 设置频率(调用频率设置公共函数)
            this.SetFreq(m_FreOut);
        }
        // 频率设置 [公共函数]
        private void SetFreq(uint paramA/*频率*/ )
        {
            //if (paramA <= 0)
            //{
            //    paramA = 1; //  如果频率小于等于0，频率默认为1hz
            //}
            ushort[] m_freqDDS = new ushort[4];

            ulong temp = (ulong)(paramA * 11.18055 / 2);
            m_freqDDS[0] = (ushort)(temp & 0x00003fff);  //0000 0000 0000 0000 0011 1111 1111 1111
            m_freqDDS[2] = m_freqDDS[0];
            m_freqDDS[1] = (ushort)((temp & 0x0fffc000) >> 14);  //0000 1111 1111 1111 1100 0000 0000 0000
            m_freqDDS[3] = m_freqDDS[1];

            m_freqDDS[0] = (ushort)(m_freqDDS[0] | 0x4000);  //0100 0000 0000 0000
            m_freqDDS[1] = (ushort)(m_freqDDS[1] | 0x4000);  //0100 0000 0000 0000
            m_freqDDS[2] = (ushort)(m_freqDDS[2] | 0x8000);  //1000 0000 0000 0000
            m_freqDDS[3] = (ushort)(m_freqDDS[3] | 0x8000);  //1000 0000 0000 0000

            // 下发命令
            {
                DLL_1.USBCtrlTrans(0x70, m_freqDDS[0], 1);//REQ_DDS_DATA0    
                DLL_1.USBCtrlTrans(0x71, m_freqDDS[1], 1);//REQ_DDS_DATA1 
                DLL_1.USBCtrlTrans(0x72, m_freqDDS[2], 1);//REQ_DDS_DATA2  
                DLL_1.USBCtrlTrans(0x73, m_freqDDS[3], 1);//REQ_DDS_DATA3 
            }
            // 参数使能
            DLL_1.USBCtrlTransSimple((Int32)0x74);
        }

        private void butFreScanFreq_Click(object sender, EventArgs e)
        {
            // 获取用户输入文本框中的值

            // 扫频频率最低值
            int.TryParse(this.LowFreq.Text.Trim(), out m_LowFreq);
            // 扫频频率最大值
            int.TryParse(this.HgihFreq.Text.Trim(), out m_HighFreq);
            // 扫频步进的时间间隔
            int.TryParse(this.TimSpan.Text.Trim(), out m_TimeSpan);
            // 扫频的步进量
            int.TryParse(this.FreqSpan.Text.Trim(), out m_FreqSpan);


            // 判断用户输入最低值与最大值，确定是递增 还是 递减
            if (m_LowFreq < m_HighFreq)
            {
                m_PN = true; // 递增 ( 起始频率小，最终频率大 )
            }
            else if (m_LowFreq > m_HighFreq)
            {
                m_PN = true; // 递减 ( 起始频率大，最终频率小 )
            }


            // 锁定输入界面
            this.LowFreq.Enabled = false;
            this.HgihFreq.Enabled = false;
            this.TimSpan.Enabled = false;
            this.FreqSpan.Enabled = false;

            // 设置扫频定时器并启动
            ScanFreq.Interval = m_TimeSpan; // 设置定时器调用时间间隔
            ScanFreq.Enabled = true;       // 启用定时器
            ScanFreq.Start();               // 启动定时器


            this.StopFreScan.Enabled = true;
            this.butFrePause.Enabled = true;
            this.butFreScanFreq.Enabled = false;
            this.StopFreScan.Enabled = true;
        }

        // 暂停扫频
        private void butFrePause_Click(object sender, EventArgs e)
        {
           // ScanFreq.Enabled = !ScanFreq.Enabled;

            if (ScanFreq.Enabled == false)
            {
                butFrePause.BackColor = Color.Yellow;
            }
            else
            {
                butFrePause.BackColor = System.Drawing.SystemColors.Control;
            }
        }

        // 结束扫频
        private void StopFreScan_Click(object sender, EventArgs e)
        {
            StopFreqTimers();
        }

        // 结束扫频公共函数
        private void StopFreqTimers()
        {
            // 停止扫频定时器
            Flatrate = 0;
            ScanFreq.Stop();
            ScanFreq.Enabled = false;
            this.butFreScanFreq.Enabled = true;
            this.StopFreScan.Enabled = false;
            this.butFrePause.Enabled = false;
            this.butFrePause.BackColor = System.Drawing.SystemColors.Control;

            this.LowFreq.Enabled = true;
            this.HgihFreq.Enabled = true;
            this.FreqSpan.Enabled = true;
            this.TimSpan.Enabled = true;
        }
        // 自动频率定时器函数
        public void ScanFreqStartHandler(object source, System.Timers.ElapsedEventArgs e)
        {
            long m_Freq = 0;
            bool bover = false; // 扫频是否结束
            string strappend = "Hz";

            // /////  ///  /// ///  /// 

            if (m_PN) // 递增
            {
                // 最低频率 + (当前第几次 * 扫频的步进量)
                m_Freq = m_LowFreq + (Flatrate * m_FreqSpan);

                if (m_Freq >= m_HighFreq) // 是否达到最终值结束
                {
                    ScanFreq.Enabled = false; // 定时器停止

                    Flatrate = 0;
                    bover = true; // 结束
                    m_Freq = m_HighFreq; // 保证频率锁定在结束值

                }
                else
                {
                    strappend += " ↑";
                }
            }
            else // 递减
            {
                // 最高频率 - (当前第几次 * 扫频的步进量)
                m_Freq = m_LowFreq - (Flatrate * m_FreqSpan);

                if (m_Freq <= m_HighFreq) // 是否达到最终值结束
                {
                    ScanFreq.Enabled = false; // 定时器停止

                    Flatrate = 0;
                    bover = true; // 结束
                    m_Freq = m_HighFreq; // 保证频率锁定在结束值

                }
                else
                {
                    strappend += " ↓";
                }
            }

            Flatrate++; // 步进计数器累加

            // 设置频率(调用频率设置公共函数)
            this.SetFreq((uint)m_Freq);

            // 是否结束
            if (bover)
            {
                StopFreqTimers();
            }
        }

        private void but_StopSignal_Click(object sender, EventArgs e)
        {
            DLL_1.USBCtrlTrans(0x2C, 0, 1);//20220816 关闭和开启DDS
        }

       

        private void but_activeSynchronization_Click(object sender, EventArgs e)
        {
            butSetFreq_Click(sender, e);
            but_CycleRead_Click(sender, e);
        }


        private System.Windows.Forms.Timer periodicTimer;       // 定时器
        private bool isPeriodicRunning = false;
        private bool isEmitting = false;   // 当前是否处于“发射状态”
        private int onMs = 100;           // 发射时长
        private int offMs = 100;
        private void but_periodicSync_Click(object sender, EventArgs e)
        {
            if (!isPeriodicRunning)
            {
                // 读取参数
                if (!int.TryParse(txt_onMs.Text, out onMs) || onMs <= 0)
                {
                    MessageBox.Show("请输入正确的发射时间 (毫秒)");
                    return;
                }
                if (!int.TryParse(txt_offMs.Text, out offMs) || offMs <= 0)
                {
                    MessageBox.Show("请输入正确的间断时间 (毫秒)");
                    return;
                }

                // 初始化定时器
                if (periodicTimer == null)
                {
                    periodicTimer = new System.Windows.Forms.Timer();
                    periodicTimer.Tick += PeriodicTimer_Tick;
                }

                // 设置初始状态：先发射
                isEmitting = true;
                periodicTimer.Interval = onMs;
                periodicTimer.Start();

                // UI 更新
                isPeriodicRunning = true;
                but_periodicSync.Text = "停止间断发射";

                // 触发一次发射
                but_StartSignal.PerformClick();
            }
            else
            {
                // 停止定时器
                periodicTimer?.Stop();
                isPeriodicRunning = false;
                but_periodicSync.Text = "启动间断发射";
            }
        }

        private void PeriodicTimer_Tick(object sender, EventArgs e)
        {
            if (isEmitting)
            {
                // 切换到间断
                isEmitting = false;
                periodicTimer.Interval = offMs;
                // 间断时，不触发发射按钮
                but_StopSignal_Click(sender,e);
            }
            else
            {
                // 切换到发射
                isEmitting = true;
                periodicTimer.Interval = onMs;
                //but_StopSignal_Click(sender, e); // 模拟发射
                if(waveform ==  1)
                {
                    butSine_Click(sender, e);
                }
                else if(waveform == 2)
                {
                    butTri_Click(sender, e);
                }
                else if (waveform == 3)
                {
                    butSquare_Click(sender, e);
                }


                


            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }
        private string saveFolderPath = "";
        private bool isSavePathSelected = false;

        private void but_SaveCSV_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "请选择保存数据的文件夹";

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    saveFolderPath = fbd.SelectedPath;
                    isSavePathSelected = true;
                    but_SaveCSV.Text = saveFolderPath; // 按钮显示路径
                }
            }
        }


        private bool saveFlag = false;            // false = 不保存, true = 保存中

        private void but_saveData_Click(object sender, EventArgs e)
        {
            if (!isSaving)
            {
                isSaving = true;
                but_saveData.Text = "停止保存数据";

                cts = new CancellationTokenSource();
                Task.Run(() => SaveDataLoop(cts.Token));
                MessageBox.Show("开始保存数据...");






            }
            else
            {
                isSaving = false;
                but_saveData.Text = "开始保存数据";

                if (cts != null)
                {
                    cts.Cancel();
                }
                MessageBox.Show("已停止保存数据");
            }

        }

        /// <summary>
        /// 定时器触发的保存数据事件
        /// </summary>
        private void timer1_Tick_SaveData(object sender, EventArgs e)
        {
        }


        public void SaveChannelData(byte[] dataArray, string fileName)
        {
        
        }


      
        private System.Threading.Timer processTimer; // 后台定时器

        private void ProcessTimer_Tick(object state)
        {
            
        }

        private async Task SaveDataLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (saveQueue.TryDequeue(out byte[] data))
                {
                    try
                    {
                        await CsvHelper.SaveBytesToCsvByPointerAsync(data, "chA", saveFolderPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("保存失败: " + ex.Message);
                    }
                }
                else
                {
                    await Task.Delay(20, token); // 队列空时稍微等待，避免CPU占满
                }
            }
        }

        public class CsvHelper
        { /// <summary>
          /// 一帧数据写一行到 CSV 文件
          /// </summary>
            public static async Task SaveBytesToCsvByPointerAsync(byte[] data, string pointerName, string directory = "")
            {
                if (data == null || data.Length == 0)
                {
                    throw new ArgumentException("数据不能为空", nameof(data));
                }
                if (string.IsNullOrWhiteSpace(pointerName))
                {
                    throw new ArgumentException("指针名称不能为空", nameof(pointerName));
                }

                if (string.IsNullOrEmpty(directory))
                {
                    directory = Directory.GetCurrentDirectory();
                }
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string filePath = Path.Combine(directory, $"{pointerName}.csv");

                // 把整个 data 数组转成一行 CSV
                string csvLine = string.Join(",", Array.ConvertAll(data, b => b.ToString()));

                using (StreamWriter sw = new StreamWriter(filePath, true, Encoding.UTF8))
                {
                    await sw.WriteLineAsync(csvLine);
                }
            }
        }



        private bool isSaving = false;                       // 是否在保存数据
        private ConcurrentQueue<byte[]> saveQueue;           // 保存队列
        private CancellationTokenSource cts;                 // 取消标志
        private byte[] g_chADataArray = new byte[64 * 1024]; // 通道 A 的数据缓冲区

        private void but_Test_Click(object sender, EventArgs e)
        {
            using (Graphics g = pictureBox_chA.CreateGraphics())
            {
                var rect = pictureBox_chA.ClientRectangle;
                PaintEventArgs pea = new PaintEventArgs(g, rect);
                pictureBox_chA_Paint(pictureBox_chA, pea);
            }
        }

        private void affrim_but_Click(object sender, EventArgs e)
        {
            double numDouble = (double)numericUpDown1.Value;
            reartimeTem_textBox.Text = "当前值: " + numDouble;
        }
    }
}