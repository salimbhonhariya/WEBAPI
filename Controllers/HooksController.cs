
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ZipFormRest.Code;
using ZipFormRest.Models;
using ZFramework;


using project.REST;

namespace ZipFormRest.Controllers
{
    [Authorize]
    public class HooksController : ZipFormsApiController
    {
        public HooksController()
        {
        }

        public HooksController(Guid transactionid) : base(transactionid)
        {
        }

        [AcceptVerbs("GET")]
        [Authorize(Roles = MethodAuthNames.GetHooksList)]
  
        public HttpResponseMessage Get(Guid transactionId)
        {
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

            var transactionData = Bridge.ReadHooks(transactionId);
            return transactionData != null ? Request.CreateResponse(HttpStatusCode.OK, transactionData) : Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("POST", "PUT")]
        [ValidateModel]
        [Authorize(Roles = MethodAuthNames.AddOrUpdateHook)]

        public HttpResponseMessage AddOrUpdateHook(Hook hook)
        {
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

            if (hook == null || hook.WebHookScopeId == string.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            if (!ping(hook.WebHookUrl))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden, "Failed reaching WebHook URL.");
            }

            Response r = null;
            if (hook.WebHookId > 0)
            {
                // Update hook
                r = Bridge.UpdateHook(hook);
            }
            else
            {
                // Create hook
                r = Bridge.StoreHook(hook);
            }
            if (!r.Result.Bit)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, r.Message);
            }
            return Request.CreateResponse(HttpStatusCode.Created);
        }

        public HttpResponseMessage Delete(int hookId)
        {
            Response r = null;
            r = Bridge.DeleteHook(hookId);
            if (!r.Result.Bit)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, r.Message);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [NonAction]
        bool ping(string url)
        {
            bool status = false;
            HttpWebResponse rs = null;
            try
            {
                var rq = (HttpWebRequest)WebRequest.Create(url);
                rq.Timeout = 15000;
                rq.Method = "HEAD";
                rq.KeepAlive = false;
                rq.UserAgent = "Mozilla/5.0 Gecko Firefox";
                rs = (HttpWebResponse)rq.GetResponse();
                if (rs.StatusCode == HttpStatusCode.OK)
                    status = true;
            }
            catch (Exception ex)
            {
            }
            if (rs != null)
            {
                rs.Close();
            }
            return status;

        }
    }
    //If model validation fails, this filter returns an HTTP response that contains the validation errors. In that case, the controller action is not invoked
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ModelState.IsValid == false)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, actionContext.ModelState);
            }

            //if (!actionContext.ModelState.IsValid)
            //{
            //    foreach (var modelStateVal in actionContext.ModelState.Values)
            //    {
            //        foreach (var error in modelStateVal.Errors)
            //        {
            //            var errorMessage = error.ErrorMessage;
            //            var exception = error.Exception;
            //            // You may log the errors if you want
            //        }
            //    }
            //}
        }
    }







}
