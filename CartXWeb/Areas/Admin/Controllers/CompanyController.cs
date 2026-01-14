using CartX.DataAccess.Data;
using CartX.DataAccess.Repository.IRepository;
using CartX.Models;
using CartX.Models.ViewModels;
using CartX.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CartXWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitofwork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitofwork.Company.GetAll().ToList();
            return View(objCompanyList);
        }
        public IActionResult Upsert(int? id)
        {
            if(id==null || id==0)
            {
                //Create
                return View(new Company());
            }
            else
            {
                //Update
                Company companyObj = _unitofwork.Company.Get(u => u.Id == id);
                return View(companyObj);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company CompanyObj)
        {
            if (ModelState.IsValid)
            {
                if(CompanyObj.Id==0)
                {
                    _unitofwork.Company.Add(CompanyObj);
                }
                else
                {
                    _unitofwork.Company.Update(CompanyObj);
                }
                _unitofwork.Save();
                TempData["success"] = "Company Created Successfully"; 
                return RedirectToAction("Index");
            }
            else
            {
                return View(CompanyObj);
            }
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var objCompanyList = _unitofwork.Company.GetAll();
            return Json(new { data = objCompanyList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var CompanyToBeDeleted = _unitofwork.Company.Get(u => u.Id == id);
            if(CompanyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }
            _unitofwork.Company.Remove(CompanyToBeDeleted);
            _unitofwork.Save();
            return Json(new { success = true, message = "Delete Successful!" });
        }
        #endregion
    }
}
