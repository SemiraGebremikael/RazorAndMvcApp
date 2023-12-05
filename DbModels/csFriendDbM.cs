using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System.Linq;

using Configuration;
using Models;
using Models.DTO;
using System.Diagnostics.Metrics;
using System.Reflection.Emit;

//DbModels namespace is the layer which contains all the C# models of
//the database tables Select queries as well as results from a call to a View,
//Stored procedure, or Function.

//C# classes corresponds to table structure (no suffix) or
//specific search results (DTO suffix)
namespace DbModels;
[Table("Friends", Schema = "supusr")]
[Index(nameof(FirstName), nameof(LastName))]
[Index(nameof(LastName), nameof(FirstName))]
public class csFriendDbM : csFriend, ISeed<csFriendDbM>
{
    static public new string Hello { get; } = $"Hello from namespace {nameof(DbModels)}, class {nameof(csFriendDbM)}";

    [Key]       // for EFC Code first
    public override Guid FriendId { get; set; }

    [Required]
    public override string FirstName { get; set; }

    //create own Foreign Key step 1
    [JsonIgnore]
    public virtual Guid? AddressId { get; set; }

    #region correcting the 1st migration error
    [NotMapped] //application layer can continue to read an Address without code change
    public override IAddress Address { get => AddressDbM; set => new NotImplementedException(); }

    [JsonIgnore]
    [ForeignKey("AddressId")]     //create own Foreign Key step 2
    public virtual csAddressDbM AddressDbM { get; set; } = null;    //This is implemented in the database table

    //a Friend can have 0 or many Pets
    [NotMapped] //application layer can continue to read a List of Pets without code change
    public override List<IPet> Pets { get => PetsDbM?.ToList<IPet>(); set => new NotImplementedException(); }

    [JsonIgnore]
    public virtual List<csPetDbM> PetsDbM { get; set; } = null;

    //a Friend can have 0 or many Quotes
    [NotMapped] //application layer can continue to read a List of Pets without code change
    public override List<IQuote> Quotes { get => QuotesDbM?.ToList<IQuote>(); set => new NotImplementedException(); }

    [JsonIgnore]
    public virtual List<csQuoteDbM> QuotesDbM { get; set; } = null;
    #endregion

    #region randomly seed this instance
    public override csFriendDbM Seed(csSeedGenerator sgen)
    {
        base.Seed(sgen);
        return this;
    }
    #endregion

    #region Update from DTO
    public csFriendDbM UpdateFromDTO(csFriendCUdto org)
    {
        FirstName = org.FirstName;
        LastName = org.LastName;
        Birthday = org.Birthday;

        return this;
    }
    #endregion

    #region constructors
    public csFriendDbM() { }
    public csFriendDbM(csFriendCUdto org)
    {
        FriendId = Guid.NewGuid();
        UpdateFromDTO(org);
    }
    #endregion
}

