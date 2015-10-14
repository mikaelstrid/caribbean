$(document).ready(function () {
    var editableImageFields = $(".editable-imagefield");
    editableImageFields.each(function () {
        var parent = $(this).parent();
        parent.addClass("editable-imagefield-parent");
        parent.css("z-index", parseInt($(this).css("z-index")) + 1);
    });
    $(".editable-imagefield[data-imagefieldtype='1']").each(function () {
        if ($(this).data('afvid')) {
            var initData = $(this).data('init');
            var width = $(this).width();
            var height = $(this).height();
            var targetImage = $("img", $(this));
            targetImage.one('load', function () {
                targetImage.guillotine({ width: width, height: height, init: initData });
                targetImage.guillotine('disable');
            }).each(function () {
                if (this.complete) $(this).trigger('load');
            });
        }
    });
    $(".editable-imagefield[data-imagefieldtype='2']").each(function () {
        $(this).parent().css('width', $(this).width());
        $(this).parent().css('height', $(this).height());
        $(this).css('width', $(this).width());
        $(this).css('height', $(this).height());

        if ($(this).data('afvid')) {
            var selectedPictureUrl = $(this).data('imgurl');
            var targetImage = $('img', $(this));
            targetImage.removeAttr('id');
            targetImage.removeAttr('width');
            targetImage.removeAttr('height');
            targetImage.hide()
                .one('load', function () {
                    var imageField = $(this).parent();
                    var initData = imageField.data('init');
                    $(this).guillotine({ width: imageField.width(), height: imageField.height(), init: initData });
                    $(this).guillotine('disable');
                    $(this).show();
                })
                .attr('src', selectedPictureUrl)
                .each(function () {
                    if (this.complete) $(this).trigger('load');
                });
        }
    });

    var parseSavedHideableFieldValue = function(savedValue) {
        if (!savedValue || !savedValue.value) return true; // Default to true if no value saved
        try {
            return JSON.parse(savedValue.value).visible;
        }
        catch (err) {
            console.log("Could not parse saved hideable field value.", savedValue);
            return true;
        }
    }
    $("img[src*='doljbar-beskrivning']").each(function () {
        var fieldDescriptorParts = $(this).attr("alt").split("|");
        if (fieldDescriptorParts.length < 3) return true;
        var name = fieldDescriptorParts[0];
        var savedValue = _.find(fieldValues, function (fv) { return fv.name === name });
        var savedVisibility = parseSavedHideableFieldValue(savedValue);
        var grandParent = $(this).parent().parent();
        grandParent.toggle(savedVisibility);
    });

    $(".crop_area").css("visibility", "hidden");
});