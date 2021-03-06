﻿#region copyright
/*
 * Open Headtracker Initiative OSC Websocket Gateway
 *
 * Copyright (c) 2021 Bo-Erik Sandholm & Roger Sandholm, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion copyright

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace OHTI_OSC_Receiver
{
    public class MdnsHelper
    {
        public void SendDiscoveryQuery(string local_dhcp_ip_address)
        {
            // multicast UDP-based mDNS-packet for discovering IP addresses

            System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);

            socket.Bind(new IPEndPoint(IPAddress.Parse(local_dhcp_ip_address), 52634));
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("224.0.0.251"), 5353);

            List<byte> bytes = new List<byte>();

            bytes.AddRange(new byte[] { 0x0, 0x0 });  // transaction id (ignored)
            bytes.AddRange(new byte[] { 0x1, 0x0 });  // standard query
            bytes.AddRange(new byte[] { 0x0, 0x1 });  // questions
            bytes.AddRange(new byte[] { 0x0, 0x0 });  // answer RRs
            bytes.AddRange(new byte[] { 0x0, 0x0 });  // authority RRs
            bytes.AddRange(new byte[] { 0x0, 0x0 });  // additional RRs
            bytes.AddRange(new byte[] { 0x05, 0x5f, 0x68, 0x74, 0x74, 0x70, 0x04, 0x5f, 0x74, 0x63, 0x70, 0x05, 0x6c, 0x6f, 0x63, 0x61, 0x6c, 0x00, 0x00, 0x0c, 0x00, 0x01 });  // _http._tcp.local: type PTR, class IN, "QM" question

            socket.SendTo(bytes.ToArray(), endpoint);
        }
    }
}
