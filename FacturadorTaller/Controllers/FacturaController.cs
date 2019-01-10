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
using iTextSharp.text.pdf.draw;

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
                          .Where(f => f.PagoStatus !="S" && f.PagoStatus !="A")
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
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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
                    var ncf = mod.Factura.Ncf;
                    var fechanc = mod.Factura.FechaNcf;

                    Factura file = DB.Factura.Find(facid);
                    file.OrdenCompraNu = ordenc;
                    file.FechaFac = fechafac;
                    file.FechaVen = fechaven;
                    file.Ncf = ncf;
                    file.FechaNcf = fechanc;
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

        // GET: Factura/Anula/5
        [Authorize(Roles = "Admin")]
        public ActionResult AnulaFac(int? id)
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AnulaFac(CreafacturaViewModel mod)
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
                    var ordeNu = mod.Factura.OrdenCompraNu;

                    Factura file = DB.Factura.Find(facid);
                    file.PagoStatus = "A";
                    file.OrdenCompraNu = ordeNu;
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
            if (VM.Factura.Consolidado != "S")
            {
                VM.PagoFac = VM.Factura.Cotizacion.TotalFactura + VM.Factura.Cotizacion.Itbis;
                VM.Balance = VM.Factura.Cotizacion.TotalFactura + VM.Factura.Cotizacion.Itbis;
            }
            else
            {
                VM.PagoFac = DB.Cotizacion.Where(c => c.ConsolidadoId == VM.Factura.Cotizacion.ConsolidadoId)
                    .Sum(c => c.TotalFactura + c.Itbis);
                VM.Balance = DB.Cotizacion.Where(c => c.ConsolidadoId == VM.Factura.Cotizacion.ConsolidadoId)
                    .Sum(c => c.TotalFactura + c.Itbis);
            }
            
            VM.Pago = DB.Pago.FirstOrDefault(f => f.FacturaId == id);
            if (VM.Pago != null)
            {
                VM.SumPago = DB.Pago
                          .Where(p => p.FacturaId == id)
                          .Sum(p => p.MontoPago);
                if (VM.Factura.Consolidado != "S")
                {
                    VM.Balance = VM.Factura.Cotizacion.TotalFactura + VM.Factura.Cotizacion.Itbis - VM.SumPago;
                }
                else
                {
                    var sumaBal = DB.Cotizacion.Where(c => c.ConsolidadoId == VM.Factura.Cotizacion.ConsolidadoId)
                                 .Sum(c => c.TotalFactura + c.Itbis);
                    VM.Balance = sumaBal - VM.SumPago;
                }
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
                    if (facm.Consolidado != "S")
                    {
                        if (facm.Cotizacion.TotalFactura + facm.Cotizacion.Itbis == sumPago)
                        {
                            facm.PagoStatus = "S";
                        }
                        decimal balance = facm.Cotizacion.TotalFactura + facm.Cotizacion.Itbis - sumPago;
                    }
                    else
                    {
                        var fac = DB.Cotizacion.Where(c => c.ConsolidadoId == facm.Cotizacion.ConsolidadoId)
                                 .Sum(c => c.TotalFactura + c.Itbis);
                        if (fac == sumPago)
                        {
                            facm.PagoStatus = "S";
                        }
                        decimal balance = fac - sumPago;
                    }
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
            PdfGen(id, cotId);
            FileStream fs = new FileStream(Server.MapPath("/Content/Factura.pdf"), FileMode.Open, FileAccess.Read);

            return File(fs, "application/pdf");
        }

        public void PdfGen(int? id, int cotId)
        {
            var VM = new FacturaViewModel();
            VM.Factura = DB.Factura.Include(f => f.Cotizacion)
                         .FirstOrDefault(c => c.FacturaId == id);
            if (VM.Factura.Consolidado != "S")
            { VM.DetalleCot = DB.DetalleCot.Include(d => d.Producto)
                .Where(c => c.CotizacionId == cotId && c.Producto.Categoria == "Servicio")
                .OrderByDescending(c => c.CotizacionId);
                VM.TotalFacb = VM.Factura.Cotizacion.TotalFactura + VM.Factura.Cotizacion.Itbis;
                VM.TotalFac = VM.Factura.Cotizacion.TotalFactura;
                VM.TotalItbis = VM.Factura.Cotizacion.Itbis;
            }
            else
            {
                VM.DetalleCot = DB.DetalleCot.Include(d => d.Producto)
                .Where(c => c.Cotizacion.ConsolidadoId == VM.Factura.Cotizacion.ConsolidadoId && c.Producto.Categoria == "Servicio")
                .OrderByDescending(c => c.CotizacionId);
                VM.TotalFacb = DB.Cotizacion.Where(c => c.ConsolidadoId == VM.Factura.Cotizacion.ConsolidadoId)
                    .Sum(c => c.TotalFactura + c.Itbis);
                VM.TotalFac = DB.Cotizacion.Where(c => c.ConsolidadoId == VM.Factura.Cotizacion.ConsolidadoId)
                    .Sum(c => c.TotalFactura);
                VM.TotalItbis = DB.Cotizacion.Where(c => c.ConsolidadoId == VM.Factura.Cotizacion.ConsolidadoId)
                    .Sum(c => c.Itbis);
            }
            foreach (var v in VM.DetalleCot)
            {
                if (v.Comentario != null)
                {
                    VM.cont = 1;

                }
                else
                {
                    VM.cont = 0;
                }
            }
            var file = new FileInfo(Server.MapPath("/Content/Factura.pdf"));
            if (file.Exists)
            {
                file.Delete();
            }
            Document doc = new Document(PageSize.LETTER, 35, 35, 120, 35);
            var output = new FileStream(Server.MapPath("/Content/Factura.pdf"), FileMode.Create);
            var writer = PdfWriter.GetInstance(doc, output);
            writer.PageEvent = new PageEventHelper();

            doc.Open();


            PdfPTable table1 = new PdfPTable(2);
            PdfPCell celda1 = new PdfPCell();
            table1.WidthPercentage = 100;
            table1.SetWidths(new int[] { 2,1 });
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;
            table1.SpacingBefore = 20f;
            table1.SpacingAfter = 30f;

            string fecha = VM.Factura.FechaFac.ToString("dd/MM/yyyy");
            Phrase phrase = new Phrase();
            phrase.Add(new Chunk("EME SOLUCIONES EN GENERAL, S.R.L.", new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.BOLD)));
            table1.AddCell(phrase);
            table1.AddCell("Factura Válida ");
            table1.AddCell("RNC: 131 - 33773 - 2");
            table1.AddCell("Para crédito fiscal");
            table1.AddCell("Fecha : " + fecha);
            table1.AddCell("NCF: " + VM.Factura.Ncf);
            table1.AddCell("Factura: " + VM.Factura.FacturaId.ToString());
            table1.AddCell("Válida Hasta: " + VM.Factura.FechaNcf);

            doc.Add(table1);

           
            table1 = new PdfPTable(1);
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.WidthPercentage = 100;
            table1.AddCell("RNC: " + VM.Factura.Cotizacion.Clientes.RncCliente);
            table1.AddCell("Cliente: "+VM.Factura.Cotizacion.Clientes.NombreCliente);
            table1.AddCell("Orden de Compra: " + VM.Factura.OrdenCompraNu);


            doc.Add(table1);

            table1 = new PdfPTable(6 + VM.cont);
            table1.WidthPercentage = 100;
            if (VM.cont != 0)
            { table1.SetWidths(new int[] { 1, 1, 1, 2, 2, 1, 1 }); }
            else
            { table1.SetWidths(new int[] { 1, 1, 1, 2, 1, 1 }); }
            table1.HorizontalAlignment = 0;
            table1.SpacingBefore = 20f;
            table1.SpacingAfter = 30f;

            table1.AddCell("Ficha");
            table1.AddCell("Cantidad");
            table1.AddCell("Valor RD$");
            table1.AddCell("Tipo Trabajo");
            if (VM.cont != 0)
            { table1.AddCell("Detalle"); }
            table1.AddCell("Itbis");
            table1.AddCell("Total RD$");

            foreach (var detalle in VM.DetalleCot)
            {
                decimal total = detalle.Cantidad * detalle.Valor;
                decimal itbisVal = 0;
                if (VM.TotalItbis != 0) { itbisVal = detalle.Valor * 0.18m; }
                table1.AddCell(detalle.FichaVehiculo);
                table1.AddCell(detalle.Cantidad.ToString());
                table1.AddCell(new PdfPCell(new Phrase(detalle.Valor.ToString("N"))) { HorizontalAlignment = Element.ALIGN_RIGHT });
                table1.AddCell(detalle.Producto.NombreProducto);
                if (VM.cont != 0) { table1.AddCell(detalle.Comentario); }
                table1.AddCell(new PdfPCell(new Phrase(itbisVal.ToString("N"))) { HorizontalAlignment = Element.ALIGN_RIGHT });
                table1.AddCell(new PdfPCell(new Phrase(total.ToString("N"))) { HorizontalAlignment = Element.ALIGN_RIGHT });
            };
            table1.AddCell("");
            table1.AddCell("");
            table1.AddCell(VM.Factura.Cotizacion.Nota);
            table1.AddCell("");
            table1.AddCell("");

            doc.Add(table1);

            table1 = new PdfPTable(4);
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell(new PdfPCell(new Phrase("Total RD: " + VM.TotalFac.ToString("C"))) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });
            doc.Add(table1);

            table1 = new PdfPTable(4);
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell(new PdfPCell(new Phrase("18% Itbis: " + VM.TotalItbis.ToString("C"))) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });

            doc.Add(table1);

            table1 = new PdfPTable(3);
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell(new PdfPCell(new Phrase("Total General RD: " + VM.TotalFacb.ToString("C"))) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });

            doc.Add(table1);

            Paragraph para = new Paragraph();
            para.Add("______________________________ \nFirma Suplidor");
            celda1.AddElement(para);
            doc.Add(para);

            doc.Close();
            doc.Dispose();

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
            string notaEmail = fac.Nota;
            int Id = fac.Factura.FacturaId;
            int cotId = fac.Factura.CotizacionId;
            var VM = new FacturaViewModel();
            VM.Factura = DB.Factura.Include(f => f.Cotizacion)
                         .FirstOrDefault(c => c.FacturaId == fac.Factura.FacturaId);
            PdfGen(Id,cotId);
            var body = "<p>Cliente: {0} </p> </p><p>{1}</p><p> </p><p>Saludos, </p><p> </p><p> </p><p>Dora De Los Santos</p><p>Ejecutivo Ventas</p>";

            MailMessage mail = new MailMessage();
            foreach (var to in femail.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                mail.To.Add(new MailAddress(to));
            }
            mail.From = new MailAddress("emesolucionessrl@gmail.com");
            mail.Subject = "Factura EME Soluciones en General";
            string Body = string.Format(body, VM.Factura.Cotizacion.Clientes.NombreCliente, notaEmail);
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

        public class PageEventHelper : PdfPageEventHelper
        {
            Font ffont = new Font(Font.FontFamily.HELVETICA, 9, Font.NORMAL);
            protected PdfPTable table;
            protected float tableHeight;

            public PageEventHelper()
            {
                Phrase frase = new Phrase();
                frase.Add(new Chunk("             EME SOLUCIONES EN GENERAL, S.R.L.", FontFactory.GetFont("Arial", 22, Font.BOLD, BaseColor.BLACK)));
                table = new PdfPTable(1);
                table.TotalWidth = 523f;
                table.LockedWidth = true;
                table.DefaultCell.Border = Rectangle.NO_BORDER;
                table.AddCell(frase);
                table.AddCell("                           REPARACION DE CAMA DE CAMIONES / SOLDADURA EN GENERAL");
                table.AddCell("                                              C/Felix Evaristo Mejia, Sector Villas Agricolas ");
                table.AddCell("                                                                  Cel.: 829-350-3671");
                tableHeight = table.TotalHeight;
            }

            public float getTableHeight()
            {
                return tableHeight;
            }



            //
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                var logo = iTextSharp.text.Image.GetInstance(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Camion.jpg"));
                logo.SetAbsolutePosition(10, 725);
                logo.ScaleAbsoluteHeight(30);
                logo.ScaleAbsoluteWidth(70);

                writer.DirectContent.AddImage(logo);

                table.WriteSelectedRows(0, -1,
                document.Left,
                document.Top + ((document.TopMargin + tableHeight) / 2),
                writer.DirectContent);
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