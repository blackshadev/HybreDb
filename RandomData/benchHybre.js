var rel = "./";
process.chdir(__dirname + "/" + rel);
var dat = require(rel + "./app.js");
var $b = require(rel + "./bench.js");

var Hybre = require(rel + './HybreDb.js');
var fs = require("fs");

var HybreStepper = $b.Stepper.extend({
	data: null,
	create: function (smts, conn) {
		this.inherited().create.call(this, smts);
		
		this.data = [];
		this.conn = conn;
	},
	exec: function(smt, cb) {
		var self = this;
		this.conn.send(smt[0], smt[1], function(d) {
			if(d.error) throw JSON.stringify(d.error);

			self.data = d;
			cb();
		});
	}
});

var HybreBenchmark = $b.Benchmark.extend({ 
	stepperClass: HybreStepper,
	create: function(oPar) {
		this.inherited().create.call(this, oPar);
		
		this.conn = oPar.connection;
	},
	getData: function() {
		this.inherited().getData.call(this);

	},
	getStmts: function(tdef, tab) {
		return [["match", { table: tab.table, condition: [ { "type": "and", "field": "prefix", "value": "Mister" } ] } ]];
	},
	getTime: function(cb) {
		cb(this.stepper.data.elapsedTime);
	},
	cStepper: function(smts) {
		this.inherited().cStepper.call(this, smts);
		this.stepper.conn = this.conn;
	},
	doStep: function(cb) {
		var self = this;
		this.conn.send("dropTable", { table: self.name }, function(d) {
			// if(d.error) throw JSON.stringify(d.error);

			self.conn.send("createTable", self.tabData, function(d) {
				if(d.error) throw JSON.stringify(d.error);
				cb();
			});

		});
	}
});


var connection = new Hybre();
var b = new HybreBenchmark({
	tableName: "people_big", 
	fileName: "results/sel_cond/HybreDb.json",
	tDef: dat.table_defs.people_big, 
	connection: connection,
	isSec: false,
	rep: 20,
	steps: [10, 100, 500, 1000, 5000, 10000, 25000, 50000, 75000, 100000, 250000, 500000]
});
b.onDone = function() { connection.close(); };

connection.events.on("ready", function() {
	b.start();
});
connection.start();
