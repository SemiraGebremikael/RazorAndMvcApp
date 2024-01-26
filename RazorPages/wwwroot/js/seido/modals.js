//Just to ensure we force js into strict mode in HTML scrips - we don't want any sloppy code
'use strict';  


//Script to set title and body of modals
function launchModal (event)  {

    //Extract info from data-* attributes
    var btn = event.relatedTarget;
    var mod_title = btn.dataset.seidoModalTitle;
    var mod_body = btn.dataset.seidoModalBody;
    var mod_cancel = btn.dataset.seidoModalCancel;
    var mod_ok = btn.dataset.seidoModalOk;

    //Set the attributes into the modal
    var modal = event.currentTarget;
    var title = modal.querySelector('.modal-title');
    var body = modal.querySelector('.modal-body');
    var btn_cancel = modal.querySelector('.btn-secondary');
    var btn_ok = modal.querySelector('.btn-primary');

    title.textContent = mod_title ?? title.textContent;
    body.textContent = mod_body ?? body.textContent;
    btn_cancel.textContent = mod_cancel ?? btn_cancel.textContent;
    btn_ok.textContent = mod_ok ?? btn_ok.textContent;    

    //if modal should post back before closing
    //mod_post_url = url to send post message to
    //mod_post_data = parameter to post
    var mod_post_data = { postdata: btn.dataset.seidoModalPostData };
    var mod_post_url = btn.dataset.seidoModalPostUrl;

    btn_ok.addEventListener('click', async event => {

      if (mod_post_url)
      {
        try {
          //send the data using post and await the reply
          const response = await fetch(mod_post_url, {
              method: 'post',
              headers: { "Content-Type": "application/json" },
              body: mod_post_data ? JSON.stringify(mod_post_data) : null
          });
          const result = await response.text();
    
          if (!response.ok) {
            throw new Error(`Transmission error ${response.status} posting to ${mod_post_url}`);
          }
        }
        catch (e) {
           alert(e);
        }
      }

      var modinst = bootstrap.Modal.getInstance(modal);
      modinst.hide();
    });

    /*
    //For testing
    window.alert(`The ${btn.nodeName} element fired the modal!`+
    `\nTitle passed to modal: ${mod_title}`+
    `\nBody passed to modal: ${mod_body}`);
    */
  };


  //Add launchHandler to all you modals
  /*
  document.getElementById('softModal').addEventListener('show.bs.modal', launchModal);
  document.getElementById('hardModal').addEventListener('show.bs.modal', launchModal);
  document.getElementById('dangerModal').addEventListener('show.bs.modal', launchModal);
*/

let melems = document.querySelectorAll('.modal.fade');
melems.forEach(elem => {
  elem.addEventListener('show.bs.modal', launchModal);
});
