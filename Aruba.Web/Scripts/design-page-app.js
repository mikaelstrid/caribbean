angular.module("aruba", [])
    .service("printService", function ($http) {
        return {
            getPages: function (printId) {
                return $http.get("/api/prints/" + printId + "/pages");
            }
        }
    })
    .controller("printEditor", function ($scope, printService) {
        printService.getPages($scope.printId)
            .then(function (response) {
                console.log(response.data);
                $scope.pages = response.data;
            }, function (response) {
                alert("Call to /api/prints/{id}/pages failed.");
            });
    });




window.pixel.designpage = (function () {

    // ReSharper disable once InconsistentNaming
    //function ViewModel() {
    //    var self = this;

    //    // === INTERNAL VARIABLES ===
    //    self.currentTextField = null; // jQuery object
    //    self.currentHtmlField = null; // jQuery object
    //    self.currentImageField = null; // jQuery object

    //    // === OBSERVABLES ===
    //    self.toolboxEditTextFieldVisible = ko.observable(false);
    //    self.toolboxEditHtmlFieldVisible = ko.observable(false);
    //    self.toolboxEditImageFieldVisible = ko.observable(false);
    //    self.textFieldValue = ko.observable();
    //    self.htmlFieldValue = ko.observable();

    //    // === SUBSCRIBES === 
    //    self.textFieldValue.subscribe(function (newValue) {
    //        if (self.currentTextField) {
    //            var trimmed = _.string.startsWith(newValue, '<p>') ? newValue.slice(3) : newValue;
    //            trimmed = _.string.endsWith(newValue, '</p>') ? trimmed.slice(0, trimmed.length-4) : newValue;
    //            self.currentTextField.html(trimmed);
    //        }
    //    });
    //    self.htmlFieldValue.subscribe(function (newValue) {
    //        if (self.currentHtmlField) {
    //            var paragraphClass = self.currentHtmlField.data('firstparagraphclass');
    //            var withParagraphClass = newValue.replace(/<p.*?>/gi, '<p class="' + paragraphClass + '">');
    //            console.log(withParagraphClass);
    //            self.currentHtmlField.html(withParagraphClass);
    //        }
    //    });

    //    // === METHODS ===
    //    self.handleTextFieldClick = function () {
    //        self._changeVisibleToolbox('edit-text-field');
    //        self.currentTextField = $(this);
    //        self.currentTextField.addClass("active");
    //        self.textFieldValue($(this).html());
    //    } 
    //    self.handleTextFieldEditorBlur = function () {
    //        if (!self.currentTextField.html()) { self.textFieldValue("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"); }
    //        self._saveFieldValue(self.currentTextField, { html: self.currentTextField.html() });
    //    }

    //    self.handleHtmlFieldClick = function () {
    //        self._changeVisibleToolbox('edit-html-field');
    //        self.currentHtmlField = $(this);
    //        self.currentHtmlField.addClass("active");
    //        self.htmlFieldValue($(this).html());
    //    }
    //    self.handleHtmlFieldEditorBlur = function () {
    //        if (!self.currentHtmlField.html()) { self.htmlFieldValue("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"); }
    //        self._saveFieldValue(self.currentHtmlField, { html: self.currentHtmlField.html() });
    //    }

    //    self.handleImageFieldClick = function () {
    //        if (self.currentImageField) self.currentImageField.click(viewModel.handleImageFieldClick);
    //        self._changeVisibleToolbox('edit-image-field');
    //        self.currentImageField = $(this);
    //        self.currentImageField.parent().addClass('active');
    //        self.currentImageField.unbind('click');
    //        $('img', self.currentImageField).guillotine('enable');
    //    }
    //    self.handleAvailableObjectImageClick = function () {
    //        var selectedPictureUrl = $(this).data('picturesrc');
    //        var targetImage = $('img', self.currentImageField);
    //        targetImage.removeAttr('id');
    //        targetImage.removeAttr('width');
    //        targetImage.removeAttr('height');
    //        targetImage.guillotine('remove');
    //        targetImage.hide()
    //            .one('load', function() {
    //                $(this).guillotine({ width: self.currentImageField.width(), height: self.currentImageField.height() });
    //                $(this).fadeIn();
    //            })
    //            .attr('src', selectedPictureUrl)
    //            .each(function() {
    //                if(this.complete) $(this).trigger('load');
    //            });
    //    }

    //    self.increaseImageSize = function () {
    //        if (!self._isCurrentImageFieldInitialized()) return;
    //        $('img', self.currentImageField).guillotine('zoomIn');
    //    }
    //    self.decreaseImageSize = function () {
    //        if (!self._isCurrentImageFieldInitialized()) return;
    //        $('img', self.currentImageField).guillotine('zoomOut');
    //    }

    //    self.handleDoneSubmitButtonClick = function () {
    //        self._saveImageFieldValue();
    //        $('#done-form').submit();
    //    }

    //    // === PRIVATE METHODS ===
    //    self._changeVisibleToolbox = function (toolbox) {
    //        self._saveImageFieldValue();
    //        self._disableAllFields();
    //        self.toolboxEditTextFieldVisible(toolbox === 'edit-text-field');
    //        self.toolboxEditHtmlFieldVisible(toolbox === 'edit-html-field');
    //        self.toolboxEditImageFieldVisible(toolbox === 'edit-image-field');
    //    }

    //    self._disableAllFields = function() {
    //        if (self.currentTextField) {
    //            self.currentTextField.removeClass("active");
    //            self.currentTextField = null;
    //        }
    //        if (self.currentHtmlField) {
    //            self.currentHtmlField.removeClass("active");
    //            self.currentHtmlField = null;
    //        }
    //        if (self.currentImageField) {
    //            self.currentImageField.parent().removeClass("active");
    //            $('img', self.currentImageField).guillotine('disable');
    //            self.currentImageField = null;
    //        }
    //    }

    //    self._saveImageFieldValue = function () {
    //        if (!self._isCurrentImageFieldInitialized()) return;
    //        var targetImage = $('img', self.currentImageField);
    //        var guillotineData = targetImage.guillotine('getData'); // { scale: 1.4, angle: 270, x: 10, y: 20, w: 400, h: 300 }
    //        var data = $.extend({}, { url: targetImage.attr('src') }, guillotineData); 
    //        self._saveFieldValue(self.currentImageField, data);
    //    }

    //    self._isCurrentImageFieldInitialized = function() {
    //        return  self.currentImageField && !($('img', self.currentImageField).attr('id')); // Image selected and not a dummy image
    //    }

    //    self._saveFieldValue = function (field, dataValue) {
    //        var pageId = field.closest('html').data('pageid');

    //        var fieldName = field.data('afname');
    //        if (fieldName) {
    //            $.post('/api/fieldvalues', { pageId: pageId, fieldName: fieldName, fieldValue: JSON.stringify(dataValue) });
    //            return;
    //        }

    //        var fieldValueId = field.data('afvid');
    //        if (fieldValueId) {
    //            $.ajax(
    //            {
    //                url: '/api/fieldvalues',
    //                type: "PUT",
    //                data: { fieldValueId: fieldValueId, fieldValue: JSON.stringify(dataValue) },
    //            });
    //        }
    //    }
    //}

    // === PRIVATE FUNCTIONS ===
    function resizeIframe(iframe, previewWidth, iframeWrapperWidth) { // iframe is a jQuery element
        var scaleValue = iframeWrapperWidth / previewWidth;
        iframe.css('transform-origin', '0 0');
        iframe.css('transform', 'scale(' + scaleValue + ')');
    }

    // === PUBLIC FUNCTIONS ===
    var initPage = function (page, actualTemplateWidth) { // page is a jQuery element
        var iframe = $('iframe', page);
        iframe.load(function () {
            //$(".editable-textfield", iframe.contents()).click(viewModel.handleTextFieldClick);
            $(".editable-textfield", iframe.contents()).click(function () { console.log("textfield clicked;") });

            //$(".editable-htmlfield", iframe.contents()).click(viewModel.handleHtmlFieldClick);
            $(".editable-htmlfield", iframe.contents()).click(function () { console.log("htmlfield clicked;") });

            var editableImageFields = $(".editable-imagefield", iframe.contents());
            editableImageFields.each(function () {
                var parent = $(this).parent();
                parent.addClass('editable-imagefield-parent');
                parent.css('z-index', parseInt($(this).css('z-index')) + 1);
            });
            //editableImageFields.click(viewModel.handleImageFieldClick);
            editableImageFields.click(function () { console.log("imagefield clicked;") });
            $(".editable-imagefield[data-imagefieldtype='1']", iframe.contents()).each(function () {
                if ($(this).data('afvid')) {
                    var initData = $(this).data('init');
                    $("img", $(this)).guillotine({ width: $(this).width(), height: $(this).height(), init: initData });
                    $("img", $(this)).guillotine('disable');
                }
            });
            $(".editable-imagefield[data-imagefieldtype='2']", iframe.contents()).each(function () {
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
                            $("img", $(this)).guillotine('disable');
                            $(this).show();
                        })
                        .attr('src', selectedPictureUrl)
                        .each(function () {
                            if (this.complete) $(this).trigger('load');
                        });
                }
            });
        });
        $(window).resize(function () {
            resizeIframe(iframe, actualTemplateWidth, page.width());
        });
        resizeIframe(iframe, actualTemplateWidth, page.width());
    }
    //var initTextFieldEditor = function (editor) {
    //    //editor.on('blur', function () { viewModel.handleTextFieldEditorBlur(); });
    //    //editor.onInit.add(function (ed, evt) {
    //    //    tinymce.dom.Event.add(ed.getBody(), 'blur', function (e, t) {
    //    //        alert('Worky Work Work!');
    //    //    });
    //    //});
    //    editor.on('init', function (ed) {
    //        tinymce.dom.Event.bind(ed.target.getBody(), 'blur', function (e, t) {
    //            alert('Worky Work Work!');
    //        });
    //    });
    //}
    //var initHtmlFieldEditor = function (editor) {
    //    editor.on('blur', function () { viewModel.handleHtmlFieldEditorBlur(); });
    //}
    //var initImageFieldEditor = function () {
    //    $('.available-object-images img').click(viewModel.handleAvailableObjectImageClick);
    //}
    //var getViewModel = function () { return viewModel; }


    // === PROCEDURAL CODE ===
    //var viewModel = new ViewModel();
    return {
        initPage: initPage
        //initTextFieldEditor: initTextFieldEditor,
        //initHtmlFieldEditor: initHtmlFieldEditor,
        //initImageFieldEditor: initImageFieldEditor,
        //getViewModel: getViewModel
    };
}());
