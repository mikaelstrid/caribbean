var page = require('webpage').create();
var system = require('system');

var address = system.args[1];
var outputPage = system.args[2];
var pageWidth = parseInt(system.args[3]);
var pageHeight = parseInt(system.args[4]);
var dpi = parseInt(system.args[5]);

var paperWidthInInch = pageWidth / dpi;
var paperHeightInInch = pageHeight / dpi;

page.paperSize = {
    width: paperWidthInInch + 'in',
    height: paperHeightInInch + 'in',
    margin: '0in'
}

page.zoomFactor = paperWidthInInch * 120 / pageWidth;

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