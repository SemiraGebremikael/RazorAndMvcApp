using System;
using Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using AppMusicRazor.SeidoHelpers;
using Microsoft.AspNetCore.Mvc;

namespace AppGoodFriendsMVC.Models
{
	public class FriendEditModel
	{
        [BindProperty]
        public string PageHeader { get; set; }

        [BindProperty]
        public csFriendIM FriendInput { get; set; }

        public List<SelectListItem> AnimalKind { set; get; } = new List<SelectListItem>().PopulateSelectList<enAnimalKind>();

        public List<SelectListItem> AnimalMood { set; get; } = new List<SelectListItem>().PopulateSelectList<enAnimalMood>();

        //For Validation
        public reModelValidationResult ValidationResult { get; set; } = new reModelValidationResult(false, null, null);


        #region Input Model
        //InputModel (IM) is locally declared classes that contains ONLY the properties of the Model
        //that are bound to the <form> tag
        //EVERY property must be bound to an <input> tag in the <form>
        //These classes are in center of ModelBinding and Validation
        public enum enStatusIM { Unknown, Unchanged, Inserted, Modified, Deleted }
        public class csFriendIM
        {
            public enStatusIM StatusIM { get; set; }

            public Guid FriendId { get; set; }

            [Required(ErrorMessage = "You must provide a first name")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "You must provide a last name")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "You must provide a Email")]
            public string Email { get; set; }

            //This is because I want to confirm modifications in PostEditAlbum
            [Required(ErrorMessage = "You must provide a first name")]
            public string editFirstName { get; set; }

            [Required(ErrorMessage = "You must provide a last name")]
            public string editLastName { get; set; }

            [Required(ErrorMessage = "You must provide a Email")]
            public string editEmail { get; set; }

            public csAddressIM Address { get; set; }

            public reModelValidationResult ValidationResult { get; set; }

            public List<csPetIM> Pets { get; set; } = new List<csPetIM>();
            public List<csQuoteIM> Quotes { get; set; } = new List<csQuoteIM>();

            public IFriend UpdateModel(IFriend model)
            {
                model.FriendId = this.FriendId;
                model.FirstName = this.FirstName;
                model.LastName = this.LastName;
                model.Email = this.Email;
                model.Address = Address.UpdateModel(model.Address);
                return model;
            }

            public csFriendIM()
            {
                StatusIM = enStatusIM.Unchanged;
                Address = new csAddressIM();
            }
            public csFriendIM(csFriendIM original)
            {
                StatusIM = original.StatusIM;
                FriendId = original.FriendId;
                FirstName = original.FirstName;
                LastName = original.LastName;
                Email = original.Email;

                editFirstName = original.editFirstName;
                editLastName = original.editLastName;
                editEmail = original.editEmail;


                Address = original.Address;
            }
            public csFriendIM(IFriend model)
            {
                StatusIM = enStatusIM.Unchanged;
                FriendId = model.FriendId;
                FirstName = editFirstName = model.FirstName;
                LastName = editLastName = model.LastName;
                Email = editEmail = model.Email;

                Address = new csAddressIM(model.Address);

                Pets = model.Pets?.Select(m => new csPetIM(m)).ToList() ?? new List<csPetIM>();
                Quotes = model.Quotes?.Select(m => new csQuoteIM(m)).ToList() ?? new List<csQuoteIM>();
            }

            public csPetIM NewPet { get; set; } = new csPetIM();

            public csQuoteIM NewQuote { get; set; } = new csQuoteIM();
        }

        public class csAddressIM
        {
            public enStatusIM StatusIM { get; set; }

            public Guid AddressId { get; set; }

            [Required(ErrorMessage = "You must enter an StreetAddress")]
            public string StreetAddress { get; set; }

            [Required(ErrorMessage = "You must enter an ZipCode")]
            public int ZipCode { get; set; }


            [Required(ErrorMessage = "You must enter an City")]
            public string City { get; set; }


            [Required(ErrorMessage = "You must enter an Country")]
            public string Country { get; set; }


            [Required(ErrorMessage = "You must enter an StreetAddress")]
            public string editStreetAddress { get; set; }


            [Required(ErrorMessage = "You must enter an ZipCode")]
            public int editZipCode { get; set; }

            [Required(ErrorMessage = "You must enter an City")]
            public string editCity { get; set; }

            [Required(ErrorMessage = "You must enter an Country")]
            public string editCountry { get; set; }


            public IAddress UpdateModel(IAddress model)
            {
                model.AddressId = this.AddressId;
                model.StreetAddress = this.StreetAddress;
                model.ZipCode = this.ZipCode;
                model.City = this.City;
                model.Country = this.Country;

                return model;
            }

            public csAddressIM() { }
            public csAddressIM(csAddressIM original)
            {
                StatusIM = original.StatusIM;
                AddressId = original.AddressId;
                StreetAddress = original.StreetAddress;
                ZipCode = original.ZipCode;
                City = original.City;
                Country = original.Country;

                editStreetAddress = original.editStreetAddress;
                editZipCode = original.editZipCode;
                editCity = original.editCity;
                editCountry = original.editCountry;

            }
            public csAddressIM(IAddress model)
            {
                StatusIM = enStatusIM.Unchanged;
                AddressId = model.AddressId;
                StreetAddress = editStreetAddress = model.StreetAddress;
                ZipCode = editZipCode = model.ZipCode;
                City = editCity = model.City;
                StreetAddress = editStreetAddress = model.StreetAddress;
                Country = editStreetAddress = model.Country;

            }
        }

        public class csPetIM
        {
            public enStatusIM StatusIM { get; set; }

            public Guid PetId { get; set; }

            [Required(ErrorMessage = "You must enter an Animal Kind")]
            public enAnimalKind Kind { get; set; }

            [Required(ErrorMessage = "You must enter an Animal Mood")]
            public enAnimalMood Mood { get; set; }


            [Required(ErrorMessage = "You must enter an Name")]
            public string Name { get; set; }

            [Required(ErrorMessage = "You must select a pet Kind")]
            public enAnimalKind? editKind { get; set; }

            [Required(ErrorMessage = "You must enter a pet Mood")]
            public enAnimalMood? editMood { get; set; }

            [Required(ErrorMessage = "You must enter an Name")]
            public string editName { get; set; }


            public IPet UpdateModel(IPet model)
            {
                model.PetId = this.PetId;
                model.Kind = this.Kind;
                model.Mood = this.Mood;
                model.Name = this.Name;

                return model;
            }

            public csPetIM() { }
            public csPetIM(csPetIM original)
            {
                StatusIM = original.StatusIM;
                PetId = original.PetId;
                Name = original.Name;

                editName = original.editName;

            }
            public csPetIM(IPet model)
            {
                StatusIM = enStatusIM.Unchanged;
                PetId = model.PetId;
                Kind = model.Kind;
                Mood = model.Mood;
                Name = editName = model.Name;

            }
        }

        public class csQuoteIM
        {
            public enStatusIM StatusIM { get; set; }

            public Guid QuoteId { get; set; }

            [Required(ErrorMessage = "You must enter an Quote")]
            public string Quote { get; set; }

            [Required(ErrorMessage = "You must enter an Author")]
            public string Author { get; set; }



            [Required(ErrorMessage = "You must enter an Quote")]
            public string editQuote { get; set; }

            [Required(ErrorMessage = "You must enter an Author")]
            public string editAuthor { get; set; }


            public IQuote UpdateModel(IQuote model)
            {
                model.QuoteId = this.QuoteId;
                model.Quote = this.Quote;
                model.Author = this.Author;

                return model;
            }

            public csQuoteIM() { }
            public csQuoteIM(csQuoteIM original)
            {
                StatusIM = original.StatusIM;
                QuoteId = original.QuoteId;
                Quote = original.Quote;
                Author = original.Author;

                editQuote = original.editQuote;
                editAuthor = original.editAuthor;

            }
            public csQuoteIM(IQuote model)
            {
                StatusIM = enStatusIM.Unchanged;
                QuoteId = model.QuoteId;
                Quote = editQuote = model.Quote;
                Author = editAuthor = model.Author;
            }
        }

        #endregion

    }
}

