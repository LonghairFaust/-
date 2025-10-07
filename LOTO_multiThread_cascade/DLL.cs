using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace OpenSource_LOTO_A02
{
    /// <summary>
    /// USBInterFace.dll
    /// </summary>
    class DLL_1
    {
        //-------------------声明动态库 USBInterFace.dll的 一些接口函数--------------------------------------------------------------------


        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "SpecifyDevIdx")]
        public static extern void SpecifyDevIdx(Int32 index); // 设置产品编号，不同型号产品编号不同

        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetMoreDeviceConcatenation")]
        public static extern void SetMoreDeviceConcatenation(byte deviceType); // 设备类型 1 - 主设备, 2 - 辅设备

        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "DeviceOpenWithID")]
        public static extern Int32 DeviceOpenWithID(int ID); // 指定设备编号 尝试打开设备
        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 DeviceOpen();

        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "DeviceClose")]
        public static extern Int32 DeviceClose(); // 关闭设备并释放资源

        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "USBCtrlTrans")]
        public static extern byte USBCtrlTrans(byte Request, UInt16 usValue, uint outBufSize); // USB传输控制命令

        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "USBCtrlTransSimple")]
        public static extern Int32 USBCtrlTransSimple(Int32 Request); // USB传输控制命令

        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetBuffer4Wr")]
        public static extern IntPtr GetBuffer4Wr(Int32 index); // 获取原始数据缓冲区首指针

        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "AiReadBulkData")]
        public static extern Int32 AiReadBulkData(Int32 SampleCount, uint EventNum, Int32 ulTimeout, IntPtr Buffer, byte Flag, uint First_PacketNum); // 批量读取原始数据块

        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetInfo")]
        public static extern void SetInfo(double dataNumPerPixar, double currentSampleRate, byte ChannelMask, Int32 m_ZrroUniInt, uint BufferOffset, uint HWbufferSize);

        [DllImport("USBInterFace.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "ResetPipe")]
        public static extern bool ResetPipe();
        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 EventCheck(Int32 ulTimeout);
    }

    /// <summary>
    /// USBInterFace2.dll
    /// </summary>
    class DLL_2
    {
        //-------------------声明动态库 USBInterFace2.dll的 一些接口函数--------------------------------------------------------------------


        [DllImport("USBInterFace2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "SpecifyDevIdx")]
        public static extern void SpecifyDevIdx(Int32 index); // 设置产品编号，不同型号产品编号不同

        [DllImport("USBInterFace2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetMoreDeviceConcatenation")]
        public static extern void SetMoreDeviceConcatenation(byte deviceType); // 设备类型 1 - 主设备, 2 - 辅设备

        [DllImport("USBInterFace2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "DeviceOpenWithID")]
        public static extern Int32 DeviceOpenWithID(int ID); // 指定设备编号 尝试打开设备

        [DllImport("USBInterFace2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "DeviceClose")]
        public static extern Int32 DeviceClose(); // 关闭设备并释放资源

        [DllImport("USBInterFace2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "USBCtrlTrans")]
        public static extern byte USBCtrlTrans(byte Request, UInt16 usValue, uint outBufSize); // USB传输控制命令

        [DllImport("USBInterFace2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "USBCtrlTransSimple")]
        public static extern Int32 USBCtrlTransSimple(Int32 Request); // USB传输控制命令

        [DllImport("USBInterFace2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetBuffer4Wr")]
        public static extern IntPtr GetBuffer4Wr(Int32 index); // 获取原始数据缓冲区首指针

        [DllImport("USBInterFace2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "AiReadBulkData")]
        public static extern Int32 AiReadBulkData(Int32 SampleCount, uint EventNum, Int32 ulTimeout, IntPtr Buffer, byte Flag, uint First_PacketNum); // 批量读取原始数据块

        [DllImport("USBInterFace2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetInfo")]
        public static extern void SetInfo(double dataNumPerPixar, double currentSampleRate, byte ChannelMask, Int32 m_ZrroUniInt, uint BufferOffset, uint HWbufferSize);

        [DllImport("USBInterFace2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "ResetPipe")]
        public static extern bool ResetPipe();
        [DllImport("USBInterFace2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 EventCheck(Int32 ulTimeout);
    }

    /// <summary>
    /// USBInterFace3.dll
    /// </summary>
    class DLL_3
    {
        //-------------------声明动态库 USBInterFace3.dll的 一些接口函数--------------------------------------------------------------------
        [DllImport("USBInterFace3.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "ResetPipe")]
        public static extern bool ResetPipe();

        [DllImport("USBInterFace3.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "SpecifyDevIdx")]
        public static extern void SpecifyDevIdx(Int32 index); // 设置产品编号，不同型号产品编号不同

        [DllImport("USBInterFace3.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetMoreDeviceConcatenation")]
        public static extern void SetMoreDeviceConcatenation(byte deviceType); // 设备类型 1 - 主设备, 2 - 辅设备

        [DllImport("USBInterFace3.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "DeviceOpenWithID")]
        public static extern Int32 DeviceOpenWithID(int ID); // 指定设备编号 尝试打开设备

        [DllImport("USBInterFace3.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "DeviceClose")]
        public static extern Int32 DeviceClose(); // 关闭设备并释放资源

        [DllImport("USBInterFace3.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "USBCtrlTrans")]
        public static extern byte USBCtrlTrans(byte Request, UInt16 usValue, uint outBufSize); // USB传输控制命令

        [DllImport("USBInterFace3.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "USBCtrlTransSimple")]
        public static extern Int32 USBCtrlTransSimple(Int32 Request); // USB传输控制命令

        [DllImport("USBInterFace3.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetBuffer4Wr")]
        public static extern IntPtr GetBuffer4Wr(Int32 index); // 获取原始数据缓冲区首指针

        [DllImport("USBInterFace3.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "AiReadBulkData")]
        public static extern Int32 AiReadBulkData(Int32 SampleCount, uint EventNum, Int32 ulTimeout, IntPtr Buffer, byte Flag, uint First_PacketNum); // 批量读取原始数据块

        [DllImport("USBInterFace3.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetInfo")]
        public static extern void SetInfo(double dataNumPerPixar, double currentSampleRate, byte ChannelMask, Int32 m_ZrroUniInt, uint BufferOffset, uint HWbufferSize);
        [DllImport("USBInterFace3.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 EventCheck(Int32 ulTimeout);
    }

    /// <summary>
    /// USBInterFace4.dll
    /// </summary>
    class DLL_4
    {
        //-------------------声明动态库 USBInterFace4.dll的 一些接口函数--------------------------------------------------------------------

        [DllImport("USBInterFace4.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "ResetPipe")]
        public static extern bool ResetPipe();
        [DllImport("USBInterFace4.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "SpecifyDevIdx")]
        public static extern void SpecifyDevIdx(Int32 index); // 设置产品编号，不同型号产品编号不同

        [DllImport("USBInterFace4.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetMoreDeviceConcatenation")]
        public static extern void SetMoreDeviceConcatenation(byte deviceType); // 设备类型 1 - 主设备, 2 - 辅设备

        [DllImport("USBInterFace4.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "DeviceOpenWithID")]
        public static extern Int32 DeviceOpenWithID(int ID); // 指定设备编号 尝试打开设备

        [DllImport("USBInterFace4.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "DeviceClose")]
        public static extern Int32 DeviceClose(); // 关闭设备并释放资源

        [DllImport("USBInterFace4.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "USBCtrlTrans")]
        public static extern byte USBCtrlTrans(byte Request, UInt16 usValue, uint outBufSize); // USB传输控制命令

        [DllImport("USBInterFace4.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "USBCtrlTransSimple")]
        public static extern Int32 USBCtrlTransSimple(Int32 Request); // USB传输控制命令

        [DllImport("USBInterFace4.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetBuffer4Wr")]
        public static extern IntPtr GetBuffer4Wr(Int32 index); // 获取原始数据缓冲区首指针

        [DllImport("USBInterFace4.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "AiReadBulkData")]
        public static extern Int32 AiReadBulkData(Int32 SampleCount, uint EventNum, Int32 ulTimeout, IntPtr Buffer, byte Flag, uint First_PacketNum); // 批量读取原始数据块

        [DllImport("USBInterFace4.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetInfo")]
        public static extern void SetInfo(double dataNumPerPixar, double currentSampleRate, byte ChannelMask, Int32 m_ZrroUniInt, uint BufferOffset, uint HWbufferSize);
        [DllImport("USBInterFace4.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 EventCheck(Int32 ulTimeout);
    }
}
