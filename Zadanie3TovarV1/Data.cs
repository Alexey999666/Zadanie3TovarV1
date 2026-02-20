using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zadanie3TovarV1.ModelsDB;

namespace Zadanie3TovarV1
{
    public static class Data
    {
        public static User CurrentUser { get; set; }
        public static bool IsLoggedIn { get; set; }
        public static Product CurrentProduct { get; set; }
    }
}
