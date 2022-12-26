using System;
using System.IO;
using System.Web;
using System.Data;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Collections.Generic;
using FileUploadDownload.Models;

namespace FileUploadDownload.Controllers
{
    public class FilesController : Controller
    {
        string conString = "Data Source=.;Initial Catalog =TEST; integrated security=true;";

        // GET: Files  
        public ActionResult Index(FileUpload model)
        {
            List<FileUpload> list = new List<FileUpload>();
            DataTable dtFiles = GetFileDetails();
            foreach (DataRow dr in dtFiles.Rows)
            {
                list.Add(new FileUpload
                {
                    FileId = @dr["SQLID"].ToString(),
                    FileName = @dr["FILENAME"].ToString(),
                    FileUrl = @dr["FILEURL"].ToString()
                });
            }
            model.FileList = list;
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase files)
        {
            FileUpload model = new FileUpload();
            List<FileUpload> list = new List<FileUpload>();
            DataTable dtFiles = GetFileDetails();
            foreach (DataRow dr in dtFiles.Rows)
            {
                list.Add(new FileUpload
                {
                    FileId = @dr["SQLID"].ToString(),
                    FileName = @dr["FILENAME"].ToString(),
                    FileUrl = @dr["FILEURL"].ToString()
                });
            }
            model.FileList = list;

            if (files != null)
            {
                var Extension = Path.GetExtension(files.FileName);
                var fileName = "my-file-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Extension;
                string path = Path.Combine(Server.MapPath("~/UploadedFiles"), fileName);
                model.FileUrl = Url.Content(Path.Combine("~/UploadedFiles/", fileName));
                model.FileName = fileName;

                if (files.ContentLength < 1048577) //dosya boyutu 1048576 byte yani 1 mb dan küçük olmalı
                {
                    if (SaveFile(model))
                    {
                        files.SaveAs(path);
                        TempData["AlertMessage"] = "Başarılı bir şekilde yüklendi";
                        return RedirectToAction("Index", "Files");
                    }
                    else
                    {
                        TempData["AlertMessage"] = "HATA! Yüklenirken bir hata oluştu, lütfen tekrar deneyin !!!";
                    }
                }
                else
                {
                    //ModelState.AddModelError("", "Dosya boyutu en fazla 1 MB olabilir");
                    TempData["AlertMessage"] = "HATA! Dosya boyutu en fazla 1 MB olabilir !!";
                    return RedirectToAction("Index", "Files");

                }
            }
            else
            {
                ModelState.AddModelError("", "Lütfen doğru dosya tipini seçin!!");
                return View(model);
            }
            return RedirectToAction("Index", "Files");
        }

        private DataTable GetFileDetails()
        {
            DataTable dtData = new DataTable();
            SqlConnection con = new SqlConnection(conString);
            con.Open();
            SqlCommand command = new SqlCommand("Select * From tblFileDetails", con);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dtData);
            con.Close();
            return dtData;
        }

        private bool SaveFile(FileUpload model)
        {
            string strQry = "INSERT INTO tblFileDetails (FILENAME,FILEURL) VALUES('" +
                model.FileName + "','" + model.FileUrl + "')";
            SqlConnection con = new SqlConnection(conString);
            con.Open();
            SqlCommand command = new SqlCommand(strQry, con);
            int numResult = command.ExecuteNonQuery();
            con.Close();
            if (numResult > 0 )
                return true;
            else
                return false;
        }

        public ActionResult DownloadFile(string filePath)
        {
            string fullName = Server.MapPath("~" + filePath);

            byte[] fileBytes = GetFile(fullName);
            return File(
                fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filePath);
        }

        byte[] GetFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new System.IO.IOException(s);
            return data;
        }

        
        public ActionResult Delete(int id)
        {
            
            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                string query = "delete from tblFileDetails where SQLID=@p1";
                SqlCommand command = new SqlCommand(query,con);
                command.Parameters.AddWithValue("@p1", id);
                command.ExecuteNonQuery();
                con.Close();
                return RedirectToAction("Index");
            }

        }
    }
}