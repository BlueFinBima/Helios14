//  Copyright 2014 Craig Courtney
//  Copyright 2020 Helios Contributors
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using GadrocsWorkshop.Helios.Interfaces.Capabilities.ProfileAwareInterface;

namespace GadrocsWorkshop.Helios.UDPInterface
{
    using GadrocsWorkshop.Helios;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Timers;

    public class BaseUDPInterface : HeliosInterface
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // const during lifetime, no access control required
        private readonly AsyncCallback _socketDataCallback;
        private static readonly System.Text.Encoding _iso_8859_1 = System.Text.Encoding.GetEncoding("iso-8859-1");  // This is the locale of the lua exports program

        // events don't need access control
        // event to notify potentially other threads that the client connection has changed
        public event EventHandler<ClientChange> ClientChanged;

        /// <summary>
        /// accessed only by main thread
        /// </summary>
        private class MainThreadAccess
        {
            static private int _id = System.Threading.Thread.CurrentThread.ManagedThreadId;

            private NetworkFunctionCollection _functions = new NetworkFunctionCollection();
            private Dictionary<string, NetworkFunction> _functionsById = new Dictionary<string, NetworkFunction>();

            private EndPoint _client = null;
            private string _clientID = ClientChange.NO_CLIENT;

            private HeliosTrigger _connectedTrigger;
            private HeliosTrigger _disconnectedTrigger;
            private HeliosTrigger _profileLoadedTrigger;

            private Timer _startuptimer;

            public NetworkFunctionCollection Functions
            {
                get
                {
                    Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == _id);
                    return _functions;
                }
            }

            public Dictionary<string, NetworkFunction> FunctionsById
            {
                get
                {
                    Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == _id);
                    return _functionsById;
                }
            }

            public EndPoint Client
            {
                get
                {
                    Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == _id);
                    return _client;
                }
                set
                {
                    Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == _id);
                    _client = value;
                }
            }

            public string ClientID
            {
                get
                {
                    Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == _id);
                    return _clientID;
                }
                set
                {
                    Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == _id);
                    _clientID = value;
                }
            }

            public HeliosTrigger ConnectedTrigger
            {
                get
                {
                    Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == _id);
                    return _connectedTrigger;
                }
                set
                {
                    Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == _id);
                    _connectedTrigger = value;
                }
            }

            public HeliosTrigger DisconnectedTrigger
            {
                get
                {
                    Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == _id);
                    return _disconnectedTrigger;
                }
                set
                {
                    Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == _id);
                    _disconnectedTrigger = value;
                }
            }

            public HeliosTrigger ProfileLoadedTrigger
            {
                get
                {
                    Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == _id);
                    return _profileLoadedTrigger;
                }
                set
                {
                    Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == _id);
                    _profileLoadedTrigger = value;
                }
            }

            public Timer StartupTimer
            {
                get
                {
                    Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == _id);
                    return _startuptimer;
                }
                set
                {
                    Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == _id);
                    _startuptimer = value;
                }
            }
        }

        /// <summary>
        /// accessed by multiple threads (main and socket pool, or multiple socket pool)
        /// </summary>
        private class SharedAccess
        {
            // contention between main and socket threads due to read/write and error recovery open/close
            private Socket _socket = null;

            // pool of receive contexts
            private Queue<ReceiveContext> _receiveContexts = new Queue<ReceiveContext>();
            private const int _cachedReceiveContexts = 16;

            private int _port = 9089;

            // lock on all the other fields, public access ok so we can lock larger
            // sections of code and rely on re-entrant locking to avoid deadlock
            public object Lock { get; } = new object();

            public bool Started
            {
                get
                {
                    lock (Lock)
                    {
                        return _socket != null;
                    }
                }
            }

            public Socket ServerSocket
            {
                get
                {
                    lock (Lock)
                    {
                        return _socket;
                    }
                }
                set
                {
                    lock (Lock)
                    {
                        _socket = value;
                    }
                }
            }

            public int Port
            {
                get
                {
                    lock (Lock)
                    {
                        return _port;
                    }
                }
                set
                {
                    lock (Lock)
                    {
                        _port = value;
                    }
                }
            }

            /// <summary>
            /// returns clean ReceiveContext or null
            /// </summary>
            /// <returns></returns>
            public ReceiveContext FetchReceiveContext()
            {
                lock(Lock)
                {
                    if (_receiveContexts.Count > 0)
                    {
                        ReceiveContext context = _receiveContexts.Dequeue();
                        context.socket = _socket;
                        return context;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            /// <summary>
            /// adds a receive context back to the pool
            /// </summary>
            public void ReturnReceiveContext(ReceiveContext context)
            {
                context.Clear();
                lock(Lock)
                {
                    if (_receiveContexts.Count < _cachedReceiveContexts)
                    {
                        _receiveContexts.Enqueue(context);
                    }
                }
            }
        }

        private class SendContext
        {
            // buffer for datagram to be sent
            // NOTE: send size is entire buffer for now, since we don't reuse these
            public byte[] dataBuffer = null;
        }

        /// <summary>
        /// owned by the socket thread pool thread that is currently processing the receive operation, then
        /// ownership is handed off to main thread for final processing.  object is not reused (yet)
        /// </summary>
        private class ReceiveContext
        {
            public Socket socket;

            public class Message
            {
                // preallocated space
                public byte[] data = new byte[2048];

                // fill level
                public int bytesReceived = 0;

                // source of datagram received
                public EndPoint fromEndPoint = new IPEndPoint(IPAddress.Any, 0);

                // tokens parsed out on socket thread pool to avoid loading main thread
                public string[] tokens = new string[1024];
                public int tokenCount = 0;

                public void Clear()
                {
                    fromEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    bytesReceived = 0;
                    tokenCount = 0;
                }
            }

            // buffers for datagrams received on one context switch
            // REVISIT: create an array of counters for sizes actually used, use to tune size under heavy load
            private Message[] _messages = new Message[10];

            // number of buffers filled
            private int _messagesFilled = 0;

            /// <summary>
            /// called before we are recycled to pool
            /// </summary>
            public void Clear()
            {
                _messagesFilled = 0;
                socket = null;
            }

            public int Length
            {
                get => _messagesFilled;
            }

            public int Capacity
            {
                get => _messages.Length;
            }

            public Message BeginWrite()
            {
                if (_messagesFilled >= _messages.Length)
                {
                    throw new IndexOutOfRangeException("logic error: attempt to fill receive context past capacity");
                }
                if (_messages[_messagesFilled] == null)
                {
                    // lazy allocate
                    _messages[_messagesFilled] = new Message();
                } else
                {
                    _messages[_messagesFilled].Clear();
                }
                return _messages[_messagesFilled];
            }

            public Message ContinueWrite(int index)
            {
                if (index != _messagesFilled)
                {
                    throw new IndexOutOfRangeException("logic error: attempt to continue write that is not current");
                }
                return _messages[index];
            }

            public void EndWrite()
            {
                _messagesFilled++;
            }

            public Message Read(int index)
            {
                if (index >= _messagesFilled)
                {
                    throw new IndexOutOfRangeException("logic error: attempt to read receive context past fill level");
                }
                return _messages[index];
            }
        }

        private MainThreadAccess _main = new MainThreadAccess();
        private SharedAccess _shared = new SharedAccess();

        public BaseUDPInterface(string name)
            : base(name)
        {
            _socketDataCallback = new AsyncCallback(OnDataReceived);

            _main.ConnectedTrigger = new HeliosTrigger(this, "", "", "Connected", "Fired on DCS connect.");
            Triggers.Add(_main.ConnectedTrigger);

            _main.DisconnectedTrigger = new HeliosTrigger(this, "", "", "Disconnected", "Fired on DCS disconnect.");
            Triggers.Add(_main.DisconnectedTrigger);

            _main.ProfileLoadedTrigger = new HeliosTrigger(this, "", "", "Profile Delay Start", "Fired 10 seconds after DCS profile is started.");
            Triggers.Add(_main.ProfileLoadedTrigger);

            _main.Functions.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Functions_CollectionChanged);
        }

        void Functions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (NetworkFunction function in e.OldItems)
                {
                    Triggers.RemoveSlave(function.Triggers);
                    Actions.RemoveSlave(function.Actions);
                    Values.RemoveSlave(function.Values);

                    foreach (ExportDataElement element in function.GetDataElements())
                    {
                        if (_main.FunctionsById.ContainsKey(element.ID))
                        {
                            _main.FunctionsById.Remove(element.ID);
                        }
                    }
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (NetworkFunction function in e.NewItems)
                {
                    Triggers.AddSlave(function.Triggers);
                    Actions.AddSlave(function.Actions);
                    Values.AddSlave(function.Values);

                    foreach (ExportDataElement element in function.GetDataElements())
                    {
                        if (!_main.FunctionsById.ContainsKey(element.ID))
                        {
                            _main.FunctionsById.Add(element.ID, function);
                        }
                        else
                        {
                            Logger.Error("UDP interface created duplicate function ID. (Interface=\"" + Name + "\", Function ID=\"" + element.ID + "\")");
                        }
                    }
                }
            }
        }

        public int Port
        {
            get
            {
                return _shared.Port;
            }
            set
            {
                int oldValue;
                lock(_shared.Lock)
                {
                    oldValue = _shared.Port;
                    if (!_shared.Port.Equals(value))
                    {
                        _shared.Port = value;
                    }
                }
                if (!oldValue.Equals(value))
                {
                    OnPropertyChanged("Port", oldValue, value, false);
                }
            }
        }

        public NetworkFunctionCollection Functions
        {
            get
            {
                return _main.Functions;
            }
        }

        protected override void OnProfileChanged(HeliosProfile oldProfile)
        {
            base.OnProfileChanged(oldProfile);
            if (oldProfile != null)
            {
                oldProfile.ProfileStarted -= new EventHandler(Profile_ProfileStarted);
                oldProfile.ProfileStopped -= new EventHandler(Profile_ProfileStopped);
            }

            if (Profile != null)
            {
                Profile.ProfileStarted += new EventHandler(Profile_ProfileStarted);
                Profile.ProfileStopped += new EventHandler(Profile_ProfileStopped);
            }
        }

        /// <summary>
        /// exclusive ownership of context is transfered to the callee
        /// </summary>
        /// <param name="context"></param>
        private void WaitForData(ReceiveContext context)
        {
            if ((!_shared.Started) || (context.socket == null))
            {
                // we are shut down
                return;
            }

            do
            {
                Logger.Debug("UDP interface waiting for socket data on {Interface}.", Name);
                try
                {
                    ReceiveContext.Message message = context.BeginWrite();
                    _ = context.socket.BeginReceiveFrom(message.data, 0, message.data.Length, SocketFlags.None, ref message.fromEndPoint, _socketDataCallback, context);
                    break;
                }
                catch (SocketException se)
                {
                    if (!HandleSocketException(se, out context.socket))
                    {
                        Logger.Error("UDP interface unable to recover from socket reset, no longer receiving data. (Interface=\"" + Name + "\")");
                        break;
                    }
                    // else retry forever
                }
            } while (true);
        }

        /// <summary>
        /// socket thread pool callback
        /// </summary>
        /// <param name="asyncResult"></param>
        private void OnDataReceived(IAsyncResult asyncResult)
        {
            if (!_shared.Started)
            {
                // ignore, we shut down since requesting receive
                return;
            }
            ReceiveContext context = asyncResult.AsyncState as ReceiveContext;
            try
            {
                ReceiveContext.Message message = context.ContinueWrite(0);
                message.bytesReceived = context.socket.EndReceiveFrom(asyncResult, ref message.fromEndPoint);
                context.EndWrite();
            }
            catch (SocketException se)
            {
                // NOTE: EndReceiveFrom isn't retriable, because the receive won't we valid after we reset socket
                if (!HandleSocketException(se, out context.socket))
                {
                    // no new receive attempt
                    return;
                }

                // recovered with probably a new socket
            }
            // drain the socket, as much as allowed, to share the context switch to main
            while ((context.socket.Available > 0) && (context.Length < context.Capacity))
            {
                ReceiveContext.Message message = context.BeginWrite();
                message.bytesReceived = 0;
                try
                {
                    message.bytesReceived = context.socket.ReceiveFrom(message.data, 0, message.data.Length, SocketFlags.None, ref message.fromEndPoint);
                    if (message.bytesReceived > 0)
                    {
                        // if we did not receive anything or throw, we use this slot again
                        context.EndWrite();
                    }
                }
                catch (SocketException se)
                {
                    if (HandleSocketException(se, out context.socket))
                    {
                        // recovered with probably a new socket
                    } else {
                        // dead, stop trying to drain
                        break;
                    }
                }
            }

            // it could be empty if all we did this iteration is throw and reset the socket 
            if (context.Length > 0)
            {
                // offload parsing from main thread to socket thread pool, without lock held
                ParseReceived(context);

                // pass ownership to main thread, process synchronously
                Dispatcher.Invoke(() => DispatchReceived(context), System.Windows.Threading.DispatcherPriority.Send);
            }

            // start next receive
            WaitForData(_shared.FetchReceiveContext() ?? new ReceiveContext() { socket = _shared.ServerSocket });
        }

        private static void ParseReceived(ReceiveContext owned)
        {
            for(int messageIndex=0; messageIndex<owned.Length; messageIndex++)
            {
                ReceiveContext.Message message = owned.Read(messageIndex);
                message.tokenCount = 0;
                int parseCount = message.bytesReceived - 1;
                int offset = 8;
                for (int scan = 9; scan < parseCount; scan++)
                {
                    if (message.data[scan] == 0x3a || message.data[scan] == 0x3d)
                    {
                        int size = scan - offset - 1;
                        message.tokens[message.tokenCount++] = _iso_8859_1.GetString(message.data, offset + 1, size);
                        offset = scan;
                    }
                }
                message.tokens[message.tokenCount++] = _iso_8859_1.GetString(message.data, offset + 1, parseCount - offset - 1);
                if (message.tokenCount % 1 > 0)
                {
                    // don't allow odd number of tokens because a lot of the parsing code is unsafe
                    message.tokenCount--;
                }
            }
        }

        private void DispatchReceived(ReceiveContext owned)
        {
            if (owned.Length > 1)
            {
                Logger.Debug("received {MessageCount} UDP messages in batch", owned.Length);
            }

            // REVISIT: could skip ahead if this batch contains a client change
            for (int messageIndex = 0; messageIndex < owned.Length; messageIndex++)
            {
                ReceiveContext.Message message = owned.Read(messageIndex);
                // store address and port, since we need it for outgoing messages
                _main.Client = message.fromEndPoint;
                if (message.bytesReceived < 13)
                {
                    HandleShortMessage(message.data, message.bytesReceived);
                    continue;
                }

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("UDP Interface received packet on {Interface}: {Packet}.", Name, _iso_8859_1.GetString(message.data, 0, message.bytesReceived));
                }

                // handle client restart or change in client
                String packetClientID = _iso_8859_1.GetString(message.data, 0, 8);
                if (!_main.ClientID.Equals(packetClientID))
                { 
                    Logger.Info("UDP interface new client connected, sending data reset command. (Interface=\"" + Name + "\", Client=\"" + _main.Client.ToString() + "\", Client ID=\"" + packetClientID + "\")");
                    string fromValue = _main.ClientID;
                    _main.ClientID = packetClientID;
                    OnClientChanged(fromValue, packetClientID);
                    SendData("R");
                }

                // tokenCount is already even at this point because we fix it up during pre-parsing
                Debug.Assert(message.tokenCount % 1 == 0);
                for (int tokenIndex = 0; tokenIndex < message.tokenCount; tokenIndex += 2)
                {
                    string id = message.tokens[tokenIndex];
                    if (_main.FunctionsById.ContainsKey(id))
                    {
                        NetworkFunction function = _main.FunctionsById[id];
                        function.ProcessNetworkData(id, message.tokens[tokenIndex + 1]);
                    }
                    else if (id == "DISCONNECT")
                    {
                        // if DISCONNECT is formatted as a valid message with a value, we get here, otherwise we
                        // handled it in HandleShortMessage
                        Logger.Debug("UDP interface received disconnect message from simulator.");
                        _main.DisconnectedTrigger.FireTrigger(BindingValue.Empty);
                    }
                    else
                    {
                        OnUnrecognizedFunction(id, message.tokens[tokenIndex+1]);
                    }
                }
            }

            _shared.ReturnReceiveContext(owned);
        }

        protected virtual void OnUnrecognizedFunction(string id, string value)
        {
            Logger.Warn($"UDP interface received data for missing function. (Key=\"{id}\")");
        }


        private void HandleShortMessage(byte[] dataBuffer, int receivedByteCount)
        {
            string RecString = _iso_8859_1.GetString(dataBuffer, 0, receivedByteCount);
            // Special case for legacy Disconnect
            if (RecString.Contains("DISCONNECT"))
            {
                Logger.Info("UDP interface disconnect from Lua.");
                _main.DisconnectedTrigger.FireTrigger(BindingValue.Empty);
                return;
            }
            Logger.Warn("UDP interface short packet received. (Interface=\"" + Name + "\")");
        }

        // WARNING: called on both Main and Socket threads, depending on where the failure occurred
        private bool HandleSocketException(SocketException se, out Socket newSocket)
        {
            if ((SocketError)se.ErrorCode == SocketError.ConnectionReset)
            {
                try
                {
                    CloseSocket();
                    newSocket = OpenSocket();
                }
                catch (SocketException secondException)
                {
                    Logger.Error("UDP interface threw exception (Interface=\"" + Name + "\")", se);
                    Logger.Error("UDP interface then threw exception reopening socket; cannot continue. (Interface=\"" + Name + "\")", secondException);
                    newSocket = null;
                    return false;
                }
                return true;
            }
            else
            {
                Logger.Error("UDP interface threw unhandled exception handling socket reset. (Interface=\"" + Name + "\")", se);
                newSocket = null;
                return false;
            }
        }

        public void SendData(string data)
        {
            try
            {
                if ((_main.Client != null) && (_main.ClientID != ClientChange.NO_CLIENT))
                {
                    Logger.Debug("UDP interface sending data on {Interface}: {Packet}.", Name, data);
                    SendContext context = new SendContext();
                    context.dataBuffer = _iso_8859_1.GetBytes(data + "\n");
                    Socket socket = _shared.ServerSocket;
                    socket?.BeginSendTo(context.dataBuffer, 0, context.dataBuffer.Length, SocketFlags.None, _main.Client, OnDataSent, context);
                }
            }
            catch (SocketException se)
            {
                // just ignore
                Logger.Debug($"UDP interface threw handled socket exception \"{se.Message}\" while sending data. (Interface=\"" + Name + "\", Data=\"" + data + "\")");
                HandleSocketException(se, out Socket unused);
            }
            catch (Exception e)
            {
                Logger.Error(e, "UDP interface threw exception sending data on {Interface}.", Name);
            }
        }

        /// <summary>
        /// socket thread pool callback
        /// </summary>
        /// <param name="asyncResult"></param>
        private void OnDataSent(IAsyncResult asyncResult)
        {
            SendContext context = asyncResult.AsyncState as SendContext;
            // currently we don't need to do anything on send
            // we are only using the async send API in order to match async reads and writes,
            // because we don't want to be an atypical user of the API
        }

        void Profile_ProfileStopped(object sender, EventArgs e)
        {
            CloseSocket();
            if (_main.StartupTimer != null)
                _main.StartupTimer.Stop();
        }


        // WARNING: called on both Main and Socket threads, depending on where a socket exception occurred
        private Socket OpenSocket()
        {
            EndPoint bindEndPoint = new IPEndPoint(IPAddress.Any, _shared.Port);
            Socket socket = new Socket(AddressFamily.InterNetwork,
                                      SocketType.Dgram,
                                      ProtocolType.Udp);
            socket.ExclusiveAddressUse = false;
            socket.Bind(bindEndPoint);
            _shared.ServerSocket = socket;

            // we need to return this to the caller, so they don't rely on _shared.ServerSocket, in case it immediately gets changed by another thread
            return socket;
        }

        private void CloseSocket()
        {
            Socket socket = null;
            lock (_shared.Lock)
            {
                socket = _shared.ServerSocket;
                _shared.ServerSocket = null;
            }
            // shutdown without holding lock
            socket?.Close();

            // hook for descendants
            OnProfileStopped();
        }

        void Profile_ProfileStarted(object sender, EventArgs e)
        {
            Logger.Debug("UDP interface {Interface} starting.", Name);
            Socket serverSocket = null;

            // hook for descendants to initialize for a profile start before we receive traffic
            OnProfileStarted();

            try
            {
                _main.Client = new IPEndPoint(IPAddress.Any, 0);
                _main.ClientID = "";
                serverSocket = OpenSocket();
            }
            catch (System.Net.Sockets.SocketException se)
            {
                Logger.Error("UDP interface startup error. (Interface=\"" + Name + "\")");
                Logger.Error("UDP Socket Exception on Profile Start.  " + se.Message, se);
            }

            // 10 seconds for Delayed Startup
            Timer timer = new Timer(10000);
            timer.AutoReset = false; // only once
            timer.Elapsed += OnStartupTimer;
            _main.StartupTimer = timer;

            // start delayed start timer
            Logger.Debug("Starting startup timer.");
            timer.Start();

            // we continue to run even if we cannot receive
            if (serverSocket != null)
            {
                // now go active
                Logger.Debug("Starting UDP receiver.");
                WaitForData(new ReceiveContext() { socket = serverSocket });
            }
        }

        /// <summary>
        /// timer thread callback
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnStartupTimer(Object source, System.Timers.ElapsedEventArgs e)
        {
            // sync notify
            Dispatcher.Invoke(new Action(OnDelayedStartup));
        }

        private void OnDelayedStartup()
        { 
            Logger.Debug("Delayed startup timer triggered.");
            _main.ProfileLoadedTrigger.FireTrigger(BindingValue.Empty);
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            // No Op
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            // No Op
        }

        protected void AddFunction(NetworkFunction function)
        {
            Functions.Add(function);
        }

        protected void AddFunction(NetworkFunction function, bool debug)
        {
            function.IsDebugMode = debug;
            Functions.Add(function);
        }

        public override void Reset()
        {
            base.Reset();
            foreach (NetworkFunction function in Functions)
            {
                function.Reset();
            }
            SendData("R");
        }

        public bool CanSend
        {
            get
            {
                return (_shared.Started && (_main.Client != null) && (_main.ClientID.Length > 0));
            }
        }

        protected virtual void OnProfileStarted()
        {
            // no code in base implementation
        }

        protected virtual void OnProfileStopped()
        {
            // no code in base implementation
        }

        protected virtual void OnClientChanged(string fromValue, string toValue)
        {
            _main.ConnectedTrigger.FireTrigger(BindingValue.Empty);
            ClientChanged?.Invoke(this, new ClientChange() { FromOpaqueHandle = fromValue, ToOpaqueHandle = toValue });
        }
    }
}