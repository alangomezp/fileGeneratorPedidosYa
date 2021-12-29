using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace GeneradorTxtPedidosYa
{
    class Sftp
    {
        public static void UploadSFTPFile(string host, string username,
                                           string password, string sourcefile, int port)
        {
            using (SftpClient client = new SftpClient(host, port, username, password))
            {
                client.Connect();

                var file = Path.GetFileName(sourcefile);

                using (FileStream fs = new FileStream(sourcefile, FileMode.Open))
                {
                    if (fs != null)
                    {
                        client.BufferSize = 4 * 1024;
                        client.UploadFile(fs, file, null);
                    }

                }
            }
        }
    }
}
 