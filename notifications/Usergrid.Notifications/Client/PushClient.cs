﻿/*
 *
 *  * Licensed to the Apache Software Foundation (ASF) under one or more
 *  *  contributor license agreements.  The ASF licenses this file to You
 *  * under the Apache License, Version 2.0 (the "License"); you may not
 *  * use this file except in compliance with the License.
 *  * You may obtain a copy of the License at
 *  *
 *  *     http://www.apache.org/licenses/LICENSE-2.0
 *  *
 *  * Unless required by applicable law or agreed to in writing, software
 *  * distributed under the License is distributed on an "AS IS" BASIS,
 *  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  * See the License for the specific language governing permissions and
 *  * limitations under the License.  For additional information regarding
 *  * copyright in this work, please see the NOTICE file in the top level
 *  * directory of this distribution.
 *
 */
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using Windows.Storage;

namespace Usergrid.Notifications.Client
{
    public class PushClient
    {
        const string DEVICE_KEY = "currentDeviceId";

        private ApigeeClient apigeeClient;
        private ApplicationDataContainer settings;
        private PushNotificationChannel channel;

        private string userId;
        public string notifierName;
        public Guid deviceID;
        public Exception lastException;

        public PushClient(ApigeeClient apigeeClient, string userId, string notifierName)
        {
            this.apigeeClient = apigeeClient;
            this.settings = ApplicationData.Current.LocalSettings;
            this.notifierName = notifierName;
            this.userId = userId;
            this.init().ContinueWith(t =>
                  {
                      this.lastException = t.Exception;
                  }  
                );
        }


        public async Task<bool> SendToast(string message)
        {
            if (this.deviceID == null)
            {
                throw new Exception("Please call PushClient.RegisterDevice first.");
            }

            var jsonObject = new JObject();
            var payloads = new JObject();
            var payload = new JObject();
            payload.Add("toast", new JValue(message));
            payloads.Add(this.notifierName, payload);
            jsonObject.Add("payloads", payloads);
            jsonObject.Add("debug", true);
            var jsonResponse = await this.apigeeClient.SendAsync(HttpMethod.Post, String.Format("users/{1}/devices/{0}/notifications", this.deviceID,userId), jsonObject);
            return jsonResponse.StatusIsOk;
        }

        public async Task<bool> SendBadge<T>(T message)
        {
            if (this.deviceID == null)
            {
                throw new Exception("Please call PushClient.RegisterDevice first.");
            }

            var jsonObject = new JObject();
            var payloads = new JObject();
            var payload = new JObject();
            payload.Add("badge", new JValue(message));
            payloads.Add(this.notifierName, payload);
            jsonObject.Add("payloads", payloads);
            jsonObject.Add("debug", true);
            var jsonResponse = await this.apigeeClient.SendAsync(HttpMethod.Post, String.Format("users/{1}/devices/{0}/notifications", this.deviceID,userId), jsonObject);
            return jsonResponse.StatusIsOk;
        }

        private async Task init()
        {
            channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync().AsTask<PushNotificationChannel>();
            if (settings.Values[DEVICE_KEY] == null)
            {
                Guid uuid = await registerDevice(true);
                settings.Values.Add(DEVICE_KEY, uuid);
                this.deviceID = uuid;
            }
            else
            {
                object tempId;
                settings.Values.TryGetValue(DEVICE_KEY, out tempId);
                this.deviceID = Guid.Parse(tempId.ToString());
                var device = await GetDevice(this.deviceID);
                if (device == null)
                {
                    Guid uuid = await registerDevice(true);
                    settings.Values[DEVICE_KEY] = uuid;
                    this.deviceID = uuid;
                }
                else
                {
                    await registerDevice(false);
                }
            }
        }

       
        private async Task<JToken> GetDevice(Guid deviceId)
        {
            var jsonResponse = await apigeeClient.SendAsync(HttpMethod.Get, "users/"+userId+"/devices/" + deviceId, null);

            if (jsonResponse.StatusIsOk)
            {
                var body = jsonResponse.GetValue("entities");
                return body != null &&  body.Value<JArray>().Count > 0 ? body.Value<JArray>()[0] : null;
            }
            else { return null; }
        }

        private async Task<Guid> registerDevice(bool isNew)
        {
            JObject obj = new JObject();
            obj.Add(this.notifierName + ".notifier.id", new JValue(channel.Uri));
            var jsonResponse = await this.apigeeClient.SendAsync(
                (isNew ? HttpMethod.Post : HttpMethod.Put), 
                "users/"+userId+"/devices/" +   (isNew ? "" : this.deviceID.ToString()), 
                obj
                );
            
            if (jsonResponse.StatusIsOk)
            {
                var entity = jsonResponse.GetValue("entities").Value<JArray>()[0];
                var uuid = Guid.Parse(entity.Value<String>("uuid"));
                return uuid;
            }
            else { 
                return Guid.Empty; 
            }
        }
    }
}
