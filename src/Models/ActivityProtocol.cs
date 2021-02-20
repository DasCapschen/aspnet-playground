using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        //need a primary key, because database
        [Key]
        public int Id { get; set; }
        /// on which date this protocol was created
        public DateTimeOffset Date { get; set; }
        /// single journal entry for this day (unlimited length)
        public string JournalEntry { get; set; }

        //instead of configuring the relationship in ApplicationDbContext
        //we could also add an Annotation here:
        //[InverseProperty("Protocol")]
        /// single entries with time and description
        public List<ProtocolEntry> Entries { get; set; }

        public class ProtocolEntry
        {
            // need primary key because database
            [Key]
            public int Id { get; set; }
            /// description of users activity (TODO: max length)
            public string Description { get; set; }
            /// when this entry was added
            public DateTimeOffset Time { get; set; }

            //needed because list in protocol class!
            [ForeignKey("ProtocolId")]
            public ActivityProtocol Protocol { get; set; }
            public int ProtocolId { get; set; }
        }
    }
}