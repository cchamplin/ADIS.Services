﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using log4net;

namespace ADIS.Services
{
    public class AspRequest : IHttpRequest
    {
        public static ILog log = LogManager.GetLogger(typeof(AspRequest));

        protected readonly IResponse response;
        protected readonly HttpRequestBase request;
        protected Dictionary<string, object> items;
        protected Dictionary<string, Cookie> cookies;

        public AspRequest(HttpContextBase context)
        {
            this.request = context.Request;
            try
            {
                //this.request = new AspResponse(context.Response);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
            items = new Dictionary<string, object>();
            if (context.Items != null && context.Items.Count > 0)
            {
                foreach (var item in context.Items.Keys)
                {
                    if (item is string)
                    {
                        items[(string)item] = context.Items[item];
                    }
                }
            }
            cookies = new Dictionary<string, Cookie>();
            if (context.Request.Cookies != null && context.Request.Cookies.Count > 0)
            {
                foreach (string cookieName in context.Request.Cookies)
                {
                    try
                    {
                        var cookie = context.Request.Cookies[cookieName];
                        var newCookie = new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain);
                        newCookie.Expires = cookie.Expires;
                        newCookie.HttpOnly = cookie.HttpOnly;
                        newCookie.Secure = cookie.Secure;
                        cookies.Add(newCookie.Name, newCookie);
                    }
                    catch (Exception ex)
                    {
                        log.Warn("Invalid cookie name: " + cookieName,ex);
                    }
                }
            }
        }

        public object InternalRequest
        {
            get { return request; }
        }

        public IResponse Response
        {
            get { return response; }
        }

        public string Method
        {
            get { return request.HttpMethod; }
        }

        public bool IsLocal
        {
            get { return request.IsLocal; }
        }

        public string UserAgent
        {
            get { return request.UserAgent; }
        }

        public Dictionary<string, System.Net.Cookie> Cookies
        {
            get { return cookies; }
        }

        public Dictionary<string, object> Items
        {
            get { return items; }
        }

        public string ResponseType
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string GetContent()
        {
            using (var reader = new StreamReader(request.InputStream))
            {
                return reader.ReadToEnd();
            }
        }

        public string RawUrl
        {
            get { return request.RawUrl; }
        }

        public string Host
        {
            get { return request.Url.Host; }
        }

        public string RemoteIP
        {
            get { return request.UserHostAddress; }
        }

        public bool IsSecure
        {
            get { return request.IsSecureConnection; }
        }

        public long Length
        {
            get { return request.ContentLength; }
        }

        public Uri UrlReferrer
        {
            get { return request.UrlReferrer; }
        }
    }
}
