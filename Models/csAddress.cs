using System;
using System.Diagnostics.Metrics;

namespace Models
{
	public class csAddress : IAddress, ISeed<csAddress>, IEquatable<csAddress>
    {
        public virtual Guid AddressId { get; set; }

        public virtual string StreetAddress { get; set; }
        public virtual int ZipCode { get; set; }
        public virtual string City { get; set; }
        public virtual string Country { get; set; }

        public override string ToString() => $"{StreetAddress}, {ZipCode} {City}, {Country}";

        public virtual List<IFriend> Friends { get; set; } = null;

        #region constructors
        public csAddress() { }
        public csAddress(csAddress org)
        {
            this.Seeded = org.Seeded;

            this.AddressId = org.AddressId;
            this.StreetAddress = org.StreetAddress;
            this.ZipCode = org.ZipCode;
            this.City = org.City;
            this.Country = org.Country;
        }
        #endregion

        #region implementing IEquatable

        public bool Equals(csAddress other) => (other != null) ? ((StreetAddress, ZipCode, City, Country) ==
            (other.StreetAddress, other.ZipCode, other.City, other.Country)) : false;

        public override bool Equals(object obj) => Equals(obj as csAddress);
        public override int GetHashCode() => (StreetAddress, ZipCode, City, Country).GetHashCode();

        #endregion

        #region randomly seed this instance
        public bool Seeded { get; set; } = false;

        public virtual csAddress Seed(csSeedGenerator sgen)
        {
            Seeded = true;
            AddressId = Guid.NewGuid();

            Country = sgen.Country;
            StreetAddress = sgen.StreetAddress(Country);
            ZipCode = sgen.ZipCode;
            City = sgen.City(Country);

            return this;
        }
        #endregion
    }
}

