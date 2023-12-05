using System.Collections.Generic;
using Configuration;

//DbModels namespace is the layer which contains all the C# models of
//the database tables Select queries as well as results from a call to a View,
//Stored procedure, or Function.

//C# classes corresponds to table structure (no suffix) or
//specific search results (DTO suffix)
namespace Models;

public class csFriend : IFriend, ISeed<csFriend>
{
    static public string Hello { get; } = $"Hello from namespace {nameof(Models)}, class {nameof(csFriend)}";

    public virtual Guid FriendId { get; set; }

    public virtual string FirstName { get; set; }
    public virtual string LastName { get; set; }

    public virtual string Email { get; set; }
    public DateTime? Birthday { get; set; } = null;

    //One Friends can only have one address
    public virtual IAddress Address { get; set; } = null;

    //One Friends can have many favorite pets
    public virtual List<IPet> Pets { get; set; } = null;

    //One Friends can have many favorite quotes
    public virtual List<IQuote> Quotes { get; set; } = null;


    public string FullName => $"{FirstName} {LastName}";

    public override string ToString()
    {
        var sRet = $"{FullName} [{FriendId}]";

        if (Address != null)
            sRet += $"\n  - Lives at {Address}";
        else
            sRet += $"\n  - Has no address";


        if (Pets != null && Pets.Count > 0)
        {
            sRet += $"\n  - Has pets";
            foreach (var pet in Pets)
            {
                sRet += $"\n     {pet}";
            }
        }
        else
            sRet += $"\n  - Has no pets";

        if (Birthday != null)
        {
            sRet += $"\n  - Has birthday on {Birthday:D}";
        }

        return sRet;
    }

    #region contructors
    public csFriend() { }

    public csFriend(csFriend org)
    {
        this.Seeded = org.Seeded;

        this.FriendId = org.FriendId;
        this.FirstName = org.FirstName;
        this.LastName = org.LastName;
        this.Email = org.Email;

        //use the ternary operator to create only if the orginal is not null
        this.Address = (org.Address != null)? new csAddress((csAddress)org.Address): null;

        //using Linq Select and copy contructor to create a list copy
        this.Pets = (org.Pets != null) ? org.Pets.Select(p => new csPet((csPet) p)).ToList<IPet>() : null;
    }
    #endregion

    #region randomly seed this instance
    public bool Seeded { get; set; } = false;

    public virtual csFriend Seed(csSeedGenerator sgen)
    {
        Seeded = true;
        FriendId = Guid.NewGuid();
        FirstName = sgen.FirstName;
        LastName = sgen.LastName;
        Email = sgen.Email(FirstName, LastName);
        Birthday = (sgen.Bool) ? sgen.DateAndTime(1970, 2000) : null;

        return this;
    }
    #endregion
}

