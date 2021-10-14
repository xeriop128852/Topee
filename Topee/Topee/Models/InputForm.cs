using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Topee.Models
{
    public class InputForm
    {
        //Users
        public string Username { get; set; }
        public string Password { get; set; }
        public string Password_confirm { get; set; }

        //bill
        public decimal Bonus { get; set; }
        public decimal Discount { get; set; }

        /*
        public string Realname { get; set; }
        public string Phonenumber { get; set; }
        public string Address { get; set; }
        //Goods
        public string Goodsname { get; set; }
        public string Price { get; set; }
        public string Describe { get; set; }
        public string Picture { get; set; }
        public string search { get; set; }
        //store
        public string Storename { get; set; }
        */


    }
}