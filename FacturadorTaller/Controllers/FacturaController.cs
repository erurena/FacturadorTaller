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
using System.Net.Mail;

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
        [Authorize(Roles = "Admin, Usuario")]
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
                          .Where(f => f.PagoStatus !="S")
                             select s;
            var ncf = DB.Ncf.Where(n => n.Estatus == null).SingleOrDefault();
            if (ncf !=null)
             {
                   if (ncf.Contador > ncf.NumFin - 10)
                   {
                       ViewBag.NcfCont = " *** Su NCF se esta acercando al limite del Final Rango : " + ncf.NumFin;
                   }
             }
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

        // GET: Factura/Edit/5
        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var VM = new CreafacturaViewModel();
            VM.Factura = DB.Factura.Find(id);
            VM.FechaFac = (VM.Factura.FechaFac).ToString("dd/MM/yyyy");
            VM.FechaVen = (VM.Factura.FechaVen).ToString("dd/MM/yyyy");
            if (VM.Factura == null)
            {
                return HttpNotFound();
            }
            return View(VM);
        }

        [Authorize(Roles = "Admin, Usuario")]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(CreafacturaViewModel mod)
        {
            if (mod == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                if (ModelState.IsValid)
                {
                    var facid = mod.Factura.FacturaId;
                    var ordenc = mod.Factura.OrdenCompraNu;
                    var fechafac = mod.GetDateF();
                    var fechaven = mod.GetDateFv();

                    Factura file = DB.Factura.Find(facid);
                    file.OrdenCompraNu = ordenc;
                    file.FechaFac = fechafac;
                    file.FechaVen = fechaven;
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

        // GET: Pagos
        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult Pago(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var VM = new PagoViewModel();
            VM.Factura = DB.Factura.Include(f => f.Cotizacion)
                .FirstOrDefault(f => f.FacturaId == id);
            VM.PagoFac = VM.Factura.Cotizacion.TotalFactura + VM.Factura.Cotizacion.Itbis;
            VM.Balance = VM.Factura.Cotizacion.TotalFactura + VM.Factura.Cotizacion.Itbis;
            VM.Pago = DB.Pago.FirstOrDefault(f => f.FacturaId == id);
            if (VM.Pago != null)
            {
                VM.SumPago = DB.Pago
                          .Where(p => p.FacturaId == id)
                          .Sum(p => p.MontoPago);
                VM.Balance = VM.Factura.Cotizacion.TotalFactura + VM.Factura.Cotizacion.Itbis - VM.SumPago;
                VM.PagoFac = VM.Balance;
            }
            return View(VM);
        }
        [Authorize(Roles = "Admin, Usuario")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pago(PagoViewModel pag)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //var cajero = User.Identity.GetUserName();
                    var facId = pag.Factura.FacturaId;
                    var pago = pag.PagoFac;
                    var bal = pag.Balance;
                    var cat = pag.Pago.Categoria;
                    var fecha = pag.GetDate();

                    if (pago > bal)
                    {
                        ViewBag.PagoInv("Pago no puede ser mayor a la deuda");
                        return View(pag);
                    }

                    Pago file = new Pago();
                   // file.CajeroId = cajero;
                    file.FacturaId = facId;
                    file.MontoPago = pago;
                    file.Categoria = cat;
                    file.FechaPago = fecha;

                    DB.Pago.Add(file);
                    DB.SaveChanges();

                    decimal sumPago = DB.Pago
                      .Where(p => p.FacturaId == file.FacturaId)
                      .Sum(p => p.MontoPago);

                    Factura facm = DB.Factura.Include(f => f.Cotizacion)
                                 .FirstOrDefault(f => f.FacturaId == file.FacturaId);
                    if (facm.Cotizacion.TotalFactura + facm.Cotizacion.Itbis == sumPago)
                    {
                        facm.PagoStatus = "S";
                    }
                    decimal balance = facm.Cotizacion.TotalFactura + facm.Cotizacion.Itbis - sumPago;
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
            return View(pag);
        }

        [Authorize(Roles = "Admin, Usuario")]
        public FileStreamResult Pdf(int? id, int cotId)
        {
            var VM = new FacturaViewModel();
            VM.Factura = DB.Factura.Include(f => f.Cotizacion)
                         .FirstOrDefault(c => c.FacturaId == id);
            VM.DetalleCot = DB.DetalleCot.Include(d => d.Producto)
                .Where(c => c.CotizacionId == cotId)
                .OrderByDescending(c => c.CotizacionId);
            var totalFac = VM.Factura.Cotizacion.TotalFactura + VM.Factura.Cotizacion.Itbis;
            var file = new FileInfo(Server.MapPath("/Content/Cotizacion.pdf"));
            if (file.Exists)
            {
                file.Delete();
            }
            Document doc = new Document(PageSize.LETTER);
            var output = new FileStream(Server.MapPath("/Content/Cotizacion.pdf"), FileMode.Create);
            var writer = PdfWriter.GetInstance(doc, output);

            doc.Open();

            var logo = iTextSharp.text.Image.GetInstance(Server.MapPath("~/Content/Camion.jpg"));
            logo.ScaleAbsoluteHeight(30);
            logo.ScaleAbsoluteWidth(70);
            doc.Add(logo);

            Chunk chunk = new Chunk("EME SOLUCIONES EN GENERAL, S.R.L.", FontFactory.GetFont("Arial", 22, Font.BOLD, BaseColor.BLACK));
            var parac = new Paragraph(chunk);
            parac.Alignment = Element.ALIGN_CENTER;
            Paragraph para = new Paragraph();
            para.Add("REPARACION DE CAMA DE CAMIONES / SOLDADURA EN GENERAL\nC/Felix Evariso Mejia, ·227," +
                " Sector Villas Agricolas \nCel.: 829-350-3671  \nRNC: 131-33773-2");
            para.Alignment = Element.ALIGN_CENTER;
            doc.Add(parac);
            doc.Add(para);

            PdfPTable table1 = new PdfPTable(3);
            PdfPCell celda1 = new PdfPCell();
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell("Factura: " + VM.Factura.FacturaId.ToString());

            doc.Add(table1);

            table1 = new PdfPTable(3);
            celda1 = new PdfPCell();
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell("NCF: " + VM.Factura.Ncf);

            doc.Add(table1);

            table1 = new PdfPTable(1);
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.AddCell("Fecha: " + VM.Factura.FechaFac.ToShortDateString());
            table1.AddCell(VM.Factura.Cotizacion.Clientes.NombreCliente);
            table1.AddCell("RNC " + VM.Factura.Cotizacion.Clientes.RncCliente);

            doc.Add(table1);

            table1 = new PdfPTable(5);
            table1.WidthPercentage = 100;
            table1.SetWidths(new int[] { 1, 1, 2, 1, 1 });
            table1.HorizontalAlignment = 0;
            table1.SpacingBefore = 20f;
            table1.SpacingAfter = 30f;

            table1.AddCell("Cantidad");
            table1.AddCell("Ficha");
            table1.AddCell("Descripcion");
            table1.AddCell("Comentario");
            table1.AddCell("Valor RD$");

            foreach (var detalle in VM.DetalleCot)
            {
                table1.AddCell(detalle.Cantidad.ToString());
                table1.AddCell(detalle.FichaVehiculo);
                table1.AddCell(detalle.Producto.NombreProducto);
                table1.AddCell(detalle.Comentario);
                table1.AddCell(detalle.Valor.ToString("N0"));
            }

            doc.Add(table1);

            table1 = new PdfPTable(4);
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell("Total RD: " + VM.Factura.Cotizacion.TotalFactura.ToString("C0"));

            doc.Add(table1);

            table1 = new PdfPTable(4);
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell("18% Itbis: " + VM.Factura.Cotizacion.Itbis.ToString("C0"));

            doc.Add(table1);

            table1 = new PdfPTable(3);
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell("Total General RD: " + totalFac.ToString("C0"));

            doc.Add(table1);

            para = new Paragraph();
            para.Add("______________________________ \nFirma Cliente");
            celda1.AddElement(para);
            doc.Add(para);

            doc.Close();
            doc.Dispose();

            FileStream fs = new FileStream(Server.MapPath("/Content/Cotizacion.pdf"), FileMode.Open, FileAccess.Read);

            return File(fs, "application/pdf");
        }
        // Email Factura
        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult Email(int? id, int cotId)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var VM = new FacturaViewModel();
            VM.Factura = DB.Factura.Include(f => f.Cotizacion)
                         .FirstOrDefault(c => c.FacturaId == id);
            VM.DetalleCot = DB.DetalleCot.Include(d => d.Producto)
                .Where(c => c.CotizacionId == cotId)
                .OrderByDescending(c => c.CotizacionId);
            if (VM.Factura == null)
            {
                return HttpNotFound();
            }
            return View(VM);
        }

        [Authorize(Roles = "Admin, Usuario")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Email(FacturaViewModel fac)
        {
            if (fac == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string femail = fac.Email;
            var VM = new FacturaViewModel();
            VM.Factura = DB.Factura.Include(f => f.Cotizacion)
                         .FirstOrDefault(c => c.FacturaId == fac.Factura.FacturaId);
            VM.DetalleCot = DB.DetalleCot.Include(d => d.Producto)
                .Where(c => c.CotizacionId == fac.Factura.Cotizacion.CotizacionId)
                .OrderByDescending(c => c.CotizacionId);
            var totalFac = VM.Factura.Cotizacion.TotalFactura + VM.Factura.Cotizacion.Itbis;
            var body = "<p>Cliente: {0} </p> </p><p> </p><p> </p><p>Saludos, </p><p> </p><p> </p><p>Dora De Los Santos</p><p>Ejecutivo Ventas</p>";
            var file = new FileInfo(Server.MapPath("/Content/Factura.pdf"));
            if (file.Exists)
            {
                file.Delete();
            }
            Document doc = new Document(PageSize.LETTER);
            var output = new FileStream(Server.MapPath("/Content/Cotizacion.pdf"), FileMode.Create);
            var writer = PdfWriter.GetInstance(doc, output);

            doc.Open();

            var logo = iTextSharp.text.Image.GetInstance(Server.MapPath("~/Content/Camion.jpg"));
            logo.ScaleAbsoluteHeight(30);
            logo.ScaleAbsoluteWidth(70);
            doc.Add(logo);

            Chunk chunk = new Chunk("EME SOLUCIONES EN GENERAL, S.R.L.", FontFactory.GetFont("Arial", 22, Font.BOLD, BaseColor.BLACK));
            var parac = new Paragraph(chunk);
            parac.Alignment = Element.ALIGN_CENTER;
            Paragraph para = new Paragraph();
            para.Add("REPARACION DE CAMA DE CAMIONES / SOLDADURA EN GENERAL\nC/Felix Evariso Mejia, ·227," +
                " Sector Villas Agricolas \nCel.: 829-350-3671  \nRNC: 131-33773-2");
            para.Alignment = Element.ALIGN_CENTER;
            doc.Add(parac);
            doc.Add(para);

            PdfPTable table1 = new PdfPTable(3);
            PdfPCell celda1 = new PdfPCell();
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell("Factura: " + VM.Factura.FacturaId.ToString());

            doc.Add(table1);

            table1 = new PdfPTable(3);
            celda1 = new PdfPCell();
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell("NCF: " + VM.Factura.Ncf);

            doc.Add(table1);

            table1 = new PdfPTable(1);
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.AddCell("Fecha: " + VM.Factura.FechaFac.ToShortDateString());
            table1.AddCell(VM.Factura.Cotizacion.Clientes.NombreCliente);
            table1.AddCell("RNC " + VM.Factura.Cotizacion.Clientes.RncCliente);

            doc.Add(table1);

            table1 = new PdfPTable(5);
            table1.WidthPercentage = 100;
            table1.SetWidths(new int[] { 1, 1, 2, 1, 1 });
            table1.HorizontalAlignment = 0;
            table1.SpacingBefore = 20f;
            table1.SpacingAfter = 30f;

            table1.AddCell("Cantidad");
            table1.AddCell("Ficha");
            table1.AddCell("Descripcion");
            table1.AddCell("Comentario");
            table1.AddCell("Valor RD$");

            foreach (var detalle in VM.DetalleCot)
            {
                table1.AddCell(detalle.Cantidad.ToString());
                table1.AddCell(detalle.FichaVehiculo);
                table1.AddCell(detalle.Producto.NombreProducto);
                table1.AddCell(detalle.Comentario);
                table1.AddCell(detalle.Valor.ToString("N0"));
            }

            doc.Add(table1);

            table1 = new PdfPTable(4);
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell("Total RD: " + VM.Factura.Cotizacion.TotalFactura.ToString("C0"));

            doc.Add(table1);

            table1 = new PdfPTable(4);
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell("18% Itbis: " + VM.Factura.Cotizacion.Itbis.ToString("C0"));

            doc.Add(table1);

            table1 = new PdfPTable(3);
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell("Total General RD: " + totalFac.ToString("C0"));

            doc.Add(table1);

            para = new Paragraph();
            para.Add("______________________________ \nFirma Cliente");
            celda1.AddElement(para);
            doc.Add(para);

            doc.Close();
            doc.Dispose();
            MailMessage mail = new MailMessage();
            mail.To.Add(new MailAddress(femail));
            mail.From = new MailAddress("emesolucionessrl@gmail.com");
            mail.Subject = "Factura EME Soluciones en General";
            string Body = string.Format(body, VM.Factura.Cotizacion.Clientes.NombreCliente);
            mail.Body = Body;
            mail.Attachments.Add(new Attachment(Server.MapPath("/Content/Factura.pdf")));
            mail.IsBodyHtml = true;
            using (var smtp = new SmtpClient())
            {
                smtp.Send(mail);
                mail.Dispose();
                return RedirectToAction("Sent", "Cotizacion");
            }
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