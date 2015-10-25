using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa
{
    internal class WebClient : System.Net.WebClient
    {
        public int Timeout { get; set; } = 5000;

        protected override WebRequest GetWebRequest(Uri uri)
        {
            var req = base.GetWebRequest(uri);
            req.Timeout = Timeout;

            var httpRequest = (HttpWebRequest)req;
            if (httpRequest != null)
            {
                httpRequest.AllowAutoRedirect = true;
            }

            return req;
        }
    }
}
