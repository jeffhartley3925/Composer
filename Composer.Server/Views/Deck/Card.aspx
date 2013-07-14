<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="compositionContent" ContentPlaceHolderID="MainContent" runat="server">
    <script src="<%= Url.Content ("~/scripts/card1.js") %>" type="text/javascript"></script>
    <script type="text/javascript">

        Sys.require([Sys.components.dataView, Sys.components.adoNetDataContext]);
        var proxy, json, composition;

        var constGuidLength = 36;

        Sys.onReady(function () {
            var servicePath = document.location.protocol + "//" + location.host + "/composer/DataService.svc";
            proxy = new Sys.Data.AdoNetServiceProxy(servicePath);
            var query = composeQuery();
            if (query.length > 0) {
                proxy.query(query, querySuccessCallback1, queryFailureCallback1);
            }
        });

        function composeQuery() {
            var id = getParameterByName("id");
            if (id.length != constGuidLength) return "";
            var query = new Sys.Data.AdoNetQueryBuilder("Compositions(guid'" + id + "')?$expand=Staffgroups/Staffs/Measures/Chords/Notes,Verses,Collaborations,Arcs");
            return "/" + query.toString();
        }

        function querySuccessCallback1(result, context, operation) {
            json = JSON.stringify(result);
            composition = Sys.Serialization.JavaScriptSerializer.deserialize(json);
            renderComposition("compositionCanvas");
        }

        function queryFailureCallback1(error, context, queryString) {
            var contextMessage = "DataContext is not null.";
            var queryMessage = "The query path is DataService.svc?" + queryString + ".";
            if (context != null) {
                contextMessage = "dataContext is null";
            }
            alert("Failed to connect to DataService.svc.\n\r" + contextMessage + "\n\r" + queryMessage);
        }
    </script>
    <style>
        .bigger {display:none;font-size:24px;font-weight:bold;border-color:transparent;background-color:transparent;position:absolute;top:10px;left:50px;}
        .smaller {display:none;font-size:24px;font-weight:bold;border-color:transparent;background-color:transparent;position:absolute;top:10px;left:80px;}
    </style>
    <input onclick="bigger()" class="bigger" type=button value="+" width="20px" height="20px"/>
    <input onclick="smaller()" class="smaller" type=button value="-" width="20px" height="20px"/>
    <canvas id="compositionCanvas">
    </canvas>
</asp:Content>
