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
    .controller("printEditorCtrl", function ($scope, printService, pageService) {
        $scope.textEditor = {
            language: 'sv',
            uiColor: '#ffffff',
            toolbarGroups: [
		        { name: 'clipboard', groups: ['clipboard', 'undo'] },
		        { name: 'editing', groups: ['find', 'selection', 'spellchecker', 'editing'] },
		        { name: 'links', groups: ['links'] },
		        { name: 'insert', groups: ['insert'] },
		        { name: 'forms', groups: ['forms'] },
		        { name: 'tools', groups: ['tools'] },
		        { name: 'document', groups: ['mode', 'document', 'doctools'] },
		        { name: 'others', groups: ['others'] },
		        '/',
		        { name: 'basicstyles', groups: ['basicstyles', 'cleanup'] },
		        { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi', 'paragraph'] },
		        { name: 'styles', groups: ['styles'] },
		        { name: 'colors', groups: ['colors'] },
		        { name: 'about', groups: ['about'] }
            ],
            removeButtons: 'Subscript,Strike,Anchor,Image,Blockquote,Styles,Format,About,Cut,Copy,Paste,PasteText,Redo,Undo,Link,Scayt,Source,Maximize,RemoveFormat,NumberedList,Indent,Outdent,Table,HorizontalRule,SpecialChar,PasteFromWord,Unlink,BulletedList,Superscript,Underline'
        };

        printService.getPages($scope.printId)
            .then(function (response) {
                console.log(response.data);
                $scope.pages = response.data;
                if (response.data.length > 0)
                    $scope.switchToPage(response.data[0].id);
            }, function (response) {
                alert("Call printService.getPages failed.");
            });

        $(window).resize(function () {
            if ($scope.currentPage)
                $scope.resizeIframe();
        });

        $scope.switchToPage = function (pageId) {
            pageService.getPage(pageId)
                .then(function (response) {
                    $scope.currentPage = response.data;
                    var p = response.data;
                    $("#dummy").css("margin-top", p.aspectRatioInPercent + "%");
                    $("#page-editor").empty().append($("<iframe id='pageEditorIframe' frameborder='0' scrolling='no' style='width: " + p.width + "px; height: " + p.height + "px;'></iframe>"));
                    $("#pageEditorIframe").attr('src', p.previewUrl);
                    $("#pageEditorIframe").load($scope.initIframe);
                    $scope.resizeIframe();
                }, function (response) {
                    alert("Call to pageService.getPage failed.");
                });
        }

        $scope.initIframe = function () {
            var iframe = $("#pageEditorIframe");

            $(".editable-textfield", iframe.contents()).click($scope.handleTextFieldClick);
            //$(".editable-textfield", iframe.contents()).click(function () { console.log("textfield clicked;") });

            //$(".editable-htmlfield", iframe.contents()).click(viewModel.handleHtmlFieldClick);
            $(".editable-htmlfield", iframe.contents()).click(function () { console.log("htmlfield clicked;") });

            var editableImageFields = $(".editable-imagefield", iframe.contents());
            editableImageFields.each(function () {
                var parent = $(this).parent();
                parent.addClass("editable-imagefield-parent");
                parent.css("z-index", parseInt($(this).css("z-index")) + 1);
            });
            //editableImageFields.click(viewModel.handleImageFieldClick);
            editableImageFields.click(function () { console.log("imagefield clicked;") });
            $(".editable-imagefield[data-imagefieldtype='1']", iframe.contents()).each(function () {
                if ($(this).data("afvid")) {
                    var initData = $(this).data("init");
                    $("img", $(this)).guillotine({ width: $(this).width(), height: $(this).height(), init: initData });
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
                            $(this).guillotine({ width: imageField.width(), height: imageField.height(), init: initData });
                            $("img", $(this)).guillotine("disable");
                            $(this).show();
                        })
                        .attr("src", selectedPictureUrl)
                        .each(function () {
                            if (this.complete) $(this).trigger("load");
                        });
                }
            });
        }

        $scope.resizeIframe = function () {
            var scaleValue = $("#page-editor").width() / $scope.currentPage.width;
            $("#page-editor iframe").css({
                "transform-origin": "0 0",
                "transform": "scale(" + scaleValue + ")"
            });
        }

        $scope.handleTextFieldClick = function () {
            $scope.textEditorValue = $(this).html();
            $scope.$apply();
        }
    });
