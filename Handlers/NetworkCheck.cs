//
// DnsChecker.cs
//
// Author:
//       Godwin peter .O <me@godwin.dev>
//
// Copyright (c) 2020 MIT
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Tiny.RestClient;
using RFIDIOT.Helpers;

namespace RFIDIOT.Handlers
{
    public class NetworkCheck
    {
        private static bool _finalStatus;
        private readonly MainLogger _logger;
        private static readonly ConfigKey _config = new ConfigKey();
        private readonly TinyRestClient _httpclient;
        readonly Ping _pingInit = new Ping();
        private readonly byte[] _buffer = new byte[32];
        private readonly int _timeout = _config.IOT_NETWORK_CHECK_TIMEOUT;
        readonly PingOptions _pingOptions = new PingOptions();
        private readonly string _host = _config.IOT_NETWORK_CHECK_ADDRESS;

        public NetworkCheck()
        {
            _logger = new MainLogger();
            _httpclient = new HTTPInitializer().Resolve();
        }
        private bool  DNSSee()
        {
            try
            {
                PingReply receivedPingReply = _pingInit.Send(_host, _timeout, _buffer, _pingOptions);
                if (receivedPingReply != null) _finalStatus = receivedPingReply.Status == IPStatus.Success;
            }
            catch (Exception)
            {
                _finalStatus = false;
            }
            return _finalStatus;
        }

        private async Task<bool> HttpCheck()
        {
            var isCompleted = false;
            try
            {
                var Request = _httpclient.GetRequest(string.Empty)
                    .ExecuteAsHttpResponseMessageAsync();
                Task.WaitAll(Request);
                await Request;
                isCompleted = Request.IsCompletedSuccessfully;
            }
            catch (Exception ex)
            {
                var excp = ex.ToString();
                excp = string.Empty;
            }

            return isCompleted;
        }

        public bool Status()
        {
            return !_config.IOT_NETWORK_CHECK || DNSSee() && HttpCheck().Result;
        }
    }
}