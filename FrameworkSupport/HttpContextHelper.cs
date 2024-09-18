using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FrameworkSupport
{
    public class HttpContextHelper
    {
        /// <summary>
        /// Reads any available info from the HttpContext.
        /// Returns a pretty-printed string of Server Values in "Key: Value" format.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetServerVariablesPrettyPrint(HttpContext context)
        {
            StringBuilder prettyPrint = new StringBuilder();
            Dictionary<string, string> serverVariables = GetServerVariables(context);

            foreach (KeyValuePair<string, string> serverVar in serverVariables)
            {
                prettyPrint.AppendLine($"{serverVar.Key}: {serverVar.Value}");
            }

            return prettyPrint.ToString();
        }

        /// <summary>
        /// Returns a Dictionary of all available Server variables in the HttpContext.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetServerVariables(HttpContext context)
        {
            Dictionary<string, string> serverVariables = new Dictionary<string, string>();
            LoadVariables(serverVariables, () => context.Features, "");
            LoadVariables(serverVariables, () => context.User, "User_");

            var ss = context.RequestServices?.GetService(typeof(ISession));
            if (ss != null)
            {
                LoadVariables(serverVariables, () => context.Session, "Session_");
            }

            LoadVariables(serverVariables, () => context.Items, "Items_");
            LoadVariables(serverVariables, () => context.Connection, "Connection_");

            return serverVariables;
        }

        /// <summary>
        /// Reads the User Agent from the Request's Http Headers
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetRawUserAgent(HttpRequest request)
        {
            const string USER_AGENT = "User-Agent";
            return request.Headers.ContainsKey(USER_AGENT) ? (request.Headers[USER_AGENT].FirstOrDefault()?.ToString()) : string.Empty;
        }

        #region Private Methods

        private static void LoadVariables(Dictionary<string, string> serverVariables, Func<object> getObject, string prefix)
        {
            object obj;
            try
            {
                obj = getObject();
                if (obj == null)
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            var props = obj.GetType().GetProperties();
            foreach (var prop in props)
            {
                object value = null;
                try
                {
                    value = prop.GetValue(obj);
                }
                catch
                {
                    // ignored
                }

                var isProcessed = false;
                if (value is IEnumerable en && en is not string)
                {
                    if (value is IDictionary<object, object> dic)
                    {
                        if (dic.Keys.Count == 0)
                        {
                            continue;
                        }
                    }

                    foreach (var item in en)
                    {
                        try
                        {
                            var keyProp = item.GetType().GetProperty("Key");
                            var valueProp = item.GetType().GetProperty("Value");

                            if (keyProp != null && valueProp != null)
                            {
                                isProcessed = true;
                                var val = valueProp.GetValue(item);
                                if (val != null && val.GetType().ToString() != val.ToString() &&
                                    !val.GetType().IsSubclassOf(typeof(Stream)))
                                {
                                    var propName =
                                        prop.Name.StartsWith("RequestHeaders",
                                            StringComparison.InvariantCultureIgnoreCase)
                                            ? "Header_"
                                            : prop.Name + "_";
                                    serverVariables.Add(prefix + propName + keyProp.GetValue(item), val.ToString());
                                }
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                if (isProcessed)
                {
                    continue;
                }

                try
                {
                    if (value != null && value.GetType().ToString() != value.ToString() &&
                        !value.GetType().IsSubclassOf(typeof(Stream)))
                    {
                        serverVariables.Add(prefix + prop.Name, value?.ToString());
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        #endregion Private Methods
    }
}