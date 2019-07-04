using System.Net.Mail;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Data.Entity;
using System.Web.Mvc;
using System.Text;
using System.Net.Mime;
using System.Security;
using System.Web;
using System.IO;
using VEH.Intranet.Logic;
using VEH.Intranet.Controllers;
using Org.BouncyCastle.X509;
using System.Security.Cryptography.X509Certificates;

namespace VEH.Intranet.Logic
{
    public class EmailLogic
    {
        private SIVEHEntities context;
        private Controller Parent;
        public EmailLogic(Controller parent, CargarDatosContext datacontext)
        {
            Parent = parent;
            this.context = datacontext.context;
        }
        public EmailLogic()
        {
        }
        public void SendEmail(string asunto, string plantilla, string remitente, string nombreRemitente, string receptor, object model = null, List<string> rutasArchivos = null, String copiacarbon = null, String copiaOculta = null)
        {
            try
            {
                using (var SecurePassword = new SecureString())
                {
                    copiaOculta = copiaOculta ?? String.Empty;
                    copiacarbon = copiacarbon ?? String.Empty;
                    Array.ForEach(ConfigurationManager.AppSettings["ClaveCorreoSistema"].ToArray(), SecurePassword.AppendChar);

                    TemplateRender templateRender = new TemplateRender(Parent);

                    using (var smtpClient = new SmtpClient
                    {
                        Host = "mail.afari.pe", //Host Afari
                        Port = 25, //Port Afari
                        EnableSsl = true,
                        Timeout = 60000,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(ConfigurationManager.AppSettings["CorreoSistema"], SecurePassword)
                    })
                    {
                        var LstReceptores = receptor.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        LstReceptores = ConvertHelpers.RemoveDuplicates(LstReceptores);
                        //foreach (var r in LstReceptores)
                        //{
                            MailAddress from = new MailAddress(remitente, nombreRemitente);
                            //MailAddress to = new MailAddress(r);//receptor);
                            MailAddress to = new MailAddress(LstReceptores[0]);
                            using (var mailMessage = new MailMessage(from, to))
                            {
                                mailMessage.From = from;
                                //if (plantilla != "info")
                                //{
                                    for (int i = 1; i < LstReceptores.Count(); i++)
                                    {
                                        mailMessage.To.Add(LstReceptores[i]);
                                    }
                                //}
                                mailMessage.Body = templateRender.Render(plantilla, model);
                                mailMessage.IsBodyHtml = true;
                                mailMessage.Subject = asunto;
                                mailMessage.Sender = from;

                                if (plantilla != "info")
                                {
                                    mailMessage.Bcc.Add(new MailAddress(remitente));
                                    var LstReceptoresOcultos = copiaOculta.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var item in LstReceptoresOcultos)
                                    {
                                        mailMessage.Bcc.Add(item);
                                    }
                                }

                                if (!String.IsNullOrEmpty(copiacarbon) && copiacarbon.Length > 5)
                                {
                                    var ccAddress = copiacarbon.Split(',');
                                    foreach (var ccA in ccAddress)
                                    {
                                        if (!String.IsNullOrEmpty(ccA))
                                        {
                                            if (ccA.Contains("@afari.pe"))
                                            {
                                                mailMessage.CC.Add(new MailAddress(ccA));
                                            }
                                            else
                                            {
                                                if (plantilla != "info")
                                                {
                                                    mailMessage.CC.Add(new MailAddress(ccA));
                                                }
                                                else
                                                {
                                                    mailMessage.CC.Add(new MailAddress("cc_" + ccA));
                                                }
                                            }
                                        }
                                    }
                                }

                                if (rutasArchivos != null)
                                {

                                    foreach (var archivo in rutasArchivos)
                                    {
                                        var rutaAlArchivo = archivo;
                                        var data = new Attachment(rutaAlArchivo, MediaTypeNames.Application.Octet);
                                        var disposition = data.ContentDisposition;
                                        disposition.FileName = Path.GetFileNameWithoutExtension(archivo) + Path.GetExtension(archivo);
                                        disposition.CreationDate = DateTime.Now;
                                        disposition.ModificationDate = DateTime.Now;
                                        disposition.ReadDate = DateTime.Now;

                                        if (data != null)
                                            mailMessage.Attachments.Add(data);

                                    }
                                }
                                ServicePointManager.ServerCertificateValidationCallback =
                                delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                         X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
                                { return true; };

                                smtpClient.Send(mailMessage);
                            }
                        //}
                    }

                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SendEmailMasivo(string asunto, string plantilla, string remitente, string nombreRemitente, string receptor, ViewModel.Templates.infoViewModel model = null, List<string> rutasArchivos = null, String copiacarbon = null, String copiaOculta = null)
        {
            try
            {
                using (var SecurePassword = new SecureString())
                {
                    copiaOculta = copiaOculta ?? String.Empty;
                    copiacarbon = copiacarbon ?? String.Empty;
                    Array.ForEach(ConfigurationManager.AppSettings["ClaveCorreoSistema"].ToArray(), SecurePassword.AppendChar);

                    TemplateRender templateRender = new TemplateRender(Parent);

                    using (var smtpClient = new SmtpClient
                    {
                        Host = "mail.afari.pe", //Host Afari
                        Port = 25, //Port Afari
                        EnableSsl = true,
                        Timeout = 60000,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(ConfigurationManager.AppSettings["CorreoSistema"], SecurePassword)
                    })
                    {
                        var LstReceptores = receptor.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        LstReceptores = ConvertHelpers.RemoveDuplicates(LstReceptores);
                        //foreach (var r in LstReceptores)
                        //{
                        MailAddress from = new MailAddress(remitente, nombreRemitente);
                        //MailAddress to = new MailAddress(r);//receptor);
                        MailAddress to = new MailAddress(LstReceptores[0]);
                        using (var mailMessage = new MailMessage(from, to))
                        {
                            mailMessage.From = from;
                            //if (plantilla != "info")
                            //{
                            for (int i = 1; i < LstReceptores.Count(); i++)
                            {
                                mailMessage.To.Add(LstReceptores[i]);
                            }
                            //}
                            mailMessage.Body += $"<!DOCTYPE html>";
                            mailMessage.Body += $"<html xmlns='http://www.w3.org/1999/xhtml'>";
                            mailMessage.Body += $"<head>";
                            mailMessage.Body += $"    <meta content='text/html; charset=utf-8' http-equiv='Content-Type'>";
                            mailMessage.Body += $"    <meta content='width=device-width, initial-scale=1.0' name='viewport'>";
                            mailMessage.Body += $"    <title>@model.Titulo</title>";
                            mailMessage.Body += $"</head>";
                            mailMessage.Body += $"<body>";
                            mailMessage.Body += $"    <div style='font-family:Calibri;font-size:15px'>";
                            mailMessage.Body += $"        " + (model.Mensaje.Replace(" ", "&nbsp;").Replace("\n", "<br/>").ToString());
                            mailMessage.Body += $"    </div>";
                            mailMessage.Body += $"    <br />";
                            mailMessage.Body += $"    <p style='font-family:Calibri;font-size:15px'>    Saludos cordiales,</p>";
                            mailMessage.Body += $"    <br />";
                            if (!String.IsNullOrEmpty(model.Firma) && model.Firma.Length > 1)
                            {
                                mailMessage.Body += $"        <img src='http://afari.pe/intranet/Resources/Files/" +model.Firma + "' />";
                            }
                            mailMessage.Body += $"</body>";
                            mailMessage.Body += $"</html>";

                            mailMessage.IsBodyHtml = true;
                            mailMessage.Subject = asunto;
                            mailMessage.Sender = from;

                            if (plantilla != "info")
                            {
                                mailMessage.Bcc.Add(new MailAddress(remitente));
                                var LstReceptoresOcultos = copiaOculta.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var item in LstReceptoresOcultos)
                                {
                                    mailMessage.Bcc.Add(item);
                                }
                            }

                            if (!String.IsNullOrEmpty(copiacarbon) && copiacarbon.Length > 5)
                            {
                                var ccAddress = copiacarbon.Split(',');
                                foreach (var ccA in ccAddress)
                                {
                                    if (!String.IsNullOrEmpty(ccA))
                                    {
                                        if (ccA.Contains("@afari.pe"))
                                        {
                                            mailMessage.CC.Add(new MailAddress(ccA));
                                        }
                                        else
                                        {
                                            if (plantilla != "info")
                                            {
                                                mailMessage.CC.Add(new MailAddress(ccA));
                                            }
                                            else
                                            {
                                                mailMessage.CC.Add(new MailAddress("cc_" + ccA));
                                            }
                                        }
                                    }
                                }
                            }

                            if (rutasArchivos != null)
                            {

                                foreach (var archivo in rutasArchivos)
                                {
                                    var rutaAlArchivo = archivo;
                                    var data = new Attachment(rutaAlArchivo, MediaTypeNames.Application.Octet);
                                    var disposition = data.ContentDisposition;
                                    disposition.FileName = Path.GetFileNameWithoutExtension(archivo) + Path.GetExtension(archivo);
                                    disposition.CreationDate = DateTime.Now;
                                    disposition.ModificationDate = DateTime.Now;
                                    disposition.ReadDate = DateTime.Now;

                                    if (data != null)
                                        mailMessage.Attachments.Add(data);

                                }
                            }
                            ServicePointManager.ServerCertificateValidationCallback =
                            delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                     X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
                            { return true; };

                            smtpClient.Send(mailMessage);
                        }
                        //}
                    }

                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}