using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DynamicsLinq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

namespace Module2LabD.Controllers
{
    public class AccountController : Controller
    {
        private string cnString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;

        // GET: Account
        public ActionResult Index()
        {
            using (var crmSvc = new CrmServiceClient(cnString))
            {
                var ctx = new OrgServiceContext(crmSvc);
                var accounts = ctx.AccountSet.OrderBy(acc => acc.Name).Take(10).ToList();
                return View(accounts);
            }
        }

        // GET: Account/Details/5
        public ActionResult Details(int id)
        {
            var acc = new Account();
            return View(acc);
        }

        // GET: Account/Create
        public ActionResult Create(Account newAccount)
        {
            using (var svc = new CrmServiceClient(cnString))
            {
                var ctx = new OrgServiceContext(svc);
                ctx.AddObject(newAccount);
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        // POST: Account/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Account/Edit/5
        public ActionResult Edit(Guid id)
        {
            using (var crmService = new CrmServiceClient(cnString))
            {
                var ctx = new OrgServiceContext(crmService);

                var account = ctx.AccountSet.Where(acc => acc.Id == id).Select(acc => acc);
                return View(account.FirstOrDefault());
            }
        }

        // POST: Account/Edit/5
        [HttpPost]
        public ActionResult Edit(Guid id, Account modifiedAccount)
        {
            using (var crmSvc = new CrmServiceClient(cnString))
            {
                var ctx = new OrgServiceContext(crmSvc);
                ctx.Attach(modifiedAccount);
                ctx.UpdateObject(modifiedAccount);
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        // GET: Account/Delete/5
        public ActionResult Delete(Guid id)
        {
            using (var crmSvc = new CrmServiceClient(cnString))
            {
                var ctx = new OrgServiceContext(crmSvc);
                var account = ctx.AccountSet.Where(acc => acc.Id == id).Select(acc => acc);
                return View(account.FirstOrDefault());
            }
        }

        // POST: Account/Delete/5
        [HttpPost]
        public ActionResult Delete(Account accountToDelete)
        {
            using (var crmSvc = new CrmServiceClient(cnString))
            {
                var ctx = new OrgServiceContext(crmSvc);

                ctx.Attach(accountToDelete);
                ctx.DeleteObject(accountToDelete);
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        public ActionResult Details(Guid id)
        {
            using (var crmSvc = new CrmServiceClient(cnString))
            {
                var ctx = new OrgServiceContext(crmSvc);
                var account = ctx.AccountSet.Where(acc => acc.Id == id).Select(acc => acc);
                return View(account.FirstOrDefault());
            }
        }

        public ActionResult Paging(int id = 0)
        {
            int pageNumber;

            if (id >= 0)
            {
                pageNumber = id;
            }
            else
            {
                pageNumber = 0;
            }

            int pageSize = 5;
            int recordsToSkip = pageSize * pageNumber;

            using (var crmService = new CrmServiceClient(cnString))
            {
                var ctx = new OrgServiceContext(crmService);

                var accounts = ctx.AccountSet.OrderBy(acc => acc.Name).Skip(recordsToSkip).Take(pageSize).ToList();
                ViewBag.nextPageNumber = pageNumber +1;
                ViewBag.previousPageNumber = pageNumber - 1;
                ViewBag.lastPage = GetAccountsCount()/pageSize;
                return View(accounts);
            }
        }

        public int GetAccountsCount()
        {
            string numberOfAccountsFetchXml = @"
<fetch distinct='false' mapping='logical' aggregate='true'>
<entity name='account'>
<attribute name='name' alias='account_count' aggregate='count'/>
</entity>
</fetch>";
            using (var crmService = new CrmServiceClient(cnString))
            {
                var numberOfAccounts = crmService.RetrieveMultiple(new FetchExpression(numberOfAccountsFetchXml));
                if (numberOfAccounts != null)
                {
                    int number = (int) ((AliasedValue) numberOfAccounts[0]["account_count"]).Value;
                    return number;
                }
            }
            return 0;
        }
    }
}
