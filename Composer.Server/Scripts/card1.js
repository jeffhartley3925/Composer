function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regexS = "[\\?&]" + name + "=([^&#]*)";
    var regex = new RegExp(regexS);
    var results = regex.exec(window.location.search);
    if (results == null)
        return "";
    else
        return decodeURIComponent(results[1].replace(/\+/g, " "));
}

//enums
//#region

var enumTimeSignature =
{
    "FourFour_1": 4,
    "FourFour_2": 11,
    "FourFour_3": 12,
    "ThreeFour": 3,
    "TwoFour": 2,
    "TwoTwo": 0
};

var enumBar =
{
    "Double": 0,
    "End": 1,
    "BeginRepeat": 2,
    "EndRepeat": 3,
    "BeginEndRepeat": 4,
    "Standard": 5,
    "None": 6,
    "StaffLeft": 7,
    "StaffRight": 8
};

var enumClef =
{
    "Treble": 1,
    "Bass": 0,
    "Alto": 2,
    "Tenor": 3
};

var enumAccidental =
{
    "Sharp": 25,
    "Flat": 26,
    "Natural": 27
};

var enumKey =
{
    "C": 0,
    "G": 1,
    "D": 2,
    "A": 3,
    "E": 4,
    "B": 5,
    "Gb": 12,
    "Db": 13,
    "Ab": 8,
    "Eb": 9,
    "Bb": 10,
    "F": 11,
    "Fs": 6,
    "Cs": 7
};

var enumNote =
{
    "Whole": 6,
    "Half": 7,
    "Quarter": 8,
    "Eighth": 9,
    "Sixteenth": 10,
    "Thirtysecond": 11,
    "DottedWhole": 96,
    "DottedHalf": 97,
    "DottedQuarter": 98,
    "DottedEighth": 99,
    "DottedSixteenth": 100,
    "DottedThirtysecond": 101
};

var enumRest =
{
    "Whole": 0,
    "Half": 1,
    "Quarter": 2,
    "Eighth": 3,
    "Sixteenth": 4,
    "Thirtysecond": 102,
    "DottedWhole": 90,
    "DottedHalf": 92,
    "DottedQuarter": 93,
    "DottedEighth": 94,
    "DottedSixteenth": 95,
    "DottedThirtysecond": 103
};

var enumOrientation =
{
    "Up": 0,
    "Down": 1 
};

//#endregion

//variables
//#region

String.prototype.trim = function () { return this.replace(/^\s\s*/, '').replace(/\s\s*$/, ''); };

var randomColorIndex = 0;
var defaultColorIndex = 2;
var colors = new Array()
colors[0] = "red";
colors[1] = "cyan";
colors[2] = "green";
colors[3] = "blue";
colors[4] = "orange";
colors[5] = "pink";
colors[6] = "black";
var defaultColor = colors[defaultColorIndex];
var canvasWidth = "0px";

var sequenceIncrement = 100;

var verseCount = 0, collaborationCount = 0;
var sgDensity = 0, sDensity = 0, mDensity = 0;
var composition;
var flagSpacing = 6;

var sharpVectorId = 25;
var flatVectorId = 26;
var naturalVectorId = 27;

var lineSpacing = 8;
var cele = null;
var ctx = null;
var timeSignatureOffset = 32;
var topDeadSpace = 20;
var provenanceHeight = 85;
var defaultLineWidth = .75;
var staffWidth = 0;
var staffDimensionAreaWidth = 104;
var staffGap = 120;
var staffOffset = -staffGap;
var staffTopOffset = 0;

var noteStrokeThickness = 0;
var noteForeground = "#333333";
var noteStroke = "#333333";
var noteOpacity = 1;

var arcStrokeThickness = 1;
var arcForeground = noteForeground;
var arcBackground = "transparent";
var arcStroke = noteStroke;
var arcOpacity = noteOpacity;

var measureBackground = "#ffffff";
var measureTopDeadSpace = 5;
var measureBottomDeadSpace = 13;

var verseFontFamily = "Lucida Sans Unicode";
var verseFontWeight = "Normal";
var verseFontSize = 12;
var verseForeground = noteForeground;
var verseHeight = 14;

var collaborations = [];
var staffgroups = [];
var staffs = [];
var measures = [];
var chords = [];
var notes = [];
var measureSpans;
var verses = [];
var arcs = [];

var canvasId = "";
//#endregion

//renderComposition
//#region

function initializeObjectCollections() {
    collaborations = [];
    staffgroups = [];
    staffs = [];
    measures = [];
    chords = [];
    notes = [];
    verses = [];
    arcs = [];
    staffOffset = -staffGap; //top justify
    staffWidth = 0;
}

function renderComposition(_canvasId) {
    measureSpans = new Array();
    canvasId = _canvasId;
    initializeObjectCollections();
    sgDensity = composition.Staffgroups.length;
    sDensity = composition.Staffgroups[0].Staffs.length;
    mDensity = composition.Staffgroups[0].Staffs[0].Measures.length;
    composition.Staffgroups.sort(function (a, b) { return a.Sequence - b.Sequence });
    for (var i = 0; i < composition.Staffgroups.length; i++) {
        var staffgroup = composition.Staffgroups[i];
        staffgroups[staffgroups.length] = staffgroup;
        staffgroup.Staffs.sort(function (a, b) { return a.Sequence - b.Sequence });
        for (var j = 0; j < staffgroup.Staffs.length; j++) {
            var staff = staffgroup.Staffs[j];
            staffs[staffs.length] = staff;
            staff.Measures.sort(function (a, b) { return a.Sequence - b.Sequence });
            for (var k = 0; k < staff.Measures.length; k++) {
                var measure = staff.Measures[k];
                measures[measures.length] = measure;
                measure.Chords.sort(function (a, b) { return a.StartTime - b.StartTime });
                var spannedNotes = new Array();
                for (var l = 0; l < measure.Chords.length; l++) {
                    var chord = measure.Chords[l];
                    chord.words = [];
                    chords[chords.length] = chord;
                    chord.Notes.sort(function (a, b) { return a.Duration - b.Duration });
                    for (var m = 0; m < chord.Notes.length; m++) {
                        var note = chord.Notes[m];
                        notes[notes.length] = note;
                        if (note.IsSpanned == 1) {
                            spannedNotes[spannedNotes.length] = note;
                        }
                    }
                }
                measureSpans[measure.Index] = spannedNotes;
            }
        }
    }

    verseCount = composition.Verses.length;
    composition.Verses.sort(function (a, b) { return a.Sequence - b.Sequence });
    for (i = 0; i < verseCount; i++) {
        var verse = composition.Verses[i];
        var words = verse.Text.split(" ");
        for (var c = 0; c < chords.length; c++) {
            var chord = chords[c];
            chord.words[i] = words[c];
        }
        verses[verses.length] = verse;
    }

    staffGap = staffGap + (verseCount * verseHeight)
    staffOffset = -staffGap;

    collaborationCount = composition.Collaborations.length;
    for (i = 0; i < collaborationCount; i++) {
        var collaboration = composition.Collaborations[i];
        collaborations[collaborations.length] = collaboration;
    }

    for (i = 0; i < composition.Arcs.length; i++) {
        var arc = composition.Arcs[i];
        arcs[arcs.length] = arc;
    }
    setScale();
    renderMeasures();
    renderProvenance();
    renderArcs();
    renderLyrics();
}
//#endregion

//renderLyrics
//#region

function renderLyrics() {
    var staff, measure, chord, word, x, y, xoffset;
    var lastMeasureWidth = null;
    var firstChordInStaff = false;
    for (var i = 0; i < verseCount; i++) {
        var lastMeasureSequence = null;
        for (var c = 0; c < chords.length; c++) {
            chord = chords[c];
            measure = GetMeasureFromId(chord.Measure_Id);
            if (measure.Sequence != lastMeasureSequence) {
                lastMeasureSequence = measure.Sequence;
                if (measure.Sequence == 0) {
                    firstChordInStaff = true;
                    xoffset = 0;
                }
                else {
                    xoffset = xoffset + (1 * lastMeasureWidth);
                }
            }
            staff = getStaffFromId(measure.Staff_Id);
            if (chord.words[i] != null) {
                word = chord.words[i];
                y = measure.Y + 75 + (i * verseHeight);
                if (firstChordInStaff) {
                    x = xoffset + chord.Location_X + 45;
                    firstChordInStaff = false;
                }
                else {
                    x = xoffset + chord.Location_X + 45 - ((word.length * 4) / 2) + 2;
                }
                ctx.font = 'normal 13px Calibri';
                ctx.fillStyle = defaultColor;
                ctx.fillText(word, x, y);
                lastMeasureWidth = (1 * measure.Width);
            }
        }
    }
}
//#endregion

//renderProvenance
//#region

function renderProvenance() {
    var title = composition.Provenance.TitleLine;
    var author = composition.Audit.TitleLine;
    var cdate = composition.Audit.CreateDate.substring(0, 10);
    var largeFontSize = composition.Provenance.LargeFontSize;
    var smallFontSize = composition.Provenance.SmallFontSize;
    var fontFamily = composition.Provenance.FontFamily;
    var author = collaborations[0].Name;

    var largeLetterWidth = parseInt(largeFontSize / 2);
    var smallLetterWidth = parseInt(smallFontSize / 2);

    if (checkContext()) {

        ctx.fillStyle = defaultColor;
        ctx.font = "Bold " + largeFontSize + "px " + fontFamily;
        ctx.textBaseline = "top";
        ctx.fillText(title, staffWidth / 2 - (title.length * largeLetterWidth) / 2, +topDeadSpace);

        ctx.fillStyle = defaultColor;
        ctx.font = smallFontSize + "px " + fontFamily;
        ctx.textBaseline = "top";
        ctx.fillText(cdate, 0, provenanceHeight - 20 + topDeadSpace);

        ctx.fillStyle = defaultColor;
        ctx.font = smallFontSize + "px " + fontFamily;
        ctx.textBaseline = "top";
        ctx.fillText(author, staffWidth - (smallLetterWidth * author.length), provenanceHeight - 20 + topDeadSpace);
    }
}
//#endregion

//renderMeasures
//#region
function renderMeasures() {
    var staff = null;
    var currentStartX = 0, currentEndX = 0, previousEndX = 0;
    measures.sort(function (a, b) { return a.Index - b.Index });
    staffWidth = GetStaffWidth();
    for (var k = 0; k < measures.length; k++) {
        var measure = measures[k];
        var staff = getStaffFromId(measure.Staff_Id);
        if (measure.Sequence == 0) {

            staffOffset = staffOffset + staffGap;

            //staff lines
            currentStartX = 0;
            currentEndX = staffDimensionAreaWidth;
            renderStaff(currentStartX, currentEndX, staffOffset + provenanceHeight + topDeadSpace, staff);
            //end staff lines

            currentStartX = currentEndX;
            currentEndX = currentEndX + parseInt(measure.Width);
        }
        else {
            currentStartX = previousEndX;
            currentEndX = previousEndX + parseInt(measure.Width);
        }
        measure.X = currentStartX;
        measure.Y = staffOffset + provenanceHeight + topDeadSpace;
        previousEndX = currentEndX;
        renderMeasure(measure.Sequence, currentStartX, currentEndX, measure.Y, measure.Bar_Id);

        renderChords(measure);
        renderSpans(measure);
    }
}
//#endregion

//renderMeasure
//#region
function renderMeasure(sequence, startX, endX, startY, barId) {

    if (checkContext()) {

        if (sequence == 0 && barId != enumBar.None) {
            renderBar(startX, startY, enumBar.StaffLeft)
        }

        for (var i = 0; i < 5; i++) {
            var y = i * lineSpacing + startY;
            drawLine(startX, y, endX, y, 1, 1)
        }

        if (barId != enumBar.None) {
            renderBar(endX, y, barId)
        }
    }
}
//#endregion

//renderBar
//#region
function renderBar(x, y, barId) {
    ctx.lineWidth = defaultLineWidth;
    x = x - 64;
    switch (barId) {
        case enumBar.StaffLeft:
            drawBarLine(x - 40, y + 31);
            break;
        case enumBar.Standard:
            drawBarLine(x - 1, y);
            break;
        case enumBar.Double:
            drawBarLine(x - 1, y);
            drawBarLine(x + 2, y);
            break;
        case enumBar.End:
            drawBarLine(x - 6, y);
            drawBarLine(x - 2, y, 3);
            break;
        case enumBar.BeginRepeat:
            break;
        case enumBar.EndRepeat:
            break;
        case enumBar.BeginEndRepeat:
            break;
    }
}
//#endregion

//renderStaff & renderStaffLines
//#region

function renderStaff(startX, endX, staffOffset, staff) {
    renderStaffLines(startX, endX, staffOffset);
    if (staff != null) {
        renderClef(staff.Clef_Id, staffOffset);
        renderKeySignature(staff.Key_Id, staffOffset);
        renderTimeSignature(staff.TimeSignature_Id, staffOffset);
    }
}

function renderStaffLines(startX, endX, staffOffset) {
    renderMeasure(null, startX, endX, staffOffset, enumBar.None);
}

//#endregion

//renderClef
//#region
function renderClef(clefId, staffOffset) {

    switch (clefId) {
        case enumClef.Bass:
            x = 2;
            y = -37 + staffOffset;

            //ctx.save();
            ctx.beginPath();

            ctx.moveTo(x + 14.3, y + 54.9);
            ctx.bezierCurveTo(x + 14.8, y + 57, x + 15.2, y + 59.1, x + 15.6, y + 61.2);
            ctx.bezierCurveTo(x + 16, y + 63.2, x + 16.4, y + 65.3, x + 16.9, y + 67.3);
            ctx.bezierCurveTo(x + 18.4, y + 66.8, x + 19.4, y + 65.8, x + 19.9, y + 64.4);
            ctx.bezierCurveTo(x + 20.4, y + 63.1, x + 20.5, y + 61.7, x + 20.3, y + 60.3);
            ctx.bezierCurveTo(x + 20, y + 58.9, x + 19.3, y + 57.6, x + 18.3, y + 56.5);
            ctx.bezierCurveTo(x + 17.2, y + 55.5, x + 15.9, y + 54.9, x + 14.3, y + 54.9);
            ctx.moveTo(x + 11.1, y + 45.1);
            ctx.bezierCurveTo(x + 10.1, y + 46, x + 9.1, y + 46.9, x + 8.1, y + 47.9);
            ctx.bezierCurveTo(x + 7.1, y + 49, x + 6.2, y + 50, x + 5.4, y + 51.2);
            ctx.bezierCurveTo(x + 4.6, y + 52.4, x + 3.9, y + 53.6, x + 3.4, y + 54.8);
            ctx.bezierCurveTo(x + 2.9, y + 56.1, x + 2.7, y + 57.5, x + 2.7, y + 58.9);
            ctx.bezierCurveTo(x + 2.7, y + 60.2, x + 3, y + 61.4, x + 3.5, y + 62.6);
            ctx.bezierCurveTo(x + 4.1, y + 63.7, x + 4.8, y + 64.7, x + 5.7, y + 65.6);
            ctx.bezierCurveTo(x + 6.6, y + 66.5, x + 7.7, y + 67.1, x + 8.9, y + 67.6);
            ctx.bezierCurveTo(x + 10.1, y + 68.1, x + 11.3, y + 68.4, x + 12.5, y + 68.4);
            ctx.bezierCurveTo(x + 12.6, y + 68.4, x + 12.8, y + 68.3, x + 13.1, y + 68.3);
            ctx.bezierCurveTo(x + 13.5, y + 68.2, x + 13.8, y + 68.2, x + 14.2, y + 68.1);
            ctx.bezierCurveTo(x + 14.6, y + 68.1, x + 14.9, y + 68, x + 15.3, y + 67.9);
            ctx.bezierCurveTo(x + 15.6, y + 67.9, x + 15.7, y + 67.8, x + 15.7, y + 67.7);
            ctx.bezierCurveTo(x + 15.6, y + 67.5, x + 15.6, y + 67.2, x + 15.6, y + 66.8);
            ctx.bezierCurveTo(x + 15.4, y + 66.1, x + 15.3, y + 65.4, x + 15.2, y + 64.7);
            ctx.bezierCurveTo(x + 15, y + 64, x + 14.9, y + 63.3, x + 14.7, y + 62.5);
            ctx.bezierCurveTo(x + 14.4, y + 61.3, x + 14.2, y + 60.1, x + 14, y + 58.8);
            ctx.bezierCurveTo(x + 13.7, y + 57.6, x + 13.5, y + 56.3, x + 13.2, y + 55);
            ctx.bezierCurveTo(x + 12.2, y + 55.4, x + 11.4, y + 55.9, x + 10.7, y + 56.7);
            ctx.bezierCurveTo(x + 10.1, y + 57.5, x + 9.7, y + 58.4, x + 9.5, y + 59.3);
            ctx.bezierCurveTo(x + 9.4, y + 60.3, x + 9.5, y + 61.2, x + 9.9, y + 62.1);
            ctx.bezierCurveTo(x + 10.2, y + 63, x + 11, y + 63.8, x + 12, y + 64.3);
            ctx.bezierCurveTo(x + 12.3, y + 64.3, x + 12.4, y + 64.4, x + 12.5, y + 64.6);
            ctx.bezierCurveTo(x + 12.6, y + 64.9, x + 12.5, y + 65, x + 12.2, y + 65);
            ctx.bezierCurveTo(x + 11.2, y + 64.7, x + 10.3, y + 64.3, x + 9.5, y + 63.7);
            ctx.bezierCurveTo(x + 8.1, y + 62.5, x + 7.3, y + 60.9, x + 7.2, y + 59.1);
            ctx.bezierCurveTo(x + 7.1, y + 58.1, x + 7.2, y + 57.2, x + 7.4, y + 56.4);
            ctx.bezierCurveTo(x + 7.7, y + 55.5, x + 8, y + 54.7, x + 8.5, y + 54);
            ctx.bezierCurveTo(x + 9.1, y + 53.2, x + 9.7, y + 52.5, x + 10.5, y + 52);
            ctx.bezierCurveTo(x + 10.5, y + 51.9, x + 10.7, y + 51.9, x + 10.8, y + 51.7);
            ctx.bezierCurveTo(x + 11, y + 51.6, x + 11.3, y + 51.5, x + 11.5, y + 51.4);
            ctx.bezierCurveTo(x + 11.7, y + 51.3, x + 11.9, y + 51.1, x + 12.1, y + 51);
            ctx.bezierCurveTo(x + 12.3, y + 50.9, x + 12.4, y + 50.9, x + 12.4, y + 50.9);
            ctx.bezierCurveTo(x + 12.1, y + 49.9, x + 11.9, y + 48.9, x + 11.7, y + 48);
            ctx.bezierCurveTo(x + 11.5, y + 47.1, x + 11.3, y + 46.1, x + 11.1, y + 45.1);
            ctx.moveTo(x + 15.5, y + 24.7);
            ctx.bezierCurveTo(x + 15.1, y + 24.6, x + 14.7, y + 24.7, x + 14.3, y + 24.8);
            ctx.bezierCurveTo(x + 13.9, y + 24.9, x + 13.5, y + 25.2, x + 13.2, y + 25.5);
            ctx.bezierCurveTo(x + 12.8, y + 25.8, x + 12.5, y + 26.1, x + 12.4, y + 26.4);
            ctx.bezierCurveTo(x + 11.9, y + 27.2, x + 11.5, y + 28.1, x + 11.1, y + 29.1);
            ctx.bezierCurveTo(x + 10.8, y + 30.1, x + 10.6, y + 31.1, x + 10.4, y + 32.2);
            ctx.bezierCurveTo(x + 10.3, y + 33.3, x + 10.3, y + 34.3, x + 10.4, y + 35.4);
            ctx.bezierCurveTo(x + 10.5, y + 36.4, x + 10.7, y + 37.4, x + 11.1, y + 38.2);
            ctx.bezierCurveTo(x + 11.7, y + 37.9, x + 12.3, y + 37.4, x + 12.9, y + 36.7);
            ctx.bezierCurveTo(x + 13.4, y + 36, x + 13.9, y + 35.3, x + 14.4, y + 34.6);
            ctx.bezierCurveTo(x + 14.9, y + 33.8, x + 15.3, y + 33, x + 15.6, y + 32.2);
            ctx.bezierCurveTo(x + 16, y + 31.4, x + 16.2, y + 30.7, x + 16.4, y + 30.1);
            ctx.bezierCurveTo(x + 16.7, y + 29.4, x + 16.8, y + 28.6, x + 16.9, y + 27.8);
            ctx.bezierCurveTo(x + 17, y + 26.9, x + 16.9, y + 26.2, x + 16.5, y + 25.6);
            ctx.bezierCurveTo(x + 16.3, y + 25.1, x + 15.9, y + 24.8, x + 15.5, y + 24.7);
            ctx.moveTo(x + 14.1, y + 19);
            ctx.bezierCurveTo(x + 14.3, y + 18.9, x + 14.6, y + 18.9, x + 14.8, y + 19.1);
            ctx.bezierCurveTo(x + 15.1, y + 19.3, x + 15.3, y + 19.6, x + 15.6, y + 19.9);
            ctx.bezierCurveTo(x + 15.8, y + 20.3, x + 16, y + 20.6, x + 16.1, y + 20.9);
            ctx.bezierCurveTo(x + 16.3, y + 21.3, x + 16.4, y + 21.5, x + 16.5, y + 21.6);
            ctx.bezierCurveTo(x + 17.1, y + 22.7, x + 17.5, y + 23.8, x + 17.7, y + 25);
            ctx.bezierCurveTo(x + 18, y + 26.2, x + 18.1, y + 27.4, x + 18.2, y + 28.6);
            ctx.bezierCurveTo(x + 18.3, y + 30.4, x + 18.2, y + 32.2, x + 17.9, y + 33.9);
            ctx.bezierCurveTo(x + 17.6, y + 35.7, x + 17, y + 37.4, x + 16.2, y + 39);
            ctx.bezierCurveTo(x + 15.9, y + 39.6, x + 15.6, y + 40.1, x + 15.3, y + 40.6);
            ctx.bezierCurveTo(x + 15, y + 41.1, x + 14.6, y + 41.6, x + 14.2, y + 42);
            ctx.bezierCurveTo(x + 14.1, y + 42.1, x + 14, y + 42.3, x + 13.8, y + 42.5);
            ctx.bezierCurveTo(x + 13.5, y + 42.7, x + 13.3, y + 42.9, x + 13.1, y + 43.2);
            ctx.bezierCurveTo(x + 12.9, y + 43.4, x + 12.7, y + 43.6, x + 12.5, y + 43.8);
            ctx.bezierCurveTo(x + 12.3, y + 44, x + 12.2, y + 44.1, x + 12.2, y + 44.2);
            ctx.bezierCurveTo(x + 12.5, y + 45.4, x + 12.7, y + 46.5, x + 12.8, y + 47.4);
            ctx.bezierCurveTo(x + 13, y + 47.9, x + 13.1, y + 48.4, x + 13.2, y + 48.9);
            ctx.bezierCurveTo(x + 13.3, y + 49.5, x + 13.4, y + 50, x + 13.5, y + 50.4);
            ctx.bezierCurveTo(x + 13.5, y + 50.4, x + 13.7, y + 50.4, x + 14.1, y + 50.4);
            ctx.bezierCurveTo(x + 14.6, y + 50.5, x + 15, y + 50.5, x + 15.6, y + 50.6);
            ctx.bezierCurveTo(x + 16.1, y + 50.7, x + 16.5, y + 50.8, x + 17, y + 50.9);
            ctx.bezierCurveTo(x + 17.4, y + 51, x + 17.7, y + 51, x + 17.8, y + 51.1);
            ctx.bezierCurveTo(x + 18.9, y + 51.6, x + 19.8, y + 52.3, x + 20.6, y + 53.2);
            ctx.bezierCurveTo(x + 21.4, y + 54, x + 22, y + 55, x + 22.4, y + 56);
            ctx.bezierCurveTo(x + 22.9, y + 57.1, x + 23.1, y + 58.2, x + 23.2, y + 59.4);
            ctx.bezierCurveTo(x + 23.2, y + 60.5, x + 23, y + 61.7, x + 22.7, y + 62.8);
            ctx.bezierCurveTo(x + 21.8, y + 65.1, x + 20.4, y + 66.7, x + 18.6, y + 67.8);
            ctx.bezierCurveTo(x + 18.3, y + 67.9, x + 18, y + 68, x + 17.7, y + 68.2);
            ctx.bezierCurveTo(x + 17.3, y + 68.3, x + 17.1, y + 68.6, x + 17.2, y + 68.9);
            ctx.lineTo(x + 17.6, y + 71);
            ctx.bezierCurveTo(x + 17.9, y + 71.9, x + 18.1, y + 72.8, x + 18.2, y + 73.9);
            ctx.bezierCurveTo(x + 18.4, y + 74.9, x + 18.5, y + 75.9, x + 18.6, y + 76.9);
            ctx.bezierCurveTo(x + 18.7, y + 78.2, x + 18.5, y + 79.4, x + 18, y + 80.4);
            ctx.bezierCurveTo(x + 17.4, y + 81.5, x + 16.7, y + 82.3, x + 15.8, y + 83);
            ctx.bezierCurveTo(x + 14.9, y + 83.6, x + 13.9, y + 84.1, x + 12.7, y + 84.2);
            ctx.bezierCurveTo(x + 11.5, y + 84.4, x + 10.3, y + 84.2, x + 9.1, y + 83.8);
            ctx.bezierCurveTo(x + 8, y + 83.3, x + 7.1, y + 82.7, x + 6.3, y + 81.9);
            ctx.bezierCurveTo(x + 5.6, y + 81, x + 5.2, y + 80, x + 5.2, y + 78.7);
            ctx.bezierCurveTo(x + 5.2, y + 78, x + 5.4, y + 77.2, x + 5.9, y + 76.4);
            ctx.bezierCurveTo(x + 6.3, y + 75.5, x + 6.9, y + 75, x + 7.6, y + 74.6);
            ctx.bezierCurveTo(x + 8.4, y + 74.2, x + 9.2, y + 74.1, x + 9.8, y + 74.3);
            ctx.bezierCurveTo(x + 10.5, y + 74.5, x + 11, y + 74.8, x + 11.5, y + 75.3);
            ctx.bezierCurveTo(x + 11.9, y + 75.8, x + 12.2, y + 76.5, x + 12.4, y + 77.2);
            ctx.bezierCurveTo(x + 12.5, y + 78, x + 12.5, y + 78.7, x + 12.3, y + 79.3);
            ctx.bezierCurveTo(x + 12.1, y + 79.9, x + 11.7, y + 80.5, x + 11.2, y + 80.9);
            ctx.bezierCurveTo(x + 10.6, y + 81.4, x + 9.8, y + 81.6, x + 8.8, y + 81.5);
            ctx.bezierCurveTo(x + 9.2, y + 82.3, x + 9.8, y + 82.8, x + 10.5, y + 83);
            ctx.bezierCurveTo(x + 11.3, y + 83.2, x + 12, y + 83.2, x + 12.8, y + 83);
            ctx.bezierCurveTo(x + 13.6, y + 82.8, x + 14.4, y + 82.5, x + 15.1, y + 82.1);
            ctx.bezierCurveTo(x + 15.8, y + 81.6, x + 16.3, y + 81.2, x + 16.7, y + 80.6);
            ctx.bezierCurveTo(x + 17, y + 80.3, x + 17.2, y + 79.8, x + 17.3, y + 79.2);
            ctx.bezierCurveTo(x + 17.4, y + 78.6, x + 17.5, y + 77.9, x + 17.5, y + 77.3);
            ctx.bezierCurveTo(x + 17.5, y + 76.6, x + 17.5, y + 76, x + 17.4, y + 75.4);
            ctx.bezierCurveTo(x + 17.4, y + 74.7, x + 17.3, y + 74.2, x + 17.2, y + 73.9);
            ctx.bezierCurveTo(x + 17.2, y + 73.1, x + 17, y + 72.4, x + 16.7, y + 71.5);
            ctx.bezierCurveTo(x + 16.4, y + 70.7, x + 16.1, y + 70, x + 15.9, y + 69.3);
            ctx.bezierCurveTo(x + 15.9, y + 69.1, x + 15.6, y + 69, x + 15.2, y + 69.1);
            ctx.bezierCurveTo(x + 14.8, y + 69.2, x + 14.5, y + 69.2, x + 14.3, y + 69.3);
            ctx.bezierCurveTo(x + 12.5, y + 69.5, x + 11.1, y + 69.4, x + 9.9, y + 69);
            ctx.bezierCurveTo(x + 8, y + 68.5, x + 6.3, y + 67.6, x + 4.9, y + 66.3);
            ctx.bezierCurveTo(x + 3.5, y + 65, x + 2.4, y + 63.5, x + 1.6, y + 61.9);
            ctx.bezierCurveTo(x + 0.8, y + 60.2, x + 0.3, y + 58.4, x + 0.2, y + 56.4);
            ctx.bezierCurveTo(x + 0.1, y + 54.5, x + 0.3, y + 52.7, x + 1, y + 50.9);
            ctx.bezierCurveTo(x + 2.1, y + 48.6, x + 3.4, y + 46.5, x + 5, y + 44.6);
            ctx.bezierCurveTo(x + 6.5, y + 42.6, x + 8.2, y + 40.8, x + 10.1, y + 39.2);
            ctx.bezierCurveTo(x + 9.8, y + 37.8, x + 9.5, y + 36.5, x + 9.2, y + 35.1);
            ctx.bezierCurveTo(x + 8.9, y + 33.8, x + 8.8, y + 32.4, x + 8.8, y + 31);
            ctx.bezierCurveTo(x + 8.8, y + 30, x + 8.8, y + 28.9, x + 9, y + 27.7);
            ctx.bezierCurveTo(x + 9.1, y + 26.5, x + 9.4, y + 25.3, x + 9.8, y + 24.1);
            ctx.bezierCurveTo(x + 10.2, y + 23, x + 10.7, y + 21.9, x + 11.4, y + 21);
            ctx.bezierCurveTo(x + 12.1, y + 20.1, x + 13, y + 19.4, x + 14.1, y + 19);
            ctx.closePath();
            ctx.fillStyle = defaultColor;
            ctx.fill();
            ctx.strokeStyle = defaultColor;
            ctx.stroke();
            //ctx.restore();

            break;
        case enumClef.Treble:
            x = 2;
            y = -39 + staffOffset;
            //ctx.save();
            ctx.beginPath();
            ctx.moveTo(x + 21.2, y + 51.3);
            ctx.bezierCurveTo(x + 21.6, y + 51.3, x + 22, y + 51.5, x + 22.4, y + 52);
            ctx.bezierCurveTo(x + 22.6, y + 52.2, x + 22.7, y + 52.5, x + 22.8, y + 52.7);
            ctx.bezierCurveTo(x + 22.9, y + 53, x + 23, y + 53.3, x + 23, y + 53.6);
            ctx.bezierCurveTo(x + 23, y + 54.3, x + 22.8, y + 54.9, x + 22.5, y + 55.3);
            ctx.bezierCurveTo(x + 22.2, y + 55.7, x + 21.8, y + 55.9, x + 21.2, y + 55.9);
            ctx.bezierCurveTo(x + 20.7, y + 55.9, x + 20.3, y + 55.7, x + 19.9, y + 55.3);
            ctx.bezierCurveTo(x + 19.5, y + 54.8, x + 19.3, y + 54.3, x + 19.3, y + 53.6);
            ctx.bezierCurveTo(x + 19.3, y + 53.1, x + 19.5, y + 52.5, x + 19.9, y + 52);
            ctx.bezierCurveTo(x + 20.3, y + 51.5, x + 20.7, y + 51.3, x + 21.2, y + 51.3);
            ctx.moveTo(x + 21.2, y + 42.1);
            ctx.bezierCurveTo(x + 21.6, y + 42.1, x + 22, y + 42.3, x + 22.5, y + 42.8);
            ctx.bezierCurveTo(x + 22.8, y + 43.2, x + 23, y + 43.8, x + 23, y + 44.4);
            ctx.bezierCurveTo(x + 23, y + 45, x + 22.8, y + 45.6, x + 22.5, y + 46);
            ctx.bezierCurveTo(x + 22, y + 46.5, x + 21.6, y + 46.7, x + 21.2, y + 46.7);
            ctx.bezierCurveTo(x + 20.7, y + 46.7, x + 20.3, y + 46.5, x + 19.9, y + 46);
            ctx.bezierCurveTo(x + 19.5, y + 45.4, x + 19.3, y + 44.9, x + 19.3, y + 44.3);
            ctx.bezierCurveTo(x + 19.3, y + 43.7, x + 19.5, y + 43.2, x + 19.9, y + 42.8);
            ctx.bezierCurveTo(x + 20.3, y + 42.3, x + 20.7, y + 42.1, x + 21.2, y + 42.1);
            ctx.moveTo(x + 9.5, y + 38.9);
            ctx.bezierCurveTo(x + 10.9, y + 38.9, x + 12.3, y + 39.4, x + 13.6, y + 40.4);
            ctx.bezierCurveTo(x + 15, y + 41.4, x + 16, y + 42.7, x + 16.8, y + 44.3);
            ctx.bezierCurveTo(x + 17.6, y + 46, x + 18, y + 47.8, x + 18, y + 49.6);
            ctx.bezierCurveTo(x + 18, y + 53, x + 17.1, y + 56.2, x + 15.4, y + 59);
            ctx.bezierCurveTo(x + 15.1, y + 59.4, x + 14.9, y + 59.7, x + 14.6, y + 60.1);
            ctx.bezierCurveTo(x + 14.4, y + 60.4, x + 14.2, y + 60.7, x + 14, y + 61);
            ctx.bezierCurveTo(x + 13.7, y + 61.4, x + 13.5, y + 61.7, x + 13.2, y + 62);
            ctx.bezierCurveTo(x + 13, y + 62.4, x + 12.7, y + 62.7, x + 12.4, y + 63.1);
            ctx.bezierCurveTo(x + 11.9, y + 63.6, x + 11.3, y + 64.2, x + 10.7, y + 64.8);
            ctx.bezierCurveTo(x + 10.1, y + 65.4, x + 9.4, y + 66, x + 8.7, y + 66.6);
            ctx.bezierCurveTo(x + 8.4, y + 66.9, x + 7.9, y + 67.3, x + 7.3, y + 67.7);
            ctx.bezierCurveTo(x + 6.8, y + 68.1, x + 6.1, y + 68.6, x + 5.3, y + 69.1);
            ctx.lineTo(x + 0.2, y + 72.5);
            ctx.lineTo(x + 4.9, y + 67.3);
            ctx.bezierCurveTo(x + 5.3, y + 67, x + 5.6, y + 66.7, x + 5.8, y + 66.5);
            ctx.bezierCurveTo(x + 5.9, y + 66.4, x + 6, y + 66.3, x + 6.1, y + 66.2);
            ctx.bezierCurveTo(x + 6.3, y + 66, x + 6.4, y + 65.8, x + 6.6, y + 65.7);
            ctx.lineTo(x + 8.3, y + 63.6);
            ctx.bezierCurveTo(x + 8.8, y + 62.9, x + 9.4, y + 62.1, x + 10, y + 61.4);
            ctx.bezierCurveTo(x + 10.6, y + 60.6, x + 11.1, y + 59.8, x + 11.5, y + 58.9);
            ctx.bezierCurveTo(x + 11.8, y + 58.2, x + 12.1, y + 57.7, x + 12.3, y + 57.2);
            ctx.bezierCurveTo(x + 12.6, y + 56.6, x + 12.8, y + 55.9, x + 13.2, y + 55);
            ctx.lineTo(x + 13.6, y + 52.4);
            ctx.bezierCurveTo(x + 13.7, y + 51.9, x + 13.8, y + 51.5, x + 13.8, y + 51.1);
            ctx.bezierCurveTo(x + 13.8, y + 50.7, x + 13.9, y + 50.3, x + 13.9, y + 50);
            ctx.lineTo(x + 13.7, y + 47.8);
            ctx.bezierCurveTo(x + 13.7, y + 47.3, x + 13.7, y + 47, x + 13.6, y + 46.6);
            ctx.bezierCurveTo(x + 13.6, y + 46.2, x + 13.5, y + 45.9, x + 13.3, y + 45.7);
            ctx.bezierCurveTo(x + 13.2, y + 45, x + 12.9, y + 44.3, x + 12.5, y + 43.4);
            ctx.bezierCurveTo(x + 12.1, y + 42.6, x + 11.8, y + 41.9, x + 11.4, y + 41.5);
            ctx.bezierCurveTo(x + 10.6, y + 40.6, x + 9.6, y + 40.1, x + 8.4, y + 40.1);
            ctx.bezierCurveTo(x + 7.2, y + 40.1, x + 6.2, y + 40.4, x + 5.1, y + 40.9);
            ctx.bezierCurveTo(x + 4.1, y + 41.5, x + 3.4, y + 42.5, x + 2.9, y + 43.9);
            ctx.lineTo(x + 2.8, y + 44.4);
            ctx.bezierCurveTo(x + 2.8, y + 44.7, x + 2.9, y + 44.9, x + 3.1, y + 45);
            ctx.lineTo(x + 3.7, y + 45.3);
            ctx.lineTo(x + 4.4, y + 45.2);
            ctx.lineTo(x + 5.3, y + 45);
            ctx.bezierCurveTo(x + 6.1, y + 45, x + 6.8, y + 45.3, x + 7.3, y + 46);
            ctx.bezierCurveTo(x + 7.9, y + 46.6, x + 8.2, y + 47.4, x + 8.2, y + 48.3);
            ctx.bezierCurveTo(x + 8.2, y + 48.9, x + 8.1, y + 49.5, x + 7.8, y + 50.1);
            ctx.bezierCurveTo(x + 7.5, y + 50.7, x + 7.1, y + 51.2, x + 6.6, y + 51.6);
            ctx.bezierCurveTo(x + 6, y + 51.9, x + 5.4, y + 52.1, x + 4.8, y + 52.1);
            ctx.bezierCurveTo(x + 3.7, y + 52.1, x + 2.8, y + 51.7, x + 2, y + 50.8);
            ctx.bezierCurveTo(x + 1.2, y + 49.9, x + 0.8, y + 48.8, x + 0.8, y + 47.5);
            ctx.bezierCurveTo(x + 0.8, y + 46.1, x + 1.1, y + 44.9, x + 1.6, y + 43.9);
            ctx.bezierCurveTo(x + 2.1, y + 42.8, x + 2.8, y + 41.9, x + 3.6, y + 41.2);
            ctx.bezierCurveTo(x + 4.4, y + 40.4, x + 5.3, y + 39.9, x + 6.4, y + 39.5);
            ctx.bezierCurveTo(x + 7.4, y + 39.1, x + 8.4, y + 38.9, x + 9.5, y + 38.9);
            ctx.fillStyle = defaultColor;
            ctx.fill();
            ctx.strokeStyle = defaultColor;
            ctx.stroke();
            ctx.closePath();
            break;
    }
}
//#endregion

//renderTimeSignature
//#region

function renderTimeSignature(timeSignatureId, staffOffset) {
    var x, y;

    switch (timeSignatureId) {
        case enumTimeSignature.FourFour_2:
            x = timeSignatureOffset;
            y = -21 + staffOffset;
            //ctx.save();
            ctx.lineWidth = .01;
            ctx.beginPath();
            ctx.moveTo(x + 8.7, y + 27.1);
            ctx.lineTo(x + 9.6, y + 27.1);
            ctx.bezierCurveTo(x + 9.6, y + 27.1, x + 9.7, y + 27.1, x + 9.9, y + 27.1);
            ctx.bezierCurveTo(x + 10, y + 27.2, x + 10.2, y + 27.2, x + 10.4, y + 27.3);
            ctx.bezierCurveTo(x + 10.9, y + 27.4, x + 11.2, y + 27.5, x + 11.3, y + 27.6);
            ctx.bezierCurveTo(x + 11.5, y + 27.7, x + 11.7, y + 27.7, x + 12, y + 27.8);
            ctx.bezierCurveTo(x + 12.5, y + 28.1, x + 12.9, y + 28.4, x + 13.4, y + 28.7);
            ctx.bezierCurveTo(x + 13.8, y + 29, x + 14.2, y + 29.4, x + 14.6, y + 29.8);
            ctx.bezierCurveTo(x + 15.3, y + 30.6, x + 15.7, y + 31.6, x + 15.7, y + 32.7);
            ctx.bezierCurveTo(x + 15.7, y + 33.6, x + 15.4, y + 34.4, x + 14.8, y + 35.1);
            ctx.bezierCurveTo(x + 14, y + 35.8, x + 13.2, y + 36.1, x + 12.4, y + 36.1);
            ctx.bezierCurveTo(x + 10.8, y + 36.1, x + 9.8, y + 35.2, x + 9.5, y + 33.5);
            ctx.bezierCurveTo(x + 9.5, y + 32.7, x + 9.8, y + 32, x + 10.3, y + 31.5);
            ctx.bezierCurveTo(x + 10.8, y + 30.9, x + 11.4, y + 30.5, x + 12.1, y + 30.3);
            ctx.lineTo(x + 12.5, y + 30.3);
            ctx.lineTo(x + 12.9, y + 30.1);
            ctx.lineTo(x + 13, y + 29.9);
            ctx.bezierCurveTo(x + 12.9, y + 29.3, x + 12.4, y + 29, x + 11.7, y + 28.7);
            ctx.bezierCurveTo(x + 11.1, y + 28.4, x + 10.4, y + 28.2, x + 9.6, y + 28.2);
            ctx.bezierCurveTo(x + 8.8, y + 28.2, x + 8.1, y + 28.4, x + 7.5, y + 28.9);
            ctx.bezierCurveTo(x + 7.2, y + 29, x + 6.9, y + 29.3, x + 6.5, y + 29.6);
            ctx.bezierCurveTo(x + 6.3, y + 29.9, x + 6, y + 30.2, x + 5.8, y + 30.5);
            ctx.bezierCurveTo(x + 5.2, y + 31.7, x + 4.8, y + 33.6, x + 4.8, y + 36.3);
            ctx.bezierCurveTo(x + 4.8, y + 39.2, x + 5, y + 40.9, x + 5.2, y + 41.5);
            ctx.bezierCurveTo(x + 5.8, y + 43.8, x + 7.2, y + 45, x + 9.4, y + 45);
            ctx.bezierCurveTo(x + 11.1, y + 45, x + 12.6, y + 44.1, x + 13.7, y + 42.1);
            ctx.bezierCurveTo(x + 14.2, y + 41.1, x + 14.6, y + 39.9, x + 14.8, y + 38.7);
            ctx.lineTo(x + 15.9, y + 38.7);
            ctx.bezierCurveTo(x + 15.8, y + 39.9, x + 15.6, y + 40.9, x + 15.2, y + 41.8);
            ctx.bezierCurveTo(x + 14.8, y + 42.7, x + 14.3, y + 43.5, x + 13.6, y + 44.2);
            ctx.bezierCurveTo(x + 12.2, y + 45.6, x + 10.4, y + 46.4, x + 8.3, y + 46.4);
            ctx.bezierCurveTo(x + 6.6, y + 46.4, x + 5.1, y + 45.9, x + 3.9, y + 44.8);
            ctx.bezierCurveTo(x + 2.6, y + 43.9, x + 1.6, y + 42.6, x + 1, y + 41);
            ctx.bezierCurveTo(x + 0.7, y + 40.3, x + 0.5, y + 39.5, x + 0.3, y + 38.6);
            ctx.bezierCurveTo(x + 0.1, y + 37.7, x + 0, y + 36.9, x + 0, y + 36.2);
            ctx.bezierCurveTo(x + 0, y + 34.7, x + 0.4, y + 33.3, x + 1.2, y + 31.9);
            ctx.bezierCurveTo(x + 2, y + 30.5, x + 3, y + 29.4, x + 4.4, y + 28.4);
            ctx.bezierCurveTo(x + 5.8, y + 27.5, x + 7.2, y + 27.1, x + 8.7, y + 27.1);
            ctx.fillStyle = defaultColor;
            ctx.fill();
            ctx.strokeStyle = defaultColor;
            ctx.stroke();
            ctx.closePath();
            break;
        case enumTimeSignature.FourFour_1:
            x = timeSignatureOffset;
            y = -17 + staffOffset;

            //ctx.save();
            ctx.lineWidth = .01;
            ctx.beginPath();

            ctx.moveTo(x + 5.746, y + 37.109);
            ctx.lineTo(x + 2.074, y + 42.638);
            ctx.lineTo(x + 5.746, y + 42.638);
            ctx.moveTo(x + 6.631, y + 33.48);
            ctx.lineTo(x + 8.251, y + 33.48);
            ctx.lineTo(x + 8.251, y + 42.638);
            ctx.lineTo(x + 10.325, y + 42.638);
            ctx.lineTo(x + 10.325, y + 44.798);
            ctx.lineTo(x + 8.251, y + 44.798);
            ctx.lineTo(x + 8.251, y + 48.168);
            ctx.lineTo(x + 5.746, y + 48.168);
            ctx.lineTo(x + 5.746, y + 44.798);
            ctx.lineTo(x + 0, y + 44.798);
            ctx.lineTo(x + 0, y + 42.914);
            ctx.moveTo(x + 5.746, y + 20.736);
            ctx.lineTo(x + 2.074, y + 26.266);
            ctx.lineTo(x + 5.746, y + 26.266);
            ctx.moveTo(x + 6.631, y + 17.107);
            ctx.lineTo(x + 8.251, y + 17.107);
            ctx.lineTo(x + 8.251, y + 26.266);
            ctx.lineTo(x + 10.325, y + 26.266);
            ctx.lineTo(x + 10.325, y + 28.426);
            ctx.lineTo(x + 8.251, y + 28.426);
            ctx.lineTo(x + 8.251, y + 31.838);
            ctx.lineTo(x + 5.746, y + 31.838);
            ctx.lineTo(x + 5.746, y + 28.426);
            ctx.lineTo(x + 0, y + 28.426);
            ctx.lineTo(x + 0, y + 26.542);
            ctx.fillStyle = defaultColor;
            ctx.fill();
            ctx.strokeStyle = defaultColor;
            ctx.stroke();
            ctx.closePath();
            break;
        case enumTimeSignature.FourFour_3:
            x = 52;
            y = -22 + staffOffset;
            break;
        case enumTimeSignature.ThreeFour:
            x = 52;
            y = -22 + staffOffset;
            break;
        case enumTimeSignature.TwoFour:
            x = 52;
            y = -22 + staffOffset;
            break;
        case enumTimeSignature.TwoTwo:
            x = 52;
            y = -22 + staffOffset;
            break;
    }
}

//#endregion

//renderKeySignature
//#region

function renderKeySignature(keyId, staffOffset) {

    switch (keyId) {
        case enumKey.C:
            break;
        case enumKey.G:
            renderAccidental(31, staffOffset - 48, sharpVectorId);
            timeSignatureOffset = 47;
            break;
        case enumKey.D:
            renderAccidental(31, staffOffset - 48, sharpVectorId);
            renderAccidental(39, staffOffset - 35, sharpVectorId);
            timeSignatureOffset = 53;
            break;
        case enumKey.A:
            renderAccidental(31, staffOffset - 48, sharpVectorId);
            renderAccidental(39, staffOffset - 35, sharpVectorId);
            renderAccidental(47, staffOffset - 52, sharpVectorId);
            timeSignatureOffset = 62;
            break;
        case enumKey.E:
            renderAccidental(31, staffOffset - 48, sharpVectorId);
            renderAccidental(39, staffOffset - 35, sharpVectorId);
            renderAccidental(47, staffOffset - 52, sharpVectorId);
            renderAccidental(55, staffOffset - 40, sharpVectorId);
            timeSignatureOffset = 71;
            break;
        case enumKey.B:
            renderAccidental(31, staffOffset - 48, sharpVectorId);
            renderAccidental(39, staffOffset - 35, sharpVectorId);
            renderAccidental(47, staffOffset - 52, sharpVectorId);
            renderAccidental(55, staffOffset - 40, sharpVectorId);
            renderAccidental(63, staffOffset - 27, sharpVectorId);
            timeSignatureOffset = 79;
            break;
        case enumKey.Db:
            renderAccidental(31, staffOffset - 32, flatVectorId);
            renderAccidental(39, staffOffset - 44, flatVectorId);
            renderAccidental(47, staffOffset - 27, flatVectorId);
            renderAccidental(55, staffOffset - 40, flatVectorId);
            renderAccidental(63, staffOffset - 24, flatVectorId);
            timeSignatureOffset = 75;
            break;
        case enumKey.Ab:
            renderAccidental(31, staffOffset - 32, flatVectorId);
            renderAccidental(39, staffOffset - 44, flatVectorId);
            renderAccidental(47, staffOffset - 27, flatVectorId);
            renderAccidental(55, staffOffset - 40, flatVectorId);
            timeSignatureOffset = 68;
            break;
        case enumKey.Gb:
            renderAccidental(31, staffOffset - 32, flatVectorId);
            renderAccidental(39, staffOffset - 44, flatVectorId);
            renderAccidental(47, staffOffset - 27, flatVectorId);
            renderAccidental(55, staffOffset - 40, flatVectorId);
            renderAccidental(63, staffOffset - 24, flatVectorId);
            renderAccidental(71, staffOffset - 36, flatVectorId);
            timeSignatureOffset = 82;
            break;
        case enumKey.Eb:
            renderAccidental(31, staffOffset - 32, flatVectorId);
            renderAccidental(39, staffOffset - 44, flatVectorId);
            renderAccidental(47, staffOffset - 27, flatVectorId);
            timeSignatureOffset = 58;
            break;
        case enumKey.Bb:
            renderAccidental(31, staffOffset - 32, flatVectorId);
            renderAccidental(39, staffOffset - 44, flatVectorId);
            timeSignatureOffset = 50;
            break;
        case enumKey.F:
            renderAccidental(31, staffOffset - 48, sharpVectorId);
            timeSignatureOffset = 43;
            break;
        case enumKey.Fs:
            renderAccidental(31, staffOffset - 48, sharpVectorId);
            renderAccidental(39, staffOffset - 35, sharpVectorId);
            renderAccidental(47, staffOffset - 52, sharpVectorId);
            renderAccidental(55, staffOffset - 40, sharpVectorId);
            renderAccidental(63, staffOffset - 27, sharpVectorId);
            renderAccidental(71, staffOffset - 44, sharpVectorId);
            timeSignatureOffset = 86;
            break;
    }
}
//#endregion

//renderAccidental
//#region
function renderAccidental(x, y, accidentalId) {

    switch (accidentalId) {
        case enumAccidental.Sharp:
            ctx.beginPath();
            ctx.lineWidth = .01;
            ctx.moveTo(x + 6, y + 46);
            ctx.lineTo(x + 3, y + 46);
            ctx.lineTo(x + 3, y + 50);
            ctx.lineTo(x + 6, y + 50);
            ctx.moveTo(x + 6, y + 38);
            ctx.lineTo(x + 7, y + 38);
            ctx.lineTo(x + 7, y + 43);
            ctx.lineTo(x + 9, y + 43);
            ctx.lineTo(x + 9, y + 46);
            ctx.lineTo(x + 7, y + 46);
            ctx.lineTo(x + 7, y + 50);
            ctx.lineTo(x + 9, y + 49);
            ctx.lineTo(x + 9, y + 52);
            ctx.lineTo(x + 7, y + 52);
            ctx.lineTo(x + 7, y + 57);
            ctx.lineTo(x + 6, y + 57);
            ctx.lineTo(x + 6, y + 53);
            ctx.lineTo(x + 3, y + 53);
            ctx.lineTo(x + 3, y + 58);
            ctx.lineTo(x + 2, y + 58);
            ctx.lineTo(x + 2, y + 53);
            ctx.lineTo(x + 0, y + 54);
            ctx.lineTo(x + 0, y + 51);
            ctx.lineTo(x + 2, y + 51);
            ctx.lineTo(x + 2, y + 47);
            ctx.lineTo(x + 0, y + 47);
            ctx.lineTo(x + 0, y + 44);
            ctx.lineTo(x + 2, y + 44);
            ctx.lineTo(x + 2, y + 40);
            ctx.lineTo(x + 3, y + 40);
            ctx.lineTo(x + 3, y + 44);
            ctx.lineTo(x + 6, y + 43);
            ctx.closePath();
            break;
        case enumAccidental.Flat:
            ctx.beginPath();
            ctx.lineWidth = .01;
            ctx.moveTo(x + 3.072, y + 45.072);
            ctx.bezierCurveTo(x + 2.176, y + 45.072, x + 1.488, y + 45.552, x + 1.008, y + 46.512);
            ctx.lineTo(x + 1.008, y + 51.504);
            ctx.lineTo(x + 2.448, y + 50.064);
            ctx.bezierCurveTo(x + 2.704, y + 49.808, x + 2.936, y + 49.584, x + 3.144, y + 49.392);
            ctx.bezierCurveTo(x + 3.352, y + 49.2, x + 3.52, y + 49.008, x + 3.648, y + 48.816);
            ctx.bezierCurveTo(x + 4.288, y + 48.08, x + 4.608, y + 47.344, x + 4.608, y + 46.608);
            ctx.bezierCurveTo(x + 4.608, y + 46.352, x + 4.56, y + 46.136, x + 4.464, y + 45.96);
            ctx.bezierCurveTo(x + 4.368, y + 45.784, x + 4.256, y + 45.616, x + 4.128, y + 45.456);
            ctx.bezierCurveTo(x + 4, y + 45.36, x + 3.856, y + 45.272, x + 3.696, y + 45.192);
            ctx.bezierCurveTo(x + 3.536, y + 45.112, x + 3.328, y + 45.072, x + 3.072, y + 45.072);
            ctx.moveTo(x + 0, y + 33.36);
            ctx.lineTo(x + 1.008, y + 33.36);
            ctx.lineTo(x + 1.008, y + 45.072);
            ctx.bezierCurveTo(x + 2, y + 44.368, x + 3.088, y + 44.016, x + 4.272, y + 44.016);
            ctx.bezierCurveTo(x + 5.072, y + 44.016, x + 5.792, y + 44.24, x + 6.432, y + 44.688);
            ctx.bezierCurveTo(x + 7.072, y + 45.136, x + 7.392, y + 45.744, x + 7.392, y + 46.512);
            ctx.bezierCurveTo(x + 7.392, y + 47.056, x + 7.152, y + 47.568, x + 6.672, y + 48.048);
            ctx.bezierCurveTo(x + 6.48, y + 48.336, x + 6.208, y + 48.624, x + 5.856, y + 48.912);
            ctx.bezierCurveTo(x + 5.504, y + 49.2, x + 5.088, y + 49.52, x + 4.608, y + 49.872);
            ctx.lineTo(x + 2.112, y + 51.696);
            ctx.bezierCurveTo(x + 1.664, y + 51.984, x + 1.269, y + 52.288, x + 0.926, y + 52.608);
            ctx.bezierCurveTo(x + 0.584, y + 52.928, x + 0.275, y + 53.232, x + 0, y + 53.52);
            ctx.closePath();
            break;
        case enumAccidental.Natural:

            break;
    }
}
//#endregion

//renderChords
//#region
function renderChords(measure) {
    measure.Chords.sort(function (a, b) { return a.StartTime - b.StartTime });
    for (var n = 0; n < measure.Chords.length; n++) {
        var chord = measure.Chords[n];
        chord.Notes.sort(function (a, b) { return a.Duration - b.Duration });

        for (var m = 0; m < chord.Notes.length; m++) {
            var note = chord.Notes[m];
            if (note.Pitch.trim() != "R") {
                renderNote(measure, chord, note);
            }
            else {
                renderRest(measure, chord, note);
            }
        }
    }
}
//#endregion

//renderNote
//#region

function renderNote(measure, chord, note) {
    var measure_x = measure.X;
    var measure_y = measure.Y;

    x = measure_x + chord.Location_X - 55;
    y = measure_y + note.Location_Y - 41;

    //ctx.fillStyle = colors[randomColorIndex++ % colors.length];

    ctx.lineWidth = .01;
    //ctx.save();

    switch (note.Vector_Id) {
        case enumNote.Whole:
            break;
        case enumNote.Half:
            renderHollowBody(x, y);
            renderStem(x, y, note.Orientation);
            break;
        case enumNote.Quarter:
            renderSolidBody(x, y);
            renderStem(x, y, note.Orientation);
            break;
        case enumNote.Eighth:
            renderSolidBody(x, y);
            renderStem(x, y, note.Orientation);
            renderFlag(x, y, note.Orientation);
            break;
        case enumNote.Sixteenth:
            renderSolidBody(x, y);
            renderStem(x, y, note.Orientation);
            renderFlag(x, y, note.Orientation);

            if (note.Orientation == enumOrientation.Down) {
                renderFlag(x, y - flagSpacing, note.Orientation);
            }
            else {
                renderFlag(x, y + flagSpacing, note.Orientation);
            }

            break;
        case enumNote.Thirtysecond:
            renderSolidBody(x, y);
            renderStem(x, y, note.Orientation);
            renderFlag(x, y, note.Orientation);

            if (note.Orientation == enumOrientation.Down) {
                renderFlag(x, y - flagSpacing, note.Orientation);
                renderFlag(x, y - flagSpacing * 2, note.Orientation);
            }
            else {
                renderFlag(x, y + flagSpacing, note.Orientation);
                renderFlag(x, y + flagSpacing * 2, note.Orientation);
            }

            break;
    }

    if (note.IsDotted) {
        renderDot(x, y);
    }
    if (note.Pitch.length == 3) {
        if (note.IsDotted) {
            x = x + 3;
        }
        switch (note.Pitch.charAt(2)) {
            case 's':
                renderAccidental(x + 13, y, 25)
                break;
            case 'f':
                renderAccidental(x + 13, y, 26)
                break;
            case 'n':
                renderAccidental(x + 13, y, 27)
                break;
        }
    }

}

function renderDot(x, y) {
    y = y + 24;
    x = x - 1;
    ctx.beginPath();
    ctx.moveTo(x + 13, y + 23);
    ctx.lineTo(x + 16, y + 23);
    ctx.lineTo(x + 16, y + 26);
    ctx.lineTo(x + 13, y + 26);
    ctx.fillStyle = defaultColor;
    ctx.fill();
    ctx.strokeStyle = defaultColor;
    ctx.stroke();
    ctx.closePath();
}

//#endregion

//renderStem
//#region
function renderStem(x, y, orientation) {
    switch (parseInt(orientation)) {
        case enumOrientation.Down:
            ctx.beginPath();
            x = x - 9;
            y = y + 102;
            ctx.moveTo(x + 9.168, y - 19.92);
            ctx.lineTo(x + 9.984, y - 19.92);
            ctx.lineTo(x + 9.984, y - 48.048);
            ctx.bezierCurveTo(x + 9.984, y - 49.04, x + 9.632, y - 49.984, x + 8.928, y - 50.88);
            ctx.lineTo(x + 9.168, y - 28.704);
            ctx.fillStyle = defaultColor;
            ctx.fill();
            ctx.strokeStyle = defaultColor;
            ctx.stroke();
            ctx.closePath();
            break;
        case enumOrientation.Up:
            ctx.beginPath();
            ctx.moveTo(x + 9.168, y + 19.92);
            ctx.lineTo(x + 9.984, y + 19.92);
            ctx.lineTo(x + 9.984, y + 48.048);
            ctx.bezierCurveTo(x + 9.984, y + 49.04, x + 9.632, y + 49.984, x + 8.928, y + 50.88);
            ctx.lineTo(x + 9.168, y + 28.704);
            ctx.fillStyle = defaultColor;
            ctx.fill();
            ctx.strokeStyle = defaultColor;
            ctx.stroke();
            ctx.closePath();
            break;
    }
}
//#endregion

//renderSolidBody
//#region
function renderSolidBody(x, y) {
    ctx.beginPath();
    ctx.lineTo(x + 9.984, y + 48.048);
    ctx.bezierCurveTo(x + 9.984, y + 49.04, x + 9.632, y + 49.984, x + 8.928, y + 50.88);
    ctx.bezierCurveTo(x + 8.512, y + 51.584, x + 7.712, y + 52.208, x + 6.528, y + 52.752);
    ctx.bezierCurveTo(x + 5.536, y + 53.264, x + 4.496, y + 53.52, x + 3.408, y + 53.52);
    ctx.bezierCurveTo(x + 2.416, y + 53.52, x + 1.616, y + 53.264, x + 1.008, y + 52.752);
    ctx.bezierCurveTo(x + 0.336, y + 52.272, x + 0, y + 51.552, x + 0, y + 50.592);
    ctx.bezierCurveTo(x + 0, y + 50.112, x + 0.072, y + 49.64, x + 0.216, y + 49.176);
    ctx.bezierCurveTo(x + 0.36, y + 48.712, x + 0.608, y + 48.272, x + 0.96, y + 47.856);
    ctx.bezierCurveTo(x + 1.28, y + 47.472, x + 1.64, y + 47.104, x + 2.04, y + 46.752);
    ctx.bezierCurveTo(x + 2.44, y + 46.4, x + 2.912, y + 46.096, x + 3.456, y + 45.84);
    ctx.bezierCurveTo(x + 4.512, y + 45.36, x + 5.472, y + 45.12, x + 6.336, y + 45.12);
    ctx.bezierCurveTo(x + 7.712, y + 45.12, x + 8.656, y + 45.376, x + 9.168, y + 45.888);
    ctx.fillStyle = defaultColor;
    ctx.fill();
    ctx.strokeStyle = defaultColor;
    ctx.stroke();
    ctx.closePath();
}
//#endregion

//renderHollowBody
//#region
function renderHollowBody(x, y) {
    ctx.beginPath();
    ctx.moveTo(x + 8.11, y + 45.89);

    ctx.bezierCurveTo(x + 6.9, y + 45.89, x + 5.22, y + 46.7, x + 3.07, y + 48.34);
    ctx.bezierCurveTo(x + 2.75, y + 48.56, x + 2.3, y + 49.01, x + 1.73, y + 49.68);
    ctx.bezierCurveTo(x + 1.15, y + 50.42, x + 0.86, y + 50.96, x + 0.86, y + 51.31);
    ctx.bezierCurveTo(x + 0.96, y + 51.7, x + 1.26, y + 51.98, x + 1.78, y + 52.18);
    ctx.bezierCurveTo(x + 2.38, y + 52.18, x + 2.99, y + 52.02, x + 3.6, y + 51.7);
    ctx.bezierCurveTo(x + 4.3, y + 51.47, x + 5.09, y + 51.01, x + 5.95, y + 50.3);
    ctx.bezierCurveTo(x + 6.21, y + 50.18, x + 6.48, y + 49.98, x + 6.77, y + 49.7);
    ctx.bezierCurveTo(x + 7.06, y + 49.43, x + 7.38, y + 49.09, x + 7.73, y + 48.67);

    ctx.bezierCurveTo(x + 8.46, y + 47.84, x + 8.83, y + 47.18, x + 8.83, y + 46.7);
    ctx.bezierCurveTo(x + 8.83, y + 46.32, x + 8.59, y + 46.05, x + 8.11, y + 45.89);
    ctx.lineTo(x + 9.84, y + 48.24);

    ctx.bezierCurveTo(x + 9.84, y + 48.62, x + 9.82, y + 48.85, x + 9.79, y + 48.91);
    ctx.bezierCurveTo(x + 9.76, y + 48.98, x + 9.68, y + 49.17, x + 9.55, y + 49.49);
    ctx.bezierCurveTo(x + 9.49, y + 49.68, x + 9.38, y + 49.88, x + 9.24, y + 50.09);
    ctx.bezierCurveTo(x + 9.1, y + 50.3, x + 8.94, y + 50.53, x + 8.78, y + 50.78);
    ctx.bezierCurveTo(x + 8.08, y + 51.58, x + 7.26, y + 52.22, x + 6.34, y + 52.7);
    ctx.bezierCurveTo(x + 5.31, y + 53.25, x + 4.4, y + 53.52, x + 3.6, y + 53.52);
    ctx.bezierCurveTo(x + 2.74, y + 53.52, x + 2.06, y + 53.42, x + 1.58, y + 53.23);
    ctx.bezierCurveTo(x + 1.01, y + 53.04, x + 0.61, y + 52.66, x + 0.38, y + 52.08);
    ctx.lineTo(x + 0.1, y + 51.12);
    ctx.bezierCurveTo(x + 0.06, y + 50.96, x + 0.04, y + 50.77, x + 0.02, y + 50.54);
    ctx.bezierCurveTo(x + 0.01, y + 50.32, x + 0, y + 50.08, x + 0, y + 49.82);
    ctx.bezierCurveTo(x + 0, y + 48.9, x + 0.34, y + 48.02, x + 1.01, y + 47.18);
    ctx.bezierCurveTo(x + 1.68, y + 46.42, x + 2.53, y + 45.79, x + 3.55, y + 45.31);
    ctx.bezierCurveTo(x + 4.58, y + 44.83, x + 5.6, y + 44.59, x + 6.62, y + 44.59);
    ctx.bezierCurveTo(x + 7.49, y + 44.59, x + 8.29, y + 44.83, x + 9.02, y + 45.31);
    ctx.fillStyle = defaultColor;
    ctx.fill();
    ctx.strokeStyle = defaultColor;
    ctx.stroke();
    ctx.closePath();
}
//#endregion

//renderFlag
//#region
function renderFlag(x, y, orientation) {
    x = x - 1;
    y = y - 2;
    switch (parseInt(orientation)) {
        case enumOrientation.Down:
            x = x - 9;
            y = y + 106;
            ctx.beginPath();
            ctx.bezierCurveTo(x + 10.048, y - 20.56, x + 10.152, y - 21.144, x + 10.296, y - 21.672);
            ctx.bezierCurveTo(x + 10.44, y - 22.2, x + 10.592, y - 22.688, x + 10.752, y - 23.136);
            ctx.bezierCurveTo(x + 10.976, y - 23.936, x + 11.328, y - 24.672, x + 11.808, y - 25.344);
            ctx.lineTo(x + 12.624, y - 26.352);
            ctx.lineTo(x + 13.776, y - 27.744);
            ctx.lineTo(x + 15.888, y - 30.192);
            ctx.bezierCurveTo(x + 17.488, y - 32.272, x + 18.288, y - 34.448, x + 18.288, y - 36.72);
            ctx.bezierCurveTo(x + 18.288, y - 37.264, x + 18.24, y - 37.864, x + 18.144, y - 38.52);
            ctx.bezierCurveTo(x + 18.048, y - 39.176, x + 17.856, y - 39.856, x + 17.568, y - 40.56);
            ctx.bezierCurveTo(x + 17.344, y - 41.104, x + 17.16, y - 41.56, x + 17.016, y - 41.928);
            ctx.bezierCurveTo(x + 16.872, y - 42.296, x + 16.672, y - 42.704, x + 16.416, y - 43.152);
            ctx.lineTo(x + 15.696, y - 43.2);
            ctx.lineTo(x + 16.512, y - 41.088);
            ctx.bezierCurveTo(x + 16.608, y - 40.8, x + 16.704, y - 40.512, x + 16.8, y - 40.224);
            ctx.bezierCurveTo(x + 16.896, y - 39.936, x + 16.976, y - 39.616, x + 17.04, y - 39.264);
            ctx.lineTo(x + 17.184, y - 37.968);
            ctx.lineTo(x + 17.232, y - 37.392);
            ctx.lineTo(x + 17.232, y - 36.432);
            ctx.bezierCurveTo(x + 17.168, y - 36.304, x + 17.128, y - 36.176, x + 17.112, y - 36.048);
            ctx.bezierCurveTo(x + 17.096, y - 35.92, x + 17.072, y - 35.792, x + 17.04, y - 35.664);
            ctx.bezierCurveTo(x + 17.04, y - 35.664, x + 16.912, y - 35.28, x + 16.656, y - 34.512);
            ctx.bezierCurveTo(x + 16.336, y - 33.52, x + 15.792, y - 32.624, x + 15.024, y - 31.824);
            ctx.bezierCurveTo(x + 14.32, y - 30.928, x + 13.552, y - 30.272, x + 12.72, y - 29.856);
            ctx.bezierCurveTo(x + 12.272, y - 29.6, x + 11.8, y - 29.392, x + 11.304, y - 29.232);
            ctx.bezierCurveTo(x + 10.808, y - 29.072, x + 10.368, y - 28.992, x + 9.984, y - 28.992);
            ctx.fillStyle = defaultColor;
            ctx.fill();
            ctx.strokeStyle = defaultColor;
            ctx.stroke();
            ctx.closePath();
            break;
        case enumOrientation.Up:
            ctx.beginPath();
            ctx.bezierCurveTo(x + 10.048, y + 20.56, x + 10.152, y + 21.144, x + 10.296, y + 21.672);
            ctx.bezierCurveTo(x + 10.44, y + 22.2, x + 10.592, y + 22.688, x + 10.752, y + 23.136);
            ctx.bezierCurveTo(x + 10.976, y + 23.936, x + 11.328, y + 24.672, x + 11.808, y + 25.344);
            ctx.lineTo(x + 12.624, y + 26.352);
            ctx.lineTo(x + 13.776, y + 27.744);
            ctx.lineTo(x + 15.888, y + 30.192);
            ctx.bezierCurveTo(x + 17.488, y + 32.272, x + 18.288, y + 34.448, x + 18.288, y + 36.72);
            ctx.bezierCurveTo(x + 18.288, y + 37.264, x + 18.24, y + 37.864, x + 18.144, y + 38.52);
            ctx.bezierCurveTo(x + 18.048, y + 39.176, x + 17.856, y + 39.856, x + 17.568, y + 40.56);
            ctx.bezierCurveTo(x + 17.344, y + 41.104, x + 17.16, y + 41.56, x + 17.016, y + 41.928);
            ctx.bezierCurveTo(x + 16.872, y + 42.296, x + 16.672, y + 42.704, x + 16.416, y + 43.152);
            ctx.lineTo(x + 15.696, y + 43.2);
            ctx.lineTo(x + 16.512, y + 41.088);
            ctx.bezierCurveTo(x + 16.608, y + 40.8, x + 16.704, y + 40.512, x + 16.8, y + 40.224);
            ctx.bezierCurveTo(x + 16.896, y + 39.936, x + 16.976, y + 39.616, x + 17.04, y + 39.264);
            ctx.lineTo(x + 17.184, y + 37.968);
            ctx.lineTo(x + 17.232, y + 37.392);
            ctx.lineTo(x + 17.232, y + 36.432);
            ctx.bezierCurveTo(x + 17.168, y + 36.304, x + 17.128, y + 36.176, x + 17.112, y + 36.048);
            ctx.bezierCurveTo(x + 17.096, y + 35.92, x + 17.072, y + 35.792, x + 17.04, y + 35.664);
            ctx.bezierCurveTo(x + 17.04, y + 35.664, x + 16.912, y + 35.28, x + 16.656, y + 34.512);
            ctx.bezierCurveTo(x + 16.336, y + 33.52, x + 15.792, y + 32.624, x + 15.024, y + 31.824);
            ctx.bezierCurveTo(x + 14.32, y + 30.928, x + 13.552, y + 30.272, x + 12.72, y + 29.856);
            ctx.bezierCurveTo(x + 12.272, y + 29.6, x + 11.8, y + 29.392, x + 11.304, y + 29.232);
            ctx.bezierCurveTo(x + 10.808, y + 29.072, x + 10.368, y + 28.992, x + 9.984, y + 28.992);
            ctx.fillStyle = defaultColor;
            ctx.fill();
            ctx.strokeStyle = defaultColor;
            ctx.stroke();
            ctx.closePath();
            break;
    }

}
//#endregion

//renderLedgerLine
//#region
function renderLedgerLine() {
    //    ctx.beginPath();
    //    ctx.fill();
    //    ctx.stroke();
    //    ctx.closePath();
}
//#endregion

//renderRest
//#region
function renderRest(measure, chord, note) {
    var measure_x = measure.X;
    var measure_y = measure.Y;

    x = measure_x + chord.Location_X;
    y = measure_y + note.Location_Y;

    //ctx.save();
    ctx.beginPath();
    switch (note.Vector_Id) {
        case enumRest.Whole:
            ctx.moveTo(x + 4.848, y + 39.6);
            ctx.lineTo(x + 17.616, y + 39.6);
            ctx.lineTo(x + 17.616, y + 43.488);
            ctx.lineTo(x + 22.272, y + 43.488);
            ctx.lineTo(x + 22.272, y + 45.408);
            ctx.lineTo(x + 0, y + 45.408);
            ctx.lineTo(x + 0, y + 43.488);
            ctx.lineTo(x + 4.848, y + 43.488);
            break;
        case enumRest.Half:
            break;
        case enumRest.Quarter:
            break;
        case enumRest.Eighth:
            break;
        case enumRest.Sixteenth:
            break;
        case enumRest.Thirtysecond:
            break;
    }
    ctx.fillStyle = defaultColor;
    ctx.fill();
    ctx.strokeStyle = defaultColor;
    ctx.stroke();
    ctx.closePath();
}

//#endregion

//renderSpans
//#region
function renderSpans(measure) {
    for (var i = 0; i < measureSpans.length; i++) {
        if (i == measure.Index) {
            var spannedNotes = measureSpans[i];
            for (var j = 0; j < spannedNotes.length; j = j + 2) {
                renderSpan(spannedNotes[j], spannedNotes[j + 1], measure.X, measure.Y);
            }
        }
    }
}
//#endregion

//renderSpans
//#region
function renderSpan(a, b, measure_x, measure_y) {
    if (b == null) {
        return;
    }
    var spanWidth = 5;
    var lineWidth = 3;
    var step = 0;
    var x, y;
    var d;
    var dur1 = a.Duration;
    var dur2 = b.Duration;

    if (a.Orientation == 0) {
        x = measure_x + a.Location_X + 10;
        y = measure_y + a.Location_Y - 22;
    }
    else {
        x = measure_x + a.Location_X + 1;
        y = measure_y + a.Location_Y + 40;
    }
    x = x - 55;
    var dX = b.Location_X - a.Location_X - 1;
    var dY = (a.Location_Y - b.Location_Y);

    var aa = (spanWidth * step);
    var ab = (spanWidth * step - lineWidth);
    var ac = dX;
    var ad = (dY + spanWidth * step);
    var ae = (dY - spanWidth * step - lineWidth);
    var af = 0;

    ctx.beginPath();
    //top span
    ctx.moveTo(x + af, y - aa);
    ctx.lineTo(x + af, y - ab);
    ctx.lineTo(x + ac, y - ae);
    ctx.lineTo(x + ac, y - ad);
    ctx.lineTo(x + af, y - aa);
    ctx.fillStyle = defaultColor;
    ctx.fill();
    ctx.strokeStyle = defaultColor;
    ctx.stroke();

    if (dur1 * 1 <= .25 && dur2 * 1 <= .25) { //middle span
        step = 1;

        switch (a.Orientation) {
            case 0:
                aa = spanWidth * step;
                ab = (spanWidth * step) - lineWidth;
                ac = dX;
                ad = dY + (spanWidth * step);
                ae = (dY + spanWidth * step - lineWidth);
                af = 0;
                d = 10;
                break;
            case 1:
                aa = -spanWidth * step;
                ab = -(spanWidth * step + lineWidth);
                ac = dX;
                ad = (dY - spanWidth * step);
                ae = (dY - spanWidth * step - lineWidth);
                af = 0;
                d = -10;
                break;
        }
        ctx.moveTo(x + af, y + d - aa);
        ctx.lineTo(x + af, y + d - ab);
        ctx.lineTo(x + ac, y + d - ae);
        ctx.lineTo(x + ac, y + d - ad);
        ctx.lineTo(x + af, y + d - aa);
        ctx.fillStyle = defaultColor;
        ctx.fill();
        ctx.strokeStyle = defaultColor;
        ctx.stroke();
    }
    ctx.closePath();

    if (dur1 * 1 + dur2 * 1 == .25) { //bottom span
        step = 2;
        switch (a.Orientation) {
            case 0:
                aa = spanWidth * step;
                ab = (spanWidth * step) - lineWidth;
                ac = dX;
                ad = dY + (spanWidth * step);
                ae = (dY + spanWidth * step - lineWidth);
                af = 0;
                d = 20;
                break;
            case 1:
                aa = -spanWidth * step;
                ab = -(spanWidth * step + lineWidth);
                ac = dX;
                ad = (dY - spanWidth * step);
                ae = (dY - spanWidth * step - lineWidth);
                af = 0;
                d = -20;
                break;
        }
        ctx.moveTo(x + af, y + d - aa);
        ctx.lineTo(x + af, y + d - ab);
        ctx.lineTo(x + ac, y + d - ae);
        ctx.lineTo(x + ac, y + d - ad);
        ctx.lineTo(x + af, y + d - aa);
        ctx.fillStyle = defaultColor;
        ctx.fill();
        ctx.strokeStyle = defaultColor;
        ctx.stroke();
    }
    step = dY / 2;

    //here we handle partial spans
    if (dur1 * 1 == .5 && dur2 * 1 == .25) {
        if (a.Orientation == 1) {
            spanWidth = -spanWidth;
        }
        if (a.Location_Y >= b.Location_Y) {
            aa = spanWidth - step + 6;
            ab = spanWidth - step - lineWidth + 6;
            ac = dX;
            ad = dY + spanWidth;
            ae = dY + spanWidth - lineWidth;
            af = dX / 2;
            d = 10;
        }
        else {
            aa = spanWidth + step;
            ab = spanWidth + step - lineWidth;
            ac = dX;
            ad = dY + spanWidth;
            ae = dY + spanWidth - lineWidth;
            af = dX / 2;
            d = -10;
        }
        //for debugging
        var result = "M " + (af).toString() + " " + (aa);
        result += " L " + (af).toString() + " " + (ab);
        result += " L " + (ac).toString() + " " + (ae);
        result += " L " + (ac).toString() + " " + (ad);
        result += " L " + (af).toString() + " " + (aa);
        result += " Z ";

        ctx.moveTo(x + af, y + d - aa);
        ctx.lineTo(x + af, y + d - ab);
        ctx.lineTo(x + ac, y + d - ae);
        ctx.lineTo(x + ac, y + d - ad);
        ctx.lineTo(x + af, y + d - aa);
        ctx.fillStyle = defaultColor;
        ctx.fill();
        ctx.strokeStyle = defaultColor;
        ctx.stroke();
    }
    else if (dur1 * 1 == .25 && dur2 * 1 == .5) {
        if (a.Location_Y >= b.Location_Y) {
            aa = spanWidth;
            ab = spanWidth - lineWidth;
            ac = dX / 2;
            ad = dY + spanWidth + step;
            ae = dY + spanWidth + step - lineWidth;
            af = 0;
            d = 10;
        }
        else {
            aa = spanWidth;
            ab = spanWidth - lineWidth;
            ac = dX / 2;
            ad = dY + spanWidth - step;
            ae = dY + spanWidth - step - lineWidth;
            af = 0;
            d = 0;
        }
        ctx.moveTo(x + af, y + d - aa);
        ctx.lineTo(x + af, y + d - ab);
        ctx.lineTo(x + ac, y + d - ae);
        ctx.lineTo(x + ac, y + d - ad);
        ctx.lineTo(x + af, y + d - aa);
        ctx.fillStyle = defaultColor;
        ctx.fill();
        ctx.strokeStyle = defaultColor;
        ctx.stroke();
    }
}
//#endregion

//renderArcs
//#region
var arcDepthAdjustment = .75;
var arcLeftAdjustment = 15;
var arcTopAdjustment = 50;

function renderArcs() {

    for (var i = 0; i < arcs.length; i++) {
        renderArc(arcs[i]);
    }
}

function renderArc(arc) {

    ctx.beginPath();

    var note1 = GetNoteFromId(arc.Note_Id1);
    var note2 = GetNoteFromId(arc.Note_Id2);
    var chord1 = GetChordFromId(note1.Chord_Id);
    var chord2 = GetChordFromId(note2.Chord_Id);
    var measure1 = GetMeasureFromId(chord1.Measure_Id);
    var measure2 = GetMeasureFromId(chord2.Measure_Id);

    var staff = getStaffFromId(measure1.Staff_Id);

    if (measure1.X > measure2.X) {
        var temp = measure1;
        measure1 = measure2;
        measure2 = temp;

        temp = chord1;
        chord1 = chord2;
        chord2 = temp;

        temp = note1;
        note1 = note2;
        note2 = temp;
    }

    //level points
    var x1 = arc.Left - arcLeftAdjustment;
    var y1 = arc.Top + arcTopAdjustment + ((staff.Sequence / sequenceIncrement) * (verseCount * verseHeight));
    var x2 = arc.X2 + arc.Left - arcLeftAdjustment;
    var y2 = y1;

    var cy;
    var c = getControlY(x1, x2);

    if (arc.ArcSweep == "Clockwise") {
        cy = y1 - c;
    }
    else {
        cy = y1 + c;
    }

    var cx = x1 + ((x2 - x1) / 2)

    var a = arc.Angle * Math.PI/180;

    //rotated points
    var rx1 = Math.cos(a) * (x1 - cx) - Math.sin(a) * (y1 - cy) + cx
    var ry1 = Math.sin(a) * (x1 - cx) + Math.cos(a) * (y1 - cy) + cy

    var rx2 = Math.cos(a) * (x2 - cx) - Math.sin(a) * (y2 - cy) + cx
    var ry2 = Math.sin(a) * (x2 - cx) + Math.cos(a) * (y2 - cy) + cy

    ctx.moveTo(rx1, ry1);
    ctx.quadraticCurveTo(cx, cy, rx2, ry2);

    ctx.moveTo(rx1, ry1);
    ctx.quadraticCurveTo(cx, cy-3, rx2, ry2);

    ctx.lineWidth = 1;
    ctx.strokeStyle = defaultColor;
    ctx.stroke();
    ctx.closePath();
}

function getControlY(x1, x2) {
    var d = Math.abs(x2 - x1);
    var y = 100;
    if (d < 375) y = 90;
    if (d < 305) y = 80;
    if (d < 235) y = 70;
    if (d < 175) y = 60;
    if (d < 155) y = 50;
    if (d < 135) y = 40;
    if (d < 95) y = 35;
    if (d < 60) y = 30;
    if (d < 50) y = 25;
    if (d < 40) y = 20;
    if (d < 30) y = 15;
    if (d < 20) y = 10;
    return y * arcDepthAdjustment;
}
//#endregion

//utilities
//#region

var scaleX = 1;
var scaleY = 1;

function setScale() {
    reSetScale();
    if (checkContext()) {
        ctx.scale(scaleX, scaleY);
    }
}

function reSetScale() {
    if (checkContext()) {
        ctx.scale(1, 1);
    }
}

function drawBarLine(x, y, lineWidth, opacity) {
    drawLine(x, y, x, y - lineSpacing * 4, lineWidth, opacity)
}

function drawLine(startX, startY, endX, endY, lineWidth, opacity) {
    if (opacity == undefined) opacity = 1;
    if (lineWidth == undefined) lineWidth = defaultLineWidth;
    ctx.beginPath();
    ctx.lineWidth = lineWidth;
    ctx.globalAlpha = opacity;
    ctx.moveTo(startX, startY);
    ctx.lineTo(endX, endY);
    ctx.strokeStyle = defaultColor;
    ctx.stroke();
    ctx.closePath();
}

function GetStaffWidth() {
    var staffWidth = 0;
    for (var k = 0; k < measures.length; k++) {
        var measure = measures[k];
        if (measure.Index < mDensity) { //in other words: Is this a measure in the first staff? 
            //if so, use it to calculate the width of the first staff
            staffWidth += parseInt(measure.Width);
        }
    }
    return staffWidth;
}

function checkContext() {
    if (cele == null || ctx == null) {
        setContext();
    }
    return (cele != null && ctx != null);
}

function setContext() {
    var fudgeFactor = 40;
    cele = document.getElementById(canvasId);
    if (cele !== null) {
        ctx = cele.getContext("2d");
        if (canvasWidth != "0px") {
            cele.width = canvasWidth;
        }
        else {
            cele.width = GetStaffWidth() * scaleX + fudgeFactor;
        }
        cele.height = sgDensity * sDensity * staffGap * scaleY + fudgeFactor;
    }
}


function GetNoteFromId(id) {
    var note = null;
    for (var i = 0; i < notes.length; i++) {
        note = notes[i];
        if (note.Id == id) {
            break;
        }
    }
    return note;
}

function GetChordFromId(id) {
    var chord = null;
    for (var i = 0; i < chords.length; i++) {
        chord = chords[i];
        if (chord.Id == id) {
            break;
        }
    }
    return chord;
}

function GetMeasureFromId(id) {
    var measure = null;
    for (var i = 0; i < measures.length; i++) {
        measure = measures[i];
        if (measure.Id == id) {
            break;
        }
    }
    return measure;
}

function getStaffFromId(id) {
    var result = null;
    for (var i = 0; i < staffs.length; i++) {
        if (staffs[i].Id == id) {
            result = staffs[i];
            break;
        }
    }
    return result;
}
//#endregion
