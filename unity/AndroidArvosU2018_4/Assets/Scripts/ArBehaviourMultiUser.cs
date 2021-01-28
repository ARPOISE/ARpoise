/*
ArBehaviourMultiUser.cs - MonoBehaviour for Arpoise multi-user handling.

Copyright (C) 2020, Tamiko Thiel and Peter Graf - All Rights Reserved

ARPOISE - Augmented Reality Point Of Interest Service 

This file is part of Arpoise.

    Arpoise is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Arpoise is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Arpoise.  If not, see <https://www.gnu.org/licenses/>.

For more information on 

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
Arpoise, see www.Arpoise.com/

*/
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    // This is just an experiment
    //
    public class ArBehaviourMultiUser : MonoBehaviour
    {
        protected virtual void Start()
        {
            //Init();
        }

        //private long _lastSend = 0;
        protected virtual void Update()
        {
            //var now = DateTime.Now.Ticks / 10000000L;
            //if (_client != null && (_lastSend == 0 || _lastSend + 5 < now))
            //{
            //    _lastSend = now;

            //    // Send Hello
            //    //
            //    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("176.9.99.163"), port);
            //    var data = new byte[5];
            //    data[0] = 3;
            //    data[1] = 0;
            //    data[2] = 0;
            //    data[3] = 0;
            //    data[4] = 0;
            //    _client.Send(data, data.Length, remoteEndPoint);
            //}
        }

        // receiving Thread
        private Thread _receiveThread;

        // udpclient object
        private UdpClient _client = null;

        protected void Init()
        {
            var port = 2000;

            _client = new UdpClient(port);

            // Start listener
            //
            _receiveThread = new Thread(new ThreadStart(ReceiveData));
            _receiveThread.IsBackground = true;
            _receiveThread.Start();

        }

        // receive thread
        private void ReceiveData()
        {
            while (true)
            {
                try
                {
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = _client.Receive(ref anyIP);
                    if (data.Length > 0)
                    {
                        print(">> " + data[0]);
                    }
                }
                catch (Exception err)
                {
                    print(err.ToString());
                }
            }
        }
    }
}
