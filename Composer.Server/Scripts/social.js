var version = 19.00;
var isInternetAccess = true;
function createLikeButtonContainers(cnt, _compositions) {
    if (!isInternetAccess) return;
    try {
    	var element = document.getElementById("hubLikes");
		element.style.display="none"
        if (element != null) {
            for (var i = 1; i <= cnt; i++) {
                var div = document.createElement("div");
                div.id = "hubLike" + i.toString();
                element.appendChild(div);
            }
        }
    }
    catch (ex) {
        alert(ex.message);
    }
}

function deleteLikeButtons(displayedCompositionCount) {
    if (!isInternetAccess) return;
	try {
		var element = document.getElementById("hubLikes");
		element.innerHTML = "";
	}
	catch (ex) {
		alert(ex.message);
	}
}
function createLikeButton(id, cid, index, lastOne) {
    if (!isInternetAccess) return;
    try {
        var url = document.location.protocol + "//" + location.host + "/composer/compositionfiles/" + id + "_" + cid + ".htm";
        $('#hubLike' + index).html('<fb:like style="position:absolute;left:485px;top:' + (216 + ((index-1) * 72)).toString() + 'px " href="' + url + '" layout="button_count" class="fb-like" data-send="false" show_faces="false" width="65" action="like" />');
        if (typeof FB !== 'undefined') {
            var o = document.getElementById('hubLike' + index);
            FB.XFBML.parse(o);
        }
        $('#hubLike' + index).click();
    }
    catch (ex) {
        alert(ex.message);
    }
    if (lastOne) {
    	var element = document.getElementById("hubLikes");
    	element.style.display = "block"
    }
}
function setLikeButtonHref(id, cid, obj) {
    if (!isInternetAccess) return;
    var url = document.location.protocol + "//" + location.host + "/composer/compositionfiles/" + id + "_" + cid + ".htm";
    try {
        $('#like').html('<fb:like href="' + url + '" layout="button_count" class="fb-like" data-send="false" show_faces="false" width="65" action="like" />');
        if (typeof FB !== 'undefined') {
            FB.XFBML.parse(document.getElementById('like'));
        }
        if (typeof FB !== 'undefined') {
            var ele = document.getElementById("comments");
            if (ele != null) {
                ele.setAttribute("style", "display:block;background-color:white;position:absolute;right:4px;top:29px;");
                ele.setAttribute("data-href", url);
                ele.setAttribute("data-width", "297"); //TODO: HARD CODED VALUE. is it ok?
                FB.XFBML.parse(document.getElementById('comments'));
            }
        }
        $('#send').html('<fb:send data-href="' + url + '" data-num-posts="2" font="lucida grande" colorscheme="light" />');
        if (typeof FB !== 'undefined') {
            FB.XFBML.parse(document.getElementById('send'));
        }

        $('#plusone').html('<g:plusone href="' + url + '" id="plusone" data-size="small"></g:plusone>');
        if (typeof FB !== 'undefined') {
            FB.XFBML.parse(document.getElementById('plusone'));
        }
        updateVersion(version);
    }
    catch (ex) {
        alert(ex.message);
    }
}

function sendRequest(message, title, compositionId, collaboratorIndex) {
    try {
        FB.ui({ method: "apprequests", message: message, title: title, redirect_uri: location.protocol + "//" + location.host + "/composer/?id=" + compositionId + "&index=" + collaboratorIndex }, requestCallback);
    }
    catch (ex) {
        slPlugin.Content.ContribShell.OnDisplayMessage("Request error");
    }
}

function requestCallback(response) {
    try {
        var json = JSON.stringify(response);
        if (json.indexOf("error") >= 0) {
            slPlugin.Content.ContribShell.OnDisplayMessage("Request Error");
        }
        else {
            slPlugin.Content.ContribShell.OnDisplayMessage("Request Sent");
        }
    }
    catch (ex) {
        slPlugin.Content.ContribShell.OnDisplayMessage("Request Error");
    }
}

function publishAction(compositionId, collaborationIndex) {
    try {
        FB.api('/me/wecontrib:create', 'post', { composition: location.protocol + '//' + location.host + '/composer/compositionfiles/' + compositionId.toString() + '_' + collaborationIndex.toString() + '.htm', description: 'TEST' },
        function (response) {
            var json = JSON.stringify(response);
            if (json.indexOf("error") >= 0) {
                slPlugin.Content.ContribShell.OnDisplayMessage("Publish Action Error");
            }
            else {
                slPlugin.Content.ContribShell.OnDisplayMessage("Publish Action Success");
            }
        });
    }
    catch (ex) {
        slPlugin.Content.ContribShell.OnDisplayMessage("Publish Action Error");
    }
}

function setTwitterButtonUrl(id, obj) {
    if (!isInternetAccess) return;
    try {
        var url = "https%3A%2F%2F" + location.host + "%2Fcomposer%2Fdeck/card%3Fid%3D" + id;

        obj.setAttribute("data-url", url);
        obj.setAttribute("href", "https://twitter.com/share?url=" + url + "&counturl=" + url);
        obj.setAttribute("data-counturl", url);
        obj.setAttribute("data-text", "Check out my composition: ");
        var script;
        var node = document.getElementsByTagName("head")[0] || document.body;
        if (node) {
            script = document.createElement("script");
            script.type = "text/javascript";
            script.src = "//platform.twitter.com/widgets.js";
            node.appendChild(script);
        }
        else {
            document.write("<script src='" + "//platform.twitter.com/widgets.js" + "' type='text/javascript'></script>");
        }
    }
    catch (ex) {
        alert(ex.Message);
    }
}
