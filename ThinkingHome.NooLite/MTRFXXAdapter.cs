﻿using System;
using ThinkingHome.NooLite.Commands;
using ThinkingHome.NooLite.SerialPort;
using Action = ThinkingHome.NooLite.Commands.Action;

namespace ThinkingHome.NooLite
{
    public class MTRFXXAdapter : IDisposable
    {
        #region common

        private readonly SerialDevice device;

        public MTRFXXAdapter(string portName)
        {
            device = SerialDevice.Create(portName, BaudRate.B9600);
            device.DataReceived += OnDataReceived;
        }

        public void Open()
        {
            device.Open();
        }

        public bool IsOpened => device.IsOpened;

        public void Dispose()
        {
            device.Dispose();
        }

        #endregion

        #region commands

        public event Action<object, byte[]> DataReceived;

        private void OnDataReceived(object sender, byte[] data)
        {
            DataReceived?.Invoke(this, data);
        }

        public void SendCommand(Mode mode, Action action, byte channel, Command command,
            RepeatCount repeatCount = RepeatCount.NoRepeat, DataFormat format = DataFormat.NoData,
            byte[] data = null, UInt32 target = 0)
        {
            var cmd = BuildCommand(mode, action, repeatCount, channel, command, format, data, target);
            device.Write(cmd);
        }

        #endregion

        #region commands: static

        private const byte START_MARKER = 171;
        private const byte STOP_MARKER = 172;

        public static byte[] BuildCommand(Mode mode, Action action, RepeatCount repeatCount, byte channel,
            Command command, DataFormat format, byte[] data, UInt32 target = 0)
        {
            byte actionAndRepeatCount = (byte) ((byte) action | ((byte) repeatCount << 6));

            var res = new byte[]
            {
                START_MARKER,         // 0: start marker
                (byte) mode,          // 1: device mode
                actionAndRepeatCount, // 2: action & repeat count
                0,                    // 3: reserved
                channel,              // 4: channel
                (byte) command,       // 5: command
                (byte) format,        // 6: data format
                0,0,0,0,              // 7..10: data
                0,0,0,0,              // 11..14: target device id
                0,                    // 15: checksum
                STOP_MARKER           // 16: stop marker
            };

            for (int i = 0; i < 15; i++) res[15] += res[i];

            return res;
        }

        #endregion
    }
}