using CartX.DataAccess.Data;
using CartX.DataAccess.Repository.IRepository;
using CartX.Models;
using CartX.Models.ViewModels;
using CartX.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CartXWeb.Services;

namespace CartXWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStorageService _storageService;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, IStorageService storageService)
        {
            _unitofwork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _storageService = storageService;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitofwork.Product.GetAll(includeProperties: "Category").ToList();
            return View(objProductList);
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitofwork.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                //Create
                return View(productVM);
            }
            else
            {
                //Update
                productVM.Product = _unitofwork.Product.Get(u => u.Id == id, includeProperties: "ProductImages");
                return View(productVM);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Upsert(ProductVM productVM, List<IFormFile>? files)
        {
            if (ModelState.IsValid)
            {
                if (productVM.Product.Id == 0)
                {
                    _unitofwork.Product.Add(productVM.Product);
                }
                else
                {
                    _unitofwork.Product.Update(productVM.Product);
                }
                _unitofwork.Save();

                if (files != null && files.Count > 0)
                {
                    foreach (IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string blobFileName = $"product-{productVM.Product.Id}/{fileName}";

                        // Upload file to storage service (local or Azure based on environment)
                        string imageUrl = await _storageService.UploadFileAsync(blobFileName, file.OpenReadStream());

                        ProductImage productImage = new()
                        {
                            ImageUrl = imageUrl,
                            ProductId = productVM.Product.Id
                        };

                        if (productVM.Product.ProductImages == null)
                        {
                            productVM.Product.ProductImages = new List<ProductImage>();
                        }

                        productVM.Product.ProductImages.Add(productImage);
                    }

                    _unitofwork.Product.Update(productVM.Product);
                    _unitofwork.Save();
                }

                TempData["success"] = "Product Created/Updated Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitofwork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }
        }

        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var imageToBeDeleted = _unitofwork.ProductImage.Get(u => u.Id == imageId);
            int productId = imageToBeDeleted.ProductId;

            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    // Extract blob filename from stored URL
                    // For local: path like "\images\products\product-1\guid.ext"
                    // For Azure: full URL like "https://..."
                    string blobFileName = ExtractBlobFileName(imageToBeDeleted.ImageUrl);
                    await _storageService.DeleteFileAsync(blobFileName);
                }

                _unitofwork.ProductImage.Remove(imageToBeDeleted);
                _unitofwork.Save();

                TempData["success"] = "Deleted Successfully";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitofwork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var productToBeDeleted = _unitofwork.Product.Get(u => u.Id == id, includeProperties: "ProductImages");
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }

            // Delete all associated images from storage
            if (productToBeDeleted.ProductImages != null && productToBeDeleted.ProductImages.Count > 0)
            {
                foreach (var image in productToBeDeleted.ProductImages)
                {
                    if (!string.IsNullOrEmpty(image.ImageUrl))
                    {
                        string blobFileName = ExtractBlobFileName(image.ImageUrl);
                        await _storageService.DeleteFileAsync(blobFileName);
                    }
                }
            }

            _unitofwork.Product.Remove(productToBeDeleted);
            _unitofwork.Save();
            return Json(new { success = true, message = "Delete Successful!" });
        }
        #endregion

        #region HELPER METHODS
        private string ExtractBlobFileName(string imageUrl)
        {
            // For local storage: path like "\images\products\product-1\guid.ext"
            if (imageUrl.StartsWith("\\"))
            {
                return imageUrl.TrimStart('\\');
            }

            // For Azure blob storage: full URL like "https://accountname.blob.core.windows.net/container/product-1/guid.ext"
            // Extract the blob name (product-1/guid.ext)
            Uri uri = new Uri(imageUrl);
            string blobPath = uri.AbsolutePath;
            // Remove leading slash and container name
            var segments = blobPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2)
            {
                // Join last segments to get "product-{id}/filename"
                return string.Join('/', segments.Skip(1));
            }

            return imageUrl;
        }
        #endregion
    }
}
