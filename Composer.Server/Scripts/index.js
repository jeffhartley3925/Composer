var dirty = "false";
var username;
var uid;
var accessToken;
var bFacebookDependenciesLoaded = false;
var targetFacebookDependencyCount = 6;
var loadedFacebookDependencyCount = 0;
var colors = ["blue", "orange", "pink"];
var facebookDependencies = ["username", "uid", "userimageurl", "friends", "names", "pictures"];
var channelurl = "//" + location.host + "/composer/channel.html";
var applicationId
if (location.host === "localhost") {
    applicationId = "449164325102238";
} else {
    applicationId = "171096762940671";
}
var done = false;

window.onbeforeunload = function () {
    if (dirty == "true") {
        return "You have unsaved changes. If you leave this page before saving your work, you will lose your changes.";
    }
};

function setDirty(bDirty) {
    dirty = bDirty.toString();
}

function fbLogin() {
    done = false;
    FB.login(function (response) {
        if (response.authResponse) {

        } else {

        }
    }, { scope: 'email' });
}

function fbLogout() {
    FB.logout(function (response) {
        window.location.reload();
    });
}

window.fbAsyncInit = function () {

    FB.init({
        appId: applicationId,
        channelUrl: channelurl,
        status: true, // check login status
        cookie: true, // enable cookies to allow the server to access the session
        xfbml: true,  // parse XFBML
        frictionlessRequests: true,
        oauth: true
    });

    FB.getLoginStatus();

    FB.Event.subscribe('edge.create',
    function (response) {
        alert('You liked the URL: ' + response);
    });

    FB.Event.subscribe('auth.statusChange', function (response) {
        //alert('The status changed: ' + response.status);
        if (response.status == "unknown") {
            window.location.reload();
        }
    });

    FB.getLoginStatus(function (response) {
        try {
            if (!done) {
                done = true;
                processLogin(response);
            }
        }
        catch (ex) {
        }
    });

    FB.Event.subscribe('auth.authResponseChange', function (response) {
        //alert('The status is: ' + response.status);
        try {
            if (!done) {
                done = true;
                processLogin(response);
            }
        }
        catch (ex) {
        }
    });

    function processLogin(response) {
        try {
            if (response.status === 'connected') {

                var ele = document.getElementById("loginButtonContainer");
                if (ele != null) {
                    // ele.setAttribute("style", "position: absolute; left: 28px; top: -1px; border-color: #f2f2f2; display: none;");
                }

                var usernameElement = document.getElementById(facebookDependencies[0]);
                var useridElement = document.getElementById(facebookDependencies[1]);
                var accessTokenElement = document.getElementById("accesstoken");
                accessToken = response.authResponse.accessToken;
                uid = response.authResponse.userID;

                getPicture(uid);
                getFriends(uid);
                useridElement.value = uid;
                checkFacebookDependencies(facebookDependencies[1]);

                FB.api('/me', function (response) {
                    username = response.name;
                    usernameElement.value = username;
                    checkFacebookDependencies(facebookDependencies[0]);
                });
                accessTokenElement.value = accessToken;
            } else if (response.status === 'not_authorized') {
                fbLogin();
            } else {
                fbLogin();
            }
        }
        catch (ex) {
            alert("Error in processLogin: " + ex.message);
        }
    }
};

function updateVersion(version) {
    $('#version').text(version);
}
var picturesElement;
var friendPictures = new Array();
var friendIds = new Array();
var friendCnt = 0;
function getFriends(id) {
    var friendsElement, namesElement;
     friendNames = new Array(); 
    try {
        friendsElement = document.getElementById(facebookDependencies[3]);
        namesElement = document.getElementById(facebookDependencies[4]);
        picturesElement = document.getElementById(facebookDependencies[5]);
        
        FB.api(id + '/friends', function (response) {
            friendCnt = response.data.length;
            for (var i = 0; i < friendCnt; i++) {
                friendIds[i] = response.data[i].id;
                friendNames[i] = response.data[i].name;
            }
            friendsElement.value = friendIds;
            namesElement.value = friendNames;
            checkFacebookDependencies(facebookDependencies[3]);
            checkFacebookDependencies(facebookDependencies[4]);
            for (var j = 0; j < friendCnt; j++) {
                getPicture2(friendIds[j])
            }

        });
    }
    catch (ex) {
        alert("Error in getFriends: " + ex.message);
    }
}

function getIndex(id) {
}

function getPicture2(id) {

    try {
        FB.api(id + '/picture', function (response) {
            var pictureUrl = response.data.url;
            friendPictures[$.inArray(id, friendIds)] = pictureUrl;
            if (friendPictures.length == friendCnt) {
                picturesElement.value = friendPictures;
                checkFacebookDependencies(facebookDependencies[5]);
            }
        });
    }
    catch (ex) {
        alert("Error in getPicture2: " + ex.message);
    }
}

function getPicture(id) {
    var urlElement;
    try {
        urlElement = document.getElementById(facebookDependencies[2]);
        FB.api(id + '/picture', function (response) {
            var pictureUrl = response.data.url;
            urlElement.value = pictureUrl;
            checkFacebookDependencies(facebookDependencies[2]);
        });
    }
    catch (ex) {
        alert("Error in getPicture: " + ex.message);
    }
}

function checkFacebookDependencies(dependency) {
    var bOldHub = true;
    var facebookDependency = "";
    var i;
    try
    {
        if (loadedFacebookDependencyCount < targetFacebookDependencyCount) {
            loadedFacebookDependencyCount = 0;
            for (i = 0; i < facebookDependencies.length; i++) {
                facebookDependency = facebookDependencies[i];
                if (document.getElementById(facebookDependency).value.toString().length > 0) {
                    loadedFacebookDependencyCount++;
                    updateFacebookDependencyLoadProgressBar();
                    if (loadedFacebookDependencyCount == targetFacebookDependencyCount) {
                        if (!bFacebookDependenciesLoaded) {
                            bFacebookDependenciesLoaded = true;
                            if (slPlugin == null) {
                                slPlugin = document.getElementById("plugin");
                            }
                            if (slPlugin != null) {
                                if (slPlugin.Content != null) {
                                    if (slPlugin.Content.ContribShell != null) {
                                        slPlugin.Content.ContribShell.OnFacebookDependenciesLoaded(loadedFacebookDependencyCount, username);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    catch (ex) {
        //alert("Error in checkFacebookDependencies: " + ex.message + " " + facebookDependency);
    }
}

function onReadySignal() {
    try {
        if (bFacebookDependenciesLoaded) {
            if (slPlugin == null) {
                slPlugin = document.getElementById("plugin");
            }
            if (slPlugin != null) {
                if (slPlugin.Content != null) {
                    if (slPlugin.Content.ContribShell != null) {
                        slPlugin.Content.ContribShell.OnFacebookDependenciesLoaded(loadedFacebookDependencyCount, username);
                    }
                }
            }
        }
    }
    catch (ex) {
        alert("Error in onReadySignal: " + ex.message);
    }
}

function updateFacebookDependencyLoadProgressBar() {
    try {
        var canvas = document.getElementById('facebookDependencyLoadingProgress');
        if (canvas.getContext) {
            var context = canvas.getContext('2d');
            canvas.Width = canvas.Width;
            for (var i = 0; i < facebookDependencies.length; i++) {
                var d = facebookDependencies[i];
                if (document.getElementById(d).value.toString().length > 0) {
                    context.beginPath();
                    context.rect(0 + (i * 15), 0, 10, 10);
                    context.fillStyle = colors[i];
                    context.fill();
                }
            }
        }
    }
    catch (ex) {
        alert("Error in updateFacebookDependencyLoadProgressBar: " + ex.message);
    }
}