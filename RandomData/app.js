var chance = new require("chance")(42);

function getRandomId(def) {
    return chance.integer({ min: 0, max: def.N - 1 });
}

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

function reset() {
    chance = new require("chance")(42);
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

        rels[from][to] = true;

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
        toFile(k, tDefs[k], generateTable(k, tDefs[k]));
    }

    for (var k in rDefs) {
        var r = rDefs[k];
        toFile(k, r, generateRelation(k, r, tDefs[r.sourceTable], tDefs[r.destinationTable]));
    }
}

function toFile(name, def, data) {
	jsonToFile(name + ".json", data);
}

var fs = require("fs");
function jsonToFile(fname, data) {
    var str = JSON.stringify(data);
    
    fs.writeFile(fname, str, function(err) {
        if (err) throw err;
        console.log("Written " + fname);
    });
}

function jsonToSql(def, o) {
	var sql = "";
	var col_order = [];
	var indices = [];

	var arr = [];
	sql += "create table `" + o.table + "` (";

	sql += "`.id` int AUTO_INCREMENT, "
	sql += o.columns.map(function(c) {
		col_order.push(c.name);
		if(c.hasIndex) indices.push(c.name);
		return "`" + c.name + "` " + def.columns[c.name].sqlType;
	}).join(", ") + ",";

	if(indices.length) {
		sql += indices.map(function(c) {
			return "INDEX (" + c + ")";
		}).join(",");
	}

	sql += "PRIMARY KEY (`.id`)";
	sql += ")";
	
	arr.push(sql);

	sql = "insert into `" + o.table + "` (" + 
			col_order.map(function(c) { 
				return "`" + c + "`"; 
			}).join(", ") + 
			") values ";
	
	sql += o.data.map(function(d) {
		return "(" + 
			col_order.map(function(c) {
				return def.columns[c].type === "text" ? "\"" + d[c] + "\"" : d[c];
			}).join(", ") + 
		")";
	}).join(", ");

	arr.push(sql);

	return arr;
}

function relToSql(def, o) {
    var sql = "";
    var col_order = [".rel.src", ".rel.dest"];
    var indices = [];

    var arr = [];
    sql += "create table `" + o.relation + "` (";

    sql += "`.rel.src` int, `.rel.dest` int, ";
    sql += o.attributes.map(function(c) {
        col_order.push(c.name);
        if(c.hasIndex) indices.push(c.name);
        return "`" + c.name + "` " + def.attributes[c.name].sqlType;
    }).join(", ") + ",";

    if(indices.length) {
        sql += indices.map(function(c) {
            return "INDEX (" + c + ")";
        }).join(",") + ",";
    }

    sql += "PRIMARY KEY (`.rel.src`, `.rel.dest`)";
    sql += ")";
    
    arr.push(sql);

    sql = "insert into `" + o.relation + "` (" + 
            col_order.map(function(c) { 
                return "`" + c + "`"; 
            }).join(", ") + 
            ") values ";
    
    sql += o.data.map(function(d) {
        return "(" + 
            col_order.map(function(c) {
                return def.attributes[c] && def.attributes[c].type === "text" ? "\"" + d[c] + "\"" : d[c];
            }).join(", ") + 
        ")";
    }).join(", ");

    arr.push(sql);

    return arr;
}

function jsonToPgSql(def, o) {
    var sql = "";
    var col_order = [];
    var indices = [];

    var arr = [];
    sql += "create table \"" + o.table + "\" (";

    sql += "\".id\" serial primary key, "
    sql += o.columns.map(function(c) {
        col_order.push(c.name);
        if(c.hasIndex) indices.push(c.name);
        return c.name + " " + def.columns[c.name].sqlType;
    }).join(", ") + ")";

    arr.push(sql);

    arr.push.apply(arr, indices.map(function(c) { 
        return "create index idx_" + c + " on \"" + o.table + "\" (" + c + ")";
    })) + ", ";

    

    sql = "explain analyze insert into \"" + o.table + "\" (" + 
            col_order.map(function(c) { 
                return "\"" + c + "\""; 
            }).join(", ") + 
            ") values ";
    
    sql += o.data.map(function(d) {
        return "(" + 
            col_order.map(function(c) {
                return def.columns[c] && def.columns[c].type === "text" ? "'" + d[c] + "'" : d[c];
            }).join(", ") + 
        ")";
    }).join(", ");

    arr.push(sql);

    return arr;
}

function relToPgSql(def, o) {
    var sql = "";
    var col_order = [".rel.src", ".rel.dest"];
    var indices = [];

    var arr = [];
    sql += "create table \"" + o.relation + "\" (";

    sql += "\".rel.src\" int, \".rel.dest\" int, ";
    sql += o.attributes.map(function(c) {
        col_order.push(c.name);
        if(c.hasIndex) indices.push(c.name);
        return "\"" + c.name + "\" " + def.attributes[c.name].sqlType;
    }).join(", ") + ",";

    if(indices.length) {
        sql += indices.map(function(c) {
            return "INDEX (" + c + ")";
        }).join(",") + ",";
    }

    sql += "PRIMARY KEY (\".rel.src\", \".rel.dest\")";
    sql += ")";
    
    arr.push(sql);

    sql = "insert into \"" + o.relation + "\" (" + 
            col_order.map(function(c) { 
                return "\"" + c + "\""; 
            }).join(", ") + 
            ") values ";
    
    sql += o.data.map(function(d) {
        return "(" + 
            col_order.map(function(c) {
                return def.attributes[c] && def.attributes[c].type === "text" ? "'" + d[c] + "'" : d[c];
            }).join(", ") + 
        ")";
    }).join(", ");

    arr.push(sql);

    return arr;
}

var table_def = {
    people_big: {
        N: 100000,
        columns: {
            prefix: {
                type: "text",
                sqlType: "varchar(255)",
                hasIndex: false,
                generator: chance.prefix.bind(chance, { full: true })
            },
            name: {
                type: "text",
                sqlType: "varchar(255)",
                hasIndex: false,
                generator: chance.name.bind(chance, { middle: true })
            },
            age: {
                type: "number",
                sqlType: "int",
                hasIndex: false,
                generator: chance.age.bind(chance)
            }
        }
    }
};

var relation_def = {
    knows: {
        N: 1000,
        sourceTable: "people_big",
        destinationTable: "people_big",
        attributes: {
            place: {
                type: "text",
                sqlType: "varchar(255)",
                hasIndex: false,
                generator: chance.state.bind(chance, { full: true })
            }
        }
    }
};


generateAll(table_def, relation_def);

module.exports = {
	generateAll: generateAll,
	table_defs: table_def,
	relation_defs: relation_def,
	jsonToSql: jsonToSql,
    jsonToPgSql: jsonToPgSql,
    generateRelation: generateRelation,
	generateTable: generateTable,
    getRandomId: getRandomId,
    getRandomName: chance.name.bind(chance, { middle: true }),
    relToSql: relToSql,
    relToPgSql: relToPgSql,
    reset: reset
};