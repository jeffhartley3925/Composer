"use strict";
soundManager.url = "soundmanager2.swf";
soundManager.flashVersion = 9;
soundManager.useFlashBlock = false;
soundManager.debugMode = false;
soundManager.useHighPerformance = true;
soundManager.useFastPolling = true;

soundManager.onready(function () {

});

var isPaused = false;
var resumeStarttime = 0;
var startingStartTime = 0;
var oInterval;

function PausePlayback() {
    isPaused = true;
    slPlugin.Content.ContribShell.SetResumeStarttime(resumeStarttime);
}

function StopPlayback() {
    isPaused = true;
    window.clearInterval(oInterval);
}

function Done() {
    if (slPlugin == null) {
        slPlugin = document.getElementById("plugin");
    }
    slPlugin.Content.ContribShell.FinishedPlayback();
}

function loadSounds(notes, pitches, soundFormat, bPolyFill, instrument) {
    var i;
    var sounds = [];
    var prevPitch = "";
    var pathIndex = 1;
    var fileExtension = soundFormat;
    var root = "instruments";
    for (i = 0; i < pitches.length; i++) {
        var pitch = pitches[i];
        pathIndex = getPathIndex(pitch, prevPitch, pathIndex);
        prevPitch = pitch;
        var src = root + "/" + instrument + "/" + pathIndex + "/" + soundFormat + "/" + pitch + "." + fileExtension;
        if (bPolyFill) {
            soundManager.createSound(pitch, src)
        }
        else {
            var audio = new Audio();
            audio.pitch = pitch;
            audio.pathIndex = pathIndex;
            audio.src = src;
            sounds[i] = audio;
        }
    }
    return sounds;
}

function getPathIndex(pitch, prevPitch, pathIndex) {
    if (pitch == prevPitch) {
        if (pathIndex == 2) {
            pathIndex = 1;
        }
        else {
            pathIndex = 2;
        }
    }
    else {
        pathIndex = 1;
    }
    return pathIndex;
}

function getPitch(notes) {
    var i;
    var pitches = [];
    try {
        for (i = 0; i < notes.length; i += 1) {
            pitches[pitches.length] = notes[i].pitch;
        }
    }
    catch (ex) {
        alert(ex.message);
    }
    return pitches;
}

function getSoundsoundFormat(bPolyFill) {
    if (bPolyFill) {
        soundFormat = "mp3";
    }
    else {
        var soundFormat = "wav";
        if (Modernizr.audio.mp3) {
            soundFormat = "mp3";
        }
        else if (Modernizr.audio.ogg) {
            soundFormat = "ogg";
        }
    }
    return soundFormat;
}

function playSelection(instrument, xml) {
    var bPolyFill = !Modernizr.audio;
    isPaused = false;
    var soundFormat = getSoundsoundFormat(bPolyFill);
    if (soundFormat.length > 0) {
        var notes = [];
        var sounds = [];
        var pitches = [];
        notes = parse(xml);
        pitches = getPitch(notes);
        sounds = loadSounds(notes, pitches, soundFormat, bPolyFill, instrument);
        notes = shiftTime(notes)
        play(notes, sounds, pitches, bPolyFill);
    }
    else {
        //get a modern browser
    }
}

function shiftTime(notes) {
    var j;
    startingStartTime = notes[0].starttime;
    if (startingStartTime > 0) {
        for (j = 0; j < notes.length; j++) {
            notes[j].starttime = notes[j].starttime - startingStartTime;
        }
    }
    return notes;
}

function play(notes, sounds, pitches, bPolyFill) {
    var i = 0;
    var noteCursor = 0;
    var referenceTime = new Date().getTime();
    var chordStarttime = notes[noteCursor].starttime;
    oInterval = window.setInterval(function () {
        if (i == notes.length) {
            dispose();
            return;
        }
        var deltaTime = new Date().getTime() - referenceTime;
        if (deltaTime / 1000 >= chordStarttime) {
            for (i = noteCursor; i < notes.length; i++) {
                var note = notes[i];
                //save the starttime into resumeStarttime so if user pauses during this
                //cycle, we know where to continue playback when/if the user resumes playback.
                resumeStarttime = note.starttime + (startingStartTime * 1);
                if (isPaused) {
                    dispose();
                    return;
                }
                if (chordStarttime == note.starttime) {
                    if (bPolyFill) {
                        soundManager.play(note.pitch);
                    }
                    else {
                        var oAudio = sounds[i];
                        if (i == notes.length - 1) {
                            oAudio.addEventListener('ended', Done);
                        }
                        if (oAudio != null) {
                            if (i == notes.length - 1) {
                                oAudio.src.add
                            }
                            oAudio.play();
                        }
                    }
                }
                else {
                    noteCursor = i;
                    chordStarttime = notes[noteCursor].starttime;
                    break;
                }
            }
        }
    }, 10);
}

function dispose() {
    window.clearInterval(oInterval);
    oInterval = null;
    isPaused = false;
}

function parse(xml) {
    var notes = [];
    var $dom = $.xmlDOM(xml, function (error) {
        alert('A parse error occurred! ' + error);
    });
    $dom.find('root > row').each(function () {
        var note = new Object();
        note.pitch = $(this).attr('pitch');
        note.instrument = $(this).attr('instrument');
        note.duration = $(this).attr('duration');
        note.starttime = $(this).attr('starttime');
        note.status = $(this).attr('status');
        notes[notes.length] = note;
    });
    return notes;
}