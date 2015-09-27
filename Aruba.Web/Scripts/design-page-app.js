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
            getImages: function (printId) {
                return $http.get("/api/realestateobjects/print/" + printId + "/images");
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
    .controller("printEditorCtrl", function ($scope, $q, printService, pageService, fieldValuesService, realEstateObjectService) {
        // Local variables
        var currentSelectedTextField = null;
        var currentSelectedHtmlField = null;
        var currentSelectedImageField = null;

        // Initialize the CKEditors
        $scope.textEditor = {
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
            removeButtons: "Underline,Subscript,Cut,Copy,Paste,PasteText,PasteFromWord,Undo,Redo,Scayt,Link,Anchor,Unlink,Image,Table,HorizontalRule,SpecialChar,Maximize,Source,Strike,Superscript,RemoveFormat,Outdent,Indent,Blockquote,Styles,About,Format"
        };

        // Scope variables
        $scope.textEditorToolboxVisible = false;
        $scope.textEditorFormReady = false;
        $scope.htmlEditorToolboxVisible = false;
        $scope.htmlEditorFormReady = false;
        $scope.imageEditorToolboxVisible = false;


        // Scope functions
        $scope.switchToPage = function (pageId) {
            $('.off-canvas-wrap').foundation('offcanvas', 'hide', 'move-right');
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


        // Text editor functions
        $scope.textEditorValue = null;
        $scope.textEditorPristineValue = null;
        $scope.handleTextFieldClick = function () {
            $scope.textEditorFormReady = false;
            $scope._changeVisibleToolbox("textEditor");
            currentSelectedTextField = $(this);
            currentSelectedTextField.addClass("active");
            $scope.textEditorValue = $(this).html();
            $scope.$apply();
            $scope.delayedSetPristine($scope.textEditorForm, 100, function () { $scope.textEditorFormReady = true; });
        }
        $scope.saveTextEditorValue = function () {
            // Remove newlines and <p> tags
            var trimmed = $scope.textEditorValue;
            trimmed = trimmed.replace(/(\r\n|\n|\r)/gm, "");
            trimmed = _.startsWith(trimmed, "<p>") ? trimmed.slice(3) : trimmed;
            trimmed = _.endsWith(trimmed, "</p>") ? trimmed.slice(0, trimmed.length - 4) : trimmed;

            // Save the new value to the database
            $scope._saveFieldValue(currentSelectedTextField, { html: trimmed })
                .then(function () {
                    console.log("Field value updated successfully.");
                    $scope.textEditorForm.$setPristine();
                }, function () {
                    alert("Field value update failed.");
                });

            // Update the print page preview
            currentSelectedTextField.html(trimmed);
        }
        $scope.revertTextEditorValue = function () {
            $scope.textEditorValue = currentSelectedTextField.html();
            $scope.delayedSetPristine($scope.textEditorForm, 100);
        }


        // HTML editor functions
        $scope.handleHtmlFieldClick = function () {
            $scope.htmlEditorFormReady = false;
            $scope._changeVisibleToolbox("htmlEditor");
            currentSelectedHtmlField = $(this);
            currentSelectedHtmlField.addClass("active");
            $scope.htmlEditorValue = $(this).html();
            $scope.$apply();
            $scope.delayedSetPristine($scope.htmlEditorForm, 100, function () { $scope.htmlEditorFormReady = true; });
        }
        $scope.saveHtmlEditorValue = function () {
            var paragraphClass = currentSelectedHtmlField.data("firstparagraphclass");
            var withParagraphClass = $scope.htmlEditorValue.replace(/<p.*?>/gi, "<p class=\"" + paragraphClass + "\">");

            // Save the new value to the database
            $scope._saveFieldValue(currentSelectedHtmlField, { html: withParagraphClass })
                .then(function () {
                    console.log("Field value updated successfully.");
                    $scope.htmlEditorForm.$setPristine();
                }, function () {
                    alert("Field value update failed.");
                });

            // Update the print page preview
            currentSelectedHtmlField.html(withParagraphClass);
        }
        $scope.revertHtmlEditorValue = function () {
            $scope.htmlEditorValue = currentSelectedHtmlField.html();
            $scope.delayedSetPristine($scope.htmlEditorForm, 100);
        }


        // Image editor functions
        $scope.handleImageFieldClick = function () {
            $scope._changeVisibleToolbox("imageEditor");
            currentSelectedImageField = $(this);
            $scope.$apply();
            currentSelectedImageField.parent().addClass("active");
            currentSelectedImageField.unbind("click");
            $("img", currentSelectedImageField).guillotine("enable");
        }
        $scope.handleObjectImageClick = function (imageUrl) {
            var targetImage = $("img", currentSelectedImageField);
            targetImage.removeAttr("id");
            targetImage.removeAttr("width");
            targetImage.removeAttr("height");
            targetImage.guillotine("remove");
            targetImage.hide()
                .one("load", function () {
                    $(this).guillotine({
                        width: currentSelectedImageField.width(),
                        height: currentSelectedImageField.height(),
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
            if (!$scope._isCurrentImageFieldInitialized()) return;
            $("img", currentSelectedImageField).guillotine("zoomIn");
        }
        $scope.decreaseImageSize = function () {
            if (!$scope._isCurrentImageFieldInitialized()) return;
            $("img", currentSelectedImageField).guillotine("zoomOut");
        }
        $scope.onImageEditorChange = function () {
            $scope._saveImageEditorValue();
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

        realEstateObjectService.getImages($scope.printId)
            .then(function (response) {
                $scope.images = response.data;
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
            if (!$scope._isCurrentImageFieldInitialized()) return;
            var targetImage = $("img", currentSelectedImageField);
            var guillotineData = targetImage.guillotine("getData"); // { scale: 1.4, angle: 270, x: 10, y: 20, w: 400, h: 300 }
            var data = $.extend({}, { url: targetImage.attr("src") }, guillotineData);
            $scope._saveFieldValue(currentSelectedImageField, data)
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
            $scope.imageEditorToolboxVisible = (toolbox === "imageEditor");
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
            editableImageFields.click($scope.handleImageFieldClick);
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

            //:#158: Everything is ready, just give the guillotine a few milliseconds to execute
            // and then show the iframe
            setTimeout(function () {
                iframe.removeClass("hide");
            }, 100);
        }

        $scope.resizeIframe = function () {
            var scaleValue = $("#page-editor").width() / $scope.currentPage.width;
            $("#page-editor iframe").css({
                "transform-origin": "0 0",
                "transform": "scale(" + scaleValue + ")"
            });
        }

        $scope._isCurrentImageFieldInitialized = function () {
            return currentSelectedImageField && !($("img", currentSelectedImageField).attr("id")); // Image selected and not a dummy image
        }

        $scope._disableAllFields = function () {
            if (currentSelectedTextField) {
                currentSelectedTextField.removeClass("active");
                currentSelectedTextField = null;
            }
            if (currentSelectedHtmlField) {
                currentSelectedHtmlField.removeClass("active");
                currentSelectedHtmlField = null;
            }
            if (currentSelectedImageField) {
                currentSelectedImageField.parent().removeClass("active");
                $('img', currentSelectedImageField).guillotine('disable');
                currentSelectedImageField.click($scope.handleImageFieldClick); //Re-register click event to the old image
                currentSelectedImageField = null;
            }
        }

    });
