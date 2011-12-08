﻿$(document).ready(function () {

    // Wire up the Zone selection div as a popup dialog
    $('#divZoneSelect').dialog({
        title: 'Move Block To',
        autoOpen: false,
        width: 290,
        height: 300,
        modal: true
    })

    /*
    $('div.zone-instance').sortable({
    appendTo: 'body',
    connectWith: 'div.zone-instance',
    handle: 'a.block-move',
    opacity: 0.6,
    start: function (event, ui) {
    var start_pos = ui.item.index();
    ui.item.data('start_pos', start_pos);
    $('div.zone-instance').addClass('outline');
    },
    stop: function (event, ui) {
    $('div.zone-instance').removeClass('outline');
    }
    }).disableSelection();
    */

    // Bind the click event of the block move anchor tag
    $('a.blockinstance-move').click(function () {

        // Get a reference to the anchor tag for use in the dialog success function
        var $moveLink = $(this);

        // Add the current block's id as an attribute of the move dialog's save button
        $('#btnSaveZoneSelect').attr('blockInstance', $(this).attr('href'));

        // Set the dialog's zone selection select box value to the block's current zone 
        $('#ddlZones').val($(this).attr('zone'));

        // Set the dialog's parent option to the current zone's parent (either the page or the layout)
        if ($(this).attr('zoneloc') == 'Page') {
            $('#rblLocation_1').removeAttr('checked');
            $('#rblLocation_0').attr('checked', 'checked');
        }
        else {
            $('#rblLocation_0').removeAttr('checked');
            $('#rblLocation_1').attr('checked', 'checked');
        }

        // Show the popup block move dialog
        $('#divZoneSelect').dialog('open');

        // Bind the dialog save button's click event
        $('#btnSaveZoneSelect').click(function () {

            // Close the popup dialog box
            $('#divZoneSelect').dialog('close');

            // The current block's id
            var blockInstanceId = $(this).attr('blockinstance');

            // The new zone selected
            var zoneName = $('#ddlZones').val();

            // Get the current block instance object
            $.ajax({
                type: 'GET',
                contentType: 'application/json',
                dataType: 'json',
                url: rock.baseUrl + 'REST/Cms/BlockInstance/' + blockInstanceId,
                success: function (getData, status, xhr) {

                    // Update the new zone
                    getData.Zone = zoneName;

                    // Set the appropriate parent value (layout or page)
                    if ($('#rblLocation_0').attr('checked') == true) {
                        getData.Layout = null;
                        getData.PageId = rock.pageId;
                    }
                    else {
                        getData.Layout = rock.layout;
                        getData.PageId = null;
                    }

                    // Save the updated block instance
                    $.ajax({
                        type: 'PUT',
                        contentType: 'application/json',
                        dataType: 'json',
                        data: JSON.stringify(getData),
                        url: rock.baseUrl + 'REST/Cms/BlockInstance/Move/' + blockInstanceId,
                        success: function (data, status, xhr) {

                            // Get a reference to the block instance's container div
                            var $source = $('#bid_' + blockInstanceId);

                            // Get a reference to the new zone's container
                            var $target = $('#zone-' + $('#ddlZones').val());

                            // Update the move anchor with the new zone name
                            $moveLink.attr('zone', $('#ddlZones').val());

                            // If the block instance's parent is the page, move it to the new zone as the last
                            // block in that zone.  If the parent is the layout, insert it as the last layout
                            // block (prior to any page block's
                            if ($('#rblLocation_0').attr('checked') == true) {
                                $target.append($source);
                                $moveLink.attr('zoneloc', 'Page');
                                $source.attr('zoneLoc', 'Page');
                            }
                            else {
                                if ($('#' + $target.attr('id') + '>[zoneLoc="Layout"]').length > 0)
                                    $source.insertAfter($('#' + $target.attr('id') + '>[zoneLoc="Layout"]:last'));
                                else
                                    $target.append($source);
                                $moveLink.attr('zoneloc', 'Layout');
                                $source.attr('zoneLoc', 'Layout');
                            }

                        },
                        error: function (xhr, status, error) {
                            alert(status + ' [' + error + ']: ' + xhr.responseText);
                        }
                    });
                },
                error: function (xhr, status, error) {
                    alert(status + ' [' + error + ']: ' + xhr.responseText);
                }
            });

            // Unbind the dialog save button's click event
            $(this).unbind('click');

        });

        // Cancel the default action of the save button
        return false;
    });


    // Bind the block instance delete anchor
    $('a.blockinstance-delete').click(function () {

        if (confirm('Are you sure you want to delete this block?')) {

            var blockInstanceId = $(this).attr('href');

            // delete the block instance
            $.ajax({
                type: 'DELETE',
                contentType: 'application/json',
                dataType: 'json',
                url: rock.baseUrl + 'api/Cms/BlockInstance/' + blockInstanceId,
                success: function (data, status, xhr) {

                    // Remove the block instance's container div
                    $('#bid_' + blockInstanceId).remove();

                },
                error: function (xhr, status, error) {
                    alert(status + ' [' + error + ']: ' + xhr.responseText);
                }
            });

        }

        // Cancel the default action of the delete anchor tag
        return false;

    });

    // Bind the page's block config anchor to toggle the display
    // of each block's container and config options
    $('#cms-admin-footer .block-config').click(function () {
        $('.zone-configuration').hide();
        $('.zone-instance').removeClass('outline');
        $('.block-configuration').toggle();
        $('.block-instance').toggleClass('outline');
        return false;
    });

    // Bind the page's zone config anchor to toggle the display
    // of each zone's container and config options
    $('#cms-admin-footer .page-zones').click(function () {
        $('.block-configuration').hide();
        $('.block-instance').removeClass('outline');
        $('.zone-configuration').toggle();
        $('.zone-instance').toggleClass('outline');
        return false;
    });

});
