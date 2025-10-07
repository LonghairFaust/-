using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using System.Threading;           // for multithread
using System.Windows.Forms;       // for messageboxs


namespace OpenSource_LOTO_A02
{
    public class LOTOReadThread //建立一个线程的类，专门处理数据读取和处理 20230726
    {
        private volatile bool _shouldStop;  //本线程是否停止，如果是true的话，本线程就空转，不执行任何数据的采集和处理。

        private delegate void DataReadEvtDelegate();
        private DataReadEvtDelegate EventDelegate;

        private Form1 c;                    //主窗体的窗口变量

        public LOTOReadThread(Control c)    //这个线程类的构造函数
        {
            this.c = (Form1)c;              //创造本线程的时候，将主窗体传递进来，方便本线程去和主窗体做交互  
            _shouldStop = false;            //本线程是否停止，如果是true的话，本线程就空转，不执行任何数据的采集和处理。
            EventDelegate = new DataReadEvtDelegate(beginInvokeMethod);
        }
        public void RequestStop() //外界调用来停止线程的循环
        {
            _shouldStop = true;
        }
        //------------------------------------------------------------
         public void DoWork()
        {
            int res;
            int count = 128 * 1024;      
            Int32 ii = 0;
            uint EventNum = 1;
            int EventTimeout = 3000; 

            while (!_shouldStop)  //采集没被停止
            {

                if(globleVariables.bCycleReadFlg == false)//没有在循环采集状态中,不执行任何采集相关的操作，一直到循环采集状态才执行
                {
                    Thread.Sleep(20); 
                    continue;
                }

     
             /*    if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                 {
                       lock (globleVariables.g_lockIO2) 
                       {
                              DLL_2.ResetPipe();
                              DLL_2.ResetPipe();
                              DLL_2.ResetPipe();
                              DLL_2.USBCtrlTransSimple((Int32)0x33); //RDDONE =1 开始AD采集
                        }
                  }
                  if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                  {
                        lock (globleVariables.g_lockIO3)
                        {
                             DLL_3.ResetPipe();
                             DLL_3.ResetPipe();
                             DLL_3.ResetPipe();
                             DLL_3.USBCtrlTransSimple((Int32)0x33); //RDDONE =1 开始AD采集
                         }
                   }
                   if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                   {
                        lock (globleVariables.g_lockIO4)
                        {
                             DLL_4.ResetPipe();
                             DLL_4.ResetPipe();
                             DLL_4.ResetPipe();
                             DLL_4.USBCtrlTransSimple((Int32)0x33); //RDDONE =1 开始AD采集
                        }
                   }*/
                   lock (globleVariables.g_lockIO)
                   {
                       DLL_1.ResetPipe();    DLL_1.ResetPipe();    DLL_1.ResetPipe(); //20230726 jiangtao.lv                
                       DLL_1.USBCtrlTransSimple((Int32)0x33); //RDDONE =1 开始AD采集
                   }   
                  //---------------------等待设备采集完成-----------------------------------------------------------------------------   
                  do //查询RAMBUSY的状态 ( 主设备)
                  { 
                        lock (globleVariables.g_lockIO)   {   ii = DLL_1.USBCtrlTransSimple((Int32)0x50);        }
                        Thread.Sleep(10); //important                         
                  } while (33 != ii);
              
                  if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                  {
                      do //查询RAMBUSY的状态 ( 主设备)
                      {
                          lock (globleVariables.g_lockIO2)    {  ii = DLL_2.USBCtrlTransSimple((Int32)0x50);   }
                          Thread.Sleep(10); //important 
                      } while (33 != ii);                     
                  }
                  if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                  {
                      do //查询RAMBUSY的状态 ( 主设备)
                      {
                         lock (globleVariables.g_lockIO3)  {  ii = DLL_3.USBCtrlTransSimple((Int32)0x50);   }
                          Thread.Sleep(10); //important 

                      } while (33 != ii);                    
                  }
                  if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                  {
                      do //查询RAMBUSY的状态 ( 主设备)
                      {
                          lock (globleVariables.g_lockIO4)  {   ii = DLL_4.USBCtrlTransSimple((Int32)0x50);  }
                          Thread.Sleep(10); //important 
                      } while (33 != ii);
                  }
                  //----------------------------------------------------------------------------------------------------------------------
                  DLL_1.ResetPipe(); DLL_1.ResetPipe(); DLL_1.ResetPipe();  //20250515 清理一下USB总线上的正在传输的数据包                  
                  DLL_1.USBCtrlTrans(0x53, 0x00, 1);// 切换16位数据线切换到OSCF4的AB通道数据模式下，获取AB通道的数据 20250515                  
                  res = DLL_1.AiReadBulkData(count, EventNum, EventTimeout, globleVariables.g_pBuffer, 0, 0);  

                  // 多设备级联(辅设备)     
                  if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                  {
                      DLL_2.ResetPipe(); DLL_2.ResetPipe(); DLL_2.ResetPipe();  //20250515 清理一下USB总线上的正在传输的数据包
                      DLL_2.USBCtrlTrans(0x53, 0x00, 1);// 切换16位数据线切换到OSCF4的AB通道数据模式下，获取5,6通道的数据 20250515     
                      res = DLL_2.AiReadBulkData(count, EventNum, EventTimeout, globleVariables.g_pBuffer_2nd, 0, 0);
                  }
                  if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                  {
                      DLL_3.ResetPipe(); DLL_3.ResetPipe(); DLL_3.ResetPipe();  //20250515 清理一下USB总线上的正在传输的数据包
                      DLL_3.USBCtrlTrans(0x53, 0x00, 1);// 切换16位数据线切换到OSCF4的AB通道数据模式下，获取9,10通道的数据 20250515  
                      res = DLL_3.AiReadBulkData(count, EventNum, EventTimeout, globleVariables.g_pBuffer_3rd, 0, 0);
                  }
                  if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                  {
                      DLL_4.ResetPipe(); DLL_4.ResetPipe(); DLL_4.ResetPipe();  //20250515 清理一下USB总线上的正在传输的数据包
                      DLL_4.USBCtrlTrans(0x53, 0x00, 1);// 切换16位数据线切换到OSCF4的AB通道数据模式下，获取13,14通道的数据 20250515  
                      res = DLL_4.AiReadBulkData(count, EventNum, EventTimeout, globleVariables.g_pBuffer_4th, 0, 0);
                  }
                     DLL_1.EventCheck(EventTimeout);       //等待设备1也就是主设备的AB的数据传输完        
                  if (globleVariables.g_OSCcnt >= 2)  {  DLL_2.EventCheck(EventTimeout);  }// 等待设备2（辅设备）的数据传输完成事件
                  if (globleVariables.g_OSCcnt >= 3) { DLL_3.EventCheck(EventTimeout); }// 等待设备3（辅设备）的数据传输完成事件                 
                  if (globleVariables.g_OSCcnt >= 4) { DLL_4.EventCheck(EventTimeout); }  // 等待设备4（辅设备）的数据传输完成事件  
            
                  //到目前为止所有设备的AB通道的数据都传输完成了，下面我们再获取CD通道的数据。
                 //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                  DLL_1.ResetPipe(); DLL_1.ResetPipe(); DLL_1.ResetPipe();  //20250515 清理一下USB总线上的正在传输的数据包
                  DLL_1.USBCtrlTrans(0x53, 0x10, 1);// 切换16位数据线切换到CD模式下，获取CD通道的数据,20250515   
                  res = DLL_1.AiReadBulkData(count, EventNum, EventTimeout, globleVariables.g_pBufferCD, 0, 0); //20240820 开始获取数据，读取128K的原始数据

                  // 多设备级联(辅设备)     
                  if (globleVariables.g_OSCcnt >= 2) //级联了设备2
                  {
                      DLL_2.ResetPipe(); DLL_2.ResetPipe(); DLL_2.ResetPipe();  //20250515 清理一下USB总线上的正在传输的数据包
                      DLL_2.USBCtrlTrans(0x53, 0x10, 1);// 切换16位数据线切换到OSCF4的CD通道数据模式下，获取5,6通道的数据 20250515     
                      res = DLL_2.AiReadBulkData(count, EventNum, EventTimeout, globleVariables.g_pBufferCD2, 0, 0);
                  }
                  if (globleVariables.g_OSCcnt >= 3) //级联了设备3
                  {
                      DLL_3.ResetPipe(); DLL_3.ResetPipe(); DLL_3.ResetPipe();  //20250515 清理一下USB总线上的正在传输的数据包
                      DLL_3.USBCtrlTrans(0x53, 0x10, 1);// 切换16位数据线切换到OSCF4的CD通道数据模式下，获取9,10通道的数据 20250515  
                      res = DLL_3.AiReadBulkData(count, EventNum, EventTimeout, globleVariables.g_pBufferCD3, 0, 0);
                  }
                  if (globleVariables.g_OSCcnt >= 4) //级联了设备4
                  {
                      DLL_4.ResetPipe(); DLL_4.ResetPipe(); DLL_4.ResetPipe();  //20250515 清理一下USB总线上的正在传输的数据包
                      DLL_4.USBCtrlTrans(0x53, 0x10, 1);// 切换16位数据线切换到OSCF4的CD通道数据模式下，获取13,14通道的数据 20250515  
                      res = DLL_4.AiReadBulkData(count, EventNum, EventTimeout, globleVariables.g_pBufferCD4, 0, 0);
                  }

                  DLL_1.EventCheck(EventTimeout);   //等待设备1（主设备）CD的数据传输完 
                  if (globleVariables.g_OSCcnt >= 2) { DLL_2.EventCheck(EventTimeout); }// 等待设备2（辅设备）的数据传输完成事件
                  if (globleVariables.g_OSCcnt >= 3) { DLL_3.EventCheck(EventTimeout); }// 等待设备3（辅设备）的数据传输完成事件                 
                  if (globleVariables.g_OSCcnt >= 4) { DLL_4.EventCheck(EventTimeout); }  // 等待设备4（辅设备）的数据传输完成事件  


                  c.Invoke(EventDelegate);//通知另一个线程ABCD都传输完成了，可以使用ABCD四通道的数据了。
                  //-----------------------------------------------------------------------------------------------------------------------------
                  if (globleVariables.CycleReadCnt == 1)  //如果是单次循环,设置状态结束采集的继续执行。
                  {
                      globleVariables.CycleReadCnt = 0xffffffff;               //默认的循环次数是一直循环，只要大于10000就是一直循环
                      globleVariables.bCycleReadFlg = false;
                  }
                  Thread.Sleep(150);                 
            } // end while

        }//end do work

        public bool MessurementValueCalculation()//进行测量值计算
        {
            byte max = 0;    byte min = 255;     byte max2 = 0;  byte min2 = 255;  //---------------------------对AB通道进行计算---------------------------------   
            for (int i = 0; i < globleVariables.DataCount; i++)
            {
                if ( max < globleVariables.g_chADataArray[i] )     {   max = globleVariables.g_chADataArray[i];   }
                if (min > globleVariables.g_chADataArray[i])   {  min = globleVariables.g_chADataArray[i];   }
                if (max2 < globleVariables.g_chBDataArray[i])   {   max2 = globleVariables.g_chBDataArray[i];   }
                if (min2 > globleVariables.g_chBDataArray[i])   {   min2 = globleVariables.g_chBDataArray[i];   }
            }
            globleVariables.g_bMaxA = max;  //通道A的字节最大值
            globleVariables.g_bMaxB = max2; //通道B的字节最大值
            globleVariables.g_bMinA = min;  //通道A的字节最大值
            globleVariables.g_bMinB = min2; //通道B的字节最大值
            globleVariables.g_bP2PA = (byte)(max - min); //通道A的字节峰峰值
            globleVariables.g_bP2PB = (byte)(max2 - min2); //通道B的字节峰峰值

            byte maxC = 0; byte minC = 255; byte maxD = 0; byte minD = 255;//---------------------------对CD通道进行计算---------------------------------   
            for (int i = 0; i < globleVariables.DataCount; i++)
            {
                if (maxC < globleVariables.g_chCDataArray[i]) { maxC = globleVariables.g_chCDataArray[i]; }
                if (minC > globleVariables.g_chCDataArray[i]) { minC = globleVariables.g_chCDataArray[i]; }
                if (maxD < globleVariables.g_chDDataArray[i]) { maxD = globleVariables.g_chDDataArray[i]; }
                if (minD > globleVariables.g_chDDataArray[i]) { minD = globleVariables.g_chDDataArray[i]; }
            }
            globleVariables.g_bMaxC = maxC;  //通道C的字节最大值
            globleVariables.g_bMaxD = maxD; //通道D的字节最大值
            globleVariables.g_bMinC = minC;  //通道C的字节最大值
            globleVariables.g_bMinD = minD; //通道D的字节最大值
            globleVariables.g_bP2PC = (byte)(maxC - minC); //通道C的字节峰峰值
            globleVariables.g_bP2PD = (byte)(maxD - minD); //通道D的字节峰峰值

            // 第二个设备
            if (globleVariables.g_OSCcnt >= 2)
            {
                max = 0; min = 255; max2 = 0; min2 = 255;  //---------------------------对5,,6通道进行计算---------------------------------  
                for (int i = 0; i < globleVariables.DataCount; i++)
                {
                    if (max < globleVariables.g_ch5DataArray[i])  { max = globleVariables.g_ch5DataArray[i];  }
                    if (min > globleVariables.g_ch5DataArray[i])  { min = globleVariables.g_ch5DataArray[i];  }
                    if (max2 < globleVariables.g_ch6DataArray[i]) { max2 = globleVariables.g_ch6DataArray[i];   }
                    if (min2 > globleVariables.g_ch6DataArray[i]) {  min2 = globleVariables.g_ch6DataArray[i];  }
                }
                globleVariables.g_bMax5 = max;  //通道5的字节最大值
                globleVariables.g_bMax6 = max2; //通道6的字节最大值
                globleVariables.g_bMin5 = min;  //通道5的字节最大值
                globleVariables.g_bMin6 = min2; //通道6的字节最大值
                globleVariables.g_bP2P5 = (byte)(max - min); //通道5的字节峰峰值
                globleVariables.g_bP2P6 = (byte)(max2 - min2); //通道6的字节峰峰值

                 maxC = 0; minC = 255;  maxD = 0;   minD = 255;//---------------------------对设备2的CD通道，也就是整体的7，8通道进行计算---------------------------------   
                for (int i = 0; i < globleVariables.DataCount; i++)
                {
                    if (maxC < globleVariables.g_ch7DataArray[i]) { maxC = globleVariables.g_ch7DataArray[i]; }
                    if (minC > globleVariables.g_ch7DataArray[i]) { minC = globleVariables.g_ch7DataArray[i]; }
                    if (maxD < globleVariables.g_ch8DataArray[i]) { maxD = globleVariables.g_ch8DataArray[i]; }
                    if (minD > globleVariables.g_ch8DataArray[i]) { minD = globleVariables.g_ch8DataArray[i]; }
                }
                globleVariables.g_bMax7 = maxC;  //通道7的字节最大值
                globleVariables.g_bMax8 = maxD; //通道8的字节最大值
                globleVariables.g_bMin7 = minC;  //通道7的字节最大值
                globleVariables.g_bMin8 = minD; //通道8的字节最大值
                globleVariables.g_bP2P7 = (byte)(maxC - minC); //通道7的字节峰峰值
                globleVariables.g_bP2P8 = (byte)(maxD - minD); //通道8的字节峰峰值
            }
            // 第3个设备
            if (globleVariables.g_OSCcnt >= 3)
            {
                max = 0;   min = 255;   max2 = 0;   min2 = 255;
                for (int i = 0; i < globleVariables.DataCount; i++)
                {
                    if (max < globleVariables.g_ch9DataArray[i])  {   max = globleVariables.g_ch9DataArray[i];  }
                    if (min > globleVariables.g_ch9DataArray[i])   {  min = globleVariables.g_ch9DataArray[i];   }
                    if (max2 < globleVariables.g_ch10DataArray[i]) {  max2 = globleVariables.g_ch10DataArray[i];   }
                    if (min2 > globleVariables.g_ch10DataArray[i])  {    min2 = globleVariables.g_ch10DataArray[i];   }
                }
                globleVariables.g_bMax9 = max;  //通道A的字节最大值
                globleVariables.g_bMax10= max2; //通道B的字节最大值
                globleVariables.g_bMin9 = min;  //通道A的字节最大值
                globleVariables.g_bMin10 = min2; //通道B的字节最大值
                globleVariables.g_bP2P9 = (byte)(max - min); //通道A的字节峰峰值
                globleVariables.g_bP2P10 = (byte)(max2 - min2); //通道A的字节峰峰值
            }
            // 第4个设备
            if (globleVariables.g_OSCcnt >= 4)
            {
                max = 0;  min = 255;   max2 = 0;   min2 = 255;
                for (int i = 0; i < globleVariables.DataCount; i++)
                {
                    if (max < globleVariables.g_ch13DataArray[i]) {  max = globleVariables.g_ch13DataArray[i];  }
                    if (min > globleVariables.g_ch13DataArray[i])    {  min = globleVariables.g_ch13DataArray[i];   }
                    if (max2 < globleVariables.g_ch14DataArray[i])   {  max2 = globleVariables.g_ch14DataArray[i];   }
                    if (min2 > globleVariables.g_ch14DataArray[i])  {  min2 = globleVariables.g_ch14DataArray[i];  }
                }
                globleVariables.g_bMax13 = max;  //通道A的字节最大值
                globleVariables.g_bMax14 = max2; //通道B的字节最大值
                globleVariables.g_bMin13 = min;  //通道A的字节最大值
                globleVariables.g_bMin14 = min2; //通道B的字节最大值
                globleVariables.g_bP2P13 = (byte)(max - min); //通道A的字节峰峰值
                globleVariables.g_bP2P14 = (byte)(max2 - min2); //通道A的字节峰峰值
            }
            return true;        
        }

        private void beginInvokeMethod() //数据处理线程的回调函数主体
        {
            unsafe
            {
                    byte* pData = (byte*)globleVariables.g_pBuffer;
                    byte* pDataCD = (byte*)globleVariables.g_pBufferCD;
            
                    for (int i = 0; i < globleVariables.DataCount; i++)
                    {
                        globleVariables.g_chADataArray[i] = *(pData + (i + globleVariables.DataOffset + globleVariables.xCOMPENSATE) * 2);      // 通道A的数据在原始缓冲区里的标号是0，2，4，6，8...
                        globleVariables.g_chBDataArray[i] = *(pData + (i + globleVariables.DataOffset + globleVariables.xCOMPENSATE) * 2 + 1);  // 通道B的数据在原始缓冲区里的标号是1，3，5，7，9...
                    }
                    c.pictureBox_chA.Invalidate();
                    c.pictureBox_chB.Invalidate();               
          
                    for (int i = 0; i < globleVariables.DataCount; i++)
                    {
                        globleVariables.g_chCDataArray[i] = *(pDataCD + (i + globleVariables.DataOffset + globleVariables.xCOMPENSATE) * 2);      // 通道C的数据在原始缓冲区里的标号是0，2，4，6，8...
                        globleVariables.g_chDDataArray[i] = *(pDataCD + (i + globleVariables.DataOffset + globleVariables.xCOMPENSATE) * 2 + 1);  // 通道D的数据在原始缓冲区里的标号是1，3，5，7，9...
                    }
                    c.pictureBox_chC.Invalidate();
                    c.pictureBox_chD.Invalidate();               
            }            
            if (globleVariables.g_OSCcnt >= 2)// 第二个设备
            {
                unsafe
                {
                    byte* pData = (byte*)globleVariables.g_pBuffer_2nd; byte* pDataCD = (byte*)globleVariables.g_pBufferCD2;
                    for (int i = 0; i < globleVariables.DataCount; i++)
                    {
                        globleVariables.g_ch5DataArray[i] = *(pData + (i + globleVariables.DataOffset) * 2);      // 通道C的数据在原始缓冲区里的标号是0，2，4，6，8...
                        globleVariables.g_ch6DataArray[i] = *(pData + (i + globleVariables.DataOffset) * 2 + 1);  // 通道D的数据在原始缓冲区里的标号是1，3，5，7，9...
                    }
                    c.pictureBox_ch5.Invalidate();
                    c.pictureBox_ch6.Invalidate();               
          
                    for (int i = 0; i < globleVariables.DataCount; i++)
                    {
                        globleVariables.g_ch7DataArray[i] = *(pDataCD + (i + globleVariables.DataOffset) * 2);      // 通道C的数据在原始缓冲区里的标号是0，2，4，6，8...
                        globleVariables.g_ch8DataArray[i] = *(pDataCD + (i + globleVariables.DataOffset) * 2 + 1);  // 通道D的数据在原始缓冲区里的标号是1，3，5，7，9...
                    }
                    c.pictureBox_ch7.Invalidate();
                    c.pictureBox_ch8.Invalidate();      
                }
            }            
            if (globleVariables.g_OSCcnt >= 3)// 第三个设备
            {
                unsafe
                {
                    byte* pData = (byte*)globleVariables.g_pBuffer_3rd; byte* pDataCD = (byte*)globleVariables.g_pBufferCD3;
                    for (int i = 0; i < globleVariables.DataCount; i++)
                    {
                        globleVariables.g_ch9DataArray[i] = *(pData + (i + globleVariables.DataOffset) * 2);      // 通道5的数据在原始缓冲区里的标号是0，2，4，6，8...
                        globleVariables.g_ch10DataArray[i] = *(pData + (i + globleVariables.DataOffset) * 2 + 1);  // 通道6的数据在原始缓冲区里的标号是1，3，5，7，9...
                    }
                    c.pictureBox_ch9.Invalidate();
                    c.pictureBox_ch10.Invalidate();

                    for (int i = 0; i < globleVariables.DataCount; i++)
                    {
                        globleVariables.g_ch11DataArray[i] = *(pDataCD + (i + globleVariables.DataOffset) * 2);      // 通道C的数据在原始缓冲区里的标号是0，2，4，6，8...
                        globleVariables.g_ch12DataArray[i] = *(pDataCD + (i + globleVariables.DataOffset) * 2 + 1);  // 通道D的数据在原始缓冲区里的标号是1，3，5，7，9...
                    }
                    c.pictureBox_ch11.Invalidate();
                    c.pictureBox_ch12.Invalidate();      
                }
            }            
            if (globleVariables.g_OSCcnt >= 4)// 第四个设备
            {
                unsafe
                {
                    byte* pData = (byte*)globleVariables.g_pBuffer_4th;  byte* pDataCD = (byte*)globleVariables.g_pBufferCD4;
                    for (int i = 0; i < globleVariables.DataCount; i++)
                    {
                        globleVariables.g_ch13DataArray[i] = *(pData + (i + globleVariables.DataOffset) * 2);      // 通道5的数据在原始缓冲区里的标号是0，2，4，6，8...
                        globleVariables.g_ch14DataArray[i] = *(pData + (i + globleVariables.DataOffset) * 2 + 1);  // 通道6的数据在原始缓冲区里的标号是1，3，5，7，9...
                    }
                    c.pictureBox_ch13.Invalidate();
                    c.pictureBox_ch14.Invalidate();

                    for (int i = 0; i < globleVariables.DataCount; i++)
                    {
                        globleVariables.g_ch15DataArray[i] = *(pDataCD + (i + globleVariables.DataOffset) * 2);      // 通道C的数据在原始缓冲区里的标号是0，2，4，6，8...
                        globleVariables.g_ch16DataArray[i] = *(pDataCD + (i + globleVariables.DataOffset) * 2 + 1);  // 通道D的数据在原始缓冲区里的标号是1，3，5，7，9...
                    }
                    c.pictureBox_ch15.Invalidate();
                    c.pictureBox_ch16.Invalidate();      
                }
            }
            MessurementValueCalculation();//测量值计算20250515

            double ppA = globleVariables.dataTransform(globleVariables.g_bP2PA, 0, globleVariables.g_CurrentScale_ch0, globleVariables.RangeV);//将设备1的原始字节数据转换成真正的电压值
            double ppB = globleVariables.dataTransform(globleVariables.g_bP2PB, 0, globleVariables.g_CurrentScale_ch1, globleVariables.RangeV_B);//将设备1的原始字节数据转换成真正的电压值
            c.label_PPvalue_A.Text = "P_P: " + ppA.ToString("0.00");
            c.label_PPvalue_B.Text =   ppB.ToString("0.00");

            ppA = globleVariables.dataTransform(globleVariables.g_bP2PC, 0, globleVariables.g_CurrentScale_chC, globleVariables.RangeV_C);//将设备1的原始字节数据转换成真正的电压值
            ppB = globleVariables.dataTransform(globleVariables.g_bP2PD, 0, globleVariables.g_CurrentScale_chD, globleVariables.RangeV_D);//将设备1的原始字节数据转换成真正的电压值
            c.label_PPvalue_C.Text =   ppA.ToString("0.00");
            c.label_PPvalue_D.Text =   ppB.ToString("0.00");

            ppA = globleVariables.dataTransform(globleVariables.g_bP2P5, 0, globleVariables.g_2ndOSC_CurrentScale_ch0, globleVariables.RangeV_2);//将设备2的原始字节数据转换成真正的电压值
            ppB = globleVariables.dataTransform(globleVariables.g_bP2P6, 0, globleVariables.g_2ndOSC_CurrentScale_ch1, globleVariables.RangeV_2B);//将设备2的原始字节数据转换成真正的电压值
            c.label_PPvalue_5.Text =   ppA.ToString("0.00");
            c.label_PPvalue_6.Text =   ppB.ToString("0.00"); 

            ppA = globleVariables.dataTransform(globleVariables.g_bP2P9, 0, globleVariables.g_3rdOSC_CurrentScale_ch0, globleVariables.RangeV_3);//将设备3的原始字节数据转换成真正的电压值
            ppB = globleVariables.dataTransform(globleVariables.g_bP2P10, 0, globleVariables.g_3rdOSC_CurrentScale_ch1, globleVariables.RangeV_3B);//将设备3的原始字节数据转换成真正的电压值
            c.label_PPvalue_9.Text =  ppA.ToString("0.00");
            c.label_PPvalue_10.Text =  ppB.ToString("0.00");

            ppA = globleVariables.dataTransform(globleVariables.g_bP2P13, 0, globleVariables.g_4thOSC_CurrentScale_ch0, globleVariables.RangeV_4);//将设备4的原始字节数据转换成真正的电压值
            ppB = globleVariables.dataTransform(globleVariables.g_bP2P14, 0, globleVariables.g_4thOSC_CurrentScale_ch1, globleVariables.RangeV_4B);//将设备4的原始字节数据转换成真正的电压值
            c.label_PPvalue_13.Text =  ppA.ToString("0.00");
            c.label_PPvalue_14.Text =  ppB.ToString("0.00");
        }
    }
}
