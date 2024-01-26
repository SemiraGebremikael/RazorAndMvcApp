using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppMusicRazor.SeidoHelpers
{
    #region Seido Helpers for Model Validation
    public record reModelValidationResult
    (
        bool HasErrors,
        IEnumerable<string> ErrorMsgs,
        IEnumerable<KeyValuePair<string, ModelStateEntry>> InvalidKeys
    );

    public static class csSeidoExtensionMVC
    {
        //Model state Validations
        public static bool IsValidPartially(this ModelStateDictionary model, out reModelValidationResult validationResult, string[] validateOnlyKeys = null)
        {
            var _invalidKeys = model
               .Where(s => s.Value.ValidationState == ModelValidationState.Invalid);

            if (validateOnlyKeys != null)
            {
                _invalidKeys = _invalidKeys.Where(s => validateOnlyKeys.Any(vk => vk == s.Key));
            }

            var _errorMsgs = _invalidKeys.SelectMany(e => e.Value.Errors).Select(e => e.ErrorMessage);
            var _hasErrors = _invalidKeys.Count() != 0;

            validationResult = new reModelValidationResult(_hasErrors, _errorMsgs, _invalidKeys);

            return !_hasErrors;
        }


        //Populate SelectLists with Enum Values and Text
        public static List<SelectListItem> PopulateSelectList<TEnum>(this List<SelectListItem> selectList) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("Not an enum type");
            }

            //Populate populate select tag with options using tag helpers
            foreach (var item in typeof(TEnum).GetEnumValues())
            {
                selectList.Add(new SelectListItem
                {
                    Value = item.ToString(),
                    Text = item.ToString()
                });
            }

            return selectList;
        }
    }
    #endregion
}

