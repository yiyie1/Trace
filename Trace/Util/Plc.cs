using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using Trace;
using System.Windows.Threading;
using Trace.Util;

namespace Plc
{
    public class Plc
    {
        private const int OVER_TIME_CHECK_MSG_INTERVEL_MS = 1000;

        private const int OVER_TIME_MOVE_MSG_INTERVEL_MS = 1000;

        public static Int32 AjustPosX = 155;
        public static Int32 AjustPosY = -15;

        public static UInt32 DefaultAngle = 80;

        //TODO
        public static Int32 CurrentPositionX = -1;
        public static Int32 CurrentPositionY = -1;
        public static UInt32 CurrentAngle = 1000;

        public static Int32 PosXToMove = 0;
        public static Int32 PosYToMove = 0;
        public static UInt32 AngleToMove = 80;

        private static bool isToMoveX = false;
        private static bool isToMoveY = false;
        private static bool isToMoveM = false;

        public static AutoResetEvent areCheckReceiveOrTimeOver = new AutoResetEvent(false);

        public static AutoResetEvent areAbdoneReceiveOrTimeOver = new AutoResetEvent(false);

        public static UInt16 lastSentRandomNumber = 0;

        private static STATESTRUCT state;
        public static STATESTRUCT State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct CMDSTRUCT
        {
            public UInt16 SYSCMD;
            public UInt16 X_CMD;
            public UInt16 Y_CMD;
            public UInt16 M_CMD;
            public Int32 X_DEST_POS;
            public Int32 Y_DEST_POS;
            public UInt32 M_DEST_POS;
            public UInt16 CMD_SERIAL;
        }

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct STATESTRUCT
        {
            public UInt16 SYS_STAT;
            public UInt16 X_STAT;
            public UInt16 Y_STAT;
            public UInt16 M_STAT;
            public UInt16 CMD_SERIAL;
        }

        public enum PLCState
        {
            AXIS_ORIGIN,
            AXIS_BUSY,
            AXIS_ABDONE,
            AXIS_ERROR,
            AXIS_HOMEDONE,
            AXIS_OVERTIME = 100
        }

        public enum CMD
        {
            CMD_NONE,
            CMD_START,
            CMD_STOP,
            CMD_HOME,
            CMD_CHECK
        }

        public static void Home()
        {
            //如果是初始状态，尚未home
            if (state.X_STAT != (UInt16)PLCState.AXIS_ERROR
                || state.Y_STAT != (UInt16)PLCState.AXIS_ERROR
                || state.M_STAT != (UInt16)PLCState.AXIS_ERROR)
            {
                //发送home命令
                SendHomeMsg();
            }
        }

        //根据XY和角度控制PLC
        public static void ControlPlc(Int32 posX, Int32 posY, UInt32 angle)
        {
            PosXToMove = posX;
            PosYToMove = posY;
            AngleToMove = angle;

            ControlPlc();
        }

        //根据XY和角度控制PLC
        public static void ControlPlc()
        {
            isToMoveX = PosXToMove != CurrentPositionX;
            isToMoveY = PosYToMove != CurrentPositionY;
            isToMoveM = AngleToMove != CurrentAngle;

            if (!isToMoveX && !isToMoveY && !isToMoveM)
            {
                return;
            }

            CMDSTRUCT cmd = new CMDSTRUCT();

            cmd.SYSCMD = (UInt16)CMD.CMD_NONE;
            cmd.SYSCMD = Reverse.ReverseBytes(cmd.SYSCMD);

            if (isToMoveX)
            {
                cmd.X_CMD = (UInt16)CMD.CMD_START;
            }
            else
            {
                cmd.X_CMD = (int)CMD.CMD_NONE;
            }
            cmd.X_CMD = Reverse.ReverseBytes(cmd.X_CMD);

            if (isToMoveY)
            {
                cmd.Y_CMD = (int)CMD.CMD_START;
            }
            else
            {
                cmd.Y_CMD = (int)CMD.CMD_NONE;
            }
            cmd.Y_CMD = Reverse.ReverseBytes(cmd.Y_CMD);

            if (isToMoveM)
            {
                cmd.M_CMD = (int)CMD.CMD_START;
            }
            else
            {
                cmd.M_CMD = (int)CMD.CMD_NONE;
            }
            cmd.M_CMD = Reverse.ReverseBytes(cmd.M_CMD);

            cmd.X_DEST_POS = PosXToMove;
            //由于坐标原点右移145mm，所以需要将坐标X值减去145发给下位机
            cmd.X_DEST_POS = cmd.X_DEST_POS - Constants.X_MOVE_OFFSET; 
            cmd.X_DEST_POS = (Int32)Reverse.ReverseBytes((UInt32)((-1) * cmd.X_DEST_POS));

            cmd.Y_DEST_POS = PosYToMove;
            cmd.Y_DEST_POS = (Int32)Reverse.ReverseBytes((UInt32)((-1) * cmd.Y_DEST_POS));

            cmd.M_DEST_POS = AngleToMove;
            cmd.M_DEST_POS = Reverse.ReverseBytes(Constants.MAX_ANGLE - cmd.M_DEST_POS);

            cmd.CMD_SERIAL = GetRandomNumber();
            cmd.CMD_SERIAL = Trace.Util.Reverse.ReverseBytes(cmd.CMD_SERIAL);
            lastSentRandomNumber = cmd.CMD_SERIAL;

            Byte[] sendBytes = Trace.Util.BytesStructConvert.StructToBytes(cmd);

            System.Diagnostics.Trace.WriteLine("SYSCMD=" + cmd.SYSCMD.ToString("x") + ", X_CMD=" + cmd.X_CMD.ToString("x")
                + ", Y_CMD=" + cmd.Y_CMD.ToString("x") + ", M_CMD=" + cmd.M_CMD.ToString("x")
                + ", X_DEST_POS=" + PosXToMove + "(" + (PosXToMove - Constants.X_MOVE_OFFSET) + ")" + ", Y_DEST_POS=" + PosYToMove + ", M_DEST_POS=" + AngleToMove
                + ", CMD_SERIAL=" + cmd.CMD_SERIAL.ToString("x"));

            App.tcpThreadMgr.Send(sendBytes);

            if (App.isDebugWithoutPlc)
            {
                Thread timerThread = new Thread(StartMovePlcTimerThread);
                timerThread.IsBackground = true;
                timerThread.Start();
            }

            areAbdoneReceiveOrTimeOver.WaitOne();

            isToMoveX = false;
            isToMoveY = false;
            isToMoveM = false;

            CurrentPositionX = PosXToMove;
            CurrentPositionY = PosYToMove;
            CurrentAngle = AngleToMove;
        }

        //检查状态
        public static STATESTRUCT CheckState()
        {
            //发送check命令
            //开启计时器
            //如果1秒内有反馈则将状态返回
            //如果1秒内没有反馈则反馈错误

            //初始化state
            state.SYS_STAT = (UInt16)PLCState.AXIS_OVERTIME;
            state.X_STAT = (UInt16)PLCState.AXIS_OVERTIME;
            state.Y_STAT = (UInt16)PLCState.AXIS_OVERTIME;
            state.M_STAT = (UInt16)PLCState.AXIS_OVERTIME;
            state.CMD_SERIAL = lastSentRandomNumber;

            //发送check消息
            SendCheckMsg();

            Thread timerThread = new Thread(StartCheckTimerThread);
            timerThread.IsBackground = true;
            timerThread.Start();

            //暂停该线程，等待
            areCheckReceiveOrTimeOver.WaitOne();

            return state;
        }

        //开启计时器，时间到了就结束PLC的Check
        private static void StartCheckTimerThread()
        {
            Thread.Sleep(OVER_TIME_CHECK_MSG_INTERVEL_MS);
            areCheckReceiveOrTimeOver.Set();
            areCheckReceiveOrTimeOver.Reset();
        }

        //开启计时器，时间到了就结束移动PLC的命令
        private static void StartMovePlcTimerThread()
        {
            Thread.Sleep(OVER_TIME_MOVE_MSG_INTERVEL_MS);
            areAbdoneReceiveOrTimeOver.Set();
        }

        //接收消息并设置state
        public static void SetState(STATESTRUCT stateToSet)
        {
            if (lastSentRandomNumber == stateToSet.CMD_SERIAL)
            {
                state.SYS_STAT = Trace.Util.Reverse.ReverseBytes(stateToSet.SYS_STAT);
                state.X_STAT =  Trace.Util.Reverse.ReverseBytes(stateToSet.X_STAT);
                state.Y_STAT = Trace.Util.Reverse.ReverseBytes(stateToSet.Y_STAT);
                state.M_STAT = Trace.Util.Reverse.ReverseBytes(stateToSet.M_STAT);
                state.CMD_SERIAL = Trace.Util.Reverse.ReverseBytes(stateToSet.CMD_SERIAL);

                //检查是否已经完成PLC的移动
                CheckAbdone();

                areCheckReceiveOrTimeOver.Set();
                areCheckReceiveOrTimeOver.Reset();
            }
        }

        private static void CheckAbdone()
        {
            if (isToMoveX || isToMoveY || isToMoveM)
            {
                bool isXOk = true;
                if (isToMoveX)
                {
                    if (state.X_STAT == (UInt16)PLCState.AXIS_ABDONE)
                    {
                        isXOk = true;
                    }
                    else
                    {
                        isXOk = false;
                    }
                }

                bool isYOk = true;
                if (isToMoveY)
                {
                    if (state.Y_STAT == (UInt16)PLCState.AXIS_ABDONE)
                    {
                        isYOk = true;
                    }
                    else
                    {
                        isYOk = false;
                    }
                }

                bool isMOk = true;
                if (isToMoveM)
                {
                    if (state.M_STAT == (UInt16)PLCState.AXIS_ABDONE)
                    {
                        isMOk = true;
                    }
                    else
                    {
                        isMOk = false;
                    }
                }

                if (isXOk && isYOk && isMOk)
                {
                    areAbdoneReceiveOrTimeOver.Set();
                }
            }
        }

        //发送检查命令
        private static void SendCheckMsg()
        {
            CMDSTRUCT cmd = new CMDSTRUCT();

            cmd.SYSCMD = (UInt16)CMD.CMD_CHECK;
            cmd.SYSCMD = Trace.Util.Reverse.ReverseBytes(cmd.SYSCMD);
            cmd.X_CMD = (UInt16)CMD.CMD_CHECK;
            cmd.X_CMD = Trace.Util.Reverse.ReverseBytes(cmd.X_CMD);
            cmd.Y_CMD = (UInt16)CMD.CMD_CHECK;
            cmd.Y_CMD = Trace.Util.Reverse.ReverseBytes(cmd.Y_CMD);
            cmd.M_CMD = (UInt16)CMD.CMD_CHECK;
            cmd.M_CMD = Trace.Util.Reverse.ReverseBytes(cmd.M_CMD);
            cmd.X_DEST_POS = 0;
            cmd.Y_DEST_POS = 0;
            cmd.M_DEST_POS = 0;

            cmd.CMD_SERIAL = GetRandomNumber();
            cmd.CMD_SERIAL = Trace.Util.Reverse.ReverseBytes(cmd.CMD_SERIAL);
            lastSentRandomNumber = cmd.CMD_SERIAL;

            Byte[] sendBytes = Trace.Util.BytesStructConvert.StructToBytes(cmd);

            System.Diagnostics.Trace.WriteLine("SYSCMD=" + cmd.SYSCMD.ToString("x") + ", X_CMD=" + cmd.X_CMD.ToString("x")
                + ", Y_CMD=" + cmd.Y_CMD.ToString("x") + ", M_CMD=" + cmd.M_CMD.ToString("x")
                + ", X_DEST_POS=" + cmd.X_DEST_POS + ", Y_DEST_POS=" + cmd.Y_DEST_POS + ", M_DEST_POS=" + cmd.M_DEST_POS
                + ", CMD_SERIAL=" + cmd.CMD_SERIAL.ToString("x"));

            App.tcpThreadMgr.Send(sendBytes);
        }

        //发送Home命令
        private static void SendHomeMsg()
        {
            CMDSTRUCT cmd = new CMDSTRUCT();

            cmd.SYSCMD = (UInt16)CMD.CMD_HOME;
            cmd.SYSCMD = Trace.Util.Reverse.ReverseBytes(cmd.SYSCMD);
            cmd.X_CMD = (UInt16)CMD.CMD_HOME;
            cmd.X_CMD = Trace.Util.Reverse.ReverseBytes(cmd.X_CMD);
            cmd.Y_CMD = (UInt16)CMD.CMD_HOME;
            cmd.Y_CMD = Trace.Util.Reverse.ReverseBytes(cmd.Y_CMD);
            cmd.M_CMD = (UInt16)CMD.CMD_HOME;
            cmd.M_CMD = Trace.Util.Reverse.ReverseBytes(cmd.M_CMD);
            cmd.X_DEST_POS = 0;
            cmd.Y_DEST_POS = 0;
            cmd.M_DEST_POS = 0;

            cmd.CMD_SERIAL = GetRandomNumber();
            cmd.CMD_SERIAL = Trace.Util.Reverse.ReverseBytes(cmd.CMD_SERIAL);
            lastSentRandomNumber = cmd.CMD_SERIAL;

            Byte[] sendBytes = Trace.Util.BytesStructConvert.StructToBytes(cmd);

            System.Diagnostics.Trace.WriteLine("SYSCMD=" + cmd.SYSCMD.ToString("x") + ", X_CMD=" + cmd.X_CMD.ToString("x")
                + ", Y_CMD=" + cmd.Y_CMD.ToString("x") + ", M_CMD=" + cmd.M_CMD.ToString("x")
                + ", X_DEST_POS=" + cmd.X_DEST_POS + ", Y_DEST_POS=" + cmd.Y_DEST_POS + ", M_DEST_POS=" + cmd.M_DEST_POS
                + ", CMD_SERIAL=" + cmd.CMD_SERIAL.ToString("x"));

            App.tcpThreadMgr.Send(sendBytes);
        }

        //获取随机数
        private static UInt16 GetRandomNumber()
        {
            Random random = new Random();
            int iResult = random.Next(0xFFFF);

            return (UInt16)iResult;
        }
    }
}
