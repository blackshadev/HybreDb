function HybreResult(el) {
    var dom = (typeof (el) === "string") ? document.getElementById(el) : el;

    this.jControl = $(dom);
    this.jLabel = $("<p/>").appendTo(this.jControl);
    this.jGraph = $("<div/>", { style: "height:100%;" }).appendTo(this.jControl);


    this.data = { nodes: new vis.DataSet(), edges: new vis.DataSet() };
    this.options = {
        groups: {},
        smoothCurves: { dynamic: false, type: "continuous" },
        stabilize: false,
        physics: { barnesHut: { gravitationalConstant: 0, centralGravity: 0, springConstant: 0 } },
        hideEdgesOnDrag: true
    };
    this.network = new vis.Network(this.jGraph[0], this.data, this.options);

    this.clear();

}

function randomColor() {
    return '#' + Math.floor(Math.random() * 16777215).toString(16);
}

HybreResult.prototype.setData = function (graph) {
    this.clear();
    if(this.network) this.network.destroy();

    this.jLabel.text("Took " + graph.elapsedTime);

    if (graph.tableData) {
        console.time("createNodes");
        this.createNodes(graph.tableData);
        console.timeEnd("createNodes");
    }
    if (graph.relationData) {
        console.time("CreateEdges");
        this.createEdges(graph.relationData);
        console.timeEnd("CreateEdges");
    }

    console.time("plottingData");
    var lotsOfData = this.data.nodes > 1000 || this.data.edges > 1000;
    this.options.stabilize = !lotsOfData;
    this.options.hideEdgesOnDrag = lotsOfData;
    this.options.physics.barnesHut = lotsOfData ? { gravitationalConstant: 0, centralGravity: 0, springConstant: 0 } : {};
    this.network = new vis.Network(this.jGraph[0], this.data, this.options);
    console.timeEnd("plottingData");

};

HybreResult.prototype.clear = function(dom) {
    this.data.edges.clear();
    this.data.nodes.clear();

    this.options.groups = {};
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

        var arr = [];
        for (var id in data[tab].rows) {
            arr.push({
                id: tab + "." + id,
                title: this.createTitle(data[tab].rows[id], tab),
                group: "tab_" + tab
            });
        }

        this.data.nodes.add(arr);

    }
};

HybreResult.prototype.createEdges = function(data) {
    var iX = 0;
    for (var rel in data) {
        var c = randomColor();
        console.log(data);
        var from_tab = data[rel].sourceTable;
        var to_tab = data[rel].destinationTable;

        var arr = [];
        for (var id in data[rel].rows) {
            var r = data[rel].rows[id];
            arr.push({
                id: rel + "_" + id,
                from: from_tab + "." + r[".rel.src"],
                to: to_tab + "." + r[".rel.dest"],
                color: {
                    color: c
                },
                style: data[rel].relationType === 1 ? "arrow" : "none",
                title: this.createTitle(r, rel)
            });
        }
        this.data.edges.add(arr);

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
