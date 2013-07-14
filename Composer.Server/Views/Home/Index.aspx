<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <meta http-equiv="CACHE-CONTROL" content="PUBLIC">
    <meta http-equiv="X-UA-Compatible" content="chrome=1, IE=edge">
    <meta property="fb:admins" content="675485908" />
    <title>We.Compose</title>
    <style type="text/css">
        .requestContainer {
            font-family: arial;
            position: relative;
            top: -1px;
            left: 590px;
            display: none;
            color: #3b5998;
            font-size: smaller;
            cursor: default;
        }

        .request {
            text-decoration: underline;
            cursor: pointer;
        }

        .fb_ltr {
            height: 735px !important;
            overflow: auto;
        }

        html, body {
            height: 100%;
            overflow: hidden;
        }

        body {
            padding: 0;
            margin: 0;
        }

        #silverlightControlHost {
            height: 100%;
            width: 100%;
            text-align: center;
        }
    </style>
    <script type="text/javascript" src="<%= Url.Content ("~/scripts/Silverlight.js") %>"></script>
    <script type="text/javascript" src="<%= Url.Content ("~/scripts/index.js") %>"></script>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <form id="sheet" runat="server" style="text-align: center; height: 100%">
        <div id="fb-root">
        </div>
        <script type="text/javascript">
            (function () {
                try {
                    var e = document.createElement('script'); e.async = true;
                    e.src = document.location.protocol + '//connect.facebook.net/en_US/all.js';
                    document.getElementById('fb-root').appendChild(e);
                }
                catch (ex) {
                }
            }());
        </script>
        <table style="height:30px;width: 100%; background-color: #f2f2f2; position: relative; left: 3px; top: 1px;">
            <tbody>
                <tr>
                    <td id="loginButtonContainer" style="border: 0px solid transparent; position: absolute; left: 98px; top: -1px; border-color: #f2f2f2; display: block;">
                        <fb:login-button autologoutlink="true" id="fbLogin" onclick="fbLogin()" class="fb-login-button"></fb:login-button>
                    </td>
                    <td id="likeButtonContainer" style="position: absolute; left: 182px; top: -2px; border-color: #f2f2f2; display: none;">
                        <div id="like">
                        </div>
                    </td>
                    <td id="sendButtonContainer" style="position: absolute; left: 264px; top: -2px; border-color: #f2f2f2; display: none;">
                        <div id="send">
                        </div>
                    </td>
                    <td id="googlePlusoneButtonContainer" style="position: absolute; left: 325px; top: -2px; border-color: #f2f2f2; display: none;">
                        <div class="g-plusone" data-size="medium" data-width="30">
                        </div>
                        <!-- Place this tag after the last +1 button tag. -->
                        <script type="text/javascript">
                            (function () {
                                var po = document.createElement('script'); po.type = 'text/javascript'; po.async = true;
                                po.src = 'https://apis.google.com/js/plusone.js';
                                var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(po, s);
                            })();
                        </script>
                    </td>
                    <td id="tweetButtonContainer" style="position: absolute; left: 394px; top: -2px; border-color: #f2f2f2; display: none;">
                        <div style="background-color: transparent;">
                            <a id="tweetButton" href="https://twitter.com/share" class="twitter-share-button"
                                data-url="" data-text="">Tweet</a>
                        </div>
                    </td>
                    <td id="pinterestButtonContainer" style="position: absolute; left: 478px; top: -2px; border-color: #f2f2f2; display: none;">
                        <div class="addthis_toolbox addthis_default_style">
                            <a layout="horizontal" class="addthis_button_pinterest_pinit"></a>
                        </div>
                        <script type="text/javascript">
                            var addthis_config = { "data_track_addressbar": false };
                        </script>
                        <script type="text/javascript" src="//s7.addthis.com/js/300/addthis_widget.js#pubid=ra-50da3f994cca6e73"></script>
                    </td>
                    <td id="requestContainer" style="text-align: left; height: 19px;">
                        <div id="requestLyricsContainer" class="requestContainer">
                            You don't have lyrics. <span id="1" class="request">Ask 1 or more of your friends to
                            write some.</span>
                        </div>
                        <div id="requestCollaboratorsContainer" class="requestContainer">
                            Ne of your friends have contributed. <span id="2" class="request">Ask 1 or more friends
                            for their input.</span>
                        </div>
                        <div id="requestListenersContainer" class="requestContainer">
                            <span id="3" class="request">Ask 1 or more friends to listen to this composition.</span>
                        </div>
                        <script>
                            var _compositionId = "";
                            var _compositionTitle = "";
                            var _collaboratorIndex = "";
                            $(document).ready(function () {
                                $("span.request").mouseover(function () {
                                    $(this).css("color", "red");
                                });
                                $("span.request").mouseout(function () {
                                    $(this).css("color", "#3b5998");
                                });
                            });
                            function hideAllRequestPrompt() {
                                $("div.requestContainer").css("display", "none");
                            }
                            function setRequestPrompt(collaborationCnt, verseCnt, compositionId, compositionTitle, collaboratorIndex) {
                                if (collaboratorIndex == 0) {
                                    _compositionId = compositionId;
                                    _compositionTitle = compositionTitle;
                                    _collaboratorIndex = collaboratorIndex;
                                    if (verseCnt == 0) {
                                        $("*#requestLyricsContainer").css("display", "block");
                                    }
                                    else if (collaborationCnt == 1) {
                                        $("*#requestCollaboratorsContainer").css("display", "block");
                                    }
                                    else {
                                        var date = new Date();
                                        var ticks = date.getTime();
                                        //duplicate selectors. will add more request prompts later
                                        if (ticks % 2 == 0) {
                                            $("*#requestListenersContainer").css("display", "block");
                                        }
                                        else {
                                            $("*#requestListenersContainer").css("display", "block");
                                        }
                                    }
                                }
                            }
                            $("span.request").click(function (event) {
                                switch (event.target.id) {
                                    case "1":
                                        sendRequest("I neded lyrics for my composition - '" + _compositionTitle + "'. Be creative!", "", _compositionId)
                                        break;
                                    case "2":
                                        sendRequest("I invite you to edit my composition - '" + _compositionTitle + "'.", "", _compositionId)
                                        break;
                                    case "3":
                                        sendRequest("Listen to my new composistion - '" + _compositionTitle + "'.", "", _compositionId)
                                        break;
                                }
                            });
                        </script>
                    </td>
                    <td style="color: #336699; font-size: x-small; position: absolute; left: 577px; top: 5px; display: block;"
                        id="version"></td>
                    <td style="color: #336699; font-size: x-small; position: absolute; left: 600px; top: 5px; display: block;"
                        id="codec"></td>
                </tr>
            </tbody>
        </table>
                <div style="z-index:1000;display:block" id="hubLikes"></div>
        <span id="silverlightControlHost">
            <object id="plugin" data="data:application/x-silverlight-2," type="application/x-silverlight-2"
                width="100%" height="100%">
                <param name="source" value="ClientBin/Composer.Silverlight.UI.xap" />
                <param name="onError" value="onSilverlightError" />
                <param name="onresize" value="onSilverlightResize" />
                <param name="onLoad" value="pluginLoaded" />
                <param name="initParams" value="compositionId=<%= ViewData["CompositionId"] %>, collaborationIndex=<%= ViewData["CollaborationIndex"] %>" />
                <param name="background" value="transparent" />
                <param name="enableRedrawRegions" value="false" />
                <param name="windowless" value="true" />
                <param name="minRuntimeVersion" value="5.0.61118.0" />
                <param name="autoUpgrade" value="true" />
                <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=5.0.61118.0" style="text-decoration: none">
                    <img src="http://go.microsoft.com/fwlink/?LinkId=161376" alt="Get Microsoft Silverlight"
                        style="border-style: none" />
                </a>
            </object>
            <iframe id="_sl_historyFrame" style="visibility: hidden; height: 0px; width: 0px; border: 0px"></iframe>
        </span>
        <fb-comment style="display: none; background-color: white; position: absolute; right: 4px; top: 31px;"
            class="fb-comments" id="comments" data-href="https://www.wecontrib.com/composer/"
            data-width="297" data-num-posts="6"></fb-comment>
        <input style="height: 0px; display: none" type="text" id="playbackXml" />
        <table style="display: block">
            <tr>
                <td>
                    <div style="text-align: left; color: black; font-size: 8pt; height: 14px;">
                        My Username
                    </div>
                    <input style="color: white; background-color: blue; font-size: 8pt; height: 14px;"
                        type="text" id="username" value="" />
                </td>
            </tr>
            <tr>
                <td>
                    <div style="text-align: left; color: black; font-size: 8pt; height: 14px;">
                        My User Id
                    </div>
                    <input style="color: black; background-color: orange; font-size: 8pt; height: 14px;"
                        type="text" id="uid" value="" />
                </td>
                <td>
                    <div style="text-align: left; color: black; font-size: 8pt; height: 14px;">
                        My Image Url
                    </div>
                    <input style="color: black; background-color: pink; font-size: 8pt; height: 14px;"
                        type="text" id="userimageurl" value="" />
                </td>
                <td colspan="4">
                    <div style="text-align: left; color: black; font-size: 8pt; height: 14px;">
                        Access Token
                    </div>
                    <input style="color: black; font-size: 8pt; height: 14px;" type="text" id="accesstoken"
                        value="" />
                </td>
                <td colspan="5">
                    <div style="text-align: left; color: black; font-size: 8pt; height: 14px;">
                        Friends
                    </div>
                    <input style="color: black; font-size: 8pt; height: 14px;" type="text" id="friends"
                        value="" />
                    <input style="color: black; font-size: 8pt; height: 14px;" type="text" id="names"
                        value="" />
                    <input style="color: black; font-size: 8pt; height: 14px;" type="text" id="pictures"
                        value="" />
                </td>
            </tr>
        </table>
        <table style="display: block">
            <tr>
                <td>
                    <textarea enableviewstate="true" viewstatemode="Enabled" runat="server" style="display: none; width: 600px;"
                        cols="1" rows="16" id="txtPNGBytes"></textarea>
                </td>
            </tr>
        </table>
        <canvas id="facebookDependencyLoadingProgress" width="200" height="0"></canvas>
        <img alt="composition" id="compositionImage" style="display: none;" src="" />

    </form>
</asp:Content>
