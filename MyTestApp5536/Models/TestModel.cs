using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyTestApp5536.Models
{
    public class TestModel
    {
        public string Name
        {
            get; set;
        }
        public int Id
        {
            get; set;
        }
        [NotMapped]
        public List<string> Filenames {
            get; set;
        } = new List<string>();
    }
}
