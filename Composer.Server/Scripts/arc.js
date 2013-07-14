var _canvas = null;
var _context = null;
var yoffset = 0;

function checkContext() {
    if (_canvas == null || _context == null) {
        setContext();
    }
    return (_canvas != null && _context != null);
}

function setContext() {
    _canvas = document.getElementById("compositionCanvas");
    if (_canvas !== null) {
        _context = _canvas.getContext("2d");
        _canvas.height = 20000
        _canvas.width = 20000
    }
}

var numberOfArcs = 1;
var firstX = 10;
var firstY = 40;
var y = firstY;
var x = firstX;
var verticalSpacing = 15;
var horizontalSpacing = 200;
var maxArcWidth = 800;
var minArcWidth = 320;

function renderArcs() {
    checkContext();
    for (arcWidth = minArcWidth; arcWidth <= maxArcWidth; arcWidth++) {
        if (arcWidth % 10 == 0) {
            for (k = -100; k >= -120; k--) {
                if (k % 10 == 0) {
                    for (i = 0; i < numberOfArcs; i++) {
                        y = y + verticalSpacing;
                        renderArc(x, y, x + arcWidth, y, y - k, k);
                    }
                }
            }
        }
    }
    _context.closePath();
}

function renderArc(x, y, x2, y2, cy, k) {
    _context.beginPath();
    _context.moveTo(x, y);
    _context.quadraticCurveTo((x + x2) / 2, cy, x2, y2);
    _context.lineWidth = 1;
    _context.strokeStyle = 'black';
    _context.stroke();
    _context.closePath();
    _context.fillText((x2 - x).toString() + "," + k.toString(), x + (x2 - x), y);
}
