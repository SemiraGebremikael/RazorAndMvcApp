//Just to ensure we force js into strict mode in HTML scrips - we don't want any sloppy code
'use strict';  

//the clickHandler copies the value from the data property data-seido-selected-item-id into data-seido-selected-item-id-target
function clickHandler (event)  {

    //Extract the selected item from data-* attributes
    var btn = event.currentTarget;
    var selectedItem = btn.dataset.seidoSelectedItemId;
    
    //Find all input elements that shall have the the selected item as value 
    let elems = document.querySelectorAll('input[data-seido-selected-item-id-target]');
    elems.forEach(elem => {
      elem.value = selectedItem;
    });

    //continues the post
    return true;
}

//Add clickHandler to all tags that define the data property data-seido-selected-item-id
let selems = document.querySelectorAll('*[data-seido-selected-item-id]');
selems.forEach(elem => {
  elem.addEventListener('click', clickHandler);
});

