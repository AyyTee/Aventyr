using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;

#if !__NOIPENDPOINT__
using NetEndPoint = System.Net.IPEndPoint;
#endif

namespace Lidgren.Network
{
	public partial class NetPeer
	{
		/// <summary>
		/// Send a message to a specific connection
		/// </summary>
		/// <param name="msg">The message to send</param>
		/// <param name="recipient">The recipient connection</param>
		/// <param name="method">How to deliver the message</param>
		public NetSendResult SendMessage(INetOutgoingMessage msg, INetConnection recipient, NetDeliveryMethod method)
		{
			return SendMessage(msg, recipient, method, 0);
		}

		/// <summary>
		/// Send a message to a specific connection
		/// </summary>
		/// <param name="msg">The message to send</param>
		/// <param name="recipient">The recipient connection</param>
		/// <param name="method">How to deliver the message</param>
		/// <param name="sequenceChannel">Sequence channel within the delivery method</param>
		public NetSendResult SendMessage(INetOutgoingMessage msg, INetConnection recipient, NetDeliveryMethod method, int sequenceChannel)
		{
            NetOutgoingMessage _msg = (NetOutgoingMessage)msg;
            NetConnection _recipient = (NetConnection)recipient;
			if (msg == null)
				throw new ArgumentNullException("msg");
			if (recipient == null)
				throw new ArgumentNullException("recipient");
			if (sequenceChannel >= NetConstants.NetChannelsPerDeliveryMethod)
				throw new ArgumentOutOfRangeException("sequenceChannel");

			NetException.Assert(
				((method != NetDeliveryMethod.Unreliable && method != NetDeliveryMethod.ReliableUnordered) ||
				((method == NetDeliveryMethod.Unreliable || method == NetDeliveryMethod.ReliableUnordered) && sequenceChannel == 0)),
				"Delivery method " + method + " cannot use sequence channels other than 0!"
			);

			NetException.Assert(method != NetDeliveryMethod.Unknown, "Bad delivery method!");

			if (_msg.m_isSent)
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
            _msg.m_isSent = true;

			bool suppressFragmentation = (method == NetDeliveryMethod.Unreliable || method == NetDeliveryMethod.UnreliableSequenced) && m_configuration.UnreliableSizeBehaviour != NetUnreliableSizeBehaviour.NormalFragmentation;

			int len = NetConstants.UnfragmentedMessageHeaderSize + msg.LengthBytes; // headers + length, faster than calling msg.GetEncodedSize
			if (len <= _recipient.m_currentMTU || suppressFragmentation)
			{
				Interlocked.Increment(ref _msg.m_recyclingCount);
				return _recipient.EnqueueMessage(_msg, method, sequenceChannel);
			}
			else
			{
				// message must be fragmented!
				if (_recipient.m_status != NetConnectionStatus.Connected)
					return NetSendResult.FailedNotConnected;
				return SendFragmentedMessage(_msg, new NetConnection[] { _recipient }, method, sequenceChannel);
			}
		}

		internal static int GetMTU(IList<INetConnection> recipients)
		{
			int count = recipients.Count;

			int mtu = int.MaxValue;
			if (count < 1)
			{
#if DEBUG
				throw new NetException("GetMTU called with no recipients");
#else
				// we don't have access to the particular peer, so just use default MTU
				return NetPeerConfiguration.kDefaultMTU;
#endif
			}

			for(int i=0;i<count;i++)
			{
				var conn = (NetConnection)recipients[i];
				int cmtu = conn.m_currentMTU;
				if (cmtu < mtu)
					mtu = cmtu;
			}
			return mtu;
		}

		/// <summary>
		/// Send a message to a list of connections
		/// </summary>
		/// <param name="msg">The message to send</param>
		/// <param name="recipients">The list of recipients to send to</param>
		/// <param name="method">How to deliver the message</param>
		/// <param name="sequenceChannel">Sequence channel within the delivery method</param>
		public void SendMessage(INetOutgoingMessage msg, IList<INetConnection> recipients, NetDeliveryMethod method, int sequenceChannel)
		{
            NetOutgoingMessage _msg = (NetOutgoingMessage)msg;
            if (msg == null)
				throw new ArgumentNullException("msg");
			if (recipients == null)
			{
				if (_msg.m_isSent == false)
					Recycle(_msg);
				throw new ArgumentNullException("recipients");
			}
			if (recipients.Count < 1)
			{
				if (_msg.m_isSent == false)
					Recycle(_msg);
				throw new NetException("recipients must contain at least one item");
			}
			if (method == NetDeliveryMethod.Unreliable || method == NetDeliveryMethod.ReliableUnordered)
				NetException.Assert(sequenceChannel == 0, "Delivery method " + method + " cannot use sequence channels other than 0!");
			if (_msg.m_isSent)
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
            _msg.m_isSent = true;

			int mtu = GetMTU(recipients);

			int len = _msg.GetEncodedSize();
			if (len <= mtu)
			{
				Interlocked.Add(ref _msg.m_recyclingCount, recipients.Count);
				foreach (NetConnection conn in recipients)
				{
					if (conn == null)
					{
						Interlocked.Decrement(ref _msg.m_recyclingCount);
						continue;
					}
					NetSendResult res = conn.EnqueueMessage(_msg, method, sequenceChannel);
					if (res == NetSendResult.Dropped)
						Interlocked.Decrement(ref _msg.m_recyclingCount);
				}
			}
			else
			{
				// message must be fragmented!
				SendFragmentedMessage(_msg, recipients, method, sequenceChannel);
			}

			return;
		}

		/// <summary>
		/// Send a message to an unconnected host
		/// </summary>
		public void SendUnconnectedMessage(INetOutgoingMessage msg, string host, int port)
		{
            NetOutgoingMessage _msg = (NetOutgoingMessage)msg;
            if (msg == null)
				throw new ArgumentNullException("msg");
			if (host == null)
				throw new ArgumentNullException("host");
			if (_msg.m_isSent)
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			if (msg.LengthBytes > m_configuration.MaximumTransmissionUnit)
				throw new NetException("Unconnected messages too long! Must be shorter than NetConfiguration.MaximumTransmissionUnit (currently " + m_configuration.MaximumTransmissionUnit + ")");

            _msg.m_isSent = true;
            _msg.m_messageType = NetMessageType.Unconnected;

			var adr = NetUtility.Resolve(host);
			if (adr == null)
				throw new NetException("Failed to resolve " + host);

			Interlocked.Increment(ref _msg.m_recyclingCount);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(new NetEndPoint(adr, port), _msg));
		}

		/// <summary>
		/// Send a message to an unconnected host
		/// </summary>
		public void SendUnconnectedMessage(INetOutgoingMessage msg, NetEndPoint recipient)
		{
            NetOutgoingMessage _msg = (NetOutgoingMessage)msg;
            if (msg == null)
				throw new ArgumentNullException("msg");
			if (recipient == null)
				throw new ArgumentNullException("recipient");
			if (_msg.m_isSent)
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			if (msg.LengthBytes > m_configuration.MaximumTransmissionUnit)
				throw new NetException("Unconnected messages too long! Must be shorter than NetConfiguration.MaximumTransmissionUnit (currently " + m_configuration.MaximumTransmissionUnit + ")");

            _msg.m_messageType = NetMessageType.Unconnected;
            _msg.m_isSent = true;

			Interlocked.Increment(ref _msg.m_recyclingCount);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(recipient, _msg));
		}

		/// <summary>
		/// Send a message to an unconnected host
		/// </summary>
		public void SendUnconnectedMessage(INetOutgoingMessage msg, IList<NetEndPoint> recipients)
		{
            NetOutgoingMessage _msg = (NetOutgoingMessage)msg;
            if (msg == null)
				throw new ArgumentNullException("msg");
			if (recipients == null)
				throw new ArgumentNullException("recipients");
			if (recipients.Count < 1)
				throw new NetException("recipients must contain at least one item");
			if (_msg.m_isSent)
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			if (msg.LengthBytes > m_configuration.MaximumTransmissionUnit)
				throw new NetException("Unconnected messages too long! Must be shorter than NetConfiguration.MaximumTransmissionUnit (currently " + m_configuration.MaximumTransmissionUnit + ")");

            _msg.m_messageType = NetMessageType.Unconnected;
            _msg.m_isSent = true;

			Interlocked.Add(ref _msg.m_recyclingCount, recipients.Count);
			foreach (NetEndPoint ep in recipients)
				m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(ep, _msg));
		}

		/// <summary>
		/// Send a message to this exact same netpeer (loopback)
		/// </summary>
		public void SendUnconnectedToSelf(INetOutgoingMessage om)
		{
            NetOutgoingMessage _om = (NetOutgoingMessage)om;
            if (om == null)
				throw new ArgumentNullException("msg");
			if (_om.m_isSent)
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");

            _om.m_messageType = NetMessageType.Unconnected;
            _om.m_isSent = true;

			if (m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.UnconnectedData) == false)
			{
				Interlocked.Decrement(ref _om.m_recyclingCount);
				return; // dropping unconnected message since it's not enabled for receiving
			}

			// convert outgoing to incoming
			NetIncomingMessage im = CreateIncomingMessage(NetIncomingMessageType.UnconnectedData, om.LengthBytes);
			im.Write(om);
			im.m_isFragment = false;
			im.m_receiveTime = NetTime.Now;
			im.m_senderConnection = null;
			im.m_senderEndPoint = m_socket.LocalEndPoint as NetEndPoint;
			NetException.Assert(im.m_bitLength == om.LengthBits);

			// recycle outgoing message
			Recycle(_om);

			ReleaseMessage(im);
		}
	}
}
