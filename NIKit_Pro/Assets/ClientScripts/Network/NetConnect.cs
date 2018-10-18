using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using LPC;

/// <summary>
/// 代理声明：连接上、连接断开、读取数据的回调
/// </summary>
public delegate void ConnectCallback(bool success);
public delegate void DisconnectCallback(bool success);
public delegate void PacketReadCallback(int cmdNo,byte[] bytes);

/// <summary>
/// 网络连接通信类
/// </summary>
public class NetConnect
{
    // 网络数据的响应者
    NetResponsor _responsor = null;

    // 1 秒算是正常
    private int echoInterval = 1 * 1000;

    // 等待消息回执
    private bool waitCmdVerify = false;

    // 等待消息确认的超时时间
    private int cmdVerifyTimeout = 0;

    // 本次延时是否已经通知过上层了
    private bool noticedeNetworkBad = false;

    // 是否需要对当前的cmd忽略消息确认机制 
    public delegate bool CheckGreenPassageDelegate(int cmdNo);
    public CheckGreenPassageDelegate checkGreenPassage = null;

    /// <summary>
    /// 统计信息
    /// </summary>
    public class StatInfo
    {
        public int TotalSendBytes = 0;
        public int TotalReadBytes = 0;
        public int DisAsyncCount = 0;
        public int BadPktCount = 0;
    }

    // 关闭消息确认机制标志
    public bool CloseMsgVerify { get; set; }

    /// <summary>
    /// 网络数据的响应者
    /// </summary>
    public NetResponsor Responser
    {
        get { return _responsor; }
        set { _responsor = value; }
    }

    // 处理连接结果
    private void NotifyConnectResult(bool success)
    {
        if (_responsor != null)
            _responsor.OnConnectSuccess();
    }

    // 处理断开连接结果
    private void NotifyDisconnectResult(bool success)
    {
        if (_responsor != null)
            _responsor.OnDisconnect();
    }

    // 处理一个完整数据包
    private void NotifyPacketResult(int cmdNo, byte[] message)
    {
        // 收到确认，包已经被服务端收到了
        // 如果是MSG_CMD_VERIFY和CloseMsgVerify才需要处理
        if (cmdNo == MsgMgr.MsgID("MSG_CMD_VERIFY") && ! CloseMsgVerify)
        {
            lock (mWaitCmdNo)
            {
                // 不处理出错的情况，这里只是记录下。
                // 出错的代码将在别的地方统一处理。
                if (mWaitCmdNo.Count < 1)
                    return;

                // 取服务端回执的 cmd 编号
                string cmdName = PktAnalyser.Unpack(cmdNo, message).AsMapping["cmd"].AsString.ToUpper();
                if (mWaitCmdNo[0] != MsgMgr.MsgID(cmdName))
                    return;

                // 正常情况
                mWaitCmdNo.RemoveAt(0);
            }

            // 收到回执，解除等待
            waitCmdVerify = false;

            // 一条消息顺利发向服务端，移除它
            lock (mPendingSendPkt)
            {
                if (mPendingSendPkt.Count > 0)
                    mPendingSendPkt.RemoveAt(0);
            }
        }

        // _responsor对象不存在
        if (_responsor == null)
            return;

        // 消息处理
        _responsor.OnRead(cmdNo, message);

        // 如果当前还处于延迟中
        if (IsDelay())
            return;

        // 解除延时, 直接向上层通知 。
        _responsor.OnUnobstructed();
    }

    private void AsyncConnectCallback(IAsyncResult ar)
    {
        Socket s = (Socket)ar.AsyncState;

        try
        {
            // 清空数据
            lock (mPendingSendPkt)
            {
                mPendingSendPkt.Clear();
            }

            s.EndConnect(ar);
        }
        catch (SocketException e)
        {
            Console.WriteLine(e.ToString());
        }

        if (!mIsConnecting)
        {
            this.Disconnect();
            return;
        }

        bool success = s.Connected;
        mIsConnected = success;
        NotifyConnectResult(success);

        if (!success)
        {
            this.Disconnect();
        }
    }

    /// <summary>
    /// 异步连接
    /// </summary>
    /// <param name="ip">目标IP</param>
    /// <param name="port">目标端口</param>
    public void NonblockConnect(string ip, int port)
    {
        IPAddress addr = null;
        if (!IPAddress.TryParse(ip, out addr))
        {
            var addrList = System.Net.Dns.GetHostEntry(ip).AddressList;
            
            for (int i = 0; i < addrList.Length; ++i)
            {
                addr = addrList[i];

                if (addr.AddressFamily == AddressFamily.InterNetworkV6)
                    break;
            }
        }

        if (mSocket == null)
        {
            mSocket = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.IP);
            mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 512 * 1024);
            mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 512 * 1024);
        }

        if (mSocket.Connected)
        {
            // 已经处于连接状态了
            return;
        }

        try
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(addr, port);
            mSocket.Blocking = false;

            // 标记正处于连接中
            mIsConnecting = true;

            // 开始连接
            mSocket.BeginConnect(remoteEndPoint, new AsyncCallback(AsyncConnectCallback), mSocket);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    /// <summary>
    /// 同步连接
    /// </summary>
    /// <param name="ip">目标IP</param>
    /// <param name="port">目标端口</param>
    public void Connect(string ip, int port)
    {
        if (mSocket == null)
        {
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 512 * 1024);
            mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 512 * 1024);
        }

        if (mSocket.Connected)
        {
            return;
        }

        // 标记正处于连接中
        mIsConnecting = true;
        mSocket.Blocking = false;
        try
        {
            mSocket.Connect(ip, port);
        }
        catch (SocketException e)
        {
            Console.WriteLine(e.ToString());
        }

        // 清空数据
        lock (mPendingSendPkt)
        {
            mPendingSendPkt.Clear();
        }

        bool success = mSocket.Connected;
        mIsConnected = success;
        NotifyConnectResult(success);

        if (!success)
        {
            this.Disconnect();
        }
    }

    /// <summary>
    /// 断开当前和服务器的连接。本函数可以被重复调用
    /// </summary>
    public void Disconnect()
    {
        // 清空数据
        lock (mPendingSendPkt)
        {
            mPendingSendPkt.Clear();
        }

        lock (mLoginSendPkt)
        {
            mLoginSendPkt.Clear();
        }

        lock (mWaitCmdNo)
        {
            mWaitCmdNo.Clear();
        }

        // 解除等待状态
        waitCmdVerify = false;

        // 断开连接，重置状态
        if (mSocket != null)
        {
            using (mSocket)
            {
                try
                {
                    mSocket.Shutdown(SocketShutdown.Both);
                    // mSocket.Disconnect(false);
                }
                catch
                {
                    // 这里什么都不干，保证异常发生后下面的代码依然被执行
                }

                // 触发断开事件
                // NotifyDisconnectResult(!mSocket.Connected);
                try
                {
                    mSocket.Close();
                }
                catch
                {
                }
                mSocket = null;
            }
        }
        mIsConnected = false;
        mIsConnecting = false;
    }

    /// <summary>
    /// 安全发送一个完整的数据包，对于上层看来，一定能够发送出去
    /// </summary>
    /// <param name="cmdNo">消息号</param>
    /// <param name="cmdBody">需要发送的原始数据</param>
    public void SendPkt(UInt16 cmdNo, byte[] cmdBody)
    {
        // 构造一个完整的数据包
        byte[] fullPkt = BuildFullSendPkt(cmdNo, cmdBody);

        // 如果是不需要阻塞的消息直接发送
        if (IsGreenPassageCmd(cmdNo))
        {
            lock (mLoginSendPkt)
            {
                mLoginSendPkt.Add(fullPkt);
            }
            return;
        }

        // 阻塞发送消息
        lock (mPendingSendPkt)
        {
            // 如果不需要确认等待消息
            if (! CloseMsgVerify)
            {
                lock (mWaitCmdNo)
                    mWaitCmdNo.Add(cmdNo);
            }

            // 添加到mPendingSendPkt列表中
            mPendingSendPkt.Add(fullPkt);
        }

        // 判断是否出现延迟了
        if (IsDelay())
        {
            _responsor.OnDelay();
            noticedeNetworkBad = true;
        }
    }

    /// <summary>
    /// 发送一个数据包
    /// </summary>
    public void SendData(byte[] data)
    {
        mSocket.Send(data);
    }

    /// <summary>
    /// 帧回调函数，每帧都要检查是否有Pending的数据需要发送；是否有数据需要读取。
    /// </summary>
    /// <param name="elapse"></param>
    public void Poll(float elapse)
    {
        if (mSocket == null)
            return;

        if (!mSocket.Connected)
        {
            // 抛出延迟事件
            if (waitCmdVerify ||
                VerifyCmdMgr.IsVerifyCmd())
                _responsor.OnDelay();

            // 断开链接处理
            this.Disconnect();

            return;
        }

        if (mSocket.Poll(0, SelectMode.SelectRead) &&
            (mSocket.Available < 1))
        {
            // 抛出延迟事件
            if (waitCmdVerify ||
                VerifyCmdMgr.IsVerifyCmd())
                _responsor.OnDelay();

            // 断开链接处理
            this.Disconnect();

            return;
        }

        // 如果有 Pending 住没有发送的数据，我们尝试发送一下
        TryToSendPendingPkt();

        // 尝试读取数据包
        TryToReadPkt();

        // 判断是否出现延迟了
        if (IsDelay() && !noticedeNetworkBad)
        {
            _responsor.OnDelay();
            noticedeNetworkBad = true;
        }
    }

    /// <summary>
    /// 判断是否出现网络延迟.
    /// </summary>
    /// <returns><c>true</c> if this instance is delay; otherwise, <c>false</c>.</returns>
    private bool IsDelay()
    {
        // 如果已经关闭了消息回执确认
        if (CloseMsgVerify)
            return false;

        // 是否开启消息等到确认
        if (waitCmdVerify)
        {
            if (TimeMgr.RealTick > cmdVerifyTimeout)
                return true;
        }

        // 判断等待消息是否延迟
        // 等待消息延迟也认为是网络延迟
        if (VerifyCmdMgr.IsDelay())
            return true;

        // 没有延迟
        return false;
    }

    /// <summary>
    /// 是否无视消息确认机制.
    /// </summary>
    private bool IsGreenPassageCmd(int cmdNo)
    {
        // 这一票是和登录相关的。外加一个心跳，加一个提交error的
        // 0xff02 是心跳
        // 0x0001~0x0080 是登录，GM 相关的。
        if ((cmdNo == MsgMgr.MsgID("CMD_INTERNAL_AUTH") ||
            cmdNo == MsgMgr.MsgID("CMD_L_GET_SERVER_LIST") ||
            cmdNo == MsgMgr.MsgID("CMD_ERROR_LOG") ||
            cmdNo == 0xff02 ||
            (cmdNo >= 0x0001 && cmdNo <= 0x0080)))
            return true;

        // 检查GreenPassage
        if (checkGreenPassage != null)
        {
            try 
            {
                return checkGreenPassage(cmdNo);
            }
            catch (Exception e)
            {
                LogMgr.Exception(e);
            }
        }

        return false;
    }

    #region 属性块

    /// <summary>
    /// 属性：判断当前是否和服务器处于连接状态
    /// </summary>
    public bool IsConnected { get { return mIsConnected; } }

    /// <summary>
    /// 属性：判断当前是否正处于尝试连接状态中
    /// </summary>
    public bool IsConnecting { get { return mIsConnecting; } }

    /// <summary>
    /// 属性：当前Pending住还没有发出去的字节数
    /// </summary>
    public int PendingSendBytes
    {
        get
        {
            int sum = 0;
            foreach (byte[] data in mPendingSendPkt)
                sum += data.Length;
            return sum;
        }
    }

    /// <summary>
    /// 数据：获取统计信息
    /// </summary>
    public StatInfo NetStatInfo { get { return mStatInfo; } }

    #endregion

    #region 私有函数，实现细节

    /// <summary>
    /// 包头信息
    /// </summary>
    private class PktHeader
    {
        public byte[] Code = new byte[2];
        public UInt16 CheckSum = 0;
        public UInt32 TickCount = 0;
        public UInt32 MsgLen = 0;
        // == sizeof(cmdNo) + cmdBody.Length
        public UInt16 CmdNo = 0;
        public const int HEADER_SIZE = 16;
        public const int MAX_BODY_SIZE = 0x7FFFF;
        public byte[] Align = new byte[2];

        /// <summary>
        /// 获取字节数组
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            // 申请一个头部
            byte[] header = new byte[HEADER_SIZE];

            // 填充数据 Code[2]
            header[0] = Code[0];
            header[1] = Code[1];

            // 填充checksum (网络序)
            UInt16 _checksum = (UInt16)System.Net.IPAddress.HostToNetworkOrder((short)CheckSum);
            byte[] bitChecksum = BitConverter.GetBytes(_checksum);
            System.Diagnostics.Debug.Assert(bitChecksum.Length == 2);
            header[2] = bitChecksum[0];
            header[3] = bitChecksum[1];

            // 填充tickcount (网络序)
            uint _tickcount = (uint)System.Net.IPAddress.HostToNetworkOrder((int)TickCount);
            byte[] bitTickcount = BitConverter.GetBytes(_tickcount);
            System.Diagnostics.Debug.Assert(bitTickcount.Length == 4);
            header[4] = bitTickcount[0];
            header[5] = bitTickcount[1];
            header[6] = bitTickcount[2];
            header[7] = bitTickcount[3];

            // 填充消息长度 (网络序)
            uint _msgLen = (uint)System.Net.IPAddress.HostToNetworkOrder((int)MsgLen);
            byte[] bitMsgLen = BitConverter.GetBytes(_msgLen);
            System.Diagnostics.Debug.Assert(bitMsgLen.Length == 4);
            header[8] = bitMsgLen[0];
            header[9] = bitMsgLen[1];
            header[10] = bitMsgLen[2];
            header[11] = bitMsgLen[3];

            header[12] = Align[0];
            header[13] = Align[1];

            // 填充消息编号（网络序）
            UInt16 _cmdNo = (UInt16)System.Net.IPAddress.HostToNetworkOrder((short)CmdNo);
            byte[] bitCmdNo = BitConverter.GetBytes(_cmdNo);
            System.Diagnostics.Debug.Assert(bitCmdNo.Length == 2);
            header[14] = bitCmdNo[0];
            header[15] = bitCmdNo[1];

            // 返回最后的数据包
            return header;
        }

        /// <summary>
        /// 从字节数据获取信息
        /// </summary>
        /// <param name="header"></param>
        public void FromBytes(byte[] header)
        {
            // 大小一定要匹配
            if (header.Length != HEADER_SIZE)
                throw new Exception("Header size not match!");

            // 填充 Code[2]
            Code[0] = header[0];
            Code[1] = header[1];

            // 填充checksum
            CheckSum = (UInt16)System.Net.IPAddress.NetworkToHostOrder((short)BitConverter.ToInt16(header, 2));

            // 填充tickcount
            TickCount = (uint)System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt32(header, 4));

            // 填充消息长度
            MsgLen = (uint)System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt32(header, 8));

            Align[0] = header[12];
            Align[1] = header[13];

            // 填充消息编号
            CmdNo = (UInt16)System.Net.IPAddress.NetworkToHostOrder((short)BitConverter.ToInt16(header, 14));
        }
    }

    /// <summary>
    /// 构造一个完整的发送数据包（含包头信息等)
    /// </summary>
    /// <param name="cmdNo">消息编号</param>
    /// <param name="cmdBody">消息体</param>
    /// <returns>带包头的完整数据包</returns>
    private byte[] BuildFullSendPkt(UInt16 cmdNo, byte[] cmdBody)
    {
        // 消息体不能太大
        if (2 + cmdBody.Length >= 0xFFFF)
            throw new Exception("Package body is to large! >= 0xFFFF");

        // 计算checksum
        UInt16 checksum = 0;
        byte[] bitarray = BitConverter.GetBytes(cmdNo);
        System.Diagnostics.Debug.Assert(bitarray.Length == 2);
        for (int i = 0; i < bitarray.Length; i++)
            checksum += (UInt16)(((checksum & 0x7F) + 1) * bitarray[i]);
        for (int i = 0; i < cmdBody.Length; i++)
            checksum += (UInt16)(((checksum & 0x7F) + 1) * cmdBody[i]);

        // 申请一个包头
        PktHeader pktHeader = new PktHeader();
        pktHeader.Code[0] = (byte)'M';
        pktHeader.Code[1] = (byte)'Z';
        pktHeader.CheckSum = checksum;
        pktHeader.TickCount = 0;
        pktHeader.MsgLen = (UInt32)(2 + cmdBody.Length);
        pktHeader.CmdNo = cmdNo;
        pktHeader.Align[0] = (byte)'A';
        pktHeader.Align[1] = (byte)'L';

        // 连接头部和消息体
        return NetUtil.ByteArrayConcat(pktHeader.GetBytes(), cmdBody);
    }

    /// <summary>
    /// 尝试发送 Pending 的数据包，直到不能再发送为止
    /// </summary>
    private void TryToSendPendingPkt()
    {
        // 正在等待确认中，不继续发送
        if (waitCmdVerify)
            return;

        // 登录相关的，优先级高。目前心跳也是在这里面
        lock (mLoginSendPkt)
        {
            while (mLoginSendPkt.Count > 0)
            {
                // 尝试发送
                byte[] pkt = mLoginSendPkt[0];
                int sentBytes = mSocket.Send(pkt);
                // 更新统计信息
                mStatInfo.TotalSendBytes += sentBytes;

                if (sentBytes < pkt.Length)
                {
                    // 发送不出去了，更新一下还剩余的数据，然后退出循环发送过程
                    mLoginSendPkt[0] = NetUtil.ByteArraySkip(pkt, sentBytes);
                    return;
                }
                else
                {
                    mLoginSendPkt.RemoveAt(0);
                    if (mLoginSendPkt.Count < 1)
                        return;
                }
            }
        }

        //
        lock (mPendingSendPkt)
        {
            while (mPendingSendPkt.Count > 0)
            {
                // 尝试发送
                byte[] pkt = mPendingSendPkt[0];

                int sentBytes = mSocket.Send(pkt);

                // 更新统计信息
                mStatInfo.TotalSendBytes += sentBytes;

                if (sentBytes < pkt.Length)
                {
                    // 发送不出去了，更新一下还剩余的数据，然后退出循环发送过程
                    mPendingSendPkt[0] = NetUtil.ByteArraySkip(pkt, sentBytes);
                    return;
                }

                // 如果关闭了消息确认功能
                if (CloseMsgVerify)
                {
                    lock (mPendingSendPkt)
                    {
                        if (mPendingSendPkt.Count > 0)
                            mPendingSendPkt.RemoveAt(0);
                    }
                }
                else
                {
                    // 已经成功的将一个包 copy 到缓冲区了，在收到确认前不再发送别的包。
                    waitCmdVerify = true;

                    // 设置下确认的超时时间
                    cmdVerifyTimeout = TimeMgr.RealTick + echoInterval;

                    // 重置下已经通知延时的标记
                    noticedeNetworkBad = false;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 读取状态机
    /// </summary>
    private enum ReadState
    {
        WAIT_SYNC1,
        WAIT_SYNC2,
        WAIT_HEADER,
        WAIT_BODY,
    }

    /// <summary>
    /// 尝试读取数据，并匹配成包的形式
    /// </summary>
    private void TryToReadPkt()
    {
        int count = 0;
        while (true)
        {
            // 每次最多只读取一定数量的消息
#if UNITY_ANDROID || UNITY_IPHONE
            if (count >= 2)
#else
            if (count >= 10)
#endif
                return;

            // 如果处于等待syn1状态
            if (mReadState == ReadState.WAIT_SYNC1)
            {
                if (mSocket == null || mSocket.Available < 1)
                    return;

                // 读取一个字节的数据
                int realRead = mSocket.Receive(mMsgHeaderBuffer, 0, 1, SocketFlags.None);
                System.Diagnostics.Debug.Assert(realRead == 1);
                mStatInfo.TotalReadBytes += realRead;

                if (mMsgHeaderBuffer[0] == (byte)'M')
                {
                    // 是我们期望的，状态切换到wait_syn2
                    mReadState = ReadState.WAIT_SYNC2;
                }
                else
                {
                    // 失步了，还是停留在这个状态
                    mReadState = ReadState.WAIT_SYNC1;
                    mStatInfo.DisAsyncCount++;
                }
            }
            // 如果处于等待syn2状态
            else if (mReadState == ReadState.WAIT_SYNC2)
            {
                if (mSocket.Available < 1)
                    return;

                // 读取一个字节的数据
                int realRead = mSocket.Receive(mMsgHeaderBuffer, 1, 1, SocketFlags.None);
                System.Diagnostics.Debug.Assert(realRead == 1);
                mStatInfo.TotalReadBytes += realRead;

                if (mMsgHeaderBuffer[1] == (byte)'Z')
                {
                    // 是我们期望的，状态切换到wait_header
                    mReadState = ReadState.WAIT_HEADER;
                }
                else
                {
                    // 失步了，回到syn1状态
                    mReadState = ReadState.WAIT_SYNC1;
                    mStatInfo.DisAsyncCount++;
                }
            }
            // 如果处于等待头部信息状态
            else if (mReadState == ReadState.WAIT_HEADER)
            {
                if (mSocket.Available < PktHeader.HEADER_SIZE - 2)
                    return;

                // 读取头部大小字节的数据
                int realRead = mSocket.Receive(mMsgHeaderBuffer, 2, PktHeader.HEADER_SIZE - 2, SocketFlags.None);
                System.Diagnostics.Debug.Assert(realRead == PktHeader.HEADER_SIZE - 2);
                mStatInfo.TotalReadBytes += realRead;

                // 切换到wait_body状态
                mReadState = ReadState.WAIT_BODY;

                // 将读取回来的数据转成头部结构
                mWaitPktHeader.FromBytes(mMsgHeaderBuffer);

                // 设置等待读取消息体信息
                mMsgWaitBodyBytes = (int)(mWaitPktHeader.MsgLen - 2);
                mMsgBodyData = null;

            }
            // 如果处于等待消息体状态
            else if (mReadState == ReadState.WAIT_BODY)
            {
                if (mSocket.Available <= 0 && mMsgWaitBodyBytes > 0)
                    // 没有数据可以读
                    return;

                int realRead = 0;
                if (mMsgWaitBodyBytes > 0)
                    // 读取我们希望等待的消息体
                    realRead = mSocket.Receive(mMsgBodyBuffer, mMsgWaitBodyBytes, SocketFlags.None);

                mStatInfo.TotalReadBytes += realRead;
                if (realRead == mMsgWaitBodyBytes)
                {
                    // 已经完整读取
                    if (mMsgBodyData == null)
                        mMsgBodyData = NetUtil.ByteArrayTake(mMsgBodyBuffer, realRead);
                    else
                        mMsgBodyData = NetUtil.ByteArrayConcat(mMsgBodyData, NetUtil.ByteArrayTake(mMsgBodyBuffer, realRead));

                    // 切换到初始状态，处理下一个包
                    mReadState = ReadState.WAIT_SYNC1;

                    count++;
                    NotifyPacketResult(mWaitPktHeader.CmdNo, mMsgBodyData);
                }
                else
                {
                    // 没有读取到完整希望的数据
                    if (mMsgBodyData == null)
                        mMsgBodyData = NetUtil.ByteArrayTake(mMsgBodyBuffer, realRead);
                    else
                        mMsgBodyData = NetUtil.ByteArrayConcat(mMsgBodyData, NetUtil.ByteArrayTake(mMsgBodyBuffer, realRead));

                    // 更新需要等待的数据
                    mMsgWaitBodyBytes -= realRead;
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// 当前是否处于连接状态
    /// </summary>
    private bool mIsConnected = false;
    /// <summary>
    /// 当前是否处于尝试连接中
    /// </summary>
    private bool mIsConnecting = false;
    /// <summary>
    /// 统计信息
    /// </summary>
    private StatInfo mStatInfo = new StatInfo();

    #region 发送包相关逻辑的数据

    /// <summary>
    /// 当前pending住没有发送出去的包列表
    /// </summary>
    private List<byte[]> mPendingSendPkt = new List<byte[]>();

    /// <summary>
    /// 等待服务端回执的 cmd no.
    /// </summary>
    private List<int> mWaitCmdNo = new List<int>();

    /// <summary>
    /// 登录相关的一些操作，不处理回执.
    /// </summary>
    private List<byte[]> mLoginSendPkt = new List<byte[]>();

    #endregion

    #region 读取包相关逻辑的数据

    /// <summary>
    /// 读取状态
    /// </summary>
    private ReadState mReadState = ReadState.WAIT_SYNC1;

    /// <summary>
    /// 消息头部读取缓冲区
    /// </summary>
    private byte[] mMsgHeaderBuffer = new byte[PktHeader.HEADER_SIZE];

    /// <summary>
    /// 消息体读取缓冲区
    /// </summary>
    private byte[] mMsgBodyBuffer = new byte[PktHeader.MAX_BODY_SIZE];

    /// <summary>
    /// 读取消息体
    /// </summary>
    private byte[] mMsgBodyData = null;

    /// <summary>
    /// 等待的消息体还有多少个字节
    /// </summary>
    private int mMsgWaitBodyBytes = 0;

    /// <summary>
    /// 当前等待的包的包头
    /// </summary>
    private PktHeader mWaitPktHeader = new PktHeader();

    #endregion

    /// <summary>
    /// 用于通信的socket对象
    /// </summary>
    private Socket mSocket = null;
}
