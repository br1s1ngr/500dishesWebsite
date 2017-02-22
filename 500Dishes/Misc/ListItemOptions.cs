using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _500Dishes.Misc
{
    internal static class ListItemOptions
    {
        internal static List<SelectListItem> BankName () {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Select a bank", Value = ""},
                new SelectListItem { Text = "Access Bank", Value = "Access Bank" },
                new SelectListItem { Text = "Diamond Bank", Value = "Diamond Bank" },
                new SelectListItem { Text = "EcoBank", Value = "EcoBank" },
                new SelectListItem { Text = "FCMB", Value = "FCMB" },
                new SelectListItem { Text = "Fidelity Bank", Value = "Fidelity Bank" },
                new SelectListItem { Text = "GTBank", Value = "GTBank" },
                new SelectListItem { Text = "Heritage Bank", Value = "Heritage Bank" },
                new SelectListItem { Text = "Jaiz Bank", Value = "Jaiz Bank" },
                new SelectListItem { Text = "Keystone Bank", Value = "Keystone Bank" }
            };
        }
    }
}