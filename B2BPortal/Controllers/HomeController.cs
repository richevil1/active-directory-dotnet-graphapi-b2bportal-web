using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Net;
using B2BPortal.Models;

namespace B2BPortal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // has the form just been submitted?
            string strFormAction = this.Request["submitButton"];
            if(strFormAction == "userSignUp")
            {
                // collect form data
                string firstName = this.Request["FirstName"];
                string lastName = this.Request["LastName"];
                string displayName = firstName + " " + lastName;
                string emailAddress = this.Request["EmailAddress"];
                string department = this.Request["Department"];
                // create table if it doesn't exist
                const string TABLENAME = "TableRequests";
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable requestTable = tableClient.GetTableReference(TABLENAME);
                requestTable.CreateIfNotExists();
                // add new entity to table (no concurrency checks for now)
                RequestEntity newRequest = new RequestEntity(displayName, emailAddress, department);
                TableOperation insert = TableOperation.InsertOrReplace(newRequest);
                requestTable.Execute(insert);
            }
            ViewBag.Message = "Sign up for access.";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Approve requests.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Edit profile.";
            return View();
        }
    }
}