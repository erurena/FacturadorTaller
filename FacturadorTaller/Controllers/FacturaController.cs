using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using FacturadorTaller.Models;
using FacturadorTaller.ViewModel;
using System.Net;
using System.Data.Entity.Infrastructure;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace FacturadorTaller.Controllers
{
    public class FacturaController : Controller
    {

        private readonly ApplicationDBContext DB;

        public FacturaController()
        {
            DB = new ApplicationDBContext();
        }
        // GET: Factura
        //[Authorize(Roles = "Admin, Usuario")]
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrWhiteSpace(sortOrder) ? "FacturaId_desc" : "";
            ViewBag.ClienteSortParm = sortOrder == "NombreCliente" ? "NombreCliente_desc" : "NombreCliente";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            var factura = from s in DB.Factura.Include(f => f.Cotizacion.Clientes)
                             select s;
            if (!string.IsNullOrEmpty(searchString))
            {
                factura = factura.Where(c => c.Ncf.Contains(searchString)
                                                  || c.OrdenCompraNu.Contains(searchString)
                                                              || c.Cotizacion.Clientes.NombreCliente.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "FacturaId_desc":
                    factura = factura.OrderByDescending(c => c.FacturaId);
                    break;
                case "NombreCliente":
                    factura = factura.OrderBy(c => c.Cotizacion.Clientes.NombreCliente);
                    break;
                case "NombreCliente_desc":
                    factura = factura.OrderByDescending(p => p.Cotizacion.Clientes.NombreCliente);
                    break;
                default:
                    factura = factura.OrderBy(c => c.FacturaId);
                    break;
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(factura.ToPagedList(pageNumber, pageSize));
        }
    }
}