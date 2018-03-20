using FacturadorTaller.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FacturadorTaller.Controllers
{
    public class ProductoController : Controller
    {
        private readonly ApplicationDBContext DB;

        public ProductoController()
        {
            DB = new ApplicationDBContext();
        }
        // GET: Cliente
        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrWhiteSpace(sortOrder) ? "NombreProducto_desc" : "";
            ViewBag.ProductoSortParm = sortOrder == "Categoria" ? "cat_desc" : "Categoria";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            var producto = from s in DB.Producto
                          select s;
            if (!string.IsNullOrEmpty(searchString))
            {
                producto = producto.Where(c => c.NombreProducto.Contains(searchString)
                                                     || c.Categoria.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "NombreProducto_desc":
                    producto = producto.OrderByDescending(c => c.NombreProducto);
                    break;
                case "Categoria":
                    producto = producto.OrderBy(c => c.Categoria);
                    break;
                case "cat_desc":
                    producto = producto.OrderByDescending(p => p.Categoria);
                    break;
                default:
                    producto = producto.OrderBy(c => c.NombreProducto);
                    break;
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(producto.ToPagedList(pageNumber, pageSize));
        }

        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Producto producto = DB.Producto.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }
            return View(producto);
        }

        // GET : Clientes/Create
        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Cliente/Create
        [Authorize(Roles = "Admin, Usuario")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Producto prod)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var nomPro = prod.NombreProducto;
                    var cat  = prod.Categoria;
                    var preciop = prod.Precio;

                    var file = new Producto();
                    file.NombreProducto = nomPro;
                    file.Categoria = cat;
                    file.Precio = preciop;
                    DB.Producto.Add(file);
                    DB.SaveChanges();

                    TempData["Status"] = "Upload successful";
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException  /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(prod);
        }

        // GET: Cliente/Edit/5
        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var producto = new Producto();
            producto = DB.Producto.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }
            return View(producto);
        }
        [Authorize(Roles = "Admin, Usuario")]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id, Producto mod)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                if (ModelState.IsValid)
                {
                    var nomPro = mod.NombreProducto;
                    var precioP = mod.Precio;
                    var cat = mod.Categoria;

                    Producto producto = DB.Producto.Find(id);
                    producto.NombreProducto = nomPro;
                    producto.Precio = precioP;
                    producto.Categoria = cat;
                    DB.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException  /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            return View(mod);
        }

        // GET: Cliente /Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }
            Producto producto = DB.Producto.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }
            return View(producto);
        }

        // POST: producto/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Producto producto = DB.Producto.Find(id);
                DB.Producto.Remove(producto);
                DB.SaveChanges();
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }
            return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DB.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}