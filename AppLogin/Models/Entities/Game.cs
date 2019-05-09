using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppLogin.Models.Entities
{
    public class Game
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(20)]
        public string Gender { get; set; }

        [StringLength(20)]
        public string Console { get; set; }
        public User User { get; set; }
    }
}
