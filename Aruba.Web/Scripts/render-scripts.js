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

        $(this).parent().css("position", "absolute");
        $(this).parent().css("left", $(this).css("left"));
        $(this).parent().css("right", $(this).css("right"));
        $(this).parent().css("top", $(this).css("top"));
        $(this).parent().css("bottom", $(this).css("bottom"));
        $(this).parent().css("z-index", $(this).css("z-index"));
        $(this).parent().css("margin-top", $(this).css("margin-top"));
        $(this).parent().css("margin-bottom", $(this).css("margin-bottom"));
        $(this).parent().css("margin-left", $(this).css("margin-left"));
        $(this).parent().css("margin-right", $(this).css("margin-right"));

        $(this).css("position", "relative");
        $(this).css("left", "0");
        $(this).css("right", "0");
        $(this).css("top", "0");
        $(this).css("bottom", "0");
        $(this).css("margin", "0");

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
});