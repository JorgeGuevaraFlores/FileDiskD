using Microsoft.Ajax.Utilities;
using PL.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace PL.Controllers
{
    public class FileController : Controller
    {
        private readonly FileService _fileService;

        public FileController()
        {
            _fileService = new FileService();
        }

        public ActionResult Index(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = @"D:\"; // Ruta por defecto si no se especifica ninguna
            }

            var entries = _fileService.GetFileSystemEntries(path);
            ViewBag.Entries = entries;
            ViewBag.Path = path;

            return View();
        }

        public ActionResult StreamFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return HttpNotFound();

            string mimeType;
            string extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".mp4":
                    mimeType = "video/mp4";
                    break;
                case ".webm":
                    mimeType = "video/webm";
                    break;
                case ".jpg":
                case ".jpeg":
                    mimeType = "image/jpeg";
                    break;
                case ".png":
                    mimeType = "image/png";
                    break;
                case ".gif":
                    mimeType = "image/gif";
                    break;
                default:
                    return new HttpStatusCodeResult(415); // Unsupported Media Type
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var rangeHeader = Request.Headers["Range"];

            if (string.IsNullOrEmpty(rangeHeader))
            {
                return File(fileStream, mimeType);
            }

            var totalLength = fileStream.Length;
            var range = rangeHeader.Replace("bytes=", "").Split('-');
            var start = long.Parse(range[0]);
            var end = range.Length > 1 && !string.IsNullOrEmpty(range[1]) ? long.Parse(range[1]) : totalLength - 1;
            var length = end - start + 1;

            Response.StatusCode = (int)HttpStatusCode.PartialContent;
            Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{totalLength}");
            Response.Headers.Add("Accept-Ranges", "bytes");
            Response.Headers.Add("Content-Length", length.ToString());

            fileStream.Position = start;
            var buffer = new byte[length];
            fileStream.Read(buffer, 0, buffer.Length);

            return File(new MemoryStream(buffer), mimeType);
        }

        [HttpPost]
        public ActionResult UploadFiles(IEnumerable<HttpPostedFileBase> files, string path)
        {
            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var fullPath = Path.Combine(path, fileName);

                    // Asegúrate de que el directorio exista
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    file.SaveAs(fullPath);
                }
               
            }
            return RedirectToAction("Index", new { path = path });
        }

        [HttpPost]
        public ActionResult DeleteSelectedFiles(string[] selectedFiles, string path)
        {
            if (selectedFiles != null)
            {
                foreach (var file in selectedFiles)
                {
                    System.IO.File.Delete(file);
                }
            }
            return RedirectToAction("Index", new { path = path });
        }


    }
}