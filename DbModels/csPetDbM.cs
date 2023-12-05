using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

using Configuration;
using Models;
using Models.DTO;

namespace DbModels
{
    [Table("Pets", Schema = "supusr")]
    public class csPetDbM : csPet, ISeed<csPetDbM>
    {
        [Key]       // for EFC Code first
        public override Guid PetId { get; set; }

        //create own Foreign Key step 1
        [JsonIgnore]
        public virtual Guid FriendId { get; set; }  //Enforces Cascade Delete
        //public virtual Guid? FriendId { get; set; } // Enforces Cascade No Action

        [Required]
        [StringLength(50)]     //String lenght max check constraint
        public override string Name { get; set; }
        
        #region correcting the 1st migration error
        [JsonIgnore]
        [ForeignKey("FriendId")]     //create own Foreign Key step 2
        public virtual csFriendDbM FriendDbM { get; set; } = null;    //This is implemented in the database table        

        [NotMapped] //application layer can continue to read an Address without code change
        public override IFriend Friend { get => FriendDbM; set => new NotImplementedException(); }
        #endregion

        #region adding more readability to an enum type in the database
        public virtual string strKind
        {
            get => Kind.ToString();
            set { }  //set is needed by EFC to include in the database, so I make it to do nothing
        }
        public virtual string strMood
        {
            get => Mood.ToString();
            set { } //set is needed by EFC to include in the database, so I make it to do nothing
        }
        #endregion

        #region randomly seed this instance
        public override csPetDbM Seed(csSeedGenerator sgen)
        {
            base.Seed(sgen);
            return this;
        }
        #endregion

        #region Update from DTO
        public csPetDbM UpdateFromDTO(csPetCUdto org)
        {
            if (org == null) return null;

            Kind = org.Kind;
            Mood = org.Mood;
            Name = org.Name;

            //We will set this when DbM model is finished
            //FriendId = org.FriendId;

            return this;
        }
        #endregion

        #region constructors
        public csPetDbM() { }
        public csPetDbM(csPetCUdto org)
        {
            PetId = Guid.NewGuid();
            UpdateFromDTO(org);
        }
        #endregion
    }
}

