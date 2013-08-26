"use strict";
soundManager.url = "soundmanager2.swf";
soundManager.flashVersion = 9;
soundManager.useFlashBlock = false;
soundManager.debugMode = false;
soundManager.useHighPerformance = true;
soundManager.useFastPolling = true;

var notes = [];
var sounds = [];
var pitches = [];
var shim;

var nIdx;
var cur;
var referenceTime;
var chordStarttime;
var at;
var eleLog = null;
var AudioType = {
	OGG: 'OGG',
	MP3: 'MP3',
	WAV: 'WAV',
	M4A: 'M4A'
};

soundManager.onready(function () {

});

var isPaused = false;
var resumeStarttime = 0;
var startingStartTime = 0;
var oInterval;
var requestId;
var logEile;
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

function prepAudio(inst) {
	//load only the audio files required to play the selection.
    var s = [];
    var p1 = null;
    var src;
    var ch = 1;
    at = getAudioType();
    for (var i = 0; i < pitches.length; i++) {
    	src = "";
    	var p2 = pitches[i];
    	
    	//there are 2 identical folders (named '1' and '2'), each containing the exact same audio files.
    	//getChannel() decides what directory the audio file for a particular note should come from.
        ch = getChannel(p2, p1, ch); 

        p1 = p2;
        src = src.concat("instruments", "/", inst, "/", ch, "/", at, "/", p2, ".", at);
        if (shim) {
            soundManager.createSound(p2, src)
        }
        else {
            var a = new Audio();
            a.pitch = p2;
            a.ch = ch;
            a.src = src;
            s[i] = a;
        }
    }
    return s;
}

function L(m){
    eleLog.value += m;
    eleLog.value += "\n";
}

function getChannel(p2, p1, b /*current ch*/) {
	//if 2 adjacent notes have the same pitch, then the same audio file is responsible for playing both notes.
	//so, the playback process might not be able to access the audio file when it needs to play the 2nd note if the 1rst note is still using it.
	//so, if there are 2 adjacent notes with the same pitch, one of them will be played by the audio file in one of the directories while the other
	//is played by the identical audio file in the other.
	var ch = 1; //assume directory '1' because most of the time (p1 != p2).
	if (p1 == p2) {
		//if both notes have same pitch then return 1 or 2 depending whether the preceding notes audio file came from directory 2 or 1 respectively.
		if (b == 2) ch = 1;
		else ch = 2;
    }
    return ch;
}

function getPitches() {
	//get only the pitches that are used in the current selection.
    var p = [];
    for (var i = 0; i < notes.length; i++) {
        p[p.length] = notes[i].pitch;
    }
    return p;
}

function getAudioType() {
    var at = AudioType.MP3;
    L(at);
	if (!shim) {
		var at = AudioType.WAV;
		if (Modernizr.audio.mp3) at = AudioType.MP3;
		else if (Modernizr.audio.ogg) at = AudioType.OGG;
	}
	return at;
}

function playSelection(inst, xml) {
    eleLog = document.getElementById("log");
    eleLog.value = "";
    shim = !Modernizr.audio;
    isPaused = false;
    notes = parse(xml);
    notes.sort(function (a, b) { return a.starttime - b.starttime });
    for (var i = 0; i < notes.length; i++) {
        L(notes[i].starttime);
    }
    pitches = getPitches();
    sounds = prepAudio(inst);
    notes = normalizeStarttimes();
    play();
}

function normalizeStarttimes() {
	//substract the first chords starttime from the starttime of each note, so
	//that when the selection does not contain the first chord in the composition,
	//the selected notes begin to play immediately anyway.
    startingStartTime = notes[0].starttime;
    if (startingStartTime > 0) {
        for (var i = 0; i < notes.length; i++) {
            notes[i].starttime = notes[i].starttime - startingStartTime;
        }
    }
    return notes;
}
var cnt = 0;
function render() {
    if (nIdx == notes.length) {
        dispose();
        return;
    }

    //var deltaTime = new Date().getTime() - referenceTime;
    var deltaTime = window.performance.now() - referenceTime;
    var tempo = 1000; //it's really inverse tempo. the higher the number, the slower the playback;
    L(deltaTime / tempo);
    cnt++;
    if (deltaTime / tempo >= chordStarttime) {
        L("           " + deltaTime / tempo + ", " + chordStarttime);
        for (nIdx = cur; nIdx < notes.length; nIdx++) {
            var note = notes[nIdx];
        	//save the starttime into resumeStarttime, in anticipation of the user clicking the 'pause' button.
        	//if the user pauses playback, the resumeStarttime value is passed application (via web service), 
            //so that if the user 'unpauses', the application can return only the chords that haven't been played yet.
            resumeStarttime = note.starttime + (startingStartTime * 1); //TODO: can't this line go inside the following 'if' block?
            if (isPaused) {
                dispose();
                return;
            }
        	//play all notes with the same starttime as the current chord.
        	//although the audio file play() function is called sequentially on each note, the 
            //notes are played simultaneously, or so close to simultaneously that you can't hear any difference. 
            if (chordStarttime == note.starttime) {
                if (shim) {
                    soundManager.play(note.pitch);
                    if (nIdx == notes.length - 1) {
                        Done();
                    }
                }
                else {
                    var a = sounds[nIdx];
                    if (nIdx == notes.length - 1) {
						//if this is the last note to play, then add 'ended' event handler so we can reset the playback controls.
                    	a.addEventListener('ended', Done);
                    	a.src.add
                    }
                    a.play();
                }
            }
            else {
                cur = nIdx; //set cursor to the index of the first note in the next chord.
                chordStarttime = notes[cur].starttime; //set chordStarttime to the starttime of the next chord.
                break;
            }
        }
    }
    requestId = window.requestAFrame(render);
}

function play() {
    nIdx = 0;
    cur = 0;
    //referenceTime = new Date().getTime();
    referenceTime = window.performance.now();
    chordStarttime = notes[cur].starttime;
    requestId = window.requestAFrame(render);
    //oInterval = window.setInterval(render, 10);
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
        note.instruemnt = $(this).attr('instrument');
        note.duration = $(this).attr('duration');
        note.starttime = $(this).attr('starttime');
        note.status = $(this).attr('status');
        notes[notes.length] = note;
    });
    return notes;
}

// handle multiple browsers for requestAnimationFrame()
window.requestAFrame = (function () {
    return window.requestAnimationFrame ||
            window.webkitRequestAnimationFrame ||
            window.mozRequestAnimationFrame ||
            window.oRequestAnimationFrame ||
            // if all else fails, use setTimeout
            function (callback) {
                return window.setTimeout(callback, 1000 / 60); // shoot for 60 fps
            };
})();

// handle multiple browsers for cancelAnimationFrame()
window.cancelAFrame = (function () {
    return window.cancelAnimationFrame ||
            window.webkitCancelAnimationFrame ||
            window.mozCancelAnimationFrame ||
            window.oCancelAnimationFrame ||
            function (id) {
                window.clearTimeout(id);
            };
})();