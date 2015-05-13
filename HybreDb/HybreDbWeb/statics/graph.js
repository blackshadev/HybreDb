function HybreResult(el) {
    var dom = (typeof (el) === "string") ? document.getElementById(el) : el;

    this.jControl = $(dom);
    this.jLabel = $("<p/>").appendTo(this.jControl);
    this.jGraph = $("<div/>", { style: "height:100%;" }).appendTo(this.jControl);

    this.clear();
    this.network = new vis.Network(this.jGraph[0], {}, {});

}

function randomColor() {
    return '#' + Math.floor(Math.random() * 16777215).toString(16);
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
        var c = randomColor();
        if (!this.options.groups["tab_" + tab]) {
            this.options.groups["tab_" + tab] = {
                color: {
                    background: c
                }
            };
        }

        for (var id in data[tab].rows) {
            this.data.nodes.push({
                id: tab + "." + id,
                title: this.createTitle(data[tab].rows[id], tab),
                group: "tab_" + tab
            });
        }

    }
};

HybreResult.prototype.createEdges = function(data) {
    var iX = 0;
    for (var rel in data) {
        var c = randomColor();
        var from_tab = data[rel].sourceTable;
        var to_tab = data[rel].destinationTable;
        
        for (var id in data[rel].rows) {
            var r = data[rel].rows[id];
            this.data.edges.push({
                id: rel + "_" + id,
                from: from_tab + "." + r[".rel.src"],
                to: to_tab + "." + r[".rel.dest"],
                color: {
                    color: c
                },
                style: "arrow",
                title: this.createTitle(r, rel)
            });
        }

        iX++;
    }
};

HybreResult.prototype.createTitle = function(d, descr) {
    var t = $("<table class=\"attribute-table\" />");
    t.append($("<caption/>").text(descr, descr));
    for (var k in d) {
        if(k[0] === ".") continue;
        $("<tr/>").append($("<td/>").text(k)).append($("<td/>").text(d[k])).appendTo(t);
    }
    return t[0];
};