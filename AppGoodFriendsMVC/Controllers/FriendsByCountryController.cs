using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AppGoodFriendsMVC.Models;
using AppMusicRazor.SeidoHelpers;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

using Models;
using Models.DTO;
using Services;
using static AppGoodFriendsMVC.Models.FriendEditModel;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppGoodFriendsMVC.Controllers
{
    public class FriendsByCountryController : Controller
    {
        private readonly IFriendsService _service;
        loginUserSessionDto _usr;


        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 4;

        //Inject services just like in WebApi
        public FriendsByCountryController(IFriendsService service) => _service = service;

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpGet]
        public async Task<IActionResult> FriendsByCountry(string country)
        {
            var vm = new FriendsByCountryModel();

            vm.Friends = await _service.ReadFriendsAsync(_usr, true, false, "", 0, 500);

            // Gruppera vänner efter land och räkna antalet vänner i varje land

            vm.FriendsByCountry = vm.Friends
                .Where(f => f.Address != null && !string.IsNullOrEmpty(f.Address.Country))
                .GroupBy(f => f.Address.Country)
                .Select(g => new CountryFriendCount
                {
                    Country = g.Key,
                    FriendCount = g.Count()
                })
                .ToList();

            return View(vm);

        }

        public async Task<IActionResult> FriendsInCountryOrCity(string country)
        {
            var friends = await _service.ReadFriendsAsync(_usr, true, false, "", 0, 500);

            var friendsInSelectedCountry = friends
                .Where(f => f.Address != null && f.Address.Country == country)
                .ToList();

            var vm = new FriendsInCountryOrCityModel
            {
                Friends = friendsInSelectedCountry,
                Addresses = friendsInSelectedCountry.Select(f => f.Address).Distinct().ToList(),
                Country = country
            };

            return View(vm);
        }


        public async Task<IActionResult> FriendsOnCity(string city)
        {
            var vm = new FriendsOnCityModel
            {
                City = city,
                FriendsInCity = new List<IFriend>(),
                NumberOfPets = new Dictionary<Guid, int>(),
                NumberOfQuotes = new Dictionary<Guid, int>()
            };


            var addresses = await _service.ReadAddressesAsync(_usr, true, false, city, 0, 500);

            vm.FriendsInCity = addresses.SelectMany(a => a.Friends).ToList();

            // Calculate the number of pets and quotes for each friend
            foreach (var friend in vm.FriendsInCity)
            {
                var friendDetails = await _service.ReadFriendAsync(_usr, friend.FriendId, false);

                if (friendDetails != null)
                {
                    vm.NumberOfPets[friend.FriendId] = friendDetails.Pets?.Count ?? 0;
                    vm.NumberOfQuotes[friend.FriendId] = friendDetails.Quotes?.Count ?? 0;
                }
            }

            return View(vm);

        }


        public async Task<IActionResult> DetailsOfFriend(Guid id)
        {
            var vm = new DetailsOfFriendModel();

            try
            {
                vm.Friend = await _service.ReadFriendAsync(_usr, id, false);
                if (vm.Friend == null)
                {
                    vm.ErrorMessage = "No friend found with the provided ID.";
                    return View(vm);
                }
            }
            catch (Exception e)
            {
                vm.ErrorMessage = e.Message;
            }

            return View(vm);
        }

        public async Task<IActionResult> ListOfAllFriends(int? pageIndex)
        {
            PageIndex = pageIndex ?? 1;

            var allFriends = await _service.ReadFriendsAsync(_usr, true, false, "", 0, 100);

            var vm = new ListOfAllFriendsModel
            {
                PaginatedFriends = new ListOfAllFriendsModel.PaginatedFriendsViewModel(allFriends, allFriends.Count, PageIndex, PageSize)
            };

            return View(vm);
        }



        #region HTTP Requests for EditFriend
        public async Task<IActionResult> FriendEdit(Guid? id)
        {
            Guid? _friendId = id;
            if (_friendId.HasValue)
            {
                //Read a friends from 

                var f = await _service.ReadFriendAsync(_usr, _friendId.Value, false);

                var vm = new FriendEditModel()
                {
                    //Populate the InputModel from the friend

                    FriendInput = new FriendEditModel.csFriendIM(f),
                    PageHeader = "Edit and Add details of a Friend"
                };
                return View(vm);
            }

            else
            {
                var vm = new FriendEditModel()
                {
                    // Create an empty friend
                    FriendInput = new FriendEditModel.csFriendIM(),
                    PageHeader = "Create a new a  Friend",
                };
                vm.FriendInput.StatusIM = FriendEditModel.enStatusIM.Inserted;
                return View(vm);
            }
        }

        [HttpPost]
        public IActionResult DeletePet(Guid petId, FriendEditModel vm)
        {
            //Set the pet as deleted, it will not be rendered
            vm.FriendInput.Pets.First(a => a.PetId == petId).StatusIM = FriendEditModel.enStatusIM.Deleted;

            return View("FriendEdit", vm);
        }

        [HttpPost]
        public IActionResult AddPet(FriendEditModel vm)
        {
            string[] keys = { "FriendInput.NewPet.Name",
                               "FriendInput.NewPet.Kind",
                               "FriendInput.NewPet.Mood"};

            if (!ModelState.IsValidPartially(out reModelValidationResult modelValidationResult, keys))
            {
                vm.ValidationResult = modelValidationResult;

                return View("FriendEdit", vm);
            }

            //Set the pet as Inserted, it will later be inserted in the database
            vm.FriendInput.NewPet.StatusIM = FriendEditModel.enStatusIM.Inserted;

            //Need to add a temp Guid so it can be deleted and edited in the form
            //A correct Guid will be created by the DTO when Inserted into the database
            vm.FriendInput.NewPet.PetId = Guid.NewGuid();

            //Add it to the Input Models pets
            vm.FriendInput.Pets.Add(new FriendEditModel.csPetIM(vm.FriendInput.NewPet));

            //Clear the NewPet so another pet can be added
            vm.FriendInput.NewPet = new FriendEditModel.csPetIM();

            return View("FriendEdit", vm);
        }

        [HttpPost]
        public IActionResult EditPet(Guid petId, FriendEditModel vm)
        {
            int idx = vm.FriendInput.Pets.FindIndex(a => a.PetId == petId);
            string[] keys = { $"FriendInput.Pets[{idx}].editName",
                              $"FriendInput.Pets[{idx}].editKind",
                              $"FriendInput.Pets[{idx}].editMood"};

            if (!ModelState.IsValidPartially(out reModelValidationResult validationResult, keys))
            {
                vm.ValidationResult = validationResult;
                return View("FriendEdit", vm);
            }

            //Set the pets as Modified, it will later be updated in the database
            var p = vm.FriendInput.Pets.First(a => a.PetId == petId);
            if (p.StatusIM != FriendEditModel.enStatusIM.Inserted)
            {
                p.StatusIM = FriendEditModel.enStatusIM.Modified;
            }

            //Implement the changes
            p.Name = p.editName;
            p.Kind = (enAnimalKind)p.editKind;
            p.Mood = (enAnimalMood)p.editMood;
            return View("FriendEdit", vm);
        }

        [HttpPost]
        public IActionResult DeleteQuote(Guid quoteId, FriendEditModel vm)
        {
            //Set the Quote as deleted, it will not be rendered
            vm.FriendInput.Quotes.First(a => a.QuoteId == quoteId).StatusIM = FriendEditModel.enStatusIM.Deleted;

            return View("FriendEdit", vm);
        }

        [HttpPost]
        public IActionResult AddQuote(FriendEditModel vm)
        {
            string[] keys = { "FriendInput.NewQuote.Quote",
                              "FriendInput.NewQuote.Author"};

            if (!ModelState.IsValidPartially(out reModelValidationResult validationResult, keys))
            {
                vm.ValidationResult = validationResult;
                return View("FriendEdit", vm);
            }

            //Set the Quote as Inserted, it will later be inserted in the database
            vm.FriendInput.NewQuote.StatusIM = enStatusIM.Inserted;

            //Need to add a temp Guid so it can be deleted and editited in the form
            //A correct Guid will be created by the DTO when Inserted into the database
            vm.FriendInput.NewQuote.QuoteId = Guid.NewGuid();

            //Add it to the Input Models Quote
            vm.FriendInput.Quotes.Add(new FriendEditModel.csQuoteIM(vm.FriendInput.NewQuote));

            //Clear the NewQuote so another Quote can be added
            vm.FriendInput.NewQuote = new FriendEditModel.csQuoteIM();

            return View("FriendEdit", vm);
        }

        [HttpPost]
        public IActionResult EditQuote(Guid quoteId, FriendEditModel vm)
        {
            int idx = vm.FriendInput.Quotes.FindIndex(a => a.QuoteId == quoteId);
            string[] keys = { $"FriendInput.Quotes[{idx}].editQuote",
                            $"FriendInput.Quotes[{idx}].editAuthor"};

            if (!ModelState.IsValidPartially(out reModelValidationResult validationResult, keys))
            {
                vm.ValidationResult = validationResult;
                return View("FriendEdit", vm);
            }

            //it will later be updated in the database
            var q = vm.FriendInput.Quotes.First(q => q.QuoteId == quoteId);
            if (q.StatusIM != FriendEditModel.enStatusIM.Inserted)
            {
                q.StatusIM = FriendEditModel.enStatusIM.Modified;
            }

            //Implement the changes
            q.Quote = q.editQuote;
            q.Author = q.editAuthor;

            return View("FriendEdit", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Undo(FriendEditModel vm)
        {
            //Reload Friend from Database
            var f = await _service.ReadFriendAsync(_usr, vm.FriendInput.FriendId, false);

            //Repopulate the InputModel
            vm.FriendInput = new FriendEditModel.csFriendIM(f);
            return View("FriendEdit", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Save(FriendEditModel vm)
        {
            string[] keys = { "FriendInput.FirstName",
                              "FriendInput.LastName",
                              "FriendInput.Email"};

            if (!ModelState.IsValidPartially(out reModelValidationResult validationResult, keys))
            {
                vm.FriendInput.ValidationResult = validationResult;
                return View("FriendEdit", vm);
            }

            //First, are we creating a new Friend or editing another
            if (vm.FriendInput.StatusIM == FriendEditModel.enStatusIM.Inserted)
            {
                var fDto = new csFriendCUdto();

                //create the Friend in the database
                fDto.FirstName = vm.FriendInput.FirstName;
                fDto.FirstName = vm.FriendInput.LastName;
                fDto.Email = vm.FriendInput.Email;

                // use AddressId from FriendInput.Address
                fDto.AddressId = vm.FriendInput.Address.AddressId;


                var newF = await _service.CreateFriendAsync(_usr, fDto);
                //get the newly created FriendId
                vm.FriendInput.FriendId = newF.FriendId;

            }

            // använd AddressId från FriendInput.Address
            var addressDto = new csAddressCUdto
            {
                AddressId = vm.FriendInput.Address.AddressId,
                StreetAddress = vm.FriendInput.Address.StreetAddress,
                ZipCode = vm.FriendInput.Address.ZipCode,
                City = vm.FriendInput.Address.City,
                Country = vm.FriendInput.Address.Country
            };


            //Do all updates for Pets
            IFriend f = await SavePets(vm);

            // Do all updates for Quotes
            f = await SaveQuotes(vm);

            //Finally, update the friend itself
            f.FirstName = vm.FriendInput.FirstName;
            f.LastName = vm.FriendInput.LastName;
            f.Email = vm.FriendInput.Email;


            // använd AddressId från FriendInput.Address
            var address = new csAddress
            {
                AddressId = vm.FriendInput.Address.AddressId,
                StreetAddress = vm.FriendInput.Address.StreetAddress,
                ZipCode = vm.FriendInput.Address.ZipCode,
                City = vm.FriendInput.Address.City,
                Country = vm.FriendInput.Address.Country
            };


            // Uppdatera befintlig adress
            var updatedAddress = await _service.UpdateAddressAsync(_usr, addressDto);
            f.Address = updatedAddress;


            f = await _service.UpdateFriendAsync(_usr, new csFriendCUdto(f));

            if (vm.FriendInput.StatusIM == FriendEditModel.enStatusIM.Inserted)
            {
                return Redirect($"~/FriendsByCountry/ListOfAllFriends");
            }

            return Redirect($"~/FriendsByCountry/ListOfAllFriends");
        }

        #endregion

        #region InputModel Pets and Quote saved to database
        private async Task<IFriend> SavePets(FriendEditModel vm)
        {
            //Check if there are deleted Pets, if so simply remove them
            var deletedPets = vm.FriendInput.Pets.FindAll(p => (p.StatusIM == FriendEditModel.enStatusIM.Deleted));
            foreach (var item in deletedPets)
            {
                //Remove from the database
                await _service.DeletePetAsync(_usr, item.PetId);
            }

            //Check if there are any new pets added, if so create them in the database
            var newPets = vm.FriendInput.Pets.FindAll(p => (p.StatusIM == FriendEditModel.enStatusIM.Inserted));
            var pf = await _service.ReadFriendAsync(_usr, vm.FriendInput.FriendId, false);
            var dtopF = new csFriendCUdto(pf);
            foreach (var item in newPets)
            {
                //Create the corresposning model and CUdto objects
                var model = item.UpdateModel(new csPet());
                var cuDto = new csPetCUdto(model) { FriendId = vm.FriendInput.FriendId };

                //Set the relationships of a newly created item and write to database
                cuDto.FriendId = vm.FriendInput.FriendId;
                model = await _service.CreatePetAsync(_usr, cuDto);

                dtopF.PetsId.Add(model.PetId);

            }

            //To update modified and deleted pets, lets first read the original
            //Note that now the deleted pets will be removed and created pets will be nicely included
            var f = await _service.UpdateFriendAsync(_usr, dtopF);


            //Check if there are any modified pets , if so update them in the database
            var modifiedPets = vm.FriendInput.Pets.FindAll(p => (p.StatusIM == FriendEditModel.enStatusIM.Modified));
            foreach (var item in modifiedPets)
            {
                var model = f.Pets.First(a => a.PetId == item.PetId);

                //Update the model from the InputModel
                model = item.UpdateModel(model);

                //Updatet the model in the database
                model = await _service.UpdatePetAsync(_usr, new csPetCUdto(model) { FriendId = vm.FriendInput.FriendId });
            }

            f = await _service.ReadFriendAsync(_usr, vm.FriendInput.FriendId, false);

            return f;
        }

        private async Task<IFriend> SaveQuotes(FriendEditModel vm)
        {
            //Check if there are deleted Quotes, if so simply remove them
            var deletedQuotes = vm.FriendInput.Quotes.FindAll(a => (a.StatusIM == FriendEditModel.enStatusIM.Deleted));
            foreach (var item in deletedQuotes)
            {
                //Remove from the database
                await _service.DeleteQuoteAsync(_usr, item.QuoteId);
            }

            //Check if there are any new Quotes added, if so create them in the database
            var newQuotes = vm.FriendInput.Quotes.FindAll(q => (q.StatusIM == FriendEditModel.enStatusIM.Inserted));
            var qf = await _service.ReadFriendAsync(_usr, vm.FriendInput.FriendId, false);
            var dtoQF = new csFriendCUdto(qf);
            foreach (var item in newQuotes)
            {
                //Create the corresposning model and CUdto objects
                var model = item.UpdateModel(new csQuote());
                var cuDto = new csQuoteCUdto(model) { QuoteId = null };

                //Set the relationships of a newly created item and write to database
                cuDto.FriendId = vm.FriendInput.FriendId;

                //Create if does not exists. 
                model = await _service.CreateQuoteAsync(_usr, cuDto);

                dtoQF.QuotesId.Add(model.QuoteId);

            }

            //To update modified and deleted Quotes, lets first read the original
            //Note that now the deleted Quotes will be removed and created Quotes will be nicely included
            var q = await _service.UpdateFriendAsync(_usr, dtoQF);


            //Check if there are any modified Quotes , if so update them in the database
            var modifiedQuotes = vm.FriendInput.Quotes.FindAll(a => (a.StatusIM == FriendEditModel.enStatusIM.Modified));
            foreach (var item in modifiedQuotes)
            {
                var model = q.Quotes.First(a => a.QuoteId == item.QuoteId);

                //Update the model from the InputModel
                model = item.UpdateModel(model);

                //Updatet the model in the database
                model = await _service.UpdateQuoteAsync(_usr, new csQuoteCUdto(model) { FriendId = vm.FriendInput.FriendId });
            }

            q = await _service.ReadFriendAsync(_usr, vm.FriendInput.FriendId, false);
            return q;
        }
        #endregion


    }
}

