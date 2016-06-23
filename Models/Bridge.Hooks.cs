using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;


namespace project.REST
{
    public static partial class Bridge
    {
        public static List<Hook> ReadHooks(Guid transactionId)
        {
           // byte[] content = ZipformAPI.GetVaultDriver().ReadSystemFile(Utils.Guid2String(transactionId), "hooks");
            //if (content != null)
            //{
            //    string data = Encoding.UTF8.GetString(content);
            //    return JsonConvert.DeserializeObject<List<Hook>>(data);
            //}
            //return new List<Hook>();
            return HookDataObject.ReadHooks(transactionId);


        }

        public static Response DeleteHook(int hookId)
        {
            // List<Hook> li = ReadHooks(transactionId);
            //li.RemoveAll(h => h.WebHookId == hookId);
            //return ZipformAPI.GetVaultDriver().StoreSystemFile(Utils.Guid2String(transactionId), "hooks",
            //       Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(li))).Length == 0;

            return HookDataObject.DeleteHook(hookId);
        }

        public static Response UpdateHook(Hook hook)
        {
            return HookDataObject.UpdateHook(hook);
        }

        public static Response StoreHook(Hook hook)
        {
            return HookDataObject.SaveHook(hook);
        }
    }

    public class HookDataObject
    {
        public enum SignatureProvider { Digitallink, Docusign, TouchSign };

        public enum SaveHookOption { CreateNewHook, UpdateHook, ActivateHook, DeactivateHook }

        public static Response SaveHook(Hook hook)
        {
            try
            {
                List<SqlParameter> p = new List<SqlParameter>();
                p.Add(new SqlParameter("@inWebHookEventID", hook.WebHookEventID));
                p.Add(new SqlParameter("@inWebHookStatus", hook.WebHookStatus));
                p.Add(new SqlParameter("@inWebHookAppId", hook.WebHookAppId));
                p.Add(new SqlParameter("@inWebHookUrl", hook.WebHookUrl));
                //1 for transaction, 2 for location and 3 for member type
                p.Add(new SqlParameter("@inWebHookScopeTypeID", hook.WebHookScopeTypeID > 0 ? hook.WebHookScopeTypeID : 1));

                //transactionID for transaction, locationID for location and memberID for member type
                p.Add(new SqlParameter("@inWebHookScopeId", hook.WebHookScopeId));

                //('ANY', 'DigitalInk', 'Docusign', 'TouchSign')
                p.Add(new SqlParameter("@inWebHookProviderName", hook.WebHookProviderName));

                return DataManager.GetInstance().ExecuteNonQuery("dbo.usp_zfo_ins_WebHook", p);

            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }



        public static Response UpdateHook(Hook hook)

        {
            try
            {
                DataManager DM = DataManager.GetInstance();

                List<SqlParameter> p = new List<SqlParameter>();
                p.Add(new SqlParameter("@inWebHookID", hook.WebHookId));
                p.Add(new SqlParameter("@inWebHookEventID", hook.WebHookEventID));
                p.Add(new SqlParameter("@inWebHookStatus", hook.WebHookStatus));
                p.Add(new SqlParameter("@inWebHookAppId", hook.WebHookAppId));
                p.Add(new SqlParameter("@inWebHookUrl", hook.WebHookUrl));
                //1 for transaction, 2 for location and 3 for member type
                p.Add(new SqlParameter("@inWebHookScopeTypeID", hook.WebHookScopeTypeID > 0 ? hook.WebHookScopeTypeID : 1));

                //transactionID for transaction, locationID for location and memberID for member type
                p.Add(new SqlParameter("@inWebHookScopeId", hook.WebHookScopeId));

                //('ANY', 'DigitalInk', 'Docusign', 'TouchSign')
                p.Add(new SqlParameter("@inWebHookProviderName", hook.WebHookProviderName));

               
                return DM.ExecuteNonQuery("ZFFW_Master.dbo.usp_zfo_upd_WebHookStatusUpdate", p);

            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        public static Response DeleteHook(int hookid)
        {
            try
            {
                DataManager DM = DataManager.GetInstance();

                List<SqlParameter> p = new List<SqlParameter>();
                p.Add(new SqlParameter("@inWebHookID", hookid));

                return DM.ExecuteNonQuery("ZFFW_Master.dbo.usp_zfo_del_WebHookDelete", p);

            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        public static List<Hook> ReadHooks(Guid transactionid)
        {
            List<Hook> list = new List<Hook>();

            List<SqlParameter> p = new List<SqlParameter>();
            p.Add(new SqlParameter("@inTransactionID", transactionid));

            DataManager DM = DataManager.GetInstance();


            try
            {
                DataTable dt = DM.ExecuteQuery("ZFFW_Master.dbo.usp_zfo_sel_WebHookList", p).Result.Object as DataTable;

                foreach (DataRow dr in dt.Rows)
                {
                    Hook hook = new Hook();
                    // hook.TransactionID = DM.ReadGuidColumn(dr,"")
                    hook.WebHookAppId = DM.ReadColumn(dr, "szWebHookAppId");
                    hook.WebHookId = DM.ReadIntColumn(dr, "szWebHookId");
                    hook.WebHookProviderName = DM.ReadColumn(dr, "szWebHookProviderName");
                    hook.WebHookScopeId = DM.ReadColumn(dr, "szWebHookScopeId");
                    hook.WebHookScopeTypeID = DM.ReadIntColumn(dr, "szWebHookScopeTypeID");
                    hook.WebHookStatus = DM.ReadBitColumn(dr, "szWebHookStatus");
                    hook.WebHookUrl = DM.ReadColumn(dr, "szWebHookUrl");
                    hook.WebHookEventID = DM.ReadIntColumn(dr, "szWebHookEventId");
                    list.Add(hook);
                }
                return list;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public class HookList : List<Hook>
        {
        }
    }
}
