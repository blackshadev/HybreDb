
module.exports = require("./jsonSvc.js").extend({
	getTable: function() {
		var t = this.ctx.request.parameter("table");

		var self = this;
		this.ctx._server.db.getTable(t, function(err, f, tab) {
			if(err) return self.error({ error: err.toString() });
			self.send(tab);
		});

	},
	createTable: function() {
		var self = this;
		this.ctx.request.body(function(d) {
			var j;

			function done(err, res) {
				if(err) 
					return self.error({ error: err.toString() });
				
				self.ctx.response.send(["done"]);
			}


			try {
				j = JSON.parse(d.toString());
				self.ctx._server.db.createTable(self.ctx.request.parameter("table"), j, done);
			} catch(e) { 
				self.error({ error : "Parse error: " + e.toString() });
				
			}
		})
		
	},
	deleteTable: function() {
		var self = this;
		this.ctx._server.db.deleteTable(this.ctx.request.parameter("table"), function(err, d) {
			if(err) return self.error({ "error": err.toString() });
			self.ctx.response.send(["done"]);
		});
	}
})