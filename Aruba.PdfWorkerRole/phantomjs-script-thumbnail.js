var page = require('webpage').create();
var system = require('system');

var address = system.args[1];
var outputPage = system.args[2];
var pageWidth = parseInt(system.args[3]);
var pageHeight = parseInt(system.args[4]);
var thumbnailWidth = parseInt(system.args[5]);
var thumbnailHeight = parseInt(system.args[6]);

page.clipRect = {
    top: 0,
    left: 0,
    width: thumbnailWidth,
    height: thumbnailHeight
};

page.viewportSize = {
    width: thumbnailWidth,
    height: thumbnailHeight
};

page.zoomFactor = thumbnailWidth / pageWidth;

page.open(address, function (status) {
    if (status !== 'success') {
        console.log('Unable to load the address!');
        phantom.exit(1);
    } else {
        window.setTimeout(function () {
            page.render(outputPage);
            phantom.exit();
        }, 200);
    }
});