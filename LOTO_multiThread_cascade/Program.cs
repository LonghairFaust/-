using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices; //这是使用c#时,要调用动态库DLL时候要引入的  

namespace OpenSource_LOTO_A02
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
    class globleVariables
    {
        public static object g_lockIO = new object();   // 这是个资源锁，用来防止多线程中发送USB命令导致冲突，设备1
        public static object g_lockIO2 = new object();  // 这是个资源锁，用来防止多线程中发送USB命令导致冲突，设备2
        public static object g_lockIO3 = new object();  // 这是个资源锁，用来防止多线程中发送USB命令导致冲突，设备3
        public static object g_lockIO4 = new object();  // 这是个资源锁，用来防止多线程中发送USB命令导致冲突，设备4

        public static int g_OSCcnt = 1;                // 有几台示波器级联
        public static volatile int DataOffset = 100;            // 每次采集到的数据的起始位置的偏移量（每通道）
        public static volatile int DataCount = 1000;            //显示多少数据量（每通道）

        public static volatile int xCOMPENSATE = 20;//级联时，主设备和辅设备之间有细微的时间差，用这个偏移量来补偿
 
        public static IntPtr g_pBuffer = (IntPtr)0;     // 第一台设备的数据缓冲区首指针,数据是AB通道的，排列顺序为ABABAB...，每个字节一个数据
        public static IntPtr g_pBufferCD = (IntPtr)0;     // 第一台设备的数据缓冲区首指针,数据是CD通道的，排列顺序为CDCDCD...，每个字节一个数据
        public static IntPtr g_pBuffer_2nd = (IntPtr)0; // 第二台设备的数据缓冲区首指针
        public static IntPtr g_pBufferCD2 = (IntPtr)0;     // 第二台设备的数据缓冲区首指针,数据是CD通道的，排列顺序为CDCDCD...，每个字节一个数据
        public static IntPtr g_pBuffer_3rd = (IntPtr)0; // 第三台设备的数据缓冲区首指针
        public static IntPtr g_pBufferCD3 = (IntPtr)0;     // 第3台设备的数据缓冲区首指针,数据是CD通道的，排列顺序为CDCDCD...，每个字节一个数据
        public static IntPtr g_pBuffer_4th = (IntPtr)0; // 第四台设备的数据缓冲区首指针
        public static IntPtr g_pBufferCD4 = (IntPtr)0;     // 第4台设备的数据缓冲区首指针,数据是CD通道的，排列顺序为CDCDCD...，每个字节一个数据

        public static byte[] g_chADataArray = new byte[64 * 1024]; // 用来放通道A原始数据的数组
        public static byte[] g_chBDataArray = new byte[64 * 1024]; // 用来放通道B原始数据的数组
        public static byte[] g_chCDataArray = new byte[64 * 1024]; // 用来放通道C原始数据的数组
        public static byte[] g_chDDataArray = new byte[64 * 1024]; // 用来放通道D原始数据的数组

        public static byte[] g_ch5DataArray = new byte[64 * 1024]; // 用来放通道5原始数据的数组
        public static byte[] g_ch6DataArray = new byte[64 * 1024]; // 用来放通道6原始数据的数组
        public static byte[] g_ch7DataArray = new byte[64 * 1024]; // 用来放通道7原始数据的数组
        public static byte[] g_ch8DataArray = new byte[64 * 1024]; // 用来放通道8原始数据的数组
        public static byte[] g_ch9DataArray = new byte[64 * 1024]; // 用来放通道5原始数据的数组
        public static byte[] g_ch10DataArray = new byte[64 * 1024]; // 用来放通道6原始数据的数组
        public static byte[] g_ch11DataArray = new byte[64 * 1024]; // 用来放通道7原始数据的数组
        public static byte[] g_ch12DataArray = new byte[64 * 1024]; // 用来放通道8原始数据的数组
        public static byte[] g_ch13DataArray = new byte[64 * 1024]; // 用来放通道5原始数据的数组
        public static byte[] g_ch14DataArray = new byte[64 * 1024]; // 用来放通道6原始数据的数组
        public static byte[] g_ch15DataArray = new byte[64 * 1024]; // 用来放通道7原始数据的数组
        public static byte[] g_ch16DataArray = new byte[64 * 1024]; // 用来放通道8原始数据的数组

        public static byte   g_bMaxA = 0; //通道A的字节最大值
        public static byte   g_bMaxB = 0; //通道B的字节最大值
        public static byte   g_bMaxC = 0; //通道C的字节最大值
        public static byte   g_bMaxD = 0; //通道D的字节最大值
        public static byte   g_bMax5 = 0; //通道5的字节最大值
        public static byte   g_bMax6 = 0; //通道6的字节最大值
        public static byte   g_bMax7 = 0; //通道7的字节最大值
        public static byte   g_bMax8 = 0; //通道8的字节最大值
        public static byte   g_bMax9 = 0; //通道的字节最大值
        public static byte   g_bMax10 = 0; //通道的字节最大值
        public static byte   g_bMax11 = 0; //通道的字节最大值
        public static byte   g_bMax12 = 0; //通道的字节最大值
        public static byte   g_bMax13= 0; //通道的字节最大值
        public static byte   g_bMax14 = 0; //通道的字节最大值
        public static byte   g_bMax15 = 0; //通道的字节最大值
        public static byte   g_bMax16 = 0; //通道的字节最大值

        public static byte   g_bP2PA = 0; //通道A的字节峰峰值
        public static byte   g_bP2PB = 0; //通道A的字节峰峰值
        public static byte   g_bP2PC = 0; //通道C的字节峰峰值
        public static byte   g_bP2PD = 0; //通道D的字节峰峰值
        public static byte   g_bP2P5 = 0; //通道5的字节峰峰值
        public static byte   g_bP2P6 = 0; //通道6的字节峰峰值
        public static byte   g_bP2P7 = 0; //通道7的字节峰峰值
        public static byte   g_bP2P8 = 0; //通道8的字节峰峰值
        public static byte   g_bP2P9 = 0; //通道的字节峰峰值
        public static byte   g_bP2P10 = 0; //通道的字节峰峰值
        public static byte   g_bP2P11 = 0; //通道的字节峰峰值
        public static byte   g_bP2P12 = 0; //通道的字节峰峰值
        public static byte   g_bP2P13 = 0; //通道的字节峰峰值
        public static byte   g_bP2P14 = 0; //通道的字节峰峰值
        public static byte   g_bP2P15 = 0; //通道的字节峰峰值
        public static byte   g_bP2P16 = 0; //通道的字节峰峰值

        public static byte   g_bMinA = 255; //通道A的字节最小值
        public static byte   g_bMinB = 255; //通道B的字节最小值
        public static byte   g_bMinC = 255; //通道C的字节最小值
        public static byte   g_bMinD = 255; //通道D的字节最小值
        public static byte   g_bMin5 = 255; //通道5的字节最小值
        public static byte   g_bMin6 = 255; //通道6的字节最小值
        public static byte   g_bMin7 = 255; //通道7的字节最小值
        public static byte   g_bMin8 = 255; //通道8的字节最小值
        public static byte   g_bMin9 = 255; //通道9的字节最小值
        public static byte   g_bMin10= 255; //通道10的字节最小值
        public static byte   g_bMin11 = 255; //通道11的字节最小值
        public static byte   g_bMin12= 255; //通道12的字节最小值
        public static byte   g_bMin13 = 255; //通道13的字节最小值
        public static byte   g_bMin14 = 255; //通道14的字节最小值
        public static byte   g_bMin15 = 255; //通道15的字节最小值
        public static byte   g_bMin16 = 255; //通道16的字节最小值

        public static bool bCycleReadFlg = false;                  //20230726 循环采集状态标志，默认为FALSE，如果是正在进行循环采集，则为TRUE。
        public static ulong CycleReadCnt = 0xffffffff;             //20230726 循环采集的次数，默认的循环次数是一直循环，只要大于10000就是一直循环

        //设备1的AB通道校准数据-----------------------------------------------------------------------------------------
        public static double g_CurrentScale_ch0 = 1.25; //通道0当前档位放大倍数调整值
        public static double g_VbiasScale_2V_ch0 = 1.25;    // 通道0校准时候的放大倍数调整值 20200101 jiangtao.lv add.
        public static double g_VbiasScale_1V_ch0 = 1.25; //通道0校准时候的放大倍数调整值
        public static double g_VbiasScale_200mV_ch0 = 1.25;
        public static double g_VbiasScale_500mV_ch0 = 1.25; //通道0校准时候的放大倍数调整值
        public static double g_VbiasScale_100mV_ch0 = 1.25;
        public static double g_VbiasScale_50mV_ch0 = 1.25;
        public static double g_VbiasScale_20mV_ch0 = 1.25;

        public static double g_CurrentScale_ch1 = 1.25; //通道0校准时候的放大倍数调整值
        public static double g_VbiasScale_2V_ch1 = 1.25;    // 通道1校准时候的放大倍数调整值 20201110 nk
        public static double g_VbiasScale_1V_ch1 = 1.25; //校准时候的放大倍数调整值
        public static double g_VbiasScale_200mV_ch1 = 1.25;
        public static double g_VbiasScale_500mV_ch1 = 1.25; //通道1校准时候的放大倍数调整值
        public static double g_VbiasScale_100mV_ch1 = 1.25;
        public static double g_VbiasScale_50mV_ch1 = 1.25;
        public static double g_VbiasScale_20mV_ch1 = 1.25;

        public static byte g_CurrentZero0 = 128;
        public static byte g_CurrentZero1 = 128;

        public static byte g_VbiasZero02v = 128; // chA 新增 20230424
        public static byte g_VbiasZero12v = 128; // chB 新增 20230424
        public static byte g_VbiasZero01v = 128;
        public static byte g_VbiasZero11v = 128;
        public static byte g_VbiasZero0500mv = 128;
        public static byte g_VbiasZero1500mv = 128;
        public static byte g_VbiasZero0200mv = 128;
        public static byte g_VbiasZero1200mv = 128; // chB 新增 20190311
        public static byte g_VbiasZero0100mv = 128;
        public static byte g_VbiasZero1100mv = 128;
        public static byte g_VbiasZero050mv = 128;
        public static byte g_VbiasZero150mv = 128;
        public static byte g_VbiasZero020mv = 128;
        public static byte g_VbiasZero120mv = 128;

        //设备1的CD通道校准数据-----------------------------------------------------------------------------------------

        public static double g_CurrentScale_chC = 1.25; //通道0当前档位放大倍数调整值
        public static double g_CurrentScale_chD = 1.25; //通道0校准时候的放大倍数调整值
        public static byte g_CurrentZeroC = 128;
        public static byte g_CurrentZeroD = 128;

        public static double g_VbiasScale_5V_chC = 1; //20240508
        public static double g_VbiasScale_5V_chD = 1; //20240508

        public static double g_VbiasScale_2V_chC = 1.25;    // 通道0校准时候的放大倍数调整值 20200101 jiangtao.lv add.
        public static double g_VbiasScale_1V_chC = 1.25; //通道0校准时候的放大倍数调整值
        public static double g_VbiasScale_200mV_chC = 1.25;
        public static double g_VbiasScale_500mV_chC = 1.25; //通道0校准时候的放大倍数调整值
        public static double g_VbiasScale_100mV_chC = 1.25;
        public static double g_VbiasScale_50mV_chC = 1.25;
        public static double g_VbiasScale_20mV_chC = 1.25;

        public static double g_VbiasScale_2V_chD = 1.25;    // 通道1校准时候的放大倍数调整值 20201110 nk
        public static double g_VbiasScale_1V_chD = 1.25; //校准时候的放大倍数调整值
        public static double g_VbiasScale_200mV_chD = 1.25;
        public static double g_VbiasScale_500mV_chD = 1.25; //通道1校准时候的放大倍数调整值
        public static double g_VbiasScale_100mV_chD = 1.25;
        public static double g_VbiasScale_50mV_chD = 1.25;
        public static double g_VbiasScale_20mV_chD = 1.25;
       // public static byte g_VbiasZeroC = 128;
       // public static byte g_VbiasZeroD = 128;
        public static byte g_VbiasZeroC5v = 128; //20240508
        public static byte g_VbiasZeroD5v = 128; //20240508
        public static byte g_VbiasZeroC2v = 128; // chA 新增 20230424
        public static byte g_VbiasZeroD2v = 128; // chB 新增 20230424
        public static byte g_VbiasZeroC1v = 128;
        public static byte g_VbiasZeroD1v = 128;
        public static byte g_VbiasZeroC500mv = 128;
        public static byte g_VbiasZeroD500mv = 128;
        public static byte g_VbiasZeroC200mv = 128;
        public static byte g_VbiasZeroD200mv = 128; // chB 新增 20190311
        public static byte g_VbiasZeroC100mv = 128;
        public static byte g_VbiasZeroD100mv = 128;
        public static byte g_VbiasZeroC50mv = 128;
        public static byte g_VbiasZeroD50mv = 128;
        public static byte g_VbiasZeroC20mv = 128;
        public static byte g_VbiasZeroD20mv = 128;

        //设备2的校准数据-----------------------------------------------------------------------------------------
        public static double g_2ndOSC_CurrentScale_ch0 = 1.25;     // 通道0当前档位放大倍数调整值
        public static double g_2ndOSC_VbiasScale_2V_ch0 = 1.25;    // 通道0校准时候的放大倍数调整值 20200101 jiangtao.lv add.
        public static double g_2ndOSC_VbiasScale_1V_ch0 = 1.25;    // 通道0校准时候的放大倍数调整值
        public static double g_2ndOSC_VbiasScale_200mV_ch0 = 1.25;
        public static double g_2ndOSC_VbiasScale_500mV_ch0 = 1.25; // 通道0校准时候的放大倍数调整值
        public static double g_2ndOSC_VbiasScale_100mV_ch0 = 1.25;
        public static double g_2ndOSC_VbiasScale_50mV_ch0 = 1.25;
        public static double g_2ndOSC_VbiasScale_20mV_ch0 = 1.25;

        public static double g_2ndOSC_CurrentScale_ch1 = 1.25; //通道0校准时候的放大倍数调整值
        public static double g_2ndOSC_VbiasScale_2V_ch1 = 1.25;    // 通道1校准时候的放大倍数调整值 20201110 nk
        public static double g_2ndOSC_VbiasScale_1V_ch1 = 1.25; //校准时候的放大倍数调整值
        public static double g_2ndOSC_VbiasScale_200mV_ch1 = 1.25;
        public static double g_2ndOSC_VbiasScale_500mV_ch1 = 1.25; //通道1校准时候的放大倍数调整值
        public static double g_2ndOSC_VbiasScale_100mV_ch1 = 1.25;
        public static double g_2ndOSC_VbiasScale_50mV_ch1 = 1.25;
        public static double g_2ndOSC_VbiasScale_20mV_ch1 = 1.25;

        public static byte g_2ndOSC_CurrentZero0 = 128;
        public static byte g_2ndOSC_CurrentZero1 = 128;

       // public static byte g_2ndOSC_VbiasZero0 = 128;
       // public static byte g_2ndOSC_VbiasZero1 = 128;
        public static byte g_2ndOSC_VbiasZero02v = 128; // chA 新增 20230424
        public static byte g_2ndOSC_VbiasZero12v = 128; // chB 新增 20230424
        public static byte g_2ndOSC_VbiasZero01v = 128;
        public static byte g_2ndOSC_VbiasZero11v = 128;
        public static byte g_2ndOSC_VbiasZero0500mv = 128;
        public static byte g_2ndOSC_VbiasZero1500mv = 128;
        public static byte g_2ndOSC_VbiasZero0200mv = 128;
        public static byte g_2ndOSC_VbiasZero1200mv = 128; // chB 新增 20190311
        public static byte g_2ndOSC_VbiasZero0100mv = 128;
        public static byte g_2ndOSC_VbiasZero1100mv = 128;
        public static byte g_2ndOSC_VbiasZero050mv = 128;
        public static byte g_2ndOSC_VbiasZero150mv = 128;

        public static byte g_2ndOSC_VbiasZero020mv = 128;
        public static byte g_2ndOSC_VbiasZero120mv = 128;

        //设备3的校准数据-----------------------------------------------------------------------------------------
        public static double g_3rdOSC_CurrentScale_ch0 = 1.25; //通道0当前档位放大倍数调整值
        public static double g_3rdOSC_VbiasScale_2V_ch0 = 1.25;    // 通道0校准时候的放大倍数调整值 20200101 jiangtao.lv add.
        public static double g_3rdOSC_VbiasScale_1V_ch0 = 1.25; //通道0校准时候的放大倍数调整值
        public static double g_3rdOSC_VbiasScale_200mV_ch0 = 1.25;
        public static double g_3rdOSC_VbiasScale_500mV_ch0 = 1.25; //通道0校准时候的放大倍数调整值
        public static double g_3rdOSC_VbiasScale_100mV_ch0 = 1.25;
        public static double g_3rdOSC_VbiasScale_50mV_ch0 = 1.25;
        public static double g_3rdOSC_VbiasScale_20mV_ch0 = 1.25;

        public static double g_3rdOSC_CurrentScale_ch1 = 1.25; //通道0校准时候的放大倍数调整值
        public static double g_3rdOSC_VbiasScale_2V_ch1 = 1.25;    // 通道1校准时候的放大倍数调整值 20201110 nk
        public static double g_3rdOSC_VbiasScale_1V_ch1 = 1.25; //校准时候的放大倍数调整值
        public static double g_3rdOSC_VbiasScale_200mV_ch1 = 1.25;
        public static double g_3rdOSC_VbiasScale_500mV_ch1 = 1.25; //通道1校准时候的放大倍数调整值
        public static double g_3rdOSC_VbiasScale_100mV_ch1 = 1.25;
        public static double g_3rdOSC_VbiasScale_50mV_ch1 = 1.25;
        public static double g_3rdOSC_VbiasScale_20mV_ch1 = 1.25;

        //public static byte g_3rdOSC_VbiasZero0 = 128;
        //public static byte g_3rdOSC_VbiasZero1 = 128;
        public static byte g_3rdOSC_CurrentZero0 = 128;
        public static byte g_3rdOSC_CurrentZero1 = 128;

        public static byte g_3rdOSC_VbiasZero02v = 128; // chA 新增 20230424
        public static byte g_3rdOSC_VbiasZero12v = 128; // chB 新增 20230424
        public static byte g_3rdOSC_VbiasZero01v = 128;
        public static byte g_3rdOSC_VbiasZero11v = 128;
        public static byte g_3rdOSC_VbiasZero0500mv = 128;
        public static byte g_3rdOSC_VbiasZero1500mv = 128;
        public static byte g_3rdOSC_VbiasZero0200mv = 128;
        public static byte g_3rdOSC_VbiasZero1200mv = 128; // chB 新增 20190311
        public static byte g_3rdOSC_VbiasZero0100mv = 128;
        public static byte g_3rdOSC_VbiasZero1100mv = 128;
        public static byte g_3rdOSC_VbiasZero050mv = 128;
        public static byte g_3rdOSC_VbiasZero150mv = 128;
        public static byte g_3rdOSC_VbiasZero020mv = 128;
        public static byte g_3rdOSC_VbiasZero120mv = 128;


        //设备4的校准数据-----------------------------------------------------------------------------------------
        public static double g_4thOSC_CurrentScale_ch0 = 1.25; //通道0当前档位放大倍数调整值
        public static double g_4thOSC_VbiasScale_2V_ch0 = 1.25;    // 通道0校准时候的放大倍数调整值 20200101 jiangtao.lv add.
        public static double g_4thOSC_VbiasScale_1V_ch0 = 1.25; //通道0校准时候的放大倍数调整值
        public static double g_4thOSC_VbiasScale_200mV_ch0 = 1.25;
        public static double g_4thOSC_VbiasScale_500mV_ch0 = 1.25; //通道0校准时候的放大倍数调整值
        public static double g_4thOSC_VbiasScale_100mV_ch0 = 1.25;
        public static double g_4thOSC_VbiasScale_50mV_ch0 = 1.25;
        public static double g_4thOSC_VbiasScale_20mV_ch0 = 1.25;

        public static double g_4thOSC_CurrentScale_ch1 = 1.25; //通道0校准时候的放大倍数调整值
        public static double g_4thOSC_VbiasScale_2V_ch1 = 1.25;    // 通道1校准时候的放大倍数调整值 20201110 nk
        public static double g_4thOSC_VbiasScale_1V_ch1 = 1.25; //校准时候的放大倍数调整值
        public static double g_4thOSC_VbiasScale_200mV_ch1 = 1.25;
        public static double g_4thOSC_VbiasScale_500mV_ch1 = 1.25; //通道1校准时候的放大倍数调整值
        public static double g_4thOSC_VbiasScale_100mV_ch1 = 1.25;
        public static double g_4thOSC_VbiasScale_50mV_ch1 = 1.25;
        public static double g_4thOSC_VbiasScale_20mV_ch1 = 1.25;

       // public static byte g_4thOSC_VbiasZero0 = 128;
       // public static byte g_4thOSC_VbiasZero1 = 128;

        public static byte g_4thOSC_CurrentZero0 = 128;
        public static byte g_4thOSC_CurrentZero1 = 128;
        
        public static byte g_4thOSC_VbiasZero02v = 128; // chA 新增 20230424
        public static byte g_4thOSC_VbiasZero12v = 128; // chB 新增 20230424
        public static byte g_4thOSC_VbiasZero01v = 128;
        public static byte g_4thOSC_VbiasZero11v = 128;
        public static byte g_4thOSC_VbiasZero0500mv = 128;
        public static byte g_4thOSC_VbiasZero1500mv = 128;
        public static byte g_4thOSC_VbiasZero0200mv = 128;
        public static byte g_4thOSC_VbiasZero1200mv = 128; // chB 新增 20190311
        public static byte g_4thOSC_VbiasZero0100mv = 128;
        public static byte g_4thOSC_VbiasZero1100mv = 128;
        public static byte g_4thOSC_VbiasZero050mv = 128;
        public static byte g_4thOSC_VbiasZero150mv = 128;

        public static byte g_4thOSC_VbiasZero020mv = 128;
        public static byte g_4thOSC_VbiasZero120mv = 128;

        //电压档位数据
        public static double RangeV = 10;      //第1个设备的电压量程范围
        public static double RangeV_B = 10;    //第1个设备的电压量程范围
        public static double RangeV_C = 10;      //第1个设备的通道C电压量程范围
        public static double RangeV_D = 10;    //第1个设备的通道D电压量程范围

        public static double RangeV_2 = 10;    //第2个设备的电压量程范围
        public static double RangeV_2B = 10;   //第2个设备的电压量程范围
        public static double RangeV_3 = 10;    //第3个设备的电压量程范围
        public static double RangeV_3B = 10;   //第3个设备的电压量程范围
        public static double RangeV_4 = 10;    //第4个设备的电压量程范围
        public static double RangeV_4B = 10;   //第4个设备的电压量程范围
        public static double dataTransform(byte bValue, byte zeroB, double Amp, double RangeV)//将设备1的原始字节数据转换成真正的电压值
        {
            double outdata = ((int)bValue - (int)zeroB) * Amp * RangeV/255;

            return outdata ;
        }

        // 标志：设备数据是否采集完成
        public static bool g_dataReady = false;





    }
}