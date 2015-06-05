using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ADIS.Services
{
    public class ListenerResponse : IResponse
    {
        protected HttpListenerResponse response;
        protected bool closed = false;
        public ListenerResponse(HttpListenerResponse response)
        {
            this.response = response;
        }

        public void Close()
        {
            closed = true;
            response.OutputStream.Flush();
            response.OutputStream.Close();
        }

        public object InternalRequest
        {
            get { return response; }
        }

        public int StatusCode
        {
            get
            {
                return this.response.StatusCode;
            }
            set
            {
                this.response.StatusCode = value;
            }
        }

        public string ContentType
        {
            get
            {
                return response.ContentType;
            }
            set
            {
               response.ContentType = value;
            }
        }

        public void AddHeader(string name, string value)
        {
            response.Headers.Add(name,value);
        }

        public void Redirect(string url)
        {
            response.Redirect(url);
        }

        public System.IO.Stream OutputStream
        {
            get { return response.OutputStream; }
        }

        public void Write(string text)
        {
            var byteData = System.Text.Encoding.UTF8.GetBytes(text);
            response.ContentLength64 = byteData.Length;
            response.OutputStream.Write(byteData,0,byteData.Length);
            closed = true;
            response.OutputStream.Flush();
            response.OutputStream.Close();
        }

        public void Flush()
        {
            response.OutputStream.Flush();
        }

        public bool Closed
        {
            get { return closed; }
        }

        public void SetLength(long length)
        {
            response.ContentLength64 = length;
        }

        public bool KeepAlive
        {
            get
            {
                return response.KeepAlive;
            }
            set
            {
                response.KeepAlive = value;
            }
        }
    }
}
