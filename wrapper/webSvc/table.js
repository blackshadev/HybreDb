var WebService = require("roadie").WebService;

module.exports = WebService.extend({

	error: function(err) {
		this.ctx.response.status(500);
		this.ctx.response.data(err);
		this.ctx.response.send();
	},
	createTable: function() {
		var self = this;
		this.ctx.request.body(function(d) {
			var j;

			function done(err, res) {
				if(err) 
					return self.error({ error: err.toString() });
				
				self.ctx.response.data(["done"]);
				self.ctx.response.send();
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
			self.ctx.response.data(["done"]);
			self.ctx.response.send();
		});
	}
})