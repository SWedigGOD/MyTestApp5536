using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTestApp5536.Data;
using MyTestApp5536.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace MyTestApp5536.Controllers {
    public class TestModelsController : Controller {
        private readonly MyTestApp5536Context _context;
        private static List<string> Filenames = new List<string>();

        public TestModelsController(MyTestApp5536Context context) {
            _context = context;
        }

        // GET: TestModels
        public async Task<IActionResult> Index() {
            Filenames.Clear();
            return View(await _context.TestModel.ToListAsync());
        }

        // GET: TestModels/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var testModel = await _context.TestModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (testModel == null) {
                return NotFound();
            }
            await setFilenames(testModel);
            return View(testModel);
        }
        private async Task setFilenames(TestModel testModel) {
            var container = getContainer(testModel);
            testModel.Filenames = container.GetBlobs().Where(x => x.Name.StartsWith(testModel.Id + "/")).Select(x => x.Name.Replace(testModel.Id + "/", "")).ToList();
        }

        // GET: TestModels/Create
        public IActionResult Create() {
            return View();
        }

        // POST: TestModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,ID")] TestModel testModel) {
            if (ModelState.IsValid) {
                _context.Add(testModel);
                await _context.SaveChangesAsync();
                await uploadFiles(testModel);
                return RedirectToAction(nameof(Index));
            }
            return View(testModel);
        }
        private async Task uploadFiles(TestModel testModel) {
            var container = getContainer(testModel);

            foreach (var filename in Filenames) {
                FileStream s = new FileStream(filename, FileMode.Open);
                await container.UploadBlobAsync(testModel.Id + "/" + filename, s);
                s.Close();
                System.IO.File.Delete(filename);
            }

            Filenames.Clear();
        }

        // GET: TestModels/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var testModel = await _context.TestModel.FindAsync(id);
            if (testModel == null) {
                return NotFound();
            }
            await setFilenames(testModel);
            return View(testModel);
        }

        // POST: TestModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Id")] TestModel testModel) {
            if (id != testModel.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(testModel);
                    await _context.SaveChangesAsync();
                    await uploadFiles(testModel);
                } catch (DbUpdateConcurrencyException) {
                    if (!TestModelExists(testModel.Id)) {
                        return NotFound();
                    } else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(testModel);
        }
        private BlobContainerClient getContainer(TestModel testModel) {
            BlobContainerClient container = new BlobContainerClient(Config.StorageACConStr, "filecontainer");
            //await container.CreateIfNotExistsAsync();
            return container;
        }
        
        public async Task<IActionResult> DeleteFile(int? id, string filename) {
            if (id == null) {
                return NotFound();
            }

            var testModel = await _context.TestModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (testModel == null) {
                return NotFound();
            }
            testModel.Filenames.Remove(filename);
            var container = getContainer(testModel);
            container.DeleteBlob(filename);
            return View("Edit", testModel);
        }
        // GET: TestModels/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var testModel = await _context.TestModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (testModel == null) {
                return NotFound();
            }
            await setFilenames(testModel);
            return View(testModel);
        }

        // POST: TestModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var testModel = await _context.TestModel.FindAsync(id);
            _context.TestModel.Remove(testModel);
            await _context.SaveChangesAsync();
            var container = getContainer(testModel);
            await setFilenames(testModel);
            //await container.DeleteAsync();
            foreach (var file in testModel.Filenames) {
                container.DeleteBlob(testModel.Id + "/" + file);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TestModelExists(int id) {
            return _context.TestModel.Any(e => e.Id == id);
        }
        [HttpPost, ActionName("AddImage")]
        public async Task<IActionResult> AddImage(ICollection<IFormFile> files) {
            if (files == null || files.Count == 0)
                return BadRequest("No files received from the upload");
            foreach (var file in files) {
                var filePath = file.FileName;//Path.GetTempFileName();
                using (var stream = System.IO.File.Create(filePath)) {
                    await file.CopyToAsync(stream);
                }
                //Files.Add(file.OpenReadStream());
                Filenames.Add(file.FileName);
                //files.FirstOrDefault().CopyToAsync
            }
            //Files.AddRange(files);
            return new AcceptedResult();
            //return new AcceptedAtActionResult("GetThumbNails", "TestModels", null, null);
            /*bool isUploaded = false;

            try {
                if (files.Count == 0)
                    return BadRequest("No files received from the upload");

                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)
                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                foreach (var formFile in files) {
                    if (StorageHelper.IsImage(formFile)) {
                        if (formFile.Length > 0) {
                            using (Stream stream = formFile.OpenReadStream()) {
                                isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig);
                            }
                        }
                    } else {
                        return new UnsupportedMediaTypeResult();
                    }
                }

                if (isUploaded) {
                    if (storageConfig.ThumbnailContainer != string.Empty)
                        return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);
                    else
                        return new AcceptedResult();
                } else
                    return BadRequest("Look like the image couldnt upload to the storage");
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }*/
        }


        // GET /api/images/thumbnails
        [HttpGet, ActionName("thumbnails")]
        public async Task<IActionResult> GetThumbNails() {
            try {
                return new ObjectResult(Filenames);
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }
    }
}
