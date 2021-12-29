using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneradorTxtPedidosYa.Models;

namespace GeneradorTxtPedidosYa
{
    class Program
    {
        static void Main(string[] args)
        {
            Funciones funct = new Funciones();

            funct.generarArchivo();
            

        }
    }
}
