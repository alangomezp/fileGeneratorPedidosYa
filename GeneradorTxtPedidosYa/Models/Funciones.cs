using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using System.Linq;
using System.Net.Mail;
using AegisImplicitMail;


namespace GeneradorTxtPedidosYa.Models
{
    class Funciones
    {
        HiperOleEntities db = new HiperOleEntities();
        string[] listaSucursales = new string[] { "sucursal1", "sucursal2", };

        public class PedidosYa : ClassMap<Reporte_PedidosYa_Result>
        {
            public PedidosYa()
            {
                Map(m => m.CODIGO).Index(0);
                Map(m => m.DESCRIPCION).Index(1);
                Map(m => m.PRICE).Index(2);
                Map(m => m.EXISTENCIA).Index(3);
            }

        }

        public void generarArchivo() 
        {
            List<Reporte_PedidosYa_Result> lista = new List<Reporte_PedidosYa_Result>();
            var codigosProductos = leerExcel();
            var codigos = String.Join(",", codigosProductos);

            for (int i = 0; i < listaSucursales.Length; i++)
            {
                
                lista = new List<Reporte_PedidosYa_Result>();
                string sucursal = listaSucursales[i];
                var CodigoPedidosYa = db.Sucursales_Activas.Where(a => a.CODIGO == sucursal).Select(a => a.PEDIDOSYA).SingleOrDefault();
                var nameSucursal = sucursal.Substring(4, 2);
                
                var resultados = db.Reporte_PedidosYa(sucursal, codigos);

                foreach (var res in resultados)
                {
                    lista.Add(new Reporte_PedidosYa_Result
                    {

                        CODIGO = res.CODIGO,
                        DESCRIPCION = res.DESCRIPCION,
                        PRICE = res.PRICE,
                        EXISTENCIA = res.EXISTENCIA

                    });
                }

                crearCSV(lista, CodigoPedidosYa, nameSucursal);
                subirViaSFTP(CodigoPedidosYa, nameSucursal);
            }

            enviarCorreo();

        }
        public string[] leerExcel()
        {

            List<String> listadoArticulos2 = new List<String>();

            string filePath = @"C:\\ListadoProductos.csv";

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    do
                    {
                        while (reader.Read())
                        {
                            var result = reader;

                            listadoArticulos2.Add(result.GetValue(0).ToString());

                        }
                    } while (reader.NextResult());


                    return listadoArticulos2.ToArray();
                }
            }
        }

        public void crearCSV(IEnumerable<Reporte_PedidosYa_Result> data, string CodigoPedidosYa, string name)
        {

            string source = name == "BC" ? @"C:\ArchivosPedidosYa\" + CodigoPedidosYa + ".csv" : @"C:\ArchivosPedidosYa\" + CodigoPedidosYa + "-" + name + ".csv";
            using (var writer = new StreamWriter(source))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.Configuration.Delimiter = ";";
                csvWriter.Configuration.HasHeaderRecord = false;
                csvWriter.Configuration.RegisterClassMap<PedidosYa>();

                csvWriter.WriteRecords(data);

            }
        }

        public void subirViaSFTP(string CodigoPedidosYa, string nameSucursal)
        {
            string source = @"C:\ArchivosPedidosYa\" + CodigoPedidosYa + "-" + nameSucursal + ".csv";
            string host = "servidor_de_pedidos_ya";
            string username = "user";
            string password = "password";
            int port = 60;

            Sftp.UploadSFTPFile(host, username, password, source, port);

        }
       
        public String enviarCorreo()
        {
            DateTime date = DateTime.Now;
            var emisor = "correo@domain.com";
            var pass = "password";
            var mensaje = new MailMessage();

            for (int i = 0; i < listaSucursales.Length; i++)
            {
                string sucursal = listaSucursales[i];
                string file = db.Sucursales_Activas.Where(a => a.CODIGO == sucursal).Select(a => a.PEDIDOSYA).SingleOrDefault();
                var name = sucursal.Substring(4, 2);
                file = @"C:\\ArchivosPedidosYa\\" + file + "-" + name + ".csv";
                StreamReader st = new StreamReader(file);

                System.Net.Mime.ContentType contentType = new System.Net.Mime.ContentType(System.Net.Mime.MediaTypeNames.Text.Plain);
                Attachment attach = new Attachment(file, contentType);
                contentType.Name = db.Sucursales_Activas.Where(a => a.CODIGO == sucursal).Select(a => a.PEDIDOSYA).SingleOrDefault() + ".csv"; 
                mensaje.Attachments.Add(attach);

            }

            SmtpClient smtpServer = new SmtpClient("smtp.office365.com", 587);
            mensaje.From = new MailAddress(emisor);
            mensaje.To.Add(new MailAddress("recptor@domain.com"));
            mensaje.Subject = "CSV Pedidos Ya";
            mensaje.IsBodyHtml = true;

            mensaje.Body = " Saludos, adjunto enviamos los archivos diarios de Pedidos Ya  <br><br> <b> Mensaje Automatico, por favor no contestar a este correo.</b>";

            smtpServer.Credentials = new System.Net.NetworkCredential(emisor, pass);

            smtpServer.EnableSsl = true;
            try
            {
                smtpServer.Send(mensaje);
                return "Se envio correctamente";

            }
            catch (Exception e)
            {

                return e.Message;
            }


        }



    }
}
