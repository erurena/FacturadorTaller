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
    public class CotizacionController : Controller
    {
        private readonly ApplicationDBContext DB;

        public CotizacionController()
        {
            DB = new ApplicationDBContext();
        }

        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrWhiteSpace(sortOrder) ? "CotizacionId_desc" : "";
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
            var cotizacion = from s in DB.Cotizacion.Include(f => f.Clientes)
                             .Where(f => f.FacturaEst !="S")
                              select s;
            if (!string.IsNullOrEmpty(searchString))
            {
                cotizacion = cotizacion.Where(c => c.Clientes.NombreCliente.Contains(searchString)
                                                              || c.Clientes.TelefonoCliente.Contains(searchString)
                                                              || c.Clientes.RncCliente.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "CotizacionId_desc":
                    cotizacion = cotizacion.OrderByDescending(c => c.CotizacionId);
                    break;
                case "NombreCliente":
                    cotizacion = cotizacion.OrderBy(c => c.Clientes.NombreCliente);
                    break;
                case "NombreCliente_desc":
                    cotizacion = cotizacion.OrderByDescending(p => p.Clientes.NombreCliente);
                    break;
                default:
                    cotizacion = cotizacion.OrderBy(c => c.CotizacionId);
                    break;
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(cotizacion.ToPagedList(pageNumber, pageSize));
        }

        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var VM = new DetalleCotViewModel();

            VM.Cotizacion = DB.Cotizacion.Include(c => c.Clientes)
                .FirstOrDefault(c => c.CotizacionId == id);
            VM.DetalleCot = DB.DetalleCot.Include(d => d.Producto)
                .Where(d => d.CotizacionId == id)
                .OrderByDescending(c => c.CotizacionId);
            return View(VM);
        }

        // GET : Cotizacion/Create
        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult CreateCot()
        {
            return View();
        }
        // POST: Cotizacion/Create
       [Authorize(Roles = "Admin, Usuario")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCot(CotizacionViewModel cot)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var usuario = User.Identity.GetUserName();
                    var cliente = cot.Cotizacion.Clientes.ClienteId;
                    var fecha = cot.GetDate();
                    var nota = cot.Cotizacion.Nota;

                    Cotizacion file = new Cotizacion();

                    file.ClienteId = cliente;
                    file.Fecha = fecha;
                    file.Nota = nota;

                    DB.Cotizacion.Add(file);
                    DB.SaveChanges();

                    TempData["Status"] = "Upload successful";
                    return RedirectToAction("ProductoFac", new { cotId = file.CotizacionId });
                }
            }
            catch (RetryLimitExceededException  /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(cot);

         }

        // GET: Factura/Edit/5
        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var VM = new CotizacionViewModel();
            VM.Cotizacion = DB.Cotizacion.Include(c => c.Clientes)
                            .FirstOrDefault(c => c.CotizacionId == id);
            VM.Fecha = (VM.Cotizacion.Fecha).ToString("dd/MM/yyyy");
            if (VM.Cotizacion == null)
            {
                return HttpNotFound();
            }
            return View(VM);
        }

        [Authorize(Roles = "Admin, Usuario")]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id, CotizacionViewModel mod)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                if (ModelState.IsValid)
                {
                    var cliente = mod.Cotizacion.Clientes.ClienteId;
                    var fecha = mod.GetDate();
                    var nota = mod.Cotizacion.Nota;

                    Cotizacion file = DB.Cotizacion.Find(id);
                    file.ClienteId = cliente;
                    file.Fecha = fecha;
                    file.Nota = nota;
                    DB.SaveChanges();
                    return RedirectToAction("ProductoFac", new { cotId = file.CotizacionId });
                }
            }
            catch (RetryLimitExceededException  /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            return View(mod);
        }


        //GET: DetalleProductos/Create
        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult ProductoFac(int? cotId)
        {
            if (cotId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var VM = new DetalleCotViewModel();

            VM.Cotizacion = DB.Cotizacion.Include(c => c.Clientes)
                .FirstOrDefault(c => c.CotizacionId == cotId);
            VM.DetalleCot = DB.DetalleCot.Include(d => d.Producto)
                .Where(c => c.CotizacionId == cotId)
                .OrderByDescending(c => c.CotizacionId);
            return View(VM);
        }

        // POST: DetalleProductos/Create
        [Authorize(Roles = "Admin, Usuario")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductoFac(DetalleCotViewModel cot, int productoId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var cotId = cot.Cotizacion.CotizacionId;
                    var clienteId = cot.Cotizacion.ClienteId;
                    var prodId = productoId;
                    var cant = cot.Cantidad;
                    var ficVeh = cot.FichaVehiculo;
                    var valor = cot.Valor;
                    var comen = cot.Comentario;
                    DateTime fechaActual = DateTime.Now.AddMonths(-3);

                    var fichExi = DB.DetalleCot.Include(c => c.Cotizacion)
                        .Where(c => c.Cotizacion.ClienteId == clienteId && c.ProductoId == prodId && c.FichaVehiculo == ficVeh 
                        && c.Cotizacion.Fecha >= fechaActual && c.Cotizacion.FacturaEst == "S");


                    if (fichExi.Any())
                    {
                        //ViewBag.Ficha = "Esta ficha ya tiene un factura con ese producto";
                        TempData["Ficha"] = "**Aviso Error*** Este cliente tiene ese producto con esta ficha de vehiculo facturado en otra Cotizacion en menos de 3 meses";
                        return RedirectToAction("ProductoFac", new { cotId });
                    }


                    DetalleCot file = new DetalleCot();

                    file.CotizacionId = cotId;
                    file.ProductoId = prodId;
                    file.Cantidad = cant;
                    file.FichaVehiculo = ficVeh;
                    file.Valor = valor;
                    file.Comentario = comen;

                    DB.DetalleCot.Add(file);
                    DB.SaveChanges();

                    Decimal monto = DB.DetalleCot
                                    .Where(d => d.CotizacionId == cotId && d.Producto.Categoria == "Servicio")
                                    .Sum(m => m.Cantidad * m.Valor);
                    Decimal itbis = monto * 0.18m;

                    Cotizacion cotm = DB.Cotizacion.Find(cotId);
                    cotm.TotalFactura = monto;
                    cotm.Itbis = itbis;
                    DB.SaveChanges();

                    TempData["Status"] = "Upload successful";
                    return RedirectToAction("ProductoFac", new { cotId = file.CotizacionId });
                }
            }
            catch (RetryLimitExceededException  /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(cot);

        }
        // Delete Cotizacion
        [Authorize(Roles = "Admin")]
        public ActionResult CotDelete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }
            var VM = new CotizacionViewModel();
            VM.Cotizacion = DB.Cotizacion.Include(c => c.Clientes)
                            .FirstOrDefault(c => c.CotizacionId == id);
            if (VM.Cotizacion == null)
            {
                return HttpNotFound();
            }
            return View(VM);
        }

        // POST: Cotizacion/Delete/
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("CotDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCotConfirmed(CotizacionViewModel fac)
        {
            try
            {
                var MId = fac.Cotizacion.CotizacionId;
                Cotizacion cotizacion = DB.Cotizacion.Find(MId);
                DB.Cotizacion.Remove(cotizacion);
                IEnumerable<DetalleCot> detallecot = DB.DetalleCot.Where(d => d.CotizacionId == MId);
                if (detallecot != null)
                {
                    DB.DetalleCot.RemoveRange(detallecot);
                }
                DB.SaveChanges();
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return RedirectToAction("FacDelete", new { id = fac.Cotizacion.CotizacionId, saveChangesError = true });
            }
            return RedirectToAction("Index");
        }

        // Email Cotizacion
        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult Email (int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var VM = new CotizacionViewModel();
            VM.Cotizacion = DB.Cotizacion.Include(c => c.Clientes)
                            .FirstOrDefault(c => c.CotizacionId == id);
            VM.DetalleCot = DB.DetalleCot.Include(d => d.Producto)
                .Where(c => c.CotizacionId == id)
                .OrderByDescending(c => c.CotizacionId);
            if (VM.Cotizacion == null)
            {
                return HttpNotFound();
            }
            return View(VM);
        }
        [Authorize(Roles = "Admin, Usuario")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Email (CotizacionViewModel cot)
        {
            if (cot == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string femail = cot.Email;
            string notaEmail = cot.Nota;
            int id = cot.Cotizacion.CotizacionId;
            var VM = new CotizacionViewModel();
            VM.Cotizacion = DB.Cotizacion.Include(c => c.Clientes)
                            .FirstOrDefault(c => c.CotizacionId == cot.Cotizacion.CotizacionId);
            PdfGen(id);
            var body = "<p>Cliente: {0} </p> </p><p>{1}</p><p> </p><p>Saludos, </p><p> </p><p> </p><p>Dora De Los Santos</p><p>Ejecutivo Ventas</p>";
          
           MailMessage mail = new MailMessage();
           foreach(var to in femail.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)) {
                mail.To.Add(new MailAddress(to));
            }
           mail.From = new MailAddress("emesolucionessrl@gmail.com");
           mail.Subject = "Cotizacion Nueva EME Soluciones en General";
           string Body = string.Format(body, VM.Cotizacion.Clientes.NombreCliente,notaEmail);
           mail.Body = Body;
           mail.Attachments.Add(new Attachment(Server.MapPath("/Content/Cotizacion.pdf")));
           mail.IsBodyHtml = true;
           using (var smtp = new SmtpClient())
           {
                 smtp.Send(mail);
                 mail.Dispose();
                 return RedirectToAction("Sent");
            }
        }

        public ActionResult Sent()
        {
            return View();
        }

        [Authorize(Roles = "Admin, Usuario")]
        public FileStreamResult Pdf(int? id)
        {
            PdfGen(id);
            FileStream fs = new FileStream(Server.MapPath("/Content/Cotizacion.pdf"), FileMode.Open, FileAccess.Read);
            return File(fs, "application/pdf");

        }

      
        public void PdfGen (int? id)
        {
            var VM = new CotizacionViewModel();
            VM.Cotizacion = DB.Cotizacion.Include(c => c.Clientes)
                            .FirstOrDefault(c => c.CotizacionId == id);
            VM.DetalleCot = DB.DetalleCot.Include(d => d.Producto)
                .Where(c => c.CotizacionId == id && c.Producto.Categoria == "Servicio")
                .OrderByDescending(c => c.CotizacionId);
            foreach(var v in VM.DetalleCot)
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
            var totalFac = VM.Cotizacion.TotalFactura + VM.Cotizacion.Itbis;
            var file = new FileInfo(Server.MapPath("/Content/Cotizacion.pdf"));
            if (file.Exists)
            {
                file.Delete();
            }
           
            Document doc = new Document(PageSize.LETTER,35,35,120,35);
            var output = new FileStream(Server.MapPath("/Content/Cotizacion.pdf"), FileMode.Create);
            var writer = PdfWriter.GetInstance(doc, output);
            writer.PageEvent = new PageEventHelper();          

            doc.Open();

            PdfPTable table1 = new PdfPTable(3);
            PdfPCell celda1 = new PdfPCell();
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;
            table1.SpacingBefore = 20f;
            table1.SpacingAfter = 10f;

            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell("Cotizacion: " + VM.Cotizacion.CotizacionId.ToString());

            doc.Add(table1);

            VM.Fecha = (VM.Cotizacion.Fecha).ToString("dd/MM/yyyy");
            table1 = new PdfPTable(1);
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.AddCell("Fecha: " + VM.Fecha);
            table1.AddCell(VM.Cotizacion.Clientes.NombreCliente);
            table1.AddCell("RNC " + VM.Cotizacion.Clientes.RncCliente);

            doc.Add(table1);

            table1 = new PdfPTable(5 + VM.cont);
            table1.WidthPercentage = 100;
            if (VM.cont != 0)
            { table1.SetWidths(new int[] { 1, 1, 2, 2, 1, 1 }); }
            else
            { table1.SetWidths(new int[] { 1, 1, 2, 1, 1 }); }
            table1.HorizontalAlignment = 0;
            table1.SpacingBefore = 20f;
            table1.SpacingAfter = 30f;

            table1.AddCell("Cantidad");
            table1.AddCell("Ficha");
            table1.AddCell("Tipo Trabajo");
            if (VM.cont != 0)
            { table1.AddCell("Detalle"); }
            table1.AddCell("Valor RD$");
            table1.AddCell("Total RD$");
            table1.HeaderRows = 1; 

            foreach (var detalle in VM.DetalleCot)
            {
                decimal total = detalle.Cantidad * detalle.Valor;
                table1.AddCell(detalle.Cantidad.ToString());
                table1.AddCell(detalle.FichaVehiculo);
                table1.AddCell(detalle.Producto.NombreProducto);
                if (VM.cont !=0) { table1.AddCell(detalle.Comentario); }
                table1.AddCell(new PdfPCell(new Phrase(detalle.Valor.ToString("N"))) { HorizontalAlignment = Element.ALIGN_RIGHT });
                table1.AddCell(new PdfPCell(new Phrase(total.ToString("N"))) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            table1.AddCell("");
            table1.AddCell("");
            table1.AddCell(VM.Cotizacion.Nota);
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
            table1.AddCell(new PdfPCell(new Phrase("Total RD: " + VM.Cotizacion.TotalFactura.ToString("C"))) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });

            doc.Add(table1);

            table1 = new PdfPTable(4);
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell(new PdfPCell(new Phrase("18% Itbis: " + VM.Cotizacion.Itbis.ToString("C"))) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });

            doc.Add(table1);

            table1 = new PdfPTable(3);
            table1.WidthPercentage = 100;
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.HorizontalAlignment = 0;

            table1.AddCell(" ");
            table1.AddCell("");
            table1.AddCell(new PdfPCell(new Phrase("Total General RD: " + totalFac.ToString("C"))) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });

            doc.Add(table1);

            doc.Close();
            doc.Dispose();

            
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
                table.AddCell("                                                                  RNC: 131-33773-2");
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



        // Delete Produto Cotizacion
                [Authorize(Roles = "Admin")]
        public ActionResult ProDelete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }
            var detalleCot = DB.DetalleCot.Find(id);
            if (detalleCot == null)
            {
                return HttpNotFound();
            }
            return View(detalleCot);
        }



        // POST: Producto/Delete/
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("ProDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProConfirmed(DetalleCot det)
        {
            try
            {
                var MId = det.DetalleCotId;
                DetalleCot detallecot = DB.DetalleCot.Find(MId);
                DB.DetalleCot.Remove(detallecot);
                DB.SaveChanges();

                Decimal monto = DB.DetalleCot
                                    .Where(d => d.CotizacionId == detallecot.CotizacionId)
                                    .Sum(m => m.Cantidad * m.Valor);
                Decimal itbis = monto * 0.18m;

                Cotizacion cotm = DB.Cotizacion.Find(detallecot.CotizacionId);
                cotm.TotalFactura = monto;
                cotm.Itbis = itbis;
                DB.SaveChanges();

                return RedirectToAction("ProductoFac", new { cotId = detallecot.CotizacionId });
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return RedirectToAction("ProDelete", new { id = det.DetalleCotId, saveChangesError = true });
            }
        }

        // GET: Cliente/Edit/5
        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult ProEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var detalleCot = new DetalleCot();
            detalleCot = DB.DetalleCot.Find(id);
            if (detalleCot == null)
            {
                return HttpNotFound();
            }
            return View(detalleCot);
        }
        [Authorize(Roles = "Admin, Usuario")]
        [HttpPost, ActionName("ProEdit")]
        [ValidateAntiForgeryToken]
        public ActionResult ProEditPost(int? id, DetalleCot mod)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                if (ModelState.IsValid)
                {
                    var cant = mod.Cantidad;
                    var fichaVeh = mod.FichaVehiculo;
                    var valor = mod.Valor;
                    var comen = mod.Comentario;

                    DetalleCot detalleCot = DB.DetalleCot.Find(id);
                    detalleCot.Cantidad = cant;
                    detalleCot.FichaVehiculo = fichaVeh;
                    detalleCot.Valor = valor;
                    detalleCot.Comentario = comen;
                    DB.SaveChanges();

                    Decimal monto = DB.DetalleCot
                                    .Where(d => d.CotizacionId == detalleCot.CotizacionId)
                                    .Sum(m => m.Cantidad * m.Valor);
                    Decimal itbis = monto * 0.18m;

                    Cotizacion cotm = DB.Cotizacion.Find(detalleCot.CotizacionId);
                    cotm.TotalFactura = monto;
                    cotm.Itbis = itbis;
                    DB.SaveChanges();

                    return RedirectToAction("ProductoFac", new { cotId = detalleCot.CotizacionId });
                }
            }
            catch (RetryLimitExceededException  /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            return View(mod);
        }

        // GET: Factura/Edit/5
        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult CreaFactura (int id)
        {
            var VM = new CreafacturaViewModel();
            VM.CotId = id;
            return View(VM);
        }

        [Authorize(Roles = "Admin, Usuario")]
        [HttpPost, ActionName("CreaFactura")]
        [ValidateAntiForgeryToken]
        public ActionResult CreaFactura(CreafacturaViewModel mod)
        {
            if (mod == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                if (ModelState.IsValid)
                {
                    var cotId = mod.CotId;
                    var fechaFac = mod.GetDateF();
                    var fechaVen = mod.GetDateFv();
                    var ncf = mod.NcfInd;
                    string fechancf;
                    if (ncf == "S")
                    {
                        var ncfMo = DB.Ncf.Where(n => n.Estatus == null).FirstOrDefault();
                        fechancf = ncfMo.NcfFecha?.ToString("dd/MM/yyyy");
                        var ncfAct = (ncfMo.NumInicio  + ncfMo.Contador);
                        var ncfCont = ncfMo.Contador + 1;
                        var ncfReleg = ncfAct.ToString().Length;
                        var ncfCero = new string ('0', (8 - ncfReleg));
                        ncf = string.Concat(ncfMo.Inicio + ncfCero +  ncfAct);
                        if (ncfCont > ncfMo.NumFin)
                        {
                            ncfMo.Estatus = "C";
                        }
                        ncfMo.Contador = ncfCont;
                        ncfMo.NumActual = ncfAct;
                        DB.SaveChanges();
                    }
                    else
                    {
                        ncf = "";
                        fechancf = "";
                    }
                    var ordenCompra = mod.Factura.OrdenCompraNu;

                    Cotizacion cotm = DB.Cotizacion.Find(cotId);
                    if (cotm.ConsolidadoId != null)
                    {
                       IEnumerable<Cotizacion> cot= DB.Cotizacion.Where(c => c.ConsolidadoId == cotm.ConsolidadoId);
                       foreach(var c in cot)
                        {
                            c.FacturaEst = "S";
                            DB.Entry(c).State = EntityState.Modified;
                        }
                        DB.SaveChanges();
                        Factura filec = new Factura();

                        filec.CotizacionId = cotId;
                        filec.FechaFac = fechaFac;
                        filec.FechaVen = fechaVen;
                        filec.Ncf = ncf;
                        filec.FechaNcf = fechancf;
                        filec.Consolidado = "S";
                        filec.OrdenCompraNu = ordenCompra;
                        DB.Factura.Add(filec);
                        DB.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    cotm.FacturaEst = "S";
                    DB.SaveChanges();

                    Factura file = new Factura();

                    file.CotizacionId = cotId;
                    file.FechaFac = fechaFac;
                    file.FechaVen = fechaVen;
                    file.Ncf = ncf;
                    file.FechaNcf = fechancf;
                    file.OrdenCompraNu = ordenCompra;
                    DB.Factura.Add(file);
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

        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult ConsolidadoFac(int id)
        {
           IEnumerable<Cotizacion> cot = DB.Cotizacion.Where(c => c.ClienteId == id && c.FacturaEst != "S");
            return View(cot);
        }

        [Authorize(Roles = "Admin, Usuario")]
        public ActionResult Consolidado(int id)
        {
            return View();
        }

        [Authorize(Roles = "Admin, Usuario")]
        [HttpPost]
        public ActionResult Consolidado(int id, Cotizacion conso)
        {
            var ind = conso.ConsolidadoId;
            Cotizacion cot = DB.Cotizacion.FirstOrDefault(c => c.CotizacionId == id && c.FacturaEst != "S");
            cot.ConsolidadoId = ind;
            DB.SaveChanges();
            return RedirectToAction("ConsolidadoFac", new { id = cot.ClienteId});
        }

        public JsonResult GetClientes()
        {
            return Json(DB.Clientes.Select(x => new
            {
                x.ClienteId,
                x.NombreCliente
            }).ToList(), JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetProductos()
        {
            return Json(DB.Producto.Select(x => new
            {
                x.ProductoId,
                x.NombreProducto,
                x.Precio
            }).ToList(), JsonRequestBehavior.AllowGet);

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