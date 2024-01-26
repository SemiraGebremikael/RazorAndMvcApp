//Just to ensure we force js into strict mode in HTML scrips - we don't want any sloppy code
'use strict';  

//Add handlers to all tags that define the data property data-seido-hide-on-collaps
elems = document.querySelectorAll('*[data-seido-hide-on-collaps]');
elems.forEach(elem => {

  //Add show event handler to all collapsable
  elem.addEventListener('show.bs.collapse', event => {

      var collapsable = event.currentTarget;
      //find all input elemenent within the collapsable
      let inputs = collapsable.querySelectorAll('input[data-seido-hide-on-collaps]');
      inputs.forEach(i => {
        i.style.display = i.dataset.seidoHideOnCollaps;
      });

    }); 

  elem.addEventListener('hide.bs.collapse', event => {

      var collapsable = event.currentTarget;
      //find all input elemenent within the collapsable
      let inputs = collapsable.querySelectorAll('input[data-seido-hide-on-collaps]');
      inputs.forEach(i => {
        i.dataset.seidoHideOnCollaps = i.style.display;
        i.style.display = "none";
      });

    });
});

//Make sure all input tags that define the data property data-seido-hide-on-collaps are initially hidden
let celems = document.querySelectorAll('*[data-seido-hide-on-collaps] input');
celems.forEach(i => { 

  i.dataset.seidoHideOnCollaps = i.style.display;
  i.style.display = "none";
});

