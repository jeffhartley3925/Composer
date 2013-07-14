<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<%@ Import Namespace="MyExtensions" %>
<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="hubContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="<%= Url.Content ("~/content/accordian.css") %>" rel="stylesheet" type="text/css" />
    <script src="<%= Url.Content ("~/scripts/hub.js") %>" type="text/javascript"></script>
    <script src="<%= Url.Content ("~/scripts/index.js") %>" type="text/javascript"></script>
    <script src="<%= Url.Content ("~/scripts/card.js") %>" type="text/javascript"></script>
    <script type="text/javascript">

        function doCreate() {

        }

        function doCommit() {
            var pendingChanges = dataContext.get_hasChanges();
            if (pendingChanges !== true) {
                alert("No pending changes to save.");
                return;
            }

            var changes = dataContext.get_changes();
            var buffer = "";
            for (var i = 0; i < changes.length; i++) {
                ch = changes[i];
                buffer += makeReadable(ch.action) + " --> " + ch.item["Id"];
                buffer += "\n";
            }
            if (confirm(buffer))
                dataContext.saveChanges();
        }

        function makeReadable(action) {
            if (action === 0)
                return "Insert";
            if (action === 1)
                return "Update";
            if (action === 2)
                return "Delete";
        }

        function convertObjectCollectionToCount(objects) {
            return objects.length;
        }

        function convertDimensionIdToName(id, binding) {
            switch (binding.Class) {
                case "Instrument":
                    switch (id) {
                        case 0:
                            return "Piano";
                            break;
                    }
                    break;
                case "Key":
                    switch (id) {
                        case 4:
                            return "Key of C";
                            break;
                    }
                    break;
                case "TimeSignature":
                    switch (id) {
                        case 4:
                            return "4/4 Time";
                            break;
                    }
                    break;
            }
            return "Lookup failed";
        }

        function fbLogout() {
            FB.logout(function (response) {
                //Do what ever you want here when logged out like reloading the page
                window.location.reload();
            });
        }
    </script>

    <form id="sheet" runat="server" style="text-align: center; height: 600px">
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
        } ());
    </script>
    <canvas id="facebookDependencyLoadingProgress" width="200" height="20">
    </canvas>
    <div style="display: none;" class="accordion" id="divAccordion">
        <table style="border: 0px;" id="masterView" class="sys-template">
            <tr sys:command="select">
                <td>
                    <section class="thin" sys:id="{{ Id }}">
                        <h2 sys:id="{{ 'header_' + Id }}">
                            <a sys:id="{{ 'anchor_' + Id }}" sys:href="{{ '#' + Id }}">{ binding Provenance.TitleLine
                                } </a>
                        </h2>
                        <div>
                            <table class="sys-template" sys:attach="dataview" dataview:data="{{ Collaborations }}"
                                id="collaborationList" style="padding: 0px; border: 0px">
                                <tr>
                                    <td style="padding: 0px; border: 0px;">
                                        <img alt="FBU" style="padding: 0px" width="28" height="28" sys:src="{ binding PictureUrl }" />
                                    </td>
                                    <td style="text-align: left; font-size: 9pt; font-family: Trebuchet MS; vertical-align: middle;
                                        padding-top: 10px; border: 0px;">
                                        { binding Name }
                                    </td>
                                    <td id="statistics" style="background-color: white; width: auto;">
                                    </td>
                                </tr>
                            </table>
                            <div sys:id="{{ 'scroll_' + Id }}" onclick="toggleScale()" style="width: 255px; margin-left: 10px;
                                margin-right: 20px; margin-bottom: 10px; height: 126px; margin-top: 1px; overflow: auto;
                                border: solid 1px black; background-color: white; float: left;">
                                <canvas style="padding: 10px; z-index: 2;" sys:id="{{ 'canvas_' + Id }}">
                                </canvas>
                            </div>
                            <div style="margin-right: 30px;" sys:id="{{ 'optionBtnRow1_' + Id }}">
                                <div class="optionButtonContainer" style="background-image: url('images/view.png');">
                                    <input sys:id="{{ 'btnView_' + Id }}" type="button" class="optionButton" />
                                </div>
                                <div class="optionButtonContainer" style="background-image: url('images/edit.png');">
                                    <input sys:id="{{ 'btnEdit_' + Id }}" type="button" class="optionButton" />
                                </div>
                                <div class="optionButtonContainer" style="background-image: url('images/listen.png');">
                                    <input sys:id="{{ 'btnListen_' + Id }}" type="button" class="optionButton" />
                                </div>
                            </div>
                            <div sys:id="{{ 'optionBtnRow2_' + Id }}">
                                <div class="optionButtonContainer" style="position: relative; background-image: url('images/contribute.png');">
                                    <input sys:id="{{ 'btnContribute_' + Id }}" type="button" class="optionButton" />
                                </div>
                                <div class="optionButtonContainer" style="position: relative; background-image: url('images/printer.png');">
                                    <input sys:id="{{ 'btnPrint_' + Id }}" type="button" class="optionButton" />
                                </div>
                            </div>

                        </div>
                    </section>
                </td>
            </tr>
        </table>
    </div>
    <div style="display: none" id="detailsView" class="sys-template">
        <input type="button" value="Commit all changes" onclick="doCommit()" />
        <input type="button" value="Create new composition" onclick="doCreate()" />
        <div>
            <input id="compositionTitle" type="text" value="" />
        </div>
        <div>
            <%= Html.SysTextBox("Title", "{binding Provenance.TitleLine}")%>
        </div>
        <div>
            <%= Html.SysTextBox("InstrumentId", "{binding Instrument_Id,convert=convertDimensionIdToName, Class=Instrument}")%>
        </div>
        <div>
            <%= Html.SysTextBox("KeyId", "{binding Key_Id,convert=convertDimensionIdToName, Class=Key}")%>
        </div>
        <div>
            <%= Html.SysTextBox("TimeSignatureId", "{binding TimeSignature_Id,convert=convertDimensionIdToName, Class=TimeSignature}")%>
        </div>
    </div>
    </form>
</asp:Content>