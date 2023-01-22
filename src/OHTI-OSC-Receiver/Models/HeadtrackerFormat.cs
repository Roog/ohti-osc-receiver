#region copyright
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

namespace OHTI_OSC_Receiver.Models
{
    public class HeadtrackerFormat
    {
        public string Address { get; set; }
        public float W { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public double Yaw { get; set; }
        public double Pitch { get; set; }
        public double Roll { get; set; }

        public float[] Euler { get; set; } = new float[3];

        public DateTime UpdatedTime { get; set; } = DateTime.Now;

        public void Save(string address, float w, float x, float y, float z)
        {
            Address = address;
            W = w;
            X = x;
            Y = y;
            Z = z;

            UpdatedTime = DateTime.Now;

            ToEuler();
        }

        private void ToEuler()
        {
            // roll (x-axis rotation)
            var sinrCosp = 2 * (W * X + Y * Z);
            var cosrCosp = 1 - 2 * (X * X + Y * Y);
            Roll = (Math.Atan2(sinrCosp, cosrCosp));

            // pitch (y-axis rotation)
            var sinp = 2 * (W * Y - Z * X);
            if (Math.Abs(sinp) >= 1)
            {
                Pitch = ((Math.PI / 2) * Math.Sign(sinp)); // use 90 degrees if out of range, copysign = sinp + or - applied to Math.PI/2
            }
            else
            {
                Pitch = (Math.Asin(sinp));
            }

            // yaw (z-axis rotation)
            var sinyCosp = 2 * (W * Z + X * Y);
            var cosyCosp = 1 - 2 * (Y * Y + Z * Z);
            Yaw = (Math.Atan2(sinyCosp, cosyCosp));

            Euler[0] = (float)Math.Round(RadiansToDegree(Yaw) * 100) / 100;
            Euler[1] = (float)Math.Round(RadiansToDegree(Pitch) * 100) / 100;
            Euler[2] = (float)Math.Round(RadiansToDegree(Roll) * 100) / 100;
        }

        public double RadiansToDegree(double radian)
        {
            return (radian * (180 / Math.PI));
        }

        public override string ToString()
        {
            return $"Data: {Address}, W: {W}, X: {X}, Y: {Y}, Z: {Z}";
        }
    }
}