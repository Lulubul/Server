namespace Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Hero
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(250)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Image { get; set; }

        public int? Damage { get; set; }

        public int? Type { get; set; }

        public int? RaceId { get; set; }

        public virtual Race Race { get; set; }

        public virtual Type Type1 { get; set; }
    }
}
