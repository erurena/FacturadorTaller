using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FacturadorTaller.Models;
using FacturadorTaller.ViewModel;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PagedList;

namespace FacturadorTaller.Controllers
{
    public class AdministracionController : Controller
    {
        private readonly ApplicationDBContext DB;
        public AdministracionController()
        {
            DB = new ApplicationDBContext();
        }
        // GET: Administracion
        //[Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View();
        }

        // GET: Cliente
        //[Authorize(Roles = "Admin")]
        public ActionResult IndexNcf(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrWhiteSpace(sortOrder) ? "Inicio_desc" : "";
            ViewBag.ProductoSortParm = sortOrder == "NumInicio" ? "NumInicio_desc" : "NumInicio";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            var ncf = from s in DB.Ncf
                          select s;
            if (!string.IsNullOrEmpty(searchString))
            {
                ncf = ncf.Where(c => c.Inicio.Contains(searchString)
                                                     || c.NumInicio.Equals(searchString)
                                                     || c.NumFin.Equals(searchString));
            }

            switch (sortOrder)
            {
                case "Inicio_desc":
                    ncf = ncf.OrderByDescending(c => c.Inicio);
                    break;
                case "NumInicio":
                    ncf = ncf.OrderBy(c => c.NumInicio);
                    break;
                case "NumInicio_desc":
                    ncf = ncf.OrderByDescending(p => p.NumInicio);
                    break;
                default:
                    ncf = ncf.OrderBy(c => c.Inicio);
                    break;
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(ncf.ToPagedList(pageNumber, pageSize));
        }

        // GET : Ncf/Create
        //[Authorize(Roles = "Admin")]
        public ActionResult CreateNcf()
        {
            return View();
        }

        // POST: Cliente/Create
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNcf(Ncf ncf)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var ini = ncf.Inicio;
                    var numIni = ncf.NumInicio;
                    var numFin = ncf.NumFin;
                    var cont = ncf.Contador;

                    var file = new Ncf();
                    file.Inicio = ini;
                    file.NumInicio = numIni;
                    file.NumFin = numFin;
                    file.Contador = cont;

                    DB.Ncf.Add(file);
                    DB.SaveChanges();

                    TempData["Status"] = "Upload successful";
                    return RedirectToAction("IndexNcf");
                }
            }
            catch (RetryLimitExceededException  /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(ncf);
        }

        // GET: Cliente/Edit/5
        //[Authorize(Roles = "Admin, Usuario")]
        public ActionResult EditNcf(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var ncf = new Ncf();
            ncf = DB.Ncf.Find(id);
            if (ncf == null)
            {
                return HttpNotFound();
            }
            return View(ncf);
        }
        //[Authorize(Roles = "Admin")]
        [HttpPost, ActionName("EditNcf")]
        [ValidateAntiForgeryToken]
        public ActionResult EditNPost(int? id, Ncf mod)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                if (ModelState.IsValid)
                {
                    var ini = mod.Inicio;
                    var numIni = mod.NumInicio;
                    var numFin = mod.NumFin;
                    var cont = mod.Contador;

                    Ncf ncf = DB.Ncf.Find(id);
                    ncf.Inicio = ini;
                    ncf.NumInicio = numIni;
                    ncf.NumFin = numFin;
                    ncf.Contador = cont;
                    DB.SaveChanges();
                    return RedirectToAction("IndexNcf");
                }
            }
            catch (RetryLimitExceededException  /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            return View(mod);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult IndexUser()
        {


            var users = DB.Users.Include(u => u.Roles);

            var userVM = users.Select(user => new UserViewModel
            {
                Username = user.UserName,
                Email = user.Email,
                RoleName = user.Roles.Select(r => r.RoleId).FirstOrDefault(),
            }).ToList();

            var model = new GroupedUserViewModel { Users = userVM };
            return View(model);
        }
        public ActionResult EliminarRoles(string NomUsuario)
        {
            if (NomUsuario == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var VmRol = new EliminarRolViewModel();
            VmRol.ApplicationUser = DB.Users.Where(u => u.UserName.Equals(NomUsuario, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            VmRol.RoleA = UserManager.GetRoles(VmRol.ApplicationUser.Id).FirstOrDefault();
            return View(VmRol);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult EliminarRoles(EliminarRolViewModel mod)
        {
            var nombreUsu = mod.ApplicationUser.UserName;
            ApplicationUser user = DB.Users.Where(u => u.UserName.Equals(nombreUsu, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var roles = UserManager.GetRoles(user.Id);
            UserManager.RemoveFromRoles(user.Id, roles.ToArray());
            return RedirectToAction("IndexUser");
        }



    }
}