function HybreResult(el) {
    var dom = (typeof (el) === "string") ? document.getElementById(el) : el;

    this.jControl = $(dom);
    this.jLabel = $("<p/>").appendTo(this.jControl);
    this.jGraph = $("<div/>", { style: "height:100%;" }).appendTo(this.jControl);

    this.clear();
    this.network = new vis.Network(this.jGraph[0], {}, {});

    this.borderColor = "blue";
    this.colors = ["red", "purple", "blue", "pink", "white"];

}

HybreResult.prototype.setData = function(graph) {
    this.clear();

    this.jLabel.text("Took " + graph.elapsedTime);
    

    if(graph.tableData) this.createNodes(graph.tableData);
    if(graph.relationData) this.createEdges(graph.relationData);

    this.network.setOptions(this.options);
    this.network.setData(this.data);

};

HybreResult.prototype.clear = function(dom) {
    this.data = { nodes: [], edges: [] };
    this.options = { groups: {}  };

    if (dom) {
        this.network.setOptions(this.options);
        this.network.setData(this.data);
    }
};

// Data is object of tables
HybreResult.prototype.createNodes = function (data) {
    var iX = 0;
    for (var tab in data) {

        if (!this.options.groups["tab_" + tab]) {
            this.options.groups["tab_" + tab] = {
                color: {
                    border: this.border,
                    background: this.colors[iX++]
                }
            };
        }

        for (var id in data[tab].rows) {
            this.data.nodes.push({
                id: tab + "." + id,
                title: this.createTitle(data[tab].rows[id]),
                group: "tab_" + tab
            });
        }

    }
};

HybreResult.prototype.createEdges = function(data) {
    var iX = 0;
    for (var rel in data) {
        
        if (!this.options.groups["rel_" + rel]) {
            this.options.groups["rel_" + rel] = {
                color: {
                    border: this.colors[this.colors.length - (iX++) + 1]
                }
            };
        }
        
        var from_tab = data[rel].sourceTable;
        var to_tab = data[rel].destinationTable;
        
        for (var id in data[rel].rows) {
            var r = data[rel].rows[id];
            this.data.edges.push({
                id: id,
                from: from_tab + "." + r[".rel.src"],
                to: to_tab + "." + r[".rel.dest"],
                group: "rel_" + rel,
                style: "arrow",
                title: this.createTitle(r)
            });
        }

    }
};

HybreResult.prototype.createTitle = function(d) {
    var t = $("<table/>");
    for (var k in d) {
        $("<tr/>").append($("<td/>").text(k)).append($("<td/>").text(d[k])).appendTo(t);
    }
    return t[0];
};