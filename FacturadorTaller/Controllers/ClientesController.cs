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
    public class ClientesController : Controller
    {
        private readonly ApplicationDBContext DB;

        public ClientesController()
        {
            DB = new ApplicationDBContext();
        }
        // GET: Cliente
        //[Authorize(Roles = "Admin, Usuario")]
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrWhiteSpace(sortOrder) ? "NombreCliente_desc" : "";
            ViewBag.ProductoSortParm = sortOrder == "RncCliente" ? "Rnc_desc" : "RncCliente";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            var cliente = from s in DB.Clientes
                          select s;
            if (!string.IsNullOrEmpty(searchString))
            {
                cliente = cliente.Where(c => c.NombreCliente.Contains(searchString)
                                                     || c.RncCliente.Contains(searchString)
                                                     || c.ContactoCliente.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "Nombrecliente_desc":
                    cliente = cliente.OrderByDescending(c => c.NombreCliente);
                    break;
                case "RncCliente":
                    cliente = cliente.OrderBy(c => c.RncCliente);
                    break;
                case "Rnc_desc":
                    cliente = cliente.OrderByDescending(p => p.RncCliente);
                    break;
                default:
                    cliente = cliente.OrderBy(c => c.NombreCliente);
                    break;
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(cliente.ToPagedList(pageNumber, pageSize));
        }

        //[Authorize(Roles = "Admin, Usuario")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Clientes clientes = DB.Clientes.Find(id);
            if (clientes == null)
            {
                return HttpNotFound();
            }
            return View(clientes);
        }

        // GET : Clientes/Create
        //[Authorize(Roles = "Admin, Usuario")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Cliente/Create
        //[Authorize(Roles = "Admin, Usuario")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Clientes cli)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var nomCli = cli.NombreCliente;
                    var rncCli = cli.RncCliente;
                    var dirCli = cli.DireccionCliente;
                    var correo = cli.CorreoCliente;
                    var tel = cli.TelefonoCliente;
                    var conCli = cli.ContactoCliente;

                    var file = new Clientes();
                    file.NombreCliente = nomCli;
                    file.DireccionCliente = dirCli;
                    file.CorreoCliente = correo;
                    file.TelefonoCliente = tel;
                    file.RncCliente = rncCli;
                    file.ContactoCliente = conCli;
                    DB.Clientes.Add(file);
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
            return View(cli);
        }

        // GET: Cliente/Edit/5
        //[Authorize(Roles = "Admin, Usuario")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var clientes = new Clientes();
            clientes = DB.Clientes.Find(id);
            if (clientes == null)
            {
                return HttpNotFound();
            }
            return View(clientes);
        }
        //[Authorize(Roles = "Admin, Usuario")]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id, Clientes mod)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                if (ModelState.IsValid)
                {
                    var nomCli = mod.NombreCliente;
                    var rncCli = mod.RncCliente;
                    var dirCli = mod.DireccionCliente;
                    var correo = mod.CorreoCliente;
                    var conCli = mod.ContactoCliente;
                    var tel = mod.TelefonoCliente;

                    Clientes cliente = DB.Clientes.Find(id);
                    cliente.NombreCliente = nomCli;
                    cliente.RncCliente = rncCli;
                    cliente.DireccionCliente = dirCli;
                    cliente.CorreoCliente = correo;
                    cliente.TelefonoCliente = tel;
                    cliente.ContactoCliente = conCli;
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
        //[Authorize(Roles = "Admin")]
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
            Clientes cliente = DB.Clientes.Find(id);
            if (cliente == null)
            {
                return HttpNotFound();
            }
            return View(cliente);
        }

        // POST: producto/Delete/5
        //[Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Clientes cliente = DB.Clientes.Find(id);
                DB.Clientes.Remove(cliente);
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