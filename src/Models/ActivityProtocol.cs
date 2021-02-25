using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using src.Areas.Identity.Data;

namespace src.Models
{
    /// Activity Protocol lets the user create entries about their activity
    /// at certain times throughout the day.
    /// user can also add a journal entry for more personal description of their day.
    /// everything is read-only, there should be no editing after creation!
    public class ActivityProtocol
    {
        //everything needs to be public because else code generation complains
        //not 100% sure why yet

        public ActivityProtocol() 
        {
            Entries = new List<ProtocolEntry>();
        }

        //need a primary key, because database
        [Key]
        public int Id { get; set; }
        
        /// on which date this protocol was created
        public DateTime Date { get; set; }

        /// single journal entry for this day (unlimited length)
        [DisplayName("Journal Entry")]
        public string JournalEntry { get; set; }

        //instead of configuring the relationship in ApplicationDbContext
        //we could also add an Annotation here:
        //[InverseProperty("Protocol")]
        /// single entries with time and description
        public List<ProtocolEntry> Entries { get; set; }

        [ForeignKey("OwnerId")]
        public ApplicationUser Owner { get; set; }
        public string OwnerId { get; set; }

        public class ProtocolEntry
        {
            // need primary key because database
            [Key]
            public int Id { get; set; }

            /// description of users activity
            [MaxLength(200)]
            public string Description { get; set; }

            /// when this entry was added
            public DateTime Time { get; set; }

            //needed because list in protocol class!
            [ForeignKey("ProtocolId")]
            public ActivityProtocol Protocol { get; set; }
            public int ProtocolId { get; set; }
        }
    }
}