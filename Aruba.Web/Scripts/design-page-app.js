angular.module("aruba", ["ngCkeditor"])
    .service("printService", function ($http) {
        return {
            getPages: function (printId) {
                return $http.get("/api/prints/" + printId + "/pages");
            }
        }
    })
    .service("pageService", function ($http) {
        return {
            getPage: function (pageId) {
                return $http.get("/api/page/" + pageId);
            }
        }
    })
    .service("realEstateObjectService", function ($http) {
        return {
            getImages: function (realEstateObjectId) {
                return $http.get("/api/realestateobjects/" + realEstateObjectId + "/images");
            }
        }
    })
    .service("fieldValuesService", function ($http) {
        return {
            addFieldValue: function (pageId, fieldName, fieldValue) {
                return $http.post("/api/fieldvalues/", { pageId: pageId, fieldName: fieldName, fieldValue: fieldValue });
            },
            updateFieldValue: function (fieldValueId, fieldValue) {
                return $http.put("/api/fieldvalues", { fieldValueId: fieldValueId, fieldValue: fieldValue });
            }
        }
    })
    .controller("printEditorCtrl", function ($scope, $q, $timeout, printService, pageService, fieldValuesService, realEstateObjectService) {
        // Local variables
        var currentSelectedTextField = null;
        var currentSelectedHtmlField = null;
        var currentSelectedObjectImageField = null;
        var currentSelectedStaffImageField = null;
        var surroundingAreaHeight = -1;

        // Initialize the CKEditors
        $scope.textEditor = {
            language: "sv",
            uiColor: "#ffffff",
            height: "6rem",
            toolbarGroups: [
		        { name: "clipboard", groups: ["clipboard", "undo"] },
		        { name: "editing", groups: ["find", "selection", "spellchecker", "editing"] },
		        { name: "links", groups: ["links"] },
		        { name: "insert", groups: ["insert"] },
		        { name: "forms", groups: ["forms"] },
		        { name: "tools", groups: ["tools"] },
		        { name: "document", groups: ["mode", "document", "doctools"] },
		        { name: "others", groups: ["others"] },
		        "/",
		        { name: "basicstyles", groups: ["basicstyles", "cleanup"] },
		        { name: "paragraph", groups: ["list", "indent", "blocks", "align", "bidi", "paragraph"] },
		        { name: "styles", groups: ["styles"] },
		        { name: "colors", groups: ["colors"] },
		        { name: "about", groups: ["about"] }
            ],
            removeButtons: "Subscript,Strike,Anchor,Image,Blockquote,Styles,Format,About,Cut,Copy,Paste,PasteText,Redo,Undo,Link,Scayt,Source,Maximize,RemoveFormat,NumberedList,Indent,Outdent,Table,HorizontalRule,SpecialChar,PasteFromWord,Unlink,BulletedList,Superscript,Underline"
        };
        $scope.htmlEditor = {
            language: "sv",
            uiColor: "#ffffff",
            toolbarGroups: [
		        { name: "clipboard", groups: ["clipboard", "undo"] },
		        { name: "editing", groups: ["find", "selection", "spellchecker", "editing"] },
		        { name: "links", groups: ["links"] },
		        { name: "insert", groups: ["insert"] },
		        { name: "forms", groups: ["forms"] },
		        { name: "tools", groups: ["tools"] },
		        { name: "document", groups: ["mode", "document", "doctools"] },
		        { name: "others", groups: ["others"] },
		        "/",
		        { name: "basicstyles", groups: ["basicstyles", "cleanup"] },
		        { name: "paragraph", groups: ["list", "indent", "blocks", "align", "bidi", "paragraph"] },
		        { name: "styles", groups: ["styles"] },
		        { name: "colors", groups: ["colors"] },
		        { name: "about", groups: ["about"] }
            ],
            removeButtons: "Underline,Subscript,Cut,Copy,Paste,PasteText,PasteFromWord,Undo,Redo,Scayt,Link,Anchor,Unlink,Image,Table,HorizontalRule,SpecialChar,Maximize,Source,Strike,Superscript,RemoveFormat,Outdent,Indent,Blockquote,Styles,About,Format,BulletedList,NumberedList"
        };

        // Scope variables
        $scope.toolboxVisible = true;
        $scope.textEditorToolboxVisible = false;
        $scope.textEditorFormReady = false;
        $scope.htmlEditorToolboxVisible = false;
        $scope.htmlEditorFormReady = false;
        $scope.objectImageEditorToolboxVisible = false;
        $scope.staffImageEditorToolboxVisible = false;
        $scope.hideableFields = [];

        // Scope functions
        $scope.switchToPage = function (pageId) {
            $(".off-canvas-wrap").foundation("offcanvas", "hide", "move-right");
            pageService.getPage(pageId)
                .then(function (response) {
                    $scope.currentPage = response.data;
                    var p = response.data;
                    $("#dummy").css("margin-top", p.aspectRatioInPercent + "%");
                    $("#page-editor").empty().append($("<iframe id='pageEditorIframe' class='hide' frameborder='0' scrolling='no' style='width: " + p.width + "px; height: " + p.height + "px;'></iframe>"));
                    $("#pageEditorIframe").attr("src", p.previewUrl);
                    $("#pageEditorIframe").load($scope.initIframe);
                    $scope.resizeIframe();
                }, function (response) {
                    alert("Call to pageService.getPage failed.");
                });
        }

        $scope.toggleToolbox = function() {
            $scope.toolboxVisible = !$scope.toolboxVisible;
            $timeout(function () { $scope.resizeIframe(); });
        }
        $scope.showToolbox = function () {
            $scope.toolboxVisible = true;
            $timeout(function () { $scope.resizeIframe(); });
        }


        // Text editor functions
        $scope.textEditorValue = null;
        $scope.textEditorPristineValue = null;      
        $scope.handleTextFieldClick = function () {
            if (currentSelectedTextField && $(this)[0] === currentSelectedTextField[0]) return;
            $scope.textEditorFormReady = false;
            $scope._changeVisibleToolbox("textEditor");
            currentSelectedTextField = $(this);
            currentSelectedTextField.addClass("active");
            $("#toolboxTab1").click();
            $scope.textEditorPristineValue = $(this).html();
            $scope.textEditorValue = $(this).html();
            $scope.$apply();
            $scope.delayedSetPristine($scope.textEditorForm, 100, function () { $scope.textEditorFormReady = true; });
            $scope.showToolbox();
        }
        $scope.saveTextEditorValue = function () {
            var trimmed = $scope._trimTextEditorValue($scope.textEditorValue);
            $scope._saveFieldValue(currentSelectedTextField, { html: trimmed })
                .then(function () {
                    console.log("Field value updated successfully.");
                    $scope.textEditorPristineValue = currentSelectedTextField.html();
                    $scope.textEditorForm.$setPristine();
                }, function () {
                    alert("Field value update failed.");
                });
        }
        $scope.revertTextEditorValue = function () {
            currentSelectedTextField.html($scope.textEditorPristineValue);
            $scope.textEditorValue = $scope.textEditorPristineValue;
            $scope.delayedSetPristine($scope.textEditorForm, 100);
        }
        $scope.$watch("textEditorValue", function (newValue) {
            if (!currentSelectedTextField) return;
            currentSelectedTextField.html($scope._trimTextEditorValue(newValue));
        });
        $scope._trimTextEditorValue = function (value) {
            // Remove newlines and <p> tags
            var nonNullValue = value ? value : "<p></p>";
            var trimmed = nonNullValue.replace(/(\r\n|\n|\r)/gm, "");
            trimmed = _.startsWith(trimmed, "<p>") ? trimmed.slice(3) : trimmed;
            trimmed = _.endsWith(trimmed, "</p>") ? trimmed.slice(0, trimmed.length - 4) : trimmed;
            return trimmed;
        }


        // HTML editor functions
        $scope.htmlEditorValue = null;
        $scope.htmlEditorPristineValue = null;
        $scope.handleHtmlFieldClick = function () {
            if (currentSelectedHtmlField && $(this)[0] === currentSelectedHtmlField[0]) return;
            $scope.htmlEditorFormReady = false;
            $scope._changeVisibleToolbox("htmlEditor");
            currentSelectedHtmlField = $(this);
            currentSelectedHtmlField.addClass("active");
            $("#toolboxTab1").click();
            $scope.htmlEditorPristineValue = $(this).html();
            $scope.htmlEditorValue = $(this).html();
            $scope.$apply();
            $scope.delayedSetPristine($scope.htmlEditorForm, 100, function () { $scope.htmlEditorFormReady = true; });
            $scope.showToolbox();
        }
        $scope.saveHtmlEditorValue = function () {
            var withParagraphClass = $scope._addParagraphClass($scope.htmlEditorValue);
            $scope._saveFieldValue(currentSelectedHtmlField, { html: withParagraphClass })
                .then(function () {
                    console.log("Field value updated successfully.");
                    $scope.htmlEditorPristineValue = currentSelectedHtmlField.html();
                    $scope.htmlEditorForm.$setPristine();
                }, function () {
                    alert("Field value update failed.");
                });

            // Update the print page preview
            currentSelectedHtmlField.html(withParagraphClass);
        }
        $scope.revertHtmlEditorValue = function () {
            currentSelectedHtmlField.html($scope.htmlEditorPristineValue);
            $scope.htmlEditorValue = $scope.htmlEditorPristineValue;
            $scope.delayedSetPristine($scope.htmlEditorForm, 100);
        }
        $scope.$watch("htmlEditorValue", function (newValue) {
            if (!currentSelectedHtmlField) return;
            currentSelectedHtmlField.html($scope._addParagraphClass(newValue));
        });
        $scope._addParagraphClass = function(value) {
            var paragraphClass = currentSelectedHtmlField.data("firstparagraphclass");
            var nonNullValue = value ? value : "<p></p>";
            return nonNullValue.replace(/<p.*?>/gi, "<p class=\"" + paragraphClass + "\">");
        }


        // Image editor functions
        $scope.handleImageFieldClick = function (currentImageField) {
            $scope.$apply();
            currentImageField.parent().addClass("active");
            currentImageField.unbind("click");
            $("img", currentImageField).guillotine("enable");
            $("#toolboxTab1").click();
            $scope.showToolbox();
        }
        $scope.handleAvailableImageClick = function (currentImageField, imageUrl) {
            var targetImage = $("img", currentImageField);
            targetImage.removeAttr("id");
            targetImage.removeAttr("width");
            targetImage.removeAttr("height");
            targetImage.guillotine("remove");
            targetImage.hide()
                .one("load", function () {
                    $(this).guillotine({
                        width: currentImageField.width(),
                        height: currentImageField.height(),
                        init: { "scale": 0.1, "angle": 0, "x": 0, "y": 0 },
                        onChange: $scope.onImageEditorChange
                    });
                    $(this).fadeIn();
                    $scope._saveImageEditorValue();
                })
                .attr("src", imageUrl)
                .each(function () {
                    if (this.complete) $(this).trigger("load");
                });
        }
        $scope.increaseImageSize = function () {
            var currentImageField = $scope._getCurrentImageField();
            if (!currentImageField) return;
            $("img", currentImageField).guillotine("zoomIn");
        }
        $scope.decreaseImageSize = function () {
            var currentImageField = $scope._getCurrentImageField();
            if (!currentImageField) return;
            $("img", currentImageField).guillotine("zoomOut");
        }
        $scope.onImageEditorChange = function () {
            $scope._saveImageEditorValue();
        }

        $scope.handleObjectImageFieldClick = function () {
            $scope._changeVisibleToolbox("objectImageEditor");
            currentSelectedObjectImageField = $(this);
            $scope.handleImageFieldClick(currentSelectedObjectImageField);
        }
        $scope.handleAvailableObjectImageClick = function (imageUrl) {
            $scope.handleAvailableImageClick(currentSelectedObjectImageField, imageUrl);
        }
        
        $scope.handleStaffImageFieldClick = function () {
            $scope._changeVisibleToolbox("staffImageEditor");
            currentSelectedStaffImageField = $(this);
            $scope.handleImageFieldClick(currentSelectedStaffImageField);
        }
        $scope.handleAvailableStaffImageClick = function (imageUrl) {
            $scope.handleAvailableImageClick(currentSelectedStaffImageField, imageUrl);
        }


        // Hideable fields functions
        $scope.updateHideableFieldsList = function (iframe) {
            $scope.hideableFields = [];
            $("img[src*='doljbar-beskrivning']", iframe.contents()).each(function () {
                var fieldDescriptorParts = $(this).attr("alt").split("|");
                if (fieldDescriptorParts.length < 3) return true;
                var name = fieldDescriptorParts[0];
                var savedValue = _.find($scope.currentPage.fieldValues, function (fv) { return fv.name === name });
                var savedVisibility = $scope._parseSavedHideableFieldValue(savedValue);
                var grandParent = $(this).parent().parent();
                $scope.hideableFields.push({
                    name: name,
                    title: fieldDescriptorParts[1],
                    description: fieldDescriptorParts[2],
                    visible: savedVisibility,
                    domId: grandParent.attr("id")
                });
                grandParent.toggle(savedVisibility);
            });
            $scope.$apply();
        }
        $scope._parseSavedHideableFieldValue = function(savedValue) {
            if (!savedValue || !savedValue.value) return true; // Default to true if no value saved
            try {
                return JSON.parse(savedValue.value).visible;
            }
            catch (err) {
                console.log("Could not parse saved hideable field value.", savedValue);
                return true;
            }
        }
        $scope.toggleHideableField = function (field) {
            field.visible = !field.visible;
            $("#" + field.domId, $("#pageEditorIframe").contents()).toggle(field.visible);
            $scope._saveHideableFieldValue($scope.currentPage.id, field.name, field.visible)
                .then(function () {
                    console.log("Hideable field value updated successfully.");
                }, function () {
                    alert("Field value update failed.");
                });
        }
        $scope._saveHideableFieldValue = function (pageId, fieldName, visible) {
            return fieldValuesService.addFieldValue(pageId, fieldName, JSON.stringify({ visible: visible }));
        }


        // Initialization
        printService.getPages($scope.printId)
            .then(function (response) {
                $scope.pages = response.data;
                if (response.data.length > 0)
                    $scope.switchToPage(response.data[0].id);
            }, function (response) {
                alert("Call printService.getPages failed.");
            });

        realEstateObjectService.getImages($scope.realEstateObjectId)
            .then(function (response) {
                $scope.objectImages = response.data.objectImages;
                $scope.staffImages = response.data.staffImages;
            }, function (response) {
                alert("Call realEstateObjectService.getImages failed.");
            });

        $(window).resize(function () {
            if ($scope.currentPage)
                $scope.resizeIframe();
        });





        // HELPER FUNCTIONS
        // This function is needed to guarantee that the form that the CKEditor is in
        // is pristine after we have changed the value.
        // The problem is that the Angular apply/digest loop is triggered after the 
        // setPristine call and sets the form to dirty
        $scope.delayedSetPristine = function (form, delayInMs, setFormReadyFunction) {
            setTimeout(function () {
                form.$setPristine();
                if (setFormReadyFunction) setFormReadyFunction();
                $scope.$apply();
            }, delayInMs);
        }



        $scope._saveImageEditorValue = function () {
            var currentImageField = $scope._getCurrentImageField();
            if (!currentImageField) return;

            var targetImage = $("img", currentImageField);
            var guillotineData = targetImage.guillotine("getData"); // { scale: 1.4, angle: 270, x: 10, y: 20, w: 400, h: 300 }
            var data = $.extend({}, { url: targetImage.attr("src") }, guillotineData);
            $scope._saveFieldValue(currentImageField, data)
                .then(function () {
                    console.log("Field value updated successfully.");
                }, function () {
                    alert("Field value update failed.");
                });
        }

        $scope._saveFieldValue = function (field, dataValue) {
            var pageId = field.closest("html").data("pageid");

            var fieldName = field.data("afname");
            if (fieldName)
                return fieldValuesService.addFieldValue(pageId, fieldName, JSON.stringify(dataValue));

            var fieldValueId = field.data("afvid");
            if (fieldValueId)
                return fieldValuesService.updateFieldValue(fieldValueId, JSON.stringify(dataValue));

            return $q.reject("Illegal field type (not afname or afvid).");
        }

        $scope._changeVisibleToolbox = function (toolbox) {
            $scope._disableAllFields();
            $scope.textEditorToolboxVisible = (toolbox === "textEditor");
            $scope.htmlEditorToolboxVisible = (toolbox === "htmlEditor");
            $scope.objectImageEditorToolboxVisible = (toolbox === "objectImageEditor");
            $scope.staffImageEditorToolboxVisible = (toolbox === "staffImageEditor");
        }

        $scope.initIframe = function () {
            var iframe = $("#pageEditorIframe");

            $(".editable-textfield", iframe.contents()).click($scope.handleTextFieldClick);
            $(".editable-htmlfield", iframe.contents()).click($scope.handleHtmlFieldClick);

            var editableImageFields = $(".editable-imagefield", iframe.contents());
            editableImageFields.each(function () {
                var parent = $(this).parent();
                parent.addClass("editable-imagefield-parent");
                parent.css("z-index", parseInt($(this).css("z-index")) + 1);
            });
            $(".editable-imagefield[data-imagefieldtype='1']", iframe.contents()).each(function () {
                if ($(this).data("afvid")) {
                    var initData = $(this).data("init");
                    $("img", $(this)).guillotine({
                        width: $(this).width(),
                        height: $(this).height(),
                        init: initData,
                        onChange: $scope.onImageEditorChange
                    });
                    $("img", $(this)).guillotine("disable");
                }
            });
            $(".editable-imagefield[data-imagefieldtype='2']", iframe.contents()).each(function () {
                $(this).parent().css("width", $(this).width());
                $(this).parent().css("height", $(this).height());
                $(this).css("width", $(this).width());
                $(this).css("height", $(this).height());
                
                if ($(this).data("afvid")) {
                    var selectedPictureUrl = $(this).data("imgurl");
                    var targetImage = $("img", $(this));
                    targetImage.removeAttr("id");
                    targetImage.removeAttr("width");
                    targetImage.removeAttr("height");
                    targetImage.hide()
                        .one("load", function () {
                            var imageField = $(this).parent();
                            var initData = imageField.data("init");
                            $(this).guillotine({
                                width: imageField.width(),
                                height: imageField.height(),
                                init: initData,
                                onChange: $scope.onImageEditorChange
                            });
                            $("img", $(this)).guillotine("disable");
                            $(this).show();
                        })
                        .attr("src", selectedPictureUrl)
                        .each(function () {
                            if (this.complete) $(this).trigger("load");
                        });
                }
            });

            $(".editable-imagefield.realestateobject", iframe.contents()).click($scope.handleObjectImageFieldClick);
            $(".editable-imagefield.staff", iframe.contents()).click($scope.handleStaffImageFieldClick);

            $scope.updateHideableFieldsList(iframe);

            //:#158: Everything is ready, just give the guillotine a few milliseconds to execute
            // and then show the iframe
            setTimeout(function () {
                iframe.removeClass("hide");
            }, 100);
        }

        $scope.resizeIframe = function () {
            if ($scope.currentPage == null) return;

            $(".page-editor-wrap").css("max-width", $scope._calculatePageEditorMaxWidth());
            $timeout(function() {
                var pageEditor = $("#page-editor");
                var scaleValue = pageEditor.width() / $scope.currentPage.width;
                $("iframe", pageEditor).css({
                    "transform-origin": "0 0",
                    "transform": "scale(" + scaleValue + ")"
                });
            });
        }

        $scope._calculatePageEditorMaxWidth = function () {
            var editSurface = $(".edit-surface");
            var verticalMargin = editSurface.outerHeight(true) - editSurface.outerHeight();
            if (surroundingAreaHeight === -1) surroundingAreaHeight = $(".tab-bar").height() + verticalMargin;
            var viewportHeight = $(window).height();
            return (viewportHeight - surroundingAreaHeight) / $scope.currentPage.aspectRatioInPercent * 100;
        }

        $scope._isImageFieldInitialized = function (imageField) {
            return imageField && !($("img", imageField).attr("id")); // Image selected and not a dummy image
        }
        $scope._getCurrentImageField = function () {
            if ($scope._isImageFieldInitialized(currentSelectedObjectImageField)) {
                return currentSelectedObjectImageField;
            } else if ($scope._isImageFieldInitialized(currentSelectedStaffImageField)) {
                return currentSelectedStaffImageField;
            } else {
                return null;
            }
        }

        $scope._disableAllFields = function () {
            if (currentSelectedTextField) {
                currentSelectedTextField.removeClass("active");
                currentSelectedTextField.html($scope.textEditorPristineValue);
                currentSelectedTextField = null;
            }
            if (currentSelectedHtmlField) {
                currentSelectedHtmlField.removeClass("active");
                currentSelectedHtmlField.html($scope.htmlEditorPristineValue);
                currentSelectedHtmlField = null;
            }
            if (currentSelectedObjectImageField) {
                currentSelectedObjectImageField.parent().removeClass("active");
                $("img", currentSelectedObjectImageField).guillotine("disable");
                currentSelectedObjectImageField.click($scope.handleObjectImageFieldClick); //Re-register click event to the old image
                currentSelectedObjectImageField = null;
            }
            if (currentSelectedStaffImageField) {
                currentSelectedStaffImageField.parent().removeClass("active");
                $("img", currentSelectedStaffImageField).guillotine("disable");
                currentSelectedStaffImageField.click($scope.handleStaffImageFieldClick); //Re-register click event to the old image
                currentSelectedStaffImageField = null;
            }
        }
    });
