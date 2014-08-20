<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Composer.Server.Default" %>

<!doctype html>

<html>
<head>
    <meta http-equiv="CACHE-CONTROL" content="PUBLIC">
    <meta http-equiv="X-UA-Compatible" content="chrome=1, IE=edge"> 
    <meta property="fb:admins" content="675485908" />
    <title>Composer.Silverlight.UI</title>
    <style type="text/css">
        html, body
        {
            height: 100%;
            overflow: auto;
        }
        body
       ` {
            padding: 0;
            margin: 0;
        }
        #silverlightControlHost
        {
            height: 100%;
            text-align: center;
        }
    </style>
    <script type="text/javascript" src="scripts/Silverlight.js"></script>
    <script type="text/javascript" src="scripts/soundmanager2.js"></script>
    <script type="text/javascript" src="scripts/social.js"></script>
    <script type="text/javascript" src="scripts/modernizr-2.6.js"></script>
    <script type="text/javascript" src="scripts/composer.js"></script>
    <script type="text/javascript" src="scripts/jquery.xmldom.js"></script>

    <script type="text/javascript">
        function setDirty() {
        }
    </script>
</head>
<body>
    <div id="fb-root">
    </div>
    <script language="javascript" type="text/javascript">
        (function () {
            try {
                var e = document.createElement('script'); e.async = true;
                e.src = document.location.protocol + '//connect.facebook.net/en_US/all.js';
                document.getElementById('fb-root').appendChild(e);
            }
            catch (ex) {
            }
        } ());
    </script>
    <form id="sheet" runat="server" style="height: 600px">
    <div style="display: block" id="silverlightControlHost">
        <object id="plugin" data="data:application/x-silverlight-2," type="application/x-silverlight-2"
            width="100%" height="600px">
            <param name="source" value="ClientBin/Composer.Silverlight.UI.xap" />
            <param name="onError" value="onSilverlightError" />
            <param name="onresize" value="onSilverlightResize" />
            <param name="onLoad" value="pluginLoaded" />
            <param name="background" value="white" />
            <param name="enableRedrawRegions" value="false" />
            <param name="minRuntimeVersion" value="4.0.50401.0" />
            <param name="autoUpgrade" value="true" />
            <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=4.0.50401.0" style="text-decoration: none">
                <img src="http://go.microsoft.com/fwlink/?LinkId=161376" alt="Get Microsoft Silverlight"
                    style="border-style: none" />
            </a>
        </object>
        <iframe id="_sl_historyFrame" style="visibility: hidden; height: 0px; width: 0px;
            border: 0px"></iframe>
    </div>
    <div class="fb-like" data-href="https://www.wecontrib.com/composer/" data-width="50"
        data-show-faces="false" style="background-color: #f7f7f7; position: absolute;
        top: 462px; left: 10px;" id="likeButton" data-layout="button_count">
    </div>
    <div id="tweetButton" style="background-color: transparent; position: absolute; top: 487px;
        left: 10px;">
        <a href="https://twitter.com/share" class="twitter-share-button" data-url="https://dev.twitter.com/pages/tweet_button"
            data-via="your_screen_name" data-text="Checking out this page about Tweet Buttons"
            data-related="anywhere:The Javascript API" data-count="none">Tweet</a>
    </div>
    <div id="fbComment" style="display: none; position: absolute; top: 1px; left: 535px;"
        class="fb-comments" data-href="wecontrib.com" data-num-posts="2" data-width="380">
    </div>
    <input style="height: 0px; display: none" type="text" id="playbackXml" />
    <table>
        <tr>
            <td>
                <input style="font-size: 8pt; height: 14px; display: none" type="text" id="uid"
                    value="" />
            </td>
            <td>
                <input style="font-size: 8pt; height: 14px; display: none" type="text" id="username"
                    value="" />
            </td>
        </tr>
        <tr>
            <td>
                <input style="font-size: 8pt; height: 14px; display: none" type="text" id="accesstoken"
                    value="" />
            </td>
            <td>
                <input style="font-size: 8pt; height: 14px; display: none" type="text" id="userimageurl"
                    value="" />
            </td>
            <td colspan="3">
                <canvas  id="facebookDependencyLoadingProgress" width="200" height="20">
                </canvas>
            </td>
        </tr>
    </table>
    </form>
</body>
</html>
