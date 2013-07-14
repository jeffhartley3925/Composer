var hubOptionColors = ["#F04115", "#B61B1B", "#3D0B01", "#233A00", "#436117", "#BAAB40"];
var hubOptions = ["Print", "View", "Edit", "Contribute", "Listen", "Edit"];

Sys.require([Sys.components.dataView, Sys.components.adoNetDataContext]);
var proxy, json, compositions, dataContext, query;
Sys.activateDom = false;
Sys.onReady(function () {

    var servicePathPrefix = document.location.protocol + "//" + location.host + "/composer";
    var servicePath = "/DataService.svc";

    dataContext = Sys.create.adoNetDataContext(
        {
            serviceUri: servicePathPrefix + servicePath,
            mergeOption: Sys.Data.MergeOption.appendOnly
        });
    dataContext.initialize();

    proxy = new Sys.Data.AdoNetServiceProxy(servicePathPrefix + servicePath);
    query = composeQuery();
    proxy.query(query, querySuccessCallback, queryFailureCallback);

    var master = Sys.create.dataView("#masterView",
    {
        dataProvider: dataContext,
        fetchOperation: query,
        autoFetch: true
    });

    var detail = Sys.create.dataView("#detailsView", {
        itemRendered: detailRendered
    });

    Sys.bind(detail, "data", master, "selectedData");

    function detailRendered(dataView, ctx) {
        Sys.bind(Sys.get("#compositionTitle", ctx), "value", ctx.dataItem, "Provenance.TitleLine");
    }
});

$(document).ready(function () {
    $("div").click(function (event) {
        var id = event.target.id;
        var pos = id.indexOf("_");
        //BRITTLE: the only div elements in this html tree that can have 'btn' as the 
        //first 3 characters in their id are the divs used as option buttons. otherwise
        //this logic breaks.
        if (id.indexOf("btn") == 0) {
            var action = id.substring(3, pos).toLowerCase();
            id = id.substring(pos + 1, id.length);
            routeSelection(id, action);
        }
        else {
            //to arrive here a user must have clicked the accordian header, composition
            //scroller or the composition canvas.
            if (id.length > 7) {
                var bCompositionClick = id.substring(0, 7) == "canvas_" || event.target.id.substring(0, 7) == "scroll_";
                //BRITTLE: note that 'canvas_' and 'scroll_' both have 7 characters. otherwise this logic breaks
                id = id.substring(7, id.length);
                elaborate(id);
                if (bCompositionClick) {
                    toggleScale(id);
                }
            }
        }
    });
});

function routeSelection(id, action) {
    switch (action) {
        case "view":
            view(id);
            break;
        case "print":
            print(id);
            break;
        case "edit":
            edit(id);
            break;
        case "contribute":
            contribute(id);
            break;
        case "listen":
            listen(id);
            break;
    }
}

function view(id) {
    location.href = "deck/card?id=" + id;
}

function print(id) {
    alert(id);
}

function edit(id) {
    location.href = "home/index?ID=" + id;
}

function contribute(id) {
    alert(id);
}

function listen(id) {
    alert(id);
}

var elaborated = false;
function elaborate(id) {
    var hdr;
    if (!elaborated) {
        //elaborated = true;
        for (var i = 0; i < compositions.length; i++) {
            ctx = null;
            composition = compositions[i];
            var guid = composition.Id;
            canvasId = "canvas_" + guid;
            renderComposition(canvasId);

            var scroll = document.getElementById("scroll_" + guid);
            if (scaleX == .5) scroll.style.width = "510px";
            else scroll.style.width = "255px";

            hdr = document.getElementById("header_" + guid);
            hdr.style.borderBottom = "0px solid #566047";

        }
        hdr = document.getElementById("header_" + id);
        hdr.style.borderBottom = "1px solid #566047";
    }
}

function composeQuery() {
    var query = new Sys.Data.AdoNetQueryBuilder("Compositions?$expand=Staffgroups/Staffs/Measures/Chords/Notes,Verses,Collaborations,Arcs");
    return "/" + query.toString();
}

function querySuccessCallback(result, context, operation) {
    json = JSON.stringify(result);
    compositions = Sys.Serialization.JavaScriptSerializer.deserialize(json);
}

function queryFailureCallback(error, context, queryString) {
    var contextMessage = "DataContext is not null.";
    var queryMessage = "The query path is DataService.svc?" + queryString + ".";
    if (context != null) {
        contextMessage = "dataContext is null";
    }
    alert("Failed to connect to DataService.svc.\n\r" + contextMessage + "\n\r" + queryMessage);
}

function toggleScale(id) {
    if (scaleX == .25) {
        scale(.5, id)
    }
    else {
        scale(.25, id);
    }
}

function scale(ratio, id) {

    canvasId = "canvas_" + id;
    var canvas = document.getElementById(canvasId);
    if (canvas !== null) {

        for (var i = 0; i < compositions.length; i++) {
            if (compositions[i].Id == id) {
                composition = compositions[i];
            }
            var scroller = document.getElementById("scroll_" + compositions[i].Id);
            var section = document.getElementById(compositions[i].Id);
            if (ratio == .5) {
                scroller.style.width = "510px";
                section.style.width = "810px";
            }
            else {
                scroller.style.width = "255px";
                section.style.width = "510px";
            }
        }
        ctx = canvas.getContext("2d");
        scaleX = ratio;
        scaleY = ratio;
        setContext();
        renderComposition(canvasId);
    }
}