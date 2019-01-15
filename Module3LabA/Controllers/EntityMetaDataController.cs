using System;
using System.Web.Mvc;
using Module3LabA.Models;

namespace Module3LabA.Controllers
{
    public class EntityMetaDataController : Controller
    {
    

        // GET: EntityMetaData/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EntityMetaData/Create
        [HttpPost]
        public ActionResult Create(CustomEntityMetaData newEntityMetaData)
        {
            try
            {
                var x = new EntityMetaDataUtils();

                // Create New Entity Based on Model Data passed to method
                   x.CreateEntity(newEntityMetaData);
                   Session["EntityName"] = newEntityMetaData.EntityName;


                return RedirectToAction("Update");
            }
            catch(Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View();
            }
        }

        // GET: EntityMetaData/Update/CustomEntity
        public ActionResult Update()
        {
            return View();
        }

        // POST:EntityMetaData/Update/CustomEntity
        [HttpPost]
        public ActionResult Update( AttributeMetaData attributeMetadata)
        {
          
                string entityName = Session["EntityName"].ToString();
                var x = new EntityMetaDataUtils();

                //update entity based on model information 
                x.UpdateEntity(entityName, attributeMetadata);
                TempData["Message"] = "Successfully updated entity " + entityName;
                
                return RedirectToAction("Update");
          
        }

        // GET: EntityMetaData/Delete/5
     
    }
}
