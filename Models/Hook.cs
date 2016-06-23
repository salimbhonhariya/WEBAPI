using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using ZFramework.ZipformObjects;
using System.ComponentModel.DataAnnotations;

namespace project.REST
{

    public sealed class Hook
    {
        [JsonProperty("WebHookId")]
        public int WebHookId { get; set; }



        [JsonProperty("WebHookEventID")]
        public int WebHookEventID { get; set; }



        [JsonProperty("WebHookStatus")]
        public Boolean WebHookStatus { get; set; }



        [JsonProperty("WebHookAppId")]
        public string WebHookAppId { get; set; }



        [JsonProperty("WebHookUrl")]
        public string WebHookUrl { get; set; }



        [JsonProperty("WebHookScopeTypeID")]
        public int WebHookScopeTypeID { get; set; }



        [JsonProperty("WebHookScopeId")]
        public string WebHookScopeId { get; set; }



        [JsonProperty("WebHookProviderName")]
        public string WebHookProviderName { get; set; }



        [JsonProperty("TransactionID")]
        public int TransactionID { get; set; }


        //[JsonProperty("status")]
        //public string Status { get; set; }

        //[JsonProperty("outstatus")]
        //public string outStatus { get; set; }

     


        //[JsonProperty("id")]
        //public int ID;

        //[JsonProperty("appId")]
        //[Required]
        //public string AppId { get; set; }

        //[JsonProperty("name")]
        //public string Name { get; set; }

        //[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        //[Required]
        //public string Status { get; set; }
        //[Required]
        //[JsonProperty("transactionId", NullValueHandling = NullValueHandling.Ignore)]
        //public Guid TransactionId { get; set; }

        ////[JsonProperty("eventnameHookissubscribedfor", NullValueHandling = NullValueHandling.Ignore)]
        ////public string eventnameHookissubscribedfor { get; set; }

        //[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        //public string URL { get; set; }

        //public static Hook Convert(ZFramework.ZipformObjects.Hook t)
        //{
        //    var status = t.Status.ToLower();
        //    var hook = new Hook
        //    {
        //        AppID = t.AppID,
        //        contextID = t.contextID,
        //        transactionId = t.TransactionId,
        //        Name = t.Name,
        //        ID = t.ID,
        //        webhookURL = t.WebhookURL,
        //        signprovider = t.signatureProvider,
        //        Status = t.Status
        //    };
        //    return hook;
        //}



        /// <summary>
        /// Implicitly converts a transaction to it's ID value. This allows Transaction objects to be used in methods
        /// that accept the transaction ID as a parameter.
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        //public static implicit operator int(Hook hook)
        //{
        //    if (hook == null)
        //        throw new ArgumentNullException("hook");
        //    return hook.ID;
        //}

        //public bool Equals(Hook other)
        //{
        //    //TODO: DateTime properties do not seem to match. Follow-up required.
        //    return other.TransactionId == TransactionId &&
        //           other.contextID == contextID &&
        //           other.signatureProvider == signatureProvider &&
        //           other.WebhookURL == WebhookURL &&
        //           other.AppID == AppID;
        //}


    }
}
