namespace Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Creature
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(250)]
        public string Description { get; set; }

        public double Damage { get; set; }

        public double Luck { get; set; }

        public double? Range { get; set; }

        public int MaxHealth { get; set; }

        public int Speed { get; set; }

        public int Armor { get; set; }

        public int? CombatModeId { get; set; }

        public int? RaceId { get; set; }

        public virtual CombatMode CombatMode { get; set; }

        public virtual Race Race { get; set; }
    }
}
