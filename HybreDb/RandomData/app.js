var chance = new require("chance")();

function generateDataset(cols, N) {
    var ds = [];
    ds.length = N;

    var defs = Object.keys(cols);

    for (var i = 0; i < N; i++) {
        var row = {};
        
        for (var y = 0; y < defs.length; y++) {
            row[defs[y]] = (cols[defs[y]].generator());
        }

        ds[i] = row;
    }

    return ds;
}

function generateTable(name, def) {
    var o = {
        table: name,
        columns: [],
        data: generateDataset(def.columns, def.N)
    };

    var cols = def.columns;
    for (var k in cols) {
        o.columns.push({
            name: k,
            dataType: cols[k].type,
            hasIndex: cols[k].hasIndex
        });
    }

    return o;
}

function generateRelation(name, def, sourceDef, destDef) {
    var rdef = {
        sourceTable: def.sourceTable,
        destinationTable: def.destinationTable,
        relation: name,
        attributes: [],
        data: null
    };

    for (var k in def.attributes) {
        rdef.attributes.push({
            name: k,
            dataType: def.attributes[k].type,
            hasIndex: !!def.attributes[k].hasIndex
        });
    }

    rdef.data = generateRelationData(def, sourceDef, destDef);

    return rdef;
}

function generateRelationData(def, sourceDef, destDef) {
    var rels = {};

    var data = [];
    data.length = def.N;

    var i = 0;
    while (i < def.N) {
        var from = chance.integer({ min: 0, max: sourceDef.N - 1 });
        var to = chance.integer({ min: 0, max: destDef.N - 1 });

        if (!rels[from])
            rels[from] = {}
        // relation already exists
        if(rels[from][to]) continue;

        var dataAttrs = {
            ".rel.src": from,
            ".rel.dest": to
        };

        for (var k in def.attributes)
            dataAttrs[k] = def.attributes[k].generator();
        

        data[i++] = dataAttrs;
    }

    return data;
}

function generateAll(tDefs, rDefs) {
    for (var k in tDefs) {
        jsonToFile(k + ".json", generateTable(k, tDefs[k]));
    }

    for (var k in rDefs) {
        var r = rDefs[k];
        jsonToFile(k + ".json", generateRelation(k, r, tDefs[r.sourceTable], tDefs[r.destinationTable]));
    }
}

var fs = require("fs");
function jsonToFile(fname, o) {
    var str = JSON.stringify(o);
    
    
    fs.writeFile(fname, str, function(err) {
        if (err) throw err;
        console.log("Written " + fname);
    });
}

var table_def = {
    people_big: {
        N: 1000,
        columns: {
            prefix: {
                type: "text",
                hasIndex: false,
                generator: chance.prefix.bind(chance, { full: true })
            },
            name: {
                type: "text",
                hasIndex: false,
                generator: chance.name.bind(chance, { middle: true })
            },
            age: {
                type: "number",
                hasIndex: false,
                generator: chance.age.bind(chance)
            }
        }
    }
};

var relation_def = {
    knows: {
        N: 10000,
        sourceTable: "people_big",
        destinationTable: "people_big",
        attributes: {
            place: {
                type: "text",
                hasIndex: true,
                generator: chance.state.bind(chance, { full: true })
            }
        }
    }
};


generateAll(table_def, relation_def);