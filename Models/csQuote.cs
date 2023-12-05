using System;
namespace Models
{
    public class csQuote : IQuote, ISeed<csQuote>, IEquatable<csQuote>
    {
        public virtual Guid QuoteId { get; set; }
        public virtual string Quote { get; set; }
        public virtual string Author { get; set; }

        //One Quote can have many friends
        public virtual List<IFriend> Friends { get; set; } = null;

        #region constructors
        public csQuote() { }

        public csQuote(csSeededQuote goodQuote)
        {
            QuoteId = Guid.NewGuid();
            Quote = goodQuote.Quote;
            Author = goodQuote.Author;
            Seeded = true;
        }

        #endregion

        #region implementing IEquatable

        public bool Equals(csQuote other) => (other != null) ? ((Quote, Author) ==
            (other.Quote, other.Author)) : false;

        public override bool Equals(object obj) => Equals(obj as csQuote);
        public override int GetHashCode() => (Quote, Author).GetHashCode();

        #endregion

        #region randomly seed this instance
        public bool Seeded { get; set; } = false;

        public virtual csQuote Seed(csSeedGenerator sgen)
        {
            Seeded = true;
            QuoteId = Guid.NewGuid();

            var _quote = sgen.Quote;
            Quote = _quote.Quote;
            Author = _quote.Author;

            return this;
        }
        #endregion
    }
}

