﻿function closeModal() {
    $('#modal-popup').modal('hide');
}

$(document).ready(function () {

    // Wire up the iframe div as a popup dialog
    $('#modal-popup').modal({
        backdrop: true,
        keyboard: true
    });

    // Bind the click event for the close modal button
    $('#modal-cancel').click(function () {
        $('#modal-popup').modal('hide');
    });

    // Bind the click event for all of the links that use the iframe to show the 
    // modal div/iframe
    $('a.show-modal-iframe').click(function () {

        // Use the anchor tag's href attribute as the source for the iframe
        $('#modal-popup-iframe').attr('src', $(this).attr('href'));

        $('#modal-popup').bind('hidden', function (event, ui) {
            //            if ($(this).attr('instance-id') != undefined)
            //                $('#blck-cnfg-trggr-' + $(this).attr('instance-id')).click();
            $('#modal-popup-iframe').attr('src', '');
            $('#modal-popup-iframe').empty();
            $('#modal-popup').unbind('hidden');
        });

        // If the anchor tag specifies a modal height, set the dialog's height
        if ($(this).attr('height') != undefined)
            $('#modal-popup div.modal-body').css('height', $(this).attr('height'));
        else
            $('#modal-popup div.modal-body').css('height', '');

        // Use the anchor tag's title attribute as the title of the dialog box
        if ($(this).attr('title') != undefined)
            $('#modal-popup h3').html($(this).attr('title'));

        // popup the dialog box
        $('#modal-popup').modal('show');

        // Cancel the default behavior of the anchor tag
        return false;

    });

});
