var Hybre = require('./HybreDb.js');
var dat = require("./app.js");
var $b = require("./bench.js");
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
	getStmts: function(tdef, tab) {
		return [["createTable", tab]];
	},
	getTime: function(cb) {
		cb(this.stepper.data.elapsedTime);
	},
	cStepper: function(smts) {
		this.inherited().cStepper.call(this, smts);
		this.stepper.conn = this.conn;
	},
	doIter: function(cb) {
		var self = this;
		this.conn.send("dropTable", { table: this.name }, function(d) {
			if(d.error) throw JSON.stringify(d.error);

			cb();
		});
	}
});


var connection = new Hybre();
var b = new HybreBenchmark({
	tableName: "people_big", 
	fileName: "hybreBench.json",
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
