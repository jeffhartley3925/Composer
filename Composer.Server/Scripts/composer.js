"use strict";

var N = [];
var S = [];
var P = [];
var b_shim;
var n_idx;
var cur_idx;
var ref_time;
var cur_st;
var aud;
var log = null;

var AudioType = {
	OGG: 'OGG',
	MP3: 'MP3',
	WAV: 'WAV',
	M4A: 'M4A'
};

soundManager.onready(function () {
    soundManager.url = "soundmanager2.swf";
    soundManager.flashVersion = 9;
    soundManager.useFlashBlock = false;
    soundManager.debugMode = false;
    soundManager.useHighPerformance = true;
    soundManager.useFastPolling = true;
});

var isPaused = false;
var res_st = 0;
var s_st = 0;
var f_id;
var log;
var tempo = 1000; //inverse value. the higher the number, the slower the playback;
var debugging = false;
var verbose = false;

function PausePlayback() {
    isPaused = true;
    slPlugin.Content.ContribShell.SetResumeStarttime(res_st);
}

function StopPlayback() {
    isPaused = true;
    window.clearInterval(f_id);
}

function notifySL() {
    if (slPlugin == null) {
        slPlugin = document.getElementById("plugin");
    }
    slPlugin.Content.ContribShell.FinishedPlayback();
}

function loadAudio(inst) {
	//load only the audio files required to play the selection.
    var s = [];
    var p1 = null;
    var src;
    var ch = 1;
    aud = getBrowserAudio();
    if (debugging) L("Selected audio: " + aud);
    for (var i = 0; i < P.length; i++) {
    	src = "";
    	var p2 = P[i];
    	
    	//there are 2 identical folders (named '1' and '2'), each containing the exact same audio files.
    	//getAudioChannel() decides what directory the audio file for a particular note should come from.
        ch = getAudioChannel(p2, p1, ch); 

        p1 = p2;
        src = src.concat("instruments", "/", inst, "/", ch /*channel*/, "/", aud, "/", p2, ".", aud);
        if (b_shim) {
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

function L(m) {
    if (debugging) {
        log.value += m;
        log.value += "\n";
    }
}

function getAudioChannel(p2, p1, b /*current ch*/) {
	//if 2 adjacent N have the same pitch, then the same audio file is responsible for playing both N.
	//so, the playback process might not be able to access the audio file when it needs to play the 2nd note if the 1rst note is still using it.
	//so, if there are 2 adjacent N with the same pitch, one of them will be played by the audio file in one of the directories 'channels' while the other
	//is played by the identical audio file in the other.
	var ch = 1; //assume directory '1' because most of the time (p1 != p2).
	if (p1 == p2) {
		//if both N have same pitch then return 1 or 2 depending whether the preceding N audio file came from directory 2 or 1 respectively.
		if (b == 2) ch = 1;
		else ch = 2;
    }
    return ch;
}

function loadPitches() {
	//get only the P that are used in the current selection.
    var p = [];
    for (var i = 0; i < N.length; i++) {
        p[p.length] = N[i].pitch;
    }
    return p;
}

function getBrowserAudio() {
    var aud = AudioType.MP3;
	if (!b_shim) {
		var aud = AudioType.WAV;
		if (Modernizr.audio.mp3) aud = AudioType.MP3;
		else if (Modernizr.audio.ogg) aud = AudioType.OGG;
	}
	return aud;
}

function playSelection(inst, xml) {
    log = document.getElementById("log");
    log.value = "";
    b_shim = !Modernizr.audio;
    isPaused = false;
    N = parse(xml);
    N.sort(function (a, b) { return a.starttime - b.starttime });
    P = loadPitches();
    S = loadAudio(inst);
    N = normalizeStarttimes();
    if (debugging) {
        L("");
        L("actual\tnote\tdelta\tpitch");
        L("------\t----\t-----\t-----");
    }
    play();
}

function normalizeStarttimes() {
	//substract the first chords starttime from the starttime of each note, so
	//that when the selection does not contain the first chord in the composition,
	//the selected N begin to play immediately anyway.
    s_st = N[0].starttime;
    if (s_st > 0) {
        for (var i = 0; i < N.length; i++) {
            N[i].starttime = N[i].starttime - s_st;
        }
    }
    return N;
}
var prev_int = 0;
function render() {
    if (n_idx == N.length) {
        dispose();
        return;
    }
    var interval;

    if (window.performance.now) {
        interval = window.performance.now() - ref_time;
    }
    else {
        interval = new Date().getTime() - ref_time;
    }

    var r = (interval / tempo);
    if (debugging && verbose) L(r.toFixed(3).toString() + "\t (" + ((interval - prev_int) / tempo).toFixed(3) + ")");
    prev_int = interval
    if (interval / tempo >= cur_st) {
        for (n_idx = cur_idx; n_idx < N.length; n_idx++) {
            var note = N[n_idx];
        	//save the starttime into res_st, in anticipation of the user clicking the 'pause' button.
        	//if the user pauses playback, the res_st value is passed application (via web service), 
            //so that if the user 'unpauses', the application can return only the chords that haven't been played yet.
            res_st = note.starttime + (s_st * 1); //TODO: can't this line go inside the following 'if' block?
            if (isPaused) {
                dispose();
                return;
            }
        	//play all N with the same starttime as the current chord.
        	//although the audio file play() function is called sequentially on each note, the 
            //N are played simultaneously, or so close to simultaneously that you can't hear any difference. 
            if (cur_st == note.starttime) {
                if (b_shim) {
                    soundManager.play(note.pitch);
                    if (n_idx == N.length - 1) {
                        notifySL();
                    }
                }
                else {
                    var a = S[n_idx];
                    if (debugging) L(r.toFixed(3) + "\t" + cur_st + "\t" + ((r - cur_st) * tempo).toFixed(2) + "\t" + a.pitch);
                    if (n_idx == N.length - 1) {
						//if this is the last note to play, then add 'ended' event handler so we can reset the playback controls.
                    	a.addEventListener('ended', notifySL);
                    	a.src.add
                    }
                    a.play();
                }
            }
            else {
                cur_idx = n_idx; //set cursor to the index of the first note in the next chord.
                cur_st = N[cur_idx].starttime; //set cur_st to the starttime of the next chord.
                break;
            }
        }
    }
    f_id = window.requestAFrame(render);
}

function play() {
    n_idx = 0;
    cur_idx = 0;
    cur_st = N[cur_idx].starttime;
    prev_int = 0;
    f_id = window.requestAFrame(render);
    start();
}

function start() {
    if (window.performance.now) {
        ref_time = window.performance.now();
    }
    else {
        ref_time = new Date().getTime();
    }
    f_id = window.requestAFrame(render);
}

function dispose() {
    window.clearInterval(f_id);
    f_id = null;
    isPaused = false;
}

function parse(xml) {
    var N = [];
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
        N[N.length] = note;
    });
    return N;
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